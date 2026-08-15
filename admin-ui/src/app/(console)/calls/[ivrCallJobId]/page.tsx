import Link from "next/link";
import { Suspense } from "react";

import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { MaskedPhone } from "@/components/privacy/MaskedPhone";
import { getCallJobDetail } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrCallJobDetail } from "@/lib/api/types";
import { requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";

import { CallDetailActions } from "./CallDetailActions";
import table from "@/components/data/DataTable.module.css";
import styles from "./page.module.css";

export const dynamic = "force-dynamic";

export default async function CallDetailPage({
  params,
}: PageProps<"/calls/[ivrCallJobId]">) {
  const { ivrCallJobId } = await params;

  return (
    <>
      <header className={styles.header}>
        <Link href="/calls" className={styles.back}>
          {`← ${t("detail.back")}`}
        </Link>
        <h1 className={styles.title}>{t("detail.title")}</h1>
        <p className={styles.jobId}>{ivrCallJobId}</p>
      </header>
      <Suspense key={ivrCallJobId} fallback={<LoadingSkeleton rows={8} />}>
        <CallDetailBody ivrCallJobId={ivrCallJobId} />
      </Suspense>
    </>
  );
}

async function CallDetailBody({ ivrCallJobId }: { ivrCallJobId: string }) {
  const session = await requireSession();
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
    <>
      <section className={styles.section}>
        <dl className={styles.summary}>
          <Field label={t("calls.colOrderCode")} value={detail.order_code_short || "—"} mono />
          <div>
            <dt>{t("calls.colPhone")}</dt>
            <dd>
              <MaskedPhone value={detail.phone_masked} />
            </dd>
          </div>
          <Field label={t("calls.colProgram")} value={detail.program_type} />
          <Field label={t("detail.orderState")} value={detail.order_state} testId="order-state" />
          <Field label={t("detail.orderVersion")} value={detail.order_version_snapshot} />
          <Field label={t("detail.jobStatus")} value={detail.status} />
          <Field label={t("detail.queueStatus")} value={detail.queue_status} />
          <Field label={t("detail.eligibility")} value={detail.eligibility_decision} />
          <Field
            label={t("detail.callRestriction")}
            value={detail.call_restriction ? "✓" : "—"}
          />
          <Field label={t("detail.policy")} value={detail.attempt_policy_code} />
          <Field label={t("detail.scriptVersion")} value={detail.script_version} />
          <Field
            label={t("detail.window")}
            value={`${formatDateTime(detail.t0_at)} → ${formatDateTime(detail.expires_at)}`}
          />
        </dl>
        {detail.blocked_reasons.length > 0 ? (
          <p className={styles.blocked}>
            {`${t("detail.blockedReasons")}: ${detail.blocked_reasons.join(", ")}`}
          </p>
        ) : null}
        <p className={styles.notice} data-testid="no-order-control">
          {t("detail.noOrderControl")}
        </p>
      </section>

      {/* `specs/ui/03` puts the per-line sellable snapshot in the trace. It is
          what Order Core decided at intake, shown as captured — IVR never
          re-evaluates sellability (DO-02). */}
      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("detail.sellableTitle")}</h2>
        {detail.sellable_status.length === 0 ? (
          <p className={styles.muted}>{t("detail.noSellable")}</p>
        ) : (
          <div className={table.scroll}>
            <table className={table.table} data-testid="sellable-table">
              <thead>
                <tr>
                  <th scope="col">{t("detail.sellableSku")}</th>
                  <th scope="col">{t("detail.sellableBatch")}</th>
                  <th scope="col">{t("detail.sellableDecision")}</th>
                  <th scope="col">{t("detail.sellableRecallHold")}</th>
                  <th scope="col">{t("detail.sellableSaleLock")}</th>
                  <th scope="col">{t("detail.sellableQualityHold")}</th>
                  <th scope="col">{t("detail.sellableCapturedAt")}</th>
                </tr>
              </thead>
              <tbody>
                {detail.sellable_status.map((line) => (
                  <tr key={`${line.sku_id}|${line.batch_id ?? ""}`}>
                    <td className={table.mono}>{line.sku_id}</td>
                    <td className={table.mono}>{line.batch_id ?? "—"}</td>
                    <td>{line.decision}</td>
                    <td>{flag(line.recall_hold)}</td>
                    <td>{flag(line.sale_lock)}</td>
                    <td>{flag(line.quality_hold)}</td>
                    <td>
                      {line.captured_at === undefined ? "—" : formatDateTime(line.captured_at)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("detail.attemptsTitle")}</h2>
        {detail.attempts.length === 0 ? (
          <p className={styles.muted}>{t("detail.noAttempts")}</p>
        ) : (
          <ol className={styles.timeline}>
            {detail.attempts.map((attempt) => (
              <li key={attempt.ivr_call_attempt_id} className={styles.timelineItem}>
                <p className={styles.timelineHead}>
                  {`${t("detail.attemptNumber")} ${formatNumber(attempt.attempt_number)} · ${attempt.status}`}
                </p>
                <dl className={styles.summary}>
                  <Field
                    label={t("detail.attemptScheduled")}
                    value={formatDateTime(attempt.scheduled_at)}
                  />
                  <Field
                    label={t("detail.attemptDisposition")}
                    value={attempt.disposition ?? "—"}
                  />
                  <Field label={t("detail.attemptDtmf")} value={describeDtmf(attempt.dtmf_key)} />
                  <Field
                    label={t("detail.attemptCounted")}
                    value={attempt.is_counted_customer_attempt ? "✓" : "—"}
                  />
                  <Field
                    label={t("detail.attemptTechnical")}
                    value={attempt.technical_exception_type ?? "—"}
                  />
                  <Field
                    label={t("detail.attemptSim")}
                    value={attempt.sim_channel_id ?? "—"}
                    mono
                  />
                  <Field label={t("detail.policy")} value={attempt.policy_version} />
                </dl>
              </li>
            ))}
          </ol>
        )}
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("detail.resultsTitle")}</h2>
        {detail.results.length === 0 ? (
          <p className={styles.muted}>{t("detail.noResults")}</p>
        ) : (
          detail.results.map((result) => (
            <dl key={result.ivr_call_result_id} className={styles.summary}>
              <Field
                label={t("detail.resultType")}
                value={result.result_type}
                testId="result-type"
              />
              <Field label={t("detail.resultFinal")} value={result.is_final_for_ivr ? "✓" : "—"} />
              <Field label={t("detail.attemptDtmf")} value={describeDtmf(result.dtmf_key)} />
              <Field
                label={t("detail.resultRecommended")}
                value={result.recommended_core_action}
              />
              <Field label={t("calls.colDeadline")} value={formatDateTime(result.created_at)} />
            </dl>
          ))
        )}
        <p className={styles.notice}>{t("detail.resultAdvisory")}</p>
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("detail.callbacksTitle")}</h2>
        {detail.callbacks.length === 0 ? (
          <p className={styles.muted}>{t("detail.noCallbacks")}</p>
        ) : (
          detail.callbacks.map((callback) => (
            <dl key={callback.callback_id} className={styles.summary}>
              <Field label="ID" value={callback.callback_id} mono />
              <Field label={t("detail.callbackState")} value={callback.result_state} />
              <Field label={t("detail.callbackDelivery")} value={callback.delivery_status} />
              <Field
                label={t("detail.callbackCoreStatus")}
                value={
                  callback.core_http_status === undefined
                    ? "—"
                    : formatNumber(callback.core_http_status)
                }
                testId="callback-core-status"
              />
              <Field
                label={t("detail.callbackCoreCode")}
                value={callback.core_response_code ?? "—"}
              />
              <Field
                label={t("detail.callbackRetry")}
                value={formatNumber(callback.retry_count)}
              />
            </dl>
          ))
        )}
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("detail.technicalTitle")}</h2>
        {detail.technical_exceptions.length === 0 ? (
          <p className={styles.muted}>{t("detail.noTechnical")}</p>
        ) : (
          detail.technical_exceptions.map((exception) => (
            <dl key={exception.technical_exception_id} className={styles.summary}>
              <Field label="ID" value={exception.technical_exception_id} mono />
              <Field label={t("detail.technicalType")} value={exception.exception_type} />
              <Field
                label={t("detail.technicalRetryAllowed")}
                value={exception.technical_retry_allowed ? "✓" : "—"}
              />
              <Field
                label={t("dashboard.attemptTechnicalRetry")}
                value={formatNumber(exception.technical_retry_count)}
              />
            </dl>
          ))
        )}
      </section>

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("detail.reviewTitle")}</h2>
        {detail.review_items.length === 0 ? (
          <p className={styles.muted}>{t("detail.noReview")}</p>
        ) : (
          detail.review_items.map((item) => (
            <dl key={item.review_item_id} className={styles.summary}>
              <Field label="ID" value={item.review_item_id} mono />
              <Field label={t("detail.reviewReason")} value={item.reason} />
              <Field label={t("detail.reviewStatus")} value={item.status} />
              <Field label={t("detail.reviewResolution")} value={item.resolution ?? "—"} />
            </dl>
          ))
        )}
      </section>

      <CallDetailActions
        technicalExceptions={detail.technical_exceptions}
        reviewItems={detail.review_items}
      />

      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>{t("detail.evidenceTitle")}</h2>
        <dl className={styles.summary}>
          <Field label={t("detail.correlationId")} value={detail.correlation_id} mono />
        </dl>
        {detail.evidence_refs.length === 0 && detail.audit_refs.length === 0 ? (
          <p className={styles.muted}>{t("detail.noEvidence")}</p>
        ) : (
          <>
            <ReferenceList title={t("detail.evidenceRefs")} items={detail.evidence_refs} />
            <ReferenceList title={t("detail.auditRefs")} items={detail.audit_refs} />
          </>
        )}
      </section>
    </>
  );
}

