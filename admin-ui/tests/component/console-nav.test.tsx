import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { PermissionProvider } from "@/components/rbac/PermissionProvider";
import { ConsoleNav } from "@/components/shell/ConsoleNav";
import type { IvrPermission, IvrRole } from "@/lib/rbac/permissions";

vi.mock("next/navigation", () => ({
  usePathname: () => "/dashboard",
}));

const ALL_PERMISSIONS: readonly IvrPermission[] = [
  "IVR_QUEUE_VIEW",
  "IVR_FLAG_READ",
  "IVR_RESULT_REVIEW",
  "IVR_DEV_TOOLING",
  "IVR_SCRIPT_EDIT",
  "IVR_SCRIPT_REVIEW",
  "IVR_SCRIPT_APPROVE_MOCK",
  "IVR_SCRIPT_APPROVE_LAB",
  "IVR_SCRIPT_APPROVE_CONTENT",
  "IVR_SCRIPT_APPROVE_PRIVACY_LEGAL",
  "IVR_SCRIPT_RETIRE",
  "IVR_RUNTIME_GATE_ADMIN",
  "IVR_CALL_TERMINATE",
  "IVR_SIM_DISABLE",
  "IVR_SIM_ENABLE",
  "IVR_MANUAL_RETRY",
  "IVR_QUEUE_PAUSE",
  "IVR_QUEUE_RESUME",
];

/** Every screen the console ships, in sidebar order. */
const EVERY_SCREEN = [
  "Tổng quan",
  "Nhật ký cuộc gọi",
  "Báo cáo & phân tích",
  "Chờ duyệt",
  "Cấu hình kịch bản",
  "Trạng thái tích hợp",
  "Cổng vận hành",
  "Seed / Mock",
];

function withSession(
  role: IvrRole,
  permissions: readonly IvrPermission[],
) {
  return (
    <PermissionProvider actorId="AGT-NAV-01" role={role} permissions={permissions}>
      <ConsoleNav />
    </PermissionProvider>
  );
}

/**
 * UT-UI-NAV-01 — W-0190.
 *
 * The regression: six of the eight entries were gated on `role === "admin"`, and `role` came from
 * the tier the SHELL was rendered with rather than from anything the viewer held. The console
 * layout renders at `read`, so the role was permanently `operator` and Reports, Review, Config,
 * Integration, Runtime gates and Seed never appeared — while all six rendered perfectly well when
 * their URL was typed by hand. A nav that hides a screen the viewer can open is not a permission
 * boundary; it is a broken menu.
 *
 * The `role` case is asserted explicitly: an operator who holds the permissions sees the screens.
 * That is the property the old rule got wrong, so it is the one worth pinning down.
 */
describe("UT-UI-NAV-01 console navigation", () => {
  it("offers every screen to a viewer holding every permission", () => {
    render(withSession("admin", ALL_PERMISSIONS));

    for (const label of EVERY_SCREEN) {
      expect(screen.getByRole("link", { name: label })).toBeInTheDocument();
    }
    expect(screen.getAllByRole("link")).toHaveLength(EVERY_SCREEN.length);
  });

  it("does not depend on the role the shell happened to render with", () => {
    render(withSession("operator", ALL_PERMISSIONS));

    expect(screen.getAllByRole("link")).toHaveLength(EVERY_SCREEN.length);
  });

  it("offers only the screens the viewer's permissions reach", () => {
    render(withSession("operator", ["IVR_QUEUE_VIEW"]));

    // Dashboard, call log and reports are the three read screens IVR_QUEUE_VIEW covers.
    expect(screen.getByRole("link", { name: "Tổng quan" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Nhật ký cuộc gọi" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Báo cáo & phân tích" })).toBeInTheDocument();

    expect(screen.queryByRole("link", { name: "Chờ duyệt" })).toBeNull();
    expect(screen.queryByRole("link", { name: "Cấu hình kịch bản" })).toBeNull();
    expect(screen.queryByRole("link", { name: "Trạng thái tích hợp" })).toBeNull();
    expect(screen.queryByRole("link", { name: "Cổng vận hành" })).toBeNull();
    expect(screen.queryByRole("link", { name: "Seed / Mock" })).toBeNull();
  });

  it("shows nothing at all to a viewer holding no permission", () => {
    render(withSession("operator", []));

    expect(screen.queryAllByRole("link")).toHaveLength(0);
  });
});
