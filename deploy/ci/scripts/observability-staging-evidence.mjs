import crypto from "node:crypto";
import fs from "node:fs";
import http from "node:http";
import os from "node:os";
import path from "node:path";

const REQUIRED_STAGE_SPANS = [
  "ivr.intake",
  "ivr.eligibility.evaluate",
  "ivr.scheduler.dispatch",
  "ivr.result.normalize",
  "ivr.callback.deliver",
];

const REQUIRED_METRICS = [
  "ivr_intake_decisions_total",
  "ivr_call_attempts_total",
  "ivr_call_results_total",
  "ivr_result_callbacks_total",
];

const SECRET_INPUT_KEYS = new Set([
  "apikey",
  "authorization",
  "credential",
  "headers",
  "password",
  "secret",
  "secretvalue",
  "token",
]);
const ALLOWED_SECRET_REFERENCE_KEYS = new Set(["headerssecretreference"]);

const PHONE_PATTERN = /(?<![0-9A-Za-z])(?:0[0-9]{9}|(?:84|\+84)[0-9]{9}|0[0-9]{2}[\s.-][0-9]{3}[\s.-][0-9]{4}|(?:84|\+84)[\s.-]*\(?[0-9]{2}\)?[\s.-][0-9]{3}[\s.-][0-9]{4})(?![0-9A-Za-z])/u;
const DIAL_TOKEN_PATTERN = /(?:dial[_-]?token)["'`: ]+[A-Za-z0-9._-]{8,}/iu;
const ADDRESS_PATTERN = /(?<![\p{L}\p{N}])(?:(?:duong|so nha|ngo|hem|ngach|thon|ap)\s+[A-Za-z0-9]|(?:đường|số nhà|ngõ|hẻm|ngách|thôn|ấp|tổ)\s+)/iu;

const mode = process.argv[2];
try {
  if (mode === "--capture") {
    await captureFromEnvironment();
  } else if (mode === "--self-test") {
    await runSelfTest();
  } else {
    throw new Error("expected --capture or --self-test");
  }
} catch (error) {
  process.stderr.write(`OBSERVABILITY_STAGING_EVIDENCE_FAIL — ${safeError(error)}\n`);
  process.exitCode = 1;
}

async function captureFromEnvironment() {
  assert(
    process.env.REAL_CUSTOMER_CALL_ALLOWED === "NO",
    "REAL_CUSTOMER_CALL_ALLOWED must be NO",
  );
  const inputPath = requiredEnvironment("IVR_STAGING_EVIDENCE_INPUT_FILE");
  const outputDirectory = process.env.IVR_STAGING_EVIDENCE_DIR
    ?? path.join(process.cwd(), "artifacts", "observability", "staging");
  const input = JSON.parse(fs.readFileSync(inputPath, "utf8"));
  input.dashboardScreenshotFile = requiredEnvironment(
    "IVR_STAGING_DASHBOARD_SCREENSHOT_FILE",
  );
  const headers = {
    tempo: readSecretHeaders("IVR_STAGING_TEMPO_HEADERS_JSON"),
    prometheus: readSecretHeaders("IVR_STAGING_PROMETHEUS_HEADERS_JSON"),
    loki: readSecretHeaders("IVR_STAGING_LOKI_HEADERS_JSON"),
    grafana: readSecretHeaders("IVR_STAGING_GRAFANA_HEADERS_JSON"),
  };

  const output = await captureEvidence(input, headers, outputDirectory, { allowHttp: false });
  process.stdout.write(
    `B06_STAGING_EVIDENCE_PASS — trace=${output.task.traceId} artifact=${output.artifactPath}\n`,
  );
}

async function captureEvidence(input, headers, outputDirectory, options) {
  validateInput(input, options);
  const secretValues = Object.values(headers).flatMap((item) => Object.values(item));
  const observationStart = input.observationWindow.start;
  const observationEnd = input.observationWindow.end;

  const tempoUrl = backendUrl(input.backends.tempoUrl, `/api/traces/${input.task.traceId}`);
  const traceDocument = await requestJson("Tempo", tempoUrl, headers.tempo);
  const spanNames = [...new Set(collectNamedValues(traceDocument))].sort();
  for (const required of REQUIRED_STAGE_SPANS) {
    assert(spanNames.includes(required), `Tempo trace is missing span '${required}'`);
  }
  const traceJson = JSON.stringify(traceDocument);
  assert(
    traceJson.includes("ginsengfood-ivr-api") && traceJson.includes("ginsengfood-ivr-worker"),
    "Tempo trace does not cross both API and Worker services",
  );
  assert(
    traceJson.includes("http.request.method") || traceJson.includes("http.method"),
    "Tempo trace has no outbound HTTP child",
  );

  const metricResultCounts = {};
  for (const metric of REQUIRED_METRICS) {
    const queryUrl = backendUrl(input.backends.prometheusUrl, "/api/v1/query", {
      query: metric,
      time: observationEnd,
    });
    const document = await requestJson("Prometheus", queryUrl, headers.prometheus);
    assertPrometheusSuccess(document, metric);
    assert(document.data.result.length > 0, `Prometheus has no series for ${metric}`);
    metricResultCounts[metric] = document.data.result.length;
  }

  const logQuery = '{service_name=~"ginsengfood-ivr-(api|worker)"}';
  const logUrl = backendUrl(input.backends.lokiUrl, "/loki/api/v1/query_range", {
    query: logQuery,
    start: observationStart,
    end: observationEnd,
    limit: "5000",
    direction: "backward",
  });
  const logDocument = await requestJson("Loki", logUrl, headers.loki);
  assertPrometheusSuccess(logDocument, "Loki log query");
  const logJson = JSON.stringify(logDocument);
  assert(logDocument.data.result.length > 0, "Loki returned no API/Worker log streams");
  assert(logJson.includes(input.task.traceId), "Loki logs do not contain the TraceId");
  assert(logJson.includes(input.task.correlationId), "Loki logs do not contain the correlation ID");
  assertNoPii(logJson, "Loki response");
  assertSecretsAbsent(logJson, secretValues);

  const dashboardUrl = backendUrl(input.backends.grafanaUrl, "/api/search", { query: "IVR" });
  const dashboards = await requestJson("Grafana", dashboardUrl, headers.grafana);
  assert(Array.isArray(dashboards), "Grafana dashboard search did not return an array");
  const dashboard = dashboards.find((item) => item.uid === input.backends.dashboardUid);
  assert(dashboard, `Grafana dashboard '${input.backends.dashboardUid}' is not provisioned`);

  const alertQuery = `ALERTS{alertname="${input.alert.name}",alertstate="firing"}`;
  const alertRangeUrl = backendUrl(input.backends.prometheusUrl, "/api/v1/query_range", {
    query: alertQuery,
    start: input.alert.start,
    end: input.alert.end,
    step: String(input.alert.stepSeconds),
  });
  const alertRange = await requestJson("Prometheus alert range", alertRangeUrl, headers.prometheus);
  assertPrometheusSuccess(alertRange, input.alert.name);
  const firingSamples = countPositiveSamples(alertRange.data.result);
  assert(firingSamples > 0, `alert '${input.alert.name}' has no firing sample in the evidence window`);

  const alertRecoveryUrl = backendUrl(input.backends.prometheusUrl, "/api/v1/query", {
    query: alertQuery,
    time: input.alert.end,
  });
  const alertRecovery = await requestJson(
    "Prometheus alert recovery",
    alertRecoveryUrl,
    headers.prometheus,
  );
  assertPrometheusSuccess(alertRecovery, input.alert.name);
  assert(
    alertRecovery.data.result.length === 0,
    `alert '${input.alert.name}' was still firing at the declared recovery time`,
  );

  const screenshot = readScreenshot(input.dashboardScreenshotFile);
  fs.mkdirSync(outputDirectory, { recursive: true });
  const screenshotName = `dashboard.${screenshot.extension}`;
  fs.copyFileSync(input.dashboardScreenshotFile, path.join(outputDirectory, screenshotName));

  const evidence = {
    schemaVersion: 1,
    status: "B06_STAGING_EVIDENCE_PASS",
    capturedAt: new Date().toISOString(),
    environment: input.environment,
    deployment: input.deployment,
    collector: input.collector,
    task: input.task,
    observationWindow: input.observationWindow,
    trace: {
      requiredStageSpans: REQUIRED_STAGE_SPANS,
      observedSpanNames: spanNames,
      apiAndWorkerPresent: true,
      outboundHttpChildPresent: true,
    },
    metrics: {
      queries: REQUIRED_METRICS,
      resultCounts: metricResultCounts,
    },
    logs: {
      query: logQuery,
      streamCount: logDocument.data.result.length,
      traceContextPresent: true,
      correlationContextPresent: true,
      piiScanPassed: true,
    },
    dashboard: {
      uid: dashboard.uid,
      title: dashboard.title,
      screenshotFile: screenshotName,
      screenshotSha256: screenshot.sha256,
    },
    alert: {
      name: input.alert.name,
      start: input.alert.start,
      end: input.alert.end,
      stepSeconds: input.alert.stepSeconds,
      firingSamples,
      recoveredAtEnd: true,
    },
    retentionAccess: input.retentionAccess,
    queryAuthenticationConfigured: Object.fromEntries(
      Object.entries(headers).map(([name, value]) => [name, Object.keys(value).length > 0]),
    ),
    realCustomerCallAllowed: false,
    credentialMaterialPersisted: false,
  };
  const serialized = `${JSON.stringify(evidence, null, 2)}\n`;
  assertSecretsAbsent(serialized, secretValues);
  assertNoPii(serialized, "staging closure manifest");
  const artifactPath = path.join(outputDirectory, "closure-manifest.json");
  fs.writeFileSync(artifactPath, serialized, "utf8");
  return { ...evidence, artifactPath };
}

function validateInput(input, options) {
  assert(input && typeof input === "object" && !Array.isArray(input), "input must be an object");
  assertNoSecretFields(input);
  assertExactKeys(input, [
    "schemaVersion",
    "environment",
    "deployment",
    "collector",
    "task",
    "observationWindow",
    "backends",
    "alert",
    "retentionAccess",
    "dashboardScreenshotFile",
  ], "input");
  assert(input.schemaVersion === 1, "schemaVersion must be 1");
  assert(input.environment === "staging", "environment must be staging");

  assertExactKeys(
    input.deployment,
    ["gitSha", "apiImage", "workerImage", "sourceRef", "capturedAt"],
    "deployment",
  );
  assertFullSha(input.deployment?.gitSha, "deployment.gitSha");
  assertDigestImage(input.deployment?.apiImage, "deployment.apiImage");
  assertDigestImage(input.deployment?.workerImage, "deployment.workerImage");
  assertSafeReference(input.deployment?.sourceRef, "deployment.sourceRef");
  assertTimestamp(input.deployment?.capturedAt, "deployment.capturedAt");

  assertExactKeys(
    input.collector,
    ["otlpEndpoint", "protocol", "namespaceLabels", "podLabels", "headersSecretReference"],
    "collector",
  );
  validateUrl(input.collector?.otlpEndpoint, "collector.otlpEndpoint", options.allowHttp);
  assert(
    input.collector?.protocol === "grpc" || input.collector?.protocol === "http/protobuf",
    "collector.protocol must be grpc or http/protobuf",
  );
  assertLabels(input.collector?.namespaceLabels, "collector.namespaceLabels");
  assertLabels(input.collector?.podLabels, "collector.podLabels");
  assert(
    /^[A-Za-z0-9._/-]+:[A-Za-z0-9._-]+$/u.test(input.collector?.headersSecretReference ?? ""),
    "collector.headersSecretReference must be a secret-name:key reference",
  );

  assertExactKeys(input.task, ["taskId", "correlationId", "traceId"], "task");
  assertSafeIdentifier(input.task?.taskId, "task.taskId");
  assertSafeIdentifier(input.task?.correlationId, "task.correlationId");
  assert(/^[a-f0-9]{32}$/u.test(input.task?.traceId ?? ""), "task.traceId must be 32 lowercase hex");

  assertExactKeys(input.observationWindow, ["start", "end"], "observationWindow");
  assertWindow(input.observationWindow, "observationWindow");
  assertExactKeys(
    input.backends,
    ["tempoUrl", "prometheusUrl", "lokiUrl", "grafanaUrl", "dashboardUid"],
    "backends",
  );
  for (const [name, value] of Object.entries(input.backends ?? {})) {
    if (name === "dashboardUid") continue;
    validateUrl(value, `backends.${name}`, options.allowHttp);
  }
  assertSafeIdentifier(input.backends?.dashboardUid, "backends.dashboardUid");

  assertExactKeys(input.alert, ["name", "start", "end", "stepSeconds"], "alert");
  assert(/^[A-Za-z_:][A-Za-z0-9_:]*$/u.test(input.alert?.name ?? ""), "alert.name is invalid");
  assertWindow(input.alert, "alert");
  assert(
    Number.isInteger(input.alert?.stepSeconds) && input.alert.stepSeconds >= 1,
    "alert.stepSeconds must be a positive integer",
  );

  assertExactKeys(
    input.retentionAccess,
    ["metricsDays", "tracesDays", "logsDays", "accessRoles", "ownerRef", "sourceRef", "approvedAt"],
    "retentionAccess",
  );
  for (const signal of ["metricsDays", "tracesDays", "logsDays"]) {
    assert(
      Number.isInteger(input.retentionAccess?.[signal]) && input.retentionAccess[signal] > 0,
      `retentionAccess.${signal} must be a positive integer`,
    );
  }
  assert(
    Array.isArray(input.retentionAccess?.accessRoles)
      && input.retentionAccess.accessRoles.length > 0,
    "retentionAccess.accessRoles must be non-empty",
  );
  for (const role of input.retentionAccess.accessRoles) {
    assertSafeReference(role, "retentionAccess.accessRoles[]");
  }
  assertSafeReference(input.retentionAccess?.ownerRef, "retentionAccess.ownerRef");
  assertSafeReference(input.retentionAccess?.sourceRef, "retentionAccess.sourceRef");
  assertTimestamp(input.retentionAccess?.approvedAt, "retentionAccess.approvedAt");
  assert(
    typeof input.dashboardScreenshotFile === "string"
      && fs.existsSync(input.dashboardScreenshotFile)
      && fs.statSync(input.dashboardScreenshotFile).isFile(),
    "dashboardScreenshotFile must point to an existing file",
  );
}

function assertExactKeys(value, expected, field) {
  assert(value && typeof value === "object" && !Array.isArray(value), `${field} must be an object`);
  const actual = Object.keys(value);
  const expectedSet = new Set(expected);
  const unknown = actual.filter((key) => !expectedSet.has(key));
  const missing = expected.filter((key) => !Object.hasOwn(value, key));
  assert(unknown.length === 0, `${field} contains unsupported fields`);
  assert(missing.length === 0, `${field} is missing required fields`);
}

function validateUrl(value, field, allowHttp) {
  let parsed;
  try {
    parsed = new URL(value);
  } catch {
    throw new Error(`${field} must be an absolute URL`);
  }
  const allowedProtocols = allowHttp ? new Set(["http:", "https:"]) : new Set(["https:"]);
  assert(allowedProtocols.has(parsed.protocol), `${field} must use HTTPS`);
  assert(!parsed.username && !parsed.password, `${field} must not contain credentials`);
  assert(!parsed.search && !parsed.hash, `${field} must not contain query or fragment data`);
}

function backendUrl(base, route, parameters = {}) {
  const url = new URL(base);
  url.pathname = `${url.pathname.replace(/\/$/u, "")}${route}`;
  for (const [name, value] of Object.entries(parameters)) url.searchParams.set(name, value);
  return url;
}

async function requestJson(backend, url, headers) {
  const response = await fetch(url, { headers, signal: AbortSignal.timeout(15_000) });
  assert(response.ok, `${backend} returned HTTP ${response.status}`);
  try {
    return await response.json();
  } catch {
    throw new Error(`${backend} did not return JSON`);
  }
}

function assertPrometheusSuccess(document, queryName) {
  assert(
    document?.status === "success" && Array.isArray(document.data?.result),
    `${queryName} query was not successful`,
  );
}

function countPositiveSamples(results) {
  let count = 0;
  for (const result of results) {
    const samples = Array.isArray(result.values) ? result.values : [result.value].filter(Boolean);
    for (const sample of samples) {
      if (Array.isArray(sample) && Number(sample[1]) > 0) count += 1;
    }
  }
  return count;
}

function collectNamedValues(value, names = []) {
  if (Array.isArray(value)) {
    for (const item of value) collectNamedValues(item, names);
  } else if (value && typeof value === "object") {
    if (typeof value.name === "string") names.push(value.name);
    for (const item of Object.values(value)) collectNamedValues(item, names);
  }
  return names;
}

function readScreenshot(filePath) {
  const value = fs.readFileSync(filePath);
  const png = value.subarray(0, 8).equals(Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]));
  const jpeg = value.length >= 3 && value[0] === 0xff && value[1] === 0xd8 && value[2] === 0xff;
  assert(png || jpeg, "dashboard screenshot must be PNG or JPEG");
  return {
    extension: png ? "png" : "jpg",
    sha256: crypto.createHash("sha256").update(value).digest("hex"),
  };
}

