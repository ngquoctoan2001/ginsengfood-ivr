#!/usr/bin/env node

import { createHash } from "node:crypto";
import {
  lstatSync,
  readFileSync,
  realpathSync,
  statSync,
} from "node:fs";
import { dirname, isAbsolute, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const REPOSITORY_ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "../../..");
const MAX_INPUT_BYTES = 512 * 1024;
const SHA_PATTERN = /^[a-f0-9]{64}$/u;
const GIT_SHA_PATTERN = /^[a-f0-9]{40}$/u;
const SAFE_ALIAS_PATTERN = /^[A-Z0-9][A-Z0-9._:/-]{1,127}$/u;

const SOURCE_PINS = Object.freeze({
  "deploy/tts/models/MODELS.lock": "bba41ea796bc6ab1c659865a1087868d413536808e00535c71e5ce4609cbe37d",
  "deploy/tts/shim/voices.json": "9a76fdabca3ad58994caa1b59c0c76f3a98facb22f11e2f9ec9210a9371ccae2",
  "docs/evidence/W-0122/voice-acceptance-manifest.json": "90927e16cbe5b4e27f48e31ba396c9069f1ae908eaadc15757e72fbc0d8558b9",
  "docs/evidence/W-0122/lab-runbook.md": "76496a7260f84053296ffabea617b7a7203eb2252ab440b76c213c21f71e3bb4",
  "docs/lab/one-sim-lab-plan.md": "be8b1f7a7dd3ac6c287bfab342fa54b63735795e13c6f515e904f25a054d2ae2",
  "docs/contracts/telephony-procurement-pack/lab-acceptance-report-template.md": "5b7ab1e0b1a796f7c1e0bb8643fefb313a76afa626a6200bf17519e306c2dbaa",
  "docs/contracts/telephony-procurement-pack/R-01-vendor-requirements.md": "1f5d7ead649e4b688301da2af6b4e72eae3907a0120c5c5bd66fd19c980fb1a0",
  "docs/contracts/telephony-procurement-pack/R-05-tts-audio-capability.md": "a1716a0747e2bd90f05252ed8fa9ec4a6a1ee0f5c7f0b19cf85cb36eb75eae8a",
  "docs/contracts/telephony-procurement-pack/R-06-to-trinh-mua-thiet-bi.md": "341153903009c37fb7fcf5ea3c5bdb253a6c7cd49f908676ff7c0612e681eea4",
});

const TTS_CALL_MATRIX = Object.freeze([
  ["B3-NORTH-ORDER-A", "NORTH", "FAKE-ORDER-A", "1", "IVR_CONFIRMED"],
  ["B3-NORTH-ORDER-B", "NORTH", "FAKE-ORDER-B", "0", "IVR_CUSTOMER_CANCELLED"],
  ["B3-CENTRAL-ORDER-A", "CENTRAL", "FAKE-ORDER-A", "1", "IVR_CONFIRMED"],
  ["B3-CENTRAL-ORDER-B", "CENTRAL", "FAKE-ORDER-B", "0", "IVR_CUSTOMER_CANCELLED"],
  ["B3-SOUTH-ORDER-A", "SOUTH", "FAKE-ORDER-A", "1", "IVR_CONFIRMED"],
  ["B3-SOUTH-ORDER-B", "SOUTH", "FAKE-ORDER-B", "0", "IVR_CUSTOMER_CANCELLED"],
]);

const REAL_SIM_SCENARIOS = Object.freeze([
  ["LAB-01", "CONFIRMED"],
  ["LAB-02", "CUSTOMER_CANCELLED"],
  ["LAB-03", "NO_INPUT_CHANNEL_RELEASED"],
  ["LAB-04", "WRONG_INPUT_REPROMPTED"],
  ["LAB-05", "BARGE_IN_CONFIRMED"],
  ["LAB-06", "NO_ANSWER_RETRY_SCHEDULED"],
  ["LAB-07", "BUSY_OR_REJECTED_NOT_NO_ANSWER"],
  ["LAB-08", "KILL_SWITCH_STOPS_NEW_DIALS"],
]);

const ARTIFACT_ROLES = Object.freeze([
  "LAB_ACCEPTANCE_REPORT",
  "TARGET_HARDWARE_PERFORMANCE",
  "PROCUREMENT_DECISION",
  "LEGAL_PRIVACY_APPROVAL",
  "SECURITY_CVE_DISPOSITION",
  "PLATFORM_TOPOLOGY_APPROVAL",
  "TELEPHONY_VENDOR_ACCEPTANCE",
  "INTERNAL_MIRROR_ATTESTATION",
  "RETENTION_ROLLBACK_REPORT",
  "PRODUCTION_CUTOVER_PACKET",
]);

const SIGNOFF_ROLES = Object.freeze([
  "PRODUCT_OWNER",
  "LEGAL_PRIVACY",
  "SECURITY",
  "PLATFORM",
  "TELEPHONY",
  "PROCUREMENT",
  "RELEASE",
]);

function fail(message) {
  throw new Error(message);
}

function sha256(value) {
  return createHash("sha256").update(value).digest("hex");
}

function hashLabel(label) {
  return sha256(Buffer.from(label, "utf8"));
}

function stableStringify(value) {
  if (Array.isArray(value)) {
    return "[" + value.map(stableStringify).join(",") + "]";
  }
  if (value !== null && typeof value === "object") {
    return "{" + Object.keys(value).sort().map((key) => {
      return JSON.stringify(key) + ":" + stableStringify(value[key]);
    }).join(",") + "}";
  }
  return JSON.stringify(value);
}

function canonicalizeB3TelephonyEvidence(bundle) {
  const copy = structuredClone(bundle);
  delete copy.bundle_sha256;
  return stableStringify(copy);
}

function canonicalizeCandidate(candidate) {
  const copy = structuredClone(candidate);
  delete copy.candidate_sha256;
  return stableStringify(copy);
}

function assertRecord(value, label) {
  if (value === null || typeof value !== "object" || Array.isArray(value)) {
    fail(label + " must be an object");
  }
}

function assertExactKeys(value, keys, label) {
  assertRecord(value, label);
  const actual = Object.keys(value).sort();
  const expected = [...keys].sort();
  if (JSON.stringify(actual) !== JSON.stringify(expected)) {
    fail(label + " keys mismatch");
  }
}

