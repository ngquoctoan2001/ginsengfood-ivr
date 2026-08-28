import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { AdminActionDialog } from "@/components/admin/AdminActionDialog";
import { PermissionProvider } from "@/components/rbac/PermissionProvider";
import { RequirePermission } from "@/components/rbac/RequirePermission";
import { IDLE_ACTION_STATE, type AdminActionState } from "@/lib/admin/action-state";
import type { IvrPermission, IvrRole } from "@/lib/rbac/permissions";

function withSession(
  role: IvrRole,
  permissions: readonly IvrPermission[],
  children: React.ReactNode,
) {
  return (
    <PermissionProvider actorId="AGT-TEST-01" role={role} permissions={permissions}>
      {children}
    </PermissionProvider>
  );
}

async function noopAction(): Promise<AdminActionState> {
  return IDLE_ACTION_STATE;
}

/** UT-UI-RBAC-01 — missing permission hides the action; holding it shows it. */
describe("UT-UI-RBAC-01 permission-gated rendering", () => {
  it("hides children when the session lacks the permission", () => {
    render(
      withSession(
        "operator",
        ["IVR_QUEUE_VIEW"],
        <RequirePermission perm="IVR_QUEUE_PAUSE">
          <button type="button">Tạm dừng hàng đợi</button>
        </RequirePermission>,
      ),
    );

    expect(screen.queryByRole("button", { name: "Tạm dừng hàng đợi" })).toBeNull();
  });

  it("renders children when the session holds the permission", () => {
    render(
      withSession(
        "admin",
        ["IVR_QUEUE_VIEW", "IVR_QUEUE_PAUSE"],
        <RequirePermission perm="IVR_QUEUE_PAUSE">
          <button type="button">Tạm dừng hàng đợi</button>
        </RequirePermission>,
      ),
    );

    expect(screen.getByRole("button", { name: "Tạm dừng hàng đợi" })).toBeInTheDocument();
  });

  it("renders the fallback instead of the action when permission is missing", () => {
    render(
      withSession(
        "operator",
        ["IVR_QUEUE_VIEW"],
        <RequirePermission
          perm="IVR_RESULT_REVIEW"
          fallback={<p>Không đủ quyền</p>}
        >
          <button type="button">Duyệt kết quả</button>
        </RequirePermission>,
      ),
    );

    expect(screen.getByText("Không đủ quyền")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Duyệt kết quả" })).toBeNull();
  });

  it("hides the whole admin action dialog, trigger and form, without the permission", () => {
    render(
      withSession(
        "operator",
        ["IVR_QUEUE_VIEW", "IVR_MANUAL_RETRY", "IVR_SIM_DISABLE"],
        <AdminActionDialog
          perm="IVR_QUEUE_PAUSE"
          label="Tạm dừng hàng đợi"
          description="Chỉ chặn nhận job mới."
          action={noopAction}
        />,
      ),
    );

    expect(screen.queryByRole("button", { name: "Tạm dừng hàng đợi" })).toBeNull();
    // The reason field must not exist either — a hidden trigger with a reachable
    // form would still be a rendered admin control.
    expect(screen.queryByLabelText("Lý do")).toBeNull();
  });

  it("shows the admin action dialog with a mandatory reason field when permitted", () => {
    render(
      withSession(
        "admin",
        ["IVR_QUEUE_VIEW", "IVR_QUEUE_PAUSE"],
        <AdminActionDialog
          perm="IVR_QUEUE_PAUSE"
          label="Tạm dừng hàng đợi"
          description="Chỉ chặn nhận job mới."
          action={noopAction}
        />,
      ),
    );

    expect(screen.getByRole("button", { name: "Tạm dừng hàng đợi" })).toBeInTheDocument();

    const reason = screen.getByLabelText("Lý do");
    expect(reason).toBeRequired();
    expect(reason).toHaveAttribute("maxLength", "500");
  });

  it("throws when a permission consumer is rendered outside the provider", () => {
    expect(() =>
      render(
        <RequirePermission perm="IVR_QUEUE_VIEW">
          <span>unreachable</span>
        </RequirePermission>,
      ),
    ).toThrowError(/PermissionProvider/);
  });
});
