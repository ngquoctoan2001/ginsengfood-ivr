#!/bin/sh
# W-0053 / P10-2 — encrypted logical backup.
#
# Two properties this script exists to guarantee, both of which a plain
# "pg_dump > file.sql && encrypt file.sql" gets wrong:
#
#   1. The plaintext dump never reaches a filesystem. pg_dump is piped straight
#      into the cipher, so there is no window in which an unencrypted copy of a
#      PiiDirect table sits on a volume waiting to be cleaned up.
#   2. The artefact is authenticated, not merely encrypted. AES-CTR without a MAC
#      is malleable: flipping a bit of ciphertext flips the same bit of plaintext,
#      and a restore would then apply attacker-chosen SQL to the database it was
#      supposed to rescue. Encrypt-then-MAC, verified before a single byte is
#      decrypted, is what makes a restore safe rather than merely possible.
#
# Key handling: one master key in, two independent subkeys derived. Using one key
# for both confidentiality and integrity is the classic mistake that turns two
# guarantees into one.
#
# Inputs (environment):
#   PGHOST PGPORT PGUSER PGPASSWORD PGDATABASE  standard libpq
#   IVR_BACKUP_KEY_FILE   file holding at least 64 hex chars of master key material
#   IVR_BACKUP_DIR        destination directory
#   IVR_BACKUP_LABEL      optional label placed in the artefact name
set -eu

: "${IVR_BACKUP_KEY_FILE:?IVR_BACKUP_KEY_FILE is required}"
: "${IVR_BACKUP_DIR:?IVR_BACKUP_DIR is required}"
: "${PGDATABASE:?PGDATABASE is required}"

# Refusing loudly matters more than it looks. The failure this prevents is a
# backup job that cannot encrypt, writes plaintext instead, and reports success.
command -v openssl >/dev/null 2>&1 || {
  echo "BACKUP_FAIL: openssl is not available; refusing to write an unencrypted backup" >&2
  exit 2
}
command -v pg_dump >/dev/null 2>&1 || {
  echo "BACKUP_FAIL: pg_dump is not available" >&2
  exit 2
}

master=$(tr -d '\n\r ' < "$IVR_BACKUP_KEY_FILE")
key_length=$(printf '%s' "$master" | wc -c | tr -d ' ')
if [ "$key_length" -lt 64 ]; then
  echo "BACKUP_FAIL: master key is ${key_length} hex chars; at least 64 (256 bits) required" >&2
  exit 2
fi

derive() {
  printf '%s' "$1" \
    | openssl dgst -sha256 -mac HMAC -macopt "hexkey:${master}" -r \
    | cut -d' ' -f1
}

key_enc=$(derive 'ivr-backup-enc-v1')
key_mac=$(derive 'ivr-backup-mac-v1')
iv=$(openssl rand -hex 16)

stamp=$(date -u '+%Y%m%dT%H%M%SZ')
label="${IVR_BACKUP_LABEL:-full}"
base="${IVR_BACKUP_DIR}/ivr-${label}-${stamp}"
mkdir -p "$IVR_BACKUP_DIR"

status_file=$(mktemp)
trap 'rm -f "$status_file"' EXIT HUP INT TERM
echo 0 > "$status_file"

# POSIX sh has no pipefail, and a silently truncated dump that encrypts cleanly is
# the worst possible outcome: a backup that restores into a partial database.
{ pg_dump --no-owner --no-privileges "$PGDATABASE" || echo "$?" > "$status_file"; } \
  | openssl enc -aes-256-ctr -K "$key_enc" -iv "$iv" > "${base}.sql.enc"

dump_status=$(cat "$status_file")
if [ "$dump_status" != "0" ]; then
  rm -f "${base}.sql.enc"
  echo "BACKUP_FAIL: pg_dump exited ${dump_status}; artefact discarded" >&2
  exit 1
fi

printf '%s' "$iv" > "${base}.iv"
cat "${base}.iv" "${base}.sql.enc" \
  | openssl dgst -sha256 -mac HMAC -macopt "hexkey:${key_mac}" -r \
  | cut -d' ' -f1 > "${base}.hmac"

bytes=$(wc -c < "${base}.sql.enc" | tr -d ' ')
{
  echo "database=${PGDATABASE}"
  echo "label=${label}"
  echo "created_utc=${stamp}"
  echo "cipher=aes-256-ctr"
  echo "integrity=hmac-sha256-encrypt-then-mac"
  echo "ciphertext_bytes=${bytes}"
} > "${base}.meta"

echo "BACKUP_OK ${base}.sql.enc ${bytes} bytes"
