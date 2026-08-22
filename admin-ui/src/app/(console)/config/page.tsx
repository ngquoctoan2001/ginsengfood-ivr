import { Suspense } from "react";

import { ErrorAlert, type ErrorEnvelopeView } from "@/components/feedback/ErrorAlert";
import { LoadingSkeleton } from "@/components/feedback/LoadingSkeleton";
import { BooleanCell } from "@/components/data/BooleanCell";
import { EnumLabel, EnumLabelList } from "@/components/data/EnumLabel";
import { StatusBadge } from "@/components/data/StatusBadge";
import {
  Callout,
  Card,
  CardStack,
  ChipList,
  DataTable,
  PageHeader,
  type Column,
} from "@/components/ui";
import { getScriptCatalog } from "@/lib/api/admin";
import { IvrApiError } from "@/lib/api/errors";
import type { IvrDtmfKey, IvrScriptCatalog, IvrScriptVersion } from "@/lib/api/types";
import { requireAdmin, requireSession } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, t } from "@/lib/i18n";

export const dynamic = "force-dynamic";

export default async function ScriptConfigPage() {
  await requireAdmin();
  return (
    <>
      <PageHeader
        title={t("config.title")}
        subtitle={t("config.subtitle")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.config") },
          ],
        }}
      />
      <Callout tone="locked" testId="config-read-only">
        {t("config.readOnlyNotice")}
      </Callout>
      <Suspense fallback={<LoadingSkeleton rows={6} variant="table" />}>
        <ScriptCatalogPanels />
      </Suspense>
    </>
  );
}

async function ScriptCatalogPanels() {
  const session = await requireSession();
  const config = readConfig();

  let catalog: IvrScriptCatalog | null = null;
  let error: ErrorEnvelopeView | null = null;

  try {
    catalog = (await getScriptCatalog({ session, config })).data;
  } catch (cause) {
    if (!(cause instanceof IvrApiError)) {
      throw cause;
    }

    error = cause.toEnvelope();
  }

  if (error !== null || catalog === null) {
    return <ErrorAlert error={error!} />;
  }

  return (
    <CardStack>
      <Callout
        tone={catalog.production_target_v1_fields_approved ? "success" : "locked"}
        testId="od-v1-15-lock"
      >
        {catalog.production_target_v1_fields_approved
          ? t("config.od15Open")
          : t("config.od15Locked")}
      </Callout>

      <Card title={t("config.versionsTitle")} flush={catalog.versions.length > 0}>
        {catalog.versions.length === 0 ? (
          <Callout tone="neutral">{t("config.noVersions")}</Callout>
        ) : (
          <DataTable
            label={t("config.versionsTitle")}
            columns={VERSION_COLUMNS}
            rows={catalog.versions}
            rowKey={(version) => `${version.template_id}:${version.version}`}
            density="compact"
            pinFirstColumn
          />
        )}
      </Card>

      <Card title={t("config.dtmfTitle")} footer={t("config.key9Notice")} flush>
        <DataTable
          label={t("config.dtmfTitle")}
          columns={DTMF_COLUMNS}
          rows={catalog.dtmf_map}
          rowKey={(key) => key.key}
          density="compact"
        />
      </Card>

      <Card title={t("config.allowedTitle")}>
        <ChipList
          label={t("config.allowedTitle")}
          items={catalog.allowed_input_fields.map((field) => ({
            key: field,
            label: field,
            tone: "success" as const,
          }))}
        />
      </Card>

      <Card title={t("config.prohibitedTitle")}>
        <ChipList
          label={t("config.prohibitedTitle")}
          items={catalog.prohibited_variables.map((variable) => ({
            key: variable,
            label: variable,
            tone: "danger" as const,
          }))}
        />
      </Card>
    </CardStack>
  );
}

const VERSION_COLUMNS: readonly Column<IvrScriptVersion>[] = [
  {
    key: "template",
    header: t("config.colTemplate"),
    variant: "mono",
    cell: (version) => version.template_id,
  },
  {
    key: "version",
    header: t("config.colVersion"),
    variant: "mono",
    cell: (version) => version.version,
  },
  {
    key: "status",
    header: t("config.colStatus"),
    cell: (version) => (
      <>
        <EnumLabel family="scriptStatus" value={version.status} />{" "}
        <StatusBadge
          tone={version.missing_approvals.length === 0 ? "success" : "warning"}
          testId={`approval-badge-${version.version}`}
        >
          {version.missing_approvals.length === 0
            ? t("config.approvedBadge")
            : t("config.notApprovedBadge")}
        </StatusBadge>
      </>
    ),
  },
  {
    key: "approvals",
    header: t("config.colApprovals"),
    variant: "wrap",
    cell: (version) => (
      <EnumLabelList
        family="approvalType"
        values={version.approvals.map((approval) => approval.approval_type)}
      />
    ),
  },
  {
    key: "missing",
    header: t("config.colMissing"),
    variant: "wrap",
    cell: (version) => (
      <EnumLabelList family="approvalType" values={version.missing_approvals} />
    ),
  },
  {
    key: "templateValid",
    header: t("config.colTemplateValid"),
    cell: (version) =>
      version.template_valid ? (
        <BooleanCell value={true} />
      ) : (
        <StatusBadge tone="danger">{t("config.templateInvalid")}</StatusBadge>
      ),
  },
  {
    key: "created",
    header: t("config.colCreated"),
    cell: (version) => formatDateTime(version.created_at),
  },
];

const DTMF_COLUMNS: readonly Column<IvrDtmfKey>[] = [
  { key: "key", header: t("config.dtmfKey"), variant: "mono", cell: (key) => key.key },
  {
    key: "meaning",
    header: t("config.dtmfMeaning"),
    variant: "wrap",
    cell: (key) => (
      <span data-testid={`dtmf-meaning-${key.key}`}>
        <EnumLabel family="dtmfMeaning" value={key.meaning} />
      </span>
    ),
  },
  {
    key: "enabled",
    header: t("config.dtmfEnabled"),
    cell: (key) => (
      <span data-testid={`dtmf-enabled-${key.key}`}>
        <BooleanCell value={key.enabled} />
      </span>
    ),
  },
];
