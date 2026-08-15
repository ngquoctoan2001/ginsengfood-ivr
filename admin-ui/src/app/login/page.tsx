import { EnvironmentBadge } from "@/components/shell/EnvironmentBadge";
import { GovernanceNotice } from "@/components/shell/GovernanceNotice";
import { MOCK_DIRECTORY } from "@/lib/auth/directory";
import {
  isSignInErrorCode,
  safeRedirectTarget,
  SIGN_IN_ERROR_KEYS,
} from "@/lib/auth/sign-in";
import { readConfig } from "@/lib/config/env";
import { t } from "@/lib/i18n";

import { LoginForm } from "./LoginForm";
import styles from "./page.module.css";

export const dynamic = "force-dynamic";

export default async function LoginPage({ searchParams }: PageProps<"/login">) {
  const config = readConfig();
  const params = await searchParams;

  const requestedNext = typeof params.next === "string" ? params.next : null;
  const next = requestedNext === null ? null : safeRedirectTarget(requestedNext, "/dashboard");

  const errorCode = typeof params.error === "string" ? params.error : null;
  const errorMessage = isSignInErrorCode(errorCode)
    ? t(SIGN_IN_ERROR_KEYS[errorCode])
    : null;

  return (
    <div className={styles.page}>
      <section className={styles.card}>
        <p className={styles.eyebrow}>{t("app.title")}</p>
        <h1 className={styles.title}>{t("auth.signIn.title")}</h1>
        <p className={styles.subtitle}>{t("auth.signIn.subtitle")}</p>

        <EnvironmentBadge
          environmentLabel={config.environmentLabel}
          executionMode={config.executionMode}
          isMockMode={config.isMockMode}
        />

        {config.isMockMode ? (
          <>
            <LoginForm
              directory={MOCK_DIRECTORY}
              next={next}
              errorMessage={errorMessage}
            />
            <p className={styles.notice}>{t("auth.signIn.mockNotice")}</p>
          </>
        ) : (
          <p className={styles.blocked} role="alert">
            {t("auth.signIn.unavailable")}
          </p>
        )}

        <GovernanceNotice realCustomerCallAllowed={config.realCustomerCallAllowed} />
      </section>
    </div>
  );
}