function assertString(value, label, minimum = 2, maximum = 240) {
  if (typeof value !== "string" || value.trim() !== value ||
      value.length < minimum || value.length > maximum) {
    fail(label + " must be a trimmed string of " + minimum + ".." + maximum + " characters");
  }
  if (/[\u0000-\u001f\u007f]/u.test(value)) fail(label + " contains a control character");
}

function assertAlias(value, label) {
  assertString(value, label);
  if (!SAFE_ALIAS_PATTERN.test(value)) fail(label + " must be an uppercase opaque alias");
}

function assertSha(value, label) {
  if (typeof value !== "string" || !SHA_PATTERN.test(value)) {
    fail(label + " must be lowercase SHA-256");
  }
}

function assertGitSha(value, label) {
  if (typeof value !== "string" || !GIT_SHA_PATTERN.test(value)) {
    fail(label + " must be a full lowercase Git SHA");
  }
}

function assertBoolean(value, expected, label) {
  if (value !== expected) fail(label + " must be " + expected);
}

function assertInteger(value, minimum, maximum, label) {
  if (!Number.isInteger(value) || value < minimum || value > maximum) {
    fail(label + " must be an integer in " + minimum + ".." + maximum);
  }
}

function parseTimestamp(value, label) {
  assertString(value, label, 20, 35);
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?Z$/u.test(value)) {
    fail(label + " must be ISO-8601 UTC");
  }
  const parsed = Date.parse(value);
  if (!Number.isFinite(parsed)) fail(label + " is not a valid timestamp");
  return parsed;
}

