// W-0060 / P11-4 — the readiness view, derived from the tracker and checked against it.
//
// Two modes, one source of truth:
//   --write   regenerate docs/release/gate-status.yaml and docs/release/readiness-board.md
//   (default) verify the committed artefacts still match the tracker, and fail if they do not
//
// Why derived rather than written: P11-4 §3 forbids a second backlog, and the way a mirror becomes
// a second backlog is not a decision — it is a month of the tracker moving while the board does
// not. Regenerating from the tracker and failing on drift makes that impossible rather than
// discouraged.
//
// Three things this refuses to do, all from §3:
//   - no percentage readiness. A percentage invites "94% ready" to be read as nearly done, when the
//     remaining 6% is every gate nobody can close.
//   - no independent status. Every readiness row carries the Work ID it mirrors.
//   - no rung is claimed. The ladder is displayed with its entry conditions and the first rung is
//     marked NOT_ATTAINED, because evidence submitted is not evidence accepted (MASTER-05).
import assert from "node:assert/strict";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../../..");
const write = process.argv.includes("--write");

const TRACKER = "prompt/_execution/prompt-execution-tracker.md";
const DECISIONS = "specs/_review/open-decisions-register.md";
const STATUS_YAML = "docs/release/gate-status.yaml";
const BOARD = "docs/release/readiness-board.md";

/**
 * The four outcome levels, with the condition that admits entry. Conditions are stated so a reader
 * can check the claim rather than take it, and so nobody has to guess what rung 2 would require.
 */
const LADDER = [
  {
    level: "IMPLEMENTATION_COMPLETE_BEHIND_MOCKS",
    entry: "every planned prompt has ACCEPTED evidence and no gate is BLOCKED_INTERNAL",
  },
  {
    level: "LAB_REAL_SIM_VERIFIED",
    entry: "one real SIM has completed the lab protocol with allowlist and kill switch evidence",
  },
  {
    level: "REAL_SALES_INTEGRATION_VERIFIED",
    entry: "Target V1 is signed and contract tests run against a real Sales sandbox",
  },
  {
    level: "PRODUCTION_REAL_ELIGIBLE",
    entry: "32 eSIM capacity measured, legal/security evidence accepted, DF-03 signed",
  },
];

/** §2.5 of the prompt: the go/no-go inputs, each mapped to the work that owns it. */
const GO_NO_GO = [
  ["two-program Sales flow", "W-0002"],
  ["speech payload and dial token", "W-0003, W-0004"],
  ["callback and auth", "W-0005, W-0006"],
  ["attempt policy", "W-0007"],
  ["one-SIM lab", "W-0008"],
  ["32 eSIM production capacity", "W-0008"],
  ["legal, security and release evidence", "W-0009"],
];

async function read(relativePath) {
  return fs.readFile(path.join(repositoryRoot, relativePath), "utf8");
}

/** One section of the tracker, from its heading to the next one named. */
function section(markdown, startHeading, endHeading) {
  const start = markdown.indexOf(startHeading);
  assert(start >= 0, `tracker has no section "${startHeading}".`);
  const end = markdown.indexOf(endHeading, start + startHeading.length);
  assert(end > start, `tracker has no section "${endHeading}" after "${startHeading}".`);
  return markdown.slice(start, end);
}

function rows(markdown, prefix) {
  return markdown
    .split("\n")
    .filter((line) => line.startsWith(`| \`${prefix}`))
    .map((line) => line.split("|").slice(1, -1).map((cell) => cell.trim()));
}

function unquote(cell) {
  return cell.replaceAll("`", "").replaceAll("*", "").trim();
}

