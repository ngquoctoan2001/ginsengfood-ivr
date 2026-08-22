"use client";

import { useEffect, useId, useRef, useState } from "react";

import { useAnchoredPosition } from "./useAnchoredPosition";
import { useHydrated } from "./useHydrated";

import { LOCALE, t } from "@/lib/i18n";

import styles from "./DateControl.module.css";

export interface DateControlProps {
  readonly name: string;
  readonly defaultValue?: string;
  /** ISO day, inclusive. Days before it cannot be picked. */
  readonly min?: string;
  /** ISO day, inclusive. Days after it cannot be picked. */
  readonly max?: string;
  readonly required?: boolean;
  readonly disabled?: boolean;
  readonly invalid?: boolean;
  readonly describedBy?: string;
  readonly labelledBy: string;
  readonly widthClass: string;
  readonly nativeClass: string;
}

/*
 * Everything below works in UTC on purpose. The wire format is a calendar day
 * with no zone, and running it through the browser's local zone is how a date
 * filter ends up one day out for anyone west of UTC.
 */

const DAY_MS = 86_400_000;

const monthFormatter = new Intl.DateTimeFormat(LOCALE, {
  timeZone: "UTC",
  month: "long",
  year: "numeric",
});

const dayFormatter = new Intl.DateTimeFormat(LOCALE, {
  timeZone: "UTC",
  day: "2-digit",
  month: "2-digit",
  year: "numeric",
});

const weekdayFormatter = new Intl.DateTimeFormat(LOCALE, {
  timeZone: "UTC",
  weekday: "short",
});

const fullDateFormatter = new Intl.DateTimeFormat(LOCALE, {
  timeZone: "UTC",
  weekday: "long",
  day: "numeric",
  month: "long",
  year: "numeric",
});

function parseIso(value: string | undefined): Date | undefined {
  if (value === undefined || !/^\d{4}-\d{2}-\d{2}$/.test(value)) {
    return undefined;
  }

  const parsed = new Date(`${value}T00:00:00Z`);
  return Number.isNaN(parsed.getTime()) ? undefined : parsed;
}

function toIso(date: Date): string {
  return date.toISOString().slice(0, 10);
}

function addDays(date: Date, days: number): Date {
  return new Date(date.getTime() + days * DAY_MS);
}

function addMonths(date: Date, months: number): Date {
  const shifted = new Date(date.getTime());
  const target = shifted.getUTCMonth() + months;
  shifted.setUTCDate(1);
  shifted.setUTCMonth(target);
  // Clamp: 31 January plus one month is the last day of February, not 3 March.
  const lastDay = new Date(Date.UTC(shifted.getUTCFullYear(), shifted.getUTCMonth() + 1, 0)).getUTCDate();
  shifted.setUTCDate(Math.min(date.getUTCDate(), lastDay));
  return shifted;
}

/** Monday-first, which is how a Vietnamese calendar is read. */
function startOfGrid(month: Date): Date {
  const first = new Date(Date.UTC(month.getUTCFullYear(), month.getUTCMonth(), 1));
  const weekday = (first.getUTCDay() + 6) % 7;
  return addDays(first, -weekday);
}

function todayUtc(): Date {
  const now = new Date();
  return new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate()));
}

/**
 * The console's day picker.
 *
 * Before hydration this is `input[type=date]` — server-rendered, submitted by
 * the surrounding GET form, and fully usable with JavaScript off. After mount
 * it becomes a calendar we control.
 *
 * That swap is worth more here than anywhere else in the kit: the native date
 * popup is the one control a stylesheet cannot reach, so a filter row mixing it
 * with the rest of the console always looked like two products bolted
 * together. What the swap must not cost is the behaviour the native control
 * gave away for free, so the calendar keeps the full keyboard model — arrows by
 * day, PageUp/PageDown by month, Home/End across the week — and every day is a
 * real button rather than a div with a click handler.
 */
