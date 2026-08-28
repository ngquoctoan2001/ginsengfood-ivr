# W-0122 — Môi trường render audition và rủi ro liên tục

Trạng thái: `SINGLE_COPY_LOCAL` — evidence Phase 1 chỉ tồn tại trên một workstation  
`REAL_CUSTOMER_CALL_ALLOWED=NO`

## 1. Vì sao cần file này

11 WAV audition và model bundle nằm dưới `artifacts/w-0122-voice-audition/` và
`artifacts/w-0122-models/`; cả hai đều bị `.gitignore`. Repo chỉ giữ **hash** của chúng trong
`audition-manifest.json`. Hệ quả phải nói thẳng:

- người khác clone repo **không** nghe được 11 giọng — phải render lại;
- nếu workstation này mất, `OD-VOICE-06` quay lại Phase 0/1, kể cả khi Owner đã nghe xong;
- `Start-W0122VoiceAudition.ps1` fail closed khi thiếu/lệch file là đúng thiết kế: nó bảo vệ tính
  toàn vẹn, nó không thay thế một bản sao.

## 2. Điều kiện để render lại ra đúng byte

| Thành phần | Pin |
| --- | --- |
| Source | `pnnbao97/VieNeu-TTS@36c4b501b0634a8f59805e6b529a058fbd30190b` |
| Model | `pnnbao-ump/VieNeu-TTS-v3-Turbo@2da0efab622a1722125991736524f080b751ef5b` |
| Codec | `OpenMOSS-Team/MOSS-Audio-Tokenizer-Nano-ONNX@ceff0d0749bfb3fa2d61149794ec6feef0d1e1ae` |
| Runtime lock | `a2f18ce2…` — `onnxruntime==1.24.4`, `numpy==2.3.4`, `soxr==1.0.0`, `tokenizers==0.22.2`, `sea-g2p==0.9.0` |
| Renderer | `deploy/ci/scripts/render-voice-audition.mjs` SHA-256 `3c77abd0…` |
| Voice roster | `deploy/tts/shim/voices.json`, 11 mục `audition_enabled: true` |
| Script | `audition-script.txt`; pin là hash của text đã trim `c20592a2…` |

Decode là greedy — `temperature=0.0`, `top_k=1`, `top_p=1.0` trong
[backend.py](../../../deploy/tts/shim/backend.py) — nên chuỗi token tái lập được với cùng bộ pin
trên. Nhưng **số thread ONNX Runtime chưa được pin**, mà thứ tự reduction float có thể đổi theo số
CPU. Vì vậy "render lại ra đúng byte" hiện là **giả định chưa đo**, không phải kết luận: chưa ai
render lại trên máy thứ hai để so hash.

Image đã render 11 WAV được tạo **trước** lần chuẩn hoá line ending ngày `2026-08-28`, nên digest
ghi ở `README.md` không tái lập được; xem mục re-pin ở đó.

Render lại (nonprod, sau khi bundle đã qua `verify-model.py --mode nonprod`):

```powershell
node deploy/ci/scripts/render-voice-audition.mjs `
  --container <ten-container-tts> `
  --output artifacts/w-0122-voice-audition
```

## 3. Việc cần làm

1. **Sao lưu hai thư mục `artifacts/` ra nơi khác trước buổi nghe của Owner.** Đây là cách rẻ nhất
   để không phải nghe lại 11 giọng.
2. Khi Infra dựng internal mirror (`OD-VOICE-07` / supply-chain gate), đưa cả model bundle và 11 WAV
   vào đó kèm digest, thay vì để trên một máy.
3. Nếu buộc phải render lại: so với `audition-manifest.json` bằng hash. Nếu byte lệch dù mọi pin
   giống hệt, đánh dấu audition cũ `STALE_RELISTEN_REQUIRED` và nghe lại — **không** sửa manifest
   cho khớp file mới.