function readSecretHeaders(name) {
  const raw = process.env[name];
  if (!raw) return {};
  let value;
  try {
    value = JSON.parse(raw);
  } catch {
    throw new Error(`${name} must be a JSON object`);
  }
  assert(value && typeof value === "object" && !Array.isArray(value), `${name} must be an object`);
  for (const [header, content] of Object.entries(value)) {
    assert(/^[A-Za-z0-9-]+$/u.test(header), `${name} contains an invalid header name`);
    assert(typeof content === "string" && content.length >= 4, `${name} has an invalid value`);
  }
  return value;
}

function assertNoSecretFields(value, prefix = "input") {
  if (Array.isArray(value)) {
    value.forEach((item, index) => assertNoSecretFields(item, `${prefix}[${index}]`));
    return;
  }
  if (!value || typeof value !== "object") return;
  for (const [key, item] of Object.entries(value)) {
    const normalized = key.replace(/[^A-Za-z0-9]/gu, "").toLowerCase();
    const containsSecretName = [...SECRET_INPUT_KEYS].some((name) => normalized.includes(name));
    assert(
      !containsSecretName || ALLOWED_SECRET_REFERENCE_KEYS.has(normalized),
      `${prefix}.${key} may not contain credential material`,
    );
    assertNoSecretFields(item, `${prefix}.${key}`);
  }
}

