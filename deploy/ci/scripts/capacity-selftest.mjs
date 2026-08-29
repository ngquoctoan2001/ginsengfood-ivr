// W-0054 / P10-3 §8 — the four capacity checks.
//
// The model produces a number that could drive a procurement decision, so the checks are about
// whether the arithmetic is the arithmetic the constraints demand — not about whether it runs.
//
//   CAP-MODEL-01  sizes from the Golden Hour peak, not the daily average, and honours one-call and
//                 cooldown.
//   CAP-SENS-02   the answer is a range with a named dominant input, not a point.
//   CAP-CALIB-03  the model is checked against the only throughput measurement that exists, and
//                 reports the gap where none exists.
//   CAP-ALERT-04  the pool the chart ships matches what the model says it can serve.
import assert from "node:assert/strict";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import YAML from "yaml";
import {
  CALL_DURATION_ASSUMPTIONS,
  CANDIDATE_POLICIES,
  CHANNEL_CONSTRAINTS,
  SESSION_LENGTH,
  UNCALIBRATED_SCENARIO,
  attemptsFor,
  channelsForWindow,
  costPerConfirmedOrder,
  monthlyCost,
  poolForDay,
  poolForProgramme,
  sweep,
} from "../../../tools/capacity-sim/capacity-model.mjs";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../../..");

// Much faster and much slower than the model assumes. The middle corner is the assumption
// itself, which CAP-DRIFT-05 keeps honest.
const SENSITIVITY_CALL_SECONDS = [25, CALL_DURATION_ASSUMPTIONS.modelCallSeconds, 70];

// Every input `cost_per_confirmed_order` needs before it can exist. Pinned, so that answering a
// row and deleting a row are equally visible -- both change the count.
const COST_INPUT_COUNT = 6;

// ---------------------------------------------------------------- CAP-MODEL-01

function sizesFromPeakNotAverage() {
  const scenario = UNCALIBRATED_SCENARIO;
  const peak = poolForDay(scenario).channels;

  // The same daily volume spread evenly across the day. If the model sized from the average this
  // would give the same answer, and the whole point of Golden Hour would be invisible to it.
  const flat = poolForDay({
    ...scenario,
    programmes: Object.fromEntries(Object.entries(scenario.programmes).map(([name, programme]) => [
      name,
      { ...programme, peakShare: programme.policy.windowSeconds / 86_400 },
    ])),
  }).channels;

  assert(
    peak > flat,
    `sizing from the peak (${peak}) is not larger than sizing from a flat day (${flat}); the model `
    + "is not seeing the burst that Golden Hour is made of.");

  // One call per channel, cooldown inside the divisor. A 40-second call with a 5-second pause
  // fits 300/45 = 6 times into a Golden Hour window, so 13 attempts need 3 channels — not 2, which
  // is what floor() would give and what a pool that misses deadlines is built on.
  assert.equal(
    channelsForWindow({ attempts: 13, windowSeconds: 300, callSeconds: CALL_DURATION_ASSUMPTIONS.modelCallSeconds, cooldownSeconds: 5 }),
    3,
    "the channel formula is not rounding up.");

  // Cooldown must cost capacity. Removing it must produce a strictly smaller pool, or it is being
  // applied somewhere that does not bite.
  const withCooldown = channelsForWindow({
    attempts: 100, windowSeconds: 300, callSeconds: CALL_DURATION_ASSUMPTIONS.modelCallSeconds, cooldownSeconds: 5,
  });
  const withoutCooldown = channelsForWindow({
    attempts: 100, windowSeconds: 300, callSeconds: CALL_DURATION_ASSUMPTIONS.modelCallSeconds, cooldownSeconds: 0,
  });
  assert(
    withCooldown > withoutCooldown,
    `cooldown does not reduce channel capacity (${withCooldown} vs ${withoutCooldown}).`);
  assert.equal(CHANNEL_CONSTRAINTS.cooldownSeconds, 5, "DT-04 cooldown constant changed.");

  // A call that does not fit in the window is a design error, reported as such rather than as a
  // very large pool that reads like a procurement problem.
  assert.equal(
    channelsForWindow({ attempts: 1, windowSeconds: 30, callSeconds: CALL_DURATION_ASSUMPTIONS.modelCallSeconds, cooldownSeconds: 5 }),
    Number.POSITIVE_INFINITY,
    "a call longer than the window did not report as unservable.");

  // Programmes sum rather than max: they overlap in time.
  const golden = poolForProgramme({
    dailyOrders: 800, eligibleRate: 0.6, peakShare: 0.15, noAnswerRate: 0.3, callSeconds: CALL_DURATION_ASSUMPTIONS.modelCallSeconds,
    policy: CANDIDATE_POLICIES.GOLDEN_HOUR,
  }).channels;
  const round = poolForProgramme({
    dailyOrders: 1_200, eligibleRate: 0.6, peakShare: 0.1, noAnswerRate: 0.3, callSeconds: CALL_DURATION_ASSUMPTIONS.modelCallSeconds,
    policy: CANDIDATE_POLICIES.TWENTY_FOUR_SEVEN,
  }).channels;
  assert.equal(
    peak,
    golden + round,
    "programme pools are not summed; taking the maximum assumes they take turns.");

  process.stdout.write(
    `CAP-MODEL-01 PASS — peak sizing (${peak} channels) exceeds flat-day sizing (${flat}), cooldown `
    + "costs capacity, partial channels round up, and overlapping programmes sum\n");
  return { peak };
}

