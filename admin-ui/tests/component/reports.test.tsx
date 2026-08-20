import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { BreakdownTable } from "@/components/reports/BreakdownTable";
import { ExportForm } from "@/components/reports/ExportForm";
import { FreshnessBanner } from "@/components/reports/FreshnessBanner";
import { TrendChart } from "@/components/reports/TrendChart";
import { MetricGrid } from "@/components/data/MetricGrid";
import { formatDuration, formatRate, toCsv } from "@/lib/analytics/format";
import type {
  IvrAnalyticsBreakdownRow,
  IvrAnalyticsDataQuality,
  IvrAnalyticsKpi,
  IvrAnalyticsTrendBucket,
} from "@/lib/api/types";
import vi from "@/i18n/vi.json";

const messages: Record<string, string> = vi;

const KPI: IvrAnalyticsKpi = {
  total_results: 13,
  total_final_results: 13,
  total_call_jobs: 13,
  total_eligible_tasks: 13,
  confirm_rate: 0.4615,
  cancel_rate: 0,
  no_answer_rate: 0.3846,
  invalid_phone_rate: 0,
  technical_rate: 0.1538,
  operational_blocked_rate: null,
  attempt_2_rate: 0.2308,
  avg_seconds_to_final: 135,
};

const QUALITY: IvrAnalyticsDataQuality = {
  generated_at: "2026-08-15T02:00:00Z",
  source: "OPERATIONAL_READ_MODEL",
  warehouse_backed: false,
  warehouse_status: "NOT_RUN" as const,
  pipeline_work_id: "W-0055",
  latest_event_at: "2026-08-15T01:58:00Z",
  freshness_seconds: 120,
  status: "FRESH",
  min_bucket_size: 5,
  suppressed_bucket_count: 1,
  scanned_rows: 13,
  truncated: false,
};

const BUCKETS: readonly IvrAnalyticsTrendBucket[] = [
  {
    bucket_start: "2026-08-14T00:00:00Z",
    program: "GOLDEN_HOUR",
    total: 11,
    confirmed: 6,
    cancelled: 0,
    no_answer: 5,
    invalid_phone: 0,
    technical: 0,
    operational_blocked: null,
    confirm_rate: 0.5455,
  },
];

const ROWS: readonly IvrAnalyticsBreakdownRow[] = [
  { key: "IVR_CONFIRMED", total: 6, confirmed: 6, confirm_rate: 1, share: 0.4615 },
  { key: "IVR_NO_ANSWER_FINAL", total: 5, confirmed: 0, confirm_rate: 0, share: 0.3846 },
];

/**
 * UT-UI-REPORT-01 — the cards show the metric the API computed, formatted, and
 * nothing the console derived itself.
 */
describe("UT-UI-REPORT-01 KPI cards", () => {
  it("renders each rate as a percentage and time-to-final as a duration", () => {
    render(
      <MetricGrid
        metrics={[
          {
            label: messages["reports.kpiConfirmRate"],
            value: formatRate(KPI.confirm_rate),
            testId: "kpi-confirm-rate",
          },
          {
            label: messages["reports.kpiAttemptTwoRate"],
            value: formatRate(KPI.attempt_2_rate),
            testId: "kpi-attempt-2-rate",
          },
          {
            label: messages["reports.kpiTimeToFinal"],
            value: formatDuration(KPI.avg_seconds_to_final),
            testId: "kpi-time-to-final",
          },
        ]}
      />,
    );

    expect(screen.getByTestId("kpi-confirm-rate")).toHaveTextContent("46,2%");
    expect(screen.getByTestId("kpi-attempt-2-rate")).toHaveTextContent("23,1%");
    expect(screen.getByTestId("kpi-time-to-final")).toHaveTextContent("2m 15s");
  });

  it("shows an absent time-to-final as unknown rather than as zero", () => {
    expect(formatDuration(undefined)).toBe("—");
    expect(formatDuration(45)).toBe("45s");
    expect(formatDuration(120)).toBe("2m");
  });

  it("states the data source and the suppressed-bucket count rather than hiding them", () => {
    render(<FreshnessBanner quality={QUALITY} />);

    expect(screen.getByTestId("freshness-status")).toHaveTextContent("Mới");
    // The pipeline does not exist yet, and the banner says so.
    expect(screen.getByTestId("analytics-source")).toHaveTextContent("CHƯA có pipeline P10-4");
    expect(screen.getByTestId("analytics-source")).toHaveTextContent("W-0055");
    expect(screen.getByTestId("suppressed-notice")).toHaveTextContent("k=5");
    expect(screen.queryByTestId("truncated-notice")).toBeNull();
  });

  it("warns when the scan cap truncated the numbers", () => {
    render(<FreshnessBanner quality={{ ...QUALITY, truncated: true, status: "STALE" }} />);

    expect(screen.getByTestId("truncated-notice")).toBeInTheDocument();
    expect(screen.getByTestId("freshness-status")).toHaveTextContent("Đã cũ");
  });
});

/**
 * UT-UI-REPORT-PII-03 — the reporting surface is aggregate-only. There is no
 * vocabulary for a customer field, so no screen can grow one by accident.
 */
