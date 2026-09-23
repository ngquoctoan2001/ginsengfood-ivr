import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { c2SelfTest } from "./acceptance-c2-checks.mjs";
import {
  collectTestEvidence, GATE_TEST_CONDITIONS, GATE_TESTS, gateRegistryErrors, READ_ONLY_SCOPES,
} from "./acceptance-test-plan.mjs";
import {
  GATE_MANIFEST, fullSweepVerdict, expectedTestAssemblies, readPinnedArtifact, validateRunEvidence, sha256,
} from "./acceptance-evidence-lib.mjs";

// W-0318 / Lô 5 of plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md (T7): the batch list the release
// owner reads before accepting work, one batch per phase. Read-only by design. It never edits the
// tracker: moving a row to ACCEPTED stays the owner's act (tracker section 1 rule 6, MASTER-05), and
// a script that did it would turn "the criteria pass" into "accepted" without anyone having read the
// evidence.
//
// Everything is read from HEAD, not from the working tree, so the list describes a commit that can be
// named, and another session's uncommitted edits cannot change a verdict. --worktree reads the
// working tree instead, for checking a change before it is committed.
//
// The four criteria are the ones in tracker section 1:
//   C1  the evidence README exists at the path gate-status.yaml names, and says
//       REAL_CUSTOMER_CALL_ALLOWED=NO
//   C2  required TestIds from the prompt, README and same-pack attachments are live and passed in
//       a --evidence bundle captured at the same commit, or have a pinned retirement with tested
//       replacements. An empty test claim or a retired UI never passes as current software. A
//       runner assertion that passed only under a stated condition is shown as XEM with it. A
//       gate the work built counts through its self-test in the sweep; work that is documents only
//       declares them and is always shown as XEM (W-0349)
//   C3  Residual/next holds nothing IVR still has to do, only external waits. A script cannot judge
//       that, so a non-empty cell is shown for the owner to read (XEM) and never passed silently
//   C4  the captured sweep ran the exact set of gates in that commit's manifest
//
//   node tools/dev/collect-acceptance-evidence.mjs --out <new-directory>
//   node deploy/ci/scripts/acceptance-batches.mjs [--evidence <acceptance-run.json>] [--phase P2]
//                                                 [--worktree] [--root <repository>]
//   node deploy/ci/scripts/acceptance-batches.mjs --self-test

const TRACKER = "prompt/_execution/prompt-execution-tracker.md";
const STATUS_YAML = "docs/release/gate-status.yaml";
const TRACEABILITY = "docs/traceability-tests.md";
const MARKER = "REAL_CUSTOMER_CALL_ALLOWED=NO";
const OLDER_WORDING = /real[- ]customer calls?: `?NO`?|REAL_CUSTOMER_CALL_ALLOWED=false/iu;

// Rows the owner could accept today. CODE_DONE has not passed its tests; BLOCKED_* waits on
// something; the terminal statuses are already decided.
const CANDIDATES = new Set(["TESTS_PASS", "EVIDENCE_SUBMITTED"]);
const KNOWN_STATUSES = new Set([
  "PLANNED", "NOT_STARTED", "IN_PROGRESS", "CODE_DONE", "TESTS_PASS", "EVIDENCE_SUBMITTED",
  "ACCEPTED", "BLOCKED_INTERNAL", "BLOCKED_EXTERNAL", "DEFERRED_TARGET", "N/A", "CANCELLED",
]);
const PASS = "ĐẠT";
const REVIEW = "XEM";
const FAIL = "KHÔNG ĐẠT";
const UNCHECKED = "CHƯA KIỂM";

function unquote(cell) {
  return cell.replaceAll("`", "").replaceAll("*", "").trim();
}

function normalise(text) {
  return text === null ? null : text.replace(/\r\n/gu, "\n");
}

/** Section 5 of the tracker, read with the same column map gate-status.mjs uses. */
export function plannedRows(tracker) {
  const start = tracker.indexOf("## 5. Planned implementation register");
  const end = tracker.indexOf("\n## 6.", start);
  assert(start >= 0 && end > start, "tracker has no section 5 followed by section 6.");
  const rows = tracker
    .slice(start, end)
    .split("\n")
    .filter((line) => line.startsWith("| `W-"))
    .map((line) => {
      const cells = line.split("|").slice(1, -1).map((cell) => cell.trim());
      return {
        id: unquote(cells[0]),
        prompt: unquote(cells[1] ?? ""),
        status: unquote(cells[4] ?? ""),
        residual: cells[8] ?? "",
      };
    });
  assert(rows.length > 0, "no W-* rows in tracker section 5; the parser is reading the wrong file.");

  // A misread column would put prose where a status belongs and quietly drop the row from every
  // batch. Same guard as gate-status.mjs, for the same reason.
  const unknown = rows.filter((row) => !KNOWN_STATUSES.has(row.status));
  assert.deepEqual(
    unknown.map((row) => `${row.id}=${row.status}`),
    [],
    "tracker section 5 has a status outside the closed vocabulary; the column map has drifted.");
  return rows;
}

/** P0..P11 for a planned prompt (`P2-1`), UNPLANNED for everything else. */
export function phaseOf(prompt) {
  const match = /^P(\d{1,2})-\d+[a-z]?$/u.exec(prompt);
  return match ? `P${Number(match[1])}` : "UNPLANNED";
}

