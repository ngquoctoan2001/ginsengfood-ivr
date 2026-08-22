import { cleanup, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";

import { PermissionProvider } from "@/components/rbac/PermissionProvider";
import { ConsoleNav } from "@/components/shell/ConsoleNav";
import { SidebarAccount } from "@/components/shell/SidebarAccount";
import type { IvrPermission, IvrRole } from "@/lib/rbac/permissions";

// Mutable so the account card's `aria-current` can be exercised on the route it points at.
const route = vi.hoisted(() => ({ pathname: "/dashboard" }));
vi.mock("next/navigation", () => ({ usePathname: () => route.pathname }));

afterEach(() => {
  route.pathname = "/dashboard";
});

/** Decision B: exactly these four, and nothing that implies an admin surface. */
const OPERATOR_PERMISSIONS: readonly IvrPermission[] = [
  "IVR_ACCOUNT_SELF_VIEW",
  "IVR_QUEUE_VIEW",
  "IVR_SIM_DISABLE",
  "IVR_MANUAL_RETRY",
];

const ADMIN_PERMISSIONS: readonly IvrPermission[] = [
  ...OPERATOR_PERMISSIONS,
  "IVR_QUEUE_PAUSE",
  "IVR_QUEUE_RESUME",
  "IVR_SIM_ENABLE",
  "IVR_RESULT_REVIEW",
  "IVR_ACCOUNT_VIEW",
  "IVR_ACCOUNT_MANAGE",
  "IVR_ACCOUNT_PASSWORD_RESET",
];

const ADMIN_ONLY_HREFS = [
  "/reports",
  "/review",
  "/config",
  "/integration",
  "/seed",
  "/accounts",
  "/roles",
];

/**
 * The rail is rendered the way AppShell renders it — links plus the account card in the footer
 * slot — because `/profile` is now reached by clicking that card, not by a tenth link.
 */
function renderNav(role: IvrRole, permissions: readonly IvrPermission[]) {
  render(
    <PermissionProvider actorId="test.user" role={role} permissions={permissions}>
      <ConsoleNav
        account={<SidebarAccount actorId="test.user" displayName="Quản trị hệ thống" />}
      />
    </PermissionProvider>,
  );
}

function hrefs(): string[] {
  return screen
    .getAllByRole("link")
    .map((link) => link.getAttribute("href") ?? "");
}

/**
 * UT-UI-NAV-07 — the sidebar's admin entries are gated on the role, not on a permission that
 * merely happens to be admin-only today.
 *
 * They used to be gated on `IVR_ACCOUNT_VIEW`, which produced the right result by coincidence:
 * grant that permission to some future read-only support role and reports, config and seed would
 * appear in their sidebar with nothing in the code stating why. The last test below is the one
 * that catches that, by handing an Operator the account-view permission and asserting the admin
 * entries stay hidden.
 */
describe("UT-UI-NAV-07 console navigation", () => {
  it("shows an Operator only the routes Decision B grants", () => {
    renderNav("Operator", OPERATOR_PERMISSIONS);

    expect(hrefs()).toEqual(["/dashboard", "/calls", "/profile"]);
  });

  it("shows an Admin every console route", () => {
    renderNav("Admin", ADMIN_PERMISSIONS);

    const shown = hrefs();
    expect(shown).toContain("/dashboard");
    expect(shown).toContain("/calls");
    expect(shown).toContain("/profile");
    for (const href of ADMIN_ONLY_HREFS) {
      expect(shown).toContain(href);
    }
  });

  it("keeps admin entries hidden from a non-Admin that holds IVR_ACCOUNT_VIEW", () => {
    renderNav("Operator", [...OPERATOR_PERMISSIONS, "IVR_ACCOUNT_VIEW"]);

    const shown = hrefs();
    for (const href of ADMIN_ONLY_HREFS) {
      expect(shown).not.toContain(href);
    }
  });

  it("emits only list items inside the navigation list", () => {
    renderNav("Admin", ADMIN_PERMISSIONS);

    const list = screen.getByRole("list");
    for (const child of Array.from(list.children)) {
      expect(child.tagName).toBe("LI");
    }
  });

  /*
   * The account card carries the profile route now, so the permission that used to gate a link
   * in the list has to gate the card. Dropping the link entirely would be the easy failure: an
   * operator who may not open the page would still see a control that answers 403.
   */
  it("names the actor on the card and points it at the profile route", () => {
    renderNav("Operator", OPERATOR_PERMISSIONS);

    const card = screen.getByRole("link", { name: /Quản trị hệ thống/ });
    expect(card).toHaveAttribute("href", "/profile");
    expect(card).toHaveTextContent("test.user");
    // The purpose is stated after the visible name rather than replacing it (WCAG 2.5.3).
    expect(card).toHaveAccessibleName(/Hồ sơ của tôi/);
  });

  it("keeps the actor readable but unlinked without IVR_ACCOUNT_SELF_VIEW", () => {
    renderNav("Operator", ["IVR_QUEUE_VIEW"]);

    expect(hrefs()).not.toContain("/profile");
    expect(screen.getByText("Quản trị hệ thống")).toBeInTheDocument();
    expect(screen.getByText("test.user")).toBeInTheDocument();
  });

  it("marks the card as the current page only while the profile route is open", () => {
    renderNav("Operator", OPERATOR_PERMISSIONS);
    expect(screen.getByRole("link", { name: /Quản trị hệ thống/ })).not.toHaveAttribute(
      "aria-current",
    );

    cleanup();
    route.pathname = "/profile";
    renderNav("Operator", OPERATOR_PERMISSIONS);
    expect(screen.getByRole("link", { name: /Quản trị hệ thống/ })).toHaveAttribute(
      "aria-current",
      "page",
    );
  });
});
