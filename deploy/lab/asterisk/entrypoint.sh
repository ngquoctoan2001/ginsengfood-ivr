#!/bin/sh
set -eu

: "${IVR_LAB_ARI_PASSWORD:?IVR_LAB_ARI_PASSWORD is required}"
: "${IVR_LAB_SIP_PASSWORD:?IVR_LAB_SIP_PASSWORD is required}"

case "${IVR_LAB_ARI_PASSWORD}${IVR_LAB_SIP_PASSWORD}" in
  *[!A-Za-z0-9._-]*)
    echo "Lab passwords may contain only letters, digits, dot, underscore and dash." >&2
    exit 2
    ;;
esac

sed "s/__ARI_PASSWORD__/${IVR_LAB_ARI_PASSWORD}/g" \
  /opt/ivr-lab/ari.conf.template > /etc/asterisk/ari.conf
sed "s/__SIP_PASSWORD__/${IVR_LAB_SIP_PASSWORD}/g" \
  /opt/ivr-lab/pjsip.conf.template > /etc/asterisk/pjsip.conf

mkdir -p /var/lib/asterisk/sounds
(cd /opt/ivr-lab/audio && sha256sum --check --strict SHA256SUMS)

# W-0106 A1 / W-0122: the fixed prose of the approved script, rendered ahead of time by VieNeu in
# the three owner-accepted regional voices. A call is assembled from these plus the order values
# the VieNeu sidecar synthesizes at call time, so every file present is installed and none is
# selected at boot.
#
# Named by content hash, not by position: `ivr-seg-<region>-<16 hex>`. The application looks a
# sentence up by that hash, so a template edit that changes the wording changes the name, the old
# file stops resolving, and the deployment fails loudly instead of playing wording nobody
# approved. The checksum check above already covered these — SHA256SUMS is verified whole.
segment_count=0
for segment_source in /opt/ivr-lab/audio/ivr-seg-*.wav; do
  [ -e "$segment_source" ] || break
  segment_name=$(basename "$segment_source")
  segment_target="/var/lib/asterisk/sounds/${segment_name}"
  cp "$segment_source" "${segment_target}.tmp"
  mv "${segment_target}.tmp" "$segment_target"
  segment_count=$((segment_count + 1))
done

if [ "$segment_count" -gt 0 ]; then
  echo "W-0122 installed ${segment_count} VieNeu fixed speech segments."
else
  # Absent is legitimate until the segments are rendered. The application fails closed on its
  # own: startup validation refuses Segmentation.FixedSegments=Catalog without a complete
  # catalog, so an empty directory cannot become a call that is missing a sentence.
  echo "W-0122 VieNeu fixed speech segments not present; hybrid playback stays unavailable."
fi

exec /usr/sbin/asterisk -f -vvv
