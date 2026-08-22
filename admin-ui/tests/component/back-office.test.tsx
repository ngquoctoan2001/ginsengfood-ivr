
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { DependencyBadge } from "@/components/data/DependencyBadge";
import vi from "@/i18n/vi.json";
import { IVR_PERMISSIONS } from "@/lib/rbac/permissions";

const messages: Record<string, string> = vi;

/** UT-UI-SCRIPT-01 — approval state is visible and KEY_9 stays NOT_ENABLED. */
describe("UT-UI-SCRIPT-01 script configuration", () => {
  it("ships copy that marks an unapproved version and never offers an approve action", () => {
    expect(messages["config.notApprovedBadge"]).toBeTruthy();
    expect(messages["config.approvedBadge"]).toBeTruthy();
    expect(messages["config.templateInvalid"]).toBeTruthy();

    // W-0109 opened the lifecycle on this screen, so "there is no button" is no
    // longer the invariant. What replaced it is narrower and is what this now
    // pins: the copy has to name the two controls that make an approval mean
    // something — a reason on every action, and a second person for the second
    // half. A notice that only said "you can approve here" would be a screen
    // that lost the point of the gate it just opened.
    expect(messages["config.lifecycleNotice"]).toMatch(/lý do/i);
    expect(messages["config.lifecycleNotice"]).toMatch(/nhật ký/i);
    expect(messages["config.lifecycleNotice"]).toMatch(/hai tài khoản khác nhau/i);
    expect(messages["config.readOnlyNotice"]).toBeUndefined();

    // Each approval half is a separate label, because each is a separate
    // permission and a separate signature. One combined "approve" would let a
    // single press stand for both halves of the production gate.
    for (const required of [
      "config.approveMock",
      "config.approveLab",
      "config.approveContent",
      "config.approvePrivacyLegal",
      "config.submitReview",
      "config.retire",
    ]) {
      expect(messages[required], required).toBeTruthy();
    }

    // Still forbidden: a single catch-all approve action.
    expect(messages["config.approve"]).toBeUndefined();

    // The Privacy/Legal copy must say out loud that it cannot be the same person.
    expect(messages["config.approvePrivacyLegalDescription"]).toMatch(/tài khoản khác|đã duyệt nội dung/i);
  });

  it("states that KEY_9 is NOT_ENABLED and cannot be turned on from the UI", () => {
    expect(messages["config.key9Notice"]).toContain("NOT_ENABLED");
    expect(messages["config.key9Notice"]).toMatch(/không thể bật/i);
    expect(messages["config.key9Notice"]).toContain("AS-07");
  });

  it("keeps the OD-V1-15 production lock explicit", () => {
    expect(messages["config.od15Locked"]).toContain("ProductionTargetV1FieldsApproved=NO");
    expect(messages["config.od15Locked"]).toContain("OD-V1-15");
  });
});

/** UT-UI-HEALTH-02 — a fail-closed dependency is labelled, and unprobed is not green. */
describe("UT-UI-HEALTH-02 dependency health badge", () => {
  it.each(["DOWN", "READY_503"] as const)("labels %s as fail-closed", (state) => {
    render(<DependencyBadge state={state} observed />);

    expect(screen.getByTestId(`state-${state}`)).toBeInTheDocument();
    expect(screen.getByTestId("fail-closed-badge")).toHaveTextContent("fail-closed");
    expect(screen.getByText("IVR quan sát trực tiếp")).toBeInTheDocument();
  });

  it("does not label a healthy dependency fail-closed", () => {
    render(<DependencyBadge state="UP" observed />);

    expect(screen.getByTestId("state-UP")).toBeInTheDocument();
    expect(screen.queryByTestId("fail-closed-badge")).toBeNull();
  });

  it("marks an unprobed dependency as not observed rather than healthy", () => {
    render(<DependencyBadge state="NOT_WIRED" observed={false} />);

    expect(screen.getByTestId("state-NOT_WIRED")).toBeInTheDocument();
    expect(screen.queryByTestId("fail-closed-badge")).toBeNull();
    expect(screen.getByText("Chưa có thăm dò")).toBeInTheDocument();
  });

  it("warns that no probing exists yet, so nothing may be read as verified", () => {
    expect(messages["integration.probingUnavailable"]).toContain("W-0040");
    expect(messages["integration.probingUnavailable"]).toMatch(/KHÔNG phải bằng chứng/);
    // And the screen offers no override.
    expect(messages["integration.subtitle"]).toMatch(/không có nút override/i);
  });
});

