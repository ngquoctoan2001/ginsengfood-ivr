import "server-only";

import type { AdminSession } from "@/lib/auth/session";
import type { AdminUiConfig } from "@/lib/config/env";

import { callIvrApi, type IvrApiResponse } from "./client";
import type {
  AdminMutationRequest,
  AdminReviewRequest,
  IvrAdminActionResult,
  IvrAdminReviewResult,
  IvrCallJobDetail,
  IvrCallJobPage,
  IvrDashboardProjection,
  IvrIntegrationStatus,
  IvrQueueProjection,
  IvrReviewQueue,
  IvrScriptCatalog,
  IvrTechnicalRetryResult,
  TechnicalRetryRequest,
} from "./types";

interface AdminCallContext {
  readonly session: AdminSession;
  readonly config: AdminUiConfig;
  readonly fetchImpl?: typeof fetch;
}

/** Filters accepted by `GET /call-jobs`. Undefined entries are omitted. */
export interface CallJobQuery {
  readonly program?: string;
  readonly status?: string;
  readonly queueStatus?: string;
  readonly resultType?: string;
  readonly orderCode?: string;
  readonly correlationId?: string;
  readonly nearExpiry?: boolean;
  readonly page?: number;
  readonly pageSize?: number;
}

/**
 * Typed admin operations.
 *
 * The SIM-channel enable/disable operations are still unused here; they arrive
 * with the configuration screens that own them (P3-3 / W-0027), so no operation
 * ships without a caller and a test.
 */
export function getQueue(
  context: AdminCallContext,
): Promise<IvrApiResponse<IvrQueueProjection>> {
  return callIvrApi<IvrQueueProjection>({
    method: "GET",
    path: "/queue",
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function pauseQueue(
  context: AdminCallContext,
  request: AdminMutationRequest,
): Promise<IvrApiResponse<IvrAdminActionResult>> {
  return callIvrApi<IvrAdminActionResult>({
    method: "POST",
    path: "/queue:pause",
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function resumeQueue(
  context: AdminCallContext,
  request: AdminMutationRequest,
): Promise<IvrApiResponse<IvrAdminActionResult>> {
  return callIvrApi<IvrAdminActionResult>({
    method: "POST",
    path: "/queue:resume",
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function getDashboard(
  context: AdminCallContext,
  filter: { program?: string; from?: string; to?: string } = {},
): Promise<IvrApiResponse<IvrDashboardProjection>> {
  return callIvrApi<IvrDashboardProjection>({
    method: "GET",
    path: `/dashboard${buildQuery({
      program: filter.program,
      from: filter.from,
      to: filter.to,
    })}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function listCallJobs(
  context: AdminCallContext,
  query: CallJobQuery = {},
): Promise<IvrApiResponse<IvrCallJobPage>> {
  return callIvrApi<IvrCallJobPage>({
    method: "GET",
    path: `/call-jobs${buildQuery({
      program: query.program,
      status: query.status,
      queue_status: query.queueStatus,
      result_type: query.resultType,
      order_code: query.orderCode,
      correlation_id: query.correlationId,
      near_expiry: query.nearExpiry === true ? "true" : undefined,
      page: query.page,
      page_size: query.pageSize,
    })}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function getCallJobDetail(
  context: AdminCallContext,
  ivrCallJobId: string,
): Promise<IvrApiResponse<IvrCallJobDetail>> {
  return callIvrApi<IvrCallJobDetail>({
    method: "GET",
    path: `/call-jobs/${encodeURIComponent(ivrCallJobId)}/detail`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function requestTechnicalRetry(
  context: AdminCallContext,
  request: TechnicalRetryRequest,
): Promise<IvrApiResponse<IvrTechnicalRetryResult>> {
  return callIvrApi<IvrTechnicalRetryResult>({
    method: "POST",
    path: "/technical-retries",
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function submitAdminReview(
  context: AdminCallContext,
  request: AdminReviewRequest,
): Promise<IvrApiResponse<IvrAdminReviewResult>> {
  return callIvrApi<IvrAdminReviewResult>({
    method: "POST",
    path: "/admin-reviews",
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function getScriptCatalog(
  context: AdminCallContext,
): Promise<IvrApiResponse<IvrScriptCatalog>> {
  return callIvrApi<IvrScriptCatalog>({
    method: "GET",
    path: "/scripts",
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function getIntegrationStatus(
  context: AdminCallContext,
  environment?: string,
): Promise<IvrApiResponse<IvrIntegrationStatus>> {
  return callIvrApi<IvrIntegrationStatus>({
    method: "GET",
    path: `/integration-status${buildQuery({ environment })}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function listReviewItems(
  context: AdminCallContext,
  query: { status?: string; page?: number; pageSize?: number } = {},
): Promise<IvrApiResponse<IvrReviewQueue>> {
  return callIvrApi<IvrReviewQueue>({
    method: "GET",
    path: `/review-items${buildQuery({
      status: query.status,
      page: query.page,
      page_size: query.pageSize,
    })}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

function buildQuery(
  parameters: Readonly<Record<string, string | number | undefined>>,
): string {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(parameters)) {
    if (value !== undefined && value !== "") {
      search.set(key, String(value));
    }
  }

  const rendered = search.toString();
  return rendered === "" ? "" : `?${rendered}`;
}
