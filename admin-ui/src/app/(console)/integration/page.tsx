import { Suspense } from "react";

import { MetricGrid, type Metric } from "@/components/data/MetricGrid";
import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { DependencyBadge } from "@/components/data/DependencyBadge";
import { EnumLabel } from "@/components/data/EnumLabel";
import {
  Callout,
  Card,
  CardStack,
  DataTable,
  PageHeader,
  type Column,
} from "@/components/ui";
import { getIntegrationStatus } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type {
  IvrDependencyStatus,
  IvrFailClosedEvent,
  IvrIntegrationStatus,
} from "@/lib/api/types";
import { requireAdmin, requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";
import { tEnum } from "@/lib/i18n/enum";

export const dynamic = "force-dynamic";

export default async function IntegrationStatusPage() {
  await requireAdmin();
  return (
    <>
      <PageHeader
        title={t("integration.title")}
        subtitle={t("integration.subtitle")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.integration") },
          ],
        }}
      />
      <Suspense fallback={<LoadingSkeleton rows={6} variant="table" />}>
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
    {
      label: t("integration.v1Notification"),
      // W-0033 / P4-5 §2.5. Stated as an invariant rather than read from the API on purpose:
      // V1 notification is immutable-off in the feature-flag guardrails, so it is not a runtime
      // variable. Plumbing it as a live value would imply a precision that does not exist, and
      // would let an operator read "no message was sent" as a failure instead of the design.
      value: t("integration.v1NotificationDisabled"),
      tone: "success",
      testId: "v1-notification",
    },
    { label: t("integration.flagRevision"), value: formatNumber(status.flag_revision) },
  ];

  return (
    <CardStack>
      {status.dependency_probing_available ? null : (
        <Callout tone="warning" testId="probing-unavailable">
          {t("integration.probingUnavailable")}
        </Callout>
      )}

      <Card title={t("integration.runtimeTitle")} accent>
        <MetricGrid metrics={runtimeMetrics} />
      </Card>

      <Card title={t("integration.dependencyTitle")} flush>
        <DataTable
          label={t("integration.dependencyTitle")}
          columns={DEPENDENCY_COLUMNS}
          rows={status.dependencies}
          rowKey={(dependency) => dependency.dependency}
          density="compact"
          pinFirstColumn
        />
      </Card>

      <Card
        title={t("integration.eventsTitle")}
        footer={`${t("dashboard.generatedAt")}: ${formatDateTime(status.generated_at)}`}
        flush={status.recent_fail_closed_events.length > 0}
      >
        {status.recent_fail_closed_events.length === 0 ? (
          <Callout tone="success">{t("integration.noEvents")}</Callout>
        ) : (
          <DataTable
            label={t("integration.eventsTitle")}
            columns={EVENT_COLUMNS}
            rows={status.recent_fail_closed_events}
            rowKey={(event) => `${event.source}:${event.reference}`}
            density="compact"
          />
        )}
      </Card>
    </CardStack>
  );
}

const DEPENDENCY_COLUMNS: readonly Column<IvrDependencyStatus>[] = [
  {
    key: "dependency",
    header: t("integration.colDependency"),
    cell: (dependency) => (
      <EnumLabel family="dependencyName" value={dependency.dependency} showCode />
    ),
  },
  {
    key: "state",
    header: t("integration.colState"),
    cell: (dependency) => (
      <DependencyBadge state={dependency.state} observed={dependency.observed} />
    ),
  },
  {
    key: "detail",
    header: t("integration.colDetail"),
    variant: "wrap",
    cell: (dependency) => <DependencyDetail dependency={dependency} />,
  },
  {
    key: "detailRaw",
    header: t("integration.colDetailRaw"),
    variant: "wrap",
    cell: (dependency) => <code>{dependency.detail}</code>,
  },
  {
    // OD-L10N-02a. `fail_closed_effect` reads like free prose but is not: the API
    // builds it from six hardcoded constants, one per dependency
    // (AdminConfigReadService.BuildDependencies). Since `dependency` is already a
    // code, the console can key the Vietnamese sentence off it and ignore the
    // server string entirely — no contract change, no regenerated DTO.
    key: "effect",
    header: t("integration.colEffect"),
    variant: "wrap",
    cell: (dependency) => (
      <EnumLabel family="failClosedEffect" value={dependency.dependency} />
    ),
  },
  {
    key: "captured",
    header: t("integration.colCaptured"),
    cell: (dependency) =>
      dependency.captured_at === undefined ? "—" : formatDateTime(dependency.captured_at),
  },
];

