import type { ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import type { MessageKey } from "@/lib/i18n";

/** Result of an admin mutation, shaped for `useActionState`. */
export type AdminActionState =
  | { readonly status: "idle" }
  | { readonly status: "invalid"; readonly messageKey: MessageKey }
  | {
      readonly status: "success";
      readonly adminActionId: string;
      readonly correlationId: string;
    }
  | { readonly status: "error"; readonly error: ErrorEnvelopeView };

export const IDLE_ACTION_STATE: AdminActionState = { status: "idle" };

/** `AdminMutationRequest.reason` — required, 1..500 characters (OpenAPI). */
export const REASON_MAX_LENGTH = 500;

export type ReasonValidation =
  | { readonly ok: true; readonly reason: string; readonly evidenceRef?: string }
  | { readonly ok: false; readonly messageKey: MessageKey };

/**
 * Validates the reason every admin action must carry (specs/api/03 §2).
 *
 * The server validates independently; this exists so the actor gets the message
 * in Vietnamese before a round trip, not so the server can trust the input.
 */
export function validateAdminMutation(formData: FormData): ReasonValidation {
  const reason = String(formData.get("reason") ?? "").trim();
  if (reason === "") {
    return { ok: false, messageKey: "action.reasonRequired" };
  }

  if (reason.length > REASON_MAX_LENGTH) {
    return { ok: false, messageKey: "action.reasonTooLong" };
  }

  const evidenceRef = String(formData.get("evidenceRef") ?? "").trim();
  return evidenceRef === "" ? { ok: true, reason } : { ok: true, reason, evidenceRef };
}
