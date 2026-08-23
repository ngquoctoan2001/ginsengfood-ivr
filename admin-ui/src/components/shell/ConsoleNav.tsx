"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Fragment, type ReactNode } from "react";

import { usePermissions } from "@/components/rbac/PermissionProvider";
import { RequirePermission } from "@/components/rbac/RequirePermission";
import { t } from "@/lib/i18n";
import type { IvrPermission } from "@/lib/rbac/permissions";

import styles from "./ConsoleNav.module.css";
import { NavIcon, type NavIconName } from "./NavIcon";

/**
 * A route is shown either because the session carries a permission the route actually uses, or
 * because the route is Admin-only.
 *
 * The admin-only entries previously borrowed `IVR_ACCOUNT_VIEW` as a stand-in for "is an admin".
 * It worked only because that permission happens to be Admin-only today: grant it to a future
 * support role and reports, config and seed would appear in their sidebar for no stated reason.
 * The server pages behind these routes gate on `requireAdmin()`, and the API gates them with
 * `IvrRoles.ConsoleAdminPolicy` — so the nav now states the same rule those two state.
 *
 * `/profile` is not in this list: it describes the actor rather than the work, so it is reached
 * by clicking the account card in the rail's footer.
 */
type NavItem = {
  readonly href: string;
  readonly label: string;
  readonly icon: NavIconName;
} & ({ readonly perm: IvrPermission } | { readonly adminOnly: true });

const NAV_ITEMS: readonly NavItem[] = [
  { href: "/dashboard", label: t("nav.dashboard"), icon: "dashboard", perm: "IVR_QUEUE_VIEW" },
  { href: "/calls", label: t("nav.callLog"), icon: "callLog", perm: "IVR_QUEUE_VIEW" },
  { href: "/reports", label: t("nav.reports"), icon: "reports", adminOnly: true },
  { href: "/review", label: t("nav.review"), icon: "review", adminOnly: true },
  { href: "/config", label: t("nav.config"), icon: "config", adminOnly: true },
  { href: "/integration", label: t("nav.integration"), icon: "integration", adminOnly: true },
  { href: "/flags", label: t("nav.flags"), icon: "flags", adminOnly: true },
  { href: "/seed", label: t("nav.seed"), icon: "seed", adminOnly: true },
  { href: "/accounts", label: t("nav.accounts"), icon: "roles", adminOnly: true },
  { href: "/roles", label: t("nav.roles"), icon: "roles", adminOnly: true },
];

export interface ConsoleNavProps {
  /**
   * The signed-in actor, pinned to the foot of the rail. It arrives as a slot rather than as
   * data because it carries the display name, which the permission context does not publish.
   */
  readonly account?: ReactNode;
}

export function ConsoleNav({ account }: ConsoleNavProps) {
  const pathname = usePathname();
  const { role } = usePermissions();

  return (
    <nav className={styles.nav} aria-label={t("nav.sectionLabel")}>
      <div className={styles.rail}>
        <ul className={styles.list}>
          {NAV_ITEMS.map((item) => {
            const entry = (
              <li>
                <Link
                  href={item.href}
                  className={styles.link}
                  aria-current={pathname === item.href ? "page" : undefined}
                >
                  <span className={styles.icon}>
                    <NavIcon name={item.icon} />
                  </span>
                  <span className={styles.label}>{item.label}</span>
                </Link>
              </li>
            );

            if ("adminOnly" in item) {
              // A Fragment, not a wrapper element: only <li> is valid inside <ul>.
              return role === "Admin" ? <Fragment key={item.href}>{entry}</Fragment> : null;
            }

            return (
              <RequirePermission key={item.href} perm={item.perm}>
                {entry}
              </RequirePermission>
            );
          })}
        </ul>

        <div className={styles.footer}>{account}</div>
      </div>
    </nav>
  );
}
