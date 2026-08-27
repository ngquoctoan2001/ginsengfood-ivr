#!/usr/bin/env node
import { createHash } from "node:crypto";
import { spawnSync } from "node:child_process";
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, resolve } from "node:path";

const options = parseArgs(process.argv.slice(2));
const transport = options.container
  ? { container: containerName(options.container) }
  : { endpoint: loopbackEndpoint(options.endpoint) };
const output = resolve(options.output);
const voicesPath = resolve(options.voices || "deploy/tts/shim/voices.json");
const scriptPath = resolve(options.script || "docs/evidence/W-0122/audition-script.txt");
const voices = JSON.parse(readFileSync(voicesPath, "utf8"));
const text = readFileSync(scriptPath, "utf8").trim();
const resume = options.resume === "true";
const timeoutSeconds = parseTimeout(options["timeout-seconds"] || "600");
mkdirSync(output, { recursive: true });

const results = [];
for (const voice of voices.voices.filter(item => item.audition_enabled === true)) {
  const filename = `audition-${voice.voice_id}.wav`;
  const target = resolve(output, filename);
  let wav;
  if (resume && existsSync(target)) {
    wav = readFileSync(target);
    validateWav(wav, 8000);
  } else {
    const pcm = await synthesize(
      transport, text, voice.voice_id, voice.speaking_rate, timeoutSeconds,
    );
    wav = wrapPcmWav(pcm, 8000);
    writeFileSync(target, wav, { flag: "wx" });
  }
  results.push({
    voice_id: voice.voice_id,
    preset: voice.preset,
    region: voice.region,
    speaking_rate: voice.speaking_rate,
    file: filename,
    bytes: wav.length,
    sha256: sha256(wav),
  });
}
writeFileSync(resolve(output, "audition-manifest.json"), JSON.stringify({
  schema_version: 1,
  work_id: "W-0122",
  script_file: basename(scriptPath),
  script_sha256: sha256(Buffer.from(text, "utf8")),
  source_commit: voices.source_commit,
  model_revision: voices.model_revision,
  voice_manifest_sha256: voices.voice_manifest_sha256,
  results,
}, null, 2) + "\n", { flag: "wx" });
process.stdout.write(`TTS_AUDITION_RENDER_PASS voices=${results.length}\n`);

function parseArgs(args) {
  const result = {};
  for (let index = 0; index < args.length; index += 2) {
    const key = args[index]?.replace(/^--/, "");
    const value = args[index + 1];
    if (!key || value === undefined) throw new Error("arguments must be --key value pairs");
    result[key] = value;
  }
  if (!result.output || Boolean(result.endpoint) === Boolean(result.container)) {
    throw new Error("--output and exactly one of --endpoint/--container are required");
  }
  return result;
}

function containerName(value) {
  if (!/^[a-zA-Z0-9][a-zA-Z0-9_.-]{0,127}$/.test(value)) {
    throw new Error("invalid container name");
  }
  return value;
}

function parseTimeout(value) {
  const parsed = Number(value);
  if (!Number.isInteger(parsed) || parsed < 30 || parsed > 900) {
    throw new Error("--timeout-seconds must be an integer from 30 to 900");
  }
  return parsed;
}

function loopbackEndpoint(value) {
  const url = new URL(value);
  if (url.protocol !== "http:" || !["127.0.0.1", "localhost", "::1"].includes(url.hostname)) {
    throw new Error("audition endpoint must be HTTP loopback");
  }
  return url;
}

async function synthesize(transport, text, voiceId, speakingRate, timeoutSeconds) {
  const body = { text, voice_id: voiceId, locale: "vi-VN", speaking_rate: speakingRate, output_format: "audio/L16", sample_rate: 8000 };
  if (transport.container) return synthesizeInContainer(transport.container, body, timeoutSeconds);
  const response = await fetch(new URL("/synthesize", transport.endpoint), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  if (!response.ok || response.headers.get("content-type") !== "audio/L16") throw new Error(`synthesis failed: ${response.status}`);
  const pcm = Buffer.from(await response.arrayBuffer());
  if (!pcm.length || pcm.length % 2 || pcm.subarray(0, 4).toString("ascii") === "RIFF") throw new Error("invalid raw PCM response");
  return pcm;
}

function synthesizeInContainer(container, body, timeoutSeconds) {
  const payload = Buffer.from(JSON.stringify(body), "utf8").toString("base64");
  const probe = [
    "import base64,urllib.request",
    `body=base64.b64decode(${JSON.stringify(payload)})`,
    "request=urllib.request.Request('http://127.0.0.1:8090/synthesize',data=body,headers={'Content-Type':'application/json'})",
    `response=urllib.request.urlopen(request,timeout=${timeoutSeconds})`,
    "audio=response.read()",
    "assert response.status==200 and response.headers.get('Content-Type')=='audio/L16'",
    "import sys;sys.stdout.buffer.write(audio)",
  ].join(";");
  const result = spawnSync("docker", ["exec", container, "python", "-c", probe], {
    encoding: null,
    maxBuffer: 32 * 1024 * 1024,
  });
  if (result.status !== 0) throw new Error("container synthesis failed");
  const pcm = Buffer.from(result.stdout);
  if (!pcm.length || pcm.length % 2 || pcm.subarray(0, 4).toString("ascii") === "RIFF") {
    throw new Error("invalid raw PCM response");
  }
  return pcm;
}

function validateWav(wav, sampleRate) {
  if (wav.length < 46 || wav.subarray(0, 4).toString("ascii") !== "RIFF"
      || wav.subarray(8, 12).toString("ascii") !== "WAVE"
      || wav.readUInt16LE(20) !== 1 || wav.readUInt16LE(22) !== 1
      || wav.readUInt32LE(24) !== sampleRate || wav.readUInt16LE(34) !== 16
      || wav.subarray(36, 40).toString("ascii") !== "data"
      || wav.readUInt32LE(40) !== wav.length - 44) {
    throw new Error("existing audition WAV is invalid");
  }
}

function wrapPcmWav(pcm, sampleRate) {
  const header = Buffer.alloc(44);
  header.write("RIFF", 0); header.writeUInt32LE(36 + pcm.length, 4); header.write("WAVE", 8);
  header.write("fmt ", 12); header.writeUInt32LE(16, 16); header.writeUInt16LE(1, 20);
  header.writeUInt16LE(1, 22); header.writeUInt32LE(sampleRate, 24); header.writeUInt32LE(sampleRate * 2, 28);
  header.writeUInt16LE(2, 32); header.writeUInt16LE(16, 34); header.write("data", 36); header.writeUInt32LE(pcm.length, 40);
  return Buffer.concat([header, pcm]);
}

function sha256(value) { return createHash("sha256").update(value).digest("hex"); }
