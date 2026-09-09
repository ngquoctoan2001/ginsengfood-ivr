import { realpathSync } from "node:fs";
import { dirname, isAbsolute, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";

/**
 * The repository root, canonicalised.
 *
 * Every validator derived this the same way and none of them resolved it:
 *
 *     const REPOSITORY_ROOT = resolve(dirname(SCRIPT_PATH), "../../..");
 *
 * Then each one checked its input with `isConfined(realpathSync(input))`. Comparing a resolved
 * path against an unresolved root is only correct while no component of the checkout path is a
 * symlink. When one is — `/tmp` on macOS, a runner workspace link, a bind mount — `relative()`
 * returns something starting with `..` for every legitimate file in the repository, `isConfined`
 * answers false, and the gate refuses its own inputs while reporting `real path escapes
 * repository root`. A path-traversal message for a checkout path is close to the worst possible
 * thing to read while debugging that, because it points at the input rather than the environment.
 *
 * `b3-telephony-evidence-validator.mjs` already resolved its root before comparing. That fix
 * never reached the other ten because each carried its own copy — which is the argument for this
 * file existing at all, rather than an eleventh identical patch.
 */
export const REPOSITORY_ROOT = realpathSync(
  resolve(dirname(fileURLToPath(import.meta.url)), "../../.."),
);

/**
 * True when `pathValue` names something inside the repository.
 *
 * Callers must pass an absolute path, and must additionally pass the `realpathSync` of anything
 * they are about to open — this function cannot see through a symlink on its own, and a link
 * inside the repository pointing out of it would otherwise satisfy the check.
 *
 * The two forms this replaced disagreed. Five copies wrote:
 *
 *     !rel.startsWith("..")
 *
 * which also rejects a legitimately named directory like `..cache`; three wrote the form kept
 * here, which rejects only a real ascent. Neither was unsafe, but a security helper existing in
 * two shapes means nobody can say which one is the rule.
 */
export function isConfined(pathValue) {
  const rel = relative(REPOSITORY_ROOT, pathValue);
  return (
    rel !== "" &&
    rel !== ".." &&
    !rel.startsWith(`..${sep}`) &&
    !isAbsolute(rel)
  );
}
