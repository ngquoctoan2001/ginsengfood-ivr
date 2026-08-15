"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { RequirePermission } from "@/components/rbac/RequirePermission";
import { t } from "@/lib/i18n";
import type { IvrPermission } from "@/lib/rbac/permissions";

import styles from "./ConsoleNav.module.css";

interface NavItem {
  readonly href: string;
  readonly label: string;
  readonly perm: IvrPermission;
}

/** Every entry is a route that exists; all are gated on IVR_QUEUE_VIEW. */
const NAV_ITEMS: readonly NavItem[] = [
  { href: "/dashboard", label: t("nav.dashboard"), perm: "IVR_QUEUE_VIEW" },
  { href: "/calls", label: t("nav.callLog"), perm: "IVR_QUEUE_VIEW" },
  { href: "/reports", label: t("nav.reports"), perm: "IVR_QUEUE_VIEW" },
  { href: "/review", label: t("nav.review"), perm: "IVR_QUEUE_VIEW" },
  { href: "/config", label: t("nav.config"), perm: "IVR_QUEUE_VIEW" },
  { href: "/integration", label: t("nav.integration"), perm: "IVR_QUEUE_VIEW" },
  { href: "/seed", label: t("nav.seed"), perm: "IVR_QUEUE_VIEW" },
  { href: "/roles", label: t("nav.roles"), perm: "IVR_QUEUE_VIEW" },
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
                {item.label}
              </Link>
            </li>
          </RequirePermission>
        ))}
      </ul>
    </nav>
  );
}
