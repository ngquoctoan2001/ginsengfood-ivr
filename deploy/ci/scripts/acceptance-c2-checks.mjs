import assert from "node:assert/strict";
import { collectTestEvidence, gateRegistryErrors, TEST_PLAN_SCHEMA } from "./acceptance-test-plan.mjs";

// Invoked by the existing acceptance-batches --self-test gate; no separate optional runner.
export function c2SelfTest({ citedTestIds, traceability, judge, indexResults, testVerdict }) {
  const trace = traceability("| `UT-X` | 1 |\n| `UT-X-01` | unit | `First` | `tests/XTests.cs` |\n");
  assert.deepEqual(citedTestIds("UT-UI-RBAC-01 E2E-UI-AUTH-05 UT-ELIG-TRUST-03", trace),
    ["E2E-UI-AUTH-05", "UT-ELIG-TRUST-03", "UT-UI-RBAC-01"], "deleted test families must not disappear");
  const input = { evidencePath: "docs/evidence/W-0010/README.md", evidenceText: "REAL_CUSTOMER_CALL_ALLOWED=NO",
    trace, results: indexResults([{ className: "XTests", name: "First", outcome: "Passed" }]),
    sweep: { ok: true, reason: "full sweep" }, runCheck: { ok: true, reason: "bound run" } };
  const row = { id: "W-0010", prompt: "P2-1", status: "TESTS_PASS", residual: "" };
  assert.equal(judge(row, input).c2.ok, false, "zero cited tests cannot pass C2");
  const duplicate = traceability("| `UT-X` | 2 |\n| `UT-X-01` | unit | `Missing` | `tests/XTests.cs` |\n| `UT-X-01` | unit | `First` | `tests/XTests.cs` |\n");
  assert.equal(testVerdict("UT-X-01", duplicate, input.results).ok, false,
    "every definition of a TestId must run, not just the last traceability row");
  let checks = 3;
  const slashTrace = traceability("| `UT-X` | 3 |\n| `UT-X-01` | unit | `First` | `tests/XTests.cs` |\n| `UT-X-02` | unit | `Second` | `tests/XTests.cs` |\n| `UT-X-03` | unit | `Third` | `tests/XTests.cs` |\n");
  assert.deepEqual(citedTestIds("IT-API-TERMINATE-09/10", trace),
    ["IT-API-TERMINATE-09", "IT-API-TERMINATE-10"], "W-0207 shorthand must retain its second test"); checks += 1;
  assert.deepEqual(citedTestIds("UT-X-01/3/01", trace), ["UT-X-01", "UT-X-03"],
    "slash is a list, not a range; preserve width and deduplicate"); checks += 1;
  assert.deepEqual(citedTestIds("CT-CI-06/06b/06d/06e/06f", trace),
    ["CT-CI-06", "CT-CI-06b", "CT-CI-06d", "CT-CI-06e", "CT-CI-06f"]); checks += 1;
  assert.deepEqual(citedTestIds("CT-CI-06..06h", trace),
    ["CT-CI-06", "CT-CI-06b", "CT-CI-06c", "CT-CI-06d", "CT-CI-06e", "CT-CI-06f", "CT-CI-06g", "CT-CI-06h"],
    "named shell assertion ranges use the fixed runner registry; the first assertion has no a suffix"); checks += 1;
  assert.deepEqual(citedTestIds("CT-CI-06b…06d/06h", trace),
    ["CT-CI-06b", "CT-CI-06c", "CT-CI-06d", "CT-CI-06h"]); checks += 1;
  for (const unknownRange of ["CT-CI-06a..06h", "CT-CI-06..06z", "UT-X-01..01h"]) {
    assert.throws(() => citedTestIds(unknownRange, trace), /invalid TestId/u,
      "named assertion ranges require both endpoints in the same fixed runner registry"); checks += 1;
  }
  assert.deepEqual(citedTestIds("UT-X-01/03..05/07", trace),
    ["UT-X-01", "UT-X-03", "UT-X-04", "UT-X-05", "UT-X-07"]); checks += 1;
  assert.deepEqual(citedTestIds("UT-X-01..03/05", trace),
    ["UT-X-01", "UT-X-02", "UT-X-03", "UT-X-05"]); checks += 1;
  assert.deepEqual(citedTestIds("W-0010/11 OD-V1-20/21 UT-X-01/IT-NEW-02", trace),
    ["IT-NEW-02", "UT-X-01"], "full IDs and non-test slash notation remain distinct"); checks += 1;
  for (const malformed of ["UT-X-01/02bad", "UT-X-01/03..02", "UT-X-01/03..9999", "UT-X-01/02b..04"]) {
    assert.throws(() => citedTestIds(malformed, trace), /invalid TestId/u,
      "a malformed continuation must not certify a valid prefix"); checks += 1;
  }
  const slashInput = { ...input, trace: slashTrace, evidenceText: `${input.evidenceText}\nUT-X-01/02` };
  assert.equal(judge(row, slashInput).c2.ok, false, "missing slash test results must reject C2"); checks += 1;
  for (const outcome of ["Failed", "NotExecuted"]) {
    assert.equal(judge(row, { ...slashInput, results: indexResults([
      { className: "XTests", name: "First", outcome: "Passed" },
      { className: "XTests", name: "Second", outcome },
    ]) }).c2.ok, false, `${outcome} slash test must reject C2`); checks += 1;
  }
  assert.equal(judge(row, { ...slashInput, results: indexResults([
    { className: "XTests", name: "First", outcome: "Passed" },
    { className: "XTests", name: "Second", outcome: "Passed" },
  ]) }).c2.ok, true, "all slash tests green can satisfy C2"); checks += 1;
  assert.equal(judge(row, { ...input, evidenceText: `${input.evidenceText}\nUT-X-01/99` }).c2.ok, false,
    "a slash test absent from traceability must reject C2"); checks += 1;
  assert.throws(() => citedTestIds("UT-X-01..9999", trace), /invalid TestId range/u); checks += 1;
  assert.throws(() => citedTestIds("UT-X-03..01", trace), /invalid TestId range/u); checks += 1;
  assert.deepEqual(citedTestIds("W-0010 OD-V1-20 M3-14 A-0663 UT-NEW-FAMILY-01", trace), ["UT-NEW-FAMILY-01"]); checks += 1;
  assert.equal(testVerdict("UT-X-01", trace, indexResults([{ className: "OtherTests", name: "First", outcome: "Passed" }])).ok,
    false, "a same-named method from a different source class cannot satisfy the test"); checks += 1;
  const documents = new Map([
    ["docs/evidence/W-0010/test-report.md", "UT-X-01"],
  ]);
  const collect = (overrides = {}) => collectTestEvidence({ workId: row.id, evidencePath: input.evidencePath,
    evidenceText: `${input.evidenceText}\n[tests](test-report.md)`, trace, extract: citedTestIds,
    read: (file) => documents.get(file) ?? null, ...overrides });
  const linked = collect();
  assert.deepEqual(linked.ids, ["UT-X-01"]);
  assert.equal(judge(row, { ...input, testEvidence: linked }).c2.ok, true); checks += 1;
  assert.equal(judge(row, { ...input, testEvidence: collect({ evidenceText: `${input.evidenceText}\n[tests](absent.md)` }) }).c2.ok, false); checks += 1;
  assert.equal(judge(row, { ...input, testEvidence: collect({ promptText: "UT-UI-RBAC-01" }) }).c2.ok, false,
    "backend-only evidence cannot satisfy mandatory UI tests in the prompt"); checks += 1;
  documents.set("docs/evidence/W-0010/test-report.md", "[cycle](README.md)\nUT-X-01");
  assert.equal(collect().documents.length, 2, "cyclic attachments are read once"); checks += 1;
  const planPath = "docs/evidence/W-0010/acceptance-tests.json";
  const plan = { schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software", requiredTestIds: ["UT-X-01"],
    retirements: [{ testId: "UT-UI-OLD-01", reason: "Owner retired the old behavior in a recorded decision.",
      decision: { commit: "a".repeat(40), path: "docs/evidence/W-0099/README.md", sha256: "b".repeat(64) },
      replacementTestIds: ["UT-X-01"] }] };
  const save = (value) => documents.set(planPath, JSON.stringify(value));
  save(plan);
  let validated = 0;
  const historical = { evidenceText: `${input.evidenceText}\nUT-UI-OLD-01`, validateDecision: () => { validated += 1; } };
  const replaced = collect(historical);
  assert.equal(validated, 1);
  assert.deepEqual(replaced.retired, ["UT-UI-OLD-01"]);
  assert.equal(judge(row, { ...input, testEvidence: replaced }).c2.ok, true); checks += 1;
  assert.equal(judge(row, { ...input, testEvidence: replaced,
    results: indexResults([{ className: "XTests", name: "First", outcome: "Failed" }]) }).c2.ok, false); checks += 1;
  assert(collect({ ...historical, validateDecision: () => { throw Error("decision hash mismatch"); } }).errors.length); checks += 1;
  assert(collect({ ...historical, validateDecision: undefined }).errors.length); checks += 1;
  save({ ...plan, retirements: [{ ...plan.retirements[0], replacementTestIds: ["UT-X-99"] }] });
  assert(collect(historical).errors.length); checks += 1;
  save({ ...plan, retirements: [{ ...plan.retirements[0], testId: "UT-X-01" }] });
  assert(collect({ ...historical, evidenceText: "UT-X-01" }).errors.length); checks += 1;
  save({ ...plan, retirements: [plan.retirements[0], plan.retirements[0]] });
  assert(collect(historical).errors.length); checks += 1;
  save({ ...plan, workId: "W-9999" }); assert(collect(historical).errors.length); checks += 1;
  documents.set(planPath, "{broken"); assert(collect(historical).errors.length); checks += 1;
  save({ schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "retired-ui", decision: plan.retirements[0].decision,
    proposedDisposition: "CANCELLED" });
  const retiredUi = collect(historical);
  assert.match(retiredUi.errors[0], /chờ Toàn/u);
  assert.equal(judge(row, { ...input, testEvidence: retiredUi }).c2.ok, false,
    "a valid retirement is a closeout proposal, never UI acceptance"); checks += 1;
  const gateEvidence = { ...input, evidenceText: `${input.evidenceText}\nCT-CI-06`,
    gateManifest: { gates: { "selftest-pii.sh": { argv: ["sh"] } } } };
  assert.equal(judge(row, gateEvidence).c2.ok, true); checks += 1;
  assert.equal(judge(row, { ...gateEvidence, gateManifest: { gates: {} } }).c2.ok, false); checks += 1;
  assert.equal(judge(row, { ...gateEvidence, sweep: { ok: false } }).c2.ok, false); checks += 1;
  assert.equal(judge(row, { ...input, evidenceText: `${input.evidenceText}\nCT-CI-999` }).c2.ok, false); checks += 1;

  // W-0346: registered runner assertions, plain and conditional, and the rules the registry obeys.
  const capacity = { gates: { "capacity-selftest.mjs": { argv: ["--self-test"] } } };
  const plain = judge(row, { ...input, evidenceText: `${input.evidenceText}\nCT-CI-05`,
    gateManifest: { gates: { "ci-config-selftest.mjs": { argv: ["--self-test"] } } } });
  assert.equal(plain.c2.ok, true);
  assert.deepEqual(plain.conditional, []);
  assert.equal(plain.verdict, "ĐẠT", "a plain runner PASS with nothing left to read passes"); checks += 1;
  const conditional = judge(row, { ...input, evidenceText: `${input.evidenceText}\nCAP-CALIB-03`, gateManifest: capacity });
  assert.equal(conditional.c2.ok, true);
  assert.equal(conditional.verdict, "XEM", "a conditional runner PASS is shown to the owner, never passed silently");
  assert.match(conditional.conditional.join("; "), /^CAP-CALIB-03 PASS_UNCALIBRATED/u); checks += 1;
  assert.equal(judge(row, { ...input, evidenceText: `${input.evidenceText}\nCAP-CALIB-03`, gateManifest: { gates: {} } }).c2.ok,
    false, "a conditional PASS still needs its runner in the sweep"); checks += 1;
  assert.equal(judge(row, { ...input, evidenceText: `${input.evidenceText}\nCAP-CALIB-03 UT-X-99`, gateManifest: capacity })
    .verdict, "KHÔNG ĐẠT", "a condition never softens another failure"); checks += 1;

  const runner = [
    'process.stdout.write("UT-Z-01 PASS — plain\\n");',
    'process.stdout.write(`UT-Z-02 ${ok ? "PASS_A" : "PASS_B"} — conditional\\n`);',
    "// UT-Z-03 PASS — a comment is not a result",
    'process.stdout.write("UT-Z-04b PASS — a longer ID does not print UT-Z-04\\n");',
  ].join("\n");
  const registry = (overrides) => gateRegistryErrors({
    manifest: { gates: { "z.mjs": { argv: [] }, "off.mjs": { sweepable: false } } },
    traced: new Map([["UT-Z-09", []]]), source: (file) => (file === "z.mjs" ? runner : null), wholeGate: [],
    registry: { "UT-Z-01": "z.mjs", "UT-Z-02": "z.mjs" }, conditions: { "UT-Z-02": "PASS_A hoặc PASS_B" }, extended: {},
    ...overrides });
  assert.deepEqual(registry({}), []); checks += 1;
  for (const [overrides, rejected] of [
    [{ registry: { "UT-Z-01": "off.mjs" }, conditions: {} }, /không chạy trong full sweep/u],
    [{ registry: { "UT-Z-09": "z.mjs" }, conditions: {} }, /test \.NET/u],
    [{ registry: { "UT-Z-03": "z.mjs" }, conditions: {} }, /không in "UT-Z-03 PASS"/u],
    [{ registry: { "UT-Z-04": "z.mjs" }, conditions: {} }, /không in "UT-Z-04 PASS"/u],
    [{ registry: { "UT-Z-02": "z.mjs" }, conditions: {} }, /có điều kiện .* chưa khai báo/u],
    [{ registry: { "UT-Z-01": "z.mjs" }, conditions: { "UT-Z-01": "x" } }, /chỉ in PASS/u],
    [{ registry: { "UT-Z-01": "z.mjs" }, conditions: { "UT-Z-05": "x" } }, /không có trong registry/u],
  ]) {
    assert.match(registry(overrides).join("; "), rejected); checks += 1;
  }
  assert.deepEqual(registry({ registry: { "UT-Z-08": "z.mjs" }, conditions: {}, wholeGate: ["UT-Z-08"] }), [],
    "a whole-gate test needs its runner in the sweep, not a per-ID line"); checks += 1;

  // W-0349: a gate the work built, and work that is documents only.
  const ownGate = { gates: { "own-validator.mjs": { argv: ["--self-test"] } } };
  const gatePlan = { schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software", gates: ["own-validator.mjs"] };
  const named = { evidenceText: `${input.evidenceText}\nChạy deploy/ci/scripts/own-validator.mjs --self-test.` };
  save(gatePlan);
  const built = collect(named);
  assert.deepEqual(built.gates, ["own-validator.mjs"]);
  const builtVerdict = judge(row, { ...input, testEvidence: built, gateManifest: ownGate });
  assert.equal(builtVerdict.c2.ok, true);
  assert.equal(builtVerdict.verdict, "ĐẠT", "a gate the work built passes through its own self-test"); checks += 1;
  assert.equal(judge(row, { ...input, testEvidence: built, gateManifest: { gates: {} } }).c2.ok, false,
    "a declared gate outside the full sweep proves nothing"); checks += 1;
  assert.equal(judge(row, { ...input, testEvidence: built, gateManifest: ownGate, sweep: { ok: false, reason: "red" } }).c2.ok,
    false, "a declared gate fails with the sweep"); checks += 1;
  assert.match(collect().errors.join("; "), /never names it/u, "the evidence must name a declared gate"); checks += 1;
  save({ ...gatePlan, gates: ["../own-validator.mjs"] });
  assert(collect(named).errors.length, "a gate is a runner name, not a path"); checks += 1;

  documents.set("docs/plan/decision.md", "# Quyết định");
  const documentPlan = { schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "document",
    reason: "Việc chỉ soạn phiếu quyết định, không đổi code, test hay gate.", deliverables: ["docs/plan/decision.md"] };
  save(documentPlan);
  const paper = collect({ evidenceText: input.evidenceText });
  const paperVerdict = judge(row, { ...input, testEvidence: paper });
  assert.equal(paperVerdict.c2.ok, true);
  assert.equal(paperVerdict.verdict, "XEM", "the owner reads a document even when nothing residual is left");
  assert.match(paperVerdict.documentNote, /^Việc tài liệu/u); checks += 1;
  assert.equal(judge(row, { ...input, testEvidence: paper, sweep: { ok: false, reason: "red" } }).c2.ok, false,
    "documents still need the documentation gates of the sweep"); checks += 1;
  assert.equal(judge(row, { ...input, testEvidence: collect({ evidenceText: `${input.evidenceText}\nUT-X-99` }) }).c2.ok,
    false, "a document plan does not excuse a cited test that is missing"); checks += 1;
  for (const broken of [
    { deliverables: ["docs/plan/absent.md"] },
    { deliverables: ["docs/../secret.md"] },
    { deliverables: ["docs/plan"] },
    { deliverables: [] },
    { reason: "ngắn" },
    { scope: "software" },
    { scope: "paper" },
  ]) {
    save({ ...documentPlan, ...broken });
    assert(collect({ evidenceText: input.evidenceText }).errors.length,
      `an invalid document plan must be refused: ${JSON.stringify(broken)}`); checks += 1;
  }
  documents.set("docs/evidence/W-0010/run.log", "PASS 20/20");
  save({ ...documentPlan, scope: "lab-run", reason: "Lượt chạy trên máy lab; công cụ lab không gate nào chạy.",
    deliverables: ["docs/evidence/W-0010/run.log"] });
  const lab = judge(row, { ...input, testEvidence: collect({ evidenceText: input.evidenceText }) });
  assert.equal(lab.c2.ok, true);
  assert.equal(lab.verdict, "XEM");
  assert.match(lab.documentNote, /^Bằng chứng chạy lab/u, "a lab run is named as one, not as a document"); checks += 1;
  save({ ...documentPlan, scope: "lab-run", deliverables: [] });
  assert(collect({ evidenceText: input.evidenceText }).errors.length, "a lab run lists the files it produced"); checks += 1;
  save({ schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software" });
  assert.match(judge(row, { ...input, testEvidence: collect({ evidenceText: input.evidenceText }) }).c2.reason,
    /không có TestId/u, "a plan that declares nothing still has no claim"); checks += 1;

  // W-0351: IDs a pack names without claiming them, and a surface removed with no replacement.
  const both = { evidenceText: `${input.evidenceText}\nUT-X-01 UT-GONE-09` };
  const mention = { testId: "UT-GONE-09", reason: "Ví dụ trong bảng luật, không phải test của việc này." };
  save({ schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software", mentions: [mention] });
  const named2 = collect(both);
  assert.deepEqual([named2.ids, named2.mentioned], [["UT-X-01"], ["UT-GONE-09"]]);
  const namedVerdict = judge(row, { ...input, testEvidence: named2 });
  assert.equal(namedVerdict.c2.ok, true);
  assert.equal(namedVerdict.verdict, "XEM", "a mention is shown to the owner, never passed silently");
  assert.match(namedVerdict.readNotes.join(" "), /UT-GONE-09/u); checks += 1;
  for (const [broken, why] of [
    [{ testId: "UT-X-01", reason: mention.reason }, "a live test cannot be turned into a mention"],
    [{ testId: "UT-GONE-09", reason: "ngắn" }, "a mention needs a reason"],
    [{ testId: "UT-NEVER-07", reason: mention.reason }, "a mention must be named by the evidence"],
  ]) {
    save({ schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software", mentions: [broken] });
    assert(collect(both).errors.length, why); checks += 1;
  }
  save({ schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software", mentions: [mention] });
  assert.match(judge(row, { ...input, testEvidence: collect({ evidenceText: `${input.evidenceText}\nUT-GONE-09` }) }).c2.reason,
    /không có TestId/u, "mentions alone are no claim"); checks += 1;
  const surface = { testId: "UT-UI-OLD-01", reason: "Owner gỡ cả console; không còn giao diện nào để test.",
    decision: plan.retirements[0].decision, replacementTestIds: [], surfaceRemoved: true };
  save({ schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software", retirements: [surface] });
  const uiBoth = { evidenceText: `${input.evidenceText}\nUT-X-01 UT-UI-OLD-01`, validateDecision: () => { validated += 1; } };
  const before = validated;
  const gone = collect(uiBoth);
  assert.equal(validated, before + 1, "a surface retirement still needs its pinned decision");
  const goneVerdict = judge(row, { ...input, testEvidence: gone });
  assert.equal(goneVerdict.c2.ok, true);
  assert.equal(goneVerdict.verdict, "XEM", "a removed surface is shown to the owner");
  assert.deepEqual(goneVerdict.retired, [], "a removed surface is not listed as replaced"); checks += 1;
  save({ schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software", retirements: [{ ...surface, surfaceRemoved: false }] });
  assert(collect(uiBoth).errors.length, "no replacement without a removed surface"); checks += 1;
  save({ schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software", retirements: [surface] });
  assert.match(judge(row, { ...input, testEvidence: collect({ ...uiBoth, evidenceText: `${input.evidenceText}\nUT-UI-OLD-01` }) })
    .c2.reason, /không có TestId/u, "a removed surface alone is no claim"); checks += 1;

  // W-0352: a TestId of a gate the offline sweep skips, through the bundle's extended run only.
  const k8s = { ...input, evidenceText: `${input.evidenceText}\nIT-K8S-LINT-01`,
    gateManifest: { gates: { "k8s-selftest.mjs": { sweepable: false, extended: [{ argv: [], expect: "K8S_SELFTEST_PASS" }] } } } };
  const printedGreen = { ok: true, reason: "EXTENDED_SWEEP_PASS 1/1",
    printed: new Map([["k8s-selftest.mjs", new Set(["IT-K8S-LINT-01"])]]) };
  const unchecked = judge(row, k8s);
  assert.equal(unchecked.verdict, "CHƯA KIỂM", "without the extended run the TestId is unchecked, not failed");
  assert.match(unchecked.c2.reason, /--extended/u); checks += 1;
  const extendedGreen = judge(row, { ...k8s, extended: printedGreen });
  assert.equal(extendedGreen.c2.ok, true);
  assert.equal(extendedGreen.verdict, "ĐẠT", "a TestId its own gate printed as PASS in the extended run passes"); checks += 1;
  for (const [overrides, why] of [
    [{ extended: { ...printedGreen, printed: new Map([["image-selftest.mjs", new Set(["IT-K8S-LINT-01"])]]) } },
      "the PASS line has to come from the owning gate's own output"],
    [{ extended: { ...printedGreen, printed: new Map([["k8s-selftest.mjs", new Set()]]) } },
      "a gate that ended green without printing this TestId did not prove it"],
    [{ extended: { ok: false, reason: "red", printed: new Map() } }, "a failed extended run proves nothing"],
    [{ extended: printedGreen, gateManifest: { gates: { "k8s-selftest.mjs": { argv: [] } } } },
      "a runner with no extended invocation proves nothing"],
    [{ extended: printedGreen, runCheck: { ok: false, reason: "rejected bundle" } }, "a rejected bundle proves nothing"],
  ]) {
    assert.equal(judge(row, { ...k8s, ...overrides }).c2.ok, false, why); checks += 1;
  }
  save({ schema: TEST_PLAN_SCHEMA, workId: row.id, scope: "software",
    mentions: [{ testId: "IT-K8S-LINT-01", reason: mention.reason }] });
  assert(collect({ evidenceText: `${input.evidenceText}\nUT-X-01 IT-K8S-LINT-01` }).errors.length,
    "an extended gate TestId is a claim, never a mention"); checks += 1;
  const importedLibrary = [
    'process.stdout.write("UT-Z-12 PASS — printed by the module the runner imports\\n");',
    "// UT-Z-13 PASS — a comment is not a result",
  ].join("\n");
  const extendedRegistry = (overrides) => gateRegistryErrors({
    manifest: { gates: { "x.mjs": { sweepable: false, extended: [{ argv: [], expect: "X_PASS" }] }, "z.mjs": { argv: [] } } },
    traced: new Map([["UT-Z-09", []]]), wholeGate: [], registry: {}, conditions: {},
    source: (file) => ({ "x.mjs": 'import { check } from "./lib.mjs";\nprocess.stdout.write("UT-Z-11 PASS\\n");',
      "lib.mjs": importedLibrary })[file] ?? null,
    extended: { "UT-Z-11": "x.mjs", "UT-Z-12": "x.mjs" }, ...overrides });
  assert.deepEqual(extendedRegistry({}), [], "the runner or a module it imports may print the PASS line"); checks += 1;
  for (const [overrides, rejected] of [
    [{ extended: { "UT-Z-11": "z.mjs" } }, /không chạy trong lượt gate mở rộng/u],
    [{ extended: { "UT-Z-09": "x.mjs" } }, /đã có chủ khác/u],
    [{ extended: { "UT-Z-13": "x.mjs" } }, /không in "UT-Z-13 PASS"/u],
  ]) {
    assert.match(extendedRegistry(overrides).join("; "), rejected); checks += 1;
  }
  return checks;
}
