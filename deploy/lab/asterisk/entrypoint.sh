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

exec /usr/sbin/asterisk -f -vvv
