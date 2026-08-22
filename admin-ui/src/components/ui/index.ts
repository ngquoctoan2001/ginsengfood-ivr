/**
 * The IVR console UI kit.
 *
 * One import path for everything a screen is built from. The rules the kit
 * enforces, and why they are worth having in one place:
 *
 * - Plain CSS Modules, no UI dependency. The console shipped without one and
 *   adding one now would be a supply-chain decision, not a styling decision.
 * - Server Components by default. Nothing here uses a hook or a browser API, so
 *   a screen can render these without shipping JavaScript for them. Files with
 *   no "use client" directive work in both worlds, which is what lets
 *   AdminActionDialog hand `Button` an onClick.
 * - Colour never carries meaning alone (globals.css rule 4). Every tone in the
 *   kit arrives with a glyph, a word, or both.
 * - Gold is decorative here and nowhere structural: control boundaries take
 *   --ivr-border-strong, because gold on white is 2.26:1 (rule 2).
 */

export { Button, ButtonGroup, LinkButton } from "./Button";
export type { ButtonProps, ButtonSize, ButtonVariant, LinkButtonProps } from "./Button";

export { Breadcrumb } from "./Breadcrumb";
export type { BreadcrumbProps, Crumb } from "./Breadcrumb";

export { Callout, CalloutStack } from "./Callout";
export type { CalloutProps, CalloutTone } from "./Callout";

export { Card, CardStack } from "./Card";
export type { CardProps } from "./Card";

export { Chip, ChipList } from "./Chip";
export type { ChipListProps, ChipProps, ChipTone } from "./Chip";

export { DataTable } from "./DataTable";
export type { Column, ColumnVariant, DataTableProps } from "./DataTable";

export { DescriptionList } from "./DescriptionList";
export type { DescriptionItem, DescriptionListProps } from "./DescriptionList";

export {
  CheckboxField,
  DateField,
  DateRangeField,
  SelectField,
  TextField,
  TextareaField,
} from "./Field";
export type {
  CheckboxFieldProps,
  ControlWidth,
  DateFieldProps,
  DateRangeFieldProps,
  SelectFieldProps,
  SelectOption,
  TextFieldProps,
  TextareaFieldProps,
} from "./Field";

export { FilterBar, countActiveFilters } from "./FilterBar";
export type { FilterBarProps } from "./FilterBar";

export { Meter } from "./Meter";
export type { MeterProps } from "./Meter";

export { PageHeader } from "./PageHeader";
export type { PageHeaderMeta, PageHeaderProps } from "./PageHeader";

export { Pagination } from "./Pagination";
export type { PaginationProps } from "./Pagination";

export { SegmentedControl } from "./SegmentedControl";
export type { SegmentOption, SegmentedControlProps } from "./SegmentedControl";

export { Timeline, TimelineItem } from "./Timeline";
export type { TimelineItemProps, TimelineProps } from "./Timeline";
