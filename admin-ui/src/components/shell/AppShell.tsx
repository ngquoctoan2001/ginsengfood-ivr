import type { ReactNode } from "react";

import { t } from "@/lib/i18n";

import styles from "./AppShell.module.css";
import { ConsoleNav } from "./ConsoleNav";
import { EnvironmentBadge } from "./EnvironmentBadge";
import { GovernanceNotice } from "./GovernanceNotice";
import { SidebarAccount } from "./SidebarAccount";

export interface AppShellProps {
  readonly actorId: string;
  readonly displayName: string;
  readonly environmentLabel: string;
  readonly executionMode: string;
  readonly isMockMode: boolean;
  readonly realCustomerCallAllowed: boolean;
  readonly children: ReactNode;
}

/**
 * The header states what the console is driving; the sidebar states who is
 * driving it. Identity, the profile route and sign-out are one card at the foot
 * of the rail rather than three things scattered along the top band.
 *
 * The actor's role and permission count are no longer threaded through here:
 * the card links to the profile page, which is where both are stated in full.
 */
export function AppShell({
  actorId,
  displayName,
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
      </header>

      <div className={styles.body}>
        <ConsoleNav
          account={
            <SidebarAccount actorId={actorId} displayName={displayName} />
          }
        />
        <main id="main-content" className={styles.main}>
          <GovernanceNotice realCustomerCallAllowed={realCustomerCallAllowed} />
          {children}
        </main>
      </div>
    </div>
  );
}
