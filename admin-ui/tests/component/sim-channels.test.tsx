import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { SimChannelActions } from "@/app/(console)/dashboard/SimChannelActions";
import { PermissionProvider } from "@/components/rbac/PermissionProvider";
import vi from "@/i18n/vi.json";
import type { IvrPermission } from "@/lib/rbac/permissions";

const messages: Record<string, string> = vi;

function withPermissions(
  granted: readonly IvrPermission[],
  children: React.ReactNode,
): React.ReactElement {
  return (
    <PermissionProvider actorId="AGT-TEST-01" role="AdminIM" permissions={granted}>
      {children}
    </PermissionProvider>
  );
}

/**
 * UT-UI-SIM-05 — the two channel controls from `specs/ui/08` §3.
 *
 * Before W-0099 these operations existed in the API and in the permission
 * vocabulary but had no control anywhere in the console.
 */
describe("UT-UI-SIM-05 SIM channel controls", () => {
  it("offers disable for an enabled channel and enable for a disabled one", () => {
    const both: IvrPermission[] = ["IVR_SIM_ENABLE", "IVR_SIM_DISABLE"];
    const { rerender } = render(
      withPermissions(both, <SimChannelActions simChannelId="SIM-01" enabled busy={false} />),
    );
    expect(screen.getByRole("button", { name: messages["sim.disable"] })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: messages["sim.enable"] })).toBeNull();

    rerender(
      withPermissions(both, <SimChannelActions simChannelId="SIM-01" enabled={false} busy={false} />),
    );
    expect(screen.getByRole("button", { name: messages["sim.enable"] })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: messages["sim.disable"] })).toBeNull();
  });

  it("hides each control from an actor without its permission", () => {
    const viewOnly: IvrPermission[] = ["IVR_QUEUE_VIEW"];
    const { rerender, container } = render(
      withPermissions(viewOnly, <SimChannelActions simChannelId="SIM-01" enabled busy={false} />),
    );
    expect(container.querySelector("button")).toBeNull();

    rerender(
      withPermissions(
        viewOnly,
        <SimChannelActions simChannelId="SIM-01" enabled={false} busy={false} />,
      ),
    );
    expect(container.querySelector("button")).toBeNull();
  });

  it("shows only the control the actor holds when it holds one of the two", () => {
    const disableOnly: IvrPermission[] = ["IVR_SIM_DISABLE"];
    const { rerender, container } = render(
      withPermissions(disableOnly, <SimChannelActions simChannelId="SIM-01" enabled busy={false} />),
    );
    expect(screen.getByRole("button", { name: messages["sim.disable"] })).toBeInTheDocument();

    // The same actor may not enable, so a disabled channel offers nothing.
    rerender(
      withPermissions(
        disableOnly,
        <SimChannelActions simChannelId="SIM-02" enabled={false} busy={false} />,
      ),
    );
    expect(container.querySelector("button")).toBeNull();
  });

  it("tells the operator that disabling a busy channel is not immediate", () => {
    render(
      withPermissions(["IVR_SIM_DISABLE"], <SimChannelActions simChannelId="SIM-02" enabled busy />),
    );

    expect(screen.getByText(messages["sim.disableBusyDescription"])).toBeInTheDocument();
    expect(messages["sim.disableBusyDescription"]).toMatch(/không cắt ngang/i);
  });

  it("carries the channel id as a form field so the control works without JS", () => {
    const { container } = render(
      withPermissions(
        ["IVR_SIM_DISABLE"],
        <SimChannelActions simChannelId="SIM-07" enabled busy={false} />,
      ),
    );

    const hidden = container.querySelector<HTMLInputElement>("input[name='simChannelId']");
    expect(hidden?.value).toBe("SIM-07");
  });

  it("requires a reason like every other audited admin action", () => {
    const { container } = render(
      withPermissions(
        ["IVR_SIM_DISABLE"],
        <SimChannelActions simChannelId="SIM-01" enabled busy={false} />,
      ),
    );

    expect(container.querySelector("textarea[name='reason']")).toBeRequired();
  });
});
