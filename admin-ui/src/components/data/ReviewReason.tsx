import { t } from "@/lib/i18n";
import { tEnum } from "@/lib/i18n/enum";
import { parseReviewReason, type ReviewReasonDetail } from "@/lib/review/reason";

import { EnumLabel } from "./EnumLabel";
import styles from "./ReviewReason.module.css";

export interface ReviewReasonProps {
  /** `ivr_review_items.reason`, exactly as the API returned it. */
  readonly value: string | null | undefined;
  /** Passed through to `EnumLabel` — on in the detail screen, off in list tables. */
  readonly showCode?: boolean;
  readonly fallback?: string;
}

/**
 * One appended segment, in words.
 *
 * An unrecognised key is printed as `key=value` rather than dropped. The console does not own
 * this format — `QueueOnlySuppressionProposer` does — so a segment added on the write side has to
 * reach the screen looking raw instead of silently disappearing, which is the same bargain NT-4
 * strikes for an untranslated enum value.
 */
function describe(detail: ReviewReasonDetail): string {
  const raw = detail.key === "" ? detail.value : `${detail.key}=${detail.value}`;

  switch (detail.key) {
    case "channel": {
      const channel = tEnum("suppressionChannel", detail.value);
      return t("review.reasonChannel", { value: channel?.label ?? detail.value });
    }

    case "signals": {
      return /^\d+$/u.test(detail.value) ? t("review.reasonSignals", { value: detail.value }) : raw;
    }

    case "admin_confirmed": {
      if (detail.value === "true") {
        return t("review.reasonAdminConfirmedYes");
      }

      // Only `true` and `false` are written today. Anything else is a format change, and
      // reading it as "no" would turn an unknown into a confident false statement about
      // whether a human signed off — the one fact this segment exists to record.
      return detail.value === "false" ? t("review.reasonAdminConfirmedNo") : raw;
    }

    default: {
      return raw;
    }
  }
}

/**
 * A review-queue reason, which is usually an enum value and sometimes an enum value plus evidence.
 *
 * Four of the five writers of `ivr_review_items.reason` store a bare code that `reviewReason`
 * answers for. `QueueOnlySuppressionProposer` stores
 * `OPTOUT_THRESHOLD_REACHED;channel=PHONECALL;signals=3;admin_confirmed=false`, so a plain
 * `EnumLabel` looked the whole string up as one key, missed, and printed ⚠ followed by the raw
 * composite — on the queue whose entire job is to tell an operator why a case is waiting.
 *
 * The split happens here and not in `tEnum` on purpose. `tEnum` serves every family and only this
 * column carries structured data; teaching the shared lookup about semicolons would impose one
 * writer's format on thirty-odd other families, and its blast radius is the whole `Ui` module.
 */
export function ReviewReason({ value, showCode = false, fallback = "—" }: ReviewReasonProps) {
  if (value === null || value === undefined || value.trim() === "") {
    return <>{fallback}</>;
  }

  const parsed = parseReviewReason(value);
  const label = <EnumLabel family="reviewReason" value={parsed.code} showCode={showCode} />;

  if (parsed.details.length === 0) {
    return label;
  }

  return (
    <span className={styles.wrapper}>
      {label}
      <span className={styles.detail}>{parsed.details.map(describe).join(" · ")}</span>
    </span>
  );
}
