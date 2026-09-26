#!/usr/bin/env node
// Q-28 (PA2, 2026-09-26). Turns the production pilot numbers into what the deployment is configured
// with and what the two approvers sign, without either of them writing a number anywhere.
//
//   node tools/ops/production-pilot-list.mjs --key-file <file> < numbers.txt
//       Reads the numbers from standard input, one per line, in any spelling a dial token may
//       reveal (0912…, +84912…, 84912…). Prints one fingerprint per number, in input order - the
//       values of Ivr__Telephony__SipTrunk__PilotDestinations__0, __1, … - and then the list hash,
//       which is the change_fingerprint of the PRODUCTION_PILOT_LIST approval. The numbers are
//       never printed back.
//   node tools/ops/production-pilot-list.mjs --hash <fingerprint>...
//       The list hash of fingerprints already in configuration: what the second approver
//       recomputes before signing, and what the gate compares against.
//
// The key is read from a file, never from an argument, so it does not land in shell history. It is
// Ivr__Telephony__SipTrunk__PilotFingerprintKey: base64, at least 32 bytes, a deployment secret.
// deploy/ci/scripts/production-pilot-selftest.mjs holds this file to the answers of the C#
// implementation (ProductionPilotFingerprint, VietnameseDestinationNumber).
import { createHash, createHmac } from "node:crypto";
import { readFileSync } from "node:fs";
import { pathToFileURL } from "node:url";

export const PREFIX = "pilot:";
export const MINIMUM_KEY_BYTES = 32;

/** VietnameseDestinationNumber.TryParse: the national significant number, or null. ASCII only. */
export function nationalDigits(value) {
  if (typeof value !== "string" || value.trim() === "") return null;
  const compact = [...value].filter((c) => (c >= "0" && c <= "9") || c === "+").join("");
  if (compact.length === 0 || compact.indexOf("+") > 0) return null;
  const digits = compact.replace(/^\++/, "");
  if (!/^[0-9]*$/.test(digits)) return null;
  const national = digits.startsWith("84") ? digits.slice(2) : digits.startsWith("0") ? digits.slice(1) : digits;
  if (national.length < 9 || national.length > 10 || national.startsWith("0")) return null;
  return national;
}

/** ProductionPilotFingerprint.Of: HMAC-SHA256 of the digits, written in the letters a-p. */
export function fingerprint(key, digits) {
  if (!Buffer.isBuffer(key) || key.length < MINIMUM_KEY_BYTES) throw new Error("the key is too short");
  if (!/^[0-9]+$/.test(digits)) throw new Error("a national number is ASCII digits only");
  let text = PREFIX;
  for (const byte of createHmac("sha256", key).update(digits, "ascii").digest()) {
    text += String.fromCharCode(97 + (byte >> 4), 97 + (byte & 0x0f));
  }
  return text;
}

/** ProductionPilotFingerprint.ListHash: SHA-256 of the distinct entries, ordinal order, one per line. */
export function listHash(fingerprints) {
  const canonical = [...new Set(fingerprints)].sort((a, b) => (a < b ? -1 : a > b ? 1 : 0)).join("\n");
  return createHash("sha256").update(canonical, "utf8").digest("hex").toUpperCase();
}

/** ProductionPilotFingerprint.TryReadKey: base64 of at least 32 bytes, or null. */
export function readKey(text) {
  const trimmed = typeof text === "string" ? text.trim() : "";
  if (!/^[A-Za-z0-9+/]+={0,2}$/.test(trimmed) || trimmed.length % 4 !== 0) return null;
  const key = Buffer.from(trimmed, "base64");
  return key.length >= MINIMUM_KEY_BYTES ? key : null;
}

export const isWellFormed = (value) => typeof value === "string" && /^pilot:[a-p]{64}$/.test(value);

function main(args) {
  if (args[0] === "--hash") {
    const entries = args.slice(1);
    const bad = entries.filter((entry) => !isWellFormed(entry));
    if (entries.length === 0 || bad.length > 0) throw new Error("--hash takes pilot fingerprints only");
    console.log(listHash(entries));
    return;
  }
  if (args[0] !== "--key-file" || !args[1]) {
    throw new Error("usage: --key-file <file> < numbers.txt, or --hash <fingerprint>...");
  }
  const key = readKey(readFileSync(args[1], "utf8"));
  if (!key) throw new Error("the key file does not hold base64 of at least 32 bytes");
  const lines = readFileSync(0, "utf8").split(/\r?\n/).map((line) => line.trim()).filter(Boolean);
  const fingerprints = lines.map((line, index) => {
    const digits = nationalDigits(line);
    if (!digits) throw new Error(`line ${index + 1} is not a usable Vietnamese number`);
    return fingerprint(key, digits);
  });
  if (fingerprints.length === 0) throw new Error("no numbers on standard input");
  fingerprints.forEach((value, index) => console.log(`PilotDestinations__${index}=${value}`));
  console.log(`PRODUCTION_PILOT_LIST change_fingerprint=${listHash(fingerprints)}`);
}

if (import.meta.url === pathToFileURL(process.argv[1] ?? "").href) {
  try {
    main(process.argv.slice(2));
  } catch (error) {
    console.error(`PRODUCTION_PILOT_LIST_REFUSED: ${error.message}`);
    process.exit(1);
  }
}
