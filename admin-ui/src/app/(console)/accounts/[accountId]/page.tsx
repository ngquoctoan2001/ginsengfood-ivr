import { notFound } from "next/navigation";

import {
  Button,
  Callout,
  Card,
  CardStack,
  DescriptionList,
  PageHeader,
  SelectField,
  TextField,
} from "@/components/ui";
import { getConsoleAccount } from "@/lib/api/accounts";
import { IvrApiError } from "@/lib/api/errors";
import { requireAdmin } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, t } from "@/lib/i18n";

import { deleteAccountAction, resetPasswordAction, updateAccountAction } from "../actions";
import { accountErrorHint } from "../error-hint";
import styles from "../accounts.module.css";

export const dynamic = "force-dynamic";

interface AccountDetailPageProps {
  readonly params: Promise<{ accountId: string }>;
  readonly searchParams: Promise<Record<string, string | string[] | undefined>>;
}

export default async function AccountDetailPage({ params, searchParams }: AccountDetailPageProps) {
  const session = await requireAdmin();
  const { accountId } = await params;
  const query = await searchParams;
  const result = typeof query.result === "string" ? query.result : null;
  const error = typeof query.error === "string" ? query.error : null;

  let account;
  try {
    account = (await getConsoleAccount({ session, config: readConfig() }, accountId)).data;
  } catch (cause) {
    if (cause instanceof IvrApiError && cause.status === 404) {
      notFound();
    }
    throw cause;
  }

  return (
    <>
      <PageHeader
        eyebrow={t("accounts.eyebrow")}
        title={account.display_name}
        meta={[{ label: t("accounts.username"), value: account.username, mono: true }]}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.accounts"), href: "/accounts" },
            { label: account.username },
          ],
        }}
      />
      {result === null ? null : <Callout tone="success">{t("accounts.resultPrefix")}: {result}.</Callout>}
      {error === null ? null : (
        <Callout tone="danger" role="alert">
          {t("accounts.errorPrefix")}: {error}.
          {accountErrorHint(error) === null ? null : ` ${accountErrorHint(error)}`}
        </Callout>
      )}
      {account.is_builtin ? (
        <Callout tone="locked">{t("accounts.builtinLocked")}</Callout>
      ) : null}

      <CardStack>
        <Card title={t("accounts.currentTitle")} accent>
          <DescriptionList
            items={[
              { label: t("accounts.username"), value: account.username, mono: true },
              { label: t("accounts.displayName"), value: account.display_name },
              { label: t("accounts.role"), value: account.role },
              { label: t("accounts.status"), value: account.status, mono: true },
              { label: t("accounts.locked"), value: account.is_locked ? t("boolean.yes") : t("boolean.no") },
              { label: t("accounts.lastLogin"), value: account.last_login_at == null ? "—" : formatDateTime(account.last_login_at) },
              { label: t("accounts.passwordChanged"), value: formatDateTime(account.password_changed_at) },
              { label: t("accounts.version"), value: String(account.version), mono: true },
            ]}
          />
        </Card>

        <Card title={t("accounts.updateTitle")}>
          <form action={updateAccountAction} className={styles.form}>
            <input type="hidden" name="account_id" value={account.account_id} />
            <input type="hidden" name="version" value={account.version} />
            <TextField label={t("accounts.displayName")} name="display_name" defaultValue={account.display_name} minLength={1} maxLength={128} required />
            <SelectField label={t("accounts.role")} name="role" defaultValue={account.role} options={[{ value: "Admin", label: t("accounts.admin") }, { value: "Operator", label: t("accounts.operator") }]} required />
            <SelectField label={t("accounts.status")} name="status" defaultValue={account.status === "DELETED" ? "DISABLED" : account.status} options={[{ value: "ACTIVE", label: t("accounts.active") }, { value: "DISABLED", label: t("accounts.disabled") }]} required />
            <div className={styles.full}><TextField label={t("accounts.reason")} name="reason" width="full" minLength={3} maxLength={500} required /></div>
            <div className={`${styles.actions} ${styles.full}`}><Button type="submit" variant="primary">{t("accounts.save")}</Button></div>
          </form>
        </Card>

        <Card title={t("accounts.resetTitle")} description={t("accounts.resetDescription")}>
          <form action={resetPasswordAction} className={styles.form}>
            <input type="hidden" name="account_id" value={account.account_id} />
            <input type="hidden" name="version" value={account.version} />
            <TextField label={t("accounts.newPassword")} name="new_password" type="password" minLength={12} maxLength={128} autoComplete="new-password" required />
            <div className={styles.rest}>
              <TextField label={t("accounts.reason")} name="reason" width="full" minLength={3} maxLength={500} required />
            </div>
            <div className={`${styles.actions} ${styles.full}`}><Button type="submit" variant="secondary">{t("accounts.resetAction")}</Button></div>
          </form>
        </Card>

        {account.is_builtin || account.status === "DELETED" ? null : (
          <Card title={t("accounts.deleteTitle")}>
            <form action={deleteAccountAction} className={styles.dangerZone}>
              <input type="hidden" name="account_id" value={account.account_id} />
              <input type="hidden" name="version" value={account.version} />
              <TextField label={t("accounts.deleteReason")} name="reason" width="full" minLength={3} maxLength={500} required />
              <div className={styles.actions}><Button type="submit" variant="danger">{t("accounts.deleteAction")}</Button></div>
            </form>
          </Card>
        )}
      </CardStack>
    </>
  );
}
