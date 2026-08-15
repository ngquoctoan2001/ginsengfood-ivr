import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { MaskedPhone } from "@/components/privacy/MaskedPhone";
import { isMaskedPhone, looksLikeRawPhone } from "@/lib/privacy/mask";

/** UT-UI-PII-04 — the console never paints a raw phone number (D-05). */
describe("UT-UI-PII-04 masked phone rendering", () => {
  it.each([
    "0912341234",
    "84912341234",
    "+84912341234",
    "0912 341 234",
    "091-234-1234",
    "+84 912 341 234",
    "(084) 912341234",
    "0912.341.234",
  ])("redacts the raw number %s instead of rendering it", (raw) => {
    render(<MaskedPhone value={raw} />);

    expect(screen.getByText("[đã ẩn]")).toBeInTheDocument();
    expect(screen.queryByText(raw)).toBeNull();
    expect(document.body.textContent).not.toContain("341234");
  });

  it("renders an already-masked value unchanged", () => {
    render(<MaskedPhone value="0912***234" />);

    expect(screen.getByLabelText("Số điện thoại đã che")).toHaveTextContent("0912***234");
  });

  it("redacts a value that is masked but still carries a full number", () => {
    render(<MaskedPhone value="0912341234 ***" />);

    expect(screen.getByText("[đã ẩn]")).toBeInTheDocument();
  });

  it("renders a placeholder for absent values", () => {
    const { container } = render(<MaskedPhone value={null} />);
    expect(container.textContent).toBe("—");
  });

  it("treats an unmasked non-phone string as unsafe rather than printing it", () => {
    render(<MaskedPhone value="Nguyễn Văn A" />);
    expect(screen.getByText("[đã ẩn]")).toBeInTheDocument();
  });

  it("recognises separator-obfuscated numbers that the server-side pattern misses", () => {
    expect(looksLikeRawPhone("0912 341 234")).toBe(true);
    expect(isMaskedPhone("0912 341 234")).toBe(false);
    // A price or an order code must not be mistaken for a phone number.
    expect(looksLikeRawPhone("560000")).toBe(false);
    expect(looksLikeRawPhone("ORD-2026-0001")).toBe(false);
  });
});
