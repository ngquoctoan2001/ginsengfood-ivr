import Link from "next/link";

import { t } from "@/lib/i18n";

import controls from "@/components/forms/Controls.module.css";

export interface DashboardFiltersProps {
  readonly program: string;
  readonly from: string;
  readonly to: string;
}

/**
 * Plain GET form — filters live in the URL, so a filtered view is shareable and
 * the screen works without client JavaScript.
 */
export function DashboardFilters({ program, from, to }: DashboardFiltersProps) {
  return (
    <form method="get" className={controls.bar} aria-label={t("dashboard.filterProgram")}>
      <label className={controls.field}>
        <span className={controls.label}>{t("dashboard.filterProgram")}</span>
        <select name="program" defaultValue={program} className={controls.control}>
          <option value="">{t("dashboard.filterAll")}</option>
          <option value="GOLDEN_HOUR">GOLDEN_HOUR</option>
          <option value="TWENTY_FOUR_SEVEN">TWENTY_FOUR_SEVEN</option>
        </select>
      </label>
      <label className={controls.field}>
        <span className={controls.label}>{t("dashboard.filterFrom")}</span>
        <input type="date" name="from" defaultValue={from} className={controls.control} />
      </label>
      <label className={controls.field}>
        <span className={controls.label}>{t("dashboard.filterTo")}</span>
        <input type="date" name="to" defaultValue={to} className={controls.control} />
      </label>
      <button type="submit" className={controls.primary}>
        {t("dashboard.filterApply")}
      </button>
      <Link href="/dashboard" className={controls.reset}>
        {t("dashboard.filterReset")}
      </Link>
    </form>
  );
}
