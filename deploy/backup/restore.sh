#!/bin/sh
# W-0053 / P10-2 — verify, then decrypt, then restore. In that order.
#
# The order is the point. Verifying after decrypting means the attacker-chosen
# SQL has already been produced, and in most implementations already piped
# somewhere. Verifying first means a tampered artefact never becomes plaintext at
# all.
#
# Inputs (environment):
#   PGHOST PGPORT PGUSER PGPASSWORD  standard libpq
#   IVR_BACKUP_KEY_FILE   master key material
#   IVR_RESTORE_TARGET    database to create and restore into
# Argument 1: path to the .sql.enc artefact
set -eu

artefact="${1:?usage: restore.sh <artefact.sql.enc>}"
: "${IVR_BACKUP_KEY_FILE:?IVR_BACKUP_KEY_FILE is required}"
: "${IVR_RESTORE_TARGET:?IVR_RESTORE_TARGET is required}"

base="${artefact%.sql.enc}"
[ -f "${base}.iv" ] || { echo "RESTORE_FAIL: missing ${base}.iv" >&2; exit 2; }
[ -f "${base}.hmac" ] || { echo "RESTORE_FAIL: missing ${base}.hmac" >&2; exit 2; }

master=$(tr -d '\n\r ' < "$IVR_BACKUP_KEY_FILE")
derive() {
  printf '%s' "$1" \
    | openssl dgst -sha256 -mac HMAC -macopt "hexkey:${master}" -r \
    | cut -d' ' -f1
}
key_enc=$(derive 'ivr-backup-enc-v1')
key_mac=$(derive 'ivr-backup-mac-v1')

expected=$(tr -d '\n\r ' < "${base}.hmac")
actual=$(cat "${base}.iv" "$artefact" \
  | openssl dgst -sha256 -mac HMAC -macopt "hexkey:${key_mac}" -r \
  | cut -d' ' -f1)

if [ "$expected" != "$actual" ]; then
  # No detail about which byte differs, and nothing decrypted. An artefact that
  # fails here is either corrupt or hostile, and the two are handled the same way.
  echo "RESTORE_REFUSED: integrity check failed for ${artefact}" >&2
  exit 3
fi

iv=$(tr -d '\n\r ' < "${base}.iv")

psql -v ON_ERROR_STOP=1 -d postgres \
  -c "DROP DATABASE IF EXISTS \"${IVR_RESTORE_TARGET}\";" \
  -c "CREATE DATABASE \"${IVR_RESTORE_TARGET}\";" >/dev/null

openssl enc -d -aes-256-ctr -K "$key_enc" -iv "$iv" -in "$artefact" \
  | psql -v ON_ERROR_STOP=1 -d "$IVR_RESTORE_TARGET" >/dev/null

echo "RESTORE_OK ${IVR_RESTORE_TARGET}"
