import {
  DateRangeField,
  FilterBar,
  SegmentedControl,
  SelectField,
  TextField,
  countActiveFilters,
} from "@/components/ui";
import { t } from "@/lib/i18n";
import { enumOptions } from "@/lib/i18n/enum";

export interface ReportFiltersProps {
  readonly program: string;
  readonly resultType: string;
  readonly scriptVariant: string;
  readonly bucket: string;
  readonly from: string;
  readonly to: string;
  readonly dimension: string;
}

const PROGRAMS = enumOptions("programType");
const RESULT_TYPES = enumOptions("resultType");

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
    <FilterBar
      label={t("reports.filterLegend")}
      resetHref="/reports"
      // Bucket and dimension always carry a value, so they are not counted:
      // they shape the view rather than narrow it.
      activeCount={countActiveFilters({ program, resultType, scriptVariant, from, to })}
    >
      <SelectField
        label={t("dashboard.filterProgram")}
        name="program"
        defaultValue={program}
        options={PROGRAMS}
        includeAll
      />

      <SelectField
        label={t("reports.filterResultType")}
        name="result_type"
        defaultValue={resultType}
        options={RESULT_TYPES}
        includeAll
      />

      {/* Stays free text: a script variant is a version identifier
          (`v3-test-approved`), not an enum, so there is no closed list to offer. */}
      <TextField
        label={t("reports.filterScriptVariant")}
        name="script_variant"
        defaultValue={scriptVariant}
        maxLength={200}
        mono
      />

      {/* Two options that shape the same series — a segmented control shows both
          at once, where a select would hide the alternative behind a click. */}
      <SegmentedControl
        name="bucket"
        label={t("reports.filterBucket")}
        value={bucket}
        options={[
          { value: "DAY", label: t("reports.bucketDay") },
          { value: "HOUR", label: t("reports.bucketHour") },
        ]}
      />

      <SelectField
        label={t("reports.filterDimension")}
        name="dimension"
        defaultValue={dimension}
        options={[
          { value: "RESULT_TYPE", label: t("reports.dimResultType") },
          { value: "SCRIPT_VARIANT", label: t("reports.dimScriptVariant") },
          { value: "PROGRAM", label: t("reports.dimProgram") },
        ]}
      />

      <DateRangeField
        fromLabel={t("dashboard.filterFrom")}
        toLabel={t("dashboard.filterTo")}
        fromName="from"
        toName="to"
        fromValue={from}
        toValue={to}
      />
    </FilterBar>
  );
}
