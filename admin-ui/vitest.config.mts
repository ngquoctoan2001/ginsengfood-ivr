import { fileURLToPath } from "node:url";

import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

export default defineConfig({
  plugins: [react()],
  resolve: {
    // Resolves the `@/*` paths declared in tsconfig.json.
    tsconfigPaths: true,
    alias: {
      // `server-only` throws on import unless the bundler applies the
      // `react-server` condition, which Vitest does not. The stub keeps the
      // marker inert under test while Next.js still enforces it at build time.
      "server-only": fileURLToPath(new URL("./tests/stubs/server-only.ts", import.meta.url)),
    },
  },
  test: {
    // Component tests run in jsdom. Node-only suites (API client, session,
    // contract drift, HTTP e2e) opt out with a `@vitest-environment node`
    // docblock so they never touch a browser shim.
    environment: "jsdom",
    setupFiles: ["./vitest.setup.ts"],
    // Builds the app once so parallel e2e suites do not race on `.next`.
    globalSetup: ["./tests/e2e/global-setup.ts"],
    include: ["tests/**/*.test.ts", "tests/**/*.test.tsx"],
    globals: false,
    // E2E-UI-AUTH-05 builds the app and boots `next start`; give it room.
    testTimeout: 30_000,
    hookTimeout: 300_000,
  },
});
