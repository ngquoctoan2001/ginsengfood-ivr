#!/bin/sh
# W-0053 / P10-2 — retention applies to backups too (DF-07).
#
# A 90-day backup of a 30-day table means the real retention period of that data
# is 90 days and the number 30 is a description. This is the half of DF-07 that
# lives outside the database, and it is the half that gets forgotten because
# nothing in the application can see it.
#
# Dry run is the default, matching the retention job it mirrors: a scheduled
# delete of customer data whose default is wrong cannot be undone.
#
# Inputs:
#   IVR_BACKUP_DIR             directory holding artefacts
#   IVR_BACKUP_MAX_AGE_DAYS    maximum age; must not exceed the longest configured
#                              retention period
#   IVR_BACKUP_DRY_RUN         "true" (default) reports without deleting
set -eu

: "${IVR_BACKUP_DIR:?IVR_BACKUP_DIR is required}"
: "${IVR_BACKUP_MAX_AGE_DAYS:?IVR_BACKUP_MAX_AGE_DAYS is required}"
dry_run="${IVR_BACKUP_DRY_RUN:-true}"

case "$IVR_BACKUP_MAX_AGE_DAYS" in
  ''|*[!0-9]*)
    echo "PRUNE_FAIL: IVR_BACKUP_MAX_AGE_DAYS must be a whole number" >&2
    exit 2
    ;;
esac

expired=$(find "$IVR_BACKUP_DIR" -type f -name 'ivr-*.sql.enc' \
  -mtime "+${IVR_BACKUP_MAX_AGE_DAYS}" | sort)

count=0
for artefact in $expired; do
  count=$((count + 1))
  if [ "$dry_run" = "true" ]; then
    echo "PRUNE_WOULD_DELETE ${artefact}"
  else
    base="${artefact%.sql.enc}"
    # Siblings go with it. An orphan .iv or .hmac is harmless, but an orphan
    # .meta reads like a backup that still exists.
    rm -f "${base}.sql.enc" "${base}.iv" "${base}.hmac" "${base}.meta"
    echo "PRUNE_DELETED ${artefact}"
  fi
done

if [ "$dry_run" = "true" ]; then
  echo "PRUNE_DRY_RUN count=${count} max_age_days=${IVR_BACKUP_MAX_AGE_DAYS}"
else
  echo "PRUNE_OK count=${count} max_age_days=${IVR_BACKUP_MAX_AGE_DAYS}"
fi
