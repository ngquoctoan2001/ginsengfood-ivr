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
  CANDIDATE_POLICIES,
  CHANNEL_CONSTRAINTS,
  UNCALIBRATED_SCENARIO,
  channelsForWindow,
  costPerConfirmedOrder,
  monthlyCost,
  poolForDay,
  poolForProgramme,
  sweep,
} from "../../../tools/capacity-sim/capacity-model.mjs";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../../..");

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
    channelsForWindow({ attempts: 13, windowSeconds: 300, callSeconds: 40, cooldownSeconds: 5 }),
    3,
    "the channel formula is not rounding up.");

  // Cooldown must cost capacity. Removing it must produce a strictly smaller pool, or it is being
  // applied somewhere that does not bite.
  const withCooldown = channelsForWindow({
    attempts: 100, windowSeconds: 300, callSeconds: 40, cooldownSeconds: 5,
  });
  const withoutCooldown = channelsForWindow({
    attempts: 100, windowSeconds: 300, callSeconds: 40, cooldownSeconds: 0,
  });
  assert(
    withCooldown > withoutCooldown,
    `cooldown does not reduce channel capacity (${withCooldown} vs ${withoutCooldown}).`);
  assert.equal(CHANNEL_CONSTRAINTS.cooldownSeconds, 5, "DT-04 cooldown constant changed.");

  // A call that does not fit in the window is a design error, reported as such rather than as a
  // very large pool that reads like a procurement problem.
  assert.equal(
    channelsForWindow({ attempts: 1, windowSeconds: 30, callSeconds: 40, cooldownSeconds: 5 }),
    Number.POSITIVE_INFINITY,
    "a call longer than the window did not report as unservable.");

  // Programmes sum rather than max: they overlap in time.
  const golden = poolForProgramme({
    dailyOrders: 800, eligibleRate: 0.6, peakShare: 0.15, noAnswerRate: 0.3, callSeconds: 40,
    policy: CANDIDATE_POLICIES.GOLDEN_HOUR,
  }).channels;
  const round = poolForProgramme({
    dailyOrders: 1_200, eligibleRate: 0.6, peakShare: 0.1, noAnswerRate: 0.3, callSeconds: 40,
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
    callSeconds: [25, 40, 70],
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
  // The only throughput evidence that exists is P5-3, and it measured the API and the scheduler --
  // never a dial, because no dial has happened. So calibration has exactly one honest form: assert
  // that the model's call-duration input is declared UNMEASURED and that the documents say so.
  const capacityDoc = await fs.readFile(
    path.join(repositoryRoot, "docs/capacity-model.md"), "utf8");

  assert(
    capacityDoc.includes("UNCALIBRATED"),
    "docs/capacity-model.md does not declare the model uncalibrated.");
  assert(
    /W-0008/.test(capacityDoc),
    "docs/capacity-model.md does not name the work that would calibrate it.");

  // And the perf report must not be cited as if it had measured call duration.
  const perfDoc = await fs.readFile(
    path.join(repositoryRoot, "docs/perf-security-report.md"), "utf8");
  assert(
    !/call duration measured|đo thời lượng cuộc gọi/i.test(perfDoc),
    "the performance report claims a measured call duration; no call has been placed.");

  process.stdout.write(
    "CAP-CALIB-03 PASS_UNCALIBRATED — the model declares itself uncalibrated and names W-0008 as "
    + "the work that would calibrate it; no document claims a measured call duration\n");
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

// ------------------------------------------------------------------------ main

const model = sizesFromPeakNotAverage();
theAnswerIsARangeWithANamedDriver();
await calibrationAgainstWhatWasActuallyMeasured();
await theShippedPoolMatchesTheModel(model);

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
