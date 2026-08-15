import "server-only";

import { callIvrApi, type IvrApiResponse } from "@/lib/api/client";
import type {
  AnalyticsDimension,
  IvrAnalyticsBreakdown,
  IvrAnalyticsExport,
  IvrAnalyticsSummary,
  IvrAnalyticsTrend,
} from "@/lib/api/types";
import type { AdminSession } from "@/lib/auth/session";
import type { AdminUiConfig } from "@/lib/config/env";

interface AnalyticsCallContext {
  readonly session: AdminSession;
  readonly config: AdminUiConfig;
  readonly fetchImpl?: typeof fetch;
}

/**
 * The reporting filter, as the console holds it. `from`/`to` are calendar days
 * from a date input; the API takes instants, so the conversion happens here in
 * one place rather than in every screen.
 */
export interface AnalyticsQuery {
  readonly program?: string;
  readonly resultType?: string;
  readonly scriptVariant?: string;
  readonly bucket?: string;
  readonly from?: string;
  readonly to?: string;
}

export function getAnalyticsSummary(
  context: AnalyticsCallContext,
  query: AnalyticsQuery = {},
): Promise<IvrApiResponse<IvrAnalyticsSummary>> {
  return callIvrApi<IvrAnalyticsSummary>({
    method: "GET",
    path: `/analytics/summary${buildQuery(query)}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function getAnalyticsTrend(
  context: AnalyticsCallContext,
  query: AnalyticsQuery = {},
): Promise<IvrApiResponse<IvrAnalyticsTrend>> {
  return callIvrApi<IvrAnalyticsTrend>({
    method: "GET",
    path: `/analytics/trend${buildQuery(query)}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function getAnalyticsBreakdown(
  context: AnalyticsCallContext,
  dimension: AnalyticsDimension,
  query: AnalyticsQuery = {},
): Promise<IvrApiResponse<IvrAnalyticsBreakdown>> {
  return callIvrApi<IvrAnalyticsBreakdown>({
    method: "GET",
    path: `/analytics/breakdown${buildQuery(query, { dimension })}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

/**
 * An export is a read the server audits, so the reason is mandatory here too —
 * the console never issues one without it.
 */
export function exportAnalytics(
  context: AnalyticsCallContext,
  dimension: AnalyticsDimension,
  reason: string,
  query: AnalyticsQuery = {},
): Promise<IvrApiResponse<IvrAnalyticsExport>> {
  return callIvrApi<IvrAnalyticsExport>({
    method: "GET",
    path: `/analytics/export${buildQuery(query, { dimension, reason })}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

function buildQuery(
  query: AnalyticsQuery,
  extra: Readonly<Record<string, string | undefined>> = {},
): string {
  const search = new URLSearchParams();
  const entries: Readonly<Record<string, string | undefined>> = {
    ...extra,
    program: query.program,
    result_type: query.resultType,
    script_variant: query.scriptVariant,
    bucket: query.bucket,
    // A date input yields a day; the API wants the whole day, not midnight.
    from: query.from === undefined || query.from === "" ? undefined : `${query.from}T00:00:00Z`,
    to: query.to === undefined || query.to === "" ? undefined : `${query.to}T23:59:59Z`,
  };

  for (const [key, value] of Object.entries(entries)) {
    if (value !== undefined && value !== "") {
      search.set(key, value);
    }
  }

  const rendered = search.toString();
  return rendered === "" ? "" : `?${rendered}`;
}
