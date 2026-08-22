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
  IvrAnalyticsDataQuality,
  IvrSimChannel,
  IvrTechnicalRetryResult,
  TechnicalRetryRequest,
} from "@/lib/api/types";
import { IVR_ERROR_CODES } from "@/lib/api/types";
import { IVR_PERMISSIONS, IVR_ROLES } from "@/lib/rbac/permissions";

function repoFile(relativePath: string): string {
  return readFileSync(
    fileURLToPath(new URL(`../../../${relativePath}`, import.meta.url)),
    "utf8",
  );
}

interface OpenApiDocument {
  paths: Record<string, Record<string, unknown>>;
  components: {
    schemas: Record<
      string,
      { required?: string[]; enum?: string[]; properties?: Record<string, unknown> }
    >;
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
  it("mirrors the console error-code superset exactly", () => {
    expect([...IVR_ERROR_CODES].sort()).toEqual(
      [
        ...(openapi.components.schemas.ConsoleAccountErrorCode.enum as string[]),
      ].sort(),
    );
  });

  /**
   * `ConsoleAccountErrorCode` was introduced as a superset so the pre-existing endpoint response
   * enums would not have to change, and the assertion above moved onto it. That left `ErrorCode`
   * — still referenced by `ErrorEnvelope` and therefore by every operation written before the
   * console API — with no drift guard at all: a code could be added there and never reach the
   * TypeScript mirror, and nothing would fail.
   *
   * These two assertions restore the guard and pin the relationship between the enums, so the
   * split stays a deliberate superset rather than two catalogues drifting apart.
   */
  it("keeps every legacy ErrorCode in the TypeScript mirror", () => {
    const legacy = openapi.components.schemas.ErrorCode.enum as string[];
    expect(legacy.length).toBeGreaterThan(0);
    expect([...legacy].sort()).toEqual(
      [...legacy].filter((code) => (IVR_ERROR_CODES as readonly string[]).includes(code)).sort(),
    );
  });

  it("keeps ConsoleAccountErrorCode a strict superset of ErrorCode", () => {
    const legacy = new Set(openapi.components.schemas.ErrorCode.enum as string[]);
    const console = openapi.components.schemas.ConsoleAccountErrorCode.enum as string[];
    const missing = [...legacy].filter((code) => !console.includes(code));

    expect(missing).toEqual([]);
    expect(console.length).toBeGreaterThan(legacy.size);
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

  it("mirrors IvrSimChannel", () => {
    const sample: Required<
      Pick<
        IvrSimChannel,
        | "sim_channel_id"
        | "enabled"
        | "status"
        | "adapter_mode"
        | "provider_name"
        | "busy"
        | "fail_count"
        | "quarantined"
      >
    > = {
      sim_channel_id: "SIM-01",
      enabled: true,
      status: "IDLE",
      adapter_mode: "MOCK",
      provider_name: "MOCK",
      busy: false,
      fail_count: 0,
      quarantined: false,
    };

    expect(Object.keys(sample).sort()).toEqual(requiredOf("IvrSimChannel"));
  });

  it("mirrors IvrAnalyticsDataQuality", () => {
    const sample: Required<
      Pick<
        IvrAnalyticsDataQuality,
        | "generated_at"
        | "source"
        | "warehouse_backed"
        | "pipeline_work_id"
        | "status"
        | "min_bucket_size"
        | "suppressed_bucket_count"
        | "scanned_rows"
        | "truncated"
        | "warehouse_status"
      >
    > = {
      generated_at: "2026-08-15T00:00:00Z",
      source: "OPERATIONAL_READ_MODEL",
      warehouse_backed: false,
      pipeline_work_id: "W-0055",
      status: "FRESH",
      min_bucket_size: 5,
      suppressed_bucket_count: 0,
      scanned_rows: 0,
      truncated: false,
      warehouse_status: "NOT_RUN",
    };

    expect(Object.keys(sample).sort()).toEqual(requiredOf("IvrAnalyticsDataQuality"));
  });

  /**
   * Derived from the client source rather than hand-listed.
   *
   * The previous version of this assertion named three paths while the console
   * reached a dozen, so it passed no matter what was added. Extracting the
   * literals means a new client function is covered the moment it is written.
   */
  it("declares every admin path the UI can reach", () => {
    const reached = reachablePaths();
    const declared = new Set(Object.keys(openapi.paths).map(toPathPattern));

    expect(reached.length).toBeGreaterThanOrEqual(12);
    for (const path of reached) {
      expect(declared.has(path), `client calls ${path}, which OpenAPI does not declare`).toBe(true);
    }
  });

  /**
   * The privacy decisions taken in W-0095/0096/0098/0099 are contract-level, so
   * they are asserted against the contract. A future edit that adds a customer
   * field to one of these schemas fails here, not in review.
   */
  it("keeps customer identity out of the schemas the console reads", () => {
    // Exact names, not substrings: `invalid_phone` and `invalid_phone_rate` are
    // counts of results whose type is IVR_INVALID_PHONE_FINAL. They contain no
    // number, and a substring rule would flag them while missing a field that
    // spells the identity differently.
    const forbidden = new Set([
      "phone",
      "phone_masked",
      "phone_ref",
      "customer_phone",
      "sim_number_ref",
      "dial_token",
      "dial_token_ciphertext",
      "order_code",
      "official_order_id",
      "address",
      "full_address",
      "payment_method",
      "payment_method_snapshot",
      "member_tier",
      "health_note",
      "lease_token",
      "leased_by_worker_id",
      "lease_fencing_generation",
    ]);

    for (const schema of [
      "IvrSimChannel",
      "IvrSimChannelList",
      "IvrAnalyticsSummary",
      "IvrAnalyticsTrend",
      "IvrAnalyticsTrendBucket",
      "IvrAnalyticsBreakdown",
      "IvrAnalyticsBreakdownRow",
      "IvrAnalyticsExport",
      "IvrAnalyticsKpi",
      "IvrAnalyticsDataQuality",
    ]) {
      const properties = Object.keys(openapi.components.schemas[schema]?.properties ?? {});
      expect(properties.length, `schema ${schema} is missing`).toBeGreaterThan(0);
      for (const property of properties) {
        expect(
          forbidden.has(property),
          `${schema}.${property} is customer identity and must not be projected`,
        ).toBe(false);
      }
    }
  });
});

/** `/call-jobs/{ivrCallJobId}/detail` and `/call-jobs/*​/detail` compare equal. */
function toPathPattern(path: string): string {
  return path.replaceAll(/\{[^}]+\}/g, "*");
}

/**
 * Pulls the `path:` literals out of the API clients.
 *
 * A template literal is scanned rather than regex-replaced because
 * `${buildQuery({ a, b })}` contains braces of its own; the scanner tracks depth
 * and emits `*` for route interpolations. A `buildQuery(...)` interpolation is
 * dropped because it is not part of the OpenAPI path.
 */
function reachablePaths(): string[] {
  const sources = [
    repoFile("admin-ui/src/lib/api/admin.ts"),
    repoFile("admin-ui/src/lib/analytics/client.ts"),
    repoFile("admin-ui/src/lib/api/accounts.ts"),
  ].join("\n");

  const paths = new Set<string>();
  for (const match of sources.matchAll(/path:\s*"([^"]+)"/g)) {
    paths.add(match[1]);
  }

