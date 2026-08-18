import { t } from "@/lib/i18n";

/**
 * A yes/no/unknown value with an accessible name.
 *
 * W-0039 / P5-5. The console used to render a bare `✓` or `—`. A screen reader announces those
 * as nothing useful, and an operator scanning a column of glyphs has to remember which mark
 * means which — the same objection as conveying meaning by colour alone (WCAG 1.4.1), wearing a
 * different costume. The glyph stays because it reads fast; the word travels with it.
 */
export function BooleanCell({ value }: { readonly value: boolean | undefined }) {
  if (value === undefined) {
    return (
      <span title={t("boolean.unknown")}>
        <span aria-hidden="true">—</span>
        <span className="sr-only">{t("boolean.unknown")}</span>
      </span>
    );
  }

  const label = value ? t("boolean.yes") : t("boolean.no");
  return (
    <span title={label}>
      <span aria-hidden="true">{value ? "✓" : "–"}</span>
      <span className="sr-only">{label}</span>
    </span>
  );
}
