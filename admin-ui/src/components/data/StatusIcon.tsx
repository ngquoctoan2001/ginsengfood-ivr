export type StatusTone = "success" | "warning" | "danger" | "neutral";

export interface StatusIconProps {
  readonly tone: StatusTone;
}

/**
 * Inline SVG status glyphs, hand-drawn so the console pulls in no icon package.
 *
 * These exist because colour alone must never carry meaning (WCAG 1.4.1). Every
 * badge pairs one of these with a written label, so the state survives both
 * greyscale and colour-blindness. They are decorative in that role — the word
 * beside them is the accessible name — hence `aria-hidden`.
 */
export function StatusIcon({ tone }: StatusIconProps) {
  return (
    <svg
      viewBox="0 0 16 16"
      width="13"
      height="13"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.75"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      {tone === "success" ? (
        <>
          <circle cx="8" cy="8" r="6.25" />
          <path d="M5.4 8.2 7.2 10l3.4-3.6" />
        </>
      ) : null}
      {tone === "warning" ? (
        <>
          <path d="M8 2.4 14.2 13H1.8L8 2.4Z" />
          <path d="M8 6.6v2.8" />
          <path d="M8 11.3h.01" />
        </>
      ) : null}
      {tone === "danger" ? (
        <>
          <circle cx="8" cy="8" r="6.25" />
          <path d="M10 6 6 10M6 6l4 4" />
        </>
      ) : null}
      {tone === "neutral" ? (
        <>
          <circle cx="8" cy="8" r="6.25" strokeDasharray="2.6 2.2" />
          <path d="M5.6 8h4.8" />
        </>
      ) : null}
    </svg>
  );
}
