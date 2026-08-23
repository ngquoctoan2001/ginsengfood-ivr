export type NavIconName =
  | "dashboard"
  | "callLog"
  | "reports"
  | "review"
  | "config"
  | "integration"
  | "seed"
  | "flags"
  | "roles";

export interface NavIconProps {
  readonly name: NavIconName;
}

/**
 * Inline SVG glyphs for the console sidebar, hand-drawn for the same reason as
 * StatusIcon: the console pulls in no icon package.
 *
 * These are decorative. Each one sits beside the section's written label, which
 * remains the accessible name of the link, so they are `aria-hidden` — a screen
 * reader announces "Tổng quan", not "grid icon, Tổng quan". Drawn on a 16 unit
 * grid with a 1.6 stroke so they hold their weight next to 13px label text.
 */
export function NavIcon({ name }: NavIconProps) {
  return (
    <svg
      viewBox="0 0 16 16"
      width="16"
      height="16"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      {name === "dashboard" ? (
        <>
          <rect x="2.2" y="2.2" width="5" height="5" rx="1.2" />
          <rect x="8.8" y="2.2" width="5" height="5" rx="1.2" />
          <rect x="2.2" y="8.8" width="5" height="5" rx="1.2" />
          <rect x="8.8" y="8.8" width="5" height="5" rx="1.2" />
        </>
      ) : null}

      {name === "callLog" ? (
        <path d="M3.1 2.7a1.1 1.1 0 0 1 1.2-.5l2 .5a1.1 1.1 0 0 1 .8 1.3l-.4 1.6a1 1 0 0 0 .3 1l2.4 2.4a1 1 0 0 0 1 .3l1.6-.4a1.1 1.1 0 0 1 1.3.8l.5 2a1.1 1.1 0 0 1-.5 1.2 2.5 2.5 0 0 1-1.7.3C6.9 12.6 3.4 9.1 2.8 4.4a2.5 2.5 0 0 1 .3-1.7Z" />
      ) : null}

      {/* A raised flag on a pole: the runtime gates, and the one screen whose
          glyph should not read as a settings cog. Settings are adjusted; these
          are raised and lowered. */}
      {name === "flags" ? (
        <>
          <path d="M4 14V2.6" />
          <path d="M4 3.2h7.6l-1.7 2.6 1.7 2.6H4" />
        </>
      ) : null}

      {name === "reports" ? (
        <>
          <path d="M2.4 13.4h11.2" />
          <path d="M4.8 11V7.4" />
          <path d="M8 11V3.2" />
          <path d="M11.2 11V5.8" />
        </>
      ) : null}

      {name === "review" ? (
        <>
          <path d="M2.6 9.6 4.4 3.5a1.1 1.1 0 0 1 1.05-.8h5.1a1.1 1.1 0 0 1 1.05.8l1.8 6.1v2.5a1.1 1.1 0 0 1-1.1 1.1H3.7a1.1 1.1 0 0 1-1.1-1.1Z" />
          <path d="M2.6 9.6h2.9l.9 1.6h3.2l.9-1.6h2.9" />
        </>
      ) : null}

      {name === "config" ? (
        <>
          <path d="M2.4 4.6h5.2M11.6 4.6h2" />
          <circle cx="9.6" cy="4.6" r="1.7" />
          <path d="M2.4 11.4h1.6M8 11.4h5.6" />
          <circle cx="6" cy="11.4" r="1.7" />
        </>
      ) : null}

      {name === "integration" ? (
        <>
          <path d="M6.7 9.3a2.7 2.7 0 0 0 4 .3l1.7-1.7a2.7 2.7 0 0 0-3.8-3.8l-1 1" />
          <path d="M9.3 6.7a2.7 2.7 0 0 0-4-.3L3.6 8.1a2.7 2.7 0 0 0 3.8 3.8l1-1" />
        </>
      ) : null}

      {name === "seed" ? (
        <>
          <ellipse cx="8" cy="3.9" rx="4.9" ry="2.1" />
          <path d="M3.1 3.9v8.2c0 1.16 2.19 2.1 4.9 2.1s4.9-.94 4.9-2.1V3.9" />
          <path d="M3.1 8c0 1.16 2.19 2.1 4.9 2.1s4.9-.94 4.9-2.1" />
        </>
      ) : null}

      {name === "roles" ? (
        <>
          <path d="M8 1.9 13.1 4v4.1c0 3-2.15 5.15-5.1 5.95C5.05 13.25 2.9 11.1 2.9 8.1V4L8 1.9Z" />
          <path d="M5.9 8 7.4 9.5l2.8-2.9" />
        </>
      ) : null}
    </svg>
  );
}
