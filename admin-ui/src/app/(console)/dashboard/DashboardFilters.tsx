import { DateRangeField, FilterBar, SelectField, countActiveFilters } from "@/components/ui";
import { t } from "@/lib/i18n";

export interface DashboardFiltersProps {
  readonly program: string;
  readonly from: string;
  readonly to: string;
}

/** The two programs the API accepts. Not an enum from the wire: an unknown
 *  value here would reach the API as garbage rather than as a filter. */
const PROGRAMS = [
  { value: "GOLDEN_HOUR", label: "GOLDEN_HOUR" },
  { value: "TWENTY_FOUR_SEVEN", label: "TWENTY_FOUR_SEVEN" },
];

/**
 * Plain GET form — filters live in the URL, so a filtered view is shareable and
 * the screen works without client JavaScript.
 */
export function DashboardFilters({ program, from, to }: DashboardFiltersProps) {
  return (
    <FilterBar
      label={t("dashboard.filterProgram")}
      resetHref="/dashboard"
      activeCount={countActiveFilters({ program, from, to })}
    >
      <SelectField
        label={t("dashboard.filterProgram")}
        name="program"
        defaultValue={program}
        options={PROGRAMS}
        includeAll
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
