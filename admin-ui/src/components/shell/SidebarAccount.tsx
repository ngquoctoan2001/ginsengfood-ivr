"use client";

import styles from "./SidebarAccount.module.css";

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
    .map((word) => word.charAt(0))
    .join("")
    .toLocaleUpperCase("vi");
}

/**
 * Who the console is acting as, at the foot of the sidebar rail.
 *
 * W-0122 turned this from a link into a label. It used to open `/profile` and
 * hide itself behind `IVR_ACCOUNT_SELF_VIEW`; there is no profile page, no
 * self-view permission and no signed-in user any more — Module 3 owns operator
 * identity and asserts it per request as `X-Actor-Id`.
 *
 * What it shows is unchanged and still the point: the name a human recognises
 * and the id every audit line is written against. Module 3 rebuilding this rail
 * will have a real session behind it, and this is the place to put the link back.
 */
export function SidebarAccount({ actorId, displayName }: SidebarAccountProps) {
  return (
    <div className={styles.account}>
      <span className={styles.identity}>
        <span className={styles.avatar} aria-hidden="true">
          {initialsOf(displayName, actorId)}
        </span>
        <span className={styles.identityText}>
          <span className={styles.name} title={displayName}>
            {displayName}
          </span>
          <span className={styles.actorId}>{actorId}</span>
        </span>
      </span>
    </div>
  );
}
