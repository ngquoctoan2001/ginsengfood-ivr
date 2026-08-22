// @vitest-environment node
import { spawn, type ChildProcess } from "node:child_process";
import { createServer as createHttpServer, type Server } from "node:http";
import { createServer } from "node:net";
import { fileURLToPath } from "node:url";

import { afterAll, beforeAll, describe, expect, it } from "vitest";

import { ADMIN_USERNAME, handleConsoleAuthStub, signInBody } from "./console-auth-stub";

const projectRoot = fileURLToPath(new URL("../../", import.meta.url));
const nextBin = fileURLToPath(new URL("../../node_modules/next/dist/bin/next", import.meta.url));

const serverEnv: NodeJS.ProcessEnv = {
  ...process.env,
  NODE_ENV: "production",
  IVR_EXECUTION_MODE: "MOCK",
  IVR_ENVIRONMENT_LABEL: "test",
  REAL_CUSTOMER_CALL_ALLOWED: "NO",
};

let server: ChildProcess | undefined;
let apiServer: Server | undefined;
let baseUrl = "";

function findFreePort(): Promise<number> {
  return new Promise((resolve, reject) => {
    const probe = createServer();
    probe.once("error", reject);
    probe.listen(0, "127.0.0.1", () => {
      const address = probe.address();
      if (address === null || typeof address === "string") {
        probe.close(() => reject(new Error("could not resolve a free port")));
        return;
      }

      const { port } = address;
      probe.close(() => resolve(port));
    });
  });
}

async function waitForServer(url: string, timeoutMs: number): Promise<void> {
  const deadline = Date.now() + timeoutMs;
  let lastError: unknown;

  while (Date.now() < deadline) {
    try {
      const response = await fetch(url, { redirect: "manual" });
      if (response.status > 0) {
        return;
      }
    } catch (error) {
      lastError = error;
    }

    await new Promise((resolve) => setTimeout(resolve, 400));
  }

  throw new Error(`server did not become ready: ${String(lastError)}`);
}

/** Minimal cookie jar: name -> value, honouring deletions. */
class CookieJar {
  private readonly cookies = new Map<string, string>();

  accept(response: Response): void {
    for (const header of response.headers.getSetCookie()) {
      const [pair, ...attributes] = header.split(";").map((part) => part.trim());
      const separator = pair.indexOf("=");
      const name = pair.slice(0, separator);
      const value = pair.slice(separator + 1);

      const expired = attributes.some(
        (attribute) =>
          attribute.toLowerCase() === "max-age=0" ||
          /^expires=.*197\d/i.test(attribute) ||
          value === "",
      );

      if (expired) {
        this.cookies.delete(name);
      } else {
        this.cookies.set(name, value);
      }
    }
  }

  header(): string {
    return [...this.cookies].map(([name, value]) => `${name}=${value}`).join("; ");
  }

  has(name: string): boolean {
    return this.cookies.has(name);
  }
}

function request(
  jar: CookieJar,
  path: string,
  init: RequestInit = {},
): Promise<Response> {
  const headers = new Headers(init.headers);
  const cookie = jar.header();
  if (cookie !== "") {
    headers.set("Cookie", cookie);
  }

  return fetch(`${baseUrl}${path}`, { ...init, headers, redirect: "manual" });
}

beforeAll(async () => {
  const apiPort = await findFreePort();
  apiServer = createHttpServer(async (request, response) => {
    if (await handleConsoleAuthStub(request, response)) return;
    response.writeHead(404, { "Content-Type": "application/json" });
    response.end(JSON.stringify({ error: { code: "IVR_NOT_FOUND", message: "not found", correlationId: "e2e" } }));
  });
  await new Promise<void>((resolve) => apiServer!.listen(apiPort, "127.0.0.1", resolve));

  // The app is built once by tests/e2e/global-setup.ts.
  const port = await findFreePort();
  baseUrl = `http://127.0.0.1:${port}`;
  server = spawn(process.execPath, [nextBin, "start", "--port", String(port)], {
    cwd: projectRoot,
    env: { ...serverEnv, IVR_API_BASE_URL: `http://127.0.0.1:${apiPort}` },
    stdio: "ignore",
  });

  await waitForServer(`${baseUrl}/login`, 60_000);
});

afterAll(async () => {
  server?.kill();
  await new Promise<void>((resolve) => apiServer?.close(() => resolve()) ?? resolve());
});

