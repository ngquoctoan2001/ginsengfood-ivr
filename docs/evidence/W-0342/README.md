# W-0342 — Tích hợp bằng chứng giấy phép vào hồ sơ và verifier

Ngày 22/09/2026. **Đã hoàn tất phần hồ sơ và kiểm bằng chứng giấy phép.**
**REAL_CUSTOMER_CALL_ALLOWED=NO; production BLOCKED.** Kết quả thuộc working tree và image
ứng viên local, không chứng nhận commit sạch hoặc triển khai S5 mới.

## Thay đổi

- [Gói bằng chứng](../../../deploy/tts/licenses/README.md) lưu nguyên byte hai model card đúng
  revision, văn bản Apache-2.0 và LICENSE nguồn MOSS bổ sung ghi công. `LICENSES.json` ghi
  nguồn/size/hash; bản thân manifest được ghim trong model lock và bằng pin độc lập ở CI.
- Mỗi một trong **13 artifact** có `license_evidence_id` trỏ đúng model/revision. Python CLI
  và Node CI kiểm tài liệu thực, nguồn đã ghim, hash, phạm vi artifact và hash preset/source.
  `license_file_sha256` vẫn null đúng thực tế upstream không có file riêng; không lấy LICENSE
  của source giả làm LICENSE của weights.
- Đã đưa **13 URI/digest mirror có receipt W-0340** vào khóa nguồn. Gate mirror cho bộ model
  này đạt; việc đó không khẳng định image ứng viên W-0342 đã được chuyển lên máy đích.
- Đồng bộ chuỗi hash model lock → voices → template, pin CI/B3 và bản nháp Helm. B3 còn ràng
  manifest giấy phép và hồ sơ giọng được chuyển liên kết; template vẫn `PENDING_EXTERNAL_INPUT`.
- Dockerfile mang bằng chứng vào `/opt/ivr-tts/licenses` và CLI vào image. Ngoại lệ `.dockerignore`
  cho model-card Markdown bảo đảm tài liệu không bị loại khỏi build. Git attributes giữ byte LF.

## Quyền đã công bố và quyết định phát hành

Bộ kiểm tra hiện trả `license_evidence=PASS`. Bằng chứng là công bố của nhà phát hành đã rà
ở [W-0341](../W-0341/README.md); đây không phải một phê duyệt Legal/Privacy được agent ký thay.
Khóa giữ `legal_gate.status=OWNER_DATA_REQUIRED`, sửa lý do lỗi thời “chưa rõ quyền thương mại
preset” thành còn thiếu quyết định phát hành riêng. Quyết định S2 bảy điểm của Nguyễn Quốc Toàn
ở W-0340 giữ nguyên; không yêu cầu ký lại bảy điểm hoặc nghe lại.

CLI `--mode production` **vẫn từ chối** khi thiếu quyết định đúng `LEGAL_PRIVACY`. Test xác nhận
`MODULE_8_OWNER` không tự mở gate; fixture có đúng authority chỉ là `TEST_ONLY`, không được
ghi vào khóa thật. Không đổi bộ kiểm tra thành chỉ tin một ô `PASS`.

## Giữ nguyên giọng và bằng chứng cũ

Đã so từng trường, chứng minh **13/13 model artifact giữ nguyên nội dung, size, revision, preset
và dependency binding**. Chỉ bổ sung tham chiếu giấy phép và mirror. Cấu hình giọng chỉ đổi
`model_lock_sha256`; backend, converter và profile tải không sửa.

[Manifest giọng dẫn xuất](voice-acceptance-manifest.json) giữ người nghe, thời điểm, quyết định,
ba lựa chọn, thông số và kết quả nghe từ hồ sơ gốc. Chỉ đổi hash khóa và thêm ghi chú rõ việc
chuyển liên kết metadata. Manifest đã được validator hiện hữu chấp nhận. Bản gốc W-0122,
phiếu S2, archive mirror W-0340 và image đã đo S5 đều giữ nguyên.

**38 binding lịch sử** đã được kiểm lại. Khóa/voices cũ được giữ nguyên byte tại
`.artifacts/W-0342/baseline/`; [verification.json](verification.json) ghi nơi lưu cho các
tham chiếu lịch sử. Không chạy lại verifier cũ rồi giả rằng hash khóa mới bằng khóa cũ.

