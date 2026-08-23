import type { IncomingMessage, ServerResponse } from "node:http";

export const ADMIN_USERNAME = "admin";
export const OPERATOR_USERNAME = "operator";
export const E2E_PASSWORD = "CorrectPassword-123!";

const ADMIN_TOKEN = "e2e-admin-opaque-token-0123456789abcdef";
const OPERATOR_TOKEN = "e2e-operator-opaque-token-0123456789abcdef";
const BASE = "/v1/ivr/order-confirmation";

const ADMIN_PERMISSIONS = [
  "IVR_QUEUE_VIEW",
  "IVR_QUEUE_PAUSE",
  "IVR_QUEUE_RESUME",
  "IVR_SIM_ENABLE",
  "IVR_SIM_DISABLE",
  "IVR_MANUAL_RETRY",
  "IVR_RESULT_REVIEW",
  "IVR_FLAG_READ",
  "IVR_RUNTIME_GATE_ADMIN",
  "IVR_ACCOUNT_VIEW",
  "IVR_ACCOUNT_MANAGE",
  "IVR_ACCOUNT_PASSWORD_RESET",
  "IVR_ACCOUNT_SELF_VIEW",
  "IVR_SCRIPT_EDIT",
  "IVR_SCRIPT_REVIEW",
  "IVR_SCRIPT_APPROVE_MOCK",
  "IVR_SCRIPT_APPROVE_LAB",
  "IVR_SCRIPT_APPROVE_CONTENT",
  "IVR_SCRIPT_APPROVE_PRIVACY_LEGAL",
  "IVR_SCRIPT_RETIRE",
] as const;

const OPERATOR_PERMISSIONS = [
  "IVR_QUEUE_VIEW",
  "IVR_SIM_DISABLE",
  "IVR_MANUAL_RETRY",
  "IVR_ACCOUNT_SELF_VIEW",
] as const;

const ADMIN_ACCOUNT = account(
  "11111111-1111-4111-8111-111111111111",
  ADMIN_USERNAME,
  "Quản trị viên",
  "Admin",
  true,
);
const OPERATOR_ACCOUNT = account(
  "22222222-2222-4222-8222-222222222222",
  OPERATOR_USERNAME,
  "Nhân viên vận hành",
  "Operator",
  false,
);

export async function handleConsoleAuthStub(
  request: IncomingMessage,
  response: ServerResponse,
): Promise<boolean> {
  const url = new URL(request.url ?? "/", "http://127.0.0.1");
  if (url.pathname === `${BASE}/auth/sign-in` && request.method === "POST") {
    const form = JSON.parse(await readBody(request)) as { username?: string; password?: string };
    const principal = credential(form.username, form.password);
    if (principal === null) {
      writeJson(response, 401, error(request, "IVR_UNAUTHENTICATED"));
      return true;
    }

    writeJson(response, 200, {
      access_token: principal.token,
      token_type: "Bearer",
      session: session(principal.account, principal.permissions),
    });
    return true;
  }

  if (url.pathname === `${BASE}/auth/session` && request.method === "GET") {
    const principal = bearer(request);
    if (principal === null) {
      writeJson(response, 401, error(request, "IVR_UNAUTHENTICATED"));
      return true;
    }

    writeJson(response, 200, session(principal.account, principal.permissions));
    return true;
  }

  if (url.pathname === `${BASE}/auth/sign-out` && request.method === "POST") {
    writeJson(response, bearer(request) === null ? 401 : 200, { revoked: true });
    return true;
  }

  if (url.pathname === `${BASE}/account-roles` && request.method === "GET") {
    const principal = bearer(request);
    if (principal?.account.role !== "Admin") {
      writeJson(response, 403, error(request, "IVR_FORBIDDEN_CALLER"));
      return true;
    }

    writeJson(response, 200, {
      roles: [
        { role: "Admin", label: "Quản trị viên", permissions: ADMIN_PERMISSIONS },
        { role: "Operator", label: "Nhân viên vận hành", permissions: OPERATOR_PERMISSIONS },
      ],
    });
    return true;
  }

  if (url.pathname === `${BASE}/accounts/me` && request.method === "GET") {
    const principal = bearer(request);
    if (principal === null) {
      writeJson(response, 401, error(request, "IVR_UNAUTHENTICATED"));
      return true;
    }

    writeJson(response, 200, principal.account);
    return true;
  }

  return false;
}

export function signInBody(username = ADMIN_USERNAME): URLSearchParams {
  return new URLSearchParams({ username, password: E2E_PASSWORD });
}

function credential(username?: string, password?: string) {
  if (password !== E2E_PASSWORD) return null;
  if (username === ADMIN_USERNAME) {
    return { token: ADMIN_TOKEN, account: ADMIN_ACCOUNT, permissions: ADMIN_PERMISSIONS };
  }
  if (username === OPERATOR_USERNAME) {
    return { token: OPERATOR_TOKEN, account: OPERATOR_ACCOUNT, permissions: OPERATOR_PERMISSIONS };
  }
  return null;
}

function bearer(request: IncomingMessage) {
  const value = request.headers.authorization;
  if (value === `Bearer ${ADMIN_TOKEN}`) {
    return { token: ADMIN_TOKEN, account: ADMIN_ACCOUNT, permissions: ADMIN_PERMISSIONS };
  }
  if (value === `Bearer ${OPERATOR_TOKEN}`) {
    return { token: OPERATOR_TOKEN, account: OPERATOR_ACCOUNT, permissions: OPERATOR_PERMISSIONS };
  }
  return null;
}

function account(id: string, username: string, displayName: string, role: "Admin" | "Operator", builtin: boolean) {
  return {
    account_id: id,
    username,
    display_name: displayName,
    role,
    status: "ACTIVE",
    is_builtin: builtin,
    is_locked: false,
    locked_until: null,
      ...(username === "trcongphuc2003"
        ? {}
        : { last_login_at: "2026-08-22T01:00:00Z" }),
    password_changed_at: "2026-08-22T00:00:00Z",
    created_at: "2026-08-22T00:00:00Z",
    updated_at: "2026-08-22T01:00:00Z",
    deleted_at: null,
    version: 1,
  } as const;
}

function session(accountView: typeof ADMIN_ACCOUNT | typeof OPERATOR_ACCOUNT, permissions: readonly string[]) {
  return {
    account: accountView,
    permissions,
    expires_at: "2099-08-22T09:00:00Z",
  };
}

function error(request: IncomingMessage, code: string) {
  return {
    error: {
      code,
      message: "Authentication failed.",
      correlationId: String(request.headers["x-correlation-id"] ?? "e2e-auth"),
    },
  };
}

function writeJson(response: ServerResponse, status: number, body: unknown): void {
  response.writeHead(status, { "Content-Type": "application/json" });
  response.end(JSON.stringify(body));
}

function readBody(request: IncomingMessage): Promise<string> {
  return new Promise((resolve, reject) => {
    const chunks: Buffer[] = [];
    request.on("data", (chunk: Buffer) => chunks.push(chunk));
    request.on("end", () => resolve(Buffer.concat(chunks).toString("utf8")));
    request.on("error", reject);
  });
}