function Field({
  label,
  value,
  mono,
  testId,
}: {
  label: string;
  value: string;
  mono?: boolean;
  testId?: string;
}) {
  return (
    <div>
      <dt>{label}</dt>
      <dd className={mono === true ? styles.mono : undefined} data-testid={testId}>
        {value}
      </dd>
    </div>
  );
}

function ReferenceList({ title, items }: { title: string; items: readonly string[] }) {
  if (items.length === 0) {
    return null;
  }

  return (
    <>
      <p className={styles.muted}>{title}</p>
      <ul className={styles.refs}>
        {items.map((reference) => (
          <li key={reference} className={styles.mono}>
            {reference}
          </li>
        ))}
      </ul>
    </>
  );
}

/** DTMF is shown as business semantics, never as a raw provider payload (D-05). */
/** A tri-state snapshot flag: set, not set, or not captured at all. */
function flag(value: boolean | undefined): string {
  return value === undefined ? "—" : value ? "✓" : "–";
}

function describeDtmf(key: string | undefined): string {
  if (key === undefined || key === "") {
    return "—";
  }

  if (key === "1") {
    return t("detail.dtmfConfirm");
  }

  return key === "0" ? t("detail.dtmfCancel") : `${key} — ${t("detail.dtmfInvalid")}`;
}
