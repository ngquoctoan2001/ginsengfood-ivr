import { redirect } from "next/navigation";

/**
 * The console has no landing page of its own, so this forwards to the dashboard.
 *
 * It used to say `proxy.ts` bounced the request to `/login` when there was no
 * session. W-0128 removed sign-in from this service — Module 3 owns operator
 * identity — so there is no session to miss and no `/login` to reach.
 */
export default function RootPage() {
  redirect("/dashboard");
}
