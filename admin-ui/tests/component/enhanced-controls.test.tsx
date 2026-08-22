import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderToStaticMarkup } from "react-dom/server";
import { afterEach, describe, expect, it } from "vitest";

import { DateField, SelectField } from "@/components/ui";
import vi from "@/i18n/vi.json";

const messages: Record<string, string> = vi;

const PROGRAMS = [
  { value: "GOLDEN_HOUR", label: "GOLDEN_HOUR" },
  { value: "TWENTY_FOUR_SEVEN", label: "TWENTY_FOUR_SEVEN" },
];

function hiddenValue(container: HTMLElement, name: string): string | undefined {
  return container.querySelector<HTMLInputElement>(`input[type=hidden][name="${name}"]`)?.value;
}

/**
 * UT-UI-CONTROL-01 — the dropdown and the calendar are enhancements, not
 * replacements.
 *
 * The console's filters are plain GET forms, and that only holds if the markup
 * the server sends is submittable on its own. These two suites check both ends
 * of the bargain: the server-rendered pass is a native control, and the
 * hydrated pass is the custom one with the same field name.
 */
describe("UT-UI-CONTROL-01 progressive enhancement", () => {
  it("sends a native select in the server-rendered markup", () => {
    const markup = renderToStaticMarkup(
      <SelectField
        label={messages["dashboard.filterProgram"]}
        name="program"
        options={PROGRAMS}
        defaultValue="GOLDEN_HOUR"
        includeAll
      />,
    );

    // A real select, with a real name: a browser with no JavaScript can still
    // pick a program and submit the filter.
    expect(markup).toContain("<select");
    expect(markup).toContain('name="program"');
    expect(markup).toContain("GOLDEN_HOUR");
    expect(markup).not.toContain('role="combobox"');
  });

  it("sends a native date input in the server-rendered markup", () => {
    const markup = renderToStaticMarkup(
      <DateField
        label={messages["dashboard.filterFrom"]}
        name="from"
        defaultValue="2026-08-01"
      />,
    );

    expect(markup).toContain('type="date"');
    expect(markup).toContain('name="from"');
    expect(markup).toContain('value="2026-08-01"');
  });
});

describe("UT-UI-CONTROL-02 the dropdown", () => {
  it("opens, picks with the pointer, and posts the chosen value", async () => {
    const user = userEvent.setup();
    const { container } = render(
      <SelectField
        label={messages["dashboard.filterProgram"]}
        name="program"
        options={PROGRAMS}
        includeAll
      />,
    );

    // Hydrated: the control is a combobox, and the value rides in a hidden
    // input under the same name the native select used.
    const trigger = screen.getByRole("combobox");
    expect(trigger).toHaveAttribute("aria-expanded", "false");
    expect(hiddenValue(container, "program")).toBe("");

    await user.click(trigger);
    expect(trigger).toHaveAttribute("aria-expanded", "true");

    const listbox = screen.getByRole("listbox");
    await user.click(within(listbox).getByRole("option", { name: /TWENTY_FOUR_SEVEN/ }));

    expect(hiddenValue(container, "program")).toBe("TWENTY_FOUR_SEVEN");
    expect(trigger).toHaveAttribute("aria-expanded", "false");
    expect(trigger).toHaveTextContent("TWENTY_FOUR_SEVEN");
  });

  it("is operable from the keyboard alone", async () => {
    const user = userEvent.setup();
    const { container } = render(
      <SelectField
        label={messages["dashboard.filterProgram"]}
        name="program"
        options={PROGRAMS}
        defaultValue="GOLDEN_HOUR"
      />,
    );

    const trigger = screen.getByRole("combobox");
    trigger.focus();

    await user.keyboard("{ArrowDown}");
    expect(trigger).toHaveAttribute("aria-expanded", "true");
    // Focus stays on the trigger; the highlighted row is published instead.
    expect(trigger).toHaveFocus();
    expect(trigger).toHaveAttribute("aria-activedescendant");

    await user.keyboard("{ArrowDown}{Enter}");
    expect(hiddenValue(container, "program")).toBe("TWENTY_FOUR_SEVEN");

    await user.keyboard("{ArrowDown}");
    expect(screen.getByRole("listbox")).toBeInTheDocument();
    await user.keyboard("{Escape}");
    expect(screen.queryByRole("listbox")).toBeNull();
    // Escape abandons the menu without changing the value.
    expect(hiddenValue(container, "program")).toBe("TWENTY_FOUR_SEVEN");
  });

  /**
   * The panel must not be laid out inside its trigger's flow. Three ancestors
   * in this console clip their contents — the filter bar and the card round
   * their corners, the data table scrolls sideways — and an absolutely
   * positioned menu inside any of them is cropped rather than overlaid. jsdom
   * computes no layout, so what is checked here is the mechanism: the panel is
   * fixed and placed from measured viewport coordinates.
   */
  it("escapes the clipping of any ancestor by positioning itself in the viewport", async () => {
    const user = userEvent.setup();
    render(
      <SelectField
        label={messages["dashboard.filterProgram"]}
        name="program"
        options={PROGRAMS}
      />,
    );

    await user.click(screen.getByRole("combobox"));

    const listbox = screen.getByRole("listbox");
    expect(listbox.style.position).toBe("fixed");
    expect(listbox.style.left).not.toBe("");
  });

  it("marks the chosen option rather than relying on its colour", async () => {
    const user = userEvent.setup();
    render(
      <SelectField
        label={messages["dashboard.filterProgram"]}
        name="program"
        options={PROGRAMS}
        defaultValue="TWENTY_FOUR_SEVEN"
      />,
    );

    await user.click(screen.getByRole("combobox"));

    expect(screen.getByRole("option", { name: /TWENTY_FOUR_SEVEN/ })).toHaveAttribute(
      "aria-selected",
      "true",
    );
    expect(screen.getByRole("option", { name: /^GOLDEN_HOUR/ })).toHaveAttribute(
      "aria-selected",
      "false",
    );
  });
});