/** UT-UI-SEED-PROD-03 — seed/mock and the mode toggle are locked. */
describe("UT-UI-SEED-PROD-03 seed and mock guards", () => {
  it("states the production lock and the REAL-mode lock separately", () => {
    expect(messages["seed.prodLocked"]).toMatch(/bị khoá hoàn toàn/i);
    expect(messages["seed.realLocked"]).toContain("DT-01");
    expect(messages["seed.realLocked"]).toContain("DF-03");
    expect(messages["seed.realLocked"]).toMatch(/không thể đổi từ giao diện/i);
  });

  it("says plainly that no seed write path exists from the console", () => {
    // Wording note (W-0102): "đường" is avoided here on purpose. The PII gate
    // scans docs/evidence/ with deliberately blunt literal patterns (W-0076), so
    // console prose that reaches an evidence capture must not use the address
    // vocabulary even in its "path" sense.
    expect(messages["seed.loaderUnavailable"]).toMatch(/không mở lối ghi dữ liệu/i);
    expect(messages["seed.title"]).toContain("non-prod");
  });
});

/** UT-UI-ROLE-04 — the matrix is fixed to the two API-owned roles. */
describe("UT-UI-ROLE-04 role and permission matrix", () => {
  it("states that permissions are mapped from one of two fixed roles", () => {
    expect(messages["roles.notManagedHere"]).toContain("hai vai trò");
    expect(messages["roles.notManagedHere"]).toContain("Ivr.Api");
    expect(messages["roles.subtitle"]).toContain("hai vai trò");
  });

  it("maps every permission to the screen that uses it", () => {
    // `Record<IvrPermission, MessageKey>` already makes a missing row a compile
    // error. What that cannot catch is a row whose text no longer names a real
    // screen, so the mapping is read and checked against the routes that exist.
    // W-0039 moved the prose into the catalogue; the check follows it there rather
    // than being dropped, because the thing it guards did not change.
    const mapping = Object.fromEntries(
      IVR_PERMISSIONS.map((permission) => [
        permission,
        messages[`roles.screen.${permission}` as keyof typeof messages],
      ]),
    ) as Record<string, string>;

    for (const permission of IVR_PERMISSIONS) {
      expect(mapping[permission], `${permission} has no screen mapping`).toBeTruthy();
    }

    // Operators can read queue/calls; reports are admin-only under Decision B.
    expect(mapping.IVR_QUEUE_VIEW).toContain(messages["nav.dashboard"]);
    expect(mapping.IVR_ACCOUNT_VIEW).toContain("tài khoản");
    // The SIM controls now exist; the mapping must not still promise them later.
    expect(mapping.IVR_SIM_ENABLE).not.toMatch(/sau|sắp|chưa có/i);
    expect(mapping.IVR_SIM_DISABLE).not.toMatch(/sau|sắp|chưa có/i);
  });

  // OD-V1-20 approved 2026-08-22: Admin holds the runtime-gate permission. The label is the
  // only place the console tells an operator what that permission actually reaches, so it must
  // name the gates rather than point at a decision that is now closed.
  it("names the gates the runtime-gate permission reaches", () => {
    const label = messages["roles.screen.IVR_RUNTIME_GATE_ADMIN"];
    expect(label).not.toContain("OD-V1-20");
    expect(label).not.toMatch(/chờ owner|chưa cấp/i);
    expect(label).toMatch(/kill switch/i);
    expect(label).toMatch(/khách thật/i);
    // Still rendered for any permission a role stops holding later.
    expect(messages["roles.ungranted"]).toBeTruthy();
  });
});
