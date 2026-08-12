import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "IVR Admin — MOCK mode",
  description: "GinsengFood IVR administration bootstrap",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="vi">
      <body>{children}</body>
    </html>
  );
}
