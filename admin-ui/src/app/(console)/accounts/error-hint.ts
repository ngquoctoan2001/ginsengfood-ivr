import { t } from "@/lib/i18n";

/**
 * Plain-language explanation for the account error codes an administrator can actually provoke.
 *
 * `IVR_ACCOUNT_CONFLICT` is the one that needs it. A successful sign-in updates `last_login_at`
 * and therefore bumps the account's optimistic-concurrency version, so an administrator whose
 * edit form was loaded before the target user signed in gets a 409 on save without anyone having
 * made a conflicting change. Keeping the check is correct — it is what stops two administrators
 * silently overwriting each other — so the fix is to say what happened instead of showing a bare
 * code that reads like a fault.
 */
export function accountErrorHint(code: string | null): string | null {
  switch (code) {
    case "IVR_ACCOUNT_CONFLICT":
      return t("accounts.hint.conflict");
    case "IVR_ACCOUNT_POLICY_VIOLATION":
      return t("accounts.hint.policyViolation");
    default:
      return null;
  }
}
