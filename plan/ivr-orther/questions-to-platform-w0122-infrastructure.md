# Phiếu yêu cầu — gửi Platform / Infra / Telephony

**Chủ đề:** Ba gate hạ tầng còn mở của W-0122 — internal mirror, target hardware, và
`OD-VOICE-08` production media sink
**Người gửi:** Team IVR / Module 8 (IVR Order Confirmation)
**Ngày gửi:** `2026-08-28` · **Trạng thái:** ⏳ CHỜ TRẢ LỜI
**Ưu tiên:** P1 — cả ba chặn production. **Không** chặn Owner voice audition hay lab

> Ba mục dưới đây độc lập nhau; trả lời được mục nào thì đóng mục đó, không cần chờ đủ ba.

---

## A. Internal mirror cho model bundle

### A.1 Vì sao cần

Production build/runtime **không được** tải weights từ Hugging Face công khai. Upstream có thể
đổi, bị gỡ, hoặc rate-limit; và một build không tái lập được thì không có provenance. Chỉ Phase 0
(nonprod) được tải bản công khai, có cờ tường minh, và vẫn phải kiểm path/size/SHA-256.

`deploy/tts/scripts/verify-model.py --mode production` hiện **fail đúng như thiết kế**, vì thiếu
`internal_mirror_uri`/`internal_mirror_digest` cho từng artifact.

### A.2 Cần mirror cái gì

| Mục | Giá trị |
| --- | --- |
| Số artifact | `13` (trong đó `11` bắt buộc lúc chạy) |
| Tổng dung lượng | `210,763,608` bytes ≈ `201 MiB` |
| File lớn nhất | `vieneu/onnx_int8/vieneu_backbone_shared.data` — `99.08 MiB` |
| Nguồn | 2 repo Hugging Face, revision đã khóa (xem `MODELS.lock`) |
| Danh sách chính xác | `deploy/tts/models/MODELS.lock` — path, size, SHA-256 từng file |

Đây là artifact **bất biến**: cùng revision thì cùng bytes. Không cần cơ chế cập nhật; cần cơ chế
**giữ nguyên và lấy lại được**.

### A.3 Câu hỏi

#### `INF-A1` — Dùng hạ tầng nào để mirror? (P1)

☐ OCI registry nội bộ (artifact/OCI image)  ☐ S3/MinIO nội bộ  ☐ Artifact repository (Nexus/Artifactory)
☐ Khác: `_______________________`

#### `INF-A2` — Xin điền URI + digest cho từng artifact (P1)

Trả về đúng dạng dưới đây cho cả 13 dòng; IVR sẽ ghi thẳng vào `MODELS.lock` rồi chạy lại verifier.

| `bundle_path` | `internal_mirror_uri` | `internal_mirror_digest` |
| --- | --- | --- |
| `vieneu/onnx_int8/vieneu_backbone_shared.data` | | |
| … (13 dòng, theo đúng thứ tự trong `MODELS.lock`) | | |

#### `INF-A3` — Backup/restore drill (P1)

☐ Có quy trình khôi phục · mô tả: `_______________________` · đã diễn tập ngày: `__________`
☐ Chưa có

Vì sao cần: mất mirror = production không khởi động được, và không có đường quay lại public
upstream vì đó chính là điều bị cấm.

> **Phụ thuộc chéo phải biết trước:** verifier production còn đòi `license_file_sha256` khác `null`
> cho **mọi** artifact. Hai model revision hiện **không có file LICENSE** — xem `L1` trong
> [phiếu Legal](questions-to-legal-od-voice-07.md). Nghĩa là dù mirror xong, gate này vẫn không qua
> được cho tới khi Legal trả lời. Đây không phải lỗi cấu hình.

---

## B. Target hardware để đo performance

### B.1 Trạng thái hiện tại: `ENV_BLOCKED`, và chỉ có đúng một điểm đo

Một lần quan sát duy nhất trên máy dev: **`3,459 ms` cho một request**. Đó không phải p95, không
phải target hardware, và không đủ để kết luận bất cứ điều gì.

Con số đó đáng lo, nên nói thẳng: `SpeechSynthesisService` tổng hợp **3 đoạn động tuần tự** trước
khi quay số. Nếu `3,459 ms` là đại diện thì pre-dial ≈ `10.4` giây/cuộc gọi cold cache.

### B.2 Ngưỡng phải đạt (đã có sẵn trong plan §4.6, không phải phát minh mới)

| Chỉ số | Ngưỡng |
| --- | --- |
| p95 mỗi request | ≤ `80%` của `TimeoutMilliseconds` = `4,000 ms` (baseline `5,000`) |
| Pre-dial cold toàn playlist | `3 × timeout` phải còn ≥ `20%` headroom dưới `60,000 ms` (lease `120s` − expected call `60s`) |
| Peak RSS | thấp hơn container memory limit ít nhất `25%` |
| Request budget | `3 × cold calls/phút` dưới `MaxRequestsPerMinute` (`60`) với ≥ `20%` headroom |
| Overload | vượt capacity phải fail nhanh, không `5xx` ngẫu nhiên, không OOM, không PCM cụt |

