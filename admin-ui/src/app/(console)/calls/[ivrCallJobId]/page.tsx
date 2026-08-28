import { Suspense } from "react";

import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { MaskedPhone } from "@/components/privacy/MaskedPhone";
import { BooleanCell } from "@/components/data/BooleanCell";
import { EnumLabel, EnumLabelList } from "@/components/data/EnumLabel";
import { ReviewReason } from "@/components/data/ReviewReason";
import {
  Callout,
  Card,
  CardStack,
  ChipList,
  DescriptionList,
  PageHeader,
  Timeline,
  TimelineItem,
} from "@/components/ui";
import { getCallJobDetail } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrCallJobDetail } from "@/lib/api/types";
import { requireScope } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";

import { CallDetailActions } from "./CallDetailActions";

export const dynamic = "force-dynamic";

export default async function CallDetailPage({
  params,
}: PageProps<"/calls/[ivrCallJobId]">) {
  await requireScope("read");
  const { ivrCallJobId } = await params;

  return (
    <>
      <PageHeader
        title={t("detail.title")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.callLog"), href: "/calls" },
            { label: ivrCallJobId },
          ],
        }}
        meta={[{ label: "ID", value: ivrCallJobId, mono: true }]}
      />
      <Suspense key={ivrCallJobId} fallback={<LoadingSkeleton rows={8} />}>
        <CallDetailBody ivrCallJobId={ivrCallJobId} />
      </Suspense>
    </>
  );
}

