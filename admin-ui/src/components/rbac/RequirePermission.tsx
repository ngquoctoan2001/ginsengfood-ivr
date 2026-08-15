"use client";

import type { ReactNode } from "react";

import type { IvrPermission } from "@/lib/rbac/permissions";

import { usePermissions } from "./PermissionProvider";

export interface RequirePermissionProps {
  readonly perm: IvrPermission;
  readonly children: ReactNode;
  /** Rendered instead of `children` when the actor lacks `perm`. Defaults to nothing. */
  readonly fallback?: ReactNode;
}

/**
 * Renders `children` only when the session carries `perm`.
 *
 * Hiding a control is a courtesy, not a control: the corresponding Ivr.Api
 * endpoint answers 403 `IVR_FORBIDDEN_CALLER` to an actor without the
 * permission regardless of what this component decided to paint.
 */
export function RequirePermission({ perm, children, fallback }: RequirePermissionProps) {
  const { can } = usePermissions();
  return <>{can(perm) ? children : (fallback ?? null)}</>;
}
