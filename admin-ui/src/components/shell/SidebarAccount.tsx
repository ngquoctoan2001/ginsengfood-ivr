"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { RequirePermission } from "@/components/rbac/RequirePermission";
import { t } from "@/lib/i18n";

import styles from "./SidebarAccount.module.css";
import { SignOutButton } from "./SignOutButton";

const PROFILE_HREF = "/profile";

export interface SidebarAccountProps {
  readonly actorId: string;
  readonly displayName: string;
}

/**
 * Up to two initials for the avatar — "Quản trị hệ thống" reads as "QT".
 *
 * The account id is the fallback rather than a placeholder glyph: an empty
 * display name is a data problem, and "AD" still tells the operator which
 * account they are on where "?" would not.
 */
function initialsOf(displayName: string, actorId: string): string {
  const words = displayName.trim().split(/\s+/u).filter((word) => word.length > 0);
  const source = words.length > 0 ? words : [actorId.trim()];

  return source
    .slice(0, 2)
    .map((word) => word.slice(0, 1))
    .join("")
    .toLocaleUpperCase("vi");
}

/**
 * Who is signed in, at the foot of the sidebar rail.
 *
 * The card is the way to the profile page, so it carries only what answers "am
 * I on the right account": the name a human recognises and the id every audit
 * line is written against. Role and permission count used to be printed here
 * too — they are the profile page's own subject, and repeating them cost a
 * 232px rail four lines to say what one click already says.
 *
 * Without `IVR_ACCOUNT_SELF_VIEW` the same card renders as plain text. An
 * operator who may not open the page should still be able to read who they are,
 * and a link to a screen that would answer 403 is worse than no link.
 */
export function SidebarAccount({ actorId, displayName }: SidebarAccountProps) {
  const pathname = usePathname();

  const identity = (
    <>
      <span className={styles.avatar} aria-hidden="true">
        {initialsOf(displayName, actorId)}
      </span>
      <span className={styles.identityText}>
        <span className={styles.name} title={displayName}>
          {displayName}
        </span>
        <span className={styles.actorId}>{actorId}</span>
      </span>
    </>
  );

  return (
    <div className={styles.account}>
      <RequirePermission
        perm="IVR_ACCOUNT_SELF_VIEW"
        fallback={<span className={styles.identity}>{identity}</span>}
      >
        <Link
          href={PROFILE_HREF}
          className={styles.identity}
          aria-current={pathname === PROFILE_HREF ? "page" : undefined}
        >
          {identity}
          {/* The visible label is the operator's own name, so the link's purpose is stated
              after it rather than in an `aria-label` that would replace it (WCAG 2.5.3). */}
          <span className="sr-only">{t("nav.profile")}</span>
        </Link>
      </RequirePermission>

      <SignOutButton />
    </div>
  );
}
