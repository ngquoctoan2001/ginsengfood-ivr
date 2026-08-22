import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { CallDetailActions } from "@/app/(console)/calls/[ivrCallJobId]/CallDetailActions";
import { PermissionProvider } from "@/components/rbac/PermissionProvider";
import type { IvrReviewItemDetail, IvrTechnicalExceptionDetail } from "@/lib/api/types";
import vi from "@/i18n/vi.json";
import type { IvrPermission, IvrRole } from "@/lib/rbac/permissions";

const TECHNICAL_EXCEPTIONS: IvrTechnicalExceptionDetail[] = [
  {
    technical_exception_id: "TECH-1",
    ivr_call_attempt_id: "ATTEMPT-1",
    exception_type: "MOCK_ADAPTER_FAULT",
    customer_attempt_counted: false,
    technical_retry_allowed: true,
    technical_retry_count: 0,
    created_at: "2026-08-15T00:00:00Z",
  },
];

const REVIEW_ITEMS: IvrReviewItemDetail[] = [
  {
    review_item_id: "REVIEW-1",
    source_type: "IVR_CALL_RESULT",
    source_id: "RESULT-1",
    reason: "verify final result evidence",
    status: "OPEN",
    created_at: "2026-08-15T00:00:00Z",
  },
];

function renderActions(role: IvrRole, permissions: readonly IvrPermission[]) {
  return render(
    <PermissionProvider actorId="AGT-TEST-01" role={role} permissions={permissions}>
      <CallDetailActions
        technicalExceptions={TECHNICAL_EXCEPTIONS}
        reviewItems={REVIEW_ITEMS}
      />
    </PermissionProvider>,
  );
}

const ADMIN_PERMISSIONS: IvrPermission[] = [
  "IVR_QUEUE_VIEW",
  "IVR_QUEUE_PAUSE",
  "IVR_QUEUE_RESUME",
  "IVR_SIM_ENABLE",
  "IVR_SIM_DISABLE",
  "IVR_RESULT_REVIEW",
  "IVR_MANUAL_RETRY",
];

/** UT-UI-NOORDER-03 — the console offers no order-state control (D-02). */
describe("UT-UI-NOORDER-03 no order transition control", () => {
  it("offers exactly the two IVR admin actions, even to the fullest role", () => {
    renderActions("Admin", ADMIN_PERMISSIONS);

    const triggers = screen
      .getAllByRole("button")
      .map((button) => button.textContent ?? "")
      // The dialogs also contain their own cancel/confirm buttons.
      .filter((label) => label !== "Huỷ" && label !== "Xác nhận");

    expect(triggers).toHaveLength(2);
    expect(triggers[0]).toContain("Yêu cầu gọi lại kỹ thuật");
    expect(triggers[1]).toContain("Ghi kết luận duyệt");
  });

  it("renders no control that could transition an order", () => {
    renderActions("Admin", ADMIN_PERMISSIONS);

    const forbidden =
      /(xác nhận|huỷ|hủy)\s+đơn|confirm\s+order|cancel\s+order|force|reset\s+attempt/i;
    for (const element of [...screen.getAllByRole("button"), ...screen.queryAllByRole("link")]) {
      expect(element.textContent ?? "").not.toMatch(forbidden);
    }
  });

  it("states in the shipped Vietnamese copy that order state belongs to Order Core", () => {
    const messages: Record<string, string> = vi;
    expect(messages["detail.noOrderControl"]).toContain("Order Core");
    expect(messages["detail.resultAdvisory"]).toContain("tham khảo");
    // No message may offer an order transition as a console action.
    for (const [key, message] of Object.entries(messages)) {
      if (key.startsWith("detail.") || key.startsWith("calls.")) {
        expect(message, key).not.toMatch(/(xác nhận|huỷ|hủy)\s+đơn hàng/i);
      }
    }
  });
});

/** UT-UI-REVIEW-04 — review needs a reason and disappears without the permission. */
describe("UT-UI-REVIEW-04 result review action", () => {
  it("requires both a reason and a resolution before it can be submitted", () => {
    renderActions("Admin", ["IVR_QUEUE_VIEW", "IVR_RESULT_REVIEW"]);

    expect(
      screen.getByRole("button", { name: /Ghi kết luận duyệt · REVIEW-1/ }),
    ).toBeInTheDocument();

    const reason = screen.getByLabelText("Lý do");
    expect(reason).toBeRequired();
    expect(reason).toHaveAttribute("maxLength", "500");

    const resolution = screen.getByLabelText("Kết luận");
    expect(resolution).toBeRequired();

    // The audited target travels with the form, not with a client-side guess.
    const hidden = document.querySelector<HTMLInputElement>('input[name="reviewItemId"]');
    expect(hidden?.value).toBe("REVIEW-1");
    expect(screen.getByText(/ghi audit kèm actor, quyền, lý do và correlation id/)).toBeInTheDocument();
  });

  it("is hidden entirely when the session lacks IVR_RESULT_REVIEW", () => {
    renderActions("Operator", ["IVR_QUEUE_VIEW", "IVR_MANUAL_RETRY"]);

    expect(screen.queryByText(/Ghi kết luận duyệt/)).toBeNull();
    expect(screen.queryByLabelText("Kết luận")).toBeNull();
    // The retry action the role does hold is still offered.
    expect(screen.getByRole("button", { name: /Yêu cầu gọi lại kỹ thuật/ })).toBeInTheDocument();
  });

  it("renders nothing at all for a viewer with neither permission", () => {
    const { container } = renderActions("Operator", ["IVR_QUEUE_VIEW"]);
    expect(container).toBeEmptyDOMElement();
  });

  it("skips a technical exception that the API marked as not retryable", () => {
    render(
      <PermissionProvider actorId="AGT-TEST-01" role="Admin" permissions={ADMIN_PERMISSIONS}>
        <CallDetailActions
          technicalExceptions={[
            { ...TECHNICAL_EXCEPTIONS[0], technical_retry_allowed: false },
          ]}
          reviewItems={[]}
        />
      </PermissionProvider>,
    );

    expect(screen.queryByText(/Yêu cầu gọi lại kỹ thuật/)).toBeNull();
  });
});