describe("UT-UI-CONTROL-03 the calendar", () => {
  it("picks a day and posts it as an ISO date", async () => {
    const user = userEvent.setup();
    const { container } = render(
      <DateField
        label={messages["dashboard.filterFrom"]}
        name="from"
        defaultValue="2026-08-14"
      />,
    );

    // The trigger reads in vi-VN notation; the wire value stays ISO.
    const trigger = screen.getByRole("button", { name: messages["dashboard.filterFrom"] });
    expect(trigger).toHaveTextContent("14/08/2026");
    expect(hiddenValue(container, "from")).toBe("2026-08-14");

    await user.click(trigger);
    const dialog = screen.getByRole("dialog");
    await user.click(within(dialog).getByRole("gridcell", { name: /, 20 tháng 8, 2026$/ }));

    expect(hiddenValue(container, "from")).toBe("2026-08-20");
    expect(trigger).toHaveTextContent("20/08/2026");
  });

  it("moves by day and by month from the keyboard", async () => {
    const user = userEvent.setup();
    const { container } = render(
      <DateField
        label={messages["dashboard.filterFrom"]}
        name="from"
        defaultValue="2026-08-14"
      />,
    );

    await user.click(screen.getByRole("button", { name: messages["dashboard.filterFrom"] }));

    // The cursor day holds DOM focus, so arrows read naturally to a screen reader.
    await user.keyboard("{ArrowRight}{ArrowDown}");
    await user.keyboard("{Enter}");

    // 14 August + 1 day + 7 days.
    expect(hiddenValue(container, "from")).toBe("2026-08-22");

    await user.click(screen.getByRole("button", { name: messages["dashboard.filterFrom"] }));
    await user.keyboard("{PageUp}{Enter}");
    expect(hiddenValue(container, "from")).toBe("2026-07-22");
  });

  it("refuses days outside the range the field was given", async () => {
    const user = userEvent.setup();
    render(
      <DateField
        label={messages["dashboard.filterTo"]}
        name="to"
        defaultValue="2026-08-14"
        min="2026-08-10"
      />,
    );

    await user.click(screen.getByRole("button", { name: messages["dashboard.filterTo"] }));
    const dialog = screen.getByRole("dialog");

    // A DateRangeField bounds each side by the other, which is what stops an
    // operator picking a "to" that precedes the "from".
    expect(within(dialog).getByRole("gridcell", { name: /, 9 tháng 8, 2026$/ })).toBeDisabled();
    expect(within(dialog).getByRole("gridcell", { name: /, 11 tháng 8, 2026$/ })).toBeEnabled();
  });

  it("escapes the clipping of any ancestor by positioning itself in the viewport", async () => {
    const user = userEvent.setup();
    render(
      <DateField
        label={messages["dashboard.filterFrom"]}
        name="from"
        defaultValue="2026-08-14"
      />,
    );

    await user.click(screen.getByRole("button", { name: messages["dashboard.filterFrom"] }));

    // The reported symptom was a calendar with its header and last week sliced
    // off by the filter bar's rounded corners.
    const dialog = screen.getByRole("dialog");
    expect(dialog.style.position).toBe("fixed");
    expect(dialog.style.left).not.toBe("");
  });

  it("clears the day when the operator asks for no constraint", async () => {
    const user = userEvent.setup();
    const { container } = render(
      <DateField
        label={messages["dashboard.filterFrom"]}
        name="from"
        defaultValue="2026-08-14"
      />,
    );

    await user.click(screen.getByRole("button", { name: messages["dashboard.filterFrom"] }));
    await user.click(screen.getByRole("button", { name: messages["date.clear"] }));

    expect(hiddenValue(container, "from")).toBe("");
    expect(
      screen.getByRole("button", { name: messages["dashboard.filterFrom"] }),
    ).toHaveTextContent(messages["date.placeholder"]);
  });
});