async function collect() {
  const tracker = await read(TRACKER);
  const decisions = await read(DECISIONS);

  // Gates from tracker section 3. Columns: id, gate, owner, status, mock path, closure evidence.
  const gates = rows(tracker, "G-").map((cells) => ({
    id: unquote(cells[0]),
    gate: cells[1],
    owner: cells[2],
    status: unquote(cells[3]),
    closureEvidence: cells[5] ?? "",
  }));
  assert(gates.length > 0, "no G-* gates found in the tracker; the parser is reading the wrong file.");

  // Section 4 and section 5 both hold W-* rows and they do NOT share a column layout. Parsing the
  // whole file at once put a section-4 deliverable description into the status column, which the
  // board then displayed as a status -- visible only because the board groups by status and three
  // sentences appeared where a state belonged. So each region is sliced out and read with its own
  // map.
  const external = section(tracker, "## 4. External request register", "## 5.");
  const planned = section(tracker, "## 5. Planned implementation register", "## 6.");

  // Section 4: id, request, owner, status, deliverable, fallback, next action.
  const externalWork = rows(external, "W-").map((cells) => ({
    id: unquote(cells[0]),
    prompt: "EXTERNAL",
    status: unquote(cells[3]),
    residual: cells[6] ?? "",
  }));

  // Section 5: id, prompt, scope, prereq, status, owner, artifacts, tests, residual.
  const plannedWork = rows(planned, "W-").map((cells) => ({
    id: unquote(cells[0]),
    prompt: unquote(cells[1]),
    status: unquote(cells[4]),
    residual: cells[8] ?? "",
  }));

  const work = [...externalWork, ...plannedWork];
  assert(work.length > 0, "no W-* rows found in the tracker.");

  // Statuses are a closed vocabulary (tracker section 1). Anything else means the parser has
  // drifted from the document again, and a readiness board built on a misread status is worse than
  // no board.
  const known = new Set([
    "PLANNED", "NOT_STARTED", "IN_PROGRESS", "CODE_DONE", "TESTS_PASS", "EVIDENCE_SUBMITTED",
    "ACCEPTED", "BLOCKED_INTERNAL", "BLOCKED_EXTERNAL", "DEFERRED_TARGET", "N/A", "CANCELLED",
  ]);
  const unknown = work.filter((row) => !known.has(row.status));
  assert.deepEqual(
    unknown.map((row) => `${row.id}=${row.status}`),
    [],
    "the tracker parser read something that is not a status; the column map has drifted.");

  const decisionIds = [...decisions.matchAll(/^\| `(OD-V1-\d{2})`/gmu)].map((match) => match[1]);
  assert(decisionIds.length > 0, "no OD-V1-* decisions found in the register.");

  return { gates, work, decisions: decisionIds };
}

/**
 * The ladder in this script is a hand-written list, and W-0059 recorded that shape as a real gap in
 * a sibling check. The prompt itself names the four levels in its definition of done, so the list
 * is compared against the prompt rather than trusted: a board displaying levels the prompt does not
 * define, or in a different order, is a board answering a different question.
 */
async function assertLadderMatchesThePrompt() {
  const prompt = await read(
    "prompt/phase-11-production-closure/P11-4-production-readiness-command-center.md");
  const declared = [...prompt.matchAll(/`([A-Z_]+(?:_[A-Z]+)*)`/g)]
    .map((match) => match[1])
    .filter((token) => token.endsWith("_MOCKS") || token.endsWith("_VERIFIED")
      || token.endsWith("_ELIGIBLE"));

  assert.deepEqual(
    LADDER.map((rung) => rung.level),
    declared,
    "the ladder in this script does not match the four levels the prompt defines, in order.");
}

/**
 * Every go/no-go input must name a Work ID the tracker actually has. An input mapped to a work item
 * that does not exist is a mapping that will never close, and it would sit in the board looking
 * exactly like one that could.
 */
function assertGoNoGoMapsToRealWork(work) {
  const known = new Set(work.map((row) => row.id));
  const dangling = [];
  for (const [input, workIds] of GO_NO_GO) {
    for (const workId of workIds.split(",").map((value) => value.trim())) {
      if (!known.has(workId)) {
        dangling.push(`${input} -> ${workId}`);
      }
    }
  }
  assert.deepEqual(dangling, [], `go/no-go inputs reference work that does not exist: ${dangling}`);

  // The prompt bolds this one, because it is the input most likely to be dropped as "later".
  assert(
    GO_NO_GO.some(([input]) => input.includes("32 eSIM")),
    "the 32 eSIM production capacity input is missing from the go/no-go list.");
}

async function evidenceFor(workId) {
  try {
    await fs.access(path.join(repositoryRoot, "docs/evidence", workId, "README.md"));
    return `docs/evidence/${workId}/README.md`;
  } catch {
    return null;
  }
}

