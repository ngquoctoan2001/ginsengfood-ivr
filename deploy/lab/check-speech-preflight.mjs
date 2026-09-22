#!/usr/bin/env node
// W-0320: shared by Start-FreeSoftphoneLab and every direct lab call. No call is submitted here.
import { execFile } from "node:child_process";
import { createHash, randomBytes, randomUUID } from "node:crypto";
import { resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { promisify } from "node:util";

const execute = promisify(execFile);
const project = "ginsengfood-ivr-dev";
const workerPath = "/var/lib/ivr/speech";
const asteriskPath = "/var/lib/asterisk/sounds/generated";
// Deliberately return only the three relevant environment values, never ARI/SIP credentials.
const inspectFormat = '{"id":{{json .Id}},"running":{{json .State.Running}},'
  + '"user":{{json .Config.User}},"image":{{json .Image}},"mounts":{{json .Mounts}},'
  + '"network":{{json .HostConfig.NetworkMode}},'
  + '"project":{{json (index .Config.Labels "com.docker.compose.project")}},'
  + '"service":{{json (index .Config.Labels "com.docker.compose.service")}},'
  + '"governance":[{{range .Config.Env}}{{if or '
  + '(eq . "IVR_EXECUTION_MODE=LAB_REAL_SIM") (eq . "REAL_CUSTOMER_CALL_ALLOWED=NO") '
  + '(eq . "VIE_NEU_BACKEND=vieneu-onnx")}}{{json .}},{{end}}{{end}}""]}';

export async function docker(args, timeout = 10_000) {
  try {
    const result = await execute("docker", args, {
      encoding: "utf8", timeout, maxBuffer: 1024 * 1024, windowsHide: true,
    });
    return result.stdout.trim();
  } catch {
    // Container output can include configuration. Do not forward it into evidence or logs.
    throw new Error("LAB_SPEECH_DOCKER_COMMAND_FAILED");
  }
}

export async function checkSpeechPreflight({
  timeoutSeconds = 180, run = docker,
  pause = ms => new Promise(done => setTimeout(done, ms)),
  now = () => performance.now(),
} = {}) {
  if (!Number.isInteger(timeoutSeconds) || timeoutSeconds < 1 || timeoutSeconds > 600) {
    throw new Error("LAB_SPEECH_INVALID_TIMEOUT");
  }
  const services = ["ivr-worker", "ivr-tts", "asterisk"];
  const containers = {};
  for (const service of services) {
    const name = `${project}-${service}-1`;
    const info = JSON.parse(await run(["inspect", "--format", inspectFormat, name]));
    if (!info.running || info.project !== project || info.service !== service) {
      throw new Error("LAB_SPEECH_CONTAINER_NOT_READY");
    }
    containers[service] = { ...info, name };
  }
  const worker = containers["ivr-worker"];
  const tts = containers["ivr-tts"];
  const asterisk = containers.asterisk;
  for (const info of [worker, tts]) {
    if (!["IVR_EXECUTION_MODE=LAB_REAL_SIM", "REAL_CUSTOMER_CALL_ALLOWED=NO"]
      .every(value => info.governance.includes(value))) {
      throw new Error("LAB_SPEECH_GOVERNANCE_REFUSED");
    }
  }
  if (!tts.governance.includes("VIE_NEU_BACKEND=vieneu-onnx")
    || tts.network !== `container:${worker.id}`) {
    throw new Error("LAB_SPEECH_SIDECAR_REFUSED");
  }
  if (worker.user !== "1654:1654") throw new Error("LAB_SPEECH_WORKER_USER_REFUSED");
  const writer = worker.mounts.find(mount => mount.Destination === workerPath);
  const reader = asterisk.mounts.find(mount => mount.Destination === asteriskPath);
  if (!writer || !reader || writer.Type !== "volume" || reader.Type !== "volume"
    || !writer.Name || writer.Name !== reader.Name || writer.RW !== true || reader.RW !== false) {
    throw new Error("LAB_SPEECH_SHARED_VOLUME_REFUSED");
  }

  const readyProbe = "import urllib.request; "
    + "assert urllib.request.urlopen('http://127.0.0.1:8090/health/ready', timeout=2).status == 200; "
    + "print('VIENEU_READY')";
  const deadline = now() + timeoutSeconds * 1000;
  let ready = false;
  while (now() < deadline) {
    try {
      const remaining = Math.max(1, Math.ceil(deadline - now()));
      ready = await run(["exec", tts.name, "python", "-c", readyProbe], Math.min(5_000, remaining)) === "VIENEU_READY";
    } catch { /* A cold model may still be loading; the monotonic deadline remains authoritative. */ }
    if (ready) break;
    await pause(Math.min(1000, Math.max(0, deadline - now())));
  }
  if (!ready) throw new Error("LAB_SPEECH_READINESS_TIMEOUT");

  const suffix = randomUUID().replaceAll("-", "");
  const probeName = `ivr-preflight-${suffix}.probe`;
  const helperName = `ivr-speech-probe-${suffix}`;
  const payload = randomBytes(32).toString("hex");
  const expected = createHash("sha256").update(payload).digest("hex");
  const helper = ["run", "--rm", "--pull", "never", "--name", helperName,
    "--network", "none", "--read-only", "--user", "1654:1654", "--cap-drop", "ALL",
    // Never copy the image's /media directory (and root ownership) into an empty live volume.
    "--security-opt", "no-new-privileges:true", "--mount", `type=volume,src=${writer.Name},dst=/media,volume-nocopy`,
    "--entrypoint", "python", tts.image];
  let probeFailure;
  try {
    // The worker image is chiseled. An isolated helper uses its exact UID/GID on its exact volume.
    // This proves filesystem access, not worker application synthesis or audible playback.
    await run([...helper, "-c", "import os,sys; from pathlib import Path; "
      + "assert (os.getuid(),os.getgid()) == (1654,1654); "
      + "p=Path('/media')/sys.argv[1]; "
      + "f=p.open('xb'); f.write(sys.argv[2].encode('ascii')); f.close()", probeName, payload]);
    const digest = await run(["exec", asterisk.name, "sha256sum", `${asteriskPath}/${probeName}`]);
    if (digest.split(/\s+/u)[0] !== expected) throw new Error("LAB_SPEECH_MEDIA_DIGEST_MISMATCH");
    const denial = await run(["exec", asterisk.name, "sh", "-c",
      'if (printf test > "$1") 2>/dev/null; then exit 31; fi; printf READ_ONLY_CONFIRMED',
      "probe", `${asteriskPath}/${probeName}.denied`]);
    if (denial !== "READ_ONLY_CONFIRMED") throw new Error("LAB_SPEECH_MEDIA_WRITE_NOT_DENIED");
  } catch (error) {
    probeFailure = error;
  } finally {
    // A timed-out docker CLI can leave its helper running. Only this random, task-owned name is removed.
    await run(["rm", "--force", helperName]).catch(() => {});
    try {
      await run([...helper, "-c", "import sys; from pathlib import Path; "
        + "p=Path('/media')/sys.argv[1]; p.unlink(missing_ok=True); "
        + "p.with_name(p.name+'.denied').unlink(missing_ok=True)", probeName]);
    } catch {
      probeFailure = new Error("LAB_SPEECH_PROBE_CLEANUP_FAILED");
    } finally {
      await run(["rm", "--force", helperName]).catch(() => {});
    }
  }
  if (probeFailure) throw probeFailure;
  return "LAB_SPEECH_PREFLIGHT_PASS readiness=200 writer_uid=1654 media_roundtrip=PASS reader_write=DENIED";
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  try {
    const args = process.argv.slice(2);
    if (args.length !== 0 && (args.length !== 2 || args[0] !== "--timeout-seconds")) {
      throw new Error("LAB_SPEECH_INVALID_ARGUMENTS");
    }
    console.log(await checkSpeechPreflight({ timeoutSeconds: args.length ? Number(args[1]) : 180 }));
  } catch (error) {
    console.error(/^LAB_SPEECH_[A-Z_]+$/u.test(error.message) ? error.message : "LAB_SPEECH_PREFLIGHT_FAILED");
    process.exitCode = 1;
  }
}
