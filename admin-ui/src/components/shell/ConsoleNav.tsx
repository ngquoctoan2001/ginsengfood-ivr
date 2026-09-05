"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import type { ReactNode } from "react";

import { RequirePermission } from "@/components/rbac/RequirePermission";
import { t } from "@/lib/i18n";
import type { IvrPermission } from "@/lib/rbac/permissions";

import styles from "./ConsoleNav.module.css";
import { NavIcon, type NavIconName } from "./NavIcon";

/**
 * A route is shown because the viewer holds a permission the route actually uses.
 *
 * W-0128 left this as a rendering rule and nothing more. There is no session behind it any more —
 * the API decides by credential tier, and will refuse an action this nav happened to reveal. What
 * the rule still buys is that a screen is not offered to someone whose console cannot use it,
 * which is exactly the job Module 3's own nav will have.
 *
 * W-0193 removed the `adminOnly` escape hatch, which hid six of these eight entries from every
 * viewer. It resolved against `role`, and `role` came from the tier the SHELL was rendered with
 * (`requireScope("read")` in the console layout) rather than from anything the viewer holds — so
 * it was permanently `operator`, and Reports, Review, Config, Integration, Runtime gates and Seed
 * were unreachable by clicking even though all six rendered fine when their URL was typed. Naming
 * a permission per entry says the same thing the flag meant to say, and says it about the viewer.
 *
 * The permission on each row is the one that screen's own controls gate on, so the nav and the
 * screen can never disagree about who the screen is for. `/reports` and `/integration` are reads,
 * and the API files reads of dashboards, reports and dependency state under the read tier.
 *
 * `/accounts`, `/roles` and `/profile` are not in this list because those screens no longer
 * exist: Module 3 owns operator identity, so the console has nothing to show about it.
 */
type NavItem = {
  readonly href: string;
  readonly label: string;
  readonly icon: NavIconName;
  readonly perm: IvrPermission;
};

const NAV_ITEMS: readonly NavItem[] = [
  { href: "/dashboard", label: t("nav.dashboard"), icon: "dashboard", perm: "IVR_QUEUE_VIEW" },
  { href: "/calls", label: t("nav.callLog"), icon: "callLog", perm: "IVR_QUEUE_VIEW" },
  { href: "/reports", label: t("nav.reports"), icon: "reports", perm: "IVR_QUEUE_VIEW" },
  { href: "/review", label: t("nav.review"), icon: "review", perm: "IVR_RESULT_REVIEW" },
  { href: "/config", label: t("nav.config"), icon: "config", perm: "IVR_SCRIPT_EDIT" },
  { href: "/integration", label: t("nav.integration"), icon: "integration", perm: "IVR_FLAG_READ" },
  { href: "/flags", label: t("nav.flags"), icon: "flags", perm: "IVR_FLAG_READ" },
  { href: "/seed", label: t("nav.seed"), icon: "seed", perm: "IVR_DEV_TOOLING" },
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
