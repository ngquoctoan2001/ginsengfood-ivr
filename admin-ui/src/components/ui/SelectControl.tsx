"use client";

import { useEffect, useId, useRef, useState } from "react";

import { useAnchoredPosition } from "./useAnchoredPosition";
import { useHydrated } from "./useHydrated";

import styles from "./SelectControl.module.css";

export interface SelectControlOption {
  readonly value: string;
  readonly label: string;
}

export interface SelectControlProps {
  readonly name: string;
  readonly options: readonly SelectControlOption[];
  readonly defaultValue?: string;
  readonly required?: boolean;
  readonly disabled?: boolean;
  readonly invalid?: boolean;
  /** Ids of the hint and error text, for aria-describedby. */
  readonly describedBy?: string;
  /** Id of the field's visible label, so the trigger inherits its name. */
  readonly labelledBy: string;
  /** Class carrying the field's width preset. */
  readonly widthClass: string;
  /** Class the native control wears before hydration, from Field.module.css. */
  readonly nativeClass: string;
}

/**
 * The console's dropdown.
 *
 * Before hydration this renders a plain `select`: server-rendered, keyboard
 * navigable, and submitted by the surrounding GET form with no JavaScript at
 * all. After mount it swaps to a listbox we can actually style, keeping the
 * value in a hidden input so the form still posts the same field.
 *
 * The swap is gated on useHydrated rather than on a `typeof window` check:
 * that check renders differently on the client than on the server and React
 * reports it as a hydration mismatch. Declaring the two snapshots through
 * useSyncExternalStore lets React schedule the upgrade itself.
 *
 * Interaction follows the ARIA combobox pattern: DOM focus stays on the
 * trigger and the highlighted row is published through aria-activedescendant,
 * so there is never a second focus ring competing with the browser's.
 */
