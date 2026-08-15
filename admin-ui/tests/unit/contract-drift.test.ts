// @vitest-environment node
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";

import { load } from "js-yaml";
import { describe, expect, it } from "vitest";

import type {
  AdminMutationRequest,
  AdminReviewRequest,
  IvrAdminActionResult,
  IvrAdminReviewResult,
  IvrQueueProjection,
  IvrTechnicalRetryResult,
  TechnicalRetryRequest,
} from "@/lib/api/types";
import { IVR_ERROR_CODES } from "@/lib/api/types";
import { MOCK_DIRECTORY } from "@/lib/auth/directory";
import { IVR_PERMISSIONS } from "@/lib/rbac/permissions";

function repoFile(relativePath: string): string {
  return readFileSync(
    fileURLToPath(new URL(`../../../${relativePath}`, import.meta.url)),
    "utf8",
  );
}

interface OpenApiDocument {
  paths: Record<string, Record<string, unknown>>;
  components: {
    schemas: Record<string, { required?: string[]; enum?: string[] }>;
  };
}

const openapi = load(
  repoFile("specs/api/openapi/ivr-order-confirmation.v1.yaml"),
) as OpenApiDocument;

function requiredOf(schema: string): string[] {
  const required = openapi.components.schemas[schema]?.required;
  expect(required, `schema ${schema} has no required list`).toBeDefined();
  return [...(required as string[])].sort();
}

/**
 * The admin UI hand-writes its request/response types because no TypeScript
 * generator is committed yet (P1-1 generates .NET only). These assertions are
 * what make "type-safe from OpenAPI" checkable rather than asserted: each sample
 * below is typed by the interface, so removing a field breaks compilation and
 * changing the contract breaks the comparison.
 */
describe("UT-UI-CONTRACT-06 OpenAPI drift", () => {
  it("mirrors the ErrorCode catalogue exactly", () => {
    expect([...IVR_ERROR_CODES].sort()).toEqual(
      [...(openapi.components.schemas.ErrorCode.enum as string[])].sort(),
    );
  });

  it("mirrors IvrQueueProjection", () => {
    const sample: IvrQueueProjection = {
      paused: false,
      pending_jobs: 0,
      active_attempts: 0,
      enabled_channels: 0,
      open_hold_incidents: 0,
      projected_at: "2026-08-15T00:00:00Z",
    };

    expect(Object.keys(sample).sort()).toEqual(requiredOf("IvrQueueProjection"));
  });

  it("mirrors AdminMutationRequest", () => {
    const sample: AdminMutationRequest = { reason: "capacity incident" };
    expect(Object.keys(sample).sort()).toEqual(requiredOf("AdminMutationRequest"));
  });

  it("mirrors IvrAdminActionResult", () => {
    const sample: IvrAdminActionResult = {
      admin_action_id: "A1",
      action_type: "QUEUE_PAUSE",
      target_type: "QUEUE",
      target_id: "default",
      status: "APPLIED",
      correlation_id: "ui-0000",
      no_policy_bypass: true,
    };

    expect(Object.keys(sample).sort()).toEqual(requiredOf("IvrAdminActionResult"));
  });

  it("mirrors TechnicalRetryRequest and IvrTechnicalRetryResult", () => {
    const request: TechnicalRetryRequest = {
      technical_exception_id: "TE1",
      target_attempt_id: "AT1",
      reason: "provider timeout",
    };
    const result: IvrTechnicalRetryResult = {
      admin_action_id: "A1",
      technical_exception_id: "TE1",
      target_attempt_id: "AT1",
      technical_retry_count: 1,
      customer_attempt_counted: false,
      queue_status: "HELD_MOCK",
      no_policy_bypass: true,
    };

    expect(Object.keys(request).sort()).toEqual(requiredOf("TechnicalRetryRequest"));
    expect(Object.keys(result).sort()).toEqual(requiredOf("IvrTechnicalRetryResult"));
  });

  it("mirrors AdminReviewRequest and IvrAdminReviewResult", () => {
    const request: AdminReviewRequest = {
      review_item_id: "RI1",
      resolution: "ANNOTATED",
      reason: "verified against evidence",
    };
    const result: IvrAdminReviewResult = {
      admin_action_id: "A1",
      review_item_id: "RI1",
      status: "RESOLVED",
      resolution: "ANNOTATED",
      result_unchanged: true,
      no_policy_bypass: true,
    };

    expect(Object.keys(request).sort()).toEqual(requiredOf("AdminReviewRequest"));
    expect(Object.keys(result).sort()).toEqual(requiredOf("IvrAdminReviewResult"));
  });

  it("declares every admin path the UI can reach", () => {
    for (const path of ["/queue", "/queue:pause", "/queue:resume"]) {
      expect(openapi.paths[path], `missing OpenAPI path ${path}`).toBeDefined();
    }
  });
});

describe("RBAC vocabulary drift", () => {
  it("mirrors IvrPermissions.cs", () => {
    const source = repoFile("src/Ivr.Api/Auth/IvrPermissions.cs");
    const declared = [...source.matchAll(/public const string \w+ = "(IVR_[A-Z_]+)";/g)].map(
      (match) => match[1],
    );

    expect(declared.length).toBeGreaterThan(0);
    expect([...IVR_PERMISSIONS].sort()).toEqual([...declared].sort());
  });

  it("mirrors the seeded agent directory", () => {
    const seed = JSON.parse(repoFile("seed/agents.sample.json")) as {
      agents: { actor_id: string; role: string; permissions: string[] }[];
    };

    expect(
      MOCK_DIRECTORY.map((entry) => ({
        actor_id: entry.actorId,
        role: entry.role,
        permissions: [...entry.permissions].sort(),
      })),
    ).toEqual(
      seed.agents.map((agent) => ({
        actor_id: agent.actor_id,
        role: agent.role,
        permissions: [...agent.permissions].sort(),
      })),
    );
  });

  it("grants no seeded role the runtime-gate permission pending OD-V1-20", () => {
    for (const entry of MOCK_DIRECTORY) {
      expect(entry.permissions).not.toContain("IVR_RUNTIME_GATE_ADMIN");
    }
  });
});
