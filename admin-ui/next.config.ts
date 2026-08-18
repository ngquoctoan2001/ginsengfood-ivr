import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // W-0043 / P7-1. Standalone output traces the modules the server actually reaches and emits a
  // self-contained bundle, so the runtime image carries neither the build toolchain nor the full
  // node_modules tree. Without it the only way to run the console in a container is to ship the
  // whole development dependency set.
  output: "standalone",
  turbopack: {
    root: __dirname,
  },
};

export default nextConfig;
