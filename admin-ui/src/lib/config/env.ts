import "server-only";

/**
 * Server-side configuration. Nothing here may be re-exported to a Client
 * Component: the API base URL, the session secret and the raw execution mode
 * all stay on the Next.js server (specs/ui/08 §4 — the browser never receives
 * an internal service token).
 */

export const MOCK_EXECUTION_MODE = "MOCK";

export interface AdminUiConfig {
  readonly apiBaseUrl: string;
  readonly executionMode: string;
  readonly isMockMode: boolean;
  readonly realCustomerCallAllowed: boolean;
  readonly environmentLabel: string;
  /** `NODE_ENV === "production"`, i.e. a production *build*. Used for cookie flags. */
  readonly isProductionRuntime: boolean;
  /**
   * Whether the deployment environment is a known non-production one.
   *
   * Deliberately an allowlist rather than `label !== "production"`: an unfamiliar
   * or misspelled label locks the non-prod screens instead of opening them. It is
   * also kept separate from `isProductionRuntime`, because every `next start`
   * runs with `NODE_ENV=production` — including staging and lab.
   */
  readonly isNonProductionEnvironment: boolean;
}

const NON_PRODUCTION_ENVIRONMENTS: ReadonlySet<string> = new Set([
  "dev",
  "development",
  "local",
  "test",
  "staging",
  "lab",
]);

const SESSION_SECRET_MIN_LENGTH = 32;

function readOptional(name: string): string | undefined {
  const value = process.env[name];
  return value === undefined || value.trim() === "" ? undefined : value.trim();
}

export function readConfig(): AdminUiConfig {
  const environmentLabel = (readOptional("IVR_ENVIRONMENT_LABEL") ?? "dev").toLowerCase();
  const executionMode = readOptional("IVR_EXECUTION_MODE") ?? MOCK_EXECUTION_MODE;
  // Fail-closed: only the literal string "NO" is meaningful today, and anything
  // other than an explicit "YES" keeps real customer calls disabled. Enabling
  // real calls is a P9-1 release gate (DF-03), never a UI environment variable.
  const realCustomerCallAllowed =
    (readOptional("REAL_CUSTOMER_CALL_ALLOWED") ?? "NO").toUpperCase() === "YES";

  return {
    apiBaseUrl: (readOptional("IVR_API_BASE_URL") ?? "http://127.0.0.1:5005").replace(
      /\/+$/,
      "",
    ),
    executionMode,
    isMockMode: executionMode.toUpperCase() === MOCK_EXECUTION_MODE,
    realCustomerCallAllowed,
    environmentLabel,
    isProductionRuntime: process.env.NODE_ENV === "production",
    isNonProductionEnvironment: NON_PRODUCTION_ENVIRONMENTS.has(environmentLabel),
  };
}

/**
 * The session signing key. Absent or too short is a hard startup error rather
 * than a silent fallback — an unsigned admin session is worse than no UI.
 */
export function readSessionSecret(): string {
  const secret = readOptional("IVR_ADMIN_UI_SESSION_SECRET");
  if (secret === undefined || secret.length < SESSION_SECRET_MIN_LENGTH) {
    throw new Error(
      `IVR_ADMIN_UI_SESSION_SECRET is required and must be at least ${SESSION_SECRET_MIN_LENGTH} characters.`,
    );
  }

  return secret;
}
