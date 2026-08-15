import { EmptyState } from "@/components/feedback/EmptyState";
import { t } from "@/lib/i18n";

export default function NotFound() {
  return (
    <div style={{ padding: "2rem" }}>
      <EmptyState title={t("state.notFoundTitle")} body={t("state.notFoundBody")} />
    </div>
  );
}