/**
 * UT-UI-CONTROL-04 — a panel that opens upward stays attached to its trigger.
 *
 * Reported from the accounts screen: the role dropdown in the create form drew
 * its two options roughly 190px clear of the field, over the table above, so it
 * read as belonging to nothing. The cause is arithmetic rather than CSS.
 * `maxHeight` is the room reserved for a panel, and `max-height` caps a box
 * without ever stretching one, so a menu reserving 272px but rendering 84px,
 * placed at `trigger.top - offset - 272`, ends 188px short of its own field.
 *
 * The fix pins the bottom edge, which does not depend on how tall the contents
 * turn out to be. What follows asserts the placement numbers rather than
 * `position: fixed` alone: the older check passed throughout the bug, because a
 * panel can be fixed, on screen, and still in the wrong place.
 *
 * jsdom lays nothing out, so the trigger box and the viewport are pinned by
 * hand — the hook reads both through the same two APIs a browser provides.
 */
describe("UT-UI-CONTROL-04 anchored panel placement", () => {
  const OFFSET = 4;
  const VIEWPORT = 900;
  const TRIGGER_HEIGHT = 36;

  const originalRect = HTMLElement.prototype.getBoundingClientRect;
  const originalHeight = window.innerHeight;

  /** Puts the control about to be rendered at a known place in a known viewport. */
  function pinTriggerAt(top: number, viewportHeight = VIEWPORT): void {
    const box = {
      top,
      bottom: top + TRIGGER_HEIGHT,
      left: 160,
      right: 460,
      width: 300,
      height: TRIGGER_HEIGHT,
      x: 160,
      y: top,
    };

    HTMLElement.prototype.getBoundingClientRect = () => ({ ...box, toJSON: () => box }) as DOMRect;
    Object.defineProperty(window, "innerHeight", { configurable: true, value: viewportHeight });
  }

  afterEach(() => {
    HTMLElement.prototype.getBoundingClientRect = originalRect;
    Object.defineProperty(window, "innerHeight", { configurable: true, value: originalHeight });
  });

  it("pins the menu by its bottom edge when it opens upward, not by a guessed top", async () => {
    // 164px of room below and 700px above: the menu flips up.
    pinTriggerAt(700);
    const user = userEvent.setup();
    render(
      <SelectField label={messages["dashboard.filterProgram"]} name="program" options={PROGRAMS} />,
    );

    await user.click(screen.getByRole("combobox"));
    const listbox = screen.getByRole("listbox");

    // Measured from the foot of the viewport, the panel's bottom edge sits
    // OFFSET above the trigger's top — as true for two options as for twenty.
    expect(listbox.style.bottom).toBe(`${VIEWPORT - 700 + OFFSET}px`);

    // `top` must be absent. Setting both edges would stretch the panel to the
    // reserved height and bring the gap back from the other side.
    expect(listbox.style.top).toBe("");
  });

  it("hangs the menu directly under the trigger when it opens downward", async () => {
    // 700px of room below leaves no reason to flip.
    pinTriggerAt(160);
    const user = userEvent.setup();
    render(
      <SelectField label={messages["dashboard.filterProgram"]} name="program" options={PROGRAMS} />,
    );

    await user.click(screen.getByRole("combobox"));
    const listbox = screen.getByRole("listbox");

    expect(listbox.style.top).toBe(`${160 + TRIGGER_HEIGHT + OFFSET}px`);
    expect(listbox.style.bottom).toBe("");
  });

  it("keeps the calendar against its field when it opens upward", async () => {
    // The calendar reserves 360px, so it flips well before the dropdown does.
    pinTriggerAt(600);
    const user = userEvent.setup();
    render(
      <DateField label={messages["dashboard.filterFrom"]} name="from" defaultValue="2026-08-14" />,
    );

    await user.click(screen.getByRole("button", { name: messages["dashboard.filterFrom"] }));
    const dialog = screen.getByRole("dialog");

    expect(dialog.style.bottom).toBe(`${VIEWPORT - 600 + OFFSET}px`);
    expect(dialog.style.top).toBe("");
  });

  it("keeps a panel on screen when neither direction has room for it", async () => {
    // A viewport too short for the panel either way. Overlapping the trigger is
    // acceptable — it is still reachable — sliding off the edge is not.
    pinTriggerAt(120, 340);
    const user = userEvent.setup();
    render(
      <SelectField label={messages["dashboard.filterProgram"]} name="program" options={PROGRAMS} />,
    );

    await user.click(screen.getByRole("combobox"));
    const listbox = screen.getByRole("listbox");

    // Which edge it hangs from is the hook's call and not the point here; that
    // it fits inside the viewport at its full reserved height is.
    const maxHeight = Number.parseFloat(String(listbox.style.maxHeight));
    const pinned = listbox.style.top === "" ? listbox.style.bottom : listbox.style.top;
    const offset = Number.parseFloat(pinned);

    expect(Number.isNaN(offset)).toBe(false);
    expect(offset).toBeGreaterThanOrEqual(0);
    expect(offset + maxHeight).toBeLessThanOrEqual(340);
  });
});