describe("UT-UI-REPORT-PII-03 privacy boundary", () => {
  it("renders a breakdown with no identifier column", () => {
    render(
      <BreakdownTable
        caption="taxonomy"
        keyLabel={messages["reports.colResultType"]}
        rows={ROWS}
        testId="taxonomy-table"
      />,
    );

    const markup = screen.getByTestId("taxonomy-table").innerHTML;
    for (const forbidden of [
      "phone",
      "dial_token",
      "order_code",
      "address",
      "payment",
      "member_tier",
      "health",
    ]) {
      expect(markup.toLowerCase(), forbidden).not.toContain(forbidden);
    }
  });

  it("ships no reporting label for any customer field", () => {
    // Labels only. The scope notice legitimately *names* the forbidden fields in
    // order to say they are absent, so prose is excluded from this scan.
    const labelKeys = Object.keys(messages).filter((entry) =>
      /^reports\.(col|kpi|filter|dim|export(Dimension|Reason|Submit))/.test(entry),
    );

    expect(labelKeys.length).toBeGreaterThan(10);
    for (const key of labelKeys) {
      expect(messages[key].toLowerCase(), key).not.toMatch(
        /số điện thoại|địa chỉ|mã đơn|dial_token|thanh toán|sức khoẻ|hạng thành viên/,
      );
    }
  });

  it("exports only the aggregate cells the server returned", () => {
    const csv = toCsv(
      ["dimension", "key", "total", "confirmed", "confirm_rate", "share"],
      [["RESULT_TYPE", "IVR_CONFIRMED", "6", "6", "1", "0.4615"]],
    );

    expect(csv).toBe(
      "dimension,key,total,confirmed,confirm_rate,share\r\n" +
        "RESULT_TYPE,IVR_CONFIRMED,6,6,1,0.4615\r\n",
    );
    expect(csv).not.toMatch(/\d{9,}/);
  });

  it("neutralises a cell a spreadsheet would read as a formula", () => {
    expect(toCsv(["key"], [["=1+1"]])).toContain("'=1+1");
    expect(toCsv(["key"], [['a"b,c']])).toContain('"a""b,c"');
  });

  it("states the reporting scope so it is not mistaken for the live console", () => {
    expect(messages["reports.scopeNotice"]).toMatch(/KHÔNG phải bảng điều hành thời gian thực/);
    expect(messages["reports.scopeNotice"]).toContain("D-05");
  });
});

/**
 * UT-UI-REPORT-EXPORT-04 — the export asks for a reason and says plainly that a
 * slice too small to be anonymous will be refused.
 */
describe("UT-UI-REPORT-EXPORT-04 export guard", () => {
  it("requires a reason at least as long as the server rule", () => {
    render(
      <ExportForm
        program=""
        resultType=""
        scriptVariant=""
        bucket="DAY"
        from=""
        to=""
        dimension="RESULT_TYPE"
        minBucketSize={5}
      />,
    );

    const reason = screen.getByTestId("export-reason");
    expect(reason).toBeRequired();
    expect(reason).toHaveAttribute("minLength", "8");
    expect(screen.getByTestId("export-form")).toHaveAttribute("action", "/reports/export");
    expect(screen.getByTestId("export-notice")).toHaveTextContent("k=5");
    expect(screen.getByTestId("export-notice")).toHaveTextContent("audit log");
  });

  it("carries the active filter so the file matches the screen", () => {
    const { container } = render(
      <ExportForm
        program="GOLDEN_HOUR"
        resultType="IVR_CONFIRMED"
        scriptVariant="SCRIPT-ORDER-CONFIRM:vA"
        bucket="HOUR"
        from="2026-08-01"
        to="2026-08-14"
        dimension="PROGRAM"
        minBucketSize={5}
      />,
    );

    const hidden = Object.fromEntries(
      [...container.querySelectorAll<HTMLInputElement>("input[type=hidden]")].map((input) => [
        input.name,
        input.value,
      ]),
    );

    expect(hidden).toEqual({
      program: "GOLDEN_HOUR",
      result_type: "IVR_CONFIRMED",
      script_variant: "SCRIPT-ORDER-CONFIRM:vA",
      bucket: "HOUR",
      from: "2026-08-01",
      to: "2026-08-14",
    });
  });

  it("offers no control that could alter the anonymity threshold", () => {
    const { container } = render(
      <ExportForm
        program=""
        resultType=""
        scriptVariant=""
        bucket="DAY"
        from=""
        to=""
        dimension="RESULT_TYPE"
        minBucketSize={5}
      />,
    );

    expect(container.querySelector("[name='min_bucket_size']")).toBeNull();
    expect(messages["reports.exportNotice"]).toMatch(/sẽ bị từ chối/);
  });
});

/** A trend bucket the server suppressed simply is not there to render. */
describe("UT-UI-REPORT-01 trend series", () => {
  it("renders the surviving buckets and an empty state for none", () => {
    const { rerender } = render(<TrendChart buckets={BUCKETS} bucket="DAY" />);

    expect(screen.getByText("GOLDEN_HOUR")).toBeInTheDocument();
    // The rate shows twice on purpose: once on the bar, once in the table that
    // carries the numbers for assistive technology.
    // W-0039: 0.5455 renders as 54,6% — Intl half-expands 54.55, where toFixed(1) gave 54.5
    // because 54.55 is not exactly representable in binary. The Intl value is the one a person
    // rounding 54.55 to one decimal would write.
    expect(screen.getAllByText("54,6%")).toHaveLength(2);
    expect(screen.queryByText("TWENTY_FOUR_SEVEN")).toBeNull();

    rerender(<TrendChart buckets={[]} bucket="DAY" />);
    expect(screen.getByTestId("trend-empty")).toBeInTheDocument();
  });
});
