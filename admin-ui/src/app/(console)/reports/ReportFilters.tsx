import Link from "next/link";

import { t } from "@/lib/i18n";

import controls from "@/components/forms/Controls.module.css";

export interface ReportFiltersProps {
  readonly program: string;
  readonly resultType: string;
  readonly scriptVariant: string;
  readonly bucket: string;
  readonly from: string;
  readonly to: string;
  readonly dimension: string;
}

/** Filters live in the URL, so a reported view is shareable and bookmarkable. */
export function ReportFilters({
  program,
  resultType,
  scriptVariant,
  bucket,
  from,
  to,
  dimension,
}: ReportFiltersProps) {
  return (
    <form method="get" className={controls.bar} aria-label={t("reports.filterLegend")}>
      <label className={controls.field}>
        <span className={controls.label}>{t("dashboard.filterProgram")}</span>
        <select name="program" defaultValue={program} className={controls.control}>
          <option value="">{t("dashboard.filterAll")}</option>
          <option value="GOLDEN_HOUR">GOLDEN_HOUR</option>
          <option value="TWENTY_FOUR_SEVEN">TWENTY_FOUR_SEVEN</option>
        </select>
      </label>

      <label className={controls.field}>
        <span className={controls.label}>{t("reports.filterResultType")}</span>
        <input
          type="text"
          name="result_type"
          defaultValue={resultType}
          maxLength={200}
          className={controls.control}
          placeholder="IVR_CONFIRMED"
        />
      </label>

      <label className={controls.field}>
        <span className={controls.label}>{t("reports.filterScriptVariant")}</span>
        <input
          type="text"
          name="script_variant"
          defaultValue={scriptVariant}
          maxLength={200}
          className={controls.control}
        />
      </label>

      <label className={controls.field}>
        <span className={controls.label}>{t("reports.filterBucket")}</span>
        <select name="bucket" defaultValue={bucket} className={controls.control}>
          <option value="DAY">{t("reports.bucketDay")}</option>
          <option value="HOUR">{t("reports.bucketHour")}</option>
        </select>
      </label>

      <label className={controls.field}>
        <span className={controls.label}>{t("reports.filterDimension")}</span>
        <select name="dimension" defaultValue={dimension} className={controls.control}>
          <option value="RESULT_TYPE">{t("reports.dimResultType")}</option>
          <option value="SCRIPT_VARIANT">{t("reports.dimScriptVariant")}</option>
          <option value="PROGRAM">{t("reports.dimProgram")}</option>
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
      <Link href="/reports" className={controls.reset}>
        {t("dashboard.filterReset")}
      </Link>
    </form>
  );
}
