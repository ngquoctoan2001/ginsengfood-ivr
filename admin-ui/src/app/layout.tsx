import type { Metadata } from "next";
import { Montserrat } from "next/font/google";

import { t } from "@/lib/i18n";

import "./globals.css";

/**
 * Montserrat, self-hosted by the build.
 *
 * The `vietnamese` subset is not optional here: the document is `lang="vi"` and
 * the whole message catalogue is diacritic-heavy, so without it every accented
 * character would fall through to the system stack and the console would render
 * in two typefaces at once.
 *
 * The variable axis carries 100–900 in a single file, which covers the four
 * weights the UI actually uses (400 body, 500 controls, 600 headings, 700 brand)
 * for the cost of one. `display: swap` keeps text readable while it loads.
 */
const montserrat = Montserrat({
  subsets: ["latin", "vietnamese"],
  display: "swap",
  variable: "--font-montserrat",
});

export const metadata: Metadata = {
  title: t("app.title"),
  description: t("app.description"),
  robots: { index: false, follow: false },
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="vi" className={montserrat.variable}>
      <body>{children}</body>
    </html>
  );
}
