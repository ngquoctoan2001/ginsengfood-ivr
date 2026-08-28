// @vitest-environment node
import { readFileSync, readdirSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

import vi from "../../src/i18n/vi.json";
import enums from "../../src/i18n/enums.vi.json";
import { enumFamilyValues, resetUntranslatedCounts, tEnum, untranslatedCounts } from "../../src/lib/i18n/enum";

function repoFile(relativePath: string): string {
  return readFileSync(fileURLToPath(new URL(`../../../${relativePath}`, import.meta.url)), "utf8");
}

/** Every `.cs` file under `src/`, so a new writer in a new file cannot escape the sweep below. */
function csharpSources(): { path: string; text: string }[] {
  const root = fileURLToPath(new URL("../../../src", import.meta.url));
  const found: { path: string; text: string }[] = [];

  const walk = (directory: string) => {
    for (const entry of readdirSync(directory, { withFileTypes: true })) {
      const full = path.join(directory, entry.name);
      if (entry.isDirectory()) {
        // Generated migrations restate every historical value as EF model snapshots. Including
        // them would assert labels for taxonomies that were replaced two migrations ago.
        if (entry.name !== "Migrations" && entry.name !== "obj" && entry.name !== "bin") {
          walk(full);
        }
      } else if (entry.name.endsWith(".cs")) {
        found.push({ path: full, text: readFileSync(full, "utf8") });
      }
    }
  };

  walk(root);
  return found;
}

/**
 * Locates a declaration by name, on a word boundary.
 *
 * `indexOf` is not enough and the difference is not academic: a plain `indexOf("class Foo")`
 * still matches after the class is renamed to `FooRenamed`, so the parser reads on happily and
 * the whole check stays green through exactly the rename it is supposed to notice.
 */
function declarationStart(source: string, keyword: string, name: string): number {
  const found = new RegExp(`${keyword}\\s+${name}\\b`, "u").exec(source);
  return found === null ? -1 : found.index;
}

/** `public const string Whatever = "VALUE";` inside one class body. */
function csharpConstants(relativePath: string, className: string): string[] {
  const source = repoFile(relativePath);
  const start = declarationStart(source, "class", className);
  if (start < 0) {
    throw new Error(`${className} no longer exists in ${relativePath}.`);
  }

  const opened = source.indexOf("{", start);
  const closed = source.indexOf("\n}", opened);
  const body = source.slice(opened, closed < 0 ? undefined : closed);

  return [...body.matchAll(/const\s+string\s+\w+\s*=\s*"([^"]+)"/gu)].map((match) => match[1]);
}

/** The string literals of one collection initializer, e.g. a `HashSet<string>` whitelist. */
function csharpLiteralSet(relativePath: string, fieldName: string): string[] {
  const source = repoFile(relativePath);
  const found = new RegExp(`\\b${fieldName}\\b`, "u").exec(source);
  const start = found === null ? -1 : found.index;
  if (start < 0) {
    throw new Error(`${fieldName} no longer exists in ${relativePath}.`);
  }

  const opened = source.indexOf("[", start);
  const closed = source.indexOf("]", opened);
  if (opened < 0 || closed < 0) {
    throw new Error(`${fieldName} in ${relativePath} is no longer a bracketed initializer.`);
  }

  return [...source.slice(opened, closed).matchAll(/"([A-Z][A-Z0-9_]+)"/gu)].map((m) => m[1]);
}

/** Every literal assigned to `<anything>.<propertyName> = "VALUE"` across `src/`. */
function csharpAssignedLiterals(propertyName: string): string[] {
  const pattern = new RegExp(`${propertyName}\\s*=\\s*"([A-Z][A-Z0-9_]+)"`, "gu");
  return csharpSources().flatMap((file) => [...file.text.matchAll(pattern)].map((m) => m[1]));
}

/**
 * Every value `CallAttemptEntity.Status` can hold.
 *
 * Unlike the other two, this taxonomy has no constants class — the values are string literals
 * spread across the scheduler, the dispatch store, the normalizer and the admin service, which is
 * precisely how it drifted out of the dictionary unnoticed. Until they are collected into one
 * class, the sweep has to go and find them.
 *
 * The ternary in `ResultRepository.ApplyAttemptOutcome` assigns four values from a single
 * `attempt.Status =`, so the scan takes the literals from the whole statement rather than the
 * first line of it.
 */
function attemptStatusLiterals(): string[] {
  const found = new Set<string>();

  for (const file of csharpSources()) {
    // `=(?!=)` — an assignment, never a comparison. Without the lookahead this collects the
    // right-hand side of `attempt.Status == "DIALING"` too, and a value that is only ever read
    // would be asserted as one the backend produces. That is how the four impossible entries got
    // into `deliveryStatus` in the first place, and a checker that reintroduces the bug it was
    // written to catch is worse than none.
    for (const match of file.text.matchAll(/attempt\.Status\s*=(?!=)\s*([\s\S]{0,400}?);/gu)) {
      for (const literal of match[1].matchAll(/"([A-Z][A-Z0-9_]+)"/gu)) {
        found.add(literal[1]);
      }
    }

    // The row is created with its opening status inside an object initializer, not an assignment.
    for (const block of file.text.matchAll(/new CallAttemptEntity[\s\S]{0,1200}?\n\s*\}/gu)) {
      for (const literal of block[0].matchAll(/Status\s*=\s*"([A-Z][A-Z0-9_]+)"/gu)) {
        found.add(literal[1]);
      }
    }
  }

  return [...found];
}

