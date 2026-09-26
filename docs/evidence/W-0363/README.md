# W-0363 — Lô `L8` của kế hoạch khắc phục `25/09`: N1, phần không phụ thuộc quyết định

Ngày 26/09/2026 · Claude, trong lượt *"tiếp tục l8"* của Toàn · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: `EVIDENCE_SUBMITTED`.** Toàn chưa nghiệm thu.

## Vì sao có việc này

N1 là luật quá độ Tech Lead chốt ngày 24/09: production không tổng hợp giọng lúc gọi, chỉ phát audio dựng sẵn. Clip do
VieNeu render offline nên vẫn đúng quyết định "chỉ VieNeu" của owner; chỉ thời điểm render đổi. Lô `L8` của kế hoạch
khắc phục `25/09` (`plan/ke-hoach-khac-phuc-m8-2026-09-25.md`) gồm ba mục không chờ quyết định: `K-48`, `K-49`, `K-50`.
Phần chờ quyết định (`Q-11`, `Q-25`, `Q-26`, `Q-29`) và nhánh phát clip cho giá trị của đơn (`Q-29.2`) nằm ngoài lô.

Phân tích tác động báo `SynthesizeSegmentedAsync` ở mức HIGH (nằm trên `DispatchAsync` của cả gateway Mock lẫn Asterisk),
nên Toàn duyệt trước khi sửa. Lô làm trên bản sao tách riêng của `d5833812`. Mã việc là `W-0363` vì `W-0362` để dành cho
lô `L7` mà phiên khác đang làm.

## Đã làm

| Mục | Đã làm | Phép kiểm |
| --- | --- | --- |
| `K-48` | `RenderedAudio.CreatePlaylist` từ chối playlist dài quá 5 phút bằng `ArgumentOutOfRangeException`; cả hai gateway đưa lỗi đó vào nhánh cuối, ghi kênh hỏng, và SIM bị cách ly vì một đơn quá dài. Nay `SpeechSynthesisService` cộng thời lượng trước khi ghép và từ chối bằng `TtsSynthesisException` mã `TTS_PLAYLIST_TOO_LONG`, nhánh giữ kênh lành. Hai giới hạn thành hằng của `RenderedAudio`. Giới hạn 64 đoạn không vượt được từ một kịch bản, vì số thứ tự đoạn dừng ở 64 ngay lúc tạo đoạn; test ghim điều đó thay vì giữ một phép kiểm không bao giờ chạy tới | `UT-N1-PLAYLIST-01` |
| `K-49` | Ba lớp chặn. Một: DI cho `PRODUCTION_REAL` đăng ký `RuntimeSynthesisForbiddenTtsProvider` và không đăng ký HTTP client của sidecar VieNeu. Hai: validator từ chối `EXTERNAL_CONFIGURABLE` ở `PRODUCTION_REAL`. Ba: `SpeechSynthesisService` ở production không gọi provider nào; production được xác định theo chế độ của cuộc gọi **hoặc** của deployment, vì gateway Asterisk (B13) vẫn truyền `LabRealSim` tới khi `K-42` xong. Kịch bản nguyên câu bị từ chối trước cache; kịch bản phân đoạn vẫn lấy câu cố định từ catalog dựng sẵn và dừng ở giá trị đầu tiên cần tổng hợp. Lỗi là `TtsSynthesisException` mã `TTS_RUNTIME_SYNTHESIS_FORBIDDEN`: `AudioError`, kênh lành, chuẩn hoá thành `IVR_TECHNICAL_EXCEPTION` không tính lượt. MOCK và LAB không đổi. Hệ quả: tới khi có `Q-29.2`, mọi cuộc gọi production dừng ở bước giọng; đó đúng là điều luật muốn, và kế hoạch cũng không mở `SIP-04` trước `Q-29.2` và `Q-29.4` | `UT-N1-NOSYNTH-01`, `UT-N1-NOSYNTH-02`, `UT-N1-NOSYNTH-03`; `UT-TTS-WHITELIST-07` giữ nguyên |
| `K-50` | Chart: `ivr.assertTtsCandidate` từ chối ứng viên W-0122 hoàn chỉnh bằng lời nêu luật N1, đặt **sau** các phép kiểm riêng của ứng viên, nên ứng viên thiếu vẫn được báo đúng chỗ thiếu. Template worker bỏ biến môi trường trỏ endpoint tổng hợp loopback. `values-prod-tts.draft.yaml` gắn nhãn `SUPERSEDED`. Gate mới `speech-transition-gate.mjs` đọc values và template, không cần docker, kiểm năm điều; `--self-test` làm hỏng từng điều trên bản sao (sáu phép) và đòi gate đỏ; gate được đăng ký trong `gate-invocations.json`. `tts-helm-selftest.mjs`: hai lần render dương ở prod (fixture đầy đủ, lần nâng timeout có đo) nay đòi đúng lời từ chối N1; các ca âm khác giữ nguyên | `speech-transition-gate.mjs`, `tts-helm-selftest.mjs` |