function assertSafeReference(value, label) {
  assertString(value, label, 3, 240);
  if (/[?#]/u.test(value) || value.includes("..") || value.includes("\\")) {
    fail(label + " must be a query-free, traversal-free metadata reference");
  }
  if (/^(?:https?|ftp):/iu.test(value)) {
    fail(label + " must be an internal artifact reference, not a network URL");
  }
}

function assertNoSensitiveMaterial(value, path = "$") {
  if (Array.isArray(value)) {
    value.forEach((item, index) => assertNoSensitiveMaterial(item, path + "[" + index + "]"));
    return;
  }
  if (value !== null && typeof value === "object") {
    for (const [key, item] of Object.entries(value)) {
      assertNoSensitiveMaterial(item, path + "." + key);
    }
    return;
  }
  if (typeof value !== "string") return;
  if (SHA_PATTERN.test(value) || GIT_SHA_PATTERN.test(value) || /^sha256:[a-f0-9]{64}$/u.test(value)) {
    return;
  }
  const forbidden = [
    [/(?<![0-9])(?:\+84|0)[35789][0-9]{8}(?![0-9])/u, "raw Vietnamese phone"],
    [/[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/iu, "email address"],
    [/-----BEGIN [A-Z ]+PRIVATE KEY-----/u, "private key"],
    [/(?:password|passwd|client_secret|access_token|api_key)\s*[:=]/iu, "credential-like material"],
    [/\b(?:sip|tel):[+0-9]/iu, "dialable URI"],
  ];
  for (const [pattern, label] of forbidden) {
    if (pattern.test(value)) fail(path + " contains forbidden " + label);
  }
}

function verifyLocalSourcePins(sourcePins) {
  assertExactKeys(sourcePins, Object.keys(SOURCE_PINS), "source_pins");
  for (const [path, expected] of Object.entries(SOURCE_PINS)) {
    assertSha(sourcePins[path], "source_pins." + path);
    if (sourcePins[path] !== expected) fail("declared source pin drift: " + path);
    const actual = sha256(readFileSync(resolve(REPOSITORY_ROOT, path)));
    if (actual !== expected) fail("local source byte drift: " + path);
  }
}

function validateCandidate(candidate, expected) {
  assertExactKeys(candidate, [
    "ivr_commit_sha",
    "tts_image_digest",
    "tts_model_bundle_sha256",
    "fixed_catalog_sha256",
    "configuration_sha256",
    "candidate_sha256",
  ], "candidate");
  assertGitSha(candidate.ivr_commit_sha, "candidate.ivr_commit_sha");
  assertSha(candidate.tts_model_bundle_sha256, "candidate.tts_model_bundle_sha256");
  assertSha(candidate.fixed_catalog_sha256, "candidate.fixed_catalog_sha256");
  assertSha(candidate.configuration_sha256, "candidate.configuration_sha256");
  if (typeof candidate.tts_image_digest !== "string" ||
      !/^sha256:[a-f0-9]{64}$/u.test(candidate.tts_image_digest)) {
    fail("candidate.tts_image_digest must be a sha256 digest");
  }
  assertSha(candidate.candidate_sha256, "candidate.candidate_sha256");
  const actualCandidateSha = sha256(Buffer.from(canonicalizeCandidate(candidate), "utf8"));
  if (candidate.candidate_sha256 !== actualCandidateSha) fail("candidate canonical hash mismatch");
  if (candidate.candidate_sha256 !== expected.candidateSha) fail("independent candidate hash mismatch");
  if (candidate.ivr_commit_sha !== expected.ivrSha) fail("independent IVR commit mismatch");
  if (candidate.tts_image_digest !== expected.ttsImageDigest) fail("independent TTS image mismatch");
  return actualCandidateSha;
}

function validateTtsCalls(calls) {
  if (!Array.isArray(calls) || calls.length !== TTS_CALL_MATRIX.length) {
    fail("tts_calls must contain exactly six calls");
  }
  const seen = new Set();
  calls.forEach((call, index) => {
    assertExactKeys(call, [
      "call_id", "region", "fixture_alias", "destination_alias", "route",
      "started_at", "completed_at", "dtmf_sent", "expected_result", "actual_result",
      "correct_voice", "correct_content", "audio_seams_checked", "media_round_trip",
      "caller_id_verified", "recording_disabled", "cdr_attempt_joined", "state",
      "evidence_ref", "evidence_sha256",
    ], "tts_calls[" + index + "]");
    const [callId, region, fixture, digit, result] = TTS_CALL_MATRIX[index];
    if (call.call_id !== callId || call.region !== region || call.fixture_alias !== fixture ||
        call.dtmf_sent !== digit || call.expected_result !== result || call.actual_result !== result) {
      fail("tts call matrix drift at index " + index);
    }
    if (seen.has(call.call_id)) fail("duplicate TTS call id");
    seen.add(call.call_id);
    if (call.destination_alias !== "LAB-A") fail("TTS destination must be opaque LAB-A");
    if (call.route !== "ASTERISK_MICROSIP_8KHZ_TARGET_HARDWARE") fail("TTS route drift");
    const start = parseTimestamp(call.started_at, "tts call started_at");
    const end = parseTimestamp(call.completed_at, "tts call completed_at");
    if (end <= start || end - start > 10 * 60 * 1000) fail("TTS call duration is invalid");
    for (const key of [
      "correct_voice", "correct_content", "audio_seams_checked", "media_round_trip",
      "caller_id_verified", "recording_disabled", "cdr_attempt_joined",
    ]) {
      assertBoolean(call[key], true, "tts call " + call.call_id + "." + key);
    }
    if (call.state !== "PASS") fail("TTS call must PASS");
    assertSafeReference(call.evidence_ref, "TTS call evidence_ref");
    assertSha(call.evidence_sha256, "TTS call evidence_sha256");
  });
}

function validateRealSimScenarios(scenarios) {
  if (!Array.isArray(scenarios) || scenarios.length !== REAL_SIM_SCENARIOS.length) {
    fail("real_sim_scenarios must contain exactly LAB-01..LAB-08");
  }
  scenarios.forEach((scenario, index) => {
    assertExactKeys(scenario, [
      "scenario_id", "destination_alias", "expected_observation", "actual_observation",
      "state", "evidence_ref", "evidence_sha256",
    ], "real_sim_scenarios[" + index + "]");
    const [id, observation] = REAL_SIM_SCENARIOS[index];
    if (scenario.scenario_id !== id || scenario.expected_observation !== observation ||
        scenario.actual_observation !== observation) {
      fail("real-SIM scenario matrix drift at index " + index);
    }
    if (scenario.destination_alias !== "LAB-A") fail("real-SIM destination must be opaque LAB-A");
    if (scenario.state !== "PASS") fail("real-SIM scenario must PASS");
    assertSafeReference(scenario.evidence_ref, "real-SIM evidence_ref");
    assertSha(scenario.evidence_sha256, "real-SIM evidence_sha256");
  });
}

function validateHardware(hardware) {
  assertExactKeys(hardware, [
    "vendor_alias", "product_family", "model_sku", "firmware_version", "channel_count",
    "volte_confirmed", "exact_sku_verified", "sim_channel_alias", "carrier_alias",
    "owner_destination_alias", "ownership_attested", "evidence_ref", "evidence_sha256",
  ], "hardware");
  for (const key of [
    "vendor_alias", "product_family", "model_sku", "firmware_version",
    "sim_channel_alias", "carrier_alias", "owner_destination_alias",
  ]) {
    assertAlias(hardware[key], "hardware." + key);
  }
  assertInteger(hardware.channel_count, 1, 64, "hardware.channel_count");
  assertBoolean(hardware.volte_confirmed, true, "hardware.volte_confirmed");
  assertBoolean(hardware.exact_sku_verified, true, "hardware.exact_sku_verified");
  assertBoolean(hardware.ownership_attested, true, "hardware.ownership_attested");
  if (hardware.owner_destination_alias !== "LAB-A") fail("owner destination alias must be LAB-A");
  assertSafeReference(hardware.evidence_ref, "hardware.evidence_ref");
  assertSha(hardware.evidence_sha256, "hardware.evidence_sha256");
}

function validateTopology(topology) {
  assertExactKeys(topology, [
    "diagram_ref", "diagram_sha256", "media_path", "dial_resolver_location",
    "ivr_sees_raw_e164", "ivr_stores_mapping_key", "dtmf_mode", "caller_id_policy_ref",
    "recording_disabled", "allowlist_enforced", "kill_switch_verified",
    "credential_from_secret_store", "cdr_uses_opaque_attempt_id",
  ], "topology");
  assertSafeReference(topology.diagram_ref, "topology.diagram_ref");
  assertSha(topology.diagram_sha256, "topology.diagram_sha256");
  if (topology.media_path !== "TTS->IVR_WORKER->ASTERISK->SIP_GATEWAY->SIM_PSTN->OWNER_DEVICE") {
    fail("topology.media_path drift");
  }
  if (topology.dial_resolver_location !== "TELEPHONY_BOUNDARY") fail("dial resolver must stay outside IVR");
  if (topology.dtmf_mode !== "RFC4733") fail("DTMF mode must be RFC4733");
  assertSafeReference(topology.caller_id_policy_ref, "topology.caller_id_policy_ref");
  assertBoolean(topology.ivr_sees_raw_e164, false, "topology.ivr_sees_raw_e164");
  assertBoolean(topology.ivr_stores_mapping_key, false, "topology.ivr_stores_mapping_key");
  for (const key of [
    "recording_disabled", "allowlist_enforced", "kill_switch_verified",
    "credential_from_secret_store", "cdr_uses_opaque_attempt_id",
  ]) {
    assertBoolean(topology[key], true, "topology." + key);
  }
}

function validateRetentionRollback(value) {
  assertExactKeys(value, [
    "retention_policy_ref", "retention_policy_sha256", "purge_proof_ref", "purge_proof_sha256",
    "raw_e164_absent_from_evidence", "audio_absent_from_evidence", "recording_absent",
    "rollback_ref", "rollback_sha256", "rollback_completed",
    "previous_provider_restored", "post_rollback_health_passed",
  ], "retention_rollback");
  for (const key of ["retention_policy_ref", "purge_proof_ref", "rollback_ref"]) {
    assertSafeReference(value[key], "retention_rollback." + key);
  }
  for (const key of ["retention_policy_sha256", "purge_proof_sha256", "rollback_sha256"]) {
    assertSha(value[key], "retention_rollback." + key);
  }
  for (const key of [
    "raw_e164_absent_from_evidence", "audio_absent_from_evidence", "recording_absent",
    "rollback_completed", "previous_provider_restored", "post_rollback_health_passed",
  ]) {
    assertBoolean(value[key], true, "retention_rollback." + key);
  }
}

function validateProductionReadiness(value, hardware) {
  assertExactKeys(value, [
    "selected_model_sku", "evaluated_vendor_count", "target_hardware_measured",
    "measured_concurrent_channels", "capacity_model_recalibrated", "internal_mirror_ready",
    "production_media_topology_approved", "vendor_disposition_contract_signed",
    "recording_off_contract_signed", "retention_contract_signed", "pilot_required",
    "cutover_default_disabled", "rollback_tested", "production_real_enabled",
  ], "production_readiness");
  if (value.selected_model_sku !== hardware.model_sku) fail("selected model/SKU does not match lab hardware");
  assertInteger(value.evaluated_vendor_count, 2, 20, "production_readiness.evaluated_vendor_count");
  assertInteger(value.measured_concurrent_channels, 1, 64, "production_readiness.measured_concurrent_channels");
  for (const key of [
    "target_hardware_measured", "capacity_model_recalibrated", "internal_mirror_ready",
    "production_media_topology_approved", "vendor_disposition_contract_signed",
    "recording_off_contract_signed", "retention_contract_signed", "pilot_required",
    "cutover_default_disabled", "rollback_tested",
  ]) {
    assertBoolean(value[key], true, "production_readiness." + key);
  }
  assertBoolean(value.production_real_enabled, false, "production_readiness.production_real_enabled");
}

function validateArtifacts(artifacts, expectedPins) {
  if (!Array.isArray(artifacts) || artifacts.length !== ARTIFACT_ROLES.length) {
    fail("artifacts must contain exactly the required ten roles");
  }
  const seen = new Set();
  artifacts.forEach((artifact, index) => {
    assertExactKeys(artifact, [
      "role", "artifact_ref", "sha256", "producer_alias", "reviewer_alias", "produced_at",
    ], "artifacts[" + index + "]");
    const role = ARTIFACT_ROLES[index];
    if (artifact.role !== role) fail("artifact role/order drift at index " + index);
    if (seen.has(role)) fail("duplicate artifact role");
    seen.add(role);
    assertSafeReference(artifact.artifact_ref, "artifact_ref");
    assertSha(artifact.sha256, "artifact.sha256");
    assertAlias(artifact.producer_alias, "artifact.producer_alias");
    assertAlias(artifact.reviewer_alias, "artifact.reviewer_alias");
    if (artifact.producer_alias === artifact.reviewer_alias) fail("artifact producer and reviewer must differ");
    parseTimestamp(artifact.produced_at, "artifact.produced_at");
    if (expectedPins.get(role) !== artifact.sha256) fail("independent artifact pin mismatch for " + role);
  });
  if (expectedPins.size !== ARTIFACT_ROLES.length) fail("independent artifact pin set is incomplete");
}

function validateSignoffs(signoffs, candidate) {
  if (!Array.isArray(signoffs) || signoffs.length !== SIGNOFF_ROLES.length) {
    fail("signoffs must contain exactly the required seven roles");
  }
  signoffs.forEach((signoff, index) => {
    assertExactKeys(signoff, [
      "role", "signer_alias", "verifier_alias", "authority_ref", "authority_sha256",
      "decision", "signed_at", "ivr_commit_sha", "tts_image_digest", "candidate_sha256",
    ], "signoffs[" + index + "]");
    if (signoff.role !== SIGNOFF_ROLES[index]) fail("signoff role/order drift at index " + index);
    assertAlias(signoff.signer_alias, "signoff.signer_alias");
    assertAlias(signoff.verifier_alias, "signoff.verifier_alias");
    if (signoff.signer_alias === signoff.verifier_alias) fail("signer and verifier must differ");
    assertSafeReference(signoff.authority_ref, "signoff.authority_ref");
    assertSha(signoff.authority_sha256, "signoff.authority_sha256");
    if (signoff.decision !== "APPROVED") fail("every required signoff must be APPROVED");
    parseTimestamp(signoff.signed_at, "signoff.signed_at");
    if (signoff.ivr_commit_sha !== candidate.ivr_commit_sha ||
        signoff.tts_image_digest !== candidate.tts_image_digest ||
        signoff.candidate_sha256 !== candidate.candidate_sha256) {
      fail("signoff candidate binding mismatch");
    }
  });
}

function validateSafety(safety) {
  const keys = [
    "contains_raw_e164", "contains_audio_or_transcript", "contains_credentials_or_secrets",
    "contains_customer_data", "recording_enabled", "validator_invokes_network_or_calls",
    "adapter_or_runtime_changed", "mock_claimed_as_real_sim", "lab_claimed_as_production",
    "production_real_enabled", "real_customer_call_allowed", "validator_claims_gate_attainment",
  ];
  assertExactKeys(safety, keys, "safety");
  for (const key of keys) assertBoolean(safety[key], false, "safety." + key);
}

function validateB3TelephonyEvidence(bundle, expected) {
  assertExactKeys(bundle, [
    "schema_version", "work_id", "evidence_state", "collected_at", "bundle_sha256",
    "source_pins", "candidate", "lab_scope", "hardware", "topology", "tts_calls",
    "real_sim_scenarios", "retention_rollback", "production_readiness",
    "artifacts", "signoffs", "safety",
  ], "bundle");
  if (bundle.schema_version !== 1 || bundle.work_id !== "W-0185") fail("bundle identity drift");
  if (bundle.evidence_state !== "COMPLETE_EXTERNAL_EVIDENCE") fail("input is not completed external evidence");
  parseTimestamp(bundle.collected_at, "collected_at");
  assertSha(bundle.bundle_sha256, "bundle_sha256");
  verifyLocalSourcePins(bundle.source_pins);
  const candidateSha = validateCandidate(bundle.candidate, expected);

  assertExactKeys(bundle.lab_scope, [
    "orders_are_fake", "destination_is_owner_controlled", "real_sim_used",
    "microsip_used_for_tts_route", "customer_calls_made", "call_count",
  ], "lab_scope");
  assertBoolean(bundle.lab_scope.orders_are_fake, true, "lab_scope.orders_are_fake");
  assertBoolean(bundle.lab_scope.destination_is_owner_controlled, true, "lab_scope.destination_is_owner_controlled");
  assertBoolean(bundle.lab_scope.real_sim_used, true, "lab_scope.real_sim_used");
  assertBoolean(bundle.lab_scope.microsip_used_for_tts_route, true, "lab_scope.microsip_used_for_tts_route");
  assertBoolean(bundle.lab_scope.customer_calls_made, false, "lab_scope.customer_calls_made");
  if (bundle.lab_scope.call_count !== 6) fail("lab_scope.call_count must be exactly 6");

  validateHardware(bundle.hardware);
  validateTopology(bundle.topology);
  validateTtsCalls(bundle.tts_calls);
  validateRealSimScenarios(bundle.real_sim_scenarios);
  validateRetentionRollback(bundle.retention_rollback);
  validateProductionReadiness(bundle.production_readiness, bundle.hardware);
  validateArtifacts(bundle.artifacts, expected.artifactPins);
  validateSignoffs(bundle.signoffs, bundle.candidate);
  validateSafety(bundle.safety);
  assertNoSensitiveMaterial(bundle);

  const actualBundleSha = sha256(Buffer.from(canonicalizeB3TelephonyEvidence(bundle), "utf8"));
  if (bundle.bundle_sha256 !== actualBundleSha) fail("bundle canonical hash mismatch");
  if (bundle.bundle_sha256 !== expected.bundleSha) fail("independent bundle hash mismatch");
  return {
    eligibleForEvidenceReview: true,
    candidateSha,
    bundleSha: actualBundleSha,
    ttsCalls: bundle.tts_calls.length,
    realSimScenarios: bundle.real_sim_scenarios.length,
    artifacts: bundle.artifacts.length,
    signoffs: bundle.signoffs.length,
  };
}

function buildPendingTemplate() {
  const pendingCall = ([callId, region, fixture, digit, result]) => ({
    call_id: callId,
    region,
    fixture_alias: fixture,
    destination_alias: "LAB-A",
    route: "ASTERISK_MICROSIP_8KHZ_TARGET_HARDWARE",
    started_at: "PENDING",
    completed_at: "PENDING",
    dtmf_sent: digit,
    expected_result: result,
    actual_result: "PENDING",
    correct_voice: false,
    correct_content: false,
    audio_seams_checked: false,
    media_round_trip: false,
    caller_id_verified: false,
    recording_disabled: false,
    cdr_attempt_joined: false,
    state: "PENDING",
    evidence_ref: "PENDING",
    evidence_sha256: "PENDING",
  });
  return {
    schema_version: 1,
    work_id: "W-0185",
    evidence_state: "PENDING_EXTERNAL_INPUT",
    collected_at: "PENDING",
    bundle_sha256: "PENDING",
    source_pins: { ...SOURCE_PINS },
    candidate: {
      ivr_commit_sha: "PENDING",
      tts_image_digest: "PENDING",
      tts_model_bundle_sha256: "PENDING",
      fixed_catalog_sha256: "PENDING",
      configuration_sha256: "PENDING",
      candidate_sha256: "PENDING",
    },
    lab_scope: {
      orders_are_fake: true,
      destination_is_owner_controlled: false,
      real_sim_used: false,
      microsip_used_for_tts_route: false,
      customer_calls_made: false,
      call_count: 0,
    },
    hardware: {
      vendor_alias: "PENDING",
      product_family: "PENDING",
      model_sku: "PENDING",
      firmware_version: "PENDING",
      channel_count: 0,
      volte_confirmed: false,
      exact_sku_verified: false,
      sim_channel_alias: "PENDING",
      carrier_alias: "PENDING",
      owner_destination_alias: "LAB-A",
      ownership_attested: false,
      evidence_ref: "PENDING",
      evidence_sha256: "PENDING",
    },
    topology: {
      diagram_ref: "PENDING",
      diagram_sha256: "PENDING",
      media_path: "TTS->IVR_WORKER->ASTERISK->SIP_GATEWAY->SIM_PSTN->OWNER_DEVICE",
      dial_resolver_location: "PENDING",
      ivr_sees_raw_e164: false,
      ivr_stores_mapping_key: false,
      dtmf_mode: "PENDING",
      caller_id_policy_ref: "PENDING",
      recording_disabled: false,
      allowlist_enforced: false,
      kill_switch_verified: false,
      credential_from_secret_store: false,
      cdr_uses_opaque_attempt_id: false,
    },
    tts_calls: TTS_CALL_MATRIX.map(pendingCall),
    real_sim_scenarios: REAL_SIM_SCENARIOS.map(([scenarioId, observation]) => ({
      scenario_id: scenarioId,
      destination_alias: "LAB-A",
      expected_observation: observation,
      actual_observation: "PENDING",
      state: "PENDING",
      evidence_ref: "PENDING",
      evidence_sha256: "PENDING",
    })),
    retention_rollback: {
      retention_policy_ref: "PENDING",
      retention_policy_sha256: "PENDING",
      purge_proof_ref: "PENDING",
      purge_proof_sha256: "PENDING",
      raw_e164_absent_from_evidence: false,
      audio_absent_from_evidence: false,
      recording_absent: false,
      rollback_ref: "PENDING",
      rollback_sha256: "PENDING",
      rollback_completed: false,
      previous_provider_restored: false,
      post_rollback_health_passed: false,
    },
    production_readiness: {
      selected_model_sku: "PENDING",
      evaluated_vendor_count: 0,
      target_hardware_measured: false,
      measured_concurrent_channels: 0,
      capacity_model_recalibrated: false,
      internal_mirror_ready: false,
      production_media_topology_approved: false,
      vendor_disposition_contract_signed: false,
      recording_off_contract_signed: false,
      retention_contract_signed: false,
      pilot_required: true,
      cutover_default_disabled: true,
      rollback_tested: false,
      production_real_enabled: false,
    },
    artifacts: ARTIFACT_ROLES.map((role) => ({
      role,
      artifact_ref: "PENDING",
      sha256: "PENDING",
      producer_alias: "PENDING",
      reviewer_alias: "PENDING",
      produced_at: "PENDING",
    })),
    signoffs: SIGNOFF_ROLES.map((role) => ({
      role,
      signer_alias: "PENDING",
      verifier_alias: "PENDING",
      authority_ref: "PENDING",
      authority_sha256: "PENDING",
      decision: "PENDING",
      signed_at: "PENDING",
      ivr_commit_sha: "PENDING",
      tts_image_digest: "PENDING",
      candidate_sha256: "PENDING",
    })),
    safety: {
      contains_raw_e164: false,
      contains_audio_or_transcript: false,
      contains_credentials_or_secrets: false,
      contains_customer_data: false,
      recording_enabled: false,
      validator_invokes_network_or_calls: false,
      adapter_or_runtime_changed: false,
      mock_claimed_as_real_sim: false,
      lab_claimed_as_production: false,
      production_real_enabled: false,
      real_customer_call_allowed: false,
      validator_claims_gate_attainment: false,
    },
  };
}

function completeFixture() {
  const value = buildPendingTemplate();
  const ivrSha = "a".repeat(40);
  const imageDigest = "sha256:" + hashLabel("TTS_IMAGE");
  value.evidence_state = "COMPLETE_EXTERNAL_EVIDENCE";
  value.collected_at = "2026-09-04T08:00:00Z";
  Object.assign(value.candidate, {
    ivr_commit_sha: ivrSha,
    tts_image_digest: imageDigest,
    tts_model_bundle_sha256: hashLabel("MODEL_BUNDLE"),
    fixed_catalog_sha256: hashLabel("FIXED_CATALOG"),
    configuration_sha256: hashLabel("CONFIGURATION"),
  });
  value.candidate.candidate_sha256 = sha256(Buffer.from(canonicalizeCandidate(value.candidate), "utf8"));
  Object.assign(value.lab_scope, {
    destination_is_owner_controlled: true,
    real_sim_used: true,
    microsip_used_for_tts_route: true,
    call_count: 6,
  });
  Object.assign(value.hardware, {
    vendor_alias: "VENDOR-A",
    product_family: "VOLTE-GATEWAY",
    model_sku: "MODEL-SKU-A",
    firmware_version: "FW-1.0.0",
    channel_count: 4,
    volte_confirmed: true,
    exact_sku_verified: true,
    sim_channel_alias: "SIM-LAB-A",
    carrier_alias: "CARRIER-A",
    ownership_attested: true,
    evidence_ref: "EVIDENCE:HARDWARE",
    evidence_sha256: hashLabel("HARDWARE"),
  });
  Object.assign(value.topology, {
    diagram_ref: "EVIDENCE:TOPOLOGY",
    diagram_sha256: hashLabel("TOPOLOGY"),
    dial_resolver_location: "TELEPHONY_BOUNDARY",
    dtmf_mode: "RFC4733",
    caller_id_policy_ref: "POLICY:CALLER-ID",
    recording_disabled: true,
    allowlist_enforced: true,
    kill_switch_verified: true,
    credential_from_secret_store: true,
    cdr_uses_opaque_attempt_id: true,
  });
  value.tts_calls.forEach((call, index) => {
    call.started_at = new Date(Date.UTC(2026, 8, 4, 8, index * 2, 0)).toISOString();
    call.completed_at = new Date(Date.UTC(2026, 8, 4, 8, index * 2, 45)).toISOString();
    call.actual_result = call.expected_result;
    for (const key of [
      "correct_voice", "correct_content", "audio_seams_checked", "media_round_trip",
      "caller_id_verified", "recording_disabled", "cdr_attempt_joined",
    ]) call[key] = true;
    call.state = "PASS";
    call.evidence_ref = "EVIDENCE:" + call.call_id;
    call.evidence_sha256 = hashLabel(call.call_id);
  });
  value.real_sim_scenarios.forEach((scenario) => {
    scenario.actual_observation = scenario.expected_observation;
    scenario.state = "PASS";
    scenario.evidence_ref = "EVIDENCE:" + scenario.scenario_id;
    scenario.evidence_sha256 = hashLabel(scenario.scenario_id);
  });
  Object.assign(value.retention_rollback, {
    retention_policy_ref: "POLICY:RETENTION",
    retention_policy_sha256: hashLabel("RETENTION_POLICY"),
    purge_proof_ref: "EVIDENCE:PURGE",
    purge_proof_sha256: hashLabel("PURGE"),
    raw_e164_absent_from_evidence: true,
    audio_absent_from_evidence: true,
    recording_absent: true,
    rollback_ref: "EVIDENCE:ROLLBACK",
    rollback_sha256: hashLabel("ROLLBACK"),
    rollback_completed: true,
    previous_provider_restored: true,
    post_rollback_health_passed: true,
  });
  Object.assign(value.production_readiness, {
    selected_model_sku: value.hardware.model_sku,
    evaluated_vendor_count: 3,
    target_hardware_measured: true,
    measured_concurrent_channels: 4,
    capacity_model_recalibrated: true,
    internal_mirror_ready: true,
    production_media_topology_approved: true,
    vendor_disposition_contract_signed: true,
    recording_off_contract_signed: true,
    retention_contract_signed: true,
    rollback_tested: true,
  });
  const artifactPins = new Map();
  value.artifacts.forEach((artifact) => {
    artifact.artifact_ref = "EVIDENCE:" + artifact.role;
    artifact.sha256 = hashLabel(artifact.role);
    artifact.producer_alias = artifact.role + "-OWNER";
    artifact.reviewer_alias = artifact.role + "-REVIEWER";
    artifact.produced_at = "2026-09-04T09:00:00Z";
    artifactPins.set(artifact.role, artifact.sha256);
  });
  value.signoffs.forEach((signoff) => {
    signoff.signer_alias = signoff.role + "-SIGNER";
    signoff.verifier_alias = signoff.role + "-VERIFIER";
    signoff.authority_ref = "AUTHORITY:" + signoff.role;
    signoff.authority_sha256 = hashLabel("AUTHORITY:" + signoff.role);
    signoff.decision = "APPROVED";
    signoff.signed_at = "2026-09-04T10:00:00Z";
    signoff.ivr_commit_sha = ivrSha;
    signoff.tts_image_digest = imageDigest;
    signoff.candidate_sha256 = value.candidate.candidate_sha256;
  });
  value.bundle_sha256 = sha256(Buffer.from(canonicalizeB3TelephonyEvidence(value), "utf8"));
  return {
    value,
    expected: {
      bundleSha: value.bundle_sha256,
      candidateSha: value.candidate.candidate_sha256,
      ivrSha,
      ttsImageDigest: imageDigest,
      artifactPins,
    },
  };
}

class DuplicateSafeJsonParser {
  constructor(text) {
    this.text = text;
    this.offset = 0;
  }

  parse() {
    const value = this.parseValue("$");
    this.skipWhitespace();
    if (this.offset !== this.text.length) fail("unexpected JSON token at offset " + this.offset);
    return value;
  }

  skipWhitespace() {
    while (/\s/u.test(this.text[this.offset] ?? "")) this.offset += 1;
  }

  parseValue(path) {
    this.skipWhitespace();
    const ch = this.text[this.offset];
    if (ch === "{") return this.parseObject(path);
    if (ch === "[") return this.parseArray(path);
    if (ch === "\"") return this.parseString();
    const primitive = this.text.slice(this.offset).match(/^(?:true|false|null|-?(?:0|[1-9]\d*)(?:\.\d+)?(?:[eE][+-]?\d+)?)/u)?.[0];
    if (!primitive) fail("invalid JSON value at " + path);
    this.offset += primitive.length;
    return JSON.parse(primitive);
  }

  parseString() {
    const start = this.offset;
    this.offset += 1;
    let escaped = false;
    while (this.offset < this.text.length) {
      const ch = this.text[this.offset];
      this.offset += 1;
      if (!escaped && ch === "\"") return JSON.parse(this.text.slice(start, this.offset));
      if (!escaped && ch === "\\") escaped = true;
      else escaped = false;
    }
    fail("unterminated JSON string at offset " + start);
  }

  parseObject(path) {
    const result = {};
    const keys = new Set();
    this.offset += 1;
    this.skipWhitespace();
    if (this.text[this.offset] === "}") {
      this.offset += 1;
      return result;
    }
    while (true) {
      this.skipWhitespace();
      if (this.text[this.offset] !== "\"") fail("object key expected at " + path);
      const key = this.parseString();
      if (keys.has(key)) fail("duplicate JSON key at " + path + "." + key);
      keys.add(key);
      this.skipWhitespace();
      if (this.text[this.offset] !== ":") fail("colon expected at " + path + "." + key);
      this.offset += 1;
      result[key] = this.parseValue(path + "." + key);
      this.skipWhitespace();
      if (this.text[this.offset] === "}") {
        this.offset += 1;
        return result;
      }
      if (this.text[this.offset] !== ",") fail("comma expected at " + path);
      this.offset += 1;
    }
  }

  parseArray(path) {
    const result = [];
    this.offset += 1;
    this.skipWhitespace();
    if (this.text[this.offset] === "]") {
      this.offset += 1;
      return result;
    }
    while (true) {
      result.push(this.parseValue(path + "[" + result.length + "]"));
      this.skipWhitespace();
      if (this.text[this.offset] === "]") {
        this.offset += 1;
        return result;
      }
      if (this.text[this.offset] !== ",") fail("comma expected at " + path);
      this.offset += 1;
    }
  }
}

function readSafeJson(inputPath) {
  const absolute = resolve(inputPath);
  const repositoryReal = realpathSync(REPOSITORY_ROOT);
  const linkInfo = lstatSync(absolute);
  if (linkInfo.isSymbolicLink() || !linkInfo.isFile()) fail("input must be a regular non-symlink file");
  const inputReal = realpathSync(absolute);
  const relativePath = relative(repositoryReal, inputReal);
  if (relativePath === "" || relativePath.startsWith("..") || isAbsolute(relativePath)) {
    fail("input must be a file inside the repository");
  }
  if (statSync(inputReal).size > MAX_INPUT_BYTES) fail("input exceeds 512 KiB");
  const bytes = readFileSync(inputReal);
  if (bytes.subarray(0, 3).equals(Buffer.from([0xef, 0xbb, 0xbf]))) fail("UTF-8 BOM is forbidden");
  const text = bytes.toString("utf8");
  if (Buffer.from(text, "utf8").compare(bytes) !== 0) fail("input must be valid UTF-8");
  return new DuplicateSafeJsonParser(text).parse();
}

function parseArguments(argv) {
  const parsed = { artifactPins: new Map() };
  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index];
    if (arg === "--self-test") parsed.selfTest = true;
    else if (arg === "--print-template") parsed.printTemplate = true;
    else if (arg === "--check-template") parsed.checkTemplate = argv[++index];
    else if (arg === "--input") parsed.input = argv[++index];
    else if (arg === "--expected-bundle-sha") parsed.bundleSha = argv[++index];
    else if (arg === "--expected-candidate-sha") parsed.candidateSha = argv[++index];
    else if (arg === "--expected-ivr-sha") parsed.ivrSha = argv[++index];
    else if (arg === "--expected-tts-image-digest") parsed.ttsImageDigest = argv[++index];
    else if (arg === "--expected-artifact") {
      const pair = argv[++index] ?? "";
      const separator = pair.indexOf("=");
      if (separator < 1) fail("--expected-artifact requires ROLE=SHA256");
      const role = pair.slice(0, separator);
      const hash = pair.slice(separator + 1);
      if (parsed.artifactPins.has(role)) fail("duplicate independent artifact pin: " + role);
      parsed.artifactPins.set(role, hash);
    } else fail("unknown argument: " + arg);
  }
  return parsed;
}