async function CallDetailBody({ ivrCallJobId }: { ivrCallJobId: string }) {
  const session = await requireScope("read");
  const config = readConfig();

  let detail: IvrCallJobDetail | null = null;
  let error: ErrorEnvelopeView | null = null;

  try {
    detail = (await getCallJobDetail({ session, config }, ivrCallJobId)).data;
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    error = cause.toEnvelope();
  }

  if (error !== null || detail === null) {
    return <ErrorAlert error={error!} />;
  }

  return (
    <CardStack>
      <Card title={t("detail.title")} accent>
        <DescriptionList
          items={[
            { label: t("calls.colOrderCode"), value: detail.order_code_short || "—", mono: true },
            { label: t("calls.colPhone"), value: <MaskedPhone value={detail.phone_masked} /> },
            {
              label: t("calls.colProgram"),
              value: <EnumLabel family="programType" value={detail.program_type} showCode />,
            },
            {
              // NT-3. Order Core owns this value set and the contract calls it an
              // opaque enum (D-02); IVR does not know it and must not invent a
              // dictionary for it. Rendered verbatim, on purpose.
              label: t("detail.orderState"),
              value: detail.order_state,
              testId: "order-state",
            },
            { label: t("detail.orderVersion"), value: detail.order_version_snapshot },
            {
              label: t("detail.jobStatus"),
              value: <EnumLabel family="jobStatus" value={detail.status} showCode />,
            },
            {
              label: t("detail.queueStatus"),
              value: <EnumLabel family="jobStatus" value={detail.queue_status} showCode />,
            },
            {
              label: t("detail.eligibility"),
              value: (
                <EnumLabel
                  family="eligibilityDecision"
                  value={detail.eligibility_decision}
                  showCode
                />
              ),
            },
            {
              label: t("detail.callRestriction"),
              value: <BooleanCell value={detail.call_restriction} />,
            },
            { label: t("detail.policy"), value: detail.attempt_policy_code },
            { label: t("detail.scriptVersion"), value: detail.script_version },
            {
              label: t("detail.voiceRegion"),
              value: (
                <EnumLabel
                  family="voiceRegion"
                  value={detail.voice_region}
                  showCode
                  // Absent is not "untranslated": it means no province could be identified in
                  // the delivery area, which is a Sales master-data signal the operator should
                  // read as such rather than as an em dash.
                  fallback={t("detail.voiceRegionUnknown")}
                  testId="voice-region"
                />
              ),
            },
            {
              // W-0113. Shown beside the region, never folded into it. An operator deciding
              // whether this number can go into something they sign needs to know whether it
              // was recorded at the call or recomputed just now from configuration that may
              // since have changed — and a value alone cannot tell them.
              label: t("detail.voiceRegionSource"),
              value: (
                <EnumLabel
                  family="voiceRegionSource"
                  value={detail.voice_region_source ?? undefined}
                  showCode
                  fallback={t("detail.voiceRegionUnknown")}
                  testId="voice-region-source"
                />
              ),
            },
            {
              label: t("detail.window"),
              value: `${formatDateTime(detail.t0_at)} → ${formatDateTime(detail.expires_at)}`,
            },
          ]}
        />

        {/* W-0113. Said in words, not only as an enum chip. The derived value is the one that
            goes quietly wrong — a config change after the call makes it describe a voice nobody
            heard, and nothing about the value itself shows that — so the screen states plainly
            which of the two is in front of the reader and whether it can be signed. */}
        {detail.voice_region_source === "DERIVED" ? (
          <Callout tone="warning" role="alert" testId="voice-region-derived">
            {t("detail.voiceRegionDerivedWarning")}
          </Callout>
        ) : null}
        {detail.voice_region_source === "RECORDED" ? (
          <Callout tone="success" testId="voice-region-recorded">
            {t("detail.voiceRegionRecordedNote")}
          </Callout>
        ) : null}

        {detail.blocked_reasons.length === 0 ? null : (
          <Callout tone="warning" title={t("detail.blockedReasons")}>
            <EnumLabelList family="blockedReason" values={detail.blocked_reasons} />
          </Callout>
        )}

        <Callout tone="locked" testId="no-order-control">
          {t("detail.noOrderControl")}
        </Callout>
      </Card>

      <Card title={t("detail.attemptsTitle")}>
        {detail.attempts.length === 0 ? (
          <Callout tone="neutral">{t("detail.noAttempts")}</Callout>
        ) : (
          <Timeline>
            {detail.attempts.map((attempt) => (
              <TimelineItem
                key={attempt.ivr_call_attempt_id}
                title={
                  <>
                    {t("detail.attemptNumber")} {formatNumber(attempt.attempt_number)} ·{" "}
                    {/* An attempt is not a job. The two taxonomies overlap on the five dispatch
                        states, which is why rendering attempts through `jobStatus` looked correct
                        for as long as a call was still in flight — and then showed a raw code the
                        moment normalisation finished, on every attempt that ever completed. */}
                    <EnumLabel family="attemptStatus" value={attempt.status} />
                  </>
                }
                meta={formatDateTime(attempt.scheduled_at)}
                tone={attempt.technical_exception_type === undefined ? undefined : "danger"}
              >
                <DescriptionList
                  items={[
                    {
                      label: t("detail.attemptScheduled"),
                      value: formatDateTime(attempt.scheduled_at),
                    },
                    {
                      label: t("detail.attemptDisposition"),
                      value: <EnumLabel family="disposition" value={attempt.disposition} />,
                    },
                    { label: t("detail.attemptDtmf"), value: describeDtmf(attempt.dtmf_key) },
                    {
                      label: t("detail.attemptCounted"),
                      value: <BooleanCell value={attempt.is_counted_customer_attempt} />,
                    },
                    {
                      label: t("detail.attemptTechnical"),
                      value: (
                        <EnumLabel
                          family="technicalExceptionType"
                          value={attempt.technical_exception_type}
                        />
                      ),
                    },
                    {
                      label: t("detail.attemptSim"),
                      value: attempt.sim_channel_id ?? "—",
                      mono: true,
                    },
                    { label: t("detail.policy"), value: attempt.policy_version },
                  ]}
                />
              </TimelineItem>
            ))}
          </Timeline>
        )}
      </Card>

      <Card
        title={t("detail.resultsTitle")}
        footer={t("detail.resultAdvisory")}
      >
        {detail.results.length === 0 ? (
          <Callout tone="neutral">{t("detail.noResults")}</Callout>
        ) : (
          detail.results.map((result) => (
            <DescriptionList
              key={result.ivr_call_result_id}
              items={[
                {
                  label: t("detail.resultType"),
                  value: <EnumLabel family="resultType" value={result.result_type} showCode />,
                  testId: "result-type",
                },
                {
                  label: t("detail.resultFinal"),
                  value: <BooleanCell value={result.is_final_for_ivr} />,
                },
                { label: t("detail.attemptDtmf"), value: describeDtmf(result.dtmf_key) },
                {
                  label: t("detail.resultRecommended"),
                  value: (
                    <EnumLabel
                      family="recommendedCoreAction"
                      value={result.recommended_core_action}
                      showCode
                    />
                  ),
                },
                { label: t("calls.colDeadline"), value: formatDateTime(result.created_at) },
              ]}
            />
          ))
        )}
      </Card>

      <Card title={t("detail.callbacksTitle")}>
        {detail.callbacks.length === 0 ? (
          <Callout tone="neutral">{t("detail.noCallbacks")}</Callout>
        ) : (
          detail.callbacks.map((callback) => (
            <DescriptionList
              key={callback.callback_id}
              items={[
                { label: "ID", value: callback.callback_id, mono: true },
                {
                  label: t("detail.callbackState"),
                  value: (
                    <EnumLabel family="callbackResultState" value={callback.result_state} showCode />
                  ),
                },
                {
                  label: t("detail.callbackDelivery"),
                  value: (
                    <EnumLabel family="deliveryStatus" value={callback.delivery_status} showCode />
                  ),
                },
                {
                  label: t("detail.callbackCoreStatus"),
                  value:
                    callback.core_http_status === undefined
                      ? "—"
                      : formatNumber(callback.core_http_status),
                  testId: "callback-core-status",
                },
                {
                  label: t("detail.callbackCoreCode"),
                  value: callback.core_response_code ?? "—",
                },
                {
                  label: t("detail.callbackRetry"),
                  value: formatNumber(callback.retry_count),
                },
              ]}
            />
          ))
        )}
      </Card>

      <Card title={t("detail.technicalTitle")}>
        {detail.technical_exceptions.length === 0 ? (
          <Callout tone="success">{t("detail.noTechnical")}</Callout>
        ) : (
          detail.technical_exceptions.map((exception) => (
            <DescriptionList
              key={exception.technical_exception_id}
              items={[
                { label: "ID", value: exception.technical_exception_id, mono: true },
                {
                  label: t("detail.technicalType"),
                  value: (
                    <EnumLabel
                      family="technicalExceptionType"
                      value={exception.exception_type}
                      showCode
                    />
                  ),
                },
                {
                  label: t("detail.technicalRetryAllowed"),
                  value: <BooleanCell value={exception.technical_retry_allowed} />,
                },
                {
                  label: t("dashboard.attemptTechnicalRetry"),
                  value: formatNumber(exception.technical_retry_count),
                },
              ]}
            />
          ))
        )}
      </Card>

      <Card title={t("detail.reviewTitle")}>
        {detail.review_items.length === 0 ? (
          <Callout tone="neutral">{t("detail.noReview")}</Callout>
        ) : (
          detail.review_items.map((item) => (
            <DescriptionList
              key={item.review_item_id}
              items={[
                { label: "ID", value: item.review_item_id, mono: true },
                {
                  label: t("detail.reviewReason"),
                  value: <ReviewReason value={item.reason} showCode />,
                },
                {
                  label: t("detail.reviewStatus"),
                  value: <EnumLabel family="reviewStatus" value={item.status} />,
                },
                {
                  // Free text an admin typed when closing the item, not an enum.
                  label: t("detail.reviewResolution"),
                  value: item.resolution ?? "—",
                },
              ]}
            />
          ))
        )}
      </Card>

      <CallDetailActions
        technicalExceptions={detail.technical_exceptions}
        reviewItems={detail.review_items}
        ivrCallJobId={detail.ivr_call_job_id}
        // Started and not ended: the same definition the API uses to decide whether
        // there is anything to cut. Derived here only to decide whether to draw the
        // button — the server re-derives it and answers 409 if the call has since
        // finished, so a stale page cannot cut a call that already ended.
        hasCallInProgress={detail.attempts.some(
          (attempt) =>
            attempt.started_at !== undefined && attempt.ended_at === undefined,
        )}
      />

      <Card title={t("detail.evidenceTitle")}>
        <DescriptionList
          items={[
            { label: t("detail.correlationId"), value: detail.correlation_id, mono: true },
          ]}
        />
        {detail.evidence_refs.length === 0 && detail.audit_refs.length === 0 ? (
          <Callout tone="neutral">{t("detail.noEvidence")}</Callout>
        ) : (
          <>
            <ReferenceList title={t("detail.evidenceRefs")} items={detail.evidence_refs} />
            <ReferenceList title={t("detail.auditRefs")} items={detail.audit_refs} />
          </>
        )}
      </Card>
    </CardStack>
  );
}

function ReferenceList({ title, items }: { title: string; items: readonly string[] }) {
  if (items.length === 0) {
    return null;
  }

  return (
    <ChipList
      label={title}
      items={items.map((reference) => ({ key: reference, label: reference }))}
    />
  );
}

/** DTMF is shown as business semantics, never as a raw provider payload (D-05). */
function describeDtmf(key: string | undefined): string {
  if (key === undefined || key === "") {
    return "—";
  }

  if (key === "1") {
    return t("detail.dtmfConfirm");
  }

  return key === "0" ? t("detail.dtmfCancel") : `${key} — ${t("detail.dtmfInvalid")}`;
}