  for (const match of sources.matchAll(/path:\s*`([\s\S]*?)`,\n/g)) {
    paths.add(collapseInterpolations(match[1]));
  }

  return [...paths].map((path) => path.split("?", 1)[0]).sort();
}

function collapseInterpolations(template: string): string {
  let out = "";
  for (let index = 0; index < template.length; index++) {
    if (template[index] === "$" && template[index + 1] === "{") {
      let depth = 1;
      index += 2;
      const expressionStart = index;
      while (index < template.length && depth > 0) {
        if (template[index] === "{") depth++;
        else if (template[index] === "}") depth--;
        index++;
      }

      index--;
      const expression = template.slice(expressionStart, index);
      out += expression.includes("buildQuery(") ? "" : "*";
      continue;
    }

    out += template[index];
  }

  return out;
}

describe("RBAC vocabulary drift", () => {
  it("mirrors IvrPermissions.cs", () => {
    const source = repoFile("src/Ivr.Api/Auth/IvrPermissions.cs");
    const declared = [...source.matchAll(/public const string \w+ = "(IVR_[A-Z_]+)";/g)].map(
      (match) => match[1],
    );

    expect(declared.length).toBeGreaterThan(0);
    expect([...IVR_PERMISSIONS].sort()).toEqual([...declared].sort());
  });

  it("mirrors the two domain roles", () => {
    const source = repoFile("src/Ivr.Domain/Accounts/ConsoleAccountPolicies.cs");
    const declared = [...source.matchAll(/public const string \w+ = "(Admin|Operator)";/g)]
      .map((match) => match[1]);

    expect([...IVR_ROLES].sort()).toEqual([...declared].sort());
  });

  // OD-V1-20 was approved on 2026-08-22 and Admin now carries both runtime-flag permissions.
  // The assertion is inverted rather than deleted: the grant is a decision with a paper trail,
  // and a silent revert would otherwise look like a routine tidy-up in review.
  it("grants Admin the runtime-flag permissions per OD-V1-20", () => {
    const source = repoFile("src/Ivr.Api/Auth/IvrRoles.cs");
    expect(source).toContain("IvrPermissions.FlagRead");
    expect(source).toContain("IvrPermissions.RuntimeGateAdmin");
    expect(source).toContain("OD-V1-20");
  });

  it("keeps the runtime-flag permissions off Operator", () => {
    const source = repoFile("src/Ivr.Api/Auth/IvrRoles.cs");
    const operatorBlock = source.slice(source.indexOf("OperatorPermissions"));
    expect(operatorBlock).not.toContain("IvrPermissions.FlagRead");
    expect(operatorBlock).not.toContain("IvrPermissions.RuntimeGateAdmin");
  });
});