/**
 * `detail` is three different things wearing one field name (OD-L10N-02a).
 *
 * Three dependencies carry a fixed sentence and one switches between two fixed
 * sentences on its state — all four are keyed off data the console already has,
 * so they translate without touching the contract.
 *
 * W-0116 adds `detail_vi` only for the two interpolated telemetry rows. The raw
 * `detail` remains visible in the adjacent column for exact log lookup. Keeping
 * the old dictionary fallback is intentional: during a rolling deployment this
 * UI can still receive a draft.16 response with no companion field.
 */
function DependencyDetail({ dependency }: { readonly dependency: IvrDependencyStatus }) {
  if (dependency.detail_vi !== undefined && dependency.detail_vi.trim().length > 0) {
    return <>{dependency.detail_vi}</>;
  }

  if (dependency.dependency === "DIAL_KILL_SWITCH") {
    const key = dependency.state === "DOWN" ? "ENGAGED" : "RELEASED";
    return <EnumLabel family="dependencyDetail" value={`DIAL_KILL_SWITCH_${key}`} />;
  }

  const translated = tEnum("dependencyDetail", dependency.dependency);
  return translated?.known === true ? <>{translated.label}</> : <>{dependency.detail}</>;
}

/**
 * A review-item event's `effect` is `"{source_type}: {reason}"` — two codes the
 * API concatenated, both of which already have dictionary entries. Splitting
 * them back apart costs one regex and translates the whole row.
 *
 * W-0116/draft.17 carries the capacity boolean separately. The scope remains a
 * code before the first colon, so the console can translate it without deriving
 * policy from English prose. Missing `hold_new_calls` falls back to the raw effect
 * for rolling compatibility with draft.16.
 */
const REVIEW_EFFECT = /^([A-Z_]+): ([A-Z_]+)$/u;
const CAPACITY_EFFECT_SCOPE = /^([A-Z_]+):/u;

function FailClosedEventEffect({ event }: { readonly event: IvrFailClosedEvent }) {
  if (event.source === "CAPACITY_INCIDENT" && event.hold_new_calls !== undefined) {
    const scope = CAPACITY_EFFECT_SCOPE.exec(event.effect)?.[1];
    if (scope !== undefined) {
      return (
        <>
          <EnumLabel family="incidentScope" value={scope} />
          {": "}
          {t(
            event.hold_new_calls
              ? "integration.capacityCallsHeld"
              : "integration.capacityDispatchNotHeld",
          )}
        </>
      );
    }
  }

  const parts = event.source === "REVIEW_ITEM" ? REVIEW_EFFECT.exec(event.effect) : null;
  if (parts === null) {
    return <>{event.effect}</>;
  }

  return (
    <>
      <EnumLabel family="reviewSourceType" value={parts[1]} />
      {": "}
      <EnumLabel family="reviewReason" value={parts[2]} />
    </>
  );
}

const EVENT_COLUMNS: readonly Column<IvrFailClosedEvent>[] = [
  {
    key: "source",
    header: t("integration.colSource"),
    cell: (event) => <EnumLabel family="failClosedEventSource" value={event.source} />,
  },
  {
    key: "reference",
    header: t("integration.colReference"),
    variant: "mono",
    cell: (event) => event.reference,
  },
  {
    key: "effect",
    header: t("integration.colEffect"),
    variant: "wrap",
    cell: (event) => <FailClosedEventEffect event={event} />,
  },
  {
    key: "correlation",
    header: t("detail.correlationId"),
    variant: "mono",
    cell: (event) => event.correlation_id,
  },
  {
    key: "occurred",
    header: t("integration.colCaptured"),
    cell: (event) => formatDateTime(event.occurred_at),
  },
];