/** Work ID -> evidence path (or null) from gate-status.yaml. */
export function evidencePaths(yaml) {
  const paths = new Map();
  let current = null;
  for (const line of yaml.split("\n")) {
    const id = /^ {2}- id: "(W-\d+)"$/u.exec(line);
    if (id) {
      current = id[1];
      continue;
    }

    const evidence = /^ {4}evidence: (?:"([^"]+)"|null)$/u.exec(line);
    if (evidence && current) {
      paths.set(current, evidence[1] ?? null);
      current = null;
    }
  }

  assert(paths.size > 0, "gate-status.yaml has no work item with an evidence field.");
  return paths;
}

/** The TestIds docs/traceability-tests.md knows, and the prefixes it groups them by. */
export function traceability(markdown) {
  const byId = new Map();
  for (const match of markdown.matchAll(
    /^\| `([A-Z][A-Z0-9-]*)` \| (unit|integration|contract|chaos) \| `([^`]*)` \| `([^`]+)` \|$/gmu)) {
    byId.set(match[1], [...(byId.get(match[1]) ?? []), { method: match[3], file: match[4] }]);
  }

  const prefixes = new Set();
  for (const match of markdown.matchAll(/^\| `([A-Z][A-Z0-9-]*)` \| \d+ \|$/gmu)) {
    prefixes.add(match[1]);
  }

  assert(byId.size > 0 && prefixes.size > 0, "docs/traceability-tests.md has no TestId rows.");
  return { byId, prefixes };
}

function prefixOf(token, prefixes) {
  // Test families remain test families when their last live definition is deleted. Numeric work,
  // decision and activity IDs (W/OD/A/M3) are deliberately not test namespaces.
  if (/^(?:UT|IT|CT|E2E|UI|COMP|SEC|PT|BI|CAP|DR|CHAOS|ARCH|DG)-.*-\d/iu.test(token)) return "test";
  for (const prefix of prefixes) {
    if (token.startsWith(`${prefix}-`) && /(?:^|-)\d/u.test(token.slice(prefix.length + 1))) {
      return prefix;
    }
  }

  return null;
}

/**
 * TestIds cited by evidence or its prompt, including deleted families. Expand numeric ranges and
 * slash suffixes (09/10, 06/06b). Invalid continuations fail instead of certifying a partial claim.
 */
export function citedTestIds(text, trace) {
  const found = new Set();
  const token = /(?<![A-Za-z0-9-])([A-Z][A-Z0-9]*(?:-[A-Z0-9]+)+(?:[a-z])?)(?![A-Za-z0-9-])/gu;
  for (const match of text.matchAll(token)) {
    const id = match[1];
    const prefix = trace.byId.has(id) ? "known" : prefixOf(id, trace.prefixes);
    if (prefix === null) {
      continue;
    }

    found.add(id);
    const start = /^(.*-)(\d+)([a-z]?)$/u.exec(id);
    if (!start) continue;
    let tail = text.slice(match.index + id.length);
    let from = Number(start[2]);
    let letter = start[3];
    while (/^(?:\/\d|(?:\.\.|…)\d)/u.test(tail)) {
      const slash = /^\/(\d+)([a-z]?)(?![A-Za-z0-9-])/u.exec(tail);
      if (slash) {
        from = Number(slash[1]);
        letter = slash[2];
        assert(Number.isSafeInteger(from), `invalid TestId suffix: ${id}${tail}`);
        found.add(`${start[1]}${slash[1].padStart(start[2].length, "0")}${letter}`);
        tail = tail.slice(slash[0].length);
        continue;
      }
      const named = /^(?:\.\.|…)(\d+)([a-z])(?![A-Za-z0-9-])/u.exec(tail);
      if (named) {
        // Historical shell groups such as CT-CI-06..06h are named assertions, not numeric
        // intervals: the first assertion is 06, followed by 06b (there is no 06a). Only the
        // fixed gate registry may define membership; an arbitrary missing endpoint must fail.
        const group = `${start[1]}${String(from).padStart(start[2].length, "0")}`;
        const gate = GATE_TESTS[`${group}${letter}`];
        assert(Number(named[1]) === from && named[2] >= letter && gate
          && GATE_TESTS[`${group}${named[2]}`] === gate, `invalid TestId named range: ${id}${tail}`);
        for (const [member, runner] of Object.entries(GATE_TESTS)) {
          const suffix = member.slice(group.length);
          if (runner === gate && member.startsWith(group) && /^[a-z]?$/u.test(suffix)
            && suffix >= letter && suffix <= named[2]) found.add(member);
        }
        letter = named[2];
        tail = tail.slice(named[0].length);
        continue;
      }
      const range = /^(?:\.\.|…)(\d+)(?![A-Za-z0-9-])/u.exec(tail);
      const to = Number(range?.[1]);
      assert(range && !letter && Number.isSafeInteger(from) && Number.isSafeInteger(to)
        && to >= from && to - from <= 500, `invalid TestId range or suffix: ${id}${tail}`);
      for (let number = from + 1; number <= to; number += 1) {
        found.add(`${start[1]}${String(number).padStart(start[2].length, "0")}`);
      }
      from = to;
      tail = tail.slice(range[0].length);
    }
  }

  return [...found].sort();
}

function attributes(tag) {
  const decoded = {};
  for (const match of tag.matchAll(/([A-Za-z_:][\w:.-]*)="([^"]*)"/gu)) {
    decoded[match[1]] = match[2]
      .replaceAll("&lt;", "<").replaceAll("&gt;", ">").replaceAll("&quot;", "\"")
      .replaceAll("&apos;", "'").replaceAll("&amp;", "&");
  }

  return decoded;
}

/**
 * Outcomes from a VSTest .trx file, keyed by the method each result ran. The result row itself only
 * carries a display name, which a DisplayName attribute replaces, so the method comes from the test
 * definition the result points at.
 */
