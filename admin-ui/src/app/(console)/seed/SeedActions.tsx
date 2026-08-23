"use client";

import { AdminActionDialog } from "@/components/admin/AdminActionDialog";
import { ButtonGroup, Callout, SelectField } from "@/components/ui";
import { t } from "@/lib/i18n";

import {
  applyIntegrationProfileAction,
  dryRunScenarioAction,
  loadSeedAction,
} from "./actions";

export interface SeedActionsProps {
  readonly scenarioIds: readonly string[];
  readonly profileIds: readonly string[];
}

/**
 * UI-07 seed loader, scenario runner and integration-status profiles (W-0112).
 *
 * Grouped by what each one touches: the loader writes rows, the runner writes nothing at all,
 * and the profile moves SIM channels. Putting the runner beside the two that change state would
 * suggest it is the same kind of act — the one thing worth knowing about it is that it is not.
 */
export function SeedActions({ scenarioIds, profileIds }: SeedActionsProps) {
  return (
    <>
      <ButtonGroup label={t("seed.loaderTitle")}>
        <AdminActionDialog
          perm="IVR_DEV_TOOLING"
          label={t("seed.loadSeed")}
          description={t("seed.loadSeedDescription")}
          action={loadSeedAction}
        >
          <Callout tone="info" testId="seed-rebase-notice">
            {t("seed.rebaseNotice")}
          </Callout>
        </AdminActionDialog>
      </ButtonGroup>

      <ButtonGroup label={t("seed.runnerTitle")}>
        <AdminActionDialog
          perm="IVR_DEV_TOOLING"
          label={t("seed.dryRun")}
          description={t("seed.dryRunDescription")}
          action={dryRunScenarioAction}
        >
          <SelectField
            label={t("seed.scenarioLabel")}
            name="scenarioId"
            required
            options={scenarioIds.map((id) => ({ value: id, label: id }))}
          />
        </AdminActionDialog>
      </ButtonGroup>

      <ButtonGroup label={t("seed.profilesTitle")}>
        <AdminActionDialog
          perm="IVR_DEV_TOOLING"
          label={t("seed.applyProfile")}
          description={t("seed.applyProfileDescription")}
          action={applyIntegrationProfileAction}
        >
          {/* Said before the press, not discovered in the response. Four of the five
              dependencies in a profile are declared and never consulted, and an operator who
              believes otherwise has rehearsed a fail-closed path that does not run. */}
          <Callout tone="warning" role="alert" testId="seed-profile-partial">
            {t("seed.profilePartialWarning")}
          </Callout>
          <SelectField
            label={t("seed.profileLabel")}
            name="profileId"
            required
            options={profileIds.map((id) => ({ value: id, label: id }))}
          />
        </AdminActionDialog>
      </ButtonGroup>
    </>
  );
}
