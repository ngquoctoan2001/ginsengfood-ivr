#!/usr/bin/env node
import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";
import { validateVoiceAcceptanceFile } from "./tts-voice-acceptance-lib.mjs";

const repoRoot = resolve(import.meta.dirname, "../../..");
const options = parseArgs(process.argv.slice(2));
const endpoint = loopbackEndpoint(options.endpoint);
const output = resolve(options.output);
const speechPlan = JSON.parse(readFileSync(resolve(options.segments || "deploy/lab/speech-segments.json"), "utf8"));
const acceptanceResult = validateVoiceAcceptanceFile(options.acceptance, repoRoot);
const acceptance = acceptanceResult.acceptance;
mkdirSync(output, { recursive: true });

const results = [];
for (const region of speechPlan.regions) {
  const selected = acceptance.selections?.[titleCase(region)];
  if (!selected?.voice_id || selected.speaking_rate === undefined) throw new Error(`missing ${region} selection`);
  for (const segment of speechPlan.segments.filter(item => item.kind === "Fixed")) {
    const pcm = await synthesize(endpoint, segment.text, selected.voice_id, selected.speaking_rate);
    const wav = wrapPcmWav(pcm, 8000);
    const filename = `${region}-s${segment.ordinal}.wav`;
    writeFileSync(resolve(output, filename), wav, { flag: "wx" });
    results.push({ region, ordinal: segment.ordinal, text_sha256: segment.textSha256, voice_id: selected.voice_id, file: filename, bytes: wav.length, sha256: sha256(wav) });
  }
}
writeFileSync(resolve(output, "source-manifest.json"), JSON.stringify({
  schema_version: 1, work_id: "W-0122", template_id: speechPlan.templateId,
  template_version: speechPlan.templateVersion, template_sha256: speechPlan.templateSha256,
  acceptance_manifest_sha256: acceptanceResult.sha256, results,
}, null, 2) + "\n", { flag: "wx" });
process.stdout.write(`TTS_FIXED_RENDER_PASS files=${results.length}\n`);

function parseArgs(args) {
  const result = {};
  for (let index = 0; index < args.length; index += 2) {
    const key = args[index]?.replace(/^--/, ""); const value = args[index + 1];
    if (!key || value === undefined) throw new Error("arguments must be --key value pairs");
    result[key] = value;
  }
  if (!result.endpoint || !result.output || !result.acceptance) throw new Error("--endpoint, --output and --acceptance are required");
  return result;
}
function loopbackEndpoint(value) {
  const url = new URL(value);
  if (url.protocol !== "http:" || !["127.0.0.1", "localhost", "::1"].includes(url.hostname)) throw new Error("render endpoint must be HTTP loopback");
  return url;
}
async function synthesize(endpoint, text, voiceId, speakingRate) {
  const response = await fetch(new URL("/synthesize", endpoint), { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ text, voice_id: voiceId, locale: "vi-VN", speaking_rate: speakingRate, output_format: "audio/L16", sample_rate: 8000 }) });
  if (!response.ok || response.headers.get("content-type") !== "audio/L16") throw new Error(`synthesis failed: ${response.status}`);
  const pcm = Buffer.from(await response.arrayBuffer());
  if (!pcm.length || pcm.length % 2 || pcm.subarray(0, 4).toString("ascii") === "RIFF") throw new Error("invalid raw PCM response");
  return pcm;
}
function wrapPcmWav(pcm, sampleRate) {
  const header = Buffer.alloc(44); header.write("RIFF", 0); header.writeUInt32LE(36 + pcm.length, 4); header.write("WAVE", 8); header.write("fmt ", 12); header.writeUInt32LE(16, 16); header.writeUInt16LE(1, 20); header.writeUInt16LE(1, 22); header.writeUInt32LE(sampleRate, 24); header.writeUInt32LE(sampleRate * 2, 28); header.writeUInt16LE(2, 32); header.writeUInt16LE(16, 34); header.write("data", 36); header.writeUInt32LE(pcm.length, 40); return Buffer.concat([header, pcm]);
}
function titleCase(value) { return value[0].toUpperCase() + value.slice(1); }
function sha256(value) { return createHash("sha256").update(value).digest("hex"); }
