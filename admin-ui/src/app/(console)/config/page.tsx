import { Suspense } from "react";

import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { StatusBadge } from "@/components/data/StatusBadge";
import { getScriptCatalog } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrScriptCatalog } from "@/lib/api/types";
import { requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, t } from "@/lib/i18n";

import table from "@/components/data/DataTable.module.css";
import styles from "./page.module.css";

export const dynamic = "force-dynamic";

export default function ScriptConfigPage() {
  return (
    <>
      <header className={styles.header}>
        <h1 className={styles.title}>{t("config.title")}</h1>
        <p className={styles.subtitle}>{t("config.subtitle")}</p>
      </header>
      <p className={styles.notice} data-testid="config-read-only">
        {t("config.readOnlyNotice")}
      </p>
      <Suspense fallback={<LoadingSkeleton rows={6} />}>
        <ScriptCatalogPanels />
      </Suspense>
    </>
  );
}

async function ScriptCatalogPanels() {
  const session = await requireSession();
  const config = readConfig();

  let catalog: IvrScriptCatalog | null = null;
  let error: ErrorEnvelopeView | null = null;

  try {
    catalog = (await getScriptCatalog({ session, config })).data;
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    error = cause.toEnvelope();
  }

  if (error !== null || catalog === null) {
    return <ErrorAlert error={error!} />;
  }

  return (
    <>
      <p
        className={
          catalog.production_target_v1_fields_approved ? styles.notice : styles.locked
        }
        data-testid="od-v1-15-lock"
      >
        {catalog.production_target_v1_fields_approved
          ? t("config.od15Open")
          : t("config.od15Locked")}
      </p>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("config.versionsTitle")}</h2>
        {catalog.versions.length === 0 ? (
          <p className={styles.muted}>{t("config.noVersions")}</p>
        ) : (
          <div className={table.scroll}>
            <table className={table.table}>
              <thead>
                <tr>
                  <th scope="col">{t("config.colTemplate")}</th>
                  <th scope="col">{t("config.colVersion")}</th>
                  <th scope="col">{t("config.colStatus")}</th>
                  <th scope="col">{t("config.colApprovals")}</th>
                  <th scope="col">{t("config.colMissing")}</th>
                  <th scope="col">{t("config.colTemplateValid")}</th>
                  <th scope="col">{t("config.colCreated")}</th>
                </tr>
              </thead>
              <tbody>
                {catalog.versions.map((version) => (
                  <tr key={`${version.template_id}:${version.version}`}>
                    <td className={table.mono}>{version.template_id}</td>
                    <td className={table.mono}>{version.version}</td>
                    <td>
                      {version.status}
                      <span className={styles.badgeSlot}>
                        <StatusBadge
                          tone={version.missing_approvals.length === 0 ? "success" : "warning"}
                          testId={`approval-badge-${version.version}`}
                        >
                          {version.missing_approvals.length === 0
                            ? t("config.approvedBadge")
                            : t("config.notApprovedBadge")}
                        </StatusBadge>
                      </span>
                    </td>
                    <td>
                      {version.approvals.length === 0
                        ? "—"
                        : version.approvals
                            .map((approval) => approval.approval_type)
                            .join(", ")}
                    </td>
                    <td>
                      {version.missing_approvals.length === 0
                        ? "—"
                        : version.missing_approvals.join(", ")}
                    </td>
                    <td>
                      {version.template_valid ? (
                        "✓"
                      ) : (
                        <span className={styles.invalid}>{t("config.templateInvalid")}</span>
                      )}
                    </td>
                    <td>{formatDateTime(version.created_at)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("config.dtmfTitle")}</h2>
        <div className={table.scroll}>
          <table className={table.table}>
            <thead>
              <tr>
                <th scope="col">{t("config.dtmfKey")}</th>
                <th scope="col">{t("config.dtmfMeaning")}</th>
                <th scope="col">{t("config.dtmfEnabled")}</th>
              </tr>
            </thead>
            <tbody>
              {catalog.dtmf_map.map((key) => (
                <tr key={key.key}>
                  <td className={table.mono}>{key.key}</td>
                  <td data-testid={`dtmf-meaning-${key.key}`}>{key.meaning}</td>
                  <td data-testid={`dtmf-enabled-${key.key}`}>
                    {key.enabled ? "✓" : "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        <p className={styles.notice} data-testid="key-9-notice">
          {t("config.key9Notice")}
        </p>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("config.allowedTitle")}</h2>
        <ul className={styles.chips}>
          {catalog.allowed_input_fields.map((field) => (
            <li key={field} className={styles.chipOk}>
              {field}
            </li>
          ))}
        </ul>
        <h2 className={styles.sectionTitle}>{t("config.prohibitedTitle")}</h2>
        <ul className={styles.chips}>
          {catalog.prohibited_variables.map((variable) => (
            <li key={variable} className={styles.chipDanger}>
              {variable}
            </li>
          ))}
        </ul>
      </section>
    </>
  );
}