function assertNoPii(value, field) {
  assert(!PHONE_PATTERN.test(value), `${field} contains a raw phone number`);
  assert(!DIAL_TOKEN_PATTERN.test(value), `${field} contains a dial token`);
  assert(!ADDRESS_PATTERN.test(value), `${field} contains a raw address pattern`);
}

function assertSecretsAbsent(value, secrets) {
  for (const secret of secrets) {
    assert(!value.includes(secret), "credential material would be persisted in evidence");
  }
}

function assertFullSha(value, field) {
  assert(/^[a-f0-9]{40}$/u.test(value ?? ""), `${field} must be a full lowercase Git SHA`);
}

function assertDigestImage(value, field) {
  assert(
    /^[^\s@]+@sha256:[a-f0-9]{64}$/u.test(value ?? ""),
    `${field} must use an exact sha256 image digest`,
  );
}

function assertSafeIdentifier(value, field) {
  assert(
    typeof value === "string" && /^[A-Za-z0-9._:-]{1,128}$/u.test(value),
    `${field} must be a bounded safe identifier`,
  );
  assertNoPii(value, field);
}

function assertSafeReference(value, field) {
  assert(
    typeof value === "string" && value.length >= 1 && value.length <= 512 && !/[\r\n]/u.test(value),
    `${field} must be a bounded single-line reference`,
  );
  assertNoPii(value, field);
}

