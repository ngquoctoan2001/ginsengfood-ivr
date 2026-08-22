import crypto from "node:crypto";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

// W-0106 A1. Emits the fixed sentences of the approved script, with the identity the runtime
// computes for each one, so a voice engineer knows exactly what to record and the recordings can
// be pinned to the wording they were made from.
//
// Generated rather than written by hand for the same reason the traceability table is: a
// hand-kept list drifts the moment someone edits a word of the template, and a drifted list is
// worse than none — the runtime would look up a sentence nobody recorded, or worse, find a
// recording of the previous wording and play it.
//
// The template is read out of the C# source rather than copied here. UT-SEG-MANIFEST-12 asserts
// this file agrees with what Ivr.Domain computes; if the two ever disagree, that test goes red.
//
// Pass --check to fail instead of writing.

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(scriptDirectory, "../../..");
const policyPath = path.join(
  repositoryRoot,
  "src/Ivr.Domain/Scripts/TargetV1SpeechPolicy.cs",
);
const outputPath = path.join(repositoryRoot, "deploy/lab/speech-segments.json");
const checkOnly = process.argv.includes("--check");

/**
 * Pulls a `public const string NAME = "a" + "b";` value out of the policy source.
 * Reading the source keeps one copy of the wording in the repository; a second copy here would
 * be the exact drift this file exists to prevent.
 */
function readConstant(source, name) {
  const declaration = new RegExp(
    `public const string ${name}\\s*=\\s*([\\s\\S]*?);`,
    "u",
  ).exec(source);
  if (declaration === null) {
    throw new Error(`${name} was not found in TargetV1SpeechPolicy.cs.`);
  }

  const parts = [...declaration[1].matchAll(/"((?:[^"\\]|\\.)*)"/gu)].map((match) =>
    match[1].replace(/\\"/gu, '"').replace(/\\\\/gu, "\\"),
  );
  if (parts.length === 0) {
    throw new Error(`${name} has no string literal parts.`);
  }

  return parts.join("");
}

/** SHA-256 over the NFC-normalized text — byte-for-byte what SpeechSegment.ComputeTextHash does. */
function textHash(text) {
  return crypto
    .createHash("sha256")
    .update(Buffer.from(text.normalize("NFC"), "utf8"))
    .digest("hex");
}

function splitTemplate(template) {
  const placeholder = /\{\{(?<name>[a-z0-9_]+)\}\}/gu;
  const segments = [];
  let cursor = 0;
  let ordinal = 0;
  for (const match of template.matchAll(placeholder)) {
    if (match.index > cursor) {
      const prose = template.slice(cursor, match.index);
      if (prose.trim().length === 0) {
        throw new Error(
          "Script template variables must be separated by spoken text, not whitespace alone.",
        );
      }

      segments.push({
        ordinal: ++ordinal,
        kind: "Fixed",
        placeholder: null,
        text: prose,
        textSha256: textHash(prose),
      });
    }

    segments.push({
      ordinal: ++ordinal,
      kind: "Dynamic",
      placeholder: match.groups.name,
      text: "",
      textSha256: null,
    });
    cursor = match.index + match[0].length;
  }

  if (cursor < template.length) {
    const prose = template.slice(cursor);
    segments.push({
      ordinal: ++ordinal,
      kind: "Fixed",
      placeholder: null,
      text: prose,
      textSha256: textHash(prose),
    });
  }

  return segments;
}

const source = await fs.readFile(policyPath, "utf8");
const template = readConstant(source, "CanonicalVietnameseTemplate");
const segments = splitTemplate(template);
const fixedSegments = segments.filter((segment) => segment.kind === "Fixed");

const manifest = {
  // Regenerate with: node deploy/ci/scripts/generate-speech-segments.mjs
  generatedBy: "deploy/ci/scripts/generate-speech-segments.mjs",
  templateId: readConstant(source, "MockTemplateId"),
  templateVersion: readConstant(source, "MockTemplateVersion"),
  templateSha256: textHash(template),
  fixedSegmentCount: fixedSegments.length,
  dynamicSegmentCount: segments.length - fixedSegments.length,
  // Characters that never need a synthesizer again, against the ones that do. This is the
  // 68/32 split the cost model in the W-0106 plan §4.6 rests on.
  fixedCharacters: fixedSegments.reduce((total, segment) => total + segment.text.length, 0),
  templateCharacters: template.length,
  regions: ["north", "central", "south"],
  segments,
};

const rendered = `${JSON.stringify(manifest, null, 2)}\n`;
if (checkOnly) {
  const current = await fs.readFile(outputPath, "utf8").catch(() => "");
  if (current !== rendered) {
    console.error(
      "SPEECH_SEGMENTS_DRIFT: deploy/lab/speech-segments.json does not match the approved template.",
    );
    process.exit(1);
  }

  console.log(`SPEECH_SEGMENTS_OK=${fixedSegments.length}`);
} else {
  await fs.writeFile(outputPath, rendered, "utf8");
  console.log(`SPEECH_SEGMENTS_WRITTEN=${fixedSegments.length}`);
}
