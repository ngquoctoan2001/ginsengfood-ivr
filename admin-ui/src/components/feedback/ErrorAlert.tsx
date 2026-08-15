import { isIvrErrorCode } from "@/lib/api/types";
import { t, type MessageKey } from "@/lib/i18n";

import styles from "./ErrorAlert.module.css";

export interface ErrorEnvelopeView {
  readonly code: string;
  readonly message: string;
  readonly correlationId: string;
  readonly details?: Readonly<Record<string, string>>;
}

export interface ErrorAlertProps {
  readonly error: ErrorEnvelopeView;
}

/**
 * The single rendering of an API-06 error envelope.
 *
 * Both the localized explanation *and* the raw `code` are shown: ops quote the
 * code in tickets, and the correlation id is what ties the screen to the server
 * log line for the same request.
 */
export function ErrorAlert({ error }: ErrorAlertProps) {
  const localized = isIvrErrorCode(error.code)
    ? t(`error.${error.code}` as MessageKey)
    : error.message;
  const details = Object.entries(error.details ?? {});

  return (
    <div className={styles.alert} role="alert">
      <p className={styles.title}>{t("state.errorTitle")}</p>
      <p className={styles.message}>{localized}</p>
      <dl className={styles.meta}>
        <dt>{t("error.code")}</dt>
        <dd data-testid="error-code">{error.code}</dd>
        <dt>{t("error.correlationId")}</dt>
        <dd data-testid="error-correlation-id">{error.correlationId}</dd>
      </dl>
      {details.length > 0 ? (
        <>
          <p className={styles.detailsTitle}>{t("error.details")}</p>
          <ul className={styles.details}>
            {details.map(([key, value]) => (
              <li key={key}>
                <span className={styles.detailKey}>{key}</span>: {value}
              </li>
            ))}
          </ul>
        </>
      ) : null}
    </div>
  );
}