export function trxOutcomes(xml) {
  const methods = new Map();
  for (const match of xml.matchAll(/<UnitTest\b([^>]*)>([\s\S]*?)<\/UnitTest>/gu)) {
    const method = /<TestMethod\b([^>]*?)\/?>/u.exec(match[2]);
    const id = attributes(match[1]).id;
    if (id && method) {
      const { className = "", name = "" } = attributes(method[1]);
      methods.set(id, { className, name });
    }
  }

  const outcomes = [];
  for (const match of xml.matchAll(/<UnitTestResult\b([^>]*?)\/?>/gu)) {
    const { testId, outcome } = attributes(match[1]);
    const method = methods.get(testId);
    if (method) {
      outcomes.push({ ...method, outcome: outcome ?? "Unknown" });
    }
  }

  return outcomes;
}

function shortClass(className) {
  return className.split(",")[0].split(".").at(-1).split("+")[0];
}

function classOfFile(file) {
  return path.posix.basename(file, ".cs").split(".")[0];
}

/** Index results by method name once, since every cited TestId is looked up. */
export function indexResults(outcomes) {
  const byMethod = new Map();
  const byClass = new Map();
  for (const outcome of outcomes) {
    byMethod.set(outcome.name, [...(byMethod.get(outcome.name) ?? []), outcome]);
    const owner = shortClass(outcome.className);
    byClass.set(owner, [...(byClass.get(owner) ?? []), outcome]);
  }

  return { byMethod, byClass, count: outcomes.length };
}

export function testVerdict(testId, trace, results) {
  const entries = trace.byId.get(testId);
  if (!entries) {
    return { ok: false, reason: `${testId} không có trong ${TRACEABILITY}` };
  }

  if (!results) {
    return { ok: null, reason: "chưa có kết quả test đã xác minh (--evidence)" };
  }

  for (const entry of entries) {
    const owners = trace.sourceClasses?.get(entry.file) ?? [classOfFile(entry.file)];
    if (owners.length === 0) return { ok: false, reason: `${testId}: thiếu source class ${entry.file}` };
    let pool;
    let note = "";
    if (entry.method) {
      const sameMethod = results.byMethod.get(entry.method) ?? [];
      pool = sameMethod.filter((outcome) => owners.includes(shortClass(outcome.className)));
    } else {
      // The generator could not name the method for a handful of TestIds. The source still names
      // its classes, so every result of those classes has to be green instead.
      pool = owners.flatMap((owner) => results.byClass.get(owner) ?? []);
      note = ` (traceability không ghi method; đã xét mọi class trong ${entry.file})`;
    }

    if (pool.length === 0) {
      return { ok: false, reason: `${testId} không có trong kết quả test${note}` };
    }

    const failed = pool.filter((outcome) => outcome.outcome !== "Passed");
    if (failed.length > 0) {
      return { ok: false, reason: `${testId} ${failed[0].outcome}${note}` };
    }
  }
  return { ok: true, reason: "" };
}

export function sweepVerdict(log, manifest) {
  return fullSweepVerdict(log, manifest);
}

export function residualVerdict(cell) {
  const text = unquote(cell).replace(/\s+/gu, " ");
  // Nearly every row ends with the marker. On its own it says nothing is left to do, so it does not
  // make a row something the owner has to read.
  const withoutMarker = text.replaceAll(MARKER, "").replace(/^[\s·;.,—–-]+|[\s·;.,—–-]+$/gu, "");
  if (/^(?:|none|không)$/iu.test(withoutMarker)) {
    return { ok: true, reason: "" };
  }

  return { ok: "review", reason: text.length > 220 ? `${text.slice(0, 217)}…` : text };
}

function joinReasons(failures) {
  const shown = failures.slice(0, 3).map((item) => item.reason);
  if (failures.length > 3) {
    shown.push(`và ${failures.length - 3} TestId khác`);
  }

  return shown.join("; ");
}

