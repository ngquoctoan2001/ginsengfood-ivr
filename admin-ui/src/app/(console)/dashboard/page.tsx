import { Suspense } from "react";

import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { MetricGrid, type Metric } from "@/components/data/MetricGrid";
import { getDashboard, listSimChannels } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrDashboardProjection, IvrSimChannelList } from "@/lib/api/types";
import { requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";
import { hasPermission } from "@/lib/rbac/permissions";

import { DashboardFilters } from "./DashboardFilters";
import { QueueActions } from "./QueueActions";
import { SimChannelActions } from "./SimChannelActions";
import table from "@/components/data/DataTable.module.css";
import styles from "./page.module.css";

export const dynamic = "force-dynamic";

export default async function DashboardPage({ searchParams }: PageProps<"/dashboard">) {
  const params = await searchParams;
  const program = typeof params.program === "string" ? params.program : "";
  const from = typeof params.from === "string" ? params.from : "";
  const to = typeof params.to === "string" ? params.to : "";

  return (
    <>
      <header className={styles.header}>
        <h1 className={styles.title}>{t("dashboard.title")}</h1>
        <p className={styles.subtitle}>{t("dashboard.subtitle")}</p>
      </header>
      <DashboardFilters program={program} from={from} to={to} />
      <Suspense key={`${program}|${from}|${to}`} fallback={<LoadingSkeleton rows={6} />}>
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
    { label: t("dashboard.confirmRate"), value: percent(dashboard.results.confirm_rate) },
    { label: t("dashboard.cancelRate"), value: percent(dashboard.results.cancel_rate) },
    { label: t("dashboard.noAnswerRate"), value: percent(dashboard.results.no_answer_rate) },
    {
      label: t("dashboard.technicalRate"),
      value: percent(dashboard.results.technical_exception_rate),
    },
    {
      label: t("dashboard.callSuccessRate"),
      value: percent(dashboard.results.call_success_rate),
      testId: "call-success-rate",
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
      label: t("dashboard.simFailureRate"),
      value: percent(dashboard.sim.failure_rate),
      tone: dashboard.sim.failure_rate > 0 ? "danger" : undefined,
      testId: "sim-failure-rate",
    },
    { label: t("dashboard.simAdapterMode"), value: dashboard.sim.adapter_mode },
  ];

  return (
    <>
      <p className={styles.generatedAt}>
        {`${t("dashboard.generatedAt")}: ${formatDateTime(dashboard.generated_at)}`}
      </p>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("dashboard.kpiTitle")}</h2>
        <MetricGrid metrics={rateMetrics} />
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("dashboard.queueTitle")}</h2>
        <MetricGrid metrics={queueMetrics} />
        {canAct ? <QueueActions /> : <p className={styles.muted}>{t("queue.noPermission")}</p>}
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("dashboard.attemptTitle")}</h2>
        <MetricGrid metrics={attemptMetrics} />
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("dashboard.simTitle")}</h2>
        <MetricGrid metrics={simMetrics} />

        {simChannels.channels.length === 0 ? (
          <p className={styles.muted}>{t("sim.noChannels")}</p>
        ) : (
          <div className={table.scroll}>
            <table className={table.table} data-testid="sim-channel-table">
              <caption className={styles.muted}>{t("sim.tableCaption")}</caption>
              <thead>
                <tr>
                  <th scope="col">{t("sim.colChannel")}</th>
                  <th scope="col">{t("sim.colState")}</th>
                  <th scope="col">{t("sim.colStatus")}</th>
                  <th scope="col">{t("sim.colBusy")}</th>
                  <th scope="col">{t("sim.colFailCount")}</th>
                  <th scope="col">{t("sim.colHealthCheck")}</th>
                  <th scope="col">{t("sim.colAction")}</th>
                </tr>
              </thead>
              <tbody>
                {simChannels.channels.map((channel) => (
                  <tr key={channel.sim_channel_id}>
                    <td className={table.mono}>{channel.sim_channel_id}</td>
                    <td>
                      {channel.enabled ? t("sim.stateEnabled") : t("sim.stateDisabled")}
                      {channel.quarantined ? ` · ${t("sim.quarantined")}` : ""}
                    </td>
                    <td>{channel.status}</td>
                    <td>
                      {channel.busy
                        ? `✓${channel.active_call_job_id === undefined ? "" : ` ${channel.active_call_job_id}`}`
                        : "—"}
                    </td>
                    <td>{formatNumber(channel.fail_count)}</td>
                    <td>
                      {channel.last_health_check_at === undefined
                        ? "—"
                        : formatDateTime(channel.last_health_check_at)}
                    </td>
                    <td>
                      <SimChannelActions
                        simChannelId={channel.sim_channel_id}
                        enabled={channel.enabled}
                        busy={channel.busy}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("dashboard.incidentTitle")}</h2>
        {dashboard.open_incidents.length === 0 ? (
          <p className={styles.muted}>{t("dashboard.noIncident")}</p>
        ) : (
          <div className={table.scroll}>
            <table className={table.table}>
              <thead>
                <tr>
                  <th scope="col">ID</th>
                  <th scope="col">{t("dashboard.incidentScope")}</th>
                  <th scope="col">{t("dashboard.incidentHold")}</th>
                  <th scope="col">{t("dashboard.incidentReason")}</th>
                  <th scope="col">{t("dashboard.incidentMissedDeadline")}</th>
                  <th scope="col">{t("dashboard.incidentOpenedAt")}</th>
                </tr>
              </thead>
              <tbody>
                {dashboard.open_incidents.map((incident) => (
                  <tr key={incident.capacity_incident_id}>
                    <td className={table.mono}>{incident.capacity_incident_id}</td>
                    <td>{incident.scope}</td>
                    <td>{incident.hold_new_calls ? "✓" : "—"}</td>
                    <td>{incident.shortage_reason ?? "—"}</td>
                    <td>{formatNumber(incident.missed_deadline_count)}</td>
                    <td>{formatDateTime(incident.opened_at)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <p className={styles.muted}>
          {`${t("dashboard.missedDeadlineTotal")}: ${formatNumber(dashboard.missed_deadline_count)}`}
        </p>
      </section>
    </>
  );
}

/** Rates arrive as API-computed fractions; the UI only formats them. */
function percent(rate: number): string {
  return `${(rate * 100).toFixed(1)}%`;
}