/** The member names of one C# enum, in declaration order. */
function csharpEnumMembers(relativePath: string, enumName: string): string[] {
  const source = repoFile(relativePath);
  const start = declarationStart(source, "enum", enumName);
  if (start < 0) {
    throw new Error(`${enumName} no longer exists in ${relativePath}.`);
  }

  const opened = source.indexOf("{", start);
  const closed = source.indexOf("}", opened);
  return source
    .slice(opened + 1, closed)
    .split(",")
    .map((member) => member.replace(/\/\/.*$/gmu, "").trim())
    .filter((member) => /^[A-Za-z_]\w*$/u.test(member));
}

/**
 * Every value `CallAttemptEntity.Disposition` can hold.
 *
 * The column is not the enum, and the difference is the whole point of this parser.
 * `PostgresTelephonyDispatchStore` writes `disposition.ToString().ToUpperInvariant()`, so
 * `RingTimeout` reaches the console as `RINGTIMEOUT` — uppercased, with no underscore inserted.
 *
 * W-0107 keyed this family off the C# member names it read in `DispositionMapper` instead, so all
 * eleven labels were unreachable and every attempt row on the call-detail screen rendered a raw
 * code behind a ⚠. Nothing caught it: the field is an open `string` in the spec, so the sweep
 * above had nothing to collect, and this test did not name the family. The transform is applied
 * here rather than restated as a literal list precisely so the next reader cannot repeat the
 * mistake by eye.
 */
function dispositionLiterals(): string[] {
  return [
    ...csharpEnumMembers("src/Ivr.Domain/Ports/ProviderPorts.cs", "SimProviderDisposition").map(
      (member) => member.toUpperCase(),
    ),
    // `ResultRepository.ApplyAttemptOutcome` overwrites the column on the capacity path, and that
    // value is not a `SimProviderDisposition` in any casing.
    "CAPACITY_EXCEPTION",
  ];
}

