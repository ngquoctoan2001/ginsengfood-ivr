import type { Metadata } from "next";

import { t } from "@/lib/i18n";

import "./globals.css";

export const metadata: Metadata = {
  title: t("app.title"),
  description: t("app.description"),
  robots: { index: false, follow: false },
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="vi">
      <body>{children}</body>
    </html>
  );
}
