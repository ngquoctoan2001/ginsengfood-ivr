import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { ErrorAlert } from "@/components/feedback/ErrorAlert";
import { parseErrorEnvelope } from "@/lib/api/errors";
import { IVR_ERROR_CODES } from "@/lib/api/types";
import vi from "@/i18n/vi.json";

/** UT-UI-ERR-02 — an API-06 envelope renders its localized message plus the raw code. */
describe("UT-UI-ERR-02 error envelope rendering", () => {
  it("renders the localized message, the stable code and the correlation id", () => {
    const error = parseErrorEnvelope(
      {
        error: {
          code: "IVR_FORBIDDEN_CALLER",
          message: "Caller is not allowed.",
          correlationId: "ui-1a2b-3c4d-5e6f-7a8b-9c0d-1e2f",
          details: { permission: "IVR_QUEUE_PAUSE" },
        },
      },
      { status: 403, correlationId: "fallback", message: "fallback" },
    );

    render(<ErrorAlert error={error.toEnvelope()} />);

    expect(screen.getByRole("alert")).toBeInTheDocument();
    expect(screen.getByTestId("error-code")).toHaveTextContent("IVR_FORBIDDEN_CALLER");
    expect(screen.getByTestId("error-correlation-id")).toHaveTextContent(
      "ui-1a2b-3c4d-5e6f-7a8b-9c0d-1e2f",
    );
    expect(
      screen.getByText("Bạn không có quyền thực hiện thao tác này."),
    ).toBeInTheDocument();
    expect(screen.getByText(/IVR_QUEUE_PAUSE/)).toBeInTheDocument();
  });

  it("degrades an unknown code to IVR_INTERNAL_ERROR instead of rendering it", () => {
    const error = parseErrorEnvelope(
      { error: { code: "SOMETHING_NEW", message: "?", correlationId: "ui-0000" } },
      { status: 500, correlationId: "ui-0000", message: "fallback" },
    );

    render(<ErrorAlert error={error.toEnvelope()} />);

    expect(screen.getByTestId("error-code")).toHaveTextContent("IVR_INTERNAL_ERROR");
    expect(screen.queryByText("SOMETHING_NEW")).toBeNull();
  });

  it("falls back to transport status and correlation id when the body is not an envelope", () => {
    const error = parseErrorEnvelope("<html>502</html>", {
      status: 502,
      correlationId: "ui-abcd",
      message: "Ivr.Api returned HTTP 502.",
    });

    render(<ErrorAlert error={error.toEnvelope()} />);

    expect(error.status).toBe(502);
    expect(screen.getByTestId("error-code")).toHaveTextContent("IVR_INTERNAL_ERROR");
    expect(screen.getByTestId("error-correlation-id")).toHaveTextContent("ui-abcd");
  });

  it("has a Vietnamese message for every code in the API-06 catalogue", () => {
    const messages: Record<string, string> = vi;
    for (const code of IVR_ERROR_CODES) {
      expect(messages[`error.${code}`], `missing translation for ${code}`).toBeTruthy();
    }
  });
});