/** Every technical code this repo's own provider adapter can raise. */
function providerTechnicalCodes(): string[] {
  const found = new Set<string>();

  for (const file of csharpSources()) {
    for (const match of file.text.matchAll(
      /AsteriskAriOperationException\([\s\S]{0,240}?"([A-Z][A-Z0-9_]+)"/gu,
    )) {
      found.add(match[1]);
    }
  }

  return [...found];
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
 * the eligibility outcome and the script outcome, `status` names
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
  decision: ["eligibilityDecision"],
  payment_method_snapshot: ["paymentMethod"],
  state: ["dependencyState"],
  status: ["freshnessStatus", "adminActionStatus", "scriptStatus"],
  warehouse_status: ["warehouseStatus"],
  voice_region: ["voiceRegion"],
  bucket: ["bucket"],
  dimension: ["analyticsDimension"],
  program: ["programType"],
  ProgramCode: ["programType"],
  ConsoleRole: ["accountRole"],
  ConsoleAccountStatus: ["accountStatus"],
  result_type: ["resultType"],

  // W-0109 script lifecycle. `items` is the element enum of `approved_for_modes`,
  // which the spec names by its array item rather than by the field.
  approval_type: ["approvalType"],
  items: ["executionMode"],

  // W-0112. The scenario runner reports whether it could answer for a scenario at all, and that
  // answer is shown to whoever is running the rehearsal — so it is translated rather than
  // exempted as an internal discriminator.
  coverage: ["scenarioCoverage"],

  // W-0113. Whether a region was recorded or re-derived is shown to whoever is deciding
  // whether the number in front of them can be signed, so it is translated, not exempted.
  voice_region_source: ["voiceRegionSource"],
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
  target_type: "audit target discriminator — one literal, rendered as a fixed label",
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

  /**
   * The spec sweep above has one blind spot, and W-0107 shipped straight through it.
   *
   * It can only enumerate what the OpenAPI document enumerates. Three fields the call-detail
   * screen renders are declared `{ type: string }` there — an open string with no `enum:` — so
   * there is nothing to collect, the sweep reports full coverage, and the runtime fills them from
   * C# anyway. A live capture found all three showing raw codes on the busiest screen in the
   * console: the attempt timeline, the callback delivery row and the eligibility decision.
   *
   * So this reads the authorities themselves. Every parser below throws when it finds nothing,
   * because a renamed class that silently yields an empty set would turn this test green in
   * exactly the situation it exists for.
   *
   * The direction is deliberate and one-way: every value the backend can produce must have a
   * label. The converse is not asserted — a family may legitimately hold spec values the runtime
   * has not emitted yet, and failing on those would punish being early rather than being wrong.
   */
  it("covers every enum value the C# authorities can produce", () => {
    const missing: string[] = [];

    const check = (family: keyof typeof enums, values: readonly string[], authority: string) => {
      expect(values.length, `no values parsed from ${authority}`).toBeGreaterThan(0);
      for (const value of values) {
        if (enums[family][value as keyof (typeof enums)[typeof family]] === undefined) {
          missing.push(`${family}.${value} — produced by ${authority}`);
        }
      }
    };

    check(
      "eligibilityDecision",
      csharpConstants("src/Ivr.Domain/Policies/EligibilityRules.cs", "EligibilityDecisions"),
      "EligibilityDecisions",
    );

    // READY and SENDING are written directly rather than through the whitelist, so the authority
    // for this field is the union of the three writers, not the whitelist alone.
    check(
      "deliveryStatus",
      [
        ...csharpLiteralSet(
          "src/Ivr.Infrastructure/Persistence/Outbox/CallbackOutboxRepository.cs",
          "AllowedDeliveryStatuses",
        ),
        ...csharpAssignedLiterals("DeliveryStatus"),
      ],
      "AllowedDeliveryStatuses + direct DeliveryStatus writers",
    );

    check("attemptStatus", attemptStatusLiterals(), "every writer of CallAttemptEntity.Status");

    check(
      "disposition",
      dispositionLiterals(),
      "SimProviderDisposition uppercased as PostgresTelephonyDispatchStore writes it",
    );

    /**
     * `ReviewItemEntity.Reason` is a union, not a taxonomy, and that is why it drifted.
     *
     * Five writers fill the column and W-0107 wrote the dictionary for one of them — §5.9 is even
     * titled "họ CALLBACK_* / CAPACITY_*", so the other four were never in scope rather than
     * overlooked. `ResultRepository` puts `NormalizedResult.Reason` there, `EligibilityRepository`
     * puts `Reasons[0].Code` there, and both sets were absent entirely.
     *
     * The coupling to `resultReason` and `technicalExceptionType` is deliberate: those two
     * families ARE the closed part of `NormalizedResult.Reason`, so requiring the review queue to
     * speak every word the result screen speaks is the same claim, stated where it can be checked.
     *
     * This deliberately over-covers. `HumanReviewRequired` is false for about half of these, so
     * `CUSTOMER_PRESSED_1` will never open a review item — but modelling that condition would mean
     * keeping a second copy of `DispositionMapper`'s branching here, and a checker that has to be
     * re-derived every time the mapper moves is one that goes stale silently. A spare label costs
     * a line of JSON; a missing one costs an operator reading a raw code mid-incident.
     */
    check(
      "reviewReason",
      [
        ...csharpConstants("src/Ivr.Domain/Policies/EligibilityRules.cs", "EligibilityReasonCodes"),
        ...csharpConstants("src/Ivr.Domain/Policies/OptOutSuppression.cs", "OptOutReasonCodes"),
        ...enumFamilyValues("resultReason"),
        ...enumFamilyValues("technicalExceptionType"),
        ...providerTechnicalCodes(),
      ],
      "EligibilityReasonCodes + OptOutReasonCodes + NormalizedResult.Reason + provider codes",
    );

    /**
     * `SuppressionChannel` reaches the console appended to a reason rather than in a field of its
     * own, so `ReviewReason` splits it out and looks it up here. Same uppercasing trap as
     * `disposition`: `SuppressionProposer` writes `Channel.ToString().ToUpperInvariant()`, so
     * `PhoneCall` arrives as `PHONECALL`, and the transform is applied rather than transcribed.
     */
    check(
      "suppressionChannel",
      csharpEnumMembers("src/Ivr.Domain/Policies/OptOutSuppression.cs", "SuppressionChannel").map(
        (member) => member.toUpperCase(),
      ),
      "SuppressionChannel uppercased as SuppressionProposer writes it",
    );

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
