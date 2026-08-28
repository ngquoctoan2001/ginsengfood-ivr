"use client";

import { hasPermission } from "@/lib/rbac/permissions";

import type { IvrPermission, IvrRole } from "@/lib/rbac/permissions";

import { createContext, useContext, useMemo, type ReactNode } from "react";



export interface PermissionContextValue {
  readonly actorId: string;
  readonly role: IvrRole;
  readonly permissions: readonly IvrPermission[];
  readonly can: (permission: IvrPermission) => boolean;
}

const PermissionContext = createContext<PermissionContextValue | null>(null);

export interface PermissionProviderProps {
  readonly actorId: string;
  readonly role: IvrRole;
  readonly permissions: readonly IvrPermission[];
  readonly children: ReactNode;
}

/**
 * Publishes the session's permission set to Client Components.
 *
 * This exists to hide controls the actor cannot use — nothing more. Ivr.Api
 * re-evaluates permissions server-side on every call (DF-01), so this context
 * is a usability layer, never an authorization decision.
 */
export function PermissionProvider({
  actorId,
  role,
  permissions,
  children,
}: PermissionProviderProps) {
  const value = useMemo<PermissionContextValue>(
    () => ({
      actorId,
      role,
      permissions,
      can: (permission) => hasPermission(permissions, permission),
    }),
    [actorId, role, permissions],
  );

  return <PermissionContext value={value}>{children}</PermissionContext>;
}

export function usePermissions(): PermissionContextValue {
  const value = useContext(PermissionContext);
  if (value === null) {
    throw new Error("usePermissions must be used inside a PermissionProvider.");
  }

  return value;
}
