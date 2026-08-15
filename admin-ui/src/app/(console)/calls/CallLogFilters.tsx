import Link from "next/link";

import { t } from "@/lib/i18n";

import controls from "@/components/forms/Controls.module.css";

export interface CallLogFiltersProps {
  readonly query: {
    readonly program: string;
    readonly status: string;
    readonly queueStatus: string;
    readonly resultType: string;
    readonly orderCode: string;
    readonly correlationId: string;
    readonly nearExpiry: boolean;
  };
}

/**
 * Filters are a plain GET form. `order_code` is sent to the API as a lookup key
 * only — the response carries `order_code_short`, never the full code (D-05).
 */
export function CallLogFilters({ query }: CallLogFiltersProps) {
  return (
    <form method="get" className={controls.bar} aria-label={t("calls.filterSubmit")}>
      <label className={controls.field}>
        <span className={controls.label}>{t("calls.filterOrderCode")}</span>
        <input
          type="text"
          name="order_code"
          defaultValue={query.orderCode}
          className={controls.control}
        />
      </label>
      <label className={controls.field}>
        <span className={controls.label}>{t("calls.filterCorrelation")}</span>
        <input
          type="text"
          name="correlation_id"
          defaultValue={query.correlationId}
          className={controls.control}
        />
      </label>
      <label className={controls.field}>
        <span className={controls.label}>{t("dashboard.filterProgram")}</span>
        <select name="program" defaultValue={query.program} className={controls.control}>
          <option value="">{t("dashboard.filterAll")}</option>
          <option value="GOLDEN_HOUR">GOLDEN_HOUR</option>
          <option value="TWENTY_FOUR_SEVEN">TWENTY_FOUR_SEVEN</option>
        </select>
      </label>
      <label className={controls.field}>
        <span className={controls.label}>{t("calls.filterQueueStatus")}</span>
        <input
          type="text"
          name="queue_status"
          defaultValue={query.queueStatus}
          className={controls.control}
        />
      </label>
      <label className={controls.field}>
        <span className={controls.label}>{t("calls.filterResultType")}</span>
        <input
          type="text"
          name="result_type"
          defaultValue={query.resultType}
          className={controls.control}
        />
      </label>
      <label className={controls.checkboxField}>
        <input
          type="checkbox"
          name="near_expiry"
          value="true"
          defaultChecked={query.nearExpiry}
        />
        <span>{t("calls.filterNearExpiry")}</span>
      </label>
      <button type="submit" className={controls.primary}>
        {t("calls.filterSubmit")}
      </button>
      <Link href="/calls" className={controls.reset}>
        {t("dashboard.filterReset")}
      </Link>
    </form>
  );
}
