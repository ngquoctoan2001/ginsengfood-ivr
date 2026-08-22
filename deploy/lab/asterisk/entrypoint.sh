#!/bin/sh
set -eu

: "${IVR_LAB_ARI_PASSWORD:?IVR_LAB_ARI_PASSWORD is required}"
: "${IVR_LAB_SIP_PASSWORD:?IVR_LAB_SIP_PASSWORD is required}"
: "${IVR_LAB_VOICE_VARIANT:=A}"

case "${IVR_LAB_ARI_PASSWORD}${IVR_LAB_SIP_PASSWORD}" in
  *[!A-Za-z0-9._-]*)
    echo "Lab passwords may contain only letters, digits, dot, underscore and dash." >&2
    exit 2
    ;;
esac

case "${IVR_LAB_VOICE_VARIANT}" in
  A|B|C)
    ;;
  *)
    echo "IVR_LAB_VOICE_VARIANT must be A, B or C." >&2
    exit 2
    ;;
esac

sed "s/__ARI_PASSWORD__/${IVR_LAB_ARI_PASSWORD}/g" \
  /opt/ivr-lab/ari.conf.template > /etc/asterisk/ari.conf
sed "s/__SIP_PASSWORD__/${IVR_LAB_SIP_PASSWORD}/g" \
  /opt/ivr-lab/pjsip.conf.template > /etc/asterisk/pjsip.conf

mkdir -p /var/lib/asterisk/sounds
speech_file=/var/lib/asterisk/sounds/ivr-lab-order-confirmation.wav
voice_variant=$(printf '%s' "${IVR_LAB_VOICE_VARIANT}" | tr '[:upper:]' '[:lower:]')
voice_file="/opt/ivr-lab/audio/ivr-lab-order-confirmation-${voice_variant}.wav"

(cd /opt/ivr-lab/audio && sha256sum --check --strict SHA256SUMS)
cp "$voice_file" "${speech_file}.tmp"
mv "${speech_file}.tmp" "$speech_file"

echo "W-0104 pinned voice variant ${IVR_LAB_VOICE_VARIANT} selected."

# W-0106: all three regional voices are installed side by side, not selected at boot. The
# application picks one per call from the delivery area, so Asterisk must be able to play any
# of them at any time. IVR_LAB_VOICE_VARIANT above stays as the W-0104 single-voice path.
#
# The suffix is "-region-<name>", not "-n|-c|-s": "-c" already belongs to W-0104 voice C and
# reusing it would overwrite that evidence file.
for region in north central south; do
  regional_source="/opt/ivr-lab/audio/ivr-lab-order-confirmation-region-${region}.wav"
  regional_target="/var/lib/asterisk/sounds/ivr-lab-order-confirmation-region-${region}.wav"
  if [ -f "$regional_source" ]; then
    cp "$regional_source" "${regional_target}.tmp"
    mv "${regional_target}.tmp" "$regional_target"
    echo "W-0106 regional voice ${region} installed."
  else
    # Absent is legitimate until the MP3s are rendered. The app fails closed on its own:
    # StaticFileTtsProvider throws when a selected voice has no media configured.
    echo "W-0106 regional voice ${region} not present; regional routing stays unavailable."
  fi
done

# W-0106 A1: fixed-segment recordings for hybrid playback. A call is assembled from these plus
# synthesized order values, so every file present is installed and none is selected at boot.
#
# Named by content hash, not by position: `ivr-seg-<region>-<16 hex>`. The application looks a
# sentence up by that hash, so a template edit that changes the wording changes the name, the old
# recording stops resolving, and the deployment fails loudly instead of playing wording nobody
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
  echo "W-0106 A1 installed ${segment_count} fixed speech segments."
else
  # Absent is legitimate until the segment MP3s are rendered. The application fails closed on
  # its own: startup validation refuses Segmentation.FixedSegments=Catalog without a complete
  # catalog, so an empty directory cannot become a call that is missing a sentence.
  echo "W-0106 A1 fixed speech segments not present; hybrid playback stays unavailable."
fi

exec /usr/sbin/asterisk -f -vvv
