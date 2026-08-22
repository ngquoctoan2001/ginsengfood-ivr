import {
  Button,
  Callout,
  Card,
  CardStack,
  DataTable,
  LinkButton,
  PageHeader,
  SelectField,
  TextField,
  type Column,
} from "@/components/ui";
import { EnumLabel } from "@/components/data/EnumLabel";
import { listConsoleAccounts, type ConsoleAccount } from "@/lib/api/accounts";
import { IvrApiError } from "@/lib/api/errors";
import { requireAdmin } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, formatNumber, t } from "@/lib/i18n";

import { createAccountAction } from "./actions";
import { accountErrorHint } from "./error-hint";
import styles from "./accounts.module.css";

export const dynamic = "force-dynamic";

interface AccountsPageProps {
  readonly searchParams: Promise<Record<string, string | string[] | undefined>>;
}

export default async function AccountsPage({ searchParams }: AccountsPageProps) {
  const session = await requireAdmin();
  const query = await searchParams;
  const result = typeof query.result === "string" ? query.result : null;
  const queryError = typeof query.error === "string" ? query.error : null;
  // Opt-in, and read from the URL so the choice survives a reload and a shared link.
  const includeDeleted = query.include_deleted === "1";
  let accounts: readonly ConsoleAccount[] = [];
  let loadError: IvrApiError | null = null;

  try {
    accounts = (
      await listConsoleAccounts({ session, config: readConfig() }, 1, 50, includeDeleted)
    ).data.items;
  } catch (cause) {
    if (cause instanceof IvrApiError) {
      loadError = cause;
    } else {
      throw cause;
    }
  }

  return (
    <>
      <PageHeader
        title={t("accounts.title")}
        subtitle={t("accounts.subtitle")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.accounts") },
          ],
        }}
      />

      {result === null ? null : (
        <Callout tone="success" role="status">{t("accounts.resultPrefix")}: {result}.</Callout>
      )}
      {queryError === null ? null : (
        <Callout tone="danger" role="alert">
          {t("accounts.errorPrefix")}: {queryError}.
          {accountErrorHint(queryError) === null ? null : ` ${accountErrorHint(queryError)}`}
        </Callout>
      )}
      {loadError === null ? null : (
        <Callout tone="danger" role="alert">
          {t("accounts.loadErrorPrefix")}: {loadError.code} · {loadError.correlationId}
        </Callout>
      )}

      <CardStack>
        <Card
          title={t("accounts.listTitle")}
          description={`${formatNumber(accounts.length)} · ${t("accounts.listDescription")}`}
          flush
          accent
          actions={
            <LinkButton
              href={includeDeleted ? "/accounts" : "/accounts?include_deleted=1"}
              size="sm"
            >
              {includeDeleted ? t("accounts.hideDeleted") : t("accounts.showDeleted")}
            </LinkButton>
          }
        >
          <DataTable
            label={t("accounts.listLabel")}
            rows={accounts}
            columns={ACCOUNT_COLUMNS}
            rowKey={(account) => account.account_id}
            density="compact"
            zebra
            empty={<span>{t("accounts.empty")}</span>}
          />
        </Card>

        <Card title={t("accounts.newTitle")} description={t("accounts.newDescription")}>
          <form action={createAccountAction} className={styles.form}>
            <TextField label={t("accounts.username")} name="username" minLength={3} maxLength={64} required mono />
            <TextField label={t("accounts.displayName")} name="display_name" minLength={1} maxLength={128} required />
            <SelectField
              label={t("accounts.role")}
              name="role"
              options={[
                { value: "Operator", label: t("accounts.operator") },
                { value: "Admin", label: t("accounts.admin") },
              ]}
              defaultValue="Operator"
              required
            />
            <TextField
              label={t("accounts.initialPassword")}
              name="password"
              type="password"
              minLength={12}
              maxLength={128}
              autoComplete="new-password"
              required
              hint={t("accounts.passwordHint")}
            />
            <div className={styles.full}>
              <TextField label={t("accounts.reason")} name="reason" width="full" minLength={3} maxLength={500} required />
            </div>
            <div className={`${styles.actions} ${styles.full}`}>
              <Button type="submit" variant="primary">{t("accounts.create")}</Button>
            </div>
          </form>
        </Card>
      </CardStack>
    </>
  );
}

const ACCOUNT_COLUMNS: readonly Column<ConsoleAccount>[] = [
  { key: "username", header: t("accounts.username"), variant: "mono", cell: (row) => row.username },
  { key: "name", header: t("accounts.displayName"), cell: (row) => row.display_name },
  {
    key: "role",
    header: t("accounts.role"),
    cell: (row) => <EnumLabel family="accountRole" value={row.role} />,
  },
  {
    key: "status",
    header: t("accounts.status"),
    cell: (row) => <EnumLabel family="accountStatus" value={row.status} />,
  },
  { key: "lastLogin", header: t("accounts.lastLogin"), cell: (row) => row.last_login_at == null ? "—" : formatDateTime(row.last_login_at) },
  { key: "action", header: t("accounts.action"), cell: (row) => <LinkButton href={`/accounts/${row.account_id}`} size="sm">{t("accounts.open")}</LinkButton> },
];
