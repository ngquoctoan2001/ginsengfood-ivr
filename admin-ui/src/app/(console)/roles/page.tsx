import { requireSession } from "@/lib/auth/guard";
import { MOCK_DIRECTORY } from "@/lib/auth/directory";
import { formatNumber, t, type MessageKey } from "@/lib/i18n";
import { IVR_PERMISSIONS, type IvrPermission } from "@/lib/rbac/permissions";

import table from "@/components/data/DataTable.module.css";
import styles from "./page.module.css";

export const dynamic = "force-dynamic";

/**
 * UI-08 role and permission matrix, read-only.
 *
 * There is no assign or revoke control. DF-01 puts permission management in
 * Permission Core; a second write path here would create a competing source of
 * truth for authorization. This screen documents the mapping and shows what the
 * current session actually holds.
 */
export default async function RolesPage() {
  const session = await requireSession();

  return (
    <>
      <header className={styles.header}>
        <h1 className={styles.title}>{t("roles.title")}</h1>
        <p className={styles.subtitle}>{t("roles.subtitle")}</p>
      </header>

      <p className={styles.notice} data-testid="roles-not-managed-here">
        {t("roles.notManagedHere")}
      </p>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("roles.currentSession")}</h2>
        <dl className={styles.summary}>
          <div>
            <dt>{t("auth.signedInAs")}</dt>
            <dd className={table.mono}>{session.actorId}</dd>
          </div>
          <div>
            <dt>{t("auth.role")}</dt>
            <dd>{session.role}</dd>
          </div>
          <div>
            <dt>{t("auth.permissionCount")}</dt>
            <dd>{formatNumber(session.permissions.length)}</dd>
          </div>
        </dl>
        <ul className={styles.chips}>
          {session.permissions.map((permission) => (
            <li key={permission} className={styles.chip}>
              {permission}
            </li>
          ))}
        </ul>
      </section>

      <section className={styles.section}>
        <div className={table.scroll}>
          <table className={table.table}>
            <thead>
              <tr>
                <th scope="col">{t("roles.colRole")}</th>
                <th scope="col">{t("roles.colActor")}</th>
                <th scope="col">{t("roles.colPermissions")}</th>
              </tr>
            </thead>
            <tbody>
              {MOCK_DIRECTORY.map((entry) => (
                <tr key={entry.actorId}>
                  <td>{entry.role}</td>
                  <td className={table.mono}>{entry.actorId}</td>
                  <td className={table.wrap}>{entry.permissions.join(", ")}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("roles.matrixTitle")}</h2>
        <div className={table.scroll}>
          <table className={table.table}>
            <thead>
              <tr>
                <th scope="col">{t("roles.colPermission")}</th>
                <th scope="col">{t("roles.colScreen")}</th>
                <th scope="col">{t("roles.colRole")}</th>
              </tr>
            </thead>
            <tbody>
              {IVR_PERMISSIONS.map((permission) => {
                const holders = MOCK_DIRECTORY.filter((entry) =>
                  entry.permissions.includes(permission),
                );
                return (
                  <tr key={permission}>
                    <td className={table.mono}>{permission}</td>
                    <td className={table.wrap}>{t(PERMISSION_SCREEN_KEYS[permission])}</td>
                    <td>
                      {holders.length === 0 ? (
                        <span className={styles.ungranted} data-testid={`ungranted-${permission}`}>
                          {t("roles.ungranted")}
                        </span>
                      ) : (
                        holders.map((entry) => entry.role).join(", ")
                      )}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </section>
    </>
  );
}

/**
 * Mirrors the permission-to-screen mapping in `specs/ui/08` §3.
 *
 * W-0039 / P5-5. The descriptions live in the message catalogue, not here. They are
 * operator-facing Vietnamese prose, and prose that sits in a component is prose a translator
 * never sees and a reviewer never diffs against the rest of the console's wording.
 */
const PERMISSION_SCREEN_KEYS: Readonly<Record<IvrPermission, MessageKey>> = {
  IVR_QUEUE_VIEW: "roles.screen.IVR_QUEUE_VIEW",
  IVR_QUEUE_PAUSE: "roles.screen.IVR_QUEUE_PAUSE",
  IVR_QUEUE_RESUME: "roles.screen.IVR_QUEUE_RESUME",
  IVR_SIM_ENABLE: "roles.screen.IVR_SIM_ENABLE",
  IVR_SIM_DISABLE: "roles.screen.IVR_SIM_DISABLE",
  IVR_MANUAL_RETRY: "roles.screen.IVR_MANUAL_RETRY",
  IVR_RESULT_REVIEW: "roles.screen.IVR_RESULT_REVIEW",
  IVR_FLAG_READ: "roles.screen.IVR_FLAG_READ",
  IVR_RUNTIME_GATE_ADMIN: "roles.screen.IVR_RUNTIME_GATE_ADMIN",
};