function yamlString(value) {
  // Quote everything and escape the two characters that would break a double-quoted scalar. The
  // status file is machine-readable input for P0-4 and P9-1, so a stray colon in a gate name must
  // not silently produce a different document.
  return `"${String(value).replaceAll("\\", "\\\\").replaceAll('"', '\\"')}"`;
}

async function render({ gates, work, decisions }) {
  const accepted = work.filter((row) => row.status === "ACCEPTED");
  const blockedExternal = work.filter((row) => row.status === "BLOCKED_EXTERNAL");
  const openGates = gates.filter((gate) => gate.status !== "CLOSED");

  const lines = [
    "# W-0060 / P11-4 — machine-readable gate status.",
    "#",
    "# GENERATED from prompt/_execution/prompt-execution-tracker.md. Do not edit by hand: the",
    "# generator runs in CI and fails the build if this file and the tracker disagree, so a manual",
    "# edit here becomes a red pipeline rather than a second source of truth.",
    "#",
    "# Consumers: the P0-4 runtime guardrail and the P9-1 release gate.",
    `generated_from: ${yamlString(TRACKER)}`,
    "generator: \"deploy/ci/scripts/gate-status.mjs\"",
    "",
    "# No percentage appears anywhere in this file, by decision (P11-4 section 3). A percentage",
    "# invites \"94% ready\" to be read as nearly done, when the remaining 6% is every gate nobody",
    "# can close.",
    "ladder:",
  ];

  for (const [index, rung] of LADDER.entries()) {
    lines.push(
      `  - level: ${yamlString(rung.level)}`,
      `    rung: ${index + 1}`,
      `    entry_condition: ${yamlString(rung.entry)}`,
      "    attained: false",
    );
  }

  lines.push(
    "",
    "# Why rung 1 is not attained, stated rather than left to inference.",
    "current_position:",
    "  attained_rung: 0",
    `  reason: ${yamlString(
      `${accepted.length} of ${work.length} work items are ACCEPTED; the rest are at most `
      + "EVIDENCE_SUBMITTED, and submitted evidence is not accepted evidence (MASTER-05)")}`,
    `  blocked_external_count: ${blockedExternal.length}`,
    `  open_gate_count: ${openGates.length}`,
    "  # Rows whose evidence lives in the tracker row rather than in an evidence pack: unplanned",
    "  # remediation work, which has no prompt and therefore no section 10 requirement. Counted so",
    "  # the difference stays visible instead of being read as missing evidence.",
    `  rows_without_evidence_pack: ${
      work.filter((row) => !/^P\d+-\d+$/.test(row.prompt)).length}`,
    "",
    "gates:",
  );

  for (const gate of gates) {
    lines.push(
      `  - id: ${yamlString(gate.id)}`,
      `    owner: ${yamlString(gate.owner)}`,
      `    status: ${yamlString(gate.status)}`,
      `    closure_evidence_required: ${yamlString(gate.closureEvidence)}`,
    );
  }

  lines.push("", "open_decisions:");
  for (const decision of decisions) {
    lines.push(`  - id: ${yamlString(decision)}`);
  }

  lines.push("", "go_no_go_inputs:");
  for (const [input, workIds] of GO_NO_GO) {
    lines.push(
      `  - input: ${yamlString(input)}`,
      `    work_ids: ${yamlString(workIds)}`,
    );
  }

  lines.push("", "# Every work item, mirrored. Status is never computed here.", "work:");
  for (const row of work) {
    const evidence = await evidenceFor(row.id);
    lines.push(
      `  - id: ${yamlString(row.id)}`,
      `    prompt: ${yamlString(row.prompt)}`,
      `    status: ${yamlString(row.status)}`,
      `    evidence: ${evidence === null ? "null" : yamlString(evidence)}`,
    );
  }

  lines.push(
    "",
    "production_flag:",
    "  real_customer_call_allowed: false",
    "  mutable_by_this_file: false",
    "  note: \"This file reports. It never sets a flag (P11-4 section 3).\"",
    "",
  );

  return lines.join("\n");
}

