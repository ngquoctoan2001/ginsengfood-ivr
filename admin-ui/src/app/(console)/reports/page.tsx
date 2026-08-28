import { Suspense } from "react";

import { MetricGrid, type Metric } from "@/components/data/MetricGrid";
import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { BreakdownTable } from "@/components/reports/BreakdownTable";
import { ExportForm } from "@/components/reports/ExportForm";
import { FreshnessBanner } from "@/components/reports/FreshnessBanner";
import { TrendChart } from "@/components/reports/TrendChart";
import { Callout, Card, CardStack, Meter, PageHeader } from "@/components/ui";
import { getAnalyticsBreakdown, getAnalyticsSummary, getAnalyticsTrend } from "@/lib/analytics/client";
import { formatDuration, formatRate } from "@/lib/analytics/format";
import { IvrApiError } from "@/lib/api/errors";
import {
  ANALYTICS_DIMENSIONS,
  type AnalyticsDimension,
  type IvrAnalyticsBreakdown,
  type IvrAnalyticsSummary,
  type IvrAnalyticsTrend,
} from "@/lib/api/types";
import { requireAdmin, requireScope } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatNumber, t } from "@/lib/i18n";
import { tEnum, type EnumFamily } from "@/lib/i18n/enum";

import { ReportFilters } from "./ReportFilters";

export const dynamic = "force-dynamic";

/**
 * P3-4 reporting console.
 *
 * This screen analyses history; it does not run anything. There is no dispatch
 * control, no order-state control and no drill-down past an aggregate bucket —
 * the live operational view is the dashboard (P3-2), and the two must not be
 * confused for one another.
 */
export default async function ReportsPage({ searchParams }: PageProps<"/reports">) {
  await requireAdmin();
  const params = await searchParams;
  const query = readQuery(params);

  return (
    <>
      <PageHeader
        title={t("reports.title")}
        subtitle={t("reports.subtitle")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.reports") },
          ],
        }}
      />

      <Callout tone="info" testId="reports-scope-notice">
        {t("reports.scopeNotice")}
      </Callout>

      <ReportFilters {...query} />

      <Suspense
        key={Object.values(query).join("|")}
        fallback={<LoadingSkeleton rows={8} variant="metrics" />}
      >
        <ReportPanels query={query} />
      </Suspense>
    </>
  );
}

interface ReportQuery {
  readonly program: string;
  readonly resultType: string;
  readonly scriptVariant: string;
  readonly bucket: string;
  readonly from: string;
  readonly to: string;
  readonly dimension: string;
}

