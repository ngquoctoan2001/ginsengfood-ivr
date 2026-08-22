import "server-only";

import type { AdminSession } from "@/lib/auth/session";
import type { AdminUiConfig } from "@/lib/config/env";

import { callIvrApi, type IvrApiResponse } from "./client";

export type ConsoleAccountRole = "Admin" | "Operator";
export type ConsoleAccountStatus = "ACTIVE" | "DISABLED" | "DELETED";

export interface ConsoleAccount {
  readonly account_id: string;
  readonly username: string;
  readonly display_name: string;
  readonly role: ConsoleAccountRole;
  readonly status: ConsoleAccountStatus;
  readonly is_builtin: boolean;
  readonly is_locked: boolean;
  readonly locked_until?: string | null;
  readonly last_login_at?: string | null;
  readonly password_changed_at: string;
  readonly created_at: string;
  readonly updated_at: string;
  readonly deleted_at?: string | null;
  readonly version: number;
}

export interface ConsoleAccountPage {
  readonly page: number;
  readonly page_size: number;
  readonly total_count: number;
  readonly items: readonly ConsoleAccount[];
}

export interface ConsoleRole {
  readonly role: ConsoleAccountRole;
  readonly label: string;
  readonly permissions: readonly string[];
}

export interface ConsoleRoleMatrix {
  readonly roles: readonly ConsoleRole[];
}

interface AccountCallContext {
  readonly session: AdminSession;
  readonly config: AdminUiConfig;
  readonly fetchImpl?: typeof fetch;
}

/**
 * Soft-deleted accounts are excluded unless `includeDeleted` is set. They are kept so audit
 * identity survives and a username is never reassigned, not so anyone can administer them.
 */
export function listConsoleAccounts(
  context: AccountCallContext,
  page = 1,
  pageSize = 50,
  includeDeleted = false,
): Promise<IvrApiResponse<ConsoleAccountPage>> {
  return callIvrApi({
    method: "GET",
    path: `/accounts?page=${page}&page_size=${pageSize}&include_deleted=${includeDeleted}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function getConsoleAccount(
  context: AccountCallContext,
  accountId: string,
): Promise<IvrApiResponse<ConsoleAccount>> {
  return callIvrApi({
    method: "GET",
    path: `/accounts/${encodeURIComponent(accountId)}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function getMyConsoleAccount(
  context: AccountCallContext,
): Promise<IvrApiResponse<ConsoleAccount>> {
  return callIvrApi({
    method: "GET",
    path: "/accounts/me",
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function getConsoleRoleMatrix(
  context: AccountCallContext,
): Promise<IvrApiResponse<ConsoleRoleMatrix>> {
  return callIvrApi({
    method: "GET",
    path: "/account-roles",
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function createConsoleAccount(
  context: AccountCallContext,
  request: {
    username: string;
    display_name: string;
    role: ConsoleAccountRole;
    password: string;
    reason: string;
  },
): Promise<IvrApiResponse<ConsoleAccount>> {
  return callIvrApi({
    method: "POST",
    path: "/accounts",
    session: context.session,
    config: context.config,
    body: request,
    fetchImpl: context.fetchImpl,
  });
}

export function updateConsoleAccount(
  context: AccountCallContext,
  accountId: string,
  request: {
    display_name?: string;
    role?: ConsoleAccountRole;
    status?: ConsoleAccountStatus;
    version: number;
    reason: string;
  },
): Promise<IvrApiResponse<ConsoleAccount>> {
  return callIvrApi({
    method: "PATCH",
    path: `/accounts/${encodeURIComponent(accountId)}`,
    session: context.session,
    config: context.config,
    body: request,
    fetchImpl: context.fetchImpl,
  });
}

export function resetConsolePassword(
  context: AccountCallContext,
  accountId: string,
  request: { new_password: string; version: number; reason: string },
): Promise<IvrApiResponse<ConsoleAccount>> {
  return callIvrApi({
    method: "POST",
    path: `/accounts/${encodeURIComponent(accountId)}:reset-password`,
    session: context.session,
    config: context.config,
    body: request,
    fetchImpl: context.fetchImpl,
  });
}

export function deleteConsoleAccount(
  context: AccountCallContext,
  accountId: string,
  request: { version: number; reason: string },
): Promise<IvrApiResponse<ConsoleAccount>> {
  return callIvrApi({
    method: "DELETE",
    path: `/accounts/${encodeURIComponent(accountId)}`,
    session: context.session,
    config: context.config,
    body: request,
    fetchImpl: context.fetchImpl,
  });
}
