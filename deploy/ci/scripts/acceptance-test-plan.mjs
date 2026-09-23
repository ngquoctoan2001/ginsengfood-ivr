import assert from "node:assert/strict";
import path from "node:path";

export const TEST_PLAN_SCHEMA = "ivr-acceptance-tests/v1";

// These are shell assertions, not VSTest results. The complete, verified sweep must include their
// owning gate. No per-work plan may map an arbitrary missing TestId onto a green shell gate.
export const GATE_TESTS = Object.freeze({
  ...Object.fromEntries(["", "b", "c", "d", "e", "f", "g", "h"]
    .map((suffix) => [`CT-CI-06${suffix}`, "selftest-pii.sh"])),
  "CT-ACCEPTANCE-C2-01": "acceptance-batches.mjs",
  "CT-ACCEPTANCE-EVIDENCE-01": "acceptance-evidence-selftest.mjs",
  // W-0346. Each runner prints "<TestId> PASS…" right after the assertion of that name, and
  // gateRegistryErrors() holds every entry to that. Only IDs a candidate cites. A gate the full
  // sweep does not run (image, k8s, oasdiff, security-scan) cannot own one.
  ...printedBy("selftest-openapi.mjs", "CT-CI-01"),
  ...printedBy("selftest-dotnet-policy.sh", "CT-CI-02", "CT-CI-03", "CT-CI-09"),
  ...printedBy("ci-config-selftest.mjs", "CT-CI-05", "CT-CI-07", "CT-CI-08", "CT-CI-10", "CT-CI-11",
    "CT-CI-12"),
  ...printedBy("review-gate-selftest.mjs", "CT-GATE-01", "CT-GATE-02", "CT-GATE-03", "CT-GATE-04"),
  ...printedBy("docs-selftest.mjs", "CT-DOC-01", "UT-DOC-PII-03"),
  ...printedBy("cd-selftest.mjs", "IT-CD-DEV-01", "IT-CD-GATE-02", "IT-CD-REAL-03", "IT-CD-ROLLBACK-04",
    "IT-CD-CONCURRENCY-05"),
  ...printedBy("progressive-selftest.mjs", "IT-CANARY-01", "IT-BG-WORKER-02", "IT-MIGRATE-03",
    "IT-FLAG-RAMP-04"),
  ...printedBy("dr-selftest.mjs", "DG-CRYPTO-01", "DG-BACKUP-02", "DG-DR-03"),
  ...printedBy("capacity-selftest.mjs", "CAP-MODEL-01", "CAP-SENS-02", "CAP-CALIB-03", "CAP-ALERT-04",
    "CAP-DRIFT-05", "CAP-SESSION-06"),
  ...printedBy("capacity-data-intake-validator.mjs", "CAP-INTAKE-VALID-01", "CAP-INTAKE-MODE-02",
    "CAP-INTAKE-MODE-03", "CAP-INTAKE-TEMPLATE-04", "CAP-INTAKE-RECEIPT-05", "CAP-INTAKE-RECEIPT-VERIFY-06",
    "CAP-INTAKE-LEDGER-07", "CAP-INTAKE-CHECKPOINT-08"),
  ...printedBy("observability-staging-evidence.mjs", "CT-OBS-STAGING-13", "CT-OBS-STAGING-14"),
});

// Here the whole gate is the test. Its manifest marker is the result, so it prints no per-ID line.
export const WHOLE_GATE_TESTS = Object.freeze(["CT-ACCEPTANCE-C2-01", "CT-ACCEPTANCE-EVIDENCE-01"]);

// A runner that prints "<TestId> PASS_<suffix>" passed under the condition it names. The list shows
// the condition beside the work, which therefore never passes without the owner reading it.
export const GATE_TEST_CONDITIONS = Object.freeze({
  "CAP-CALIB-03": "PASS_UNCALIBRATED: mô hình tự khai chưa hiệu chỉnh và chỉ ra W-0008; PASS_CALIBRATED chỉ khi có số đo cuộc gọi thật",
  "CAP-ALERT-04": "PASS_WITH_NOT_PROVEN=COST_METRIC: pool prod khớp đỉnh của mô hình; cost_per_confirmed_order chưa đo vì còn chờ báo giá (W-0008)",
  "CAP-DRIFT-05": "PASS_DECLARED_DISAGREEMENT: thời lượng cuộc gọi của mô hình, spec và runtime lệch nhau có khai báo; PASS_CALIBRATED khi đã hiệu chỉnh",
  "CAP-SESSION-06": "PASS_UNANSWERED: độ dài phiên vẫn là câu hỏi mở đã khai báo, mô hình tính theo mốc đã chốt",
  "DG-DR-03": "PASS_SINGLE_HOST: RPO=0 và RTO trong ngân sách, nhưng trên một host hai container, không phải multi-AZ",
});

function printedBy(runner, ...ids) {
  return Object.fromEntries(ids.map((id) => [id, runner]));
}

/**
 * Why registry entries cannot stand, one message each. An entry stands when its runner is in the
 * full sweep (argv in the manifest), no .NET test owns its TestId (the entry would hide that test's
 * result), and the runner prints "<TestId> PASS" outside a comment. A PASS with a suffix is
 * conditional: it must be declared, and a declaration needs one.
 */
