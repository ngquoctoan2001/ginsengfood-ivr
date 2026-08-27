# W-0122 lab runbook

Status: `NOT_RUN`. This runbook does not authorize a real customer call.

## Preconditions

- `models/MODELS.lock` bundle passes `verify-model.py --mode nonprod`.
- Owner has listened to all 11 candidates through the 8 kHz Asterisk/MicroSIP path and signed
  `voice-acceptance-manifest.json` for exactly one North, Central and South preset; the separate
  artifact passes `tts-voice-acceptance-gate.mjs --acceptance`.
- The 12 fixed WAV files and generated catalog config exist and the Asterisk image was rebuilt.
- Only fake order data and the opaque allowlisted lab destination are used.

## Compose validation

Set the three accepted voice IDs, their accepted rates, the verified model bundle directory,
`IVR_VIENEU_VOICE_ACCEPTANCE_MANIFEST` to the signed artifact, and existing ARI/SIP lab secrets.
Then validate all three overlays:

```powershell
docker compose -f docker-compose.dev.yml -f docker-compose.softphone.yml `
  -f docker-compose.vieneu-tts.yml config --quiet
```

Start the stack. `ivr-tts` shares `ivr-worker` networking, so there is intentionally no TTS host
port. The worker image is chiseled and has no shell/HTTP client; use the TTS container's Python
client, which reaches the same loopback namespace:

```powershell
docker compose -f docker-compose.dev.yml -f docker-compose.softphone.yml `
  -f docker-compose.vieneu-tts.yml exec ivr-tts python -c `
  "import urllib.request; print(urllib.request.urlopen('http://127.0.0.1:8090/health/ready').status)"
```

Do not invoke the call script before readiness returns `200` and the permission probe below passes.

## Permission probe

1. Inspect worker user and volume owner/group/mode; expected worker UID/GID and volume owner are
   `1654:1654`, mode `0750`.
2. Have the worker runtime create a random non-customer probe file in `/var/lib/ivr/speech`.
3. Confirm Asterisk reads identical bytes at `/var/lib/asterisk/sounds/generated`.
4. Confirm an Asterisk write fails because its mount is read-only, then remove the probe.

Record commands and outputs in `lab-call-results.md`; never record order text/audio in logs.

## Acceptance and rollback

Run exactly 2 fake orders × 3 regions, listen to every fixed/dynamic seam, verify media round-trip,
run the isolated retention procedure, then deliberately restore the previously accepted provider
configuration. Record duration and outcome. No silent SaaS fallback is allowed.
