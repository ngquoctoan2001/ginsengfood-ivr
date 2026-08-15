import { redirect } from "next/navigation";

/**
 * The console has no landing page of its own. `proxy.ts` bounces the request on
 * to `/login` when there is no session.
 */
export default function RootPage() {
  redirect("/dashboard");
}