/** E2E-UI-AUTH-05 — unauthenticated redirect, sign-in, sign-out. */
describe("E2E-UI-AUTH-05 authentication flow", () => {
  it("redirects an unauthenticated request to the login page", async () => {
    const jar = new CookieJar();
    const response = await request(jar, "/dashboard");

    expect([302, 303, 307, 308]).toContain(response.status);
    const location = new URL(response.headers.get("Location") ?? "", baseUrl);
    expect(location.pathname).toBe("/login");
    expect(location.searchParams.get("next")).toBe("/dashboard");
  });

  it("serves the login page in Vietnamese with the governance notice", async () => {
    const jar = new CookieJar();
    const response = await request(jar, "/login");
    const html = await response.text();

    expect(response.status).toBe(200);
    expect(html).toContain("Đăng nhập");
    expect(html).toContain("REAL_CUSTOMER_CALL_ALLOWED=NO");
    expect(html).toContain('lang="vi"');
  });

  it("signs in, reaches the console, and signs back out", async () => {
    const jar = new CookieJar();

    const signIn = await request(jar, "/api/auth/sign-in", {
      method: "POST",
      body: new URLSearchParams({ ...Object.fromEntries(signInBody()), next: "/dashboard" }),
    });
    jar.accept(signIn);

    expect(signIn.status).toBe(303);
    expect(new URL(signIn.headers.get("Location") ?? "", baseUrl).pathname).toBe("/dashboard");

    const sessionCookie = signIn.headers
      .getSetCookie()
      .find((header) => header.startsWith("ivr_admin_session="));
    expect(sessionCookie).toBeDefined();
    expect(sessionCookie).toMatch(/HttpOnly/i);
    expect(sessionCookie).toMatch(/SameSite=Strict/i);
    expect(sessionCookie).toMatch(/Path=\//i);

    const dashboard = await request(jar, "/dashboard");
    const dashboardHtml = await dashboard.text();
    expect(dashboard.status).toBe(200);
    expect(dashboardHtml).toContain("Tổng quan vận hành IVR");
    expect(dashboardHtml).toContain(ADMIN_USERNAME);
    // The auth-only stub has no dashboard projection, so the typed 404 envelope
    // must render rather than an unhandled exception page.
    expect(dashboardHtml).toContain("IVR_NOT_FOUND");

    const loginWhileAuthenticated = await request(jar, "/login");
    expect([302, 303, 307, 308]).toContain(loginWhileAuthenticated.status);
    expect(
      new URL(loginWhileAuthenticated.headers.get("Location") ?? "", baseUrl).pathname,
    ).toBe("/dashboard");

    const signOut = await request(jar, "/api/auth/sign-out", { method: "POST" });
    jar.accept(signOut);

    expect(signOut.status).toBe(303);
    expect(new URL(signOut.headers.get("Location") ?? "", baseUrl).pathname).toBe("/login");
    expect(jar.has("ivr_admin_session")).toBe(false);

    const afterSignOut = await request(jar, "/dashboard");
    expect([302, 303, 307, 308]).toContain(afterSignOut.status);
    expect(new URL(afterSignOut.headers.get("Location") ?? "", baseUrl).pathname).toBe(
      "/login",
    );
  });

  it("rejects invalid credentials without issuing a session", async () => {
    const jar = new CookieJar();
    const response = await request(jar, "/api/auth/sign-in", {
      method: "POST",
      body: new URLSearchParams({ username: "ghost", password: "wrong-password" }),
    });
    jar.accept(response);

    expect(response.status).toBe(303);
    const location = new URL(response.headers.get("Location") ?? "", baseUrl);
    expect(location.pathname).toBe("/login");
    expect(location.searchParams.get("error")).toBe("invalidCredentials");
    expect(jar.has("ivr_admin_session")).toBe(false);
  });

  it("ignores an off-origin redirect target supplied at sign-in", async () => {
    const jar = new CookieJar();
    const response = await request(jar, "/api/auth/sign-in", {
      method: "POST",
      body: new URLSearchParams({ ...Object.fromEntries(signInBody()), next: "//evil.example/steal" }),
    });

    const location = new URL(response.headers.get("Location") ?? "", baseUrl);
    expect(location.origin).toBe(baseUrl);
    expect(location.pathname).toBe("/dashboard");
  });

  it("emits relative redirect locations so the origin cannot shift", async () => {
    // Regression guard: an absolute Location built from the Host header sends a
    // 127.0.0.1 visitor to localhost, which drops the SameSite=Strict cookie and
    // bounces them back to sign-in.
    const jar = new CookieJar();
    const responses = [
      await request(jar, "/dashboard"),
      await request(jar, "/api/auth/sign-in", {
        method: "POST",
        body: signInBody(),
      }),
      await request(jar, "/api/auth/sign-out", { method: "POST" }),
    ];

    for (const response of responses) {
      const location = response.headers.get("Location") ?? "";
      expect(location.startsWith("/"), `absolute Location: ${location}`).toBe(true);
    }
  });

  it("rejects a cross-site sign-in post", async () => {
    const response = await fetch(`${baseUrl}/api/auth/sign-in`, {
      method: "POST",
      redirect: "manual",
      headers: { "Sec-Fetch-Site": "cross-site" },
      body: signInBody(),
    });

    expect(response.status).toBe(403);
    expect(response.headers.getSetCookie()).toHaveLength(0);
  });

  it("rejects a forged session cookie", async () => {
    const response = await fetch(`${baseUrl}/queue`, {
      redirect: "manual",
      headers: {
        Cookie:
          "ivr_admin_session=eyJhY3RvcklkIjoiQUdULUFETUlOLTAxIn0.not-a-valid-signature",
      },
    });

    expect([302, 303, 307, 308]).toContain(response.status);
    expect(new URL(response.headers.get("Location") ?? "", baseUrl).pathname).toBe(
      "/login",
    );
  });
});
