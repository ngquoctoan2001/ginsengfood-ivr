#!/bin/sh
# W-0053 / P10-2 — rebuild a synchronous standby after a failover. This is the
# step that puts RPO back to 0.
#
# failover.sh leaves one primary and no standby. From then on every commit is
# acknowledged by a single machine: RPO is no longer 0, and nothing fails, alerts
# or looks different.
#
# The order below is the point of the script. synchronous_standby_names makes the
# primary wait at every COMMIT for a standby of that name. Set it before that
# standby streams and every write on the only database in service hangs for as
# long as the base backup takes, which on production data is not seconds. So:
# copy, start, wait until the PRIMARY reports the standby streaming, and only then
# name it. DG-DR-REBUILD-05 checks that order on the primary, not in this output.
#
# Runs on the new standby's host, as the postgres OS user.
#
# Inputs:
#   PGDATA                   data directory for the new standby; empty or absent
#   IVR_DR_PRIMARY_HOST      the promoted primary, where failover.sh ran
#   IVR_DR_STANDBY_NAME      name the primary will wait for, [a-z0-9_]
#   IVR_DR_PRIMARY_PORT      default 5432
#   IVR_DR_REPLICATION_USER  role with REPLICATION, default repl
#   IVR_DR_ADMIN_USER        superuser on the primary, default postgres
#   IVR_DR_VERIFY_DATABASE   database to commit a probe row into (optional)
# Passwords come from the libpq password file (PGPASSFILE, else ~/.pgpass), never
# from arguments, and the standby keeps using that file to reconnect. TLS follows
# the usual libpq variables (PGSSLMODE, PGSSLROOTCERT).
#
# Not fencing. Nothing here stops the old primary from coming back; see section 6
# of docs/dr-topology.md.
set -eu

: "${PGDATA:?PGDATA is required}"
: "${IVR_DR_PRIMARY_HOST:?IVR_DR_PRIMARY_HOST is required}"
: "${IVR_DR_STANDBY_NAME:?IVR_DR_STANDBY_NAME is required}"
host=$IVR_DR_PRIMARY_HOST
name=$IVR_DR_STANDBY_NAME
port=${IVR_DR_PRIMARY_PORT:-5432}
repl_user=${IVR_DR_REPLICATION_USER:-repl}
admin_user=${IVR_DR_ADMIN_USER:-postgres}
log="$PGDATA/log/rebuild-standby.log"

fail() {
  echo "DR_REBUILD_FAIL: $*" >&2
  exit 1
}

# -w: a missing password-file entry fails here instead of waiting on a prompt.
primary() {
  psql -v ON_ERROR_STOP=1 -AtqXw -h "$host" -p "$port" -U "$admin_user" -d postgres "$@"
}

# wait_for <expected> <sql>: poll the primary once a second for up to 60s.
wait_for() {
  attempt=0
  while [ "$attempt" -lt 60 ]; do
    if [ "$(primary -c "$2" 2>/dev/null || true)" = "$1" ]; then
      return 0
    fi
    attempt=$((attempt + 1))
    sleep 1
  done
  return 1
}

echo "DR_REBUILD_START primary=${host}:${port} standby=${name} pgdata=${PGDATA}"

# 0. Refuse whatever would make the incident worse.
[ "$(id -u)" != 0 ] || fail "run as the postgres OS user, not root"
case "$name" in
  *[!a-z0-9_]*) fail "IVR_DR_STANDBY_NAME must match [a-z0-9_]+: ${name}" ;;
esac
# Never emptied here: pointed at the wrong directory by mistake, a script that
# deletes first destroys it.
if [ -e "$PGDATA" ] && [ -n "$(ls -A "$PGDATA")" ]; then
  fail "${PGDATA} is not empty; empty it by hand once you are sure it is not a live data directory"
