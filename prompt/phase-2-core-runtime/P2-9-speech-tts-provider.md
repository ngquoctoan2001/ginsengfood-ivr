# PROMPT P2-9 — Speech Rendering & TTS Provider Boundary

## 0. Meta
| | |
| --- | --- |
| **ID** | `P2-9` |
| **Work ID** | `W-0066` (canonical tracker §5) |
| **Phase** | 2 — Core runtime in MOCK |
| **Prereq (blockedBy)** | `P2-4`, `P2-7`, `P1-5` (`IRetentionJob` cho purge audio cache) |
| **Blocks** | `P8-1` (real SIM lab không thể phát tiếng nếu thiếu slice này) |
| **Governance flag** | `REAL_CUSTOMER_CALL_ALLOWED=NO` · `IVR_EXECUTION_MODE=MOCK` |
| **Stack** | .NET 10 · PostgreSQL |
| **Execution mode** | `MOCK` (real provider adapter chỉ được wire ở `LAB_REAL_SIM` sau `OD-V1-19`) |

## 1. ROLE
Bạn là **Senior .NET Engineer (Voice/Media)**. Bạn xây **port + adapter** cho việc biến `privacy_safe_order_summary` thành audio tiếng Việt mà khách nghe được, với ranh giới privacy chặt. Bạn **không** chọn nhà cung cấp TTS — đó là quyết định owner (`OD-V1-19`).

## 2. CONTEXT
`README-governance` §3 liệt kê “speech renderer + TTS provider” là provider bắt buộc. `P2-4` mới chỉ có `ISpeechRenderer` trả **text** và một fake TTS; chính `P2-4` ghi “Do not claim TTS pronunciation is live-verified”. `P8-1` gọi `play` trên vendor adapter nhưng **không nêu nguồn audio**. Không prompt nào trong thư viện implement audio thật. Vì nghiệp vụ cốt lõi là “khách nghe đơn của mình rồi bấm 1 hoặc 0”, một câu đọc sai/không hiểu được là lỗi business, không phải lỗi kỹ thuật nhỏ. Slice này đóng lỗ hổng ở mức port/adapter/test, để `P8-1` có thứ để phát.

## 3. SOURCE SPECS (đọc trước khi code — bắt buộc)
- `docs/contracts/target-v1-closure-pack/T-03-speech-summary.md` (payload lời thoại)
- `specs/functional/04-call-execution-dtmf.md` (danh sách biến canonical + forbidden)
- `specs/api/04-sim-adapter-contract.md` (`play_script`, trust boundary, recording)
- `specs/data/05-pii-policy.md`, `specs/database/05-retention-and-privacy.md`
- `specs/_review/open-decisions-register.md` `OD-V1-15`, `OD-V1-19`
- `prompt/phase-2-core-runtime/P2-4-sim-adapter-mock.md`, `P2-7-script-content-management.md`
- `prompt/README-governance.md` §2.7, §4

## 4. DECISIONS & CONSTRAINTS
- **`OD-V1-19` (OWNER_DECISION_REQUIRED):** vendor TTS chưa chọn. Prompt này tạo **port + fake adapter + một adapter skeleton có cấu hình**, không hard-code vendor SDK nào.
- **`OD-V1-15` (OWNER_DECISION_REQUIRED):** whitelist biến script chưa được duyệt cho `items[]` và `delivery_area_short`. Renderer phải đọc whitelist từ config theo mode: `MOCK`/`LAB` dùng bộ Target V1, `PRODUCTION_REAL` **fail-closed** nếu whitelist chưa có approval record.
- **DT-05:** recording OFF. Audio sinh ra là ephemeral; không lưu vào evidence thường.
- **D-05:** không bao giờ đưa raw phone hoặc full address vào text hoặc audio, kể cả qua `pronunciation_hints`.
- **PDPA:** nếu vendor TTS là dịch vụ ngoài, nội dung đơn hàng rời khỏi mạng nội bộ → phải có DPA + được ghi vào `P10-1` data inventory. Prompt này tạo chỗ ghi, không tự kết luận là hợp lệ.

