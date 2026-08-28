/**
 * `ivr_review_items.reason` is a union of five writers, and one of them does not write an enum.
 *
 * `QueueOnlySuppressionProposer.ProposeAsync` stores the opt-out proposal as a code with its
 * evidence appended — `OPTOUT_THRESHOLD_REACHED;channel=PHONECALL;signals=3;admin_confirmed=false`
 * — so the CRM inbox row carries what the proposal was based on, not just what it concluded. That
 * is the right call on the write side: a proposal an operator cannot audit is a proposal they
 * cannot act on.
 *
 * It is only wrong on the read side. `tEnum` looks the whole string up as one key, misses, and the
 * review queue renders ⚠ plus forty characters of `key=value` where a label belongs. Splitting
 * here rather than in `tEnum` is deliberate: `tEnum` answers for thirty-odd families and only this
 * column carries structured data, so teaching the shared lookup about semicolons would push one
 * writer's format onto every other family — and `tEnum` has the whole `Ui` module downstream.
 */
export interface ReviewReasonDetail {
  /** `channel`, `signals`, … — empty when the segment carried no `=`. */
  readonly key: string;
  readonly value: string;
}

export interface ParsedReviewReason {
  /** The part `reviewReason` can answer for. Equal to the input when nothing was appended. */
  readonly code: string;
  /** Everything after the first `;`, in the order the writer emitted it. */
  readonly details: readonly ReviewReasonDetail[];
}

/**
 * Splits a stored reason into the part the dictionary answers for and the evidence behind it.
 *
 * A segment with no `=` keeps an empty `key` and is still returned rather than dropped. The
 * console does not own this format — the writer does — so an unrecognised segment is something to
 * show, not something to swallow. Same reasoning as NT-4: "I do not understand this" has to be a
 * thing the screen can say out loud.
 */
export function parseReviewReason(raw: string): ParsedReviewReason {
  const segments = raw.split(";");
  const details: ReviewReasonDetail[] = [];

  for (const segment of segments.slice(1)) {
    const trimmed = segment.trim();
    if (trimmed === "") {
      continue;
    }

    const separator = trimmed.indexOf("=");
    details.push(
      separator < 0
        ? { key: "", value: trimmed }
        : { key: trimmed.slice(0, separator), value: trimmed.slice(separator + 1) },
    );
  }

  return { code: segments[0].trim(), details };
}
