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
 * A route is shown either because the viewer holds a permission the route actually uses, or
 * because the route is admin-only.
 *
 * W-0128 left this as a rendering rule and nothing more. There is no session and no role behind
 * it any more — the API decides by credential tier, and will refuse an action this nav happened
 * to reveal. What the rule still buys is that a screen is not offered to someone whose console
 * cannot use it, which is exactly the job Module 3's own nav will have.
 *
 * The admin-only entries once borrowed `IVR_ACCOUNT_VIEW` as a stand-in for "is an admin". That
 * worked only while the permission happened to be admin-only, and the account system it came from
 * is gone; naming the intent directly is what survives a permission model changing owner.
 *
 * `/accounts`, `/roles` and `/profile` are not in this list because those screens no longer
 * exist: Module 3 owns operator identity, so the console has nothing to show about it.
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
              return role === "admin" ? <Fragment key={item.href}>{entry}</Fragment> : null;
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
