"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { RequirePermission } from "@/components/rbac/RequirePermission";
import { t } from "@/lib/i18n";
import type { IvrPermission } from "@/lib/rbac/permissions";

import styles from "./ConsoleNav.module.css";
import { NavIcon, type NavIconName } from "./NavIcon";

interface NavItem {
  readonly href: string;
  readonly label: string;
  readonly icon: NavIconName;
  readonly perm: IvrPermission;
}

/** Every entry is a route that exists; all are gated on IVR_QUEUE_VIEW. */
const NAV_ITEMS: readonly NavItem[] = [
  { href: "/dashboard", label: t("nav.dashboard"), icon: "dashboard", perm: "IVR_QUEUE_VIEW" },
  { href: "/calls", label: t("nav.callLog"), icon: "callLog", perm: "IVR_QUEUE_VIEW" },
  { href: "/reports", label: t("nav.reports"), icon: "reports", perm: "IVR_QUEUE_VIEW" },
  { href: "/review", label: t("nav.review"), icon: "review", perm: "IVR_QUEUE_VIEW" },
  { href: "/config", label: t("nav.config"), icon: "config", perm: "IVR_QUEUE_VIEW" },
  { href: "/integration", label: t("nav.integration"), icon: "integration", perm: "IVR_QUEUE_VIEW" },
  { href: "/seed", label: t("nav.seed"), icon: "seed", perm: "IVR_QUEUE_VIEW" },
  { href: "/roles", label: t("nav.roles"), icon: "roles", perm: "IVR_QUEUE_VIEW" },
];

export function ConsoleNav() {
  const pathname = usePathname();

  return (
    <nav className={styles.nav} aria-label={t("nav.sectionLabel")}>
      <ul className={styles.list}>
        {NAV_ITEMS.map((item) => (
          <RequirePermission key={item.href} perm={item.perm}>
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
          </RequirePermission>
        ))}
      </ul>
    </nav>
  );
}