fi
recovery=$(primary -c 'SELECT pg_is_in_recovery()') || fail "cannot query ${host}:${port} as ${admin_user}"
[ "$recovery" = "f" ] || fail "${host} is still in recovery; promote it with failover.sh first"
# Empty after failover.sh. Anything else is either the inherited setting failover.sh
# clears (writes are hanging right now) or a primary that already has a standby.
current=$(primary -c 'SHOW synchronous_standby_names')
[ -z "$current" ] || fail "${host} already waits for '${current}'; run failover.sh first"
# Otherwise the streaming check below could be answered by a different standby.
taken=$(primary -c "SELECT count(*) FROM pg_stat_replication WHERE application_name = '${name}'")
[ "$taken" = "0" ] || fail "a standby named ${name} is already connected to ${host}"

# 1. Copy the primary. -R writes standby.signal and a primary_conninfo that carries
#    application_name, the name the primary will wait for. --checkpoint=fast: the
#    default spread checkpoint can hold the copy back for minutes, all at RPO > 0.
pg_basebackup -h "$host" -p "$port" -U "$repl_user" -w -D "$PGDATA" \
  -R -X stream --checkpoint=fast -d "application_name=${name}"
chmod 700 "$PGDATA"
echo "DR_REBUILD_BASEBACKUP_OK"

# 2. Start it, and wait for the primary's view: streaming means connected and
#    caught up, which is the state the primary picks a synchronous standby from.
mkdir -p "$(dirname "$log")"
pg_ctl -D "$PGDATA" -l "$log" -w -t 60 start || fail "the standby did not start; see ${log}"
wait_for streaming "SELECT state FROM pg_stat_replication WHERE application_name = '${name}'" \
  || fail "${name} is not streaming from ${host} after 60s; see ${log}"
echo "DR_REBUILD_STREAMING name=${name}"

# 3. Only now make the primary wait for it. From here a failure must not leave the
#    primary waiting for a standby that is not there: that is the hang failover.sh
#    clears, recreated by the script meant to finish its job. So until step 4 has
#    proved the pair works, any exit puts the setting back to ''.
sync_set=0
release() {
  if [ "$sync_set" = 1 ]; then
    echo "DR_REBUILD_RELEASING_SYNC: ${host} would otherwise hold every write for ${name}" >&2
    primary -c "ALTER SYSTEM SET synchronous_standby_names = ''" -c "SELECT pg_reload_conf()" >/dev/null \
      || echo "DR_REBUILD_RELEASE_FAILED: clear synchronous_standby_names on ${host} by hand" >&2
  fi
}
trap release EXIT
trap 'exit 1' INT TERM

sync_set=1
primary -c "ALTER SYSTEM SET synchronous_standby_names = '${name}'" -c "SELECT pg_reload_conf()" >/dev/null
echo "DR_REBUILD_SYNC_SET name=${name}"

# 4. Prove it. The primary reports the standby synchronous, and a commit comes
#    back: a client-side timeout, because statement_timeout does not end a wait
#    for synchronous replication, which happens at COMMIT (DG-DR-03).
wait_for sync "SELECT sync_state FROM pg_stat_replication WHERE application_name = '${name}' AND state = 'streaming'" \
  || fail "${name} did not become synchronous within 60s"
echo "DR_REBUILD_IN_SYNC name=${name}"

if [ -n "${IVR_DR_VERIFY_DATABASE:-}" ]; then
  timeout 15 psql -v ON_ERROR_STOP=1 -AtqXw -h "$host" -p "$port" -U "$admin_user" -d "$IVR_DR_VERIFY_DATABASE" \
    -c "CREATE TABLE IF NOT EXISTS ivr_dr_rebuild_probe (probe_at timestamptz PRIMARY KEY)" \
    -c "INSERT INTO ivr_dr_rebuild_probe VALUES (now())" >/dev/null \
    || fail "a commit on ${host} did not come back within 15s"
  echo "DR_REBUILD_WRITE_OK database=${IVR_DR_VERIFY_DATABASE}"
fi

sync_set=0
echo "DR_REBUILD_OK standby=${name} is synchronous; RPO is 0 again"
