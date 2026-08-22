import type { InputHTMLAttributes, ReactNode, TextareaHTMLAttributes } from "react";

import { t } from "@/lib/i18n";

import { DateControl } from "./DateControl";
import styles from "./Field.module.css";
import { SelectControl } from "./SelectControl";

/** Width preset, so a row of filters lines up rather than sizing to content. */
export type ControlWidth = "sm" | "md" | "lg" | "full";

const WIDTH: Readonly<Record<ControlWidth, string>> = {
  sm: styles.sizeSm,
  md: styles.sizeMd,
  lg: styles.sizeLg,
  full: styles.sizeFull,
};

interface FieldShellProps {
  readonly label: string;
  /** Explanatory line under the control — a format, a unit, a constraint. */
  readonly hint?: string;
  /** Validation message. Its presence also marks the control invalid. */
  readonly error?: string;
  readonly width?: ControlWidth;
}

/**
 * The label/control/message shell every field shares.
 *
 * The control is wrapped by the `label` rather than bound to it by id, so the
 * shell itself stays a Server Component: no useId, no hydration, and no chance
 * of a duplicate id when the same filter appears on two screens. The hint, the
 * error and the label all still need ids to be referenced, and those derive
 * from the field name — unique within a form by definition.
 *
 * Two of the fields below hand their control to a Client Component, because a
 * dropdown menu and a calendar cannot be drawn without one. Those replace the
 * native element with a button after mount, which breaks the implicit label
 * association — hence the label id.
 */
function FieldShell({
  label,
  hint,
  error,
  name,
  children,
}: FieldShellProps & { readonly name: string; readonly children: ReactNode }) {
  return (
    <label className={styles.field}>
      {/* The id is what an enhanced control points aria-labelledby at: once
          SelectControl or DateControl swaps the native element for a button,
          the implicit label association no longer reaches it. */}
      <span className={styles.label} id={`${name}-label`}>
        {label}
      </span>
      {children}
      {hint === undefined ? null : (
        <span className={styles.hint} id={`${name}-hint`}>
          {hint}
        </span>
      )}
      {error === undefined ? null : (
        <span className={styles.error} id={`${name}-error`} role="alert">
          <ErrorGlyph />
          {error}
        </span>
      )}
    </label>
  );
}

function describedBy(name: string, hint?: string, error?: string): string | undefined {
  const ids = [
    hint === undefined ? "" : `${name}-hint`,
    error === undefined ? "" : `${name}-error`,
  ]
    .filter((id) => id !== "")
    .join(" ");

  return ids === "" ? undefined : ids;
}

export interface TextFieldProps
  extends FieldShellProps,
    Omit<InputHTMLAttributes<HTMLInputElement>, "className" | "name" | "size" | "width"> {
  readonly name: string;
  /** Leading glyph inside the control — a magnifier on a search, for instance. */
  readonly adornment?: ReactNode;
  /** Renders the value in the monospace face: ids, codes, correlation ids. */
  readonly mono?: boolean;
}

/** Single-line text entry. */
export function TextField({
  label,
  hint,
  error,
  width = "md",
  name,
  adornment,
  mono,
  type = "text",
  ...rest
}: TextFieldProps) {
  const control = (
    <input
      {...rest}
      type={type}
      name={name}
      className={`${styles.control} ${WIDTH[width]} ${mono === true ? styles.mono : ""}`}
      aria-invalid={error === undefined ? undefined : true}
      aria-describedby={describedBy(name, hint, error)}
    />
  );

  return (
    <FieldShell label={label} hint={hint} error={error} name={name}>
      {adornment === undefined ? (
        control
      ) : (
        <span className={`${styles.adorned} ${WIDTH[width]}`}>
          <span className={styles.adornment}>{adornment}</span>
          {control}
        </span>
      )}
    </FieldShell>
  );
}

export interface SelectOption {
  readonly value: string;
  readonly label: string;
}

export interface SelectFieldProps extends FieldShellProps {
  readonly name: string;
  readonly options: readonly SelectOption[];
  readonly defaultValue?: string;
  readonly required?: boolean;
  readonly disabled?: boolean;
  /**
   * Prepends an option with an empty value — the shape every filter on this
   * console uses for "no constraint".
   */
  readonly includeAll?: boolean;
  readonly allLabel?: string;
}

/**
 * The console's dropdown.
 *
 * The markup the server sends is a real `select`, so the field is usable and
 * submittable before any JavaScript arrives and stays that way if none does.
 * SelectControl upgrades it to a styled listbox after mount — see that file for
 * why the swap happens in an effect rather than at first render.
 *
 * The "all" option is prepended here rather than inside the control: it is a
 * filter convention ("no constraint"), not a property of a dropdown.
 */
