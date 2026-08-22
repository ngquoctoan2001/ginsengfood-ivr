import { NextResponse } from "next/server";

import { exportAnalytics } from "@/lib/analytics/client";
import { toCsv } from "@/lib/analytics/format";
import { IvrApiError } from "@/lib/api/errors";
import { ANALYTICS_DIMENSIONS, type AnalyticsDimension } from "@/lib/api/types";
import { requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { t } from "@/lib/i18n";

export const dynamic = "force-dynamic";

const MIN_REASON_LENGTH = 8;

/**
 * Streams the server's sanitized aggregate extract as CSV.
 *
 * The handler adds nothing to the payload: it renders exactly the columns and
 * rows the analytics API returned, having already applied k-anonymity and
 * written the audit entry. A value the server withheld cannot reappear here.
 *
 * The 422 from a re-identifying slice is passed through rather than swallowed —
 * a refused export must read as refused, not as an empty file.
 */
export async function GET(request: Request): Promise<NextResponse> {
  const session = await requireSession();
  if (session.role !== "Admin") {
    return problem(403, "IVR_FORBIDDEN_CALLER", t("reports.export.forbidden"));
  }

  const url = new URL(request.url);
  const reason = (url.searchParams.get("reason") ?? "").trim();
  if (reason.length < MIN_REASON_LENGTH) {
    return problem(
      400,
      "IVR_MALFORMED_REQUEST",
      `Lý do xuất dữ liệu là bắt buộc và cần tối thiểu ${MIN_REASON_LENGTH} ký tự.`,
    );
  }

  const dimension = asDimension(url.searchParams.get("dimension"));

  try {
    const { data } = await exportAnalytics({ session, config: readConfig() }, dimension, reason, {
      program: url.searchParams.get("program") ?? undefined,
      resultType: url.searchParams.get("result_type") ?? undefined,
      scriptVariant: url.searchParams.get("script_variant") ?? undefined,
      bucket: url.searchParams.get("bucket") ?? undefined,
      from: url.searchParams.get("from") ?? undefined,
      to: url.searchParams.get("to") ?? undefined,
    });

    return new NextResponse(toCsv(data.columns, data.rows), {
      status: 200,
      headers: {
        "Content-Type": "text/csv; charset=utf-8",
        "Content-Disposition": `attachment; filename="ivr-analytics-${dimension.toLowerCase()}.csv"`,
        // The extract is audited and aggregate, but it is still reporting data:
        // no shared cache should keep a copy.
        "Cache-Control": "no-store",
        "X-Ivr-Audit-Ref": data.audit_ref,
        "X-Ivr-Suppressed-Rows": String(data.suppressed_row_count),
      },
    });
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    const envelope = cause.toEnvelope();
    return problem(cause.status, envelope.code, envelope.message);
  }
}

function asDimension(value: string | null): AnalyticsDimension {
  return value !== null && ANALYTICS_DIMENSIONS.includes(value as AnalyticsDimension)
    ? (value as AnalyticsDimension)
    : "RESULT_TYPE";
}

function problem(status: number, code: string, message: string): NextResponse {
  return NextResponse.json({ error: { code, message } }, { status, headers: { "Cache-Control": "no-store" } });
}
