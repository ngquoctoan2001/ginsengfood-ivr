import { readFileSync } from "node:fs";
import { resolve } from "node:path";

import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { DependencyBadge } from "@/components/data/DependencyBadge";
import vi from "@/i18n/vi.json";
import { MOCK_DIRECTORY } from "@/lib/auth/directory";
import { IVR_PERMISSIONS } from "@/lib/rbac/permissions";

const messages: Record<string, string> = vi;

/** UT-UI-SCRIPT-01 — approval state is visible and KEY_9 stays NOT_ENABLED. */
describe("UT-UI-SCRIPT-01 script configuration", () => {
  it("ships copy that marks an unapproved version and never offers an approve action", () => {
    expect(messages["config.notApprovedBadge"]).toBeTruthy();
    expect(messages["config.approvedBadge"]).toBeTruthy();
    expect(messages["config.templateInvalid"]).toBeTruthy();

    // The screen states plainly that approval is not a console action.
    expect(messages["config.readOnlyNotice"]).toMatch(/không có nút/i);
    expect(messages["config.readOnlyNotice"]).toContain("OD-V1-15");

    // No lifecycle action label exists for this screen, so none can be wired to
    // a control. `config.approvedBadge` and `config.colApprovals` describe state
    // and a column, not actions.
    for (const forbidden of [
      "config.approve",
      "config.approveMock",
      "config.approveLab",
      "config.approveContent",
      "config.approvePrivacyLegal",
      "config.submitForReview",
      "config.retire",
      "config.createDraft",
    ]) {
      expect(messages[forbidden], forbidden).toBeUndefined();
    }
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

/** UT-UI-ROLE-04 — the matrix is reference only; assignment lives in Permission Core. */
describe("UT-UI-ROLE-04 role and permission matrix", () => {
  it("states that permissions are not managed in this console", () => {
    expect(messages["roles.notManagedHere"]).toContain("Permission Core");
    expect(messages["roles.notManagedHere"]).toMatch(/không có nút gán hay thu hồi/i);
    expect(messages["roles.subtitle"]).toContain("DF-01");
  });

  it("maps every permission to the screen that uses it", () => {
    // `Record<IvrPermission, string>` already makes a missing row a compile
    // error. What that cannot catch is a row whose text no longer names a real
    // screen, so the mapping is read and checked against the routes that exist.
    // jsdom leaves `import.meta.url` without a file scheme, so the path is
    // resolved from the Vitest project root instead.
    const source = readFileSync(
      resolve(process.cwd(), "src/app/(console)/roles/page.tsx"),
      "utf8",
    );
    const mapping = Object.fromEntries(
      [...source.matchAll(/^\s{2}(IVR_[A-Z_]+):\s*\n?\s*"([^"]*)"/gm)].map((match) => [
        match[1],
        match[2],
      ]),
    );

    for (const permission of IVR_PERMISSIONS) {
      expect(mapping[permission], `${permission} has no screen mapping`).toBeTruthy();
    }

    // The view permission gates every read screen in the nav, reporting included.
    expect(mapping.IVR_QUEUE_VIEW).toContain(messages["nav.reports"]);
    expect(mapping.IVR_QUEUE_VIEW).toContain(messages["nav.dashboard"]);
    // The SIM controls now exist; the mapping must not still promise them later.
    expect(mapping.IVR_SIM_ENABLE).not.toMatch(/sau|sắp|chưa có/i);
    expect(mapping.IVR_SIM_DISABLE).not.toMatch(/sau|sắp|chưa có/i);
  });

  it("shows the runtime-gate permission as held by nobody", () => {
    const holders = MOCK_DIRECTORY.filter((entry) =>
      (entry.permissions as readonly string[]).includes("IVR_RUNTIME_GATE_ADMIN"),
    );
    expect(holders).toHaveLength(0);
    expect(messages["roles.ungranted"]).toBeTruthy();
  });
});
