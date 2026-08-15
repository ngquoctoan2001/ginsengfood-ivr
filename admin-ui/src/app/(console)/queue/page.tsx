import { redirect } from "next/navigation";

/**
 * `/queue` was the P3-1 foundation screen. P3-2 folded it into the operational
 * dashboard (UI-01), which owns the queue panel and the pause/resume controls;
 * keeping both would have shown the same admin actions in two places.
 */
export default function QueueRedirectPage() {
  redirect("/dashboard");
}
