import type { AnchorHTMLAttributes, ButtonHTMLAttributes, ReactNode } from "react";
import Link from "next/link";

import { t } from "@/lib/i18n";

import styles from "./Button.module.css";

export type ButtonVariant = "primary" | "secondary" | "ghost" | "danger";
export type ButtonSize = "sm" | "md" | "lg";

interface ButtonBase {
  readonly variant?: ButtonVariant;
  readonly size?: ButtonSize;
  /** Fills the width of its container — dialog footers on narrow screens. */
  readonly block?: boolean;
  /** Leading glyph. Decorative: the label beside it is the accessible name. */
  readonly icon?: ReactNode;
}

export interface ButtonProps
  extends ButtonBase,
    Omit<ButtonHTMLAttributes<HTMLButtonElement>, "className"> {
  /** Swaps the label for a spinner and disables the control while a submit runs. */
  readonly pending?: boolean;
  readonly children: ReactNode;
}

function classesFor({ variant = "secondary", size = "md", block }: ButtonBase & { size?: ButtonSize }): string {
  return [styles.button, styles[variant], styles[size], block === true ? styles.block : ""]
    .filter((name) => name !== "")
    .join(" ");
}

/**
 * The console's button.
 *
 * This is a shared component rather than a Server Component: it carries no
 * directive, so a Client Component can import it and hand it an `onClick`,
 * while a server-rendered form can use the same file for its submit control.
 */
export function Button({
  variant,
  size,
  block,
  icon,
  pending,
  children,
  disabled,
  type = "button",
  ...rest
}: ButtonProps) {
  return (
    <button
      {...rest}
      type={type}
      className={classesFor({ variant, size, block })}
      disabled={disabled === true || pending === true}
      aria-busy={pending === true ? true : undefined}
    >
      {pending === true ? (
        <>
          <span className={styles.spinner} aria-hidden="true" />
          <span>{t("action.submitting")}</span>
        </>
      ) : (
        <>
          {icon === undefined ? null : <span className={styles.icon}>{icon}</span>}
          <span>{children}</span>
        </>
      )}
    </button>
  );
}

export interface LinkButtonProps
  extends ButtonBase,
    Omit<AnchorHTMLAttributes<HTMLAnchorElement>, "className" | "href"> {
  readonly href: string;
  readonly children: ReactNode;
}

/**
 * A navigation that looks like a button.
 *
 * Kept distinct from `Button` on purpose: "go somewhere" and "do something" are
 * different affordances, and a screen reader announces them differently. Reach
 * for this whenever the control is really a link — a filter reset, a back
 * action, a page in a pager.
 */
export function LinkButton({
  href,
  variant,
  size,
  block,
  icon,
  children,
  ...rest
}: LinkButtonProps) {
  return (
    <Link {...rest} href={href} className={classesFor({ variant, size, block })}>
      {icon === undefined ? null : <span className={styles.icon}>{icon}</span>}
      <span>{children}</span>
    </Link>
  );
}

export interface ButtonGroupProps {
  /** Aligns the row to the end of its container — dialog and card footers. */
  readonly align?: "start" | "end";
  /**
   * Names the set of controls. Supply it when the group is a toolbar an
   * operator navigates to, and leave it off for an incidental pair of buttons,
   * where a redundant group role is noise rather than help.
   */
  readonly label?: string;
  readonly children: ReactNode;
}

/** A row of related controls at one consistent gap. */
export function ButtonGroup({ align = "start", label, children }: ButtonGroupProps) {
  return (
    <div
      className={`${styles.group} ${align === "end" ? styles.groupEnd : ""}`}
      role={label === undefined ? undefined : "group"}
      aria-label={label}
    >
      {children}
    </div>
  );
}
