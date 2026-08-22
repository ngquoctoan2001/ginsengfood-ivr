import { Button, Callout, SelectField, TextField } from "@/components/ui";
import { t } from "@/lib/i18n";

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

      <SelectField
        label={t("reports.exportDimension")}
        name="dimension"
        defaultValue={dimension}
        options={[
          { value: "RESULT_TYPE", label: t("reports.dimResultType") },
          { value: "SCRIPT_VARIANT", label: t("reports.dimScriptVariant") },
          { value: "PROGRAM", label: t("reports.dimProgram") },
        ]}
      />

      <TextField
        label={t("reports.exportReason")}
        name="reason"
        required
        minLength={8}
        maxLength={200}
        width="lg"
        placeholder={t("reports.exportReasonPlaceholder")}
        data-testid="export-reason"
      />

      <Button type="submit" variant="primary" data-testid="export-submit" icon={<DownloadGlyph />}>
        {t("reports.exportSubmit")}
      </Button>

      <div className={styles.notice}>
        <Callout tone="info" testId="export-notice">
          {`${t("reports.exportNotice")} (k=${minBucketSize})`}
        </Callout>
      </div>
    </form>
  );
}

/** Decorative: the button label is the accessible name. */
function DownloadGlyph() {
  return (
    <svg
      viewBox="0 0 16 16"
      width="13"
      height="13"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <path d="M8 2.6v7.2M5.2 7.4 8 10.2l2.8-2.8" />
      <path d="M2.8 11.4v1a1.4 1.4 0 0 0 1.4 1.4h7.6a1.4 1.4 0 0 0 1.4-1.4v-1" />
    </svg>
  );
}