export function DateControl({
  name,
  defaultValue,
  min,
  max,
  required,
  disabled,
  invalid,
  describedBy,
  labelledBy,
  widthClass,
  nativeClass,
}: DateControlProps) {
  const enhanced = useHydrated();
  const [open, setOpen] = useState(false);
  const [value, setValue] = useState(defaultValue ?? "");
  const [cursor, setCursor] = useState<string>(defaultValue ?? "");

  const anchorRef = useRef<HTMLSpanElement>(null);
  const triggerRef = useRef<HTMLButtonElement>(null);
  const gridRef = useRef<HTMLDivElement>(null);
  const dialogId = useId();
  // 280 wide, 360 tall — measured, not guessed: six week rows plus the header
  // and the footer come to 357px.
  const panel = useAnchoredPosition(anchorRef, open, 360, 280);
  const { measure } = panel;

  useEffect(() => {
    if (!open) {
      return;
    }

    function onPointerDown(event: PointerEvent) {
      if (!anchorRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    }

    document.addEventListener("pointerdown", onPointerDown);
    return () => document.removeEventListener("pointerdown", onPointerDown);
  }, [open]);

  // Roving tabindex: the cursor day is the only tabbable cell, and it takes DOM
  // focus so arrow keys read naturally to a screen reader.
  useEffect(() => {
    if (!open) {
      return;
    }

    const cell = gridRef.current?.querySelector<HTMLButtonElement>('[data-cursor="true"]');
    cell?.focus();
  }, [open, cursor]);

  const selectedDate = parseIso(value);
  const minDate = parseIso(min);
  const maxDate = parseIso(max);

  function openCalendar() {
    // Measured in the handler so the calendar is placed before its first paint.
    measure();
    setCursor(value === "" ? toIso(clamp(todayUtc(), minDate, maxDate)) : value);
    setOpen(true);
  }

  function close(restoreFocus = true) {
    setOpen(false);
    if (restoreFocus) {
      triggerRef.current?.focus();
    }
  }

  function commit(day: Date) {
    setValue(toIso(day));
    setCursor(toIso(day));
    close();
  }

  function moveCursor(days: number, months = 0) {
    const current = parseIso(cursor) ?? todayUtc();
    const next = months === 0 ? addDays(current, days) : addMonths(current, months);
    setCursor(toIso(clamp(next, minDate, maxDate)));
  }

  function onGridKeyDown(event: React.KeyboardEvent<HTMLDivElement>) {
    switch (event.key) {
      case "ArrowLeft":
        event.preventDefault();
        moveCursor(-1);
        return;
      case "ArrowRight":
        event.preventDefault();
        moveCursor(1);
        return;
      case "ArrowUp":
        event.preventDefault();
        moveCursor(-7);
        return;
      case "ArrowDown":
        event.preventDefault();
        moveCursor(7);
        return;
      case "PageUp":
        event.preventDefault();
        moveCursor(0, -1);
        return;
      case "PageDown":
        event.preventDefault();
        moveCursor(0, 1);
        return;
      case "Home": {
        event.preventDefault();
        const current = parseIso(cursor) ?? todayUtc();
        moveCursor(-((current.getUTCDay() + 6) % 7));
        return;
      }
      case "End": {
        event.preventDefault();
        const current = parseIso(cursor) ?? todayUtc();
        moveCursor(6 - ((current.getUTCDay() + 6) % 7));
        return;
      }
      case "Escape":
        event.preventDefault();
        close();
        return;
      default:
        return;
    }
  }

  if (!enhanced) {
    return (
      <input
        type="date"
        name={name}
        className={`${nativeClass} ${widthClass}`}
        defaultValue={defaultValue}
        min={min}
        max={max}
        required={required}
        disabled={disabled}
        aria-invalid={invalid === true ? true : undefined}
        aria-describedby={describedBy}
      />
    );
  }

  const cursorDate = parseIso(cursor) ?? todayUtc();
  const gridStart = startOfGrid(cursorDate);
  const today = toIso(todayUtc());
  // Six weeks always: a month that needs five and a month that needs six must
  // not change the popover's height as the operator pages through them.
  const days = Array.from({ length: 42 }, (_, index) => addDays(gridStart, index));
  const weeks = Array.from({ length: 6 }, (_, week) => days.slice(week * 7, week * 7 + 7));

  return (
    <span className={`${styles.anchor} ${widthClass}`} ref={anchorRef}>
      <input type="hidden" name={name} value={value} />

      <button
        type="button"
        ref={triggerRef}
        className={styles.trigger}
        aria-haspopup="dialog"
        aria-expanded={open}
        aria-labelledby={labelledBy}
        aria-describedby={describedBy}
        aria-invalid={invalid === true ? true : undefined}
        aria-required={required === true ? true : undefined}
        disabled={disabled}
        onClick={() => (open ? close() : openCalendar())}
      >
        <span className={selectedDate === undefined ? styles.placeholder : undefined}>
          {selectedDate === undefined ? t("date.placeholder") : dayFormatter.format(selectedDate)}
        </span>
        <CalendarGlyph />
      </button>

      {open ? (
        <div
          id={dialogId}
          role="dialog"
          aria-modal="false"
          aria-labelledby={`${dialogId}-month`}
          className={styles.calendar}
          style={panel.style}
        >
          <div className={styles.head}>
            <button
              type="button"
              className={styles.nav}
              onClick={() => moveCursor(0, -1)}
              aria-label={t("date.previousMonth")}
            >
              <Arrow direction="left" />
            </button>
            <span className={styles.month} id={`${dialogId}-month`} aria-live="polite">
              {monthFormatter.format(cursorDate)}
            </span>
            <button
              type="button"
              className={styles.nav}
              onClick={() => moveCursor(0, 1)}
              aria-label={t("date.nextMonth")}
            >
              <Arrow direction="right" />
            </button>
          </div>

          <div className={styles.weekdays} aria-hidden="true">
            {days.slice(0, 7).map((day) => (
              <span key={day.toISOString()} className={styles.weekday}>
                {weekdayFormatter.format(day)}
              </span>
            ))}
          </div>

          {/*
            A real grid: rows of gridcells, which is the structure a screen
            reader needs to announce "week 3, Thursday" rather than reading 42
            loose buttons. `aria-selected` belongs on the gridcell — a plain
            button does not support it — so each day carries role="gridcell"
            and stays a `button` element for the click and disabled behaviour
            that comes with it.
          */}
          <div className={styles.grid} role="grid" onKeyDown={onGridKeyDown} ref={gridRef}>
            {weeks.map((week) => (
              <div key={toIso(week[0])} className={styles.week} role="row">
                {week.map((day) => {
                  const iso = toIso(day);
                  const outside = day.getUTCMonth() !== cursorDate.getUTCMonth();
                  const blocked =
                    (minDate !== undefined && day < minDate) ||
                    (maxDate !== undefined && day > maxDate);

                  return (
                    <button
                      key={iso}
                      type="button"
                      role="gridcell"
                      className={[
                        styles.day,
                        outside ? styles.outside : "",
                        iso === today ? styles.today : "",
                        iso === value ? styles.selected : "",
                      ]
                        .filter((className) => className !== "")
                        .join(" ")}
                      data-cursor={iso === cursor}
                      tabIndex={iso === cursor ? 0 : -1}
                      disabled={blocked}
                      aria-selected={iso === value}
                      aria-current={iso === today ? "date" : undefined}
                      aria-label={fullDateFormatter.format(day)}
                      onClick={() => commit(day)}
                    >
                      {day.getUTCDate()}
                    </button>
                  );
                })}
              </div>
            ))}
          </div>

          <div className={styles.foot}>
            <button
              type="button"
              className={styles.action}
              onClick={() => commit(clamp(todayUtc(), minDate, maxDate))}
            >
              {t("date.today")}
            </button>
            <button
              type="button"
              className={styles.action}
              onClick={() => {
                setValue("");
                close();
              }}
            >
              {t("date.clear")}
            </button>
          </div>
        </div>
      ) : null}
    </span>
  );
}

function clamp(day: Date, min: Date | undefined, max: Date | undefined): Date {
  if (min !== undefined && day < min) {
    return min;
  }

  if (max !== undefined && day > max) {
    return max;
  }

  return day;
}

function CalendarGlyph() {
  return (
    <svg
      className={styles.icon}
      viewBox="0 0 16 16"
      width="14"
      height="14"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <rect x="2.4" y="3.4" width="11.2" height="10.2" rx="1.6" />
      <path d="M2.4 6.6h11.2M5.6 2.2v2.4M10.4 2.2v2.4" />
    </svg>
  );
}

function Arrow({ direction }: { readonly direction: "left" | "right" }) {
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
      <path d={direction === "left" ? "M9.8 3.6 5.4 8l4.4 4.4" : "M6.2 3.6 10.6 8l-4.4 4.4"} />
    </svg>
  );
}