export function SelectControl({
  name,
  options,
  defaultValue,
  required,
  disabled,
  invalid,
  describedBy,
  labelledBy,
  widthClass,
  nativeClass,
}: SelectControlProps) {
  const enhanced = useHydrated();
  const [open, setOpen] = useState(false);
  const [value, setValue] = useState(defaultValue ?? options[0]?.value ?? "");
  const [activeIndex, setActiveIndex] = useState(0);

  const anchorRef = useRef<HTMLSpanElement>(null);
  const triggerRef = useRef<HTMLButtonElement>(null);
  const listRef = useRef<HTMLUListElement>(null);
  const typeahead = useRef({ query: "", at: 0 });

  const listId = useId();
  const selectedIndex = options.findIndex((option) => option.value === value);
  const selected = selectedIndex === -1 ? undefined : options[selectedIndex];
  // 272 = the panel's 16rem max-height plus its offset from the trigger.
  const panel = useAnchoredPosition(anchorRef, open, 272);
  const { measure } = panel;

  // Dismiss on an outside pointer press. Pointerdown rather than click so the
  // menu closes on press, matching how a native dropdown behaves.
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

  // Keep the highlighted row in view when the list is longer than its box.
  useEffect(() => {
    if (!open) {
      return;
    }

    // Optional-called: jsdom and older engines ship the element without it,
    // and keeping a row visible is a nicety, not a requirement.
    listRef.current?.children[activeIndex]?.scrollIntoView?.({ block: "nearest" });
  }, [open, activeIndex]);

  function openList(startAt = selectedIndex === -1 ? 0 : selectedIndex) {
    // Placed here rather than in an effect so the panel lands in the right spot
    // on its first paint. 272px is the panel's max height plus its offset.
    measure();
    setActiveIndex(Math.max(0, startAt));
    setOpen(true);
  }

  function commit(index: number) {
    const option = options[index];
    if (option === undefined) {
      return;
    }

    setValue(option.value);
    setOpen(false);
    triggerRef.current?.focus();
  }

  function onKeyDown(event: React.KeyboardEvent<HTMLButtonElement>) {
    switch (event.key) {
      case "ArrowDown":
      case "ArrowUp": {
        event.preventDefault();
        if (!open) {
          openList();
          return;
        }

        const step = event.key === "ArrowDown" ? 1 : -1;
        setActiveIndex((current) => wrap(current + step, options.length));
        return;
      }

      case "Home":
      case "End": {
        if (!open) {
          return;
        }

        event.preventDefault();
        setActiveIndex(event.key === "Home" ? 0 : options.length - 1);
        return;
      }

      case "Enter":
      case " ": {
        event.preventDefault();
        if (open) {
          commit(activeIndex);
        } else {
          openList();
        }

        return;
      }

      case "Escape": {
        if (open) {
          event.preventDefault();
          setOpen(false);
        }

        return;
      }

      case "Tab": {
        setOpen(false);
        return;
      }

      default: {
        // Typeahead: printable keys jump to the next option that starts with
        // what has been typed, the way a native select does. Repeated presses
        // within a second extend the query rather than restarting it.
        if (event.key.length !== 1 || event.metaKey || event.ctrlKey || event.altKey) {
          return;
        }

        const now = event.timeStamp;
        const state = typeahead.current;
        state.query = now - state.at > 1000 ? event.key : state.query + event.key;
        state.at = now;

        const match = options.findIndex((option) =>
          option.label.toLocaleLowerCase().startsWith(state.query.toLocaleLowerCase()),
        );
        if (match === -1) {
          return;
        }

        event.preventDefault();
        if (open) {
          setActiveIndex(match);
        } else {
          commit(match);
        }
      }
    }
  }

  if (!enhanced) {
    return (
      <span className={`${styles.anchor} ${widthClass}`}>
        <select
          name={name}
          className={nativeClass}
          defaultValue={defaultValue}
          required={required}
          disabled={disabled}
          aria-invalid={invalid === true ? true : undefined}
          aria-describedby={describedBy}
        >
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
        <Chevron className={styles.chevron} floating />
      </span>
    );
  }

  return (
    <span className={`${styles.anchor} ${widthClass}`} ref={anchorRef}>
      {/* The value the surrounding GET form actually submits. */}
      <input type="hidden" name={name} value={value} />

      <button
        type="button"
        ref={triggerRef}
        className={styles.trigger}
        role="combobox"
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-controls={listId}
        aria-activedescendant={open ? `${listId}-${activeIndex}` : undefined}
        aria-labelledby={labelledBy}
        aria-describedby={describedBy}
        aria-invalid={invalid === true ? true : undefined}
        aria-required={required === true ? true : undefined}
        disabled={disabled}
        onClick={() => (open ? setOpen(false) : openList())}
        onKeyDown={onKeyDown}
      >
        <span
          className={`${styles.value} ${selected?.value === "" ? styles.placeholder : ""}`}
        >
          {selected?.label ?? ""}
        </span>
        <Chevron className={styles.chevron} />
      </button>

      {open ? (
        <ul
          id={listId}
          ref={listRef}
          role="listbox"
          className={styles.popover}
          style={panel.style}
          aria-labelledby={labelledBy}
        >
          {options.map((option, index) => (
            <li
              key={option.value}
              id={`${listId}-${index}`}
              role="option"
              className={styles.option}
              aria-selected={option.value === value}
              data-active={index === activeIndex}
              onPointerEnter={() => setActiveIndex(index)}
              onClick={(event) => {
                // The field shell wraps all of this in a `label`, and a click
                // on a non-interactive descendant of a label is forwarded to
                // the labelled control — here, the trigger. Without this the
                // menu would reopen the instant an option was chosen.
                event.preventDefault();
                commit(index);
              }}
            >
              <span>{option.label}</span>
              {option.value === value ? <Tick /> : null}
            </li>
          ))}
        </ul>
      ) : null}
    </span>
  );
}

function wrap(index: number, length: number): number {
  if (length === 0) {
    return 0;
  }

  return (index + length) % length;
}

function Chevron({ className, floating }: { className: string; floating?: boolean }) {
  return (
    <svg
      className={className}
      style={
        floating === true
          ? { position: "absolute", right: "0.75rem", top: "50%", transform: "translateY(-50%)", pointerEvents: "none" }
          : undefined
      }
      viewBox="0 0 16 16"
      width="14"
      height="14"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <path d="M4 6.2 8 10.2l4-4" />
    </svg>
  );
}

/** Decorative: the row already carries aria-selected. */
function Tick() {
  return (
    <svg
      className={styles.tick}
      viewBox="0 0 16 16"
      width="13"
      height="13"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <path d="M3.6 8.4 6.4 11.2l6-6.4" />
    </svg>
  );
}