function assertExpectedArguments(options) {
  assertSha(options.bundleSha, "--expected-bundle-sha");
  assertSha(options.candidateSha, "--expected-candidate-sha");
  assertGitSha(options.ivrSha, "--expected-ivr-sha");
  if (typeof options.ttsImageDigest !== "string" ||
      !/^sha256:[a-f0-9]{64}$/u.test(options.ttsImageDigest)) {
    fail("--expected-tts-image-digest must be a sha256 digest");
  }
  const roles = [...options.artifactPins.keys()];
  if (JSON.stringify(roles.sort()) !== JSON.stringify([...ARTIFACT_ROLES].sort())) {
    fail("--expected-artifact must supply each required role exactly once");
  }
  for (const [role, hash] of options.artifactPins) assertSha(hash, "--expected-artifact " + role);
}

function runSelfTest() {
  const { value, expected } = completeFixture();
  const result = validateB3TelephonyEvidence(value, expected);
  if (!result.eligibleForEvidenceReview) fail("positive fixture was not eligible");

  const mutations = [
    ["identity", (v) => { v.work_id = "W-0122"; }],
    ["state", (v) => { v.evidence_state = "PENDING_EXTERNAL_INPUT"; }],
    ["source pin", (v) => { v.source_pins["docs/lab/one-sim-lab-plan.md"] = hashLabel("DRIFT"); }],
    ["candidate hash", (v) => { v.candidate.configuration_sha256 = hashLabel("DRIFT"); }],
    ["customer call", (v) => { v.lab_scope.customer_calls_made = true; }],
    ["call count", (v) => { v.lab_scope.call_count = 5; }],
    ["missing TTS call", (v) => { v.tts_calls.pop(); }],
    ["TTS call order", (v) => { [v.tts_calls[0], v.tts_calls[1]] = [v.tts_calls[1], v.tts_calls[0]]; }],
    ["wrong TTS result", (v) => { v.tts_calls[0].actual_result = "IVR_CUSTOMER_CANCELLED"; }],
    ["voice unchecked", (v) => { v.tts_calls[0].correct_voice = false; }],
    ["recording present", (v) => { v.tts_calls[0].recording_disabled = false; }],
    ["missing real-SIM scenario", (v) => { v.real_sim_scenarios.pop(); }],
    ["wrong real-SIM result", (v) => { v.real_sim_scenarios[6].actual_observation = "NO_ANSWER"; }],
    ["non-VoLTE hardware", (v) => { v.hardware.volte_confirmed = false; }],
    ["raw E164 visible", (v) => { v.topology.ivr_sees_raw_e164 = true; }],
    ["recording topology", (v) => { v.topology.recording_disabled = false; }],
    ["allowlist disabled", (v) => { v.topology.allowlist_enforced = false; }],
    ["kill switch unverified", (v) => { v.topology.kill_switch_verified = false; }],
    ["retention missing", (v) => { v.retention_rollback.recording_absent = false; }],
    ["rollback missing", (v) => { v.retention_rollback.rollback_completed = false; }],
    ["procurement mismatch", (v) => { v.production_readiness.selected_model_sku = "OTHER-SKU"; }],
    ["production enabled", (v) => { v.production_readiness.production_real_enabled = true; }],
    ["missing artifact", (v) => { v.artifacts.pop(); }],
    ["artifact custody", (v) => { v.artifacts[0].reviewer_alias = v.artifacts[0].producer_alias; }],
    ["missing signoff", (v) => { v.signoffs.pop(); }],
    ["signoff denied", (v) => { v.signoffs[0].decision = "PENDING"; }],
    ["signoff custody", (v) => { v.signoffs[0].verifier_alias = v.signoffs[0].signer_alias; }],
    ["signoff binding", (v) => { v.signoffs[0].ivr_commit_sha = "b".repeat(40); }],
    ["raw phone", (v) => { v.hardware.product_family = "CALL-" + ["090", "123", "4567"].join(""); }],
    ["real customer allowed", (v) => { v.safety.real_customer_call_allowed = true; }],
    ["gate claim", (v) => { v.safety.validator_claims_gate_attainment = true; }],
  ];
  let refused = 0;
  for (const [label, mutate] of mutations) {
    const changed = structuredClone(value);
    mutate(changed);
    changed.bundle_sha256 = sha256(Buffer.from(canonicalizeB3TelephonyEvidence(changed), "utf8"));
    const changedExpected = { ...expected, bundleSha: changed.bundle_sha256 };
    try {
      validateB3TelephonyEvidence(changed, changedExpected);
      fail("mutation was accepted: " + label);
    } catch (error) {
      if (String(error.message).startsWith("mutation was accepted")) throw error;
      refused += 1;
    }
  }

  const wrongArtifactPins = new Map(expected.artifactPins);
  wrongArtifactPins.set("LAB_ACCEPTANCE_REPORT", hashLabel("WRONG"));
  try {
    validateB3TelephonyEvidence(value, { ...expected, artifactPins: wrongArtifactPins });
    fail("independent artifact mismatch was accepted");
  } catch (error) {
    if (String(error.message).startsWith("independent artifact mismatch was accepted")) throw error;
    refused += 1;
  }

  const pending = buildPendingTemplate();
  if (pending.evidence_state !== "PENDING_EXTERNAL_INPUT" ||
      pending.tts_calls.length !== 6 || pending.real_sim_scenarios.length !== 8 ||
      pending.artifacts.length !== 10 || pending.signoffs.length !== 7) {
    fail("pending template shape drift");
  }
  try {
    validateB3TelephonyEvidence(pending, expected);
    fail("pending template was accepted as complete");
  } catch (error) {
    if (String(error.message).startsWith("pending template was accepted")) throw error;
    refused += 1;
  }
  try {
    new DuplicateSafeJsonParser('{"work_id":"W-0185","work_id":"W-0122"}').parse();
    fail("duplicate JSON key was accepted");
  } catch (error) {
    if (String(error.message).startsWith("duplicate JSON key was accepted")) throw error;
    refused += 1;
  }

  process.stdout.write(
    "B3_TELEPHONY_EVIDENCE_SELF_TEST_PASS valid=1 refusal=" + refused +
    " tts_calls=6 real_sim_scenarios=8 artifacts=10 signoffs=7\n",
  );
}

