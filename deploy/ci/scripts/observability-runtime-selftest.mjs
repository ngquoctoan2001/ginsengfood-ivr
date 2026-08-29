import fs from "node:fs";
import path from "node:path";

const REQUIRED_STAGE_SPANS = [
  "ivr.intake",
  "ivr.eligibility.evaluate",
  "ivr.scheduler.dispatch",
  "ivr.result.normalize",
  "ivr.callback.deliver",
];

/**
 * Verifies the application signals against the real LGTM APIs while the existing MOCK E2E stack
 * is still alive. The caller supplies its already-proven Docker/SQL helpers so this test observes
 * the same task rather than constructing a second, weaker fixture.
 */
export function verifyObservabilityRuntime({ docker, compose, psql, sleepSeconds, repositoryRoot }) {
  const taskId = "TASK-E2E-CONFIRM";
  const traceParent = psql(
    `SELECT trace_parent FROM ivr_confirmation_tasks WHERE task_id = '${taskId}'`,
  );
  const match = /^00-([a-f0-9]{32})-[a-f0-9]{16}-[a-f0-9]{2}$/u.exec(traceParent);
  assert(match, `${taskId}: persisted trace_parent is missing or is not W3C.`);
  const traceId = match[1];

  let traceDocument = null;
  let spanNames = [];
  for (let attempt = 0; attempt < 20; attempt += 1) {
    try {
      traceDocument = JSON.parse(lgtmGet(docker, compose, 3200, `/api/traces/${traceId}`));
      spanNames = collectNamedValues(traceDocument);
      if (REQUIRED_STAGE_SPANS.every((name) => spanNames.includes(name))) break;
    } catch {
      // Tempo answers 404 until the batch exporter flushes; bounded polling turns that into an
      // expected state rather than a flaky fixed sleep.
    }
    sleepSeconds(3);
  }

  for (const required of REQUIRED_STAGE_SPANS) {
    assert(spanNames.includes(required), `trace ${traceId} is missing span '${required}'.`);
  }
  const traceJson = JSON.stringify(traceDocument);
  assert(
    traceJson.includes("ginsengfood-ivr-api") && traceJson.includes("ginsengfood-ivr-worker"),
    `trace ${traceId} does not cross both API and Worker resources.`,
  );
  assert(
    traceJson.includes("http.request.method") || traceJson.includes("http.method"),
    `trace ${traceId} has no instrumented outbound HTTP child span.`,
  );

  const requiredMetrics = [
    "ivr_intake_decisions_total",
    "ivr_call_attempts_total",
    "ivr_call_results_total",
    "ivr_result_callbacks_total",
  ];
  const metricDocuments = {};
  for (const metric of requiredMetrics) {
    metricDocuments[metric] = awaitJson(
      () => lgtmGet(docker, compose, 9090, `/api/v1/query?query=${metric}`),
      (document) => document.status === "success" && document.data?.result?.length > 0,
      sleepSeconds,
      `Prometheus never received ${metric}`,
    );
  }

  const logQuery = encodeURIComponent('{service_name=~"ginsengfood-ivr-(api|worker)"}');
  const logDocument = awaitJson(
    () => lgtmGet(
      docker,
      compose,
      3100,
      `/loki/api/v1/query_range?query=${logQuery}&limit=500&direction=backward`,
    ),
    (document) => document.status === "success" && document.data?.result?.length > 0,
    sleepSeconds,
    "Loki never received an API/Worker log record",
  );
  const logJson = JSON.stringify(logDocument);
  assert(logJson.includes(traceId), "OTLP logs do not carry the task trace context.");
  assert(
    logJson.includes("corr-e2e-CONFIRM"),
    "OTLP logs do not carry the safe correlation context.",
  );
  assert(!/(^|\D)(0|84)\d{9}(\D|$)/u.test(logJson), "OTLP logs contain a raw phone number.");
  assert(!logJson.includes("dev-internal-token-not-a-real-secret"), "OTLP logs contain the internal token.");
  assert(!logJson.includes("dev-ordercore-token-not-a-real-secret"), "OTLP logs contain the Order Core token.");

  const dashboards = JSON.parse(lgtmGet(
    docker,
    compose,
    3000,
    "/api/search?query=IVR",
    ["-u", "admin:admin"],
  ));
  assert(
    Array.isArray(dashboards) && dashboards.some((item) => item.title?.includes("IVR")),
    "Grafana did not provision the IVR SLO dashboard.",
  );

  const artifactDirectory = path.join(repositoryRoot, "artifacts", "observability");
  fs.mkdirSync(artifactDirectory, { recursive: true });
  fs.writeFileSync(
    path.join(artifactDirectory, "runtime-proof.json"),
    JSON.stringify({
      taskId,
      traceId,
      requiredStageSpans: REQUIRED_STAGE_SPANS,
      observedSpanNames: [...new Set(spanNames)].sort(),
      metricResultCounts: Object.fromEntries(
        requiredMetrics.map((metric) => [metric, metricDocuments[metric].data.result.length]),
      ),
      logStreamCount: logDocument.data.result.length,
      dashboards: dashboards.map((item) => ({ uid: item.uid, title: item.title })),
      piiValuesPersisted: false,
      traceAndCorrelationContextPresent: true,
      realCustomerCallAllowed: false,
    }, null, 2),
    "utf8",
  );

  process.stdout.write(
    `IT-OBS-TRACE-02 PASS — ${traceId} has all five stages across API/Worker and an outbound HTTP child\n`,
  );
  process.stdout.write(
    "IT-OBS-EXPORT-11 PASS — four metric groups, trace/correlation PII-safe logs and a "
    + "provisioned dashboard are queryable from LGTM\n",
  );
}

function lgtmGet(docker, compose, port, route, extra = []) {
  return docker([
    ...compose,
    "exec",
    "-T",
    "otel-lgtm",
    "curl",
    "--fail",
    "--silent",
    "--show-error",
    ...extra,
    `http://127.0.0.1:${port}${route}`,
  ]);
}

function awaitJson(read, predicate, sleepSeconds, failure) {
  let last = null;
  for (let attempt = 0; attempt < 20; attempt += 1) {
    try {
      last = JSON.parse(read());
      if (predicate(last)) return last;
    } catch {
      // Backend startup/export flush is eventually consistent and bounded by this loop.
    }
    sleepSeconds(3);
  }
  throw new Error(`${failure}. Last response: ${JSON.stringify(last)}`);
}

function collectNamedValues(value, names = []) {
  if (Array.isArray(value)) {
    for (const item of value) collectNamedValues(item, names);
    return names;
  }
  if (value && typeof value === "object") {
    if (typeof value.name === "string") names.push(value.name);
    for (const item of Object.values(value)) collectNamedValues(item, names);
  }
  return names;
}

function assert(condition, message) {
  if (!condition) throw new Error(message);
}
