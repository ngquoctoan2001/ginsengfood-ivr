import { MetricGrid, type Metric } from "@/components/data/MetricGrid";
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
 * Seed loading and scenario runs stay on the CLI: no API exists for them and the
 * console does not open a write path into the database.
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

        <Card title={t("seed.loaderTitle")}>
          <Callout tone="locked" testId="seed-loader-unavailable">
            {t("seed.loaderUnavailable")}
          </Callout>
        </Card>

        <Card title={t("seed.profilesTitle")} footer={t("seed.profileSource")}>
          <ChipList
            label={t("seed.profilesTitle")}
            items={INTEGRATION_STATUS_PROFILES.map((profile) => ({
              key: profile,
              label: profile,
            }))}
          />
        </Card>
      </CardStack>
    </>
  );
}

/**
 * Names only, mirroring `seed/integration-status.sample.json`. Listing them makes
 * the available fail-closed rehearsals discoverable without giving the console a
 * way to apply one.
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
