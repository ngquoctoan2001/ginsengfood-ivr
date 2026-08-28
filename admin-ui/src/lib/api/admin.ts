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
  IvrReviewQueue,
  IvrScriptCatalog,
  IvrSimChannelList,
  IvrTechnicalRetryResult,
  TechnicalRetryRequest,
  IvrScriptActionResult,
  IvrScriptApprovalRequest,
  IvrScriptDraftRequest,
  IvrScriptTransitionRequest,
  IvrScriptVersionDetail,
  IvrFeatureFlagMutationRequest,
  IvrFeatureFlagMutationResult,
  IvrFeatureFlagReadResult,
  IvrKillSwitchVerification,
  SeedLoadRequest,
  IvrSeedLoadResult,
  IvrScenarioDryRunResult,
  IvrIntegrationProfileResult,
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
  /** Calendar-day bounds, already widened to instants by the caller. */
  readonly from?: string;
  readonly to?: string;
  readonly page?: number;
  readonly pageSize?: number;
}

/**
 * Typed admin operations.
 *
 * Every function here has a caller and a test. `GET /queue` deliberately has no
 * wrapper: P3-2 folded that screen into the dashboard, which reads
 * `GET /dashboard` instead, so a wrapper for it would be an operation the
 * console never issues.
 */
export function pauseQueue(
  context: AdminCallContext,
  request: AdminMutationRequest,
): Promise<IvrApiResponse<IvrAdminActionResult>> {
  return callIvrApi<IvrAdminActionResult>({
    method: "POST",
    path: "/queue:pause",
    body: request,
    actionReason: request.reason,
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
    actionReason: request.reason,
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
      from: query.from,
      to: query.to,
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
    actionReason: request.reason,
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

export function listSimChannels(
  context: AdminCallContext,
): Promise<IvrApiResponse<IvrSimChannelList>> {
  return callIvrApi<IvrSimChannelList>({
    method: "GET",
    path: "/sim-channels",
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

/**
 * Disabling a busy channel is accepted, not refused: it stops new dispatch and
 * takes effect when the current call ends. The console shows `busy` so the
 * operator knows which of the two it is asking for.
 */
export function disableSimChannel(
  context: AdminCallContext,
  simChannelId: string,
  request: AdminMutationRequest,
): Promise<IvrApiResponse<IvrAdminActionResult>> {
  return callIvrApi<IvrAdminActionResult>({
    method: "POST",
    path: `/sim-channels/${encodeURIComponent(simChannelId)}:disable`,
    body: request,
    actionReason: request.reason,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function enableSimChannel(
  context: AdminCallContext,
  simChannelId: string,
  request: AdminMutationRequest,
): Promise<IvrApiResponse<IvrAdminActionResult>> {
  return callIvrApi<IvrAdminActionResult>({
    method: "POST",
    path: `/sim-channels/${encodeURIComponent(simChannelId)}:enable`,
    body: request,
    actionReason: request.reason,
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

/**
 * Script lifecycle (W-0109).
 *
 * The version key travels in the path, and the reason in the body, matching the
 * other admin mutations. Nothing here decides who may approve: Ivr.Api answers
 * 403 when the caller is the creator or the account that already signed the
 * other half of the production pair, and 409 when the version's state refuses.
 */
export function getScriptVersion(
  context: AdminCallContext,
  templateId: string,
  version: string,
): Promise<IvrApiResponse<IvrScriptVersionDetail>> {
  return callIvrApi<IvrScriptVersionDetail>({
    method: "GET",
    path: `/scripts/${encodeURIComponent(templateId)}/${encodeURIComponent(version)}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function createScriptDraft(
  context: AdminCallContext,
  request: IvrScriptDraftRequest,
): Promise<IvrApiResponse<IvrScriptActionResult>> {
  return callIvrApi<IvrScriptActionResult>({
    method: "POST",
    path: "/scripts",
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function submitScriptForReview(
  context: AdminCallContext,
  templateId: string,
  version: string,
  request: IvrScriptTransitionRequest,
): Promise<IvrApiResponse<IvrScriptActionResult>> {
  return callIvrApi<IvrScriptActionResult>({
    method: "POST",
    path: `/scripts/${encodeURIComponent(templateId)}/${encodeURIComponent(version)}:submit`,
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function approveScriptVersion(
  context: AdminCallContext,
  templateId: string,
  version: string,
  request: IvrScriptApprovalRequest,
): Promise<IvrApiResponse<IvrScriptActionResult>> {
  return callIvrApi<IvrScriptActionResult>({
    method: "POST",
    path: `/scripts/${encodeURIComponent(templateId)}/${encodeURIComponent(version)}:approve`,
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function retireScriptVersion(
  context: AdminCallContext,
  templateId: string,
  version: string,
  request: IvrScriptTransitionRequest,
): Promise<IvrApiResponse<IvrScriptActionResult>> {
  return callIvrApi<IvrScriptActionResult>({
    method: "POST",
    path: `/scripts/${encodeURIComponent(templateId)}/${encodeURIComponent(version)}:retire`,
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

/**
 * Runtime gates (W-0110).
 *
 * The mutation needs an Idempotency-Key, which `callIvrApi` supplies for POSTs,
 * and an X-Actor-Id the server checks against the authenticated subject — a
 * mismatch is 403, so the header is never a client-chosen identity.
 */
export function getFeatureFlags(
  context: AdminCallContext,
  environment: string,
): Promise<IvrApiResponse<IvrFeatureFlagReadResult>> {
  return callIvrApi<IvrFeatureFlagReadResult>({
    method: "GET",
    path: `/feature-flags/${encodeURIComponent(environment)}`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function verifyKillSwitch(
  context: AdminCallContext,
  environment: string,
): Promise<IvrApiResponse<IvrKillSwitchVerification>> {
  return callIvrApi<IvrKillSwitchVerification>({
    method: "GET",
    path: `/feature-flags/${encodeURIComponent(environment)}/kill-switch`,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function mutateFeatureFlags(
  context: AdminCallContext,
  environment: string,
  request: IvrFeatureFlagMutationRequest,
): Promise<IvrApiResponse<IvrFeatureFlagMutationResult>> {
  return callIvrApi<IvrFeatureFlagMutationResult>({
    method: "POST",
    path: `/feature-flags/${encodeURIComponent(environment)}`,
    body: request,
    actionReason: request.reason,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

/**
 * Cutting a call that is already in progress (W-0111).
 *
 * Records the request; the worker's dispatch loop performs the hangup. The
 * response therefore says the cut was asked for, not that the line is down.
 */
export function terminateCallJob(
  context: AdminCallContext,
  ivrCallJobId: string,
  request: AdminMutationRequest,
): Promise<IvrApiResponse<IvrAdminActionResult>> {
  return callIvrApi<IvrAdminActionResult>({
    method: "POST",
    path: `/call-jobs/${encodeURIComponent(ivrCallJobId)}:terminate`,
    body: request,
    actionReason: request.reason,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

/**
 * Cutting every call currently in progress (W-0111).
 *
 * A separate call from engaging the kill switch on purpose: that stops the next
 * call, this ends conversations already under way.
 */
export function terminateAllCallJobs(
  context: AdminCallContext,
  request: AdminMutationRequest,
): Promise<IvrApiResponse<IvrAdminActionResult>> {
  return callIvrApi<IvrAdminActionResult>({
    method: "POST",
    path: "/call-jobs:terminate-all",
    body: request,
    actionReason: request.reason,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

/**
 * UI-07 developer surface (W-0112). Absent in production: the API does not map these routes when
 * the deployment is production by environment, execution mode or REAL_CUSTOMER_CALL_ALLOWED, so a
 * production console sees 404 rather than a permission error.
 */
export function loadDevSeed(
  context: AdminCallContext,
  request: SeedLoadRequest,
): Promise<IvrApiResponse<IvrSeedLoadResult>> {
  return callIvrApi<IvrSeedLoadResult>({
    method: "POST",
    path: "/dev/seed:load",
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function dryRunDevScenario(
  context: AdminCallContext,
  scenarioId: string,
  request: AdminMutationRequest,
): Promise<IvrApiResponse<IvrScenarioDryRunResult>> {
  return callIvrApi<IvrScenarioDryRunResult>({
    method: "POST",
    path: `/dev/scenarios/${encodeURIComponent(scenarioId)}:dry-run`,
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}

export function applyDevIntegrationProfile(
  context: AdminCallContext,
  profileId: string,
  request: AdminMutationRequest,
): Promise<IvrApiResponse<IvrIntegrationProfileResult>> {
  return callIvrApi<IvrIntegrationProfileResult>({
    method: "POST",
    path: `/dev/integration-profiles/${encodeURIComponent(profileId)}:apply`,
    body: request,
    session: context.session,
    config: context.config,
    fetchImpl: context.fetchImpl,
  });
}