function usage() {
  return [
    "Usage:",
    "  node deploy/ci/scripts/b3-telephony-evidence-validator.mjs --self-test",
    "  node deploy/ci/scripts/b3-telephony-evidence-validator.mjs --print-template",
    "  node deploy/ci/scripts/b3-telephony-evidence-validator.mjs --check-template <path>",
    "  node deploy/ci/scripts/b3-telephony-evidence-validator.mjs --input <path> \\",
    "    --expected-bundle-sha <64hex> --expected-candidate-sha <64hex> \\",
    "    --expected-ivr-sha <40hex> --expected-tts-image-digest sha256:<64hex> \\",
    "    --expected-artifact ROLE=SHA256  # repeat for all ten roles",
  ].join("\n");
}

const options = parseArguments(process.argv.slice(2));
try {
  if (options.selfTest) {
    runSelfTest();
  } else if (options.printTemplate) {
    process.stdout.write(JSON.stringify(buildPendingTemplate(), null, 2) + "\n");
  } else if (options.checkTemplate) {
    const template = readSafeJson(options.checkTemplate);
    if (stableStringify(template) !== stableStringify(buildPendingTemplate())) {
      fail("template content drift");
    }
    process.stdout.write("B3_TELEPHONY_TEMPLATE_VALID_NOT_READY tts_calls=6 real_sim_scenarios=8 artifacts=10 signoffs=7\n");
  } else if (options.input) {
    assertExpectedArguments(options);
    const input = readSafeJson(options.input);
    const result = validateB3TelephonyEvidence(input, options);
    process.stdout.write(
      "B3_TELEPHONY_EVIDENCE_PASS eligible=EVIDENCE_REVIEW_ONLY bundle_sha256=" +
      result.bundleSha + " candidate_sha256=" + result.candidateSha +
      " tts_calls=" + result.ttsCalls + " real_sim_scenarios=" + result.realSimScenarios +
      " artifacts=" + result.artifacts + " signoffs=" + result.signoffs +
      " production_authorized=NO real_customer_call_allowed=NO\n",
    );
  } else {
    fail(usage());
  }
} catch (error) {
  process.stderr.write("B3_TELEPHONY_EVIDENCE_REFUSED " + error.message + "\n");
  process.exitCode = 1;
}

export {
  ARTIFACT_ROLES,
  REAL_SIM_SCENARIOS,
  SIGNOFF_ROLES,
  SOURCE_PINS,
  TTS_CALL_MATRIX,
  buildPendingTemplate,
  canonicalizeB3TelephonyEvidence,
  validateB3TelephonyEvidence,
};