function assertLabels(value, field) {
  assert(value && typeof value === "object" && !Array.isArray(value), `${field} must be an object`);
  const entries = Object.entries(value);
  assert(entries.length > 0, `${field} must be non-empty`);
  for (const [key, content] of entries) {
    assert(/^[A-Za-z0-9._/-]+$/u.test(key), `${field} has an invalid key`);
    assert(/^[A-Za-z0-9._/-]+$/u.test(content) && content !== "*", `${field} has an invalid value`);
  }
}

function assertWindow(value, field) {
  assertTimestamp(value?.start, `${field}.start`);
  assertTimestamp(value?.end, `${field}.end`);
  assert(Date.parse(value.start) < Date.parse(value.end), `${field}.start must precede end`);
}

function assertTimestamp(value, field) {
  assert(
    typeof value === "string" && Number.isFinite(Date.parse(value)),
    `${field} must be an RFC3339 timestamp`,
  );
}

function requiredEnvironment(name) {
  const value = process.env[name];
  assert(typeof value === "string" && value.length > 0, `${name} is required`);
  return value;
}

function safeError(error) {
  return error instanceof Error ? error.message : "unknown failure";
}

function assert(condition, message) {
  if (!condition) throw new Error(message);
}

async function runSelfTest() {
  const temporaryDirectory = fs.mkdtempSync(path.join(os.tmpdir(), "ivr-staging-observability-"));
  const screenshotPath = path.join(temporaryDirectory, "dashboard.png");
  fs.writeFileSync(
    screenshotPath,
    Buffer.from("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=", "base64"),
  );
  const traceId = "0123456789abcdef0123456789abcdef";
  const correlationId = "corr-staging-proof-01";
  const server = http.createServer((request, response) => {
    const requestUrl = new URL(request.url, "http://127.0.0.1");
    response.setHeader("content-type", "application/json");
    if (requestUrl.pathname.startsWith("/api/traces/")) {
      const stageSpans = requestUrl.pathname.endsWith("f".repeat(32))
        ? REQUIRED_STAGE_SPANS.slice(0, -1)
        : REQUIRED_STAGE_SPANS;
      response.end(JSON.stringify({
        resources: ["ginsengfood-ivr-api", "ginsengfood-ivr-worker"],
        spans: [
          ...stageSpans.map((name) => ({ name })),
          { name: "System.Net.Http", attributes: { "http.request.method": "POST" } },
        ],
      }));
      return;
    }
    if (requestUrl.pathname === "/loki/api/v1/query_range") {
      response.end(JSON.stringify({
        status: "success",
        data: { result: [{ stream: { service_name: "ginsengfood-ivr-api", traceId }, values: [["1", correlationId]] }] },
      }));
      return;
    }
    if (requestUrl.pathname === "/api/search") {
      response.end(JSON.stringify([{ uid: "ivr-order-confirmation-slo", title: "IVR SLO" }]));
      return;
    }
    if (requestUrl.pathname === "/api/v1/query_range") {
      response.end(JSON.stringify({ status: "success", data: { result: [{ values: [[1, "1"]] }] } }));
      return;
    }
    if (requestUrl.pathname === "/api/v1/query") {
      const isAlert = requestUrl.searchParams.get("query")?.startsWith("ALERTS{");
      response.end(JSON.stringify({
        status: "success",
        data: { result: isAlert ? [] : [{ metric: {}, value: [1, "1"] }] },
      }));
      return;
    }
    response.statusCode = 404;
    response.end("{}");
  });
  await new Promise((resolve) => server.listen(0, "127.0.0.1", resolve));

  try {
    const address = server.address();
    const baseUrl = `http://127.0.0.1:${address.port}`;
    const template = JSON.parse(fs.readFileSync(
      path.join(process.cwd(), "deploy", "observability", "staging-evidence.template.json"),
      "utf8",
    ));
    template.dashboardScreenshotFile = screenshotPath;
    validateInput(template, { allowHttp: false });
    const input = selfTestInput(baseUrl, screenshotPath, traceId, correlationId);
    const secret = "Bearer self-test-query-credential";
    const headers = {
      tempo: { Authorization: secret },
      prometheus: { Authorization: secret },
      loki: { Authorization: secret },
      grafana: { Authorization: secret },
    };
    const output = await captureEvidence(
      input,
      headers,
      path.join(temporaryDirectory, "evidence"),
      { allowHttp: true },
    );
    const serialized = fs.readFileSync(output.artifactPath, "utf8");
    assert(output.status === "B06_STAGING_EVIDENCE_PASS", "positive fixture did not pass");
    assert(!serialized.includes(secret), "self-test credential leaked into the artifact");

    await expectFailure(
      () => captureEvidence(
        { ...input, task: { ...input.task, traceId: "f".repeat(32) } },
        headers,
        path.join(temporaryDirectory, "negative-trace"),
        { allowHttp: true },
      ),
      "incomplete trace",
    );
    await expectFailure(
      () => captureEvidence(
        { ...input, deployment: { ...input.deployment, workerImage: "mutable:latest" } },
        headers,
        path.join(temporaryDirectory, "negative-image"),
        { allowHttp: true },
      ),
      "mutable image",
    );
    await expectFailure(
      () => captureEvidence(
        { ...input, authorization: "must-not-be-in-input" },
        headers,
        path.join(temporaryDirectory, "negative-secret"),
        { allowHttp: true },
      ),
      "credential field",
    );
    await expectFailure(
      () => captureEvidence(
        input,
        headers,
        path.join(temporaryDirectory, "negative-http"),
        { allowHttp: false },
      ),
      "insecure backend URL",
    );

    process.stdout.write(
      "CT-OBS-STAGING-13 PASS — the template and complete backend-shaped proof produce a PII-safe closure manifest\n",
    );
    process.stdout.write(
      "CT-OBS-STAGING-14 PASS — incomplete traces, mutable images, credential fields and HTTP staging endpoints fail closed\n",
    );
  } finally {
    await new Promise((resolve, reject) => server.close((error) => error ? reject(error) : resolve()));
  }
}

