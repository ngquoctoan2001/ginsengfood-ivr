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
});

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