async function renderBoard({ gates, work, decisions }) {
  const byStatus = new Map();
  for (const row of work) {
    byStatus.set(row.status, (byStatus.get(row.status) ?? 0) + 1);
  }

  const blocked = work.filter((row) => row.status === "BLOCKED_EXTERNAL");
  const openGates = gates.filter((gate) => gate.status !== "CLOSED");

  const lines = [
    "# Production readiness board — `W-0060` · `P11-4`",
    "",
    `Sinh từ \`${TRACKER}\` bởi \`deploy/ci/scripts/gate-status.mjs\`. **Không sửa tay** — CI đối`,
    "chiếu và đỏ nếu hai bên lệch.",
    "",
    "## 1. Đây là gương, không phải backlog thứ hai",
    "",
    "Tracker là nguồn duy nhất. Bảng này **không** có trạng thái riêng: mỗi dòng mang đúng Work ID",
    "và trạng thái của tracker. Cách một tấm gương biến thành backlog thứ hai không phải là một",
    "quyết định — nó là một tháng tracker đi tiếp còn bảng thì không.",
    "",
    "**Không có phần trăm ở đâu cả.** Một con số phần trăm mời người đọc hiểu \"94% xong\" là gần",
    "xong, trong khi 6% còn lại là toàn bộ những cổng **không ai đóng được**.",
    "",
    "## 2. Bốn nấc, và nấc đang đứng",
    "",
    "| Nấc | Điều kiện vào | Đạt chưa |",
    "| --- | --- | --- |",
  ];

  for (const [index, rung] of LADDER.entries()) {
    lines.push(`| ${index + 1}. \`${rung.level}\` | ${rung.entry} | ❌ |`);
  }

  const accepted = work.filter((row) => row.status === "ACCEPTED").length;
  lines.push(
    "",
    `**Đang ở nấc 0.** ${accepted}/${work.length} work item ở trạng thái \`ACCEPTED\`; phần còn lại`,
    "cao nhất là `EVIDENCE_SUBMITTED`, và **evidence đã nộp không phải evidence đã được chấp nhận**",
    "(`MASTER-05`). Chỉ Release owner chuyển sang `ACCEPTED`.",
    "",
    "## 3. Phân bố trạng thái (đếm, không phải tỉ lệ)",
    "",
    "| Trạng thái | Số work item |",
    "| --- | --- |",
  );

  for (const [status, count] of [...byStatus.entries()].sort((a, b) => b[1] - a[1])) {
    lines.push(`| \`${status}\` | ${count} |`);
  }

  lines.push(
    "",
    "## 4. Cổng còn mở",
    "",
    "| Gate | Chủ sở hữu | Trạng thái | Đóng bằng gì |",
    "| --- | --- | --- | --- |",
  );
  for (const gate of openGates) {
    lines.push(`| \`${gate.id}\` | ${gate.owner} | \`${gate.status}\` | ${gate.closureEvidence} |`);
  }

  lines.push(
    "",
    "## 5. Đầu vào go/no-go (`P11-4` §2.5)",
    "",
    "| Đầu vào | Work ID |",
    "| --- | --- |",
  );
  for (const [input, workIds] of GO_NO_GO) {
    lines.push(`| ${input} | ${workIds} |`);
  }

  lines.push(
    "",
    `Cả bảy đầu vào đều chưa đạt. **${blocked.length}** work item ở \`BLOCKED_EXTERNAL\`, và`,
    `**${decisions.length}** quyết định \`OD-V1-*\` còn mở.`,
    "",
    "## 6. Kill switch và rollback",
    "",
    "| | Trạng thái |",
    "| --- | --- |",
    "| `REAL_CUSTOMER_CALL_ALLOWED` | `false` ở **cả 4** môi trường, ép lúc render chart |",
    "| kill switch | bắt buộc bật khi chế độ khác `MOCK`, ép lúc render |",
    "| rollback | `helm rollback --atomic` + `after_script`; **chưa lượt deploy nào từng chạy** |",
    "| cắt ngang cuộc đang gọi | W-0111: Admin/Operator có `IVR_CALL_TERMINATE`; API ghi yêu cầu, worker poll (mặc định ≤500 ms) rồi gateway hang up. Đây là cơ chế riêng, không gộp vào kill switch; mới có evidence software/MOCK, chưa phải SIM/carrier UAT |",
    "",
    "## 7. Cái bảng này KHÔNG nói",
    "",
    "- **Không nói \"xong hết prompt\" là sẵn sàng go-live.** Nấc 1 còn chưa đạt, và nó là nấc thấp",
    "  nhất trong bốn nấc.",
    "- **Không tự bật cờ nào.** File này báo cáo; nó không đặt giá trị (`P11-4` §3).",
    "- **Không đóng cổng ngoài bằng một báo cáo.** Chỉ artifact thật đóng được (`P11-4` §3).",
    "- **Không kiểm chất lượng của evidence.** Nó kiểm evidence **có tồn tại** và trạng thái được",
    "  mirror đúng; nó không đọc nội dung.",
    "",
  );

  return lines.join("\n");
}

