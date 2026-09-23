# W-0126 — Khắc phục phát hiện rà soát `W-0122`

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

W-0126 rà soát độc lập W-0122 bằng cách chạy lại gate và sửa các phát hiện: chuỗi hash TTS được ghim theo byte CRLF nên chỉ xanh trên đúng một máy (F1, sửa bằng .gitattributes eol=lf rồi tính lại pin), hai validator chấp nhận giọng lấy kỳ vọng từ hai nguồn khác nhau (F2), hash kịch bản audition không được đối chiếu với file (F3), timeout TTS trong Helm gấp 6 lần baseline (F4), và ba thứ được trình bày như gate mà không job nào chạy (F6). Kiểm chứng bằng năm runner đã sửa hoặc tạo mới — tts-provenance-gate.mjs, tts-voice-acceptance-gate.mjs, tts-helm-selftest.mjs, lab-converter-selftest.mjs, tts-fixed-render-selftest.mjs — cùng ba test Python mới trong deploy/tts/tests/test_shim.py; bằng chứng nằm ở ba commit 0aad8d8, 8eba466, 5ec7342, các mục A-0375..A-0377 của tracker và gói docs/evidence/W-0122/.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md
- 0aad8d8
- 8eba466
- 5ec7342
- docs/evidence/W-0122/README.md
- docs/evidence/W-0122/audition-environment.md
- deploy/tts/tests/test_shim.py
- .gitattributes

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Gate trong full sweep có self-test kiểm thay đổi của việc này: `tts-provenance-gate.mjs`, `tts-voice-acceptance-gate.mjs`, `tts-helm-selftest.mjs`, `lab-converter-selftest.mjs`, `tts-fixed-render-selftest.mjs`.