// ----------------------------------------------------------------- CAP-SENS-02

function theAnswerIsARangeWithANamedDriver() {
  const result = sweep(UNCALIBRATED_SCENARIO, {
    dailyOrders: [1_000, 2_000, 4_000],
    callSeconds: SENSITIVITY_CALL_SECONDS,
    noAnswerRate: [0.15, 0.3, 0.5],
  });

  assert(
    result.maximum > result.minimum,
    "the sensitivity sweep produced a single value; every input here is an unmeasured assumption "
    + "and an interval is the only honest output.");
  assert(result.results.length === 27, `expected 27 corners, got ${result.results.length}.`);
  assert(result.dominant.spread > 0, "no input moves the answer; the sweep is not varying anything.");

  process.stdout.write(
    `CAP-SENS-02 PASS — ${result.results.length} corners give ${result.minimum}..${result.maximum} `
    + `channels; the input that moves it most is ${result.dominant.input} `
    + `(spread ${result.dominant.spread.toFixed(1)} channels)\n`);
  return result;
}

// ---------------------------------------------------------------- CAP-CALIB-03

async function calibrationAgainstWhatWasActuallyMeasured() {
  const capacityDoc = await fs.readFile(
    path.join(repositoryRoot, "docs/capacity-model.md"), "utf8");

  // The perf report measured API/scheduler throughput, not a dial. This remains forbidden even
  // after W-0008 exists: calibration must cite the real-call evidence, never relabel P5-3.
  const perfDoc = await fs.readFile(
    path.join(repositoryRoot, "docs/perf-security-report.md"), "utf8");
  assert(
    !/call duration measured|đo thời lượng cuộc gọi/i.test(perfDoc),
    "the performance report claims a measured call duration, but it never measured a dial.");

  if (!CALL_DURATION_ASSUMPTIONS.calibrated) {
    assert(
      capacityDoc.includes("UNCALIBRATED"),
      "docs/capacity-model.md does not declare the model uncalibrated.");
    assert(
      /W-0008/.test(capacityDoc),
      "docs/capacity-model.md does not name the work that would calibrate it.");
    process.stdout.write(
      "CAP-CALIB-03 PASS_UNCALIBRATED — the model declares itself uncalibrated and names W-0008 "
      + "as the work that would calibrate it; no document relabels P5-3 as call evidence\n");
    return;
  }

  const evidence = CALL_DURATION_ASSUMPTIONS.calibratedBy;
  assert(
    typeof evidence === "string" && /^docs\/evidence\/W-0008\//.test(evidence),
    "calibratedBy must point to an artifact under docs/evidence/W-0008/.");
  await fs.access(path.join(repositoryRoot, evidence));
  assert(
    capacityDoc.includes("CALIBRATED") && capacityDoc.includes(evidence),
    "docs/capacity-model.md must declare CALIBRATED and cite the exact W-0008 artifact.");
  process.stdout.write(
    `CAP-CALIB-03 PASS_CALIBRATED — real-call evidence exists at ${evidence}; P5-3 remains `
    + "scheduler/API evidence only\n");
}

// ---------------------------------------------------------------- CAP-ALERT-04