Helm nay **enforce** phần lease headroom lúc render, và mọi giá trị timeout trên baseline bắt buộc
có `worker.tts.approvals.performanceRef`.

### B.3 Câu hỏi

#### `INF-B1` — Cấp được môi trường đo không? (P1)

☐ Có · CPU: `______` · RAM: `______` · ngày sẵn sàng: `__________`
☐ Chưa — sớm nhất: `__________`

#### `INF-B2` — Resource request/limit đề xuất cho sidecar TTS (P1)

`requests.cpu` `______` · `requests.memory` `______` · `limits.cpu` `______` · `limits.memory` `______`

Chart hiện để trống có chủ ý và **fail closed** nếu render mà chưa điền — không có default nào lấy
từ laptop.

#### `INF-B3` — Nếu không đạt ngưỡng thì hướng nào? (P2)

☐ Tăng capacity  ☐ Giảm concurrency  ☐ Tăng tỉ lệ đoạn cố định (bớt đoạn động)
☐ Dừng nhánh self-host

> Xin **đừng** chọn "nâng timeout". §4.6 loại hướng đó, và Helm nay chặn ở render time.

---

## C. `OD-VOICE-08` — production media sink

### C.1 Vấn đề, nói ngắn

Worker ghi file audio động ra đĩa rồi trả cho Asterisk một media reference dạng
`sound:generated/<digest>`. Ở lab, worker và Asterisk **dùng chung một named volume** nên việc này
chạy được.

Ở production **không có Asterisk trong Helm chart** — chart chỉ có `ivr-api` và `ivr-worker`. Nên
worker sẽ ghi file vào một filesystem mà **không consumer nào đọc được**. Copy nguyên mô hình lab
sang production rồi gọi là xong là sai.

Đây là quyết định kiến trúc, không phải một dòng values.

### C.2 Các topology hợp lệ

| # | Phương án | Được gì | Mất gì |
| ---: | --- | --- | --- |
| 1 | Đưa Asterisk/media consumer **vào cùng Pod** với worker, dùng `emptyDir` chung | Không cần storage chia sẻ; vòng đời file trùng vòng đời Pod; nhanh nhất | Đổi mô hình deploy telephony; scale worker = scale Asterisk |
| 2 | **RWX PVC** dùng chung giữa worker Pod và telephony Pod | Giữ nguyên tách biệt worker/telephony | Cần storage class RWX; thêm điểm hỏng; phải giải quyết quyền POSIX cho UID `1654` |
| 3 | Object storage + telephony adapter fetch | Bền, dễ backup | Thêm độ trễ vào đúng đường pre-dial vốn đã sát budget; thêm bề mặt lưu trữ chứa audio đơn hàng |

Chart hiện **chỉ** hỗ trợ phương án 2 và mặc định `mediaSink.type: UNDECIDED` — render sẽ fail cho
tới khi có quyết định.

### C.3 Câu hỏi

#### `INF-C1` — Chọn topology nào? (P1)

☐ 1 — cùng Pod + `emptyDir`  ☐ 2 — RWX PVC  ☐ 3 — object storage + adapter
☐ Khác: `_______________________`

#### `INF-C2` — Quyền filesystem ở production (P1)

Lab dùng init container ghim digest để tạo mount root `1654:1654` mode `0750`. Production dùng gì?

☐ `runAsUser`/`runAsGroup`/`fsGroup`  ☐ CSI driver có hỗ trợ  ☐ Khác: `____________`

☐ Đã xác nhận Asterisk/telephony chỉ **đọc**, không ghi được vào mount đó

#### `INF-C3` — Retention ở production (P1)

Lab chứng minh bằng one-shot purge trên DB dùng một lần. Production thì:

☐ CronJob dùng chung storage  ☐ Sidecar retention  ☐ TTL của storage layer
☐ Khác: `_______________________`

#### `INF-C4` — Ai ký `OD-VOICE-08`? (P1)

Platform: `____________` · Telephony: `____________` · Release owner: `____________`

---

## D. Việc phía IVR sẽ làm sau khi có trả lời

| Trả lời | Hành động |
| --- | --- |
| `INF-A2` đủ 13 dòng | Ghi vào `MODELS.lock`, chạy `verify-model.py --mode production`; vẫn cần `L1` mới qua được |
| `INF-B1`/`B2` | Chạy bộ đo §4.6 trên đúng môi trường đó, xuất evidence, điền resources vào chart |
| `INF-C1`–`C4` | Cập nhật chart theo topology đã chọn, chạy lại Helm gate, mở lại `OD-VOICE-08` |
| Không trả lời | Ba gate giữ nguyên trạng thái mở. Không suy ra từ kết quả lab, và **không** deploy production |
