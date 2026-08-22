"use client";

import { useSyncExternalStore } from "react";

/** Nothing ever changes, so the subscription is a no-op. */
function subscribe(): () => void {
  return () => {};
}

/**
 * False while rendering on the server and through the hydration pass, true
 * afterwards.
 *
 * This is how the two enhanced controls decide when it is safe to replace a
 * native element with a custom one. `useSyncExternalStore` rather than a
 * `useState` + `useEffect` pair: React is told outright that the server and
 * client snapshots differ, so it schedules the swap itself instead of being
 * surprised by a setState during an effect — which is both a cascading render
 * and, since React 19, a lint error.
 *
 * The pattern is what keeps progressive enhancement honest here. The server
 * markup is the plain control, so a page with no JavaScript still submits; the
 * upgrade happens only once there demonstrably is a client to run it.
 */
export function useHydrated(): boolean {
  return useSyncExternalStore(
    subscribe,
    () => true,
    () => false,
  );
}
