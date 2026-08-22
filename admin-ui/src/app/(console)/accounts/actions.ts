"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";

import {
  createConsoleAccount,
  deleteConsoleAccount,
  resetConsolePassword,
  updateConsoleAccount,
  type ConsoleAccountRole,
  type ConsoleAccountStatus,
} from "@/lib/api/accounts";
import { IvrApiError } from "@/lib/api/errors";
import { requireAdmin } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";

const ROLES: ReadonlySet<string> = new Set(["Admin", "Operator"]);
const STATUSES: ReadonlySet<string> = new Set(["ACTIVE", "DISABLED"]);

export async function createAccountAction(formData: FormData): Promise<void> {
  const session = await requireAdmin();
  const role = required(formData, "role");
  let destination = "/accounts?error=IVR_ACCOUNT_POLICY_VIOLATION";

  if (ROLES.has(role)) {
    try {
      await createConsoleAccount(
        { session, config: readConfig() },
        {
          username: required(formData, "username"),
          display_name: required(formData, "display_name"),
          role: role as ConsoleAccountRole,
          password: required(formData, "password"),
          reason: required(formData, "reason"),
        },
      );
      revalidatePath("/accounts");
      destination = "/accounts?result=created";
    } catch (cause) {
      destination = accountErrorDestination("/accounts", cause);
    }
  }

  redirect(destination);
}

export async function updateAccountAction(formData: FormData): Promise<void> {
  const session = await requireAdmin();
  const accountId = required(formData, "account_id");
  const role = required(formData, "role");
  const status = required(formData, "status");
  let destination = accountDetail(accountId, "error=IVR_ACCOUNT_POLICY_VIOLATION");

  if (ROLES.has(role) && STATUSES.has(status)) {
    try {
      await updateConsoleAccount(
        { session, config: readConfig() },
        accountId,
        {
          display_name: required(formData, "display_name"),
          role: role as ConsoleAccountRole,
          status: status as ConsoleAccountStatus,
          version: version(formData),
          reason: required(formData, "reason"),
        },
      );
      revalidatePath("/accounts");
      revalidatePath(`/accounts/${accountId}`);
      destination = accountDetail(accountId, "result=updated");
    } catch (cause) {
      destination = accountErrorDestination(`/accounts/${accountId}`, cause);
    }
  }

  redirect(destination);
}

export async function resetPasswordAction(formData: FormData): Promise<void> {
  const session = await requireAdmin();
  const accountId = required(formData, "account_id");
  let destination: string;
  try {
    await resetConsolePassword(
      { session, config: readConfig() },
      accountId,
      {
        new_password: required(formData, "new_password"),
        version: version(formData),
        reason: required(formData, "reason"),
      },
    );
    revalidatePath("/accounts");
    revalidatePath(`/accounts/${accountId}`);
    destination = accountDetail(accountId, "result=password-reset");
  } catch (cause) {
    destination = accountErrorDestination(`/accounts/${accountId}`, cause);
  }

  redirect(destination);
}

export async function deleteAccountAction(formData: FormData): Promise<void> {
  const session = await requireAdmin();
  const accountId = required(formData, "account_id");
  let destination: string;
  try {
    await deleteConsoleAccount(
      { session, config: readConfig() },
      accountId,
      { version: version(formData), reason: required(formData, "reason") },
    );
    revalidatePath("/accounts");
    destination = "/accounts?result=deleted";
  } catch (cause) {
    destination = accountErrorDestination(`/accounts/${accountId}`, cause);
  }

  redirect(destination);
}

function required(formData: FormData, name: string): string {
  return String(formData.get(name) ?? "").trim();
}

function version(formData: FormData): number {
  const parsed = Number.parseInt(required(formData, "version"), 10);
  return Number.isSafeInteger(parsed) && parsed > 0 ? parsed : 0;
}

function accountDetail(accountId: string, query: string): string {
  return `/accounts/${encodeURIComponent(accountId)}?${query}`;
}

function accountErrorDestination(base: string, cause: unknown): string {
  const code = cause instanceof IvrApiError ? cause.code : "IVR_INTERNAL_ERROR";
  return `${base}?error=${encodeURIComponent(code)}`;
}
