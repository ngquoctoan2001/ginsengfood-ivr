# W-0315 — TTS chỉ còn VieNeu: gỡ hướng cũ khỏi code, lab, tài liệu và lịch sử

**Ngày:** `2026-09-18` · **Baseline:** `main@53c2eb5` · **Loại:** code + lab + tài liệu ·
Origin=`OWNER_DECISION` `2026-09-18`

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Quyết định

Owner chốt `S4` ngày `17/09` và nhắc lại ngày `18/09`: **VieNeu-TTS tự host là bộ đọc duy nhất**
(*"only vieneu"*), cả production lẫn lab. Cùng ngày `18/09` owner chọn thêm hai điều:

1. Lab bỏ mọi audio không phải VieNeu (ElevenLabs, edge-tts) và provider `STATIC_FILE`.
2. Lịch sử của hướng giọng đọc cũ **xoá sạch**, không giữ làm tư liệu.

Việc này làm luôn Lô 3 mục `2` và Lô 4 bước `3` của
[`vuong-mac-va-quyet-dinh-2026-09-17.md`](../../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md).

## 1. Code

| Thay đổi | Chi tiết |
| --- | --- |
| Xoá bộ ghép clip | `RecordedSpeechComposer.cs` + test. Chưa từng nối vào runtime; `gitnexus_impact` upstream = `0` |
| Khôi phục bản trước bộ ghép clip | `VietnameseNumberSpeller`, `VietnameseOrderScriptRenderer`, `DeliveryRegionResolver` và test của chúng về đúng nội dung trước `69d82b7`/`a6a997b`/`1bcba9e` |
| Gỡ `STATIC_FILE` | `StaticFileTtsProvider`, hằng `StaticFileProvider`, `FileMediaReference`/`FileDurationSeconds` (global lẫn theo miền), `RegionalVoiceMap.TryGetMedia`, nhánh DI và luật validator tương ứng; `appsettings.json` của Api/Worker |
| Endpoint chỉ loopback | `TtsProviderOptionsValidator` từ chối mọi endpoint không phải loopback: bộ đọc duy nhất là VieNeu sidecar chung network namespace với worker |
| Mã lỗi | `TTS_FIXED_SEGMENT_NOT_RECORDED` → `TTS_FIXED_SEGMENT_NOT_RENDERED`. Chỉ `SpeechSynthesisService` phát ra; không nằm trong contract, DB hay tài liệu mã lỗi |
| Chú thích | Catalog đoạn cố định là file VieNeu render sẵn; client HTTP là client của VieNeu sidecar |

**Rủi ro `HIGH` đã báo trước khi sửa:** `VietnameseOrderScriptRenderer.TotalAmountClips` nằm trên hai
luồng `DispatchAsync` (Asterisk, Mock). Đưa về `Spell(...) + " đồng"` — cách làm trước `W-0234`.

**Chứng minh văn bản đọc không đổi một byte** (test tạm, đã xoá trước commit):

| So sánh | Kết quả |
| --- | --- |
| Tổng tiền của renderer tại baseline so với `Spell + " đồng"` | trùng hash, `320.374` số tiền × `5` kiểu đọc |
| `Spell` / `SpellQuantity` trước và sau khôi phục | trùng hash, `320.374` số tiền + `200.010` số lượng × `5` kiểu đọc + `3` miền |

**Test:** `UT-AST-AUDIO-03` → `UT-AST-AUDIO-08` (profile lab dựng đúng client VieNeu, endpoint
loopback). `UT-TTS-EXT-CFG-06` thêm hai ca: HTTPS ra ngoài bị từ chối, `localhost` được nhận. Đổi
tên phương thức của `UT-SEG-MISSING-07`, `UT-SEG-CATALOG-08`. Gỡ các test của phần đã xoá:
`UT-TTS-STATIC-REGION-05`, `UT-VOICE-CFG-04`, `UT-VOICE-CLIP-06`…`10`, `UT-VOICE-AREA-01`…`04`,
`UT-VOICE-3B-01`…`09`, `UT-VOICE-4B-07`.

## 2. Lab

| Thay đổi | Chi tiết |
| --- | --- |
| Xoá | `6` WAV không phải VieNeu, `manifest.txt`, `Convert-LabVoiceAudio.ps1`, `Invoke-FptAiVoiceAudition.ps1`, `Set-AsteriskLabVoice.ps1` |
| `SHA256SUMS` | còn đúng `12` đoạn VieNeu; `sha256sum --check --strict` PASS |
| `entrypoint.sh` | bỏ biến thể giọng; chỉ cài `12` đoạn |
| `docker-compose.softphone.yml` | không còn cấu hình giọng; overlay `docker-compose.vieneu-tts.yml` mang toàn bộ |
| `Start`/`Stop-FreeSoftphoneLab.ps1` | luôn nạp overlay VieNeu; ba giọng đọc từ manifest Owner đã ký; `-ModelBundle` bắt buộc |
| `Convert-LabSegmentAudio.ps1` | chỉ nhận `.wav` (VieNeu); khối cấu hình sinh ra dán vào `environment:` của `ivr-worker` trong overlay, không lặp khoá `RegionalVoices__Enabled` |
| `segments-compose-env.yml` | cùng định dạng mới; ghép thử vào overlay rồi `docker compose config` ra đủ `12` `TextHash`, không trùng khoá |

