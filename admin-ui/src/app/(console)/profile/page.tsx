import { Callout, Card, DescriptionList, PageHeader } from "@/components/ui";
import { getMyConsoleAccount } from "@/lib/api/accounts";
import { requirePermission } from "@/lib/auth/guard";
import { readConfig } from "@/lib/config/env";
import { formatDateTime, t } from "@/lib/i18n";

export const dynamic = "force-dynamic";

export default async function ProfilePage() {
  const session = await requirePermission("IVR_ACCOUNT_SELF_VIEW");
  const profile = (await getMyConsoleAccount({ session, config: readConfig() })).data;

  return (
    <>
      <PageHeader
        title={t("profile.title")}
        subtitle={t("profile.subtitle")}
        breadcrumb={{
          label: t("nav.breadcrumbLabel"),
          items: [
            { label: t("nav.console"), href: "/dashboard" },
            { label: t("nav.profile") },
          ],
        }}
      />
      <Callout tone="info">{t("profile.notice")}</Callout>
      <Card title={profile.display_name} accent>
        <DescriptionList
          items={[
            { label: t("accounts.username"), value: profile.username, mono: true },
            { label: t("accounts.role"), value: profile.role },
            { label: t("accounts.status"), value: profile.status, mono: true },
            { label: t("accounts.lastLogin"), value: profile.last_login_at == null ? "—" : formatDateTime(profile.last_login_at) },
            { label: t("accounts.passwordChanged"), value: formatDateTime(profile.password_changed_at) },
          ]}
        />
      </Card>
    </>
  );
}