Quota S5 giữ **2 CPU/4 GiB**, một inference, ORT 1; deadline **30/90/120 giây**, queue 8.
Không gọi điện, không chạy tải S5, không yêu cầu người dùng nghe trong W-0342.

## Kết quả kiểm

| Kiểm tra | Kết quả |
| --- | --- |
| Node/Python kiểm gói giấy phép thật | 4 tài liệu / 2 model / 13 artifact đạt |
| Chung một bộ ca lỗi ở hai ngôn ngữ | **36/36 bị từ chối ở mỗi phía**; các ca sửa manifest được tính lại hash để kiểm logic bên trong, không chỉ dừng ở hash ngoài |
| Kiểm sai bằng chứng | Thiếu/sửa file, path escape, nguồn main thay revision, SPDX khác, mượn card model khác, sai phạm vi, sai preset, giả standalone LICENSE và tự nâng thành phê duyệt đều bị chặn |
| Image ứng viên | **27/27 unittest**, gồm kiểm symlink và authority; HTTP contract offline đạt, nonroot, không expose port |
| Chuỗi ảnh → file nguồn | **16/16 file** shim/lock/CLI/tài liệu trong layer khớp byte working tree |
| Bundle model S5 hiện có | CLI nonprod đạt 13/13; `release_blockers=LEGAL` |
| CLI production với khóa thật | Từ chối đúng kỳ vọng; bằng chứng license đạt không bỏ phê duyệt |
| Giọng và công cụ lab | Acceptance selftest + manifest dẫn xuất, audition và fixed-render guard đạt; không phát audio |
| B3 | Selftest valid=1/refusal=34; template đúng và vẫn chưa sẵn sàng |
| Trivy đúng archive ứng viên | Không phát hiện CVE: 26 gói Wolfi + 24 gói Python |

Image ứng viên index `f5fd0909d798`, config `2c98db9c0ad2`. Quét offline với DB đã tải trong
W-0340 ngày 22/09 lúc 02:21 UTC, UpdatedAt 21/09 lúc 19:11 UTC; còn hạn tại lượt quét 04:00 UTC.
**Không tải lại DB trong W-0342.** Đã kiểm chuỗi index → manifest → config → Trivy Metadata.ImageID,
cùng hash archive và DB. Trivy ArtifactID ở lượt này là định danh khác, không dùng thay image config.
Image mới chưa đưa lên mirror/S5 và chưa dùng gọi khách; scan cũ tiếp tục chỉ áp dụng cho image cũ.

Hai lỗi lúc kiểm ban đầu được giữ trong log: Windows sandbox từ chối thư mục Temp/process con
(chạy lại đúng quyền đạt); build bỏ qua Markdown (sửa `.dockerignore`, image cuối đạt).
Không thay test expectation để bỏ qua hai lỗi này.

GitNexus đã refresh index-only. `validate`: LOW, 4 mục/3 direct/0 process; hai liên kết OpenAPI
confidence 0.5 là trùng tên, đọc nguồn xác nhận không gọi gate này. Python `main`: LOW,
3 mục/1 direct/0 process. Các pin LOW. Detect-changes chung working tree: 20 file/22 symbol,
0 process, LOW; có WIP tài liệu từ trước, chưa bao quát các symbol mới chưa index.

Lệnh kiểm chính:

```text
node deploy/ci/scripts/tts-provenance-gate.mjs --selftest
node deploy/ci/scripts/tts-voice-acceptance-gate.mjs --acceptance docs/evidence/W-0342/voice-acceptance-manifest.json
python deploy/tts/scripts/verify-model.py --lock deploy/tts/models/MODELS.lock --bundle .artifacts/W-0333/vieneu-s5/models --mode nonprod
```

Raw/log/script kiểm đóng gói: `.artifacts/W-0342/`. Hash và kết quả: [verification.json](verification.json).

**Việc đã làm:** tích hợp giấy phép và mirror đã kiểm, đồng bộ provenance, kiểm âm/dương và image.
**Đề xuất bước tiếp theo:** chốt quyết định phát hành tổng thể và đưa ứng viên đã kiểm lên mirror/S5
theo đợt triển khai riêng; không dùng W-0342 để tự bật production hoặc nhận lại phép đo tải cũ cho image mới.
