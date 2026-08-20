#!/bin/sh
# W-0053 / P10-2 — promote a standby, and finish the job.
#
# This exists as a script rather than as a numbered list in a document because the
# second step is the one people leave out, and leaving it out produces a database
# that passes every health check and hangs on the first write.
#
# What happens without step 2: pg_basebackup copies postgresql.auto.conf, so a
# standby built from a primary that uses synchronous replication inherits
# synchronous_standby_names. After promotion that setting still names a standby —
# one that no longer exists. The promoted node leaves recovery, accepts
# connections, answers reads, and every INSERT blocks forever in SyncRep with no
# error and no timeout. DG-DR-03 asserts exactly that failure before clearing it,
# so this step cannot quietly stop being necessary.
#
# Inputs:
#   PGDATA                  data directory of the standby being promoted
#   IVR_DR_VERIFY_DATABASE  database to write a probe row into (optional)
# Deliberately NOT automated: see section 6 of docs/dr-topology.md. Promoting while
# the primary is merely unreachable produces split-brain, and with synchronous
# replication that means two nodes that both believe they took a commit. A human
# confirms the primary is gone; this script does the rest.
set -eu

: "${PGDATA:?PGDATA is required}"

echo "DR_FAILOVER_START pgdata=${PGDATA}"

# 1. Promote — unless someone already did.
#
# Idempotent on purpose. The realistic sequence is an operator who ran pg_ctl
# promote by hand, watched the node come up, found writes hanging, and only then
# went looking for the runbook. A script that refuses to run at that point is a
# script that does not help during the one incident it exists for.
if [ "$(psql -AtqX -d postgres -c 'SELECT pg_is_in_recovery()')" = "f" ]; then
  echo "DR_FAILOVER_ALREADY_PROMOTED"
else
  pg_ctl promote -D "$PGDATA"
fi

# Wait for recovery to end. pg_ctl returns once the request is accepted, which is
# earlier than the node being a primary.
attempt=0
while [ "$attempt" -lt 60 ]; do
  if [ "$(psql -AtqX -d postgres -c 'SELECT pg_is_in_recovery()')" = "f" ]; then
    break
  fi
  attempt=$((attempt + 1))
  sleep 1
done

if [ "$(psql -AtqX -d postgres -c 'SELECT pg_is_in_recovery()')" != "f" ]; then
  echo "DR_FAILOVER_FAIL: still in recovery after 60s" >&2
  exit 1
fi
echo "DR_FAILOVER_PROMOTED"

# 2. Release the synchronous requirement inherited from the old primary.
inherited=$(psql -AtqX -d postgres -c 'SHOW synchronous_standby_names')
if [ -n "$inherited" ]; then
  echo "DR_FAILOVER_CLEARING_SYNC inherited=${inherited}"
  psql -v ON_ERROR_STOP=1 -AtqX -d postgres \
    -c "ALTER SYSTEM SET synchronous_standby_names = ''" \
    -c "SELECT pg_reload_conf()" >/dev/null
fi

# 3. Prove the node actually serves writes. Promotion is not the goal; a working
#    database is, and the difference between the two is what step 2 covers.
if [ -n "${IVR_DR_VERIFY_DATABASE:-}" ]; then
  psql -v ON_ERROR_STOP=1 -AtqX -d "$IVR_DR_VERIFY_DATABASE" \
    -c "SET statement_timeout='15s'" \
    -c "CREATE TABLE IF NOT EXISTS ivr_dr_failover_probe (probe_at timestamptz PRIMARY KEY)" \
    -c "INSERT INTO ivr_dr_failover_probe VALUES (now())" >/dev/null
  echo "DR_FAILOVER_WRITE_OK database=${IVR_DR_VERIFY_DATABASE}"
fi

echo "DR_FAILOVER_OK"
echo "DR_FAILOVER_NEXT: rebuild a standby before closing the incident — a lone primary has "
echo "DR_FAILOVER_NEXT: silently returned RPO to non-zero, and nobody has said so."
