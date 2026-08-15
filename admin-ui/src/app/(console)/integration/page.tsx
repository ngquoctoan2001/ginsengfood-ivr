import { Suspense } from "react";

import { MetricGrid, type Metric } from "@/components/data/MetricGrid";
import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { getIntegrationStatus } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrIntegrationStatus } from "@/lib/api/types";
import { requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";

import { DependencyBadge } from "@/components/data/DependencyBadge";
import table from "@/components/data/DataTable.module.css";
import styles from "./page.module.css";

export const dynamic = "force-dynamic";

export default function IntegrationStatusPage() {
  return (
    <>
      <header className={styles.header}>
        <h1 className={styles.title}>{t("integration.title")}</h1>
        <p className={styles.subtitle}>{t("integration.subtitle")}</p>
      </header>
      <Suspense fallback={<LoadingSkeleton rows={6} />}>
        <IntegrationStatusPanels />
      </Suspense>
    </>
  );
}

async function IntegrationStatusPanels() {
  const session = await requireSession();
  const config = readConfig();

  let status: IvrIntegrationStatus | null = null;
  let error: ErrorEnvelopeView | null = null;

  try {
    status = (await getIntegrationStatus({ session, config })).data;
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    error = cause.toEnvelope();
  }

  if (error !== null || status === null) {
    return <ErrorAlert error={error!} />;
  }

  const runtimeMetrics: Metric[] = [
    { label: t("governance.executionMode"), value: status.execution_mode },
    { label: t("seed.simProvider"), value: status.sim_provider },
    { label: t("seed.salesProvider"), value: status.sales_provider },
    { label: t("detail.policy"), value: status.attempt_policy_version },
    {
      label: t("integration.killSwitch"),
      value: status.global_dial_kill_switch
        ? t("integration.killSwitchOn")
        : t("integration.killSwitchOff"),
      tone: status.global_dial_kill_switch ? "warning" : "success",
      testId: "kill-switch",
    },
    { label: t("integration.flagRevision"), value: formatNumber(status.flag_revision) },
  ];

  return (
    <>
      {status.dependency_probing_available ? null : (
        <p className={styles.warning} data-testid="probing-unavailable">
          {t("integration.probingUnavailable")}
        </p>
      )}

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("integration.runtimeTitle")}</h2>
        <MetricGrid metrics={runtimeMetrics} />
      </section>

      <section className={styles.section}>
        <div className={table.scroll}>
          <table className={table.table}>
            <thead>
              <tr>
                <th scope="col">{t("integration.colDependency")}</th>
                <th scope="col">{t("integration.colState")}</th>
                <th scope="col">{t("integration.colDetail")}</th>
                <th scope="col">{t("integration.colEffect")}</th>
                <th scope="col">{t("integration.colCaptured")}</th>
              </tr>
            </thead>
            <tbody>
              {status.dependencies.map((dependency) => (
                <tr key={dependency.dependency}>
                  <td className={table.mono}>{dependency.dependency}</td>
                  <td>
                    <DependencyBadge
                      state={dependency.state}
                      observed={dependency.observed}
                    />
                  </td>
                  <td className={table.wrap}>{dependency.detail}</td>
                  <td className={table.wrap}>{dependency.fail_closed_effect}</td>
                  <td>
                    {dependency.captured_at === undefined
                      ? "—"
                      : formatDateTime(dependency.captured_at)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("integration.eventsTitle")}</h2>
        {status.recent_fail_closed_events.length === 0 ? (
          <p className={styles.muted}>{t("integration.noEvents")}</p>
        ) : (
          <div className={table.scroll}>
            <table className={table.table}>
              <thead>
                <tr>
                  <th scope="col">{t("integration.colSource")}</th>
                  <th scope="col">{t("integration.colReference")}</th>
                  <th scope="col">{t("integration.colEffect")}</th>
                  <th scope="col">{t("detail.correlationId")}</th>
                  <th scope="col">{t("integration.colCaptured")}</th>
                </tr>
              </thead>
              <tbody>
                {status.recent_fail_closed_events.map((event) => (
                  <tr key={`${event.source}:${event.reference}`}>
                    <td>{event.source}</td>
                    <td className={table.mono}>{event.reference}</td>
                    <td className={table.wrap}>{event.effect}</td>
                    <td className={table.mono}>{event.correlation_id}</td>
                    <td>{formatDateTime(event.occurred_at)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <p className={styles.muted}>
        {`${t("dashboard.generatedAt")}: ${formatDateTime(status.generated_at)}`}
      </p>
    </>
  );
}
