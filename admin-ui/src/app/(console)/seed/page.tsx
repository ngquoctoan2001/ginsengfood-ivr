import { MetricGrid, type Metric } from "@/components/data/MetricGrid";
import { readConfig } from "@/lib/config/env";
import { requireSession } from "@/lib/auth/guard";
import { t } from "@/lib/i18n";

import styles from "./page.module.css";

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
  await requireSession();
  const config = readConfig();

  if (!config.isNonProductionEnvironment) {
    return (
      <>
        <header className={styles.header}>
          <h1 className={styles.title}>{t("seed.title")}</h1>
        </header>
        <p className={styles.locked} role="alert" data-testid="seed-prod-locked">
          {t("seed.prodLocked")}
        </p>
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
      <header className={styles.header}>
        <h1 className={styles.title}>{t("seed.title")}</h1>
        <p className={styles.subtitle}>{t("seed.subtitle")}</p>
      </header>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("seed.adapterTitle")}</h2>
        <MetricGrid metrics={adapterMetrics} />
        <p className={styles.locked} data-testid="real-mode-locked">
          {t("seed.realLocked")}
        </p>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("seed.loaderTitle")}</h2>
        <p className={styles.notice} data-testid="seed-loader-unavailable">
          {t("seed.loaderUnavailable")}
        </p>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("seed.profilesTitle")}</h2>
        <ul className={styles.chips}>
          {INTEGRATION_STATUS_PROFILES.map((profile) => (
            <li key={profile} className={styles.chip}>
              {profile}
            </li>
          ))}
        </ul>
        <p className={styles.notice}>{t("seed.profileSource")}</p>
      </section>
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
