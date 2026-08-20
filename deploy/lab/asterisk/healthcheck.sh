#!/bin/sh
set -eu

curl --fail --silent --show-error \
  --user "ivr-lab:${IVR_LAB_ARI_PASSWORD}" \
  http://127.0.0.1:8088/ari/asterisk/ping >/dev/null
