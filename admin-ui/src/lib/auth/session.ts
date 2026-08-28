import "server-only";

/**
 * W-0122. The console has no signed-in user any more; `AdminSession` is now the
 * service identity a screen calls the API with. Re-exported from its old path so
 * the reference implementation keeps compiling for whoever reads it next.
 */
export type { AdminSession, AdminScope } from "./guard";