export function gateRegistryErrors({ manifest, traced, source, registry = GATE_TESTS,
  conditions = GATE_TEST_CONDITIONS, wholeGate = WHOLE_GATE_TESTS }) {
  const errors = [];
  for (const [id, runner] of Object.entries(registry)) {
    if (!Array.isArray(manifest?.gates?.[runner]?.argv)) {
      errors.push(`${id}: ${runner} không chạy trong full sweep`);
      continue;
    }
    if (traced.has(id)) {
      errors.push(`${id} là TestId của test .NET; registry sẽ che kết quả của test đó`);
      continue;
    }
    if (wholeGate.includes(id)) continue;
    const text = source(runner);
    if (typeof text !== "string") {
      errors.push(`${id}: không đọc được ${runner}`);
      continue;
    }
    const escaped = id.replace(/[.*+?^${}()|[\]\\]/gu, "\\$&");
    const printed = new RegExp(`(?<![A-Za-z0-9-])${escaped} (PASS\\w*|\\$\\{[^}]*\\bPASS\\w*[^}]*\\})`, "gu");
    const markers = text.split("\n").filter((line) => !/^\s*(?:\/\/|\/\*|\*|#)/u.test(line))
      .flatMap((line) => [...line.matchAll(printed)].map((match) => match[1]));
    const conditional = markers.some((marker) => marker !== "PASS");
    if (markers.length === 0) {
      errors.push(`${id}: ${runner} không in "${id} PASS"`);
    } else if (conditional && !Object.hasOwn(conditions, id)) {
      errors.push(`${id}: ${runner} in PASS có điều kiện (${markers.join(", ")}) mà chưa khai báo`);
    } else if (!conditional && Object.hasOwn(conditions, id)) {
      errors.push(`${id}: khai báo điều kiện nhưng ${runner} chỉ in PASS`);
    }
  }

  for (const id of Object.keys(conditions)) {
    if (!Object.hasOwn(registry, id)) errors.push(`${id}: có điều kiện nhưng không có trong registry`);
  }
  return errors;
}

/** Read only same-pack Markdown attachments; every read is supplied by the pinned Git reader. */
export function collectTestEvidence({ workId, evidencePath, evidenceText, promptText = "", trace,
  extract, read, validateDecision }) {
  const empty = { ids: [], retired: [], documents: [], errors: [] };
  if (!evidencePath || evidenceText === null) return empty;
  try {
    const directory = path.posix.dirname(evidencePath);
    const planPath = `${directory}/acceptance-tests.json`;
    const raw = read(planPath);
    const plan = raw === null ? null : JSON.parse(raw);
    if (plan) {
      assert.equal(plan.schema, TEST_PLAN_SCHEMA, "unsupported acceptance test plan schema");
      assert.equal(plan.workId, workId, "test plan belongs to another work item");
      assert(["software", "retired-ui"].includes(plan.scope), "unknown acceptance scope");
    }
    const queue = [evidencePath];
    const documents = new Map();
    while (queue.length) {
      const file = queue.shift();
      if (documents.has(file)) continue;
      assert(documents.size < 32, "too many evidence attachments");
      const text = file === evidencePath ? evidenceText : read(file);
      assert.equal(typeof text, "string", `missing evidence attachment: ${file}`);
      documents.set(file, text);
      for (const match of text.matchAll(/\]\(([^\s)#]+\.md)(?:#[^\s)]*)?\)/gu)) {
        const link = match[1];
        if (/^[a-z]+:/iu.test(link) || link.startsWith("/")) continue;
        const target = path.posix.normalize(path.posix.join(path.posix.dirname(file), link));
        // Links outside the work's evidence pack are references, not claims about its test run.
        if (target.startsWith(`${directory}/`)) queue.push(target);
      }
    }
    const cited = new Set([...documents.values(), promptText].flatMap((text) => extract(text, trace)));
    const required = plan?.requiredTestIds ?? [];
    assert(Array.isArray(required) && required.every((id) => typeof id === "string" && /^[A-Z][A-Z0-9-]*-\d[A-Za-z0-9-]*$/u.test(id)),
      "requiredTestIds must contain explicit TestIds");
    for (const id of required) cited.add(id);
    const retired = plan?.retirements ?? [];
    assert(Array.isArray(retired), "retirements must be an array");
    const seen = new Set();
    for (const item of retired) {
      assert(cited.has(item.testId), `retired TestId was not cited: ${item.testId}`);
      assert(!trace.byId.has(item.testId), `cannot retire an active TestId: ${item.testId}`);
      assert(!seen.has(item.testId), `duplicate retirement: ${item.testId}`);
      seen.add(item.testId);
      assert(typeof item.reason === "string" && item.reason.trim().length >= 20, "retirement needs a specific reason");
      assert(Array.isArray(item.replacementTestIds) && item.replacementTestIds.length > 0,
        "retirement needs active replacement tests");
      assert(item.replacementTestIds.every((id) => trace.byId.has(id)), "replacement TestId is missing");
      assert(typeof validateDecision === "function", "retirement requires pinned decision verification");
      validateDecision(item.decision);
      cited.delete(item.testId);
      for (const id of item.replacementTestIds) cited.add(id);
    }
    if (plan?.scope === "retired-ui") {
      assert(typeof validateDecision === "function", "retired UI requires pinned decision verification");
      validateDecision(plan.decision);
      assert.equal(plan.proposedDisposition, "CANCELLED", "retired UI must propose an explicit closeout");
      return { ...empty, documents: [...documents.keys()], errors: [
        "UI đã ngừng phạm vi theo quyết định đã ghim; hồ sơ đề nghị CANCELLED, chờ Toàn. Test backend không chứng nhận UI.",
      ] };
    }
    return { ids: [...cited].sort(), retired: [...seen].sort(), documents: [...documents.keys()], errors: [] };
  } catch (error) {
    return { ...empty, errors: [`hồ sơ C2 không hợp lệ: ${error.message.split("\n")[0]}`] };
  }
}
