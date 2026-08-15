import { t } from "@/lib/i18n";

import controls from "@/components/forms/Controls.module.css";
import styles from "./ExportForm.module.css";

export interface ExportFormProps {
  readonly program: string;
  readonly resultType: string;
  readonly scriptVariant: string;
  readonly bucket: string;
  readonly from: string;
  readonly to: string;
  readonly dimension: string;
  readonly minBucketSize: number;
}

/**
 * A plain GET form onto the CSV route handler.
 *
 * `reason` is `required` with a `minLength` that matches the server's rule, so
 * the browser stops the obvious case — but the server is what enforces it. The
 * form carries the active filter as hidden fields so the file matches the screen
 * the operator is looking at.
 */
export function ExportForm({
  program,
  resultType,
  scriptVariant,
  bucket,
  from,
  to,
  dimension,
  minBucketSize,
}: ExportFormProps) {
  return (
    <form
      method="get"
      action="/reports/export"
      className={styles.form}
      data-testid="export-form"
      aria-label={t("reports.exportTitle")}
    >
      <input type="hidden" name="program" value={program} />
      <input type="hidden" name="result_type" value={resultType} />
      <input type="hidden" name="script_variant" value={scriptVariant} />
      <input type="hidden" name="bucket" value={bucket} />
      <input type="hidden" name="from" value={from} />
      <input type="hidden" name="to" value={to} />

      <label className={controls.field}>
        <span className={controls.label}>{t("reports.exportDimension")}</span>
        <select name="dimension" defaultValue={dimension} className={controls.control}>
          <option value="RESULT_TYPE">{t("reports.dimResultType")}</option>
          <option value="SCRIPT_VARIANT">{t("reports.dimScriptVariant")}</option>
          <option value="PROGRAM">{t("reports.dimProgram")}</option>
        </select>
      </label>

      <label className={controls.field}>
        <span className={controls.label}>{t("reports.exportReason")}</span>
        <input
          type="text"
          name="reason"
          required
          minLength={8}
          maxLength={200}
          className={controls.control}
          data-testid="export-reason"
          placeholder={t("reports.exportReasonPlaceholder")}
        />
      </label>

      <button type="submit" className={controls.primary} data-testid="export-submit">
        {t("reports.exportSubmit")}
      </button>

      <p className={styles.notice} data-testid="export-notice">
        {`${t("reports.exportNotice")} (k=${minBucketSize})`}
      </p>
    </form>
  );
}
