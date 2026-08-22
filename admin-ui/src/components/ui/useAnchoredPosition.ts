"use client";

import { useCallback, useEffect, useState, type CSSProperties, type RefObject } from "react";

/** Gap between the control and the panel hanging off it. */
const OFFSET = 4;

/** Keeps a panel this far from the viewport edge when it would otherwise spill. */
const EDGE = 8;

/** Below this a panel is unusable, so it overlaps the trigger rather than shrink further. */
const MIN_HEIGHT = 160;

/** How much roomier "above" must be before the panel flips up rather than down. */
const FLIP_MARGIN = 48;

export interface AnchoredPosition {
  /** Inline style to spread onto the panel. */
  readonly style: CSSProperties;
  /** Call when opening the panel, to place it before the first paint. */
  readonly measure: () => void;
}

/**
 * Positions a popover panel against its trigger, in viewport coordinates.
 *
 * `position: fixed` rather than `absolute` is the whole point. An absolutely
 * positioned panel is still laid out inside its nearest positioned ancestor and
 * is clipped by any ancestor that sets `overflow`, and this console has several
 * that legitimately do: the filter bar and the card both clip to round their
 * corners, and a data table scrolls horizontally. A dropdown opening inside one
 * of those came out sliced in half. Fixed positioning takes the panel out of
 * that flow entirely, so no ancestor can crop it and no z-index arms race is
 * needed.
 *
 * The cost of leaving the flow is that the panel no longer follows its trigger,
 * so the position is re-measured on scroll — capture phase, because the scroll
 * may happen in a container rather than on the window — and on resize.
 *
 * Measuring is deliberately not done in an effect: the caller measures inside
 * the event that opens the panel, and the listeners registered here only
 * re-measure afterwards. Setting state straight from an effect body would be a
 * cascading render, and React 19 rightly lints it.
 */
export function useAnchoredPosition(
  anchorRef: RefObject<HTMLElement | null>,
  open: boolean,
  /** Roughly how tall the panel is, used to decide whether it opens upward. */
  estimatedHeight: number,
  /** Fixed panel width, for a panel that does not match its trigger. */
  panelWidth?: number,
): AnchoredPosition {
  const [style, setStyle] = useState<CSSProperties>({ position: "fixed", visibility: "hidden" });

  const measure = useCallback(() => {
    const box = anchorRef.current?.getBoundingClientRect();
    if (box === undefined) {
      return;
    }

    const width = panelWidth ?? box.width;
    const spaceBelow = window.innerHeight - box.bottom - OFFSET - EDGE;
    const spaceAbove = box.top - OFFSET - EDGE;

    /*
     * Flip upward only when there is meaningfully more room up there. The
     * margin matters: without it, a control sitting mid-screen with 329px below
     * and 331px above opened upward on a two-pixel difference, which reads as
     * random when the same filter bar opens downward on the next screen. Down
     * is the habit, so down wins ties and near-ties.
     */
    const openUp = spaceBelow < estimatedHeight && spaceAbove > spaceBelow + FLIP_MARGIN;

    /*
     * Cap the panel to the room it actually has, then place it against that
     * capped height. Deciding the direction without also bounding the height is
     * what put the calendar at top: -21 — there was more room above than below,
     * but not enough above for all six weeks, so it opened upward straight off
     * the top of the window. A panel that has to shrink scrolls; one that
     * overflows the viewport is simply unreachable.
     */
    const room = Math.max(MIN_HEIGHT, openUp ? spaceAbove : spaceBelow);
    const maxHeight = Math.min(estimatedHeight, room);
    const top = openUp ? Math.max(EDGE, box.top - OFFSET - maxHeight) : box.bottom + OFFSET;
    const left = Math.max(EDGE, Math.min(box.left, window.innerWidth - width - EDGE));

    setStyle({
      position: "fixed",
      top,
      left,
      maxHeight,
      overflowY: "auto",
      minWidth: box.width,
      maxWidth: window.innerWidth - left - EDGE,
      ...(panelWidth === undefined ? {} : { width: panelWidth }),
    });
  }, [anchorRef, estimatedHeight, panelWidth]);

  useEffect(() => {
    if (!open) {
      return;
    }

    window.addEventListener("scroll", measure, true);
    window.addEventListener("resize", measure);
    return () => {
      window.removeEventListener("scroll", measure, true);
      window.removeEventListener("resize", measure);
    };
  }, [open, measure]);

  return { style, measure };
}
