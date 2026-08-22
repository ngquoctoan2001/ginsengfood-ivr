// @vitest-environment node
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

import vi from "../../src/i18n/vi.json";
import enums from "../../src/i18n/enums.vi.json";
import { enumFamilyValues, resetUntranslatedCounts, tEnum, untranslatedCounts } from "../../src/lib/i18n/enum";

function repoFile(relativePath: string): string {
  return readFileSync(fileURLToPath(new URL(`../../../${relativePath}`, import.meta.url)), "utf8");
}

/**
 * Every enum the OpenAPI spec declares, wherever it is declared.
 *
 * The spec writes enums three ways and all three are in use, so the scan reads
 * the text rather than walking the parsed tree: an inline
 * `decision: { type: string, enum: [A, B] }`, an `enum: [A, B]` on its own line
 * under the property, and a `-` list. A checker that understood only one shape
 * would report full coverage while silently skipping two thirds of the spec —
 * which is exactly the failure mode this test exists to prevent.
 */
interface SpecEnum {
  readonly owner: string;
  readonly line: number;
  readonly values: readonly string[];
}

function collectSpecEnums(): readonly SpecEnum[] {
  const lines = repoFile("specs/api/openapi/ivr-order-confirmation.v1.yaml").split("\n");
  const found: SpecEnum[] = [];

  function ownerAbove(index: number): string {
    for (let k = index - 1; k >= 0 && k > index - 12; k -= 1) {
      const named = /^\s*([A-Za-z_][A-Za-z0-9_]*):\s*$/u.exec(lines[k]);
      if (named !== null) {
        return named[1];
      }
    }

    return "(unknown)";
  }

  function parameterNameAbove(index: number): string {
    for (let k = index - 1; k >= 0 && k > index - 8; k -= 1) {
      const named = /^\s*(?:-\s*)?name:\s*([A-Za-z_][A-Za-z0-9_-]*)\s*$/u.exec(lines[k]);
      if (named !== null) {
        return named[1];
      }
    }

    return "(unknown)";
  }

  for (let i = 0; i < lines.length; i += 1) {
    const inline = /^\s*([a-z_]+):\s*\{[^}]*enum:\s*\[([^\]]+)\]/u.exec(lines[i]);
    if (inline !== null) {
      // A query/header parameter writes its constraint as `schema: { enum: [...] }`,
      // so the property name on the line is the useless word "schema". The name
      // that identifies it sits a few lines above as `- name: program`.
      found.push({
        owner: inline[1] === "schema" ? parameterNameAbove(i) : inline[1],
        line: i + 1,
        values: splitList(inline[2]),
      });
      continue;
    }

    const ownLine = /^\s*enum:\s*\[([^\]]+)\]\s*$/u.exec(lines[i]);
    if (ownLine !== null) {
      found.push({ owner: ownerAbove(i), line: i + 1, values: splitList(ownLine[1]) });
      continue;
    }

    if (/^\s*enum:\s*$/u.test(lines[i])) {
      const values: string[] = [];
      let j = i + 1;
      while (j < lines.length) {
        const item = /^\s*-\s+(\S+)\s*$/u.exec(lines[j]);
        if (item === null) {
          break;
        }

        values.push(item[1]);
        j += 1;
      }

      if (values.length > 0) {
        found.push({ owner: ownerAbove(i), line: i + 1, values });
      }

      i = j - 1;
    }
  }

  return found;
}

function splitList(raw: string): readonly string[] {
  return raw
    .split(",")
    .map((value) => value.trim())
    .filter((value) => value !== "");
}

/**
 * Which spec enum the console renders, and which dictionary answers for it.
 *
 * An entry here is a claim that operators read these values on a screen. The
 * exemption list below is the other half of the same claim — the values that
 * reach a screen but must NOT be translated, each with the reason.
 */
/**
 * Several property names are reused across unrelated schemas — `decision` names
 * both the per-line sellable verdict and the eligibility outcome, `status` names
 * data freshness and the admin-action result. So an owner maps to the set of
 * families that could answer for it, and the enum is covered when any one of
 * them holds every value.
 *
 * Keyed by name rather than by line: line numbers move whenever the spec is
 * edited, and a coverage test that has to be renumbered after every unrelated
 * change gets deleted rather than maintained.
 */
const COVERED: Readonly<Record<string, readonly (keyof typeof enums)[]>> = {
  ResultType: ["resultType"],
  decision: ["sellableDecision", "eligibilityDecision"],
  payment_method_snapshot: ["paymentMethod"],
  state: ["dependencyState"],
  status: ["freshnessStatus", "adminActionStatus"],
  warehouse_status: ["warehouseStatus"],
  voice_region: ["voiceRegion"],
  bucket: ["bucket"],
  dimension: ["analyticsDimension"],
  program: ["programType"],
  ProgramCode: ["programType"],
  ConsoleRole: ["accountRole"],
  ConsoleAccountStatus: ["accountStatus"],
  result_type: ["resultType"],
};

/**
 * Spec enums the console deliberately does not translate.
 *
 * NT-3 is the load-bearing one. `order_state` is typed `string` with the
 * description "Opaque enum owned by Order Core" — IVR does not know the value
 * set and inventing a dictionary for it would be fabricating meaning it has no
 * authority over (D-02). It is listed here so that a future reader finds a
 * decision rather than what looks like an oversight and "fixes" it.
 */