## 5. INPUTS / DEPENDENCIES
- `REAL_AVAILABLE`: `privacy_safe_order_summary` schema, script template/version (P2-7), renderer text (P2-4).
- `MOCK_REQUIRED`: fake TTS trả audio tổng hợp tất định (ví dụ tone/PCM sinh thuật toán) để test không cần mạng.
- `OWNER_DECISION_REQUIRED`: `OD-V1-19` vendor + `OD-V1-15` whitelist.
- `BLOCKED_EXTERNAL`: audio codec/format thực tế mà gateway chấp nhận (`IR-TEL-*`, `W-0008`).

## 6. BUILD STEPS
1. Định nghĩa `ITtsProvider` trong `Ivr.Domain`: `Task<RenderedAudio> SynthesizeAsync(SpeechScript script, TtsOptions options, CancellationToken ct)`; `RenderedAudio { Format, SampleRate, Duration, ContentRef }` — **không** chứa PII, chỉ ref.
2. `TtsOptions`: `locale` (mặc định `vi-VN`), `voiceId`, `speakingRate`, `pronunciationHints`, `maxDurationSeconds`, `timeout`.
3. `FakeDeterministicTtsProvider` (dùng ở `MOCK`): sinh audio tất định từ hash của text để snapshot test ổn định; không gọi mạng.
4. `ConfigurableExternalTtsProvider` **skeleton**: đọc endpoint/credential/format từ config; **ném `NotConfiguredException` khi chưa có vendor** (`OD-V1-19`). Không nhúng SDK vendor nào.
5. **Audio cache**: cache theo `(script_template_id, script_version, hash(privacy_safe_order_summary), voiceId, locale)`; TTL ≤ `confirmation_window` và ≤ retention của speech snapshot; cache key **không** chứa PII thô; purge do `IRetentionJob` (P1-5).
6. **Timeout/fallback**: quá `timeout` hoặc provider lỗi → `IVR_TECHNICAL_EXCEPTION` với `is_counted_customer_attempt=false`; **không** map thành no-answer; không fallback sang text-only đọc thiếu nội dung.
7. **Pronunciation**: áp `pronunciation_hints` từ task; có bộ từ điển tên sản phẩm tiếng Việt; xử lý Unicode/dấu, số tiền VND đọc theo nhóm, số lượng, và câu rút gọn khi vượt giới hạn dòng item.
8. **Privacy guard trước synth**: chạy lại full-address/phone detector trên text cuối cùng; vi phạm → chặn synth, `IVR_PII_POLICY_VIOLATION`, không phát.
9. **Cost/rate limit**: đếm ký tự/số request per provider, expose metric, có bound cấu hình; ghi mô hình chi phí sơ bộ vào `docs/capacity-model.md` (P10-3 dùng lại).
10. Wire vào `ISimGateway.play` để `P8-1` chỉ cần cấu hình provider, không sửa domain.

## 7. OUTPUT ARTIFACTS
| Path | Nội dung |
| --- | --- |
| `src/Ivr.Domain/Speech/ITtsProvider.cs`, `TtsOptions.cs`, `RenderedAudio.cs` | Port |
| `src/Ivr.Infrastructure/Speech/FakeDeterministicTtsProvider.cs` | Fake dùng ở MOCK |
| `src/Ivr.Infrastructure/Speech/ConfigurableExternalTtsProvider.cs` | Skeleton, fail nếu chưa cấu hình |
| `src/Ivr.Infrastructure/Speech/AudioCache.cs` | Cache + TTL + purge hook |
| `src/Ivr.Infrastructure/Speech/SpeechPrivacyGuard.cs` | Detector trước synth |
| `tests/Ivr.UnitTests/Speech/**` | snapshot + negative tests |
| `docs/capacity-model.md` (phần TTS) | ký tự/phút, rate limit, chi phí ước tính |

