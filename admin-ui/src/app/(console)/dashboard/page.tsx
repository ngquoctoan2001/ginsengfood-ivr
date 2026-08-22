import { Suspense } from "react";

import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { BooleanCell } from "@/components/data/BooleanCell";
import { EnumLabel } from "@/components/data/EnumLabel";
import { MetricGrid, type Metric } from "@/components/data/MetricGrid";
import {
  Callout,
  Card,
  CardStack,
  DataTable,
  Meter,
  PageHeader,
  type Column,
} from "@/components/ui";
import { formatRate } from "@/lib/analytics/format";
import { getDashboard, listSimChannels } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type {
  IvrCapacityIncidentSummary,
  IvrDashboardProjection,
  IvrSimChannel,
  IvrSimChannelList,
} from "@/lib/api/types";
import { requirePermission, requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";
import { tEnum } from "@/lib/i18n/enum";
import { hasPermission } from "@/lib/rbac/permissions";

import { DashboardFilters } from "./DashboardFilters";
import { QueueActions } from "./QueueActions";
import { SimChannelActions } from "./SimChannelActions";

export const dynamic = "force-dynamic";

export default async function DashboardPage({ searchParams }: PageProps<"/dashboard">) {
  await requirePermission("IVR_QUEUE_VIEW");
  const params = await searchParams;
  const program = typeof params.program === "string" ? params.program : "";
  const from = typeof params.from === "string" ? params.from : "";
  const to = typeof params.to === "string" ? params.to : "";

  return (
    <>
      <PageHeader title={t("dashboard.title")} subtitle={t("dashboard.subtitle")} />
      <DashboardFilters program={program} from={from} to={to} />
      <Suspense
        key={`${program}|${from}|${to}`}
        fallback={<LoadingSkeleton rows={8} variant="metrics" />}
      >
        <DashboardPanels program={program} from={from} to={to} />
      </Suspense>
    </>
  );
}

async function DashboardPanels({
  program,
  from,
  to,
}: {
  program: string;
  from: string;
  to: string;
}) {
  const session = await requireSession();
  const config = readConfig();

  let dashboard: IvrDashboardProjection | null = null;
  let simChannels: IvrSimChannelList | null = null;
  let error: ErrorEnvelopeView | null = null;

  try {
    // Independent reads, issued together rather than as a waterfall.
    const [dashboardResponse, simResponse] = await Promise.all([
      getDashboard(
        { session, config },
        {
          program: program === "" ? undefined : program,
          // The API takes an instant; a date input gives a day.
          from: from === "" ? undefined : `${from}T00:00:00Z`,
          to: to === "" ? undefined : `${to}T23:59:59Z`,
        },
      ),
      listSimChannels({ session, config }),
    ]);
    dashboard = dashboardResponse.data;
    simChannels = simResponse.data;
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    error = cause.toEnvelope();
  }

  if (error !== null || dashboard === null || simChannels === null) {
    return <ErrorAlert error={error!} />;
  }

  const canAct =
    hasPermission(session.permissions, "IVR_QUEUE_PAUSE") ||
    hasPermission(session.permissions, "IVR_QUEUE_RESUME");

  const rateMetrics: Metric[] = [
    { label: t("dashboard.cancelRate"), value: percent(dashboard.results.cancel_rate) },
    { label: t("dashboard.noAnswerRate"), value: percent(dashboard.results.no_answer_rate) },
    {
      label: t("dashboard.technicalRate"),
      value: percent(dashboard.results.technical_exception_rate),
    },
    { label: t("dashboard.resultTotal"), value: formatNumber(dashboard.results.total) },
  ];

  const queueMetrics: Metric[] = [
    {
      label: t("queue.status"),
      value: dashboard.queue.paused ? t("queue.statusPaused") : t("queue.statusRunning"),
      tone: dashboard.queue.paused ? "warning" : "success",
      testId: "queue-status",
    },
    { label: t("dashboard.queued"), value: formatNumber(dashboard.queue.queued) },
    { label: t("dashboard.heldMock"), value: formatNumber(dashboard.queue.held_mock) },
    {
      label: t("dashboard.heldAdminReview"),
      value: formatNumber(dashboard.queue.held_admin_review),
    },
    { label: t("dashboard.dispatching"), value: formatNumber(dashboard.queue.dispatching) },
    { label: t("dashboard.openTotal"), value: formatNumber(dashboard.queue.open_total) },
    { label: t("dashboard.closedTotal"), value: formatNumber(dashboard.queue.closed_total) },
    {
      label: t("dashboard.nearExpiry"),
      value: formatNumber(dashboard.queue.near_expiry),
      tone: dashboard.queue.near_expiry > 0 ? "warning" : undefined,
    },
    {
      label: t("dashboard.attemptTwoPending"),
      value: formatNumber(dashboard.queue.attempt_two_pending),
      testId: "attempt-two-pending",
    },
    {
      label: t("dashboard.blocked"),
      value: formatNumber(dashboard.queue.blocked),
      tone: dashboard.queue.blocked > 0 ? "warning" : undefined,
      testId: "queue-blocked",
    },
  ];

  const attemptMetrics: Metric[] = [
    { label: t("dashboard.attemptTotal"), value: formatNumber(dashboard.attempts.total) },
    {
      label: t("dashboard.attemptCounted"),
      value: formatNumber(dashboard.attempts.counted_customer_attempts),
    },
    {
      label: t("dashboard.attemptTechnicalRetry"),
      value: formatNumber(dashboard.attempts.technical_retries),
    },
    { label: t("dashboard.attemptActive"), value: formatNumber(dashboard.attempts.active) },
  ];

  const simMetrics: Metric[] = [
    { label: t("dashboard.simTotal"), value: formatNumber(dashboard.sim.total) },
    { label: t("dashboard.simEnabled"), value: formatNumber(dashboard.sim.enabled) },
    { label: t("dashboard.simIdle"), value: formatNumber(dashboard.sim.idle) },
    { label: t("dashboard.simActive"), value: formatNumber(dashboard.sim.active) },
    { label: t("dashboard.simDisabled"), value: formatNumber(dashboard.sim.disabled) },
    {
      label: t("dashboard.simHealthFailed"),
      value: formatNumber(dashboard.sim.health_failed),
      tone: dashboard.sim.health_failed > 0 ? "danger" : undefined,
    },
    {
      label: t("dashboard.simQuarantined"),
      value: formatNumber(dashboard.sim.quarantined),
      tone: dashboard.sim.quarantined > 0 ? "warning" : undefined,
    },
    {
      // Metric.value is a plain string, so this reads the label directly rather
      // than rendering EnumLabel. The raw code stays visible on the Seed screen,
      // which is where the adapter mode is actually reasoned about.
      label: t("dashboard.simAdapterMode"),
      value: tEnum("executionMode", dashboard.sim.adapter_mode)?.label ?? "—",
    },
  ];

  return (
    <CardStack>
      {/*
       * The three rates an operator is answerable for get a meter as well as a
       * figure. The number is still the reading — the bar is a second channel
       * for the same value, so nothing depends on judging a length.
       */}
      <Card
        title={t("dashboard.kpiTitle")}
        description={`${t("dashboard.generatedAt")}: ${formatDateTime(dashboard.generated_at)}`}
        accent
      >
        <Meter
          label={t("dashboard.confirmRate")}
          value={percent(dashboard.results.confirm_rate)}
          ratio={dashboard.results.confirm_rate}
          tone="success"
        />
        <Meter
          label={t("dashboard.callSuccessRate")}
          value={percent(dashboard.results.call_success_rate)}
          ratio={dashboard.results.call_success_rate}
          testId="call-success-rate"
        />
        <MetricGrid metrics={rateMetrics} />
      </Card>

      <Card
        title={t("dashboard.queueTitle")}
        actions={canAct ? <QueueActions /> : undefined}
      >
        <MetricGrid metrics={queueMetrics} />
        {canAct ? null : <Callout tone="neutral">{t("queue.noPermission")}</Callout>}
      </Card>

      <Card title={t("dashboard.attemptTitle")}>
        <MetricGrid metrics={attemptMetrics} />
      </Card>

      <Card title={t("dashboard.simTitle")}>
        <MetricGrid metrics={simMetrics} />
        <Meter
          label={t("dashboard.simFailureRate")}
          value={percent(dashboard.sim.failure_rate)}
          ratio={dashboard.sim.failure_rate}
          tone={dashboard.sim.failure_rate > 0 ? "danger" : undefined}
          testId="sim-failure-rate"
        />
        {simChannels.channels.length === 0 ? (
          <Callout tone="neutral">{t("sim.noChannels")}</Callout>
        ) : (
          <DataTable
            label={t("dashboard.simTitle")}
            testId="sim-channel-table"
            caption={t("sim.tableCaption")}
            columns={SIM_COLUMNS}
            rows={simChannels.channels}
            rowKey={(channel) => channel.sim_channel_id}
            density="compact"
            pinFirstColumn
          />
        )}
      </Card>

      <Card
        title={t("dashboard.incidentTitle")}
        footer={`${t("dashboard.missedDeadlineTotal")}: ${formatNumber(dashboard.missed_deadline_count)}`}
      >
        {dashboard.open_incidents.length === 0 ? (
          <Callout tone="success">{t("dashboard.noIncident")}</Callout>
        ) : (
          <DataTable
            label={t("dashboard.incidentTitle")}
            columns={INCIDENT_COLUMNS}
            rows={dashboard.open_incidents}
            rowKey={(incident) => incident.capacity_incident_id}
            density="compact"
          />
        )}
      </Card>
    </CardStack>
  );
}

const SIM_COLUMNS: readonly Column<IvrSimChannel>[] = [
  {
    key: "id",
    header: t("sim.colChannel"),
    variant: "mono",
    cell: (channel) => channel.sim_channel_id,
  },
  {
    key: "state",
    header: t("sim.colState"),
    cell: (channel) =>
      `${channel.enabled ? t("sim.stateEnabled") : t("sim.stateDisabled")}${
        channel.quarantined ? ` · ${t("sim.quarantined")}` : ""
      }`,
  },
  {
    key: "status",
    header: t("sim.colStatus"),
    cell: (channel) => <EnumLabel family="simStatus" value={channel.status} />,
  },
  {
    key: "busy",
    header: t("sim.colBusy"),
    cell: (channel) => (
      <>
        <BooleanCell value={channel.busy} />
        {channel.busy && channel.active_call_job_id !== undefined
          ? ` ${channel.active_call_job_id}`
          : ""}
      </>
    ),
  },
  {
    key: "failCount",
    header: t("sim.colFailCount"),
    variant: "numeric",
    cell: (channel) => formatNumber(channel.fail_count),
  },
  {
    key: "healthCheck",
    header: t("sim.colHealthCheck"),
    cell: (channel) =>
      channel.last_health_check_at === undefined
        ? "—"
        : formatDateTime(channel.last_health_check_at),
  },
  {
    key: "action",
    header: t("sim.colAction"),
    cell: (channel) => (
      <SimChannelActions
        simChannelId={channel.sim_channel_id}
        enabled={channel.enabled}
        busy={channel.busy}
      />
    ),
  },
];

const INCIDENT_COLUMNS: readonly Column<IvrCapacityIncidentSummary>[] = [
  {
    key: "id",
    header: "ID",
    variant: "mono",
    cell: (incident) => incident.capacity_incident_id,
  },
  {
    key: "scope",
    header: t("dashboard.incidentScope"),
    cell: (incident) => <EnumLabel family="incidentScope" value={incident.scope} />,
  },
  {
    key: "hold",
    header: t("dashboard.incidentHold"),
    cell: (incident) => <BooleanCell value={incident.hold_new_calls} />,
  },
  {
    key: "reason",
    header: t("dashboard.incidentReason"),
    variant: "wrap",
    cell: (incident) => <EnumLabel family="shortageReason" value={incident.shortage_reason} />,
  },
  {
    key: "missed",
    header: t("dashboard.incidentMissedDeadline"),
    variant: "numeric",
    cell: (incident) => formatNumber(incident.missed_deadline_count),
  },
  {
    key: "openedAt",
    header: t("dashboard.incidentOpenedAt"),
    cell: (incident) => formatDateTime(incident.opened_at),
  },
];

/**
 * Rates arrive as API-computed fractions; the UI only formats them.
 * W-0039: delegates to the shared formatter so the dashboard and the reports screen cannot
 * drift into two different notations for the same number.
 */
function percent(rate: number): string {
  return formatRate(rate);
}