`docker compose config --quiet` PASS cho `dev + softphone + vieneu-tts` và
`dev + softphone + vieneu-tts-audition`. **Chưa chạy** một cuộc gọi lab thật sau thay đổi — cần bundle
model VieNeu trên máy.

## 3. Tài liệu đang dùng

Viết lại theo quyết định: register `OD-V1-19`, `OD-VOICE-01`/`04`/`05`; `00-CHUA-XONG.md` (nhóm A,
`today-03`, bỏ `m8-16`); phiếu cho Sếp mục `4`–`5`; `vuong-mac…` (đánh dấu Lô 3 mục `2`, Lô 4 bước
`3`); bàn giao `17/09`; bốn file worklist `07/09`–`16/09`; `IR-07` (bản đã gửi — sửa tại chỗ bốn dòng
và thêm mục *Đính chính bổ sung `2026-09-18`*); gói procurement `R-02`/`R-04`/`R-05`/`README`;
`capacity-model`, `cost-model`; kế hoạch lab 1 SIM; README lab; ghi chú đầu `P2-9`;
`specs/ui/04-ivr-menu-config.md`; một dòng của spec `V0.3`; hai review và một báo cáo có ghi ngày.

**Ghim lại trong cùng lượt** (tính trên byte LF):

| Ghim | Ở đâu |
| --- | --- |
| `one-sim-lab-plan.md`, `R-05`, `W-0122/lab-runbook.md` | `b3-telephony-evidence-validator.mjs` + template `W-0185` |
| spec `V0.3` | `deploy/ci/pins/external-decision-artifacts.sha256` |
| manifest trên | ba validator external-decision + template `W-0164`, `W-0165`, `W-0170` |
| `external-decision-response-validator.mjs`, `…-routing-validator.mjs` | `external-decision-closure-validator.mjs` + template `W-0170` |

## 4. Lịch sử — xoá theo lựa chọn của owner

- Xoá `16` thư mục evidence: `W-0228`…`W-0242`, `W-0244`. Giữ `W-0243` (guard `public_name`, không
  thuộc hướng cũ) và sửa một câu trong đó.
- Tracker: `16` dòng tương ứng → `CANCELLED`, nội dung trung tính trỏ về `W-0315`. `W-0309` §3 viết
  lại (ba phiếu VieNeu gom về `S2`/`S5`); `W-0226`, `W-0227` bỏ mục chỗ rẽ cũ; sửa câu chữ ở `W-0048`,
  `W-0057`, `W-0104`, `W-0108`, `W-0113`, `W-0243`, `W-0245`, `W-0246` và vài dòng Activity.
- `W-0284/evidence-inventory.json`: `16` mục khớp lại tracker. `W-0293/reviewed-lines.json`: bỏ `39`
  dòng trỏ vào README đã xoá (`121` → `82` dòng, `45` → `37` file).
- Hai link tới `manifest.txt` đã xoá trong `W-0106/voice-audition-kit.md` đổi thành chữ thường kèm ghi chú;
  không còn link markdown nào trỏ vào file hay thư mục đã xoá.

**Lệch quy tắc, ghi rõ:** tracker ghi *"Không xóa lịch sử"*. Owner chọn xoá sạch ngày `18/09`.
Toàn văn cũ vẫn nằm trong lịch sử git trước commit này.

**Cố ý giữ nguyên:**

| Cái gì | Vì sao |
| --- | --- |
| Tên ElevenLabs/edge-tts trong evidence `W-0104`, `W-0106`, `W-0108` và các dòng tracker của chúng | Là lịch sử lab của những tính năng **vẫn đang chạy** (softphone lab, định tuyến giọng theo miền, ghép đoạn). Không có câu nào nói về hướng giọng đọc cũ |
| `W-0197/api-matrix.json`, `W-0286/*.json` | Ảnh chụp dấu vân tay mã nguồn. Sửa sẽ lệch hash tổng và lệch dòng mà `.gitleaksignore` đã duyệt |
| `.codex-doc-memory/` | Chỉ mapper `markdown-doc-reader` chính thức được sinh lại |
| `docs/documents/**` | Tài liệu nguồn của owner; không chứa hướng cũ |
| Bản `IR-07` Module 3 đang giữ | Không gửi lại. Chính mục *Đính chính bổ sung `2026-09-18`* ghi: không đổi contract, M3 không phải sửa code, đề xuất `M3-05`/`M3-06` giữ nguyên |