const EXEMPT: Readonly<Record<string, string>> = {
  "sales-platform": "wire header value, never rendered",
  "ivr-worker": "wire header value, never rendered",
  "ivr.internal.write": "auth scope, never rendered",
  Bearer: "auth scheme, never rendered",
  VND: "currency code — formatCurrencyVnd renders the symbol",
  "vi-VN": "locale tag, never rendered",
  "ivr-order-confirmation.v1": "contract id, never rendered",
  environment: "deployment target, shown as-is in ops tooling",
  ErrorCode: "translated through vi.json error.* keys, not the enum dictionary",
  ConsoleAccountErrorCode: "translated through vi.json error.* keys",
  order_state: "NT-3 — opaque enum owned by Order Core (D-02)",
  ivr_confirmation_required: "boolean literal constraint, not a value set",
  input_signal_only: "boolean literal constraint",
  no_direct_order_update: "boolean literal constraint",
  no_payment_or_revenue_effect: "boolean literal constraint",
  requires_core_revalidation: "boolean literal constraint",
  no_policy_bypass: "boolean literal constraint",
  customer_attempt_counted: "boolean literal constraint",
  token_type: "auth scheme, never rendered",
  "X-Source-System": "wire header, never rendered",
  "X-Service-Scope": "wire header, never rendered",
};

describe("UT-L10N-COVER-03 every rendered enum value has a Vietnamese label", () => {
  it("covers each OpenAPI enum the console renders", () => {
    const missing: string[] = [];

    for (const specEnum of collectSpecEnums()) {
      const families = COVERED[specEnum.owner];
      if (families === undefined) {
        // Not claimed as rendered — it must then be claimed as exempt, either by
        // owner name or by being a single-value literal constraint.
        const exemptByOwner = Object.hasOwn(EXEMPT, specEnum.owner);
        const exemptByValue = specEnum.values.every((value) => Object.hasOwn(EXEMPT, value));
        const literalConstraint =
          specEnum.values.length === 1 && /^(true|false)$/u.test(specEnum.values[0]);

        if (!exemptByOwner && !exemptByValue && !literalConstraint) {
          missing.push(
            `spec enum \`${specEnum.owner}\` (line ${specEnum.line}) is neither in COVERED nor EXEMPT: ` +
              `${specEnum.values.join(", ")}`,
          );
        }

        continue;
      }

      const covered = families.some((family) => {
        const known = new Set(enumFamilyValues(family));
        return specEnum.values.every((value) => known.has(value));
      });

      if (!covered) {
        missing.push(
          `no family in [${families.join(", ")}] covers every value of \`${specEnum.owner}\` ` +
            `(spec line ${specEnum.line}): ${specEnum.values.join(", ")}`,
        );
      }
    }

    expect(missing).toEqual([]);
  });

  it("keeps a value in exactly one catalogue, never in both", () => {
    // Two copies of the same translation drift apart, and the one that drifts is
    // whichever the reader is not looking at. UT-L10N-NODUP-06.
    //
    // `error.*` is excluded, and it is not a loophole. A handful of tokens are
    // deliberately in both catalogues because the API uses them on two different
    // axes: `IVR_OPERATIONAL_BLOCKED` is an API rejection code AND a call result
    // type, and the spec says so — ResultType's own description notes IVR never
    // emits it as a call result because it is a pre-call decision. They need
    // different words ("Đang có blocker vận hành." vs "Bị chặn do vận hành"), so
    // collapsing them into one entry would make one of the two screens lie.
    const interfaceKeys = Object.keys(vi).filter((key) => !key.startsWith("error."));
    const duplicated: string[] = [];

    for (const [family, table] of Object.entries(enums)) {
      for (const value of Object.keys(table)) {
        const collisions = interfaceKeys.filter((key) => key.endsWith(`.${value}`));
        for (const collision of collisions) {
          duplicated.push(`${family}.${value} also lives in vi.json as ${collision}`);
        }
      }
    }

    expect(duplicated).toEqual([]);
  });

  it("has no empty family and no blank label", () => {
    const broken: string[] = [];
    for (const [family, table] of Object.entries(enums)) {
      const entries = Object.entries(table);
      if (entries.length === 0) {
        broken.push(`${family} is empty`);
      }

      for (const [value, label] of entries) {
        if (label.trim() === "") {
          broken.push(`${family}.${value} has a blank label`);
        }

        if (label === value) {
          broken.push(`${family}.${value} was never translated — the label repeats the code`);
        }
      }
    }

    expect(broken).toEqual([]);
  });
});

describe("UT-L10N-ENUM-01 an unknown value is reported, never blanked", () => {
  it("returns the raw code and marks it unknown", () => {
    resetUntranslatedCounts();

    const resolved = tEnum("resultType", "IVR_SOMETHING_INVENTED_IN_2027");

    // NT-4. The screen must be able to say "I have no word for this" out loud.
    // A blank cell reads as absent data to whoever is on shift, which is a
    // different and much worse claim than "untranslated".
    expect(resolved).toEqual({
      label: "IVR_SOMETHING_INVENTED_IN_2027",
      code: "IVR_SOMETHING_INVENTED_IN_2027",
      known: false,
    });
    expect(untranslatedCounts()).toEqual({ "resultType.IVR_SOMETHING_INVENTED_IN_2027": 1 });
  });

  it("separates absent from untranslated", () => {
    expect(tEnum("resultType", undefined)).toBeNull();
    expect(tEnum("resultType", null)).toBeNull();
    expect(tEnum("resultType", "")).toBeNull();
  });

  it("resolves a known value without counting it", () => {
    resetUntranslatedCounts();

    expect(tEnum("resultType", "IVR_CONFIRMED")).toEqual({
      label: "Khách đã xác nhận",
      code: "IVR_CONFIRMED",
      known: true,
    });
    expect(untranslatedCounts()).toEqual({});
  });
});
