#!/usr/bin/env node
import { randomUUID } from "node:crypto";
import { spawnSync } from "node:child_process";
import { sep } from "node:path";
import { fileURLToPath } from "node:url";

const image = process.env.IVR_TTS_SELFTEST_IMAGE || "ivr-tts:w0122-selftest";
const name = `ivr-tts-selftest-${randomUUID().replaceAll("-", "").slice(0, 12)}`;
const testLoaderName = `ivr-tts-test-loader-${randomUUID().replaceAll("-", "").slice(0, 12)}`;
const testVolume = `ivr-tts-tests-${randomUUID().replaceAll("-", "").slice(0, 12)}`;

function run(command, args, options = {}) {
  const result = spawnSync(command, args, {
    cwd: new URL("../../../", import.meta.url),
    encoding: "utf8",
    stdio: options.capture ? "pipe" : "inherit",
  });
  if (result.status !== 0) {
    throw new Error(`${command} failed (${result.status}): ${result.stderr || ""}`);
  }
  return result.stdout || "";
}

try {
  if (!process.env.IVR_TTS_SELFTEST_IMAGE) {
    run("docker", ["build", "--file", "deploy/tts/Dockerfile.tts", "--tag", image, "."]);
  }
  const configuredUser = run("docker", ["image", "inspect", image, "--format", "{{.Config.User}}"], { capture: true }).trim();
  if (configuredUser !== "1654:1654") throw new Error(`unexpected image user: ${configuredUser}`);

  run("docker", ["volume", "create", testVolume], { capture: true });
  run("docker", [
    "run", "--detach", "--name", testLoaderName, "--user", "0:0",
    "--mount", `type=volume,source=${testVolume},target=/tests`,
    "--entrypoint", "python", image, "-c", "import time; time.sleep(300)",
  ], { capture: true });
  const testsSource = `${fileURLToPath(new URL("../../tts/tests", import.meta.url))}${sep}.`;
  run("docker", ["cp", testsSource, `${testLoaderName}:/tests`]);
  run("docker", ["rm", "--force", testLoaderName], { capture: true });
  run("docker", [
    "run", "--rm", "--network", "none", "--read-only",
    "--tmpfs", "/tmp:rw,noexec,nosuid,nodev,size=16m", "--cap-drop", "ALL",
    "--security-opt", "no-new-privileges:true",
    "--mount", `type=volume,source=${testVolume},target=/tests,readonly`,
    "--entrypoint", "python", image,
    "-m", "unittest", "discover", "-s", "/tests", "-v",
  ]);

  run("docker", [
    "run", "--detach", "--name", name, "--network", "none", "--read-only",
    "--tmpfs", "/tmp:rw,noexec,nosuid,nodev,size=16m", "--cap-drop", "ALL",
    "--security-opt", "no-new-privileges:true",
    "--env", "IVR_EXECUTION_MODE=MOCK",
    "--env", "VIE_NEU_BACKEND=deterministic-test",
    "--env", "VIE_NEU_HOST=127.0.0.1",
    image,
  ]);

  const probe = String.raw`
import json, time, urllib.error, urllib.request
base = "http://127.0.0.1:8090"
for _ in range(60):
    try:
        if urllib.request.urlopen(base + "/health/ready", timeout=1).status == 200:
            break
    except Exception:
        time.sleep(0.5)
else:
    raise SystemExit("readiness timeout")

body = json.dumps({
    "text": "Xin chào Quý khách.",
    "voice_id": "test-north",
    "locale": "vi-VN",
    "speaking_rate": 1.0,
    "output_format": "audio/L16",
    "sample_rate": 8000,
}, ensure_ascii=False).encode()
request = urllib.request.Request(base + "/synthesize", data=body, headers={"Content-Type":"application/json"})
with urllib.request.urlopen(request, timeout=10) as response:
    audio = response.read()
    assert response.status == 200
    assert response.headers["Content-Type"] == "audio/L16"
    assert len(audio) > 1600 and len(audio) % 2 == 0 and not audio.startswith(b"RIFF")

bad = urllib.request.Request(base + "/synthesize", data=body, headers={"Content-Type":"text/plain"})
try:
    urllib.request.urlopen(bad, timeout=2)
    raise AssertionError("invalid content type accepted")
except urllib.error.HTTPError as error:
    assert error.code == 415 and error.read() == b""

extra = json.loads(body)
extra["unexpected"] = True
bad = urllib.request.Request(base + "/synthesize", data=json.dumps(extra).encode(), headers={"Content-Type":"application/json"})
try:
    urllib.request.urlopen(bad, timeout=2)
    raise AssertionError("extra field accepted")
except urllib.error.HTTPError as error:
    assert error.code == 422 and error.read() == b""

print("TTS_CONTAINER_CONTRACT_PASS")
`;
  const output = run("docker", ["exec", name, "python", "-c", probe], { capture: true });
  process.stdout.write(output);
  const ports = run("docker", ["image", "inspect", image, "--format", "{{json .Config.ExposedPorts}}"], { capture: true }).trim();
  if (ports !== "null" && ports !== "{}") throw new Error(`image exposes ports: ${ports}`);
  process.stdout.write("TTS_CONTAINER_SELFTEST_PASS nonroot=YES ports=NONE network=NONE\n");
} finally {
  spawnSync("docker", ["rm", "--force", name], { stdio: "ignore" });
  spawnSync("docker", ["rm", "--force", testLoaderName], { stdio: "ignore" });
  spawnSync("docker", ["volume", "rm", "--force", testVolume], { stdio: "ignore" });
}
