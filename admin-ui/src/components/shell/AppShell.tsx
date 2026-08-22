import type { ReactNode } from "react";

import { formatNumber, t } from "@/lib/i18n";
import type { IvrRole } from "@/lib/rbac/permissions";

import styles from "./AppShell.module.css";
import { ConsoleNav } from "./ConsoleNav";
import { EnvironmentBadge } from "./EnvironmentBadge";
import { GovernanceNotice } from "./GovernanceNotice";
import { SignOutButton } from "./SignOutButton";

export interface AppShellProps {
  readonly actorId: string;
  readonly displayName: string;
  readonly role: IvrRole;
  readonly permissionCount: number;
  readonly environmentLabel: string;
  readonly executionMode: string;
  readonly isMockMode: boolean;
  readonly realCustomerCallAllowed: boolean;
  readonly children: ReactNode;
}

export function AppShell({
  actorId,
  displayName,
  role,
  permissionCount,
  environmentLabel,
  executionMode,
  isMockMode,
  realCustomerCallAllowed,
  children,
}: AppShellProps) {
  return (
    <div className={styles.shell}>
      <a className={styles.skipLink} href="#main-content">
        {t("app.skipToContent")}
      </a>

      <header className={styles.header}>
        <span className={styles.brand}>{t("app.shortTitle")}</span>
        <EnvironmentBadge
          environmentLabel={environmentLabel}
          executionMode={executionMode}
          isMockMode={isMockMode}
        />
        <div className={styles.actor}>
          <span className={styles.actorLine}>
            <span className={styles.actorKey}>{t("auth.signedInAs")}</span>
            <span className={styles.actorValue}>{displayName} · {actorId}</span>
          </span>
          <span className={styles.actorLine}>
            <span className={styles.actorKey}>{t("auth.role")}</span>
            <span className={styles.actorValue}>{role}</span>
            <span className={styles.actorKey}>{t("auth.permissionCount")}</span>
            <span className={styles.actorValue}>{formatNumber(permissionCount)}</span>
          </span>
        </div>
        <SignOutButton />
      </header>

      <div className={styles.body}>
        <ConsoleNav />
        <main id="main-content" className={styles.main}>
          <GovernanceNotice realCustomerCallAllowed={realCustomerCallAllowed} />
          {children}
        </main>
      </div>
    </div>
  );
}
