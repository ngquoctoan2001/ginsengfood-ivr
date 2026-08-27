import { MetricGrid, type Metric } from "@/components/data/MetricGrid";
import { SeedActions } from "./SeedActions";
import { Callout, Card, CardStack, ChipList, PageHeader } from "@/components/ui";
import { readConfig } from "@/lib/config/env";
import { requireAdmin } from "@/lib/auth/guard";
import { t } from "@/lib/i18n";

export const dynamic = "force-dynamic";

/**
 * UI-07 seed/mock, read-only.
 *
 * Two guards are structural rather than cosmetic: the whole screen refuses to
 * render outside a non-production environment, and there is no control that can
 * change the adapter mode. Moving to REAL needs a purchased SIM (DT-01) and the
 * release gate (DF-03); it is not a toggle.
 *
 * W-0112 gives the three UI-07 actions an API. What has not changed is where the guard lives:
 * `Ivr.Api` does not map those routes outside a non-production deployment, so this screen hiding
 * its controls in production is a courtesy to the reader rather than the control. Changing the
 * adapter mode is still not offered — moving to REAL needs a purchased SIM (DT-01) and the
 * release gate (DF-03), and it is not a toggle.
 */
export default async function SeedMockPage() {
  await requireAdmin();
  const config = readConfig();

  if (!config.isNonProductionEnvironment) {
    return (
      <>
        <PageHeader title={t("seed.title")} />
        <Callout tone="locked" role="alert" testId="seed-prod-locked">
          {t("seed.prodLocked")}
        </Callout>
      </>
    );
  }

  const adapterMetrics: Metric[] = [
    {
      label: t("seed.adapterMode"),
      value: config.executionMode,
      tone: config.isMockMode ? "success" : "warning",
      testId: "adapter-mode",
    },
    { label: t("governance.environment"), value: config.environmentLabel },
    {
      label: t("auth.role"),
      value: config.realCustomerCallAllowed ? "REAL_CALL=YES" : "REAL_CALL=NO",
      tone: config.realCustomerCallAllowed ? "danger" : "success",
    },
  ];

  return (
    <>
      <PageHeader
        title={t("seed.title")}
        subtitle={t("seed.subtitle")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.seed") },
          ],
        }}
      />

      <CardStack>
        <Card title={t("seed.adapterTitle")} accent>
          <MetricGrid metrics={adapterMetrics} />
          <Callout tone="locked" testId="real-mode-locked">
            {t("seed.realLocked")}
          </Callout>
        </Card>

        <Card title={t("seed.loaderTitle")} footer={t("seed.profileSource")}>
          <ChipList
            label={t("seed.profilesTitle")}
            items={INTEGRATION_STATUS_PROFILES.map((profile) => ({
              key: profile,
              label: profile,
            }))}
          />
          <SeedActions
            scenarioIds={CALL_SCENARIOS}
            profileIds={INTEGRATION_STATUS_PROFILES}
          />
        </Card>
      </CardStack>
    </>
  );
}

/**
 * Names only, mirroring `seed/integration-status.sample.json` and
 * `seed/call-scenarios.sample.json`.
 *
 * Duplicated from the seed files rather than fetched, because this list only has to name the
 * choices; the API reads the files itself and answers 404 for an id it does not find. A stale
 * entry here therefore produces a clear refusal, not a silently wrong rehearsal.
 */
const INTEGRATION_STATUS_PROFILES: readonly string[] = [
  "STATUS-all-up",
  "STATUS-order-core-down",
  "STATUS-ops-down",
  "STATUS-ops-ready-503",
  "STATUS-crm-down",
  "STATUS-sim-down",
  "STATUS-evidence-down",
];

const CALL_SCENARIOS: readonly string[] = [
  "SCN-001-confirm",
  "SCN-002-cancel",
  "SCN-003-no-answer-final",
  "SCN-004-busy-then-confirm",
  "SCN-005-invalid-phone",
  "SCN-006-technical-exception",
  "SCN-007-window-expired",
  "SCN-008-operational-block-recall",
  "SCN-009-race-recall-after-key1",
  "SCN-010-m3-authoritative-call",
  "SCN-011-duplicate-callback",
  "SCN-012-opt-out-block",
  "SCN-013-not-official-order",
  "SCN-014-needs-support-key9",
  "SCN-015-capacity-incident",
];