async function ReportPanels({ query }: { query: ReportQuery }) {
  const session = await requireScope("read");
  const config = readConfig();
  const dimension = asDimension(query.dimension);
  const filter = {
    program: query.program,
    resultType: query.resultType,
    scriptVariant: query.scriptVariant,
    bucket: query.bucket,
    from: query.from,
    to: query.to,
  };

  let summary: IvrAnalyticsSummary | null = null;
  let trend: IvrAnalyticsTrend | null = null;
  let breakdown: IvrAnalyticsBreakdown | null = null;
  let error: ErrorEnvelopeView | null = null;

  try {
    // Independent reads, so they are issued together rather than in a waterfall.
    const [summaryResponse, trendResponse, breakdownResponse] = await Promise.all([
      getAnalyticsSummary({ session, config }, filter),
      getAnalyticsTrend({ session, config }, filter),
      getAnalyticsBreakdown({ session, config }, dimension, filter),
    ]);
    summary = summaryResponse.data;
    trend = trendResponse.data;
    breakdown = breakdownResponse.data;
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    error = cause.toEnvelope();
  }

  if (error !== null || summary === null || trend === null || breakdown === null) {
    return <ErrorAlert error={error!} />;
  }

  // Every figure below is already computed by the analytics API; the console
  // formats, it never derives (P3-4 §4).
  const rateMetrics: Metric[] = [
    { label: t("reports.kpiCancelRate"), value: formatRate(summary.kpi.cancel_rate) },
    { label: t("reports.kpiNoAnswerRate"), value: formatRate(summary.kpi.no_answer_rate) },
    {
      label: t("reports.kpiInvalidPhoneRate"),
      value: formatRate(summary.kpi.invalid_phone_rate),
    },
    {
      label: t("reports.kpiTechnicalRate"),
      value: formatRate(summary.kpi.technical_rate),
      tone: summary.kpi.technical_rate > 0 ? "warning" : undefined,
    },
    {
      // Null is the expected answer here, not an error: a pre-call block leaves
      // no call result to count, so the rate stays null until an intake-block
      // fact source exists (DT-06). formatRate renders that as an em dash — the
      // screen must not round it to 0% and claim no block happened.
      label: t("reports.kpiOperationalBlockedRate"),
      value: formatRate(summary.kpi.operational_blocked_rate),
      testId: "kpi-operational-blocked-rate",
    },
    {
      label: t("reports.kpiTimeToFinal"),
      value: formatDuration(summary.kpi.avg_seconds_to_final),
      testId: "kpi-time-to-final",
    },
  ];

  const volumeMetrics: Metric[] = [
    { label: t("reports.kpiTotalResults"), value: formatNumber(summary.kpi.total_results) },
    {
      label: t("reports.kpiTotalFinalResults"),
      value: formatNumber(summary.kpi.total_final_results),
    },
    { label: t("reports.kpiTotalJobs"), value: formatNumber(summary.kpi.total_call_jobs) },
    {
      label: t("reports.kpiEligibleTasks"),
      value: formatNumber(summary.kpi.total_eligible_tasks),
      testId: "kpi-eligible-tasks",
    },
  ];

  return (
    <CardStack>
      <FreshnessBanner quality={summary.data_quality} />

      <Card title={t("reports.kpiTitle")} accent>
        <Meter
          label={t("reports.kpiConfirmRate")}
          value={formatRate(summary.kpi.confirm_rate)}
          ratio={summary.kpi.confirm_rate}
          tone="success"
          testId="kpi-confirm-rate"
        />
        <Meter
          label={t("reports.kpiAttemptTwoRate")}
          value={formatRate(summary.kpi.attempt_2_rate)}
          ratio={summary.kpi.attempt_2_rate}
          testId="kpi-attempt-2-rate"
        />
        <MetricGrid metrics={rateMetrics} />
      </Card>

      <Card title={t("reports.volumeTitle")}>
        <MetricGrid metrics={volumeMetrics} />
      </Card>

      <Card title={t("reports.trendTitle")}>
        <TrendChart buckets={trend.buckets} bucket={trend.filter.bucket} />
      </Card>

      <Card title={t("reports.taxonomyTitle")} flush>
        <BreakdownTable
          caption={t("reports.taxonomyCaption")}
          keyLabel={t("reports.colResultType")}
          rows={summary.result_taxonomy}
          testId="taxonomy-table"
          family="resultType"
        />
      </Card>

      <Card title={t("reports.breakdownTitle")} flush>
        <BreakdownTable
          caption={`${t("reports.breakdownCaption")} — ${
            tEnum("analyticsDimension", breakdown.dimension)?.label ?? breakdown.dimension
          }`}
          keyLabel={t(`reports.dim${dimensionSuffix(breakdown.dimension)}`)}
          rows={breakdown.rows}
          testId="breakdown-table"
          family={BREAKDOWN_FAMILY[breakdown.dimension]}
        />
      </Card>

      <Card title={t("reports.exportTitle")}>
        <ExportForm
          program={query.program}
          resultType={query.resultType}
          scriptVariant={query.scriptVariant}
          bucket={query.bucket}
          from={query.from}
          to={query.to}
          dimension={dimension}
          minBucketSize={summary.data_quality.min_bucket_size}
        />
      </Card>
    </CardStack>
  );
}

function readQuery(params: Readonly<Record<string, string | string[] | undefined>>): ReportQuery {
  const read = (key: string): string => (typeof params[key] === "string" ? params[key] : "");

  return {
    program: read("program"),
    resultType: read("result_type"),
    scriptVariant: read("script_variant"),
    bucket: read("bucket") === "HOUR" ? "HOUR" : "DAY",
    from: read("from"),
    to: read("to"),
    dimension: asDimension(read("dimension")),
  };
}

/** An unknown dimension falls back rather than reaching the API as garbage. */
function asDimension(value: string): AnalyticsDimension {
  return ANALYTICS_DIMENSIONS.includes(value as AnalyticsDimension)
    ? (value as AnalyticsDimension)
    : "RESULT_TYPE";
}

/**
 * Which dictionary the breakdown's `key` column belongs to.
 *
 * SCRIPT_VARIANT is deliberately absent: its keys are script version strings,
 * not enum values, so it renders as a raw code rather than through a dictionary
 * that would flag every row as untranslated.
 */
const BREAKDOWN_FAMILY: Readonly<Record<AnalyticsDimension, EnumFamily | undefined>> = {
  RESULT_TYPE: "resultType",
  PROGRAM: "programType",
  SCRIPT_VARIANT: undefined,
};

function dimensionSuffix(dimension: AnalyticsDimension): "ResultType" | "ScriptVariant" | "Program" {
  switch (dimension) {
    case "SCRIPT_VARIANT":
      return "ScriptVariant";
    case "PROGRAM":
      return "Program";
    default:
      return "ResultType";
  }
}