/** One row against the four criteria. `sweep` is shared by every row of the same commit. */
export function judge(row, { evidencePath, evidenceText, trace, results, sweep, runCheck, testEvidence, gateManifest }) {
  let c1;
  if (!evidencePath) {
    c1 = { ok: false, reason: "gate-status.yaml không trỏ tới gói bằng chứng nào" };
  } else if (evidenceText === null) {
    c1 = { ok: false, reason: `không có ${evidencePath}` };
  } else if (!evidenceText.includes(MARKER)) {
    // Early packs say the same thing in other words. The criterion asks for the marker, so they still
    // fail, but the reason says the statement is there, so the fix reads as a one-line edit rather
    // than as missing evidence.
    const older = OLDER_WORDING.exec(evidenceText);
    c1 = {
      ok: false,
      reason: `${evidencePath} thiếu ${MARKER}${older ? ` (gói ghi dạng cũ: "${older[0].replaceAll("`", "")}")` : ""}`,
    };
  } else {
    c1 = { ok: true, reason: "" };
  }

  const cited = testEvidence?.ids ?? (evidenceText ? citedTestIds(evidenceText, trace) : []);
  const declaredGates = testEvidence?.gates ?? [];
  const deliverables = testEvidence?.deliverables ?? [];
  const documentOnly = READ_ONLY_SCOPES.includes(testEvidence?.scope);
  const labRun = testEvidence?.scope === "lab-run";
  // Everything that is not a .NET result stands or falls with the verified full sweep of this commit.
  const bySweep = (runner, reason) => ({
    ok: runner && !Array.isArray(gateManifest?.gates?.[runner]?.argv) ? false
      : runCheck?.ok !== true ? (runCheck?.ok ?? null) : sweep.ok,
    reason,
  });
  const verdicts = [
    ...cited.map((id) => GATE_TESTS[id]
      ? bySweep(GATE_TESTS[id], `${id} cần full sweep có ${GATE_TESTS[id]}`)
      : testVerdict(id, trace, results)),
    // W-0349: a gate the work built is tested by its own self-test in the sweep.
    ...declaredGates.map((gate) => bySweep(gate, `${gate} cần chạy và đạt trong full sweep`)),
    // A document has no test of its own; the documentation and PII gates of the sweep can still fail it.
    ...(documentOnly ? [bySweep(null, "tài liệu cần full sweep đạt")] : []),
  ];
  // A conditional gate PASS still counts for C2, but the owner reads the condition (W-0346).
  const conditional = cited.filter((id) => GATE_TESTS[id] && Object.hasOwn(GATE_TEST_CONDITIONS, id))
    .map((id) => `${id} ${GATE_TEST_CONDITIONS[id]}`);
  const documentNote = !documentOnly ? null : labRun
    ? `Bằng chứng chạy lab, không có test phần mềm trong sweep (${deliverables.length} tệp có tại commit): ${testEvidence.reason}`
    : `Việc tài liệu, không có test phần mềm (${deliverables.length} tài liệu có tại commit): ${testEvidence.reason}`;
  const failures = verdicts.filter((verdict) => verdict.ok === false);
  let c2;
  if (testEvidence?.errors?.length) {
    c2 = { ok: false, reason: testEvidence.errors.join("; ") };
  } else if (cited.length === 0 && declaredGates.length === 0 && !documentOnly) {
    c2 = { ok: false, reason: "không có TestId hoặc khai báo kiểm chứng để xét C2" };
  } else if (failures.length > 0) {
    c2 = { ok: false, reason: joinReasons(failures) };
  } else if (runCheck?.ok !== true) {
    c2 = runCheck ?? { ok: null, reason: "chưa có gói kết quả gắn với commit (--evidence)" };
  } else if (verdicts.some((verdict) => verdict.ok === null)) {
    c2 = { ok: null, reason: "chưa có kết quả test đã xác minh (--evidence)" };
  } else {
    const parts = [
      ...(cited.length ? [`${cited.length} TestId xanh`] : []),
      ...(testEvidence?.retired?.length ? [`${testEvidence.retired.length} ID lịch sử có quyết định thay thế`] : []),
      ...(conditional.length ? [`${conditional.length} gate đạt có điều kiện`] : []),
      ...(declaredGates.length ? [`${declaredGates.length} gate của chính việc này đạt trong full sweep`] : []),
      ...(documentOnly ? [`${deliverables.length} ${labRun ? "tệp bằng chứng lab" : "tài liệu"} có tại commit`] : []),
    ];
    c2 = { ok: true, reason: parts.join("; ") };
  }

  const c3 = residualVerdict(row.residual);
  const hard = [c1, c2, sweep];
  let verdict = PASS;
  if (hard.some((check) => check.ok === false)) {
    verdict = FAIL;
  } else if (hard.some((check) => check.ok === null)) {
    verdict = UNCHECKED;
  } else if (c3.ok === "review" || conditional.length > 0 || documentOnly) {
    verdict = REVIEW;
  }

  return { id: row.id, status: row.status, phase: phaseOf(row.prompt), cited, gates: declaredGates,
    retired: testEvidence?.retired ?? [], conditional, documentNote, c1, c2, c3, verdict };
}

function phaseOrder(phase) {
  return phase === "UNPLANNED" ? 99 : Number(phase.slice(1));
}

function mark(check) {
  if (check.ok === true) {
    return "✅";
  }

  return check.ok === null ? "⏳" : check.ok === "review" ? "👀" : "❌";
}

function cellText(text) {
  return text.replaceAll("|", "\\|");
}

/** The report the owner reads. Markdown, Vietnamese, one table per batch. */
export function render({ head, dirtyNote, rows, judged, sweep, resultsNote, phaseFilter }) {
  const lines = [];
  lines.push(`# Danh sách đề nghị nghiệm thu — \`${head}\``, "");
  lines.push("Script **chỉ đọc**: không sửa tracker. Chỉ Toàn chuyển một dòng sang `ACCEPTED`, sau khi");
  lines.push("đọc bằng chứng — danh sách này không thay cho việc đọc.", "");
  if (dirtyNote) {
    lines.push(`> ⚠️ ${dirtyNote}`, "");
  }

  lines.push("| Nguồn | Trạng thái |", "| --- | --- |");
  lines.push(`| Kết quả test (\`C2\`) | ${resultsNote} |`);
  lines.push(`| Gate sweep (\`C4\`) | ${mark(sweep)} ${sweep.reason} |`, "");

  const planned = rows.filter((row) => row.prompt && phaseOf(row.prompt) !== "UNPLANNED");
  const accepted = planned.filter((row) => row.status === "ACCEPTED" || row.status === "N/A"
    || row.status === "CANCELLED");
  const blockedInternal = rows.filter((row) => row.status === "BLOCKED_INTERNAL");
  lines.push("## Nấc 1", "");
  lines.push(`Prompt đã lên kế hoạch đã xong (\`ACCEPTED\`, \`N/A\`, \`CANCELLED\`): **${accepted.length}/${planned.length}**`);
  lines.push(`· \`BLOCKED_INTERNAL\`: **${blockedInternal.length}**. Nấc 1 đạt khi hai số này là`);
  lines.push(`**${planned.length}/${planned.length}** và **0**.`, "");

  const phases = [...new Set(judged.map((item) => item.phase))]
    .filter((phase) => !phaseFilter || phase === phaseFilter)
    .sort((left, right) => phaseOrder(left) - phaseOrder(right));

  lines.push("## Tổng theo đợt", "");
  lines.push(`| Đợt | Ứng viên | ${PASS} | ${REVIEW} | ${FAIL} | ${UNCHECKED} |`, "| --- | ---: | ---: | ---: | ---: | ---: |");
  for (const phase of phases) {
    const batch = judged.filter((item) => item.phase === phase);
    const count = (verdict) => batch.filter((item) => item.verdict === verdict).length;
    lines.push(`| \`${phase}\` | ${batch.length} | ${count(PASS)} | ${count(REVIEW)} | ${count(FAIL)} | ${count(UNCHECKED)} |`);
  }

  lines.push("");
  lines.push(`\`${PASS}\`: đủ bốn điều. \`${REVIEW}\`: C1, C2, C4 đạt, còn cột Residual, điều kiện kèm PASS của gate, hoặc`);
  lines.push(`nội dung của việc thuần tài liệu cần Toàn đọc. \`${FAIL}\`: hỏng ít nhất một điều, lý do ở dưới. \`${UNCHECKED}\`: thiếu kết quả test`);
  lines.push("hoặc log gate sweep để kết luận.", "");

  for (const phase of phases) {
    const batch = judged.filter((item) => item.phase === phase)
      .sort((left, right) => left.id.localeCompare(right.id));
    lines.push(`## Đợt \`${phase}\``, "");
    lines.push("| Work ID | Trạng thái | C1 bằng chứng | C2 test | C3 residual | Kết luận |", "| --- | --- | :---: | :---: | :---: | --- |");
    for (const item of batch) {
      lines.push(`| \`${item.id}\` | \`${item.status}\` | ${mark(item.c1)} | ${mark(item.c2)} | ${mark(item.c3)} | **${item.verdict}** |`);
    }

    const explained = batch.filter((item) => item.verdict !== PASS);
    if (explained.length > 0) {
      lines.push("");
      for (const item of explained) {
        const reasons = [item.c1, item.c2].filter((check) => check.ok === false || check.ok === null)
          .map((check) => check.reason);
        if (item.retired?.length) reasons.push(`Test lịch sử đã thay thế: ${item.retired.join(", ")}; xem acceptance-tests.json trong gói bằng chứng`);
        if (item.c2.ok === true && item.conditional?.length) reasons.push(`Gate đạt có điều kiện: ${item.conditional.join("; ")}`);
        if (item.c2.ok === true && item.documentNote) reasons.push(item.documentNote);
        if (item.c3.ok === "review") {
          reasons.push(`Residual: ${item.c3.reason}`);
        }

        lines.push(`- \`${item.id}\` — ${cellText(reasons.join(" · "))}`);
      }
    }

    lines.push("");
  }

  return `${lines.join("\n")}\n`;
}

function git(root, args, input) {
  const result = spawnSync("git", args, { cwd: root, input, maxBuffer: 1 << 30, windowsHide: true });
  assert.equal(result.status, 0, `git ${args.join(" ")} failed: ${result.stderr?.toString() ?? ""}`);
  return result.stdout;
}

/** Many files at HEAD through one `git cat-file --batch`, rather than one process per file. */
function readAtHead(root, files, commit) {
  const output = git(root, ["cat-file", "--batch"], `${files.map((file) => `${commit}:${file}`).join("\n")}\n`);
  const texts = new Map();
  let offset = 0;
  for (const file of files) {
    const newline = output.indexOf(0x0a, offset);
    const header = output.subarray(offset, newline).toString("utf8");
    offset = newline + 1;
    if (header.endsWith(" missing")) {
      texts.set(file, null);
      continue;
    }

    const size = Number(header.split(" ")[2]);
    texts.set(file, output.subarray(offset, offset + size).toString("utf8"));
    offset += size + 1;
  }

  return texts;
}

function readWorktree(root, files) {
  const texts = new Map();
  for (const file of files) {
    try {
      texts.set(file, normalise(fs.readFileSync(path.join(root, file), "utf8")));
    } catch {
      texts.set(file, null);
    }
  }

  return texts;
}

/**
 * gateRegistryErrors() over the runners as `read` sees them. Only the self-test calls it: a gate
 * TestId counts for C2 only with a verified full sweep, and that sweep runs this self-test.
 */
function registryErrors(read, manifest, trace) {
  const file = (runner) => `deploy/ci/scripts/${runner}`;
  const texts = read([...new Set(Object.values(GATE_TESTS))].map(file));
  return gateRegistryErrors({ manifest, traced: trace.byId, source: (runner) => normalise(texts.get(file(runner)) ?? null) });
}

function argument(name) {
  const index = process.argv.indexOf(name);
  return index === -1 ? null : process.argv[index + 1];
}

function main() {
  const root = path.resolve(argument("--root")
    ?? path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../.."));
  const fromWorktree = process.argv.includes("--worktree");
  // Resolve HEAD once. A concurrent commit must not mix tracker and evidence from different trees.
  const head = git(root, ["rev-parse", "HEAD"]).toString("utf8").trim();
  const source = { commit: head, tree: git(root, ["rev-parse", `${head}^{tree}`]).toString("utf8").trim() };
  const pinnedRead = (files) => readAtHead(root, files, head);
  const read = fromWorktree ? (files) => readWorktree(root, files) : pinnedRead;
  assert(!process.argv.includes("--trx") && !process.argv.includes("--sweep-log"),
    "Unbound --trx/--sweep-log results cannot prove a commit. Run tools/dev/collect-acceptance-evidence.mjs, then pass --evidence <acceptance-run.json>.");
  const dirty = git(root, ["status", "--porcelain"]).toString("utf8").trim().split("\n").filter(Boolean);
  const dirtyNote = fromWorktree
    ? `Đọc cây làm việc (--worktree), không phải HEAD: có ${dirty.length} file chưa commit, nên kết quả không gắn với commit nào.`
    : dirty.length > 0
      ? `Cây làm việc có ${dirty.length} file chưa commit. Danh sách đọc từ commit ${head}; chỉ nhận kết quả đã thu trên cây sạch tại đúng commit đó.`
      : "";

  const base = read([TRACKER, STATUS_YAML, TRACEABILITY]);
  const rows = plannedRows(normalise(base.get(TRACKER)));
  const evidence = evidencePaths(normalise(base.get(STATUS_YAML)));
  const trace = traceability(normalise(base.get(TRACEABILITY)));
  const sourceFiles = [...new Set([...trace.byId.values()].flat().map((entry) => entry.file))];
  const sources = read(sourceFiles);
  trace.sourceClasses = new Map(sourceFiles.map((file) => [file,
    [...(sources.get(file) ?? "").matchAll(/\bclass\s+(\w+)/gu)].map((match) => match[1])]));
  const gateManifest = JSON.parse(read([GATE_MANIFEST]).get(GATE_MANIFEST));
  const trackedPaths = git(root, ["ls-tree", "-r", "--name-only", head, "prompt"]).toString("utf8").trim().split("\n");
  const promptPaths = trackedPaths.filter((file) => /\/P\d+-\d+[a-z]?-.*\.md$/u.test(file));
  const prompts = read(promptPaths);
  const cache = new Map();
  const readOne = (file) => {
    if (!cache.has(file)) cache.set(file, normalise(read([file]).get(file)));
    return cache.get(file);
  };
  const validateDecision = (decision) => {
    assert.match(decision?.commit ?? "", /^[0-9a-f]{40}$/u, "decision needs a full commit");
    assert.match(decision?.sha256 ?? "", /^[0-9a-f]{64}$/u, "decision needs a pinned hash");
    assert(typeof decision.path === "string" && !decision.path.includes("..")
      && /^(?:docs|plan|specs)\/[A-Za-z0-9_./-]+\.md$/u.test(decision.path),
      "decision must be a repository-relative evidence or decision document");
    git(root, ["merge-base", "--is-ancestor", decision.commit, head]);
    const historical = readAtHead(root, [decision.path], decision.commit).get(decision.path);
    assert.equal(typeof historical, "string", "decision document missing at pinned commit");
    assert.equal(sha256(historical), decision.sha256, "decision document hash mismatch");
    assert(typeof decision.quote === "string" && decision.quote.trim().length >= 20
      && historical.includes(decision.quote), "decision quote is missing from the pinned document");
  };

  const candidates = rows.filter((row) => CANDIDATES.has(row.status));
  const paths = [...new Set(candidates.map((row) => evidence.get(row.id)).filter(Boolean))];
  const texts = read(paths);

  let results = null;
  let resultsNote = "⏳ chưa có — chạy `node tools/dev/collect-acceptance-evidence.mjs --out <thư mục mới>`, rồi đưa `--evidence <acceptance-run.json>`";
  let runCheck = { ok: null, reason: "chưa có gói kết quả gắn với commit (--evidence)" };
  let sweep = sweepVerdict(null);
  const evidenceFile = argument("--evidence");
  if (evidenceFile) {
    try {
      assert(!fromWorktree, "--worktree is inspection only and cannot be used for acceptance");
      const file = path.resolve(evidenceFile);
      const bundle = JSON.parse(fs.readFileSync(file, "utf8"));
      const pinned = pinnedRead([GATE_MANIFEST, "Ivr.sln"]);
      const assemblies = expectedTestAssemblies(pinned.get("Ivr.sln"), (project) => pinnedRead([project]).get(project));
      const verified = validateRunEvidence(bundle, {
        source, manifestBytes: pinned.get(GATE_MANIFEST), assemblies,
        readArtifact: (artifact) => readPinnedArtifact(path.dirname(file), artifact),
      });
      results = indexResults(verified.texts.flatMap(trxOutcomes));
      assert.equal(results.count, verified.count, "TRX result parsing is incomplete");
      resultsNote = `${verified.texts.length} file \`.trx\`, ${results.count} kết quả · SHA/hash/đủ project đã kiểm`;
      runCheck = { ok: true, reason: "" };
      sweep = verified.sweep;
    } catch (error) {
      const reason = `gói kết quả bị từ chối: ${error.message.split("\n")[0]}`;
      results = null;
      resultsNote = `❌ ${reason}`;
      runCheck = { ok: false, reason };
      sweep = { ok: false, reason };
      process.exitCode = 1;
    }
  }

  const judged = candidates.map((row) => judge(row, {
    evidencePath: evidence.get(row.id) ?? null,
    evidenceText: evidence.get(row.id) ? normalise(texts.get(evidence.get(row.id))) : null,
    trace,
    results,
    sweep,
    runCheck,
    gateManifest,
    testEvidence: collectTestEvidence({ workId: row.id, evidencePath: evidence.get(row.id),
      evidenceText: evidence.get(row.id) ? normalise(texts.get(evidence.get(row.id))) : null,
      promptText: prompts.get(promptPaths.find((file) => path.posix.basename(file).startsWith(`${row.prompt}-`))) ?? "",
      trace, extract: citedTestIds, read: readOne, validateDecision }),
  }));

  process.stdout.write(render({
    head, dirtyNote, rows, judged, sweep, resultsNote, phaseFilter: argument("--phase"),
  }));
}

function selfTest() {
  const tracker = [
    "## 5. Planned implementation register",
    "",
    "| Work ID | Prompt | Scope summary | Prereq | Status | Owner | Artifacts/MR | Tests/evidence | Residual/next |",
    "| --- | --- | --- | --- | --- | --- | --- | --- | --- |",
    "| `W-0010` | `P0-1` | a | — | TESTS_PASS | o | a | t | — |",
    "| `W-0011` | `P0-2` | a | — | EVIDENCE_SUBMITTED | o | a | t | Chờ Module 3 trả lời `M3-14` |",
    "| `W-0012` | `P0-3` | a | — | TESTS_PASS | o | a | t | — |",
    "| `W-0013` | `P1-1` | a | — | TESTS_PASS | o | a | t | — |",
    "| `W-0014` | `P1-2` | a | — | TESTS_PASS | o | a | t | — |",
    "| `W-0015` | `P1-3` | a | — | TESTS_PASS | o | a | t | — |",
    "| `W-0016` | Sửa lỗi (Origin=`UNPLANNED`) | a | — | TESTS_PASS | o | a | t | — |",
    "| `W-0017` | `P2-1` | a | — | ACCEPTED | o | a | t | — |",
    "| `W-0018` | `P2-2` | a | — | CODE_DONE | o | a | t | — |",
    "| `W-0019` | Tài liệu (Origin=`UNPLANNED`) | a | — | EVIDENCE_SUBMITTED | o | a | t | — |",
    "",
    "## 6. Unplanned work insertion template",
  ].join("\n");
  const yaml = ["W-0010", "W-0011", "W-0012", "W-0014", "W-0015", "W-0016", "W-0017", "W-0018", "W-0019"]
    .map((id) => `  - id: "${id}"\n    prompt: "p"\n    status: "TESTS_PASS"\n    evidence: "docs/evidence/${id}/README.md"`)
    .concat(['  - id: "W-0013"\n    prompt: "p"\n    status: "TESTS_PASS"\n    evidence: null'])
    .join("\n");
  const trace = traceability([
    "| Prefix | Tagged tests |", "| --- | ---: |", "| `UT-X` | 4 |", "| `UT-BOOT` | 1 |", "",
    "| `UT-X-01` | unit | `First` | `tests/Ivr.UnitTests/XTests.cs` |",
    "| `UT-X-02` | unit | `Second` | `tests/Ivr.UnitTests/XTests.cs` |",
    "| `UT-X-03` | unit | `Third` | `tests/Ivr.UnitTests/XTests.cs` |",
    "| `UT-X-04B` | unit | `` | `tests/Ivr.UnitTests/YTests.cs` |",
    "| `UT-BOOT-03-LINUX-PATH` | unit | `Separators` | `tests/Ivr.UnitTests/BootTests.cs` |",
  ].join("\n"));
  const trx = [
    '<TestRun><TestDefinitions>',
    '<UnitTest name="Ivr.UnitTests.XTests.First" id="a"><TestMethod className="Ivr.UnitTests.XTests" name="First" /></UnitTest>',
    '<UnitTest name="A custom display name" id="b"><TestMethod className="Ivr.UnitTests.XTests" name="Second" /></UnitTest>',
    '<UnitTest name="Ivr.UnitTests.YTests.Any" id="c"><TestMethod className="Ivr.UnitTests.YTests" name="Any" /></UnitTest>',
    '<UnitTest name="Ivr.UnitTests.BootTests.Separators" id="d"><TestMethod className="Ivr.UnitTests.BootTests" name="Separators" /></UnitTest>',
    '</TestDefinitions><Results>',
    '<UnitTestResult testId="a" testName="Ivr.UnitTests.XTests.First" outcome="Passed" />',
    '<UnitTestResult testId="b" testName="A custom display name" outcome="Failed"><Output /></UnitTestResult>',
    '<UnitTestResult testId="c" testName="Ivr.UnitTests.YTests.Any" outcome="Passed" />',
    '<UnitTestResult testId="d" testName="Ivr.UnitTests.BootTests.Separators" outcome="Passed" />',
    '</Results></TestRun>',
  ].join("\n");
  const evidence = {
    "W-0010": `${MARKER}\nTestId \`UT-X-01\` xanh, cùng \`UT-BOOT-03-LINUX-PATH\`. Không phải test: W-0310, OD-V1-11, M3-14.`,
    "W-0011": `${MARKER}\n\`UT-X-01\``,
    "W-0012": "Real customer calls: `NO`\n`UT-X-01`",
    "W-0014": `${MARKER}\n\`UT-X-99\` đã bị xoá`,
    "W-0015": `${MARKER}\n\`UT-X-02\``,
    "W-0016": `${MARKER}\n\`UT-X-01..03\``,
    "W-0019": `${MARKER}\nChỉ tài liệu. \`UT-X-04B\``,
  };

  const rows = plannedRows(tracker);
  const paths = evidencePaths(yaml);
  const results = indexResults(trxOutcomes(trx));
  const manifest = { gates: { "a.mjs": { argv: [], expect: "A" } } };
  const pass = sweepVerdict("  ok   a.mjs 0.1s A\nGATE_SWEEP_PASS 1/1 run, 0 skipped by manifest\n", manifest);
  const run = (sweep, withResults) => Object.fromEntries(rows
    .filter((row) => CANDIDATES.has(row.status))
    .map((row) => {
      const judged = judge(row, {
        evidencePath: paths.get(row.id) ?? null,
        evidenceText: evidence[row.id] ?? null,
        trace,
        results: withResults ? results : null,
        sweep,
        runCheck: withResults ? { ok: true, reason: "" } : undefined,
      });
      return [row.id, judged];
    }));

  assert.deepEqual(["P0-1", "P11-4", "Planning realignment", "P2-9 (rework)"].map(phaseOf), ["P0", "P11", "UNPLANNED", "UNPLANNED"]);
  assert.deepEqual(citedTestIds("`UT-X-01..03` và UT-X-01…02", trace), ["UT-X-01", "UT-X-02", "UT-X-03"]);
  assert.deepEqual(citedTestIds("W-0310 OD-V1-11 M3-14 A-0637 UT-XY-01", trace), ["UT-XY-01"]);

  const full = run(pass, true);
  assert.deepEqual(Object.keys(full).sort(), ["W-0010", "W-0011", "W-0012", "W-0013", "W-0014", "W-0015", "W-0016", "W-0019"],
    "only TESTS_PASS and EVIDENCE_SUBMITTED rows are candidates");
  assert.equal(full["W-0010"].verdict, PASS);
  assert.deepEqual(full["W-0010"].cited, ["UT-BOOT-03-LINUX-PATH", "UT-X-01"]);
  assert.equal(full["W-0011"].verdict, REVIEW, "a non-empty residual is shown, never passed");
  assert.equal(full["W-0012"].verdict, FAIL);
  assert.match(full["W-0012"].c1.reason,
    /thiếu REAL_CUSTOMER_CALL_ALLOWED=NO \(gói ghi dạng cũ: "Real customer calls: NO"\)$/u,
    "older wording still fails the criterion, and the reason says the statement is there");
  assert.equal(OLDER_WORDING.test("Thiếu dòng đánh dấu. Không gọi ai."), false);
  assert.equal(residualVerdict("— · `REAL_CUSTOMER_CALL_ALLOWED=NO`").ok, true, "the marker alone is not residual work");
  assert.equal(residualVerdict("Chờ M3 · `REAL_CUSTOMER_CALL_ALLOWED=NO`").ok, "review");
  assert.equal(full["W-0013"].verdict, FAIL);
  assert.match(full["W-0013"].c1.reason, /không trỏ tới gói bằng chứng/u);
  assert.equal(full["W-0014"].verdict, FAIL);
  assert.match(full["W-0014"].c2.reason, /UT-X-99 không có trong/u);
  assert.equal(full["W-0015"].verdict, FAIL, "the method comes from the definition, not the display name");
  assert.match(full["W-0015"].c2.reason, /UT-X-02 Failed/u);
  assert.equal(full["W-0016"].verdict, FAIL, "a range claims every test in it");
  assert.match(full["W-0016"].c2.reason, /UT-X-02 Failed.*UT-X-03 không có trong kết quả test/u);
  assert.equal(full["W-0016"].phase, "UNPLANNED");
  assert.equal(full["W-0019"].verdict, PASS);
  assert.match(full["W-0019"].c2.reason, /1 TestId xanh/u);

  const withoutResults = run(pass, false);
  assert.equal(withoutResults["W-0010"].verdict, UNCHECKED);
  assert.equal(withoutResults["W-0014"].verdict, FAIL, "a TestId missing from traceability fails even without results");

  const failedSweep = run(sweepVerdict("GATE_SWEEP_FAIL 0/1 run, 0 skipped by manifest\n", manifest), true);
  assert.equal(failedSweep["W-0010"].verdict, FAIL);
  assert.equal(run(sweepVerdict(null), true)["W-0010"].verdict, UNCHECKED);
  assert.equal(sweepVerdict("GATE_SWEEP_PASS 40/41 run").ok, false, "every gate has to run");

  assert.throws(() => plannedRows(tracker.replace("| TESTS_PASS | o | a | t | — |", "| a status | o | a | t | — |")),
    /closed vocabulary/u);

  const report = render({
    head: "abc1234", dirtyNote: "", rows, judged: Object.values(full), sweep: pass,
    resultsNote: "4 kết quả", phaseFilter: null,
  });
  assert.match(report, /\| `P0` \| 3 \| 1 \| 1 \| 1 \| 0 \|/u);
  assert.match(report, /Residual: Chờ Module 3 trả lời M3-14/u);
  assert.ok(!report.includes("W-0017"), "accepted rows are not proposed again");

  const c2Checks = c2SelfTest({ citedTestIds, traceability, judge, indexResults, testVerdict });

  // W-0346: the real registry against the real runners. A bad entry fails this gate, so the sweep
  // fails, and C2 counts a gate TestId only when the sweep passed at the same commit.
  const repository = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "../../..");
  const live = (files) => readWorktree(repository, files);
  const liveErrors = registryErrors(live, JSON.parse(live([GATE_MANIFEST]).get(GATE_MANIFEST)),
    traceability(live([TRACEABILITY]).get(TRACEABILITY)));
  assert.deepEqual(liveErrors, [], `GATE_TESTS: ${liveErrors.join("; ")}`);
  process.stdout.write(`ACCEPTANCE_BATCHES_SELFTEST_PASS — 4 criteria, 8 candidate rows, 2 non-candidates, ${c2Checks} C2 regression checks, ${Object.keys(GATE_TESTS).length} gate TestIds held to their runners\n`);
}

if (process.argv.includes("--self-test")) {
  selfTest();
} else if (process.argv[1] && path.resolve(process.argv[1]) === path.resolve(fileURLToPath(import.meta.url))) {
  main();
}