export function SelectField({
  label,
  hint,
  error,
  width = "md",
  name,
  options,
  includeAll,
  allLabel,
  defaultValue,
  required,
  disabled,
}: SelectFieldProps) {
  const allOptions =
    includeAll === true
      ? [{ value: "", label: allLabel ?? t("dashboard.filterAll") }, ...options]
      : options;

  return (
    <FieldShell label={label} hint={hint} error={error} name={name}>
      <SelectControl
        name={name}
        options={allOptions}
        defaultValue={defaultValue}
        required={required}
        disabled={disabled}
        invalid={error !== undefined}
        describedBy={describedBy(name, hint, error)}
        labelledBy={`${name}-label`}
        widthClass={WIDTH[width]}
        nativeClass={`${styles.control} ${styles.select}`}
      />
    </FieldShell>
  );
}

export interface DateFieldProps extends FieldShellProps {
  readonly name: string;
  /** ISO day, `yyyy-mm-dd`. */
  readonly defaultValue?: string;
  /** ISO day, inclusive: earlier days cannot be picked. */
  readonly min?: string;
  /** ISO day, inclusive: later days cannot be picked. */
  readonly max?: string;
  readonly required?: boolean;
  readonly disabled?: boolean;
}

/**
 * A day picker.
 *
 * `input[type=date]` is what the server renders, so the field works with no
 * JavaScript. DateControl upgrades it to a styled calendar after mount, and
 * keeps the keyboard model the native control would otherwise have provided:
 * arrows by day, PageUp/PageDown by month, Home/End across the week.
 */
export function DateField({
  label,
  hint,
  error,
  width = "sm",
  name,
  defaultValue,
  min,
  max,
  required,
  disabled,
}: DateFieldProps) {
  return (
    <FieldShell label={label} hint={hint} error={error} name={name}>
      <DateControl
        name={name}
        defaultValue={defaultValue}
        min={min}
        max={max}
        required={required}
        disabled={disabled}
        invalid={error !== undefined}
        describedBy={describedBy(name, hint, error)}
        labelledBy={`${name}-label`}
        widthClass={WIDTH[width]}
        nativeClass={`${styles.control} ${styles.date}`}
      />
    </FieldShell>
  );
}

export interface DateRangeFieldProps {
  readonly fromLabel: string;
  readonly toLabel: string;
  readonly fromName: string;
  readonly toName: string;
  readonly fromValue: string;
  readonly toValue: string;
}

/**
 * Two day pickers read as one control.
 *
 * Every screen that filters by time asks for the same pair, and rendering them
 * as two unrelated fields made "from" and "to" look independently optional.
 * Each side also bounds the other, so an inverted range cannot be picked in the
 * first place. The arrow between them is decorative — the two labels carry the
 * meaning.
 */
export function DateRangeField({
  fromLabel,
  toLabel,
  fromName,
  toName,
  fromValue,
  toValue,
}: DateRangeFieldProps) {
  return (
    <div className={styles.dateRange}>
      <DateField
        label={fromLabel}
        name={fromName}
        defaultValue={fromValue}
        max={toValue === "" ? undefined : toValue}
      />
      <RangeArrow />
      <DateField
        label={toLabel}
        name={toName}
        defaultValue={toValue}
        min={fromValue === "" ? undefined : fromValue}
      />
    </div>
  );
}

export interface TextareaFieldProps
  extends FieldShellProps,
    Omit<TextareaHTMLAttributes<HTMLTextAreaElement>, "className" | "name"> {
  readonly name: string;
}

/** Multi-line entry — an admin action's reason, a review note. */
export function TextareaField({
  label,
  hint,
  error,
  width = "full",
  name,
  rows = 3,
  ...rest
}: TextareaFieldProps) {
  return (
    <FieldShell label={label} hint={hint} error={error} name={name}>
      <textarea
        {...rest}
        name={name}
        rows={rows}
        className={`${styles.control} ${styles.textarea} ${WIDTH[width]}`}
        aria-invalid={error === undefined ? undefined : true}
        aria-describedby={describedBy(name, hint, error)}
      />
    </FieldShell>
  );
}

export interface CheckboxFieldProps
  extends Omit<InputHTMLAttributes<HTMLInputElement>, "className" | "name" | "type" | "width"> {
  readonly name: string;
  readonly label: string;
}

/**
 * A single boolean filter.
 *
 * Drawn as a bordered pill that fills in when checked, so a filter bar shows
 * which switches are on at a glance rather than only under inspection.
 */
export function CheckboxField({ name, label, ...rest }: CheckboxFieldProps) {
  return (
    <label className={styles.checkbox}>
      <input {...rest} type="checkbox" name={name} className={styles.checkboxInput} />
      <span>{label}</span>
    </label>
  );
}


function RangeArrow() {
  return (
    <svg
      className={styles.dateArrow}
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
      <path d="M2.6 8h10.8M9.6 4.4 13.4 8l-3.8 3.6" />
    </svg>
  );
}

function ErrorGlyph() {
  return (
    <svg
      viewBox="0 0 16 16"
      width="12"
      height="12"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      <circle cx="8" cy="8" r="6.25" />
      <path d="M8 4.9v3.6M8 11.1h.01" />
    </svg>
  );
}
