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
speech_file=/var/lib/asterisk/sounds/ivr-lab-order-confirmation.wav
if [ ! -f "$speech_file" ]; then
  espeak-ng -v vi -s 145 -w /tmp/ivr-lab-source.wav \
    "Xin chào chị An. Chị có đơn E hai E không không một, gồm hai hộp nước hồng sâm, tổng tiền năm trăm sáu mươi nghìn đồng, giao đến phường Bến Nghé, Quận Một. Bấm phím một để xác nhận đơn hàng, bấm phím không để hủy."
  ffmpeg -hide_banner -loglevel error -y -i /tmp/ivr-lab-source.wav \
    -ar 8000 -ac 1 -codec:a pcm_s16le "$speech_file"
  rm -f /tmp/ivr-lab-source.wav
fi

exec /usr/sbin/asterisk -f -vvv
