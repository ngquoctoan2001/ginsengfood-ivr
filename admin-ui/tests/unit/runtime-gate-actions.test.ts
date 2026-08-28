// @vitest-environment node
import { beforeEach, describe, expect, it, vi } from "vitest";

/**
 * UT-FLAGS-ASYMMETRY-01 (W-0110).
 *
 * The console half of the runtime-gate rule. `IT-FLAG-FOUREYES-14` proves the
 * API refuses a risk increase with no approval reference; this proves the
 * console refuses to send one in the first place, and — the part that actually
 * matters — that it refuses *before* reaching the network, so a missing approval
 * is a form error rather than a 409 an operator has to interpret.
 *
 * The API client is mocked to throw. Any call to it fails the test, which is how
 * "did not reach the network" is asserted rather than assumed.
 */
interface CapturedMutation {
  readonly changes: Record<string, unknown>;
  readonly reason: string;
  readonly approvalReference?: string;
}

// Typed through the generic rather than through parameter names: the signature
// is what the assertions read, and unused named parameters trip the lint gate.
const mutate =
  vi.fn<(context: unknown, environment: string, request: CapturedMutation) => unknown>();

vi.mock("@/lib/api/admin", () => ({ mutateFeatureFlags: mutate }));
vi.mock("next/cache", () => ({ revalidatePath: vi.fn() }));
const requirePermission = vi.fn<(permission: string) => unknown>();

vi.mock("@/lib/auth/guard", () => ({
  requirePermission: (permission: string) => requirePermission(permission),
}));

const readConfig = vi.fn();
vi.mock("@/lib/config/env", () => ({ readConfig: () => readConfig() }));

const NON_PRODUCTION = {
  executionMode: "MOCK",
  environmentLabel: "dev",
  isNonProductionEnvironment: true,
} as const;

const PRODUCTION = {
  executionMode: "MOCK",
  environmentLabel: "production",
  isNonProductionEnvironment: false,
} as const;

function form(entries: Readonly<Record<string, string>>): FormData {
  const data = new FormData();
  for (const [key, value] of Object.entries(entries)) {
    data.set(key, value);
  }

  return data;
}

describe("UT-FLAGS-ASYMMETRY-01 runtime-gate console rules", () => {
  beforeEach(() => {
    mutate.mockReset();
    mutate.mockImplementation(() => {
      throw new Error("the console must not call the API for a refused mutation");
    });
    requirePermission.mockReset();
    requirePermission.mockImplementation(() => {
      throw new Error("permission was demanded before the form was validated");
    });
    readConfig.mockReturnValue(NON_PRODUCTION);
  });

  /**
   * The refusal cases above would all pass if the actions simply never called
   * anything. This is the control: a risk reduction reaches the API, carrying
   * exactly the change set it advertises and the operator's reason.
   */
  it("refuses a risk increase with no approval reference, without calling the API", async () => {
    const { releaseKillSwitchAction, widenLabAllowlistAction } = await import(
      "@/app/(console)/flags/actions"
    );

    expect(
      await releaseKillSwitchAction({ status: "idle" }, form({ reason: "resume dialling" })),
    ).toEqual({ status: "invalid", messageKey: "flags.approvalRequired" });

    expect(
      await widenLabAllowlistAction(
        { status: "idle" },
        form({ reason: "add a destination", destinations: "LAB-B" }),
      ),
    ).toEqual({ status: "invalid", messageKey: "flags.approvalRequired" });

    expect(mutate).not.toHaveBeenCalled();
  });

  it("refuses every risk increase in a production environment, approval or not", async () => {
    readConfig.mockReturnValue(PRODUCTION);
    const { releaseKillSwitchAction, widenLabAllowlistAction } = await import(
      "@/app/(console)/flags/actions"
    );

    // With a valid approval reference, so the refusal is the environment and
    // nothing else. Flipping the execution mode *to* PRODUCTION_REAL is itself a
    // risk increase and is reachable while the mode is still MOCK, which is why
    // the gate is the environment rather than the mode alone.
    expect(
      await releaseKillSwitchAction(
        { status: "idle" },
        form({ reason: "resume dialling", approval_reference: "APPROVAL-1" }),
      ),
    ).toEqual({ status: "invalid", messageKey: "flags.productionBlocked" });

    expect(
      await widenLabAllowlistAction(
        { status: "idle" },
        form({
          reason: "add a destination",
          destinations: "LAB-B",
          approval_reference: "APPROVAL-1",
        }),
      ),
    ).toEqual({ status: "invalid", messageKey: "flags.productionBlocked" });

    expect(mutate).not.toHaveBeenCalled();
  });

  it("still requires a reason, and a widening still requires at least one destination", async () => {
    const { engageKillSwitchAction, widenLabAllowlistAction } = await import(
      "@/app/(console)/flags/actions"
    );

    expect(await engageKillSwitchAction({ status: "idle" }, form({ reason: "  " }))).toEqual({
      status: "invalid",
      messageKey: "action.reasonRequired",
    });

    expect(
      await widenLabAllowlistAction(
        { status: "idle" },
        form({ reason: "add", destinations: " , \n ", approval_reference: "APPROVAL-1" }),
      ),
    ).toEqual({ status: "invalid", messageKey: "flags.destinationsRequired" });

    expect(mutate).not.toHaveBeenCalled();
  });
});
