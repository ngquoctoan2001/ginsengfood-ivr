import {
  CheckboxField,
  DateRangeField,
  FilterBar,
  SelectField,
  TextField,
  countActiveFilters,
} from "@/components/ui";
import { t } from "@/lib/i18n";
import { enumOptions } from "@/lib/i18n/enum";

export interface CallLogFiltersProps {
  readonly query: {
    readonly program: string;
    readonly status: string;
    readonly queueStatus: string;
    readonly resultType: string;
    readonly orderCode: string;
    readonly correlationId: string;
    readonly nearExpiry: boolean;
    readonly from: string;
    readonly to: string;
  };
}

/**
 * W-0107. Options come from the data dictionary, so the label an operator picks
 * is Vietnamese while the value submitted to the API stays the code (NT-5).
 *
 * Until now `status`, `queue_status` and `result_type` were free-text monospace
 * boxes: the operator had to type `IVR_CONFIRMED` from memory, and a single
 * mistyped character returned an empty table with no explanation. That is a
 * usability defect the localisation work is fixing on the way past, not a
 * separate feature.
 */
const PROGRAMS = enumOptions("programType");
const JOB_STATUSES = enumOptions("jobStatus");
const RESULT_TYPES = enumOptions("resultType");

/**
 * Filters are a plain GET form. `order_code` is sent to the API as a lookup key
 * only — the response carries `order_code_short`, never the full code (D-05).
 */
export function CallLogFilters({ query }: CallLogFiltersProps) {
  return (
    <FilterBar
      label={t("calls.filterSubmit")}
      submitLabel={t("calls.filterSubmit")}
      resetHref="/calls"
      // Listed field by field rather than spread: the caller's query object also
      // carries the page number, which is not a filter.
      activeCount={countActiveFilters({
        orderCode: query.orderCode,
        correlationId: query.correlationId,
        program: query.program,
        status: query.status,
        queueStatus: query.queueStatus,
        resultType: query.resultType,
        from: query.from,
        to: query.to,
        nearExpiry: query.nearExpiry,
      })}
    >
      <TextField
        label={t("calls.filterOrderCode")}
        name="order_code"
        defaultValue={query.orderCode}
        mono
        adornment={<SearchGlyph />}
      />
      <TextField
        label={t("calls.filterCorrelation")}
        name="correlation_id"
        defaultValue={query.correlationId}
        mono
        adornment={<SearchGlyph />}
      />
      <SelectField
        label={t("dashboard.filterProgram")}
        name="program"
        defaultValue={query.program}
        options={PROGRAMS}
        includeAll
      />
      {/* Job status and queue status are different axes: a job can be CLOSED
          while its queue status records how it left the queue. Both are filters
          the API accepts, so both get a control. They draw on the same
          vocabulary, hence the same dictionary family. */}
      <SelectField
        label={t("calls.filterStatus")}
        name="status"
        defaultValue={query.status}
        options={JOB_STATUSES}
        includeAll
      />
      <SelectField
        label={t("calls.filterQueueStatus")}
        name="queue_status"
        defaultValue={query.queueStatus}
        options={JOB_STATUSES}
        includeAll
      />
      <SelectField
        label={t("calls.filterResultType")}
        name="result_type"
        defaultValue={query.resultType}
        options={RESULT_TYPES}
        includeAll
      />
      <DateRangeField
        fromLabel={t("dashboard.filterFrom")}
        toLabel={t("dashboard.filterTo")}
        fromName="from"
        toName="to"
        fromValue={query.from}
        toValue={query.to}
      />
      <CheckboxField
        name="near_expiry"
        value="true"
        defaultChecked={query.nearExpiry}
        label={t("calls.filterNearExpiry")}
      />
    </FilterBar>
  );
}

/** Decorative: the field label is the accessible name. */
function SearchGlyph() {
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
      <circle cx="7.2" cy="7.2" r="4.4" />
      <path d="m10.6 10.6 2.6 2.6" />
    </svg>
  );
}