## Kiểm bằng cách làm hỏng

Mỗi phép gỡ đúng một phần của bản sửa trong bản sao, build lại, chạy test hoặc gate được nêu, rồi ghi lại byte gốc
(không dùng `git checkout`). Bảy phép đều đỏ đúng chỗ, hai lượt đối chứng đều xanh: `K-48` một phép; `K-49` năm phép (DI,
validator, service chỉ tin chế độ của người gọi, kịch bản nguyên câu, giá trị của đơn trong kịch bản phân đoạn); `K-50` một
phép, bỏ lời từ chối N1 khỏi chart, làm cả `speech-transition-gate.mjs` lẫn `tts-helm-selftest.mjs` đỏ. Số liệu ở
[mutation-results.json](mutation-results.json).

## Kiểm chứng

| Phần | Kết quả |
| --- | --- |
| Build cả solution trong bản sao, analyzer là lỗi | 0 cảnh báo, 0 lỗi |
| Bộ unit trong bản sao | `879/879` |
| Bộ integration trong bản sao | `429/430`, 21 phút vì máy đang chạy cả CI lẫn test của ops-core. Test đỏ duy nhất là `IT-API-MATRIX-38`: nó gọi `git ls-files`, mà bản sao không phải repo git |
| Traceability, `generate-test-traceability.mjs --check` | sinh lại, `910` dòng, `TEST_TRACEABILITY_CURRENT` |
| Gate sweep trong bản sao | `44/45` ở lượt đầu: `selftest-dotnet-policy.sh` đỏ vì công cụ `Ivr.CiPolicy` chưa được build Release trong bản sao mới, mà gate chạy nó bằng `dotnet run --no-build`. Build công cụ rồi chạy lại gate đó: đạt. `speech-transition-gate.mjs` và `tts-helm-selftest.mjs` đạt trong sweep |
| `ci-config-selftest.mjs --self-test`, `acceptance-batches.mjs --self-test`, `docs-selftest.mjs` | đạt |
| Trên `main` sau khi đưa vào (`d5833812` cộng lô này), 08:43–08:48 | build 0 cảnh báo; unit `879/879`; `IT-API-MATRIX-38` cùng các test integration về speech, TTS, feature flag và profile cấu hình `23/23`; `speech-transition-gate.mjs`, `tts-helm-selftest.mjs`, `generate-test-traceability.mjs --check`, `acceptance-batches.mjs --self-test`, `ci-config-selftest.mjs`, `docs-selftest.mjs` đạt |

## Còn lại

- `Q-29.2` (nhánh phát clip cho giá trị của đơn), `Q-29.3` (công cụ ngân hàng clip), `Q-29.4` (render và nghe duyệt),
  `Q-29.5` (tài liệu, khối helm `worker.speech.prerendered`) theo kế hoạch, sau quyết định `Q-29`.
- `K-42` thuộc lô `L7` sẽ bỏ hằng `LabRealSim` ở gateway Asterisk; lớp chặn theo chế độ deployment của `K-49` vẫn giữ
  sau đó làm phòng thủ kép.
- Bằng chứng cũ của W-0122 và W-0317 không sửa: chúng tả thiết kế trước luật 24/09.
