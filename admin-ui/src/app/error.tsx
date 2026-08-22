"use client";

import { useEffect } from "react";

import { Button, Callout } from "@/components/ui";
import { t } from "@/lib/i18n";

import styles from "./error.module.css";

/**
 * Route-level error boundary.
 *
 * `error.message` is intentionally not rendered: an unexpected server error can
 * carry a stack, a URL or a payload fragment, and this console is bound by D-05
 * never to surface unmasked data. The digest is enough to find the server log.
 */
export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error("Unhandled admin UI error", error.digest ?? "no-digest");
  }, [error]);

  return (
    <div className={styles.wrapper}>
      <Callout tone="danger" title={t("state.errorTitle")} role="alert">
        {error.digest === undefined ? "" : `digest: ${error.digest}`}
      </Callout>
      <Button variant="secondary" onClick={reset}>
        {t("state.retry")}
      </Button>
    </div>
  );
}