## 8. TESTS TO WRITE
| Test ID | Loại | Assert |
| --- | --- | --- |
| `UT-TTS-SNAPSHOT-01` | unit | Render tiếng Việt có dấu, 1 item và nhiều item, câu rút gọn phần còn lại — snapshot ổn định. |
| `UT-TTS-VND-02` | unit | Số tiền lớn đọc đúng nhóm; số lượng và đơn vị đúng. |
| `UT-TTS-PRON-03` | unit | `pronunciation_hints` được áp; tên sản phẩm Unicode/emoji không làm vỡ render. |
| `UT-TTS-PII-04` | unit | Text chứa địa chỉ đường phố hoặc dãy số điện thoại → chặn synth, `IVR_PII_POLICY_VIOLATION`. |
| `UT-TTS-TIMEOUT-05` | unit | Provider timeout → `IVR_TECHNICAL_EXCEPTION`, `is_counted_customer_attempt=false`, không phải no-answer. |
| `UT-TTS-NOTCONFIGURED-06` | unit | `ConfigurableExternalTtsProvider` chưa cấu hình → fail-closed, không im lặng fallback. |
| `UT-TTS-WHITELIST-07` | unit | `PRODUCTION_REAL` + whitelist chưa có approval record → fail-closed (`OD-V1-15`). |
| `IT-TTS-CACHE-08` | integration | Cache hit/miss theo key; TTL không vượt window; purge xoá đúng. |
| `IT-TTS-MODE-09` | integration | `MOCK` không thể khởi tạo external provider; không mở socket ra ngoài. |

## 9. REVIEW / ACCEPTANCE GATE
**Self-review:**
- [ ] Không vendor SDK nào bị hard-code.
- [ ] Không audio/PII nào lọt vào evidence thường.
- [ ] Timeout/lỗi provider không bao giờ bị tính là customer no-answer.
- [ ] `MOCK` không có egress.

**Reviewer (GitLab MR):** kiểm ranh giới port/adapter đủ để thay vendor mà không sửa `Ivr.Domain`; kiểm privacy guard chạy **sau** render và **trước** synth.

## 10. EVIDENCE EXPECTED
Ghi vào `docs/evidence/W-0066/`: snapshot text đã sanitize, metadata audio (format/duration/sample-rate, **không** kèm file audio chứa nội dung đơn thật), báo cáo PII scan, kết quả 9 nhóm test, phần TTS của capacity/cost model.

## 11. FORBIDDEN
- ❌ Chọn/khoá nhà cung cấp TTS khi `OD-V1-19` chưa có quyết định owner.
- ❌ Gửi raw phone, full address hoặc trường ngoài whitelist tới bất kỳ provider nào.
- ❌ Lưu audio nội dung đơn thật vào `docs/evidence/` hoặc log.
- ❌ Bật recording (DT-05).
- ❌ Map lỗi TTS thành `IVR_NO_ANSWER_*`.
- ❌ Tuyên bố pronunciation đã được nghiệm thu khi chưa có lab evidence (`P8-1`).

## 12. DEFINITION OF DONE
- [ ] Build + test + lint pass; 9 nhóm test §8 xanh.
- [ ] Evidence §10 đầy đủ trong `docs/evidence/W-0066/`.
- [ ] `P8-1` có thể wire provider thật **chỉ bằng cấu hình**, không sửa `Ivr.Domain`.
- [ ] Đạt tối đa `TESTS_PASS` (mock-only). Pronunciation acceptance vẫn `NOT_RUN` cho tới lab.
- [ ] `OD-V1-19` và `OD-V1-15` vẫn mở; slice này **không** đóng chúng.

## 13. TRACKER UPDATE (bắt buộc)
- Before: `W-0066` → `IN_PROGRESS` + baseline/prereq.
- During: checkpoint; dependency phát sinh lấy Work ID kế tiếp.
- After: files, commands/results, evidence links, residual gates (`OD-V1-19`, `OD-V1-15`, `W-0008`); chỉ reviewer/owner chuyển `ACCEPTED`.
