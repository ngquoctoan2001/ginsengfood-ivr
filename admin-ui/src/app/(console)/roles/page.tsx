import {
  Callout,
  Card,
  CardStack,
  ChipList,
  DataTable,
  DescriptionList,
  PageHeader,
  type Column,
} from "@/components/ui";
import { getConsoleRoleMatrix, type ConsoleRole } from "@/lib/api/accounts";
import { requireAdmin } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatNumber, t, type MessageKey } from "@/lib/i18n";
import { IVR_PERMISSIONS, type IvrPermission } from "@/lib/rbac/permissions";

export const dynamic = "force-dynamic";

/**
 * W-0105 role and permission matrix, read from Ivr.Api.
 */
export default async function RolesPage() {
  const session = await requireAdmin();
  const matrix = (await getConsoleRoleMatrix({ session, config: readConfig() })).data;

  return (
    <>
      <PageHeader
        title={t("roles.title")}
        subtitle={t("roles.subtitle")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.roles") },
          ],
        }}
      />

      <Callout tone="info" testId="roles-not-managed-here">
        {t("roles.notManagedHere")}
      </Callout>

      <CardStack>
        <Card title={t("roles.currentSession")} accent>
          <DescriptionList
            items={[
              { label: t("auth.signedInAs"), value: session.actorId, mono: true },
              { label: t("auth.role"), value: session.role },
              {
                label: t("auth.permissionCount"),
                value: formatNumber(session.permissions.length),
              },
            ]}
          />
          <ChipList
            label={t("roles.colPermissions")}
            items={session.permissions.map((permission) => ({
              key: permission,
              label: permission,
              tone: "info" as const,
            }))}
          />
        </Card>

        <Card title={t("roles.directoryTitle")} flush>
          <DataTable
            label={t("roles.directoryTitle")}
            columns={ROLE_COLUMNS}
            rows={matrix.roles}
            rowKey={(entry) => entry.role}
            density="compact"
          />
        </Card>

        <Card title={t("roles.matrixTitle")} flush>
          <DataTable
            label={t("roles.matrixTitle")}
            columns={matrixColumns(matrix.roles)}
            rows={IVR_PERMISSIONS}
            rowKey={(permission) => permission}
            density="compact"
            pinFirstColumn
            zebra
          />
        </Card>
      </CardStack>
    </>
  );
}

const ROLE_COLUMNS: readonly Column<ConsoleRole>[] = [
  { key: "role", header: t("roles.colRole"), cell: (entry) => entry.label },
  {
    key: "permissions",
    header: t("roles.colPermissions"),
    variant: "wrap",
    cell: (entry) => entry.permissions.join(", "),
  },
];

function matrixColumns(roles: readonly ConsoleRole[]): readonly Column<IvrPermission>[] {
  return [
  {
    key: "permission",
    header: t("roles.colPermission"),
    variant: "mono",
    cell: (permission) => permission,
  },
  {
    key: "screen",
    header: t("roles.colScreen"),
    variant: "wrap",
    cell: (permission) => t(PERMISSION_SCREEN_KEYS[permission]),
  },
  {
    key: "roles",
    header: t("roles.colRole"),
    cell: (permission) => {
      const holders = roles.filter((entry) => entry.permissions.includes(permission));
      return holders.length === 0 ? (
        <span data-testid={`ungranted-${permission}`}>{t("roles.ungranted")}</span>
      ) : (
        holders.map((entry) => entry.label).join(", ")
      );
    },
  },
  ];
}

/**
 * Mirrors the permission-to-screen mapping in `specs/ui/08` §3.
 *
 * W-0039 / P5-5. The descriptions live in the message catalogue, not here. They are
 * operator-facing Vietnamese prose, and prose that sits in a component is prose a translator
 * never sees and a reviewer never diffs against the rest of the console's wording.
 */
const PERMISSION_SCREEN_KEYS: Readonly<Record<IvrPermission, MessageKey>> = {
  IVR_QUEUE_VIEW: "roles.screen.IVR_QUEUE_VIEW",
  IVR_QUEUE_PAUSE: "roles.screen.IVR_QUEUE_PAUSE",
  IVR_QUEUE_RESUME: "roles.screen.IVR_QUEUE_RESUME",
  IVR_SIM_ENABLE: "roles.screen.IVR_SIM_ENABLE",
  IVR_SIM_DISABLE: "roles.screen.IVR_SIM_DISABLE",
  IVR_MANUAL_RETRY: "roles.screen.IVR_MANUAL_RETRY",
  IVR_RESULT_REVIEW: "roles.screen.IVR_RESULT_REVIEW",
  IVR_FLAG_READ: "roles.screen.IVR_FLAG_READ",
  IVR_RUNTIME_GATE_ADMIN: "roles.screen.IVR_RUNTIME_GATE_ADMIN",
  IVR_ACCOUNT_VIEW: "roles.screen.IVR_ACCOUNT_VIEW",
  IVR_ACCOUNT_MANAGE: "roles.screen.IVR_ACCOUNT_MANAGE",
  IVR_ACCOUNT_PASSWORD_RESET: "roles.screen.IVR_ACCOUNT_PASSWORD_RESET",
  IVR_ACCOUNT_SELF_VIEW: "roles.screen.IVR_ACCOUNT_SELF_VIEW",
  IVR_SCRIPT_EDIT: "roles.screen.IVR_SCRIPT_EDIT",
  IVR_SCRIPT_REVIEW: "roles.screen.IVR_SCRIPT_REVIEW",
  IVR_SCRIPT_APPROVE_MOCK: "roles.screen.IVR_SCRIPT_APPROVE_MOCK",
  IVR_SCRIPT_APPROVE_LAB: "roles.screen.IVR_SCRIPT_APPROVE_LAB",
  IVR_SCRIPT_APPROVE_CONTENT: "roles.screen.IVR_SCRIPT_APPROVE_CONTENT",
  IVR_SCRIPT_APPROVE_PRIVACY_LEGAL: "roles.screen.IVR_SCRIPT_APPROVE_PRIVACY_LEGAL",
  IVR_SCRIPT_RETIRE: "roles.screen.IVR_SCRIPT_RETIRE",
  IVR_CALL_TERMINATE: "roles.screen.IVR_CALL_TERMINATE",
  IVR_DEV_TOOLING: "roles.screen.IVR_DEV_TOOLING",
};