async function theShippedPoolMatchesTheModel({ peak }) {
  const environments = ["dev", "staging", "lab", "prod"];
  const pools = {};
  for (const environment of environments) {
    const values = YAML.parse(await fs.readFile(
      path.join(repositoryRoot, `deploy/helm/ivr/values-${environment}.yaml`), "utf8"));
    pools[environment] = values.worker?.hpa?.simPoolSize;
    assert(
      typeof pools[environment] === "number",
      `${environment} declares no worker.hpa.simPoolSize.`);
  }

  // prod is the target the model is for. The pool must be at least what the model says the peak
  // needs, or the chart ships a ceiling that guarantees missed deadlines.
  assert(
    pools.prod >= peak,
    `prod ships simPoolSize=${pools.prod} while the model needs ${peak} at peak.`);

  // And the ladder must not run backwards: a lab pool larger than prod would mean the rehearsal is
  // bigger than the thing it rehearses.
  assert(
    pools.dev <= pools.staging && pools.staging <= pools.lab && pools.lab <= pools.prod,
    `sim pool sizes are not monotonic across the ladder: ${JSON.stringify(pools)}`);

  // The capacity alert now exists (W-0041 residual closed 2026-08-19), and this is where the rule
  // and the model are tied together. The rule is zero-tolerance -- ANY missed deadline opens a
  // ticket -- and that is only defensible while the shipped pool covers the modelled peak. The
  // assert above is therefore not just a chart check any more: it is the premise the alert rests
  // on, and its failure message has to say so.
  const rules = YAML.parse(await fs.readFile(
    path.join(repositoryRoot, "deploy/observability/alerts/ivr-slo.rules.yml"), "utf8"));
  const alerts = rules.groups.flatMap((group) => group.rules ?? []);
  const deadlineAlert = alerts.find(
    (rule) => typeof rule.expr === "string" && rule.expr.includes("ivr_missed_deadline_total"));

  assert(
    deadlineAlert !== undefined,
    "no alert rule reads ivr_missed_deadline_total. The metric has a call site "
    + "(PostgresSchedulerStore.CloseMissedDeadlinesAsync), so a missing rule means misses are "
    + "being counted and nobody is being told.");

  // Zero-tolerance, and asserted as such: a threshold above zero would be a number nobody derived.
  // The model cannot say "N misses per hour is acceptable" -- it has no queueing model and no
  // channel-failure model (see docs/capacity-model.md). What it CAN say is that under its own
  // assumptions the shipped pool misses nothing, which makes any miss a falsified assumption.
  assert(
    /\)\s*>\s*0\s*$/.test(deadlineAlert.expr.trim()),
    `the capacity alert compares against something other than zero (${deadlineAlert.expr.trim()}). `
    + "The model licenses zero and nothing else: it does not model queueing or channel failure, so "
    + "it cannot tell you which non-zero miss rate is acceptable.");

  // And it must unlatch. A counter compared against zero fires once and then forever.
  assert(
    /increase\(|rate\(/.test(deadlineAlert.expr),
    "the capacity alert reads the counter directly rather than through increase()/rate(); a "
    + "zero-threshold rule on a monotonic counter never stops firing once it has fired.");

  // The alert must send the on-call at the model, not at a purchase order. An uncalibrated model
  // that fires an alert saying "buy more SIMs" would convert an unmeasured assumption into a
  // procurement decision, which is exactly what P10-3 section 4 forbids.
  assert(
    !/\b(buy|purchase|procure|mua thêm)\b/i.test(JSON.stringify(deadlineAlert.annotations ?? {})),
    "the capacity alert tells the on-call to buy capacity. The model is UNCALIBRATED (W-0008); it "
    + "cannot justify a purchase, only a recalibration.");

  // The other half of the ARCH-06 section 1 gap, still open and still asserted so it cannot rot
  // into a silent omission: cost_per_confirmed_order has no instrument because it has no
  // numerator. The denominator is measurable today (analytics.agg_kpi_daily.confirmed_count), the
  // numerator needs a vendor quote that does not exist. The moment any quote row in the cost model
  // stops being blocked, that reason expires -- and this check goes red saying so rather than
  // letting "not instrumented" quietly outlive its excuse.
  //
  // The rows are selected STRUCTURALLY -- every data row of the section 3 table -- and not by the
  // blocked marker they are being tested for. Selecting by the marker was the first version of
  // this check and it was self-erasing: a row that stopped being blocked simply left the
  // selection, and `every(blocked)` over the survivors stayed true. A gate whose subject can walk
  // out of its own sample is not a gate.
  const costDoc = await fs.readFile(path.join(repositoryRoot, "docs/cost-model.md"), "utf8");
  const inputSection = costDoc.split(/^## /m).find((section) => section.startsWith("3. "));
  assert(inputSection !== undefined, "docs/cost-model.md no longer has a section 3.");
  const quoteRows = inputSection.split("\n").filter(
    (line) => line.startsWith("|") && !/^\|\s*(Đầu vào|-)/.test(line));
  assert(
    quoteRows.length === COST_INPUT_COUNT,
    `the cost model input table has ${quoteRows.length} rows, expected ${COST_INPUT_COUNT}. `
    + "Rows leaving the table is the same silence as rows being answered.");
  const answered = quoteRows.filter((line) => !line.includes("❌"));
  assert(
    answered.length === 0,
    `${answered.length} cost input(s) are no longer blocked. cost_per_confirmed_order can now be `
    + "instrumented: the denominator already exists in analytics.agg_kpi_daily.confirmed_count, so "
    + "replace this check with one that asserts the metric and its alert exist.");

  process.stdout.write(
    `CAP-ALERT-04 PASS_WITH_NOT_PROVEN=COST_METRIC — prod ships ${pools.prod} channels against a `
    + `modelled peak of ${peak}, and the ladder is monotonic (${environments.map(
      (environment) => `${environment}=${pools[environment]}`).join(", ")}). ${deadlineAlert.alert} `
    + "is zero-tolerance on ivr_missed_deadline_total, which the shipped pool justifies. "
    + `cost_per_confirmed_order stays uninstrumented: ${quoteRows.length}/${quoteRows.length} `
    + "cost inputs are still blocked on a vendor quote (W-0008)\n");
}

// --------------------------------------------------------------- CAP-DRIFT-05

function assertCalibratedDurationSemantics(declared, label) {
  assert.equal(
    declared.schedulerDefaultSeconds,
    declared.modelCallSeconds,
    `${label}: the scheduler estimate and model callSeconds must describe the same channel `
    + "occupancy measurement.");
  assert.equal(
    declared.specConservativeSeconds,
    declared.modelCallSeconds + CHANNEL_CONSTRAINTS.cooldownSeconds,
    `${label}: the spec cycle must equal measured channel occupancy plus cooldown. Making all `
    + "three values equal would make channelsForWindow add cooldown twice.");
}

async function callDurationHasOneDeclaredSourceAndDoesNotDriftSilently() {
  // W-0132. The three call-duration numbers used to live in three files with nothing comparing
  // them, so any one of them could move and every gate stayed green. This check does not unify
  // them -- unifying means claiming a measurement nobody took -- it makes the disagreement
  // declared, and fails the moment a number moves without the declaration moving with it.
  const declared = CALL_DURATION_ASSUMPTIONS;

  // Exercise the future calibrated relationship without pretending this TEST_ONLY shape is a
  // measurement. This keeps the exit path executable before W-0008 exists.
  assert.throws(
    () => assertCalibratedDurationSemantics({
      modelCallSeconds: 40,
      specConservativeSeconds: 40,
      schedulerDefaultSeconds: 40,
    }, "TEST_ONLY equal-values mutation"),
    /Making all three values equal would make channelsForWindow add cooldown twice/,
    "the historical equal-values escape hatch must stay rejected because it double-counts cooldown.");
  assertCalibratedDurationSemantics({
    modelCallSeconds: 40,
    specConservativeSeconds: 45,
    schedulerDefaultSeconds: 40,
  }, "TEST_ONLY calibrated shape");

  if (declared.calibrated) {
    assertCalibratedDurationSemantics(declared, "calibrated declaration");
    assert(
      typeof declared.calibratedBy === "string" && declared.calibratedBy.length > 0,
      "calibrated is true but calibratedBy names no evidence.");
  } else {
    assert.equal(declared.modelCallSeconds, 40,
      "the model's call-duration assumption moved. That is allowed only with W-0008 evidence and "
      + "a calibrated declaration, not as an edit to a scenario literal.");
    assert.equal(declared.specConservativeSeconds, 50,
      "the spec-implied full channel cycle moved; re-derive it from spec §23 or W-0008 evidence.");
    assert.equal(declared.schedulerDefaultSeconds, 60,
      "the declared scheduler default moved; it must track SchedulerCapacity.cs.");
    assert.equal(declared.calibratedBy, null,
      "an uncalibrated model must not cite calibration evidence.");
    assert.equal(declared.calibrationWork, "W-0008",
      "the work that would calibrate this is no longer named.");

    // The spec never writes 50s down -- it writes ~192 calls for 32 SIM in a five-minute window.
    // If that arithmetic stops holding, the declared 50s is no longer what the current spec means.
    const specImpliedCalls = 32 * Math.floor(300 / declared.specConservativeSeconds);
    assert.equal(specImpliedCalls, 192,
      `spec §23 sizes 32 SIM at ~192 calls per 300s window, but the declared `
      + `${declared.specConservativeSeconds}s gives ${specImpliedCalls}.`);
  }

  // The runtime default is the one number that lives in C#, so read it rather than trust the copy.
  const schedulerSource = await fs.readFile(
    path.join(repositoryRoot, "src/Ivr.Infrastructure/Scheduling/SchedulerCapacity.cs"), "utf8");
  const runtimeDefault = /ExpectedCallDurationSeconds\s*{\s*get;\s*set;\s*}\s*=\s*(\d+)\s*;/
    .exec(schedulerSource);
  assert(runtimeDefault, "could not find the ExpectedCallDurationSeconds default in "
    + "SchedulerCapacity.cs; the declaration can no longer be checked against the runtime.");
  assert.equal(Number(runtimeDefault[1]), declared.schedulerDefaultSeconds,
    `SchedulerCapacity.cs defaults ExpectedCallDurationSeconds to ${runtimeDefault[1]}s while `
    + `CALL_DURATION_ASSUMPTIONS declares ${declared.schedulerDefaultSeconds}s. One of them moved `
    + "alone, which is exactly the drift this check exists to catch.");

  // The sensitivity sweep has to explore around the assumption it is testing, or it is sweeping
  // around a number nobody uses any more.
  assert(
    SENSITIVITY_CALL_SECONDS.includes(declared.modelCallSeconds),
    `the sensitivity sweep varies ${SENSITIVITY_CALL_SECONDS.join("/")}s but the model now assumes `
    + `${declared.modelCallSeconds}s; the sweep is centred on a stale value.`);

  process.stdout.write(
    `CAP-DRIFT-05 ${declared.calibrated ? "PASS_CALIBRATED" : "PASS_DECLARED_DISAGREEMENT"} — `
    + `model occupancy ${declared.modelCallSeconds}s, spec full-cycle `
    + `${declared.specConservativeSeconds}s, runtime occupancy estimate `
    + `${declared.schedulerDefaultSeconds}s (read back from SchedulerCapacity.cs). `
    + (declared.calibrated
      ? `Calibration evidence: ${declared.calibratedBy}\n`
      : `They disagree by design and ${declared.calibrationWork} is the work that would settle them\n`));
}

// ------------------------------------------------------------- CAP-SESSION-06

function sessionLengthStaysUnansweredAndCannotBeSubstitutedQuietly() {
  // W-0134 / OD-19. The model has no session-length input, and adding one naively is not a neutral
  // refactor -- it swaps a conservative "the peak lands at once" for an unapproved "the peak
  // arrives evenly", and the sizing collapses. This check keeps the input declared, keeps the
  // unsourced 45-minute figure out of the arithmetic, and refuses a session length that arrives
  // without an arrival profile beside it.
  assert.equal(SESSION_LENGTH.decisionId, "M8-OD-C",
    "the decision that would answer session length is no longer named.");

  // The danger is measured here rather than described in a comment, so it fails if it stops being
  // true. Golden Hour branch of UNCALIBRATED_SCENARIO, sized both ways.
  const gh = UNCALIBRATED_SCENARIO.programmes.GOLDEN_HOUR;
  const peakOrders = UNCALIBRATED_SCENARIO.dailyOrders * gh.share
    * UNCALIBRATED_SCENARIO.eligibleRate * gh.peakShare;
  const attempts = attemptsFor({
    orders: peakOrders,
    noAnswerRate: UNCALIBRATED_SCENARIO.noAnswerRate,
    maxAttempts: gh.policy.maxAttempts,
  });
  const sizing = (windowSeconds) => channelsForWindow({
    attempts,
    windowSeconds,
    callSeconds: CALL_DURATION_ASSUMPTIONS.modelCallSeconds,
    cooldownSeconds: CHANNEL_CONSTRAINTS.cooldownSeconds,
  });

  const asWindow = sizing(gh.policy.windowSeconds);
  const asSession = sizing(SESSION_LENGTH.unsourcedSpecCandidateSeconds);
  assert(
    asSession * 4 < asWindow,
    `substituting the session length used to collapse the sizing (${asWindow} -> ${asSession} `
    + "channels) and no longer does. If the model changed shape, re-derive whether the "
    + "uniform-arrival assumption is still hiding inside the substitution before relaxing this.");

  // A session length may only enter the model together with the assumption that makes it mean
  // something. One without the other is the 8x under-size with a decision-shaped label on it.
  if (SESSION_LENGTH.sessionSeconds !== null) {
    assert.equal(SESSION_LENGTH.answered, true,
      "a session length was set while the decision is still recorded as unanswered.");
    assert(
      SESSION_LENGTH.arrivalProfile !== null,
      `a session length (${SESSION_LENGTH.sessionSeconds}s) was set with no arrivalProfile. `
      + `Sizing against it instead of the ${gh.policy.windowSeconds}s confirmation window takes `
      + `Golden Hour from ${asWindow} channels to ${asSession}, which is only correct if orders `
      + "really do arrive evenly across the session. Decide that, or leave the window in place.");
    assert.notEqual(
      SESSION_LENGTH.sessionSeconds, SESSION_LENGTH.unsourcedSpecCandidateSeconds,
      "the session length was set to the 45-minute figure from the §14.1 column header, which the "
      + "spec itself calls an assumption rather than a decision. It needs its own source.");
  } else {
    assert.equal(SESSION_LENGTH.answered, false,
      "session length is declared answered but carries no value.");
    assert.equal(SESSION_LENGTH.arrivalProfile, null,
      "an arrival profile was declared without a session length for it to apply to.");
  }

  // And the model must still be sizing the way this check assumes it is.
  assert.equal(SESSION_LENGTH.sizedAgainst, "policy.windowSeconds",
    "the declared sizing base changed; CAP-SESSION-06 is no longer describing the model.");
  const live = poolForProgramme({
    dailyOrders: UNCALIBRATED_SCENARIO.dailyOrders * gh.share,
    eligibleRate: UNCALIBRATED_SCENARIO.eligibleRate,
    peakShare: gh.peakShare,
    noAnswerRate: UNCALIBRATED_SCENARIO.noAnswerRate,
    callSeconds: UNCALIBRATED_SCENARIO.callSeconds,
    policy: gh.policy,
  });
  assert.equal(live.channels, asWindow,
    `the model sized Golden Hour at ${live.channels} channels while the declared base `
    + `(${SESSION_LENGTH.sizedAgainst}) gives ${asWindow}. Something started substituting a `
    + "different time base.");

  process.stdout.write(
    `CAP-SESSION-06 PASS_UNANSWERED — session length is declared open under `
    + `${SESSION_LENGTH.decisionId} and the model still sizes against `
    + `${SESSION_LENGTH.sizedAgainst}. Substituting the unsourced 45-minute figure would take `
    + `Golden Hour from ${asWindow} to ${asSession} channels, so a session length is refused `
    + `unless an arrival profile is decided with it\n`);
}

// ------------------------------------------------------------------------ main

const model = sizesFromPeakNotAverage();
theAnswerIsARangeWithANamedDriver();
await calibrationAgainstWhatWasActuallyMeasured();
await theShippedPoolMatchesTheModel(model);
await callDurationHasOneDeclaredSourceAndDoesNotDriftSilently();
sessionLengthStaysUnansweredAndCannotBeSubstitutedQuietly();

const monthly = monthlyCost({
  channels: 32, simMonthlyCost: 1, gatewayMonthlyCost: 1, infraMonthlyCost: 1,
});
assert(monthly === 34, "the cost model arithmetic changed.");
assert(
  costPerConfirmedOrder({ monthly, confirmedOrdersPerMonth: 0 }) === null,
  "cost per confirmed order returns a number when nothing was confirmed; that would be a division "
  + "by an outcome that did not happen.");

process.stdout.write(
  "CAPACITY_SELFTEST_PASS_UNCALIBRATED — the arithmetic is checked, the answer is a range, and the "
  + "inputs are assumptions nobody has measured. This model must not drive a purchase until W-0008 "
  + "produces a measured call duration.\n");
