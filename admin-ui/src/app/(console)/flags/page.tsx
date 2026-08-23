import { Suspense } from "react";

import { BooleanCell } from "@/components/data/BooleanCell";
import { EnumLabel } from "@/components/data/EnumLabel";
import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import {
  Callout,
  Card,
  CardStack,
  ChipList,
  DescriptionList,
  PageHeader,
} from "@/components/ui";
import { getFeatureFlags, verifyKillSwitch } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrFeatureFlagReadResult, IvrKillSwitchVerification } from "@/lib/api/types";
import { requireAdmin, requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { t } from "@/lib/i18n";

import { RuntimeGateActions } from "./RuntimeGateActions";

export const dynamic = "force-dynamic";

/**
 * Runtime gates (W-0110).
 *
 * `OD-V1-20` moved the P0 constraint from "no role holds this permission" to
 * "every press needs four-eyes, a matching actor and an audit record". Until
 * this screen existed the only way to press was a hand-built `curl` — which is
 * precisely the path with no four-eyes and nothing to tell the person pressing
 * which direction they are moving the risk in.
 */
export default async function RuntimeGatesPage() {
  await requireAdmin();
  return (
    <>
      <PageHeader
        title={t("flags.title")}
        subtitle={t("flags.subtitle")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [{ label: t("nav.console"), href: "/dashboard" }, { label: t("nav.flags") }],
        }}
      />
      <Suspense fallback={<LoadingSkeleton rows={6} variant="table" />}>
        <RuntimeGatePanels />
      </Suspense>
    </>
  );
}

async function RuntimeGatePanels() {
  const session = await requireSession();
  const config = readConfig();
  const context = { session, config };
  const environment = config.environmentLabel;

  // The kill-switch probe is read first and on its own, because it is the one
  // call that answers even when the provider is down: it reports
  // providerReadable=false rather than throwing. The snapshot read throws in
  // that case, and an operator staring at an error page during an incident must
  // still be told whether dialling is stopped.
  let killSwitch: IvrKillSwitchVerification | null = null;
  let killSwitchError: ErrorEnvelopeView | null = null;
  try {
    killSwitch = (await verifyKillSwitch(context, environment)).data;
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    killSwitchError = cause.toEnvelope();
  }

  let flags: IvrFeatureFlagReadResult | null = null;
  let flagsError: ErrorEnvelopeView | null = null;
  try {
    flags = (await getFeatureFlags(context, environment)).data;
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    flagsError = cause.toEnvelope();
  }

  // Unreadable is displayed as ENGAGED, never as blank and never as "off". A
  // gate whose state cannot be read is a gate that must be assumed closed, and
  // an empty cell would read as "nothing is stopping calls".
  const readable = killSwitch?.providerReadable ?? false;
  const killSwitchEngaged = readable ? killSwitch!.globalDialKillSwitch : true;

  return (
    <CardStack>
      <Callout
        tone={killSwitchEngaged ? "success" : "danger"}
        role="alert"
        testId="kill-switch-state"
      >
        {killSwitchEngaged ? t("flags.killSwitchEngaged") : t("flags.killSwitchReleased")}
      </Callout>

      {readable ? null : (
        <Callout tone="warning" role="alert" testId="flags-provider-unreadable">
          {t("flags.providerUnreadable")}
        </Callout>
      )}

      <Card title={t("flags.killSwitchTitle")}>
        <DescriptionList
          layout="rows"
          items={[
            {
              label: t("flags.environment"),
              value: environment,
              mono: true,
              testId: "flags-environment",
            },
            {
              label: t("flags.revision"),
              value: readable ? String(killSwitch!.revision) : t("flags.unknown"),
              mono: true,
              testId: "flags-revision",
            },
            {
              label: t("flags.killSwitch"),
              value: killSwitchEngaged ? t("flags.engaged") : t("flags.released"),
              testId: "flags-kill-switch",
            },
            {
              label: t("flags.realCallsEnabled"),
              // Not read from the snapshot: when the provider is unreadable the
              // snapshot is unavailable, and the honest answer is still "no".
              value: readable && killSwitch!.realCallsEnabled
                ? t("flags.yes")
                : t("flags.no"),
              testId: "flags-real-calls",
            },
          ]}
        />
        {killSwitchError === null ? null : <ErrorAlert error={killSwitchError} />}
      </Card>

      {flags === null ? (
        <Card title={t("flags.snapshotTitle")}>
          <Callout tone="warning" role="alert">
            {t("flags.snapshotUnavailable")}
          </Callout>
          {flagsError === null ? null : <ErrorAlert error={flagsError} />}
        </Card>
      ) : (
        <Card title={t("flags.snapshotTitle")} footer={t("flags.snapshotFooter")}>
          <DescriptionList
            layout="rows"
            items={[
              {
                label: t("flags.executionMode"),
                value: (
                  <EnumLabel family="executionMode" value={flags.snapshot.executionMode} />
                ),
                testId: "flags-execution-mode",
              },
              {
                label: t("flags.realCustomerCallAllowed"),
                value: <BooleanCell value={flags.snapshot.realCustomerCallAllowed} />,
                testId: "flags-real-customer-call-allowed",
              },
              {
                label: t("flags.recordingEnabled"),
                value: <BooleanCell value={flags.snapshot.recordingEnabled} />,
              },
              {
                label: t("flags.attemptPolicyVersion"),
                value: flags.snapshot.attemptPolicyVersion,
                mono: true,
              },
              {
                label: t("flags.fromCache"),
                value: <BooleanCell value={flags.fromCache} />,
              },
            ]}
          />
        </Card>
      )}

      <Card title={t("flags.allowlistTitle")} footer={t("flags.allowlistFooter")}>
        {flags === null || flags.snapshot.labDestinationAllowlist.length === 0 ? (
          <Callout tone="neutral" testId="flags-allowlist-empty">
            {t("flags.allowlistEmpty")}
          </Callout>
        ) : (
          <ChipList
            label={t("flags.allowlistTitle")}
            items={flags.snapshot.labDestinationAllowlist.map((destination) => ({
              key: destination,
              label: destination,
              // "danger", not "neutral": every entry here is a real telephone
              // destination the lab is allowed to dial.
              tone: "danger" as const,
            }))}
          />
        )}
      </Card>

      <Card title={t("flags.actionsTitle")} footer={t("flags.asymmetryNotice")}>
        <RuntimeGateActions
          allowRiskIncrease={
            config.isNonProductionEnvironment && config.executionMode !== "PRODUCTION_REAL"
          }
        />
      </Card>
    </CardStack>
  );
}
