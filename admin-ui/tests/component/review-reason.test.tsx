import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { ReviewReason } from "@/components/data/ReviewReason";
import { parseReviewReason } from "@/lib/review/reason";
import enums from "@/i18n/enums.vi.json";

/** The exact shape `QueueOnlySuppressionProposer.ProposeAsync` writes. */
const COMPOSITE = "OPTOUT_THRESHOLD_REACHED;channel=PHONECALL;signals=3;admin_confirmed=false";

describe("parseReviewReason splits a stored reason from the evidence behind it", () => {
  it("returns a bare code unchanged, with no details", () => {
    expect(parseReviewReason("CALLBACK_TIMEOUT")).toEqual({
      code: "CALLBACK_TIMEOUT",
      details: [],
    });
  });

  it("splits the opt-out composite into its code and its segments", () => {
    expect(parseReviewReason(COMPOSITE)).toEqual({
      code: "OPTOUT_THRESHOLD_REACHED",
      details: [
        { key: "channel", value: "PHONECALL" },
        { key: "signals", value: "3" },
        { key: "admin_confirmed", value: "false" },
      ],
    });
  });

  it("keeps a segment that carries no `=` rather than dropping it", () => {
    // The console does not own this format. A segment it cannot parse is something to show.
    expect(parseReviewReason("SOME_CODE;bare").details).toEqual([{ key: "", value: "bare" }]);
  });

  it("ignores empty segments from a trailing or doubled separator", () => {
    expect(parseReviewReason("SOME_CODE;;channel=PHONECALL;").details).toEqual([
      { key: "channel", value: "PHONECALL" },
    ]);
  });
});

describe("ReviewReason renders the opt-out proposal as words, not as a raw composite", () => {
  it("translates the code and reads the evidence back in Vietnamese", () => {
    render(<ReviewReason value={COMPOSITE} />);

    // Before W-0107's follow-up this rendered ⚠ plus the whole 74-character string, because
    // `tEnum` looked the composite up as one key. `data-enum-known` is the structural proof
    // that the dictionary answered, independent of how the label is worded.
    const label = screen.getByText(enums.reviewReason.OPTOUT_THRESHOLD_REACHED);
    expect(label.closest("[data-enum-known]")?.getAttribute("data-enum-known")).toBe("true");

    expect(screen.getByText(/Gọi điện/u).textContent).toContain("3");
    expect(screen.getByText(/chưa có admin xác nhận/u)).toBeTruthy();
  });

  it("leaves a bare code to EnumLabel, with no detail line", () => {
    const { container } = render(<ReviewReason value="CALLBACK_TIMEOUT" />);

    expect(screen.getByText(enums.reviewReason.CALLBACK_TIMEOUT)).toBeTruthy();
    expect(container.textContent).not.toContain("·");
  });

  it("shows an unreadable segment verbatim instead of guessing at it", () => {
    // `admin_confirmed` is the one segment where a wrong guess asserts that a human did or did
    // not sign off. An unexpected value has to stay visible rather than collapse into "no".
    render(<ReviewReason value="OPTOUT_ADMIN_CONFIRMED;admin_confirmed=maybe;weird=1" />);

    expect(screen.getByText(/admin_confirmed=maybe/u).textContent).toContain("weird=1");
  });

  it("separates absent from untranslated", () => {
    const { container } = render(<ReviewReason value={null} />);
    expect(container.textContent).toBe("—");
  });
});