function selfTestInput(baseUrl, screenshotPath, traceId, correlationId) {
  return {
    schemaVersion: 1,
    environment: "staging",
    deployment: {
      gitSha: "0123456789abcdef0123456789abcdef01234567",
      apiImage: `registry.invalid/ivr-api@sha256:${"a".repeat(64)}`,
      workerImage: `registry.invalid/ivr-worker@sha256:${"b".repeat(64)}`,
      sourceRef: "platform-deployment-capture-01",
      capturedAt: "2026-08-29T01:00:00Z",
    },
    collector: {
      otlpEndpoint: `${baseUrl}/otlp`,
      protocol: "grpc",
      namespaceLabels: { "kubernetes.io/metadata.name": "observability" },
      podLabels: { "app.kubernetes.io/name": "otel-collector" },
      headersSecretReference: "ivr-staging-otel:headers",
    },
    task: { taskId: "TASK-STAGING-MOCK-01", correlationId, traceId },
    observationWindow: { start: "2026-08-29T01:00:00Z", end: "2026-08-29T01:20:00Z" },
    backends: {
      tempoUrl: baseUrl,
      prometheusUrl: baseUrl,
      lokiUrl: baseUrl,
      grafanaUrl: baseUrl,
      dashboardUid: "ivr-order-confirmation-slo",
    },
    alert: {
      name: "IvrChannelAutoDisableBurst",
      start: "2026-08-29T01:00:00Z",
      end: "2026-08-29T01:20:00Z",
      stepSeconds: 30,
    },
    retentionAccess: {
      metricsDays: 30,
      tracesDays: 14,
      logsDays: 30,
      accessRoles: ["ivr-observability-readers"],
      ownerRef: "platform-observability",
      sourceRef: "platform-policy-01",
      approvedAt: "2026-08-29T00:00:00Z",
    },
    dashboardScreenshotFile: screenshotPath,
  };
}

async function expectFailure(action, label) {
  let failed = false;
  try {
    await action();
  } catch {
    failed = true;
  }
  assert(failed, `${label} negative control unexpectedly passed`);
}