## 5. Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **`1151/1151`**, `0` warning — contract `24` · unit `756` · chaos `8` · integration `363`. `1184` → `1151`: bớt test của phần đã gỡ, thêm `UT-AST-AUDIO-08` |
| `generate-test-traceability --check` | `TEST_TRACEABILITY_CURRENT=722` |
| Gate sweep (Git Bash) | `GATE_SWEEP_PASS 40/40 run, 21 skipped by manifest` — gồm `lab-converter-selftest`, `b3-telephony`, năm gate external-decision, `capacity-selftest`, `dr-selftest` (`136.4s`) |
| `scan-pii.sh docs/evidence` | `PII_SCAN_PASS` (chạy trong sweep) |
| gitleaks `v8.30.0` trên đúng các dòng lượt này thêm | `no leaks found` — CI quét theo commit, nên đây là phần commit này mang vào |
| `b3-telephony-evidence-validator --check-template` (template `W-0185`) | `B3_TELEPHONY_TEMPLATE_VALID_NOT_READY` — đúng trạng thái mẫu chưa điền |
| `docker compose config --quiet` | PASS: `dev + softphone + vieneu-tts`, `dev + softphone + vieneu-tts-audition` |
| `gitnexus_detect_changes` | `medium`; `3` luồng chạm, đều qua một dòng chú thích trong `PrivacySafeSpeech.cs`; symbol còn lại đúng phạm vi |

## 6. Còn lại

- Lô 4 phần làm được ngay: quét lại Trivy, thử đổi bản cài nền, soạn cấu hình production nháp.
- Chờ: đo trên máy thật (`S5`) · người duyệt nghe `12` đoạn và chỗ nối (`S1`) · `6` cuộc gọi thử
  MicroSIP (`S5`) · ký rủi ro `3` và `4` (`S2`).
- Chạy lại lab bằng VieNeu sau thay đổi này khi có bundle model trên máy.

## 7. Bổ sung sau commit — `18/09`

Toàn hỏi việc VieNeu đã xong hết chưa. Trước khi trả lời, quét lại mọi file đang theo dõi thì ra mấy
chỗ `9af20d3` bỏ sót. Sửa trong một commit follow-up; commit cũ giữ nguyên.

| File | Chỗ sót | Nay là |
| --- | --- | --- |
| `docs/lab/one-sim-lab-plan.md` §5, §9 | Bước 4 và một dòng bảng thời gian còn theo hướng giọng đọc cũ | Bước 4: bundle model VieNeu đã kiểm trên máy lab; dòng thời gian bỏ |
| `docs/lab/one-sim-lab-plan.md` §0.1, §1 | Còn nhắc một provider phát file dự kiến | §0.1 mục `3`: chưa có nguồn audio sau `ITtsProvider`; §1 trỏ về VieNeu sidecar ở §0.2 |
| `docs/evidence/W-0057/README.md` | Dòng mô tả `R-05` còn theo hướng cũ | Khớp nội dung `R-05` sau `W-0315` |
| `prompt/phase-8-sim-pilot/P8-1-real-sim-adapter.md` | DoD còn để `OD-V1-19` chờ vendor | `OD-V1-19` đã đóng `17/09`, không chờ vendor |
| README này §4, §6 và dòng tracker `W-0315` | Ghi việc gửi đính chính `IR-07` cho Module 3 | Bỏ: chính mục đính chính ghi không đổi gì phía Module 3, nên không gửi |
| Tracker `A-0644` | Thiếu hai cột Actor và Evidence/result | Bổ sung |

**Ghim lại:** `one-sim-lab-plan.md` → `c78c6364…` trong `b3-telephony-evidence-validator.mjs` và
template `W-0185`. Hai file đó không bị ghim ở đâu khác.

**Cách quét:** `git grep` trên mọi file đang theo dõi, trừ `.codex-doc-memory/`, `docs/documents/`,
`third_party/` và hai bộ JSON ở §4. Tìm cách diễn đạt của hướng cũ bằng tiếng Việt lẫn tiếng Anh, các
định danh đã gỡ ở §1–§2 và tên bộ đọc khác. Chỗ còn lại thuộc ba loại: chính sách cuộc gọi `DT-05`
(chủ đề khác), lịch sử lab `W-0104`/`W-0106`/`W-0108` giữ theo §4, và chữ trùng ngẫu nhiên bên trong
từ khác.

| Kiểm chứng | Kết quả |
| --- | --- |
| `b3-telephony-evidence-validator --self-test` | `B3_TELEPHONY_EVIDENCE_SELF_TEST_PASS` |
| `--check-template` (template `W-0185`) | `B3_TELEPHONY_TEMPLATE_VALID_NOT_READY` — đúng trạng thái mẫu chưa điền |
| `scan-pii.sh docs/evidence` | `PII_SCAN_PASS` |
| `docs-selftest.mjs` | `API_DOCS_SELFTEST_PASS` |
| `gate-status.mjs` | `GATE_STATUS_PASS` |
| gitleaks `v8.30.0` trên các dòng thêm vào | `no leaks found` |
| .NET | `0` file `.cs` ⇒ không chạy lại solution |
| `gitnexus_impact` `SOURCE_PINS` · `gitnexus_detect_changes` | `LOW`, `0` phụ thuộc · `low`, `0` luồng |