// ------------------------------------------------------------------------ main

const collected = await collect();
await assertLadderMatchesThePrompt();
assertGoNoGoMapsToRealWork(collected.work);
const yaml = await render(collected);
const board = await renderBoard(collected);

if (write) {
  await fs.mkdir(path.join(repositoryRoot, "docs/release"), { recursive: true });
  await fs.writeFile(path.join(repositoryRoot, STATUS_YAML), yaml, "utf8");
  await fs.writeFile(path.join(repositoryRoot, BOARD), board, "utf8");
  process.stdout.write(
    `GATE_STATUS_WRITTEN gates=${collected.gates.length} work=${collected.work.length} `
    + `decisions=${collected.decisions.length}\n`);
} else {
  const committedYaml = await read(STATUS_YAML);
  const committedBoard = await read(BOARD);

  assert.equal(
    committedYaml,
    yaml,
    `${STATUS_YAML} does not match the tracker. Regenerate with --write; do not edit it by hand.`);
  assert.equal(
    committedBoard,
    board,
    `${BOARD} does not match the tracker. Regenerate with --write.`);

  // Every readiness item maps to a Work ID, and no rung is claimed.
  assert(
    !/attained: true/.test(yaml),
    "a ladder rung is marked attained; no rung has been reached and none may be claimed.");
  // Values and table cells only. The first version of this check tested the whole file and went
  // red on the comment explaining why percentages are forbidden -- the rule is about readiness
  // expressed as a percentage, not about the word appearing in prose that argues against it. So it
  // reads the data: YAML lines that are not comments, and board lines that are table rows.
  const yamlValues = yaml.split("\n").filter((line) => !line.trimStart().startsWith("#"));
  const boardCells = board.split("\n").filter((line) => line.startsWith("|"));
  const percentages = [...yamlValues, ...boardCells].filter((line) => /\d+\s?%/.test(line));
  assert.deepEqual(
    percentages,
    [],
    "a percentage appeared in a readiness value; P11-4 section 3 forbids it: "
    + percentages.join(" / "));
  assert(
    /real_customer_call_allowed: false/.test(yaml),
    "the readiness view no longer reports the production flag as false.");

  // Prompt-backed slices only. A prompt has a DoD demanding a section 10 evidence pack; an
  // unplanned remediation row does not, and its evidence has always lived in the tracker row
  // itself. The first version of this check demanded a directory for all 103 rows and flagged 20
  // remediation items -- a rule that would have been satisfied by creating 20 empty directories,
  // which is the opposite of what it is for. The remediation rows are counted in the YAML instead,
  // so they stay visible rather than becoming invisible.
  const missingEvidence = collected.work
    .filter((row) => /^P\d+-\d+$/.test(row.prompt))
    .filter((row) => ["TESTS_PASS", "EVIDENCE_SUBMITTED", "ACCEPTED"].includes(row.status))
    .filter((row) => !yaml.includes(`docs/evidence/${row.id}/README.md`));
  assert.deepEqual(
    missingEvidence.map((row) => row.id),
    [],
    "prompt-backed work items claim progressed status with no evidence pack: "
    + missingEvidence.map((row) => row.id).join(", "));

  process.stdout.write(
    `GATE_STATUS_PASS — ${collected.gates.length} gates, ${collected.work.length} work items and `
    + `${collected.decisions.length} open decisions mirrored from the tracker; no rung claimed, no `
    + "percentage, production flag reported false\n");
}
