import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const projectRoot = fileURLToPath(new URL("../../", import.meta.url));
const nextBin = fileURLToPath(new URL("../../node_modules/next/dist/bin/next", import.meta.url));

/**
 * Builds the app once for every e2e suite.
 *
 * Each suite used to build for itself, which meant two `next build` processes
 * writing `.next` at the same time when Vitest ran the files in parallel.
 */
export default function setup(): void {
  const build = spawnSync(process.execPath, [nextBin, "build"], {
    cwd: projectRoot,
    env: {
      ...process.env,
      NODE_ENV: "production",
    },
    encoding: "utf8",
  });

  if (build.status !== 0) {
    throw new Error(`next build failed: ${build.stdout}\n${build.stderr}`);
  }
}
