# REVIEW — Open Decisions Register

Trạng thái: `OPEN` · Cập nhật: `2026-09-05` (`W-0194` — owner ký 19 dòng trong một lượt; xem
[gói ký OD-V1](../../plan/ivr-orther/od-v1-signoff-2026-09-05.md)). Không đóng bằng suy luận.

> Mock/fake fixture **không bao giờ** đóng một dòng nào trong bảng này. Mock chỉ cho phép code tiếp tục.

**Còn mở sau lượt ký `2026-09-05`: bốn dòng, và không dòng nào mở vì thiếu ý kiến.**
`OD-V1-09` (nửa sau) và `OD-V1-10` cần **số đo** từ SIM thật và từ lượt chạy tải — ký trên giấy
lúc này là ký một điều chưa biết. `OD-V1-11` và `OD-V1-21` cần **người thứ hai**: duyệt kịch bản
cho `PRODUCTION_REAL` đòi ba actor id khác nhau (`ScriptContentContracts.EnsureApprovalAllowed`),
và merge request đòi một approval độc lập. Chữ ký không tạo ra người thứ hai, nên bốn dòng này
được giữ mở thay vì đóng cho đẹp bảng.

## P0 — real Sales integration/business acceptance

| ID | Decision/data | Owner | Current | Closure evidence |
| --- | --- | --- | --- | --- |
| `OD-V1-01` | program/payment/IVR-required/callable matrix | Sales Product/Core | ✅ `CLOSED` 2026-09-05 — ký đúng ma trận runtime đang thực thi: `GOLDEN_HOUR+ONLINE` và `TWENTY_FOUR_SEVEN+COD`, cả hai cần `ivr_confirmation_required=true` và `order_state=CONFIRMING`; mọi tổ hợp khác từ chối fail-closed | signed matrix + producer tests |
| `OD-V1-02` | generic callback path and ACK taxonomy | Sales API/Core | ✅ `CLOSED` 2026-09-05 — ký Target V1: `POST /api/v1/internal/orders/{orderId}/ivr-result-callbacks`; ACK 200 `ACCEPTED`/`DUPLICATE_ACCEPTED`/`BLOCKED_BY_CORE`/`REVIEW_REQUIRED`, 409 `REJECTED_STALE`/`IDEMPOTENCY_CONFLICT`. Endpoint Golden Hour giữ vai trò compatibility-only | OpenAPI + contract tests |
| `OD-V1-03` | order version exposure/bump/stale behavior | Sales Core | ✅ `CLOSED` 2026-09-05 — Sales phơi `order_version`; IVR đính bản nó thấy; Sales revalidate và tự quyết. Lệch phiên bản trả ACK `REJECTED_STALE` và **không** ghi đè kết quả khách đã bấm | DTO + stale tests |
| `OD-V1-04` | speech-safe summary schema/content/item limits | Sales/Product/Privacy | ✅ `CLOSED` 2026-09-05 — ký schema `privacy_safe_order_summary` hiện hành; quy tắc gộp: quá 3 dòng hàng thì đọc 2 dòng đầu + "và N món khác", **tổng tiền không đổi** | schema + samples + approval |
| `OD-V1-05` | dial-token issue/resolve/TTL/one-use | Sales/Security/Telephony | ✅ `CLOSED` 2026-09-05 — Sales cấp token lúc tạo task; TTL theo `OD-V1-17`; mỗi lần resolve ghi audit kèm `attempt_id`; resolve quá hạn hoặc quá số lần → từ chối và mở review, **không** im lặng bỏ qua | API/threat model/tests |
| `OD-V1-06` | no-answer/timeout/revalidation semantics | Sales Product/Core | ✅ `CLOSED` 2026-09-05 — `IVR_NO_ANSWER_FINAL` là khuyến nghị: Core không đổi trạng thái, đơn tự hết hạn theo timeout. IVR không bao giờ hủy đơn. Khớp `DS-02` | sequence + runtime tests |
| `OD-V1-07` | production auth and mTLS | Security/Platform | ✅ `CLOSED` 2026-09-05 — JWT ký khóa bất đối xứng, JWKS, TTL ≤ 10 phút, bắt buộc scope `ivr.task.write`. mTLS **hoãn** tới khi có hạ tầng thật; token dùng chung tĩnh bị từ chối dứt điểm khi provider là `TARGET_V1` | signed auth profile + tests |

## P0 — lab/production calls

Cột `Current` được thêm `2026-09-05` (`W-0194`). Trước đó bảng này chỉ có bốn cột, nên
`gate-status.mjs` đọc nhầm cột `Gate` làm trạng thái — một dòng đã đóng ở đây sẽ không bao giờ
được đếm là đóng. Bốn cột như các bảng khác là điều kiện để bảng điều khiển nói đúng.

| ID | Decision/data | Owner | Current | Gate |
| --- | --- | --- | --- | --- |
| `OD-V1-08` | final attempt policy/version, two-program bundle/hash, T0/counting/retry/quiet-hours/cutover | Product + Order Core + M3 | ✅ `CLOSED` 2026-09-05 — owner ký bộ số `D-10` làm production version `gh-247-prod-v1`: GH `2/[0,150]/300s`, 24/7 `2/[0,450]/900s`; khung giờ gọi `08:00–21:00` ICT; technical retry trần `1`, backoff `60s`; `T0` = lúc Order Core mở cửa sổ. Bộ phase-8 bị supersede vì tự mâu thuẫn (cửa sổ 600s cho một "Giờ Vàng" 5 phút) | production; triển khai ở GĐ 2 |
| `OD-V1-09` | 1 SIM lab protocol/DTMF/disposition/allowlist | Infra/vendor | ⏳ `HALF_SIGNED` 2026-09-05 — **đã ký**: giao thức `LAB-01..08`, allowlist đúng một alias đích, `REAL_CUSTOMER_CALL_ALLOWED=NO` suốt lab. **Chưa ký**: bảng ánh xạ tín hiệu nhà mạng → result; bảng hiện tại là ứng viên và chỉ đo được khi có SIM thật | LAB_REAL_SIM |
| `OD-V1-10` | 32 eSIM capacity/failover/caller-ID/cost | Infra/procurement | ⛔ `NOT_SIGNED` — cố ý. Con số 32 là giả định chứ chưa phải phép đo; ký bây giờ là ký một điều chưa biết. Mở lại sau bước 4.5 (mô hình tải) và 5.4 (thông lượng thật một kênh) | production |
| `OD-V1-11` | script/legal/do-not-call/retention | Legal/Privacy | ⏳ `CONTENT_SIGNED / APPROVER_QUORUM_UNRESOLVED` 2026-09-05 — **đã ký**: ghi âm TẮT vĩnh viễn ở V1; metadata cuộc gọi 90 ngày; bằng chứng DTMF 180 ngày; audit admin 1 năm; `phone_ref`/dial token TTL ngắn nhất, xóa khi job đóng. **Chưa giải được bằng chữ ký**: `PRODUCTION_REAL` cần ba actor id khác nhau (`ScriptContentContracts.EnsureApprovalAllowed`: người tạo không được duyệt; `CONTENT` ≠ `PRIVACY_LEGAL`). Dự án hiện có một người | customer calls |
| `OD-V1-12` | pilot/release authority/kill switch | Release owner | ✅ `CLOSED` 2026-09-05 — owner là release authority. `REAL_CUSTOMER_CALL_ALLOWED` chỉ bật bằng quyết định ký tên có ngày kèm bằng chứng lab đạt và kịch bản đã duyệt; không bao giờ bằng biến môi trường hay một lượt deploy | production |

## P0 — mở bởi red-team review 2026-08-12 (W-0062)

| ID | Decision/data | Owner | Current | Closure evidence | Gate |
| --- | --- | --- | --- | --- | --- |
| `OD-V1-13` | **Golden Hour ONLINE có thuộc scope IVR không.** Business source hiện đọc được là COD-only: `plan/ivr-orther/decisions-log.md` DS-01 (“IVR-callable = CHỈ `CONFIRMING` VÀ CHỈ khi `payment_method_snapshot=COD`”, source-read từ Sales platform). Target V1 §4 đề xuất thêm `GOLDEN_HOUR+ONLINE`. Delta này **chưa được owner phê duyệt**. | Product/Business + Sales Core | ✅ `CLOSED` 2026-09-05 — owner chọn **giữ cả hai chương trình**. Toàn bộ runtime/OpenAPI/DB/seed/test đã xây hai chương trình; rút về COD-only là xóa code đang chạy để khớp một lần đọc nguồn cũ hơn. **Kèm theo một việc thật chưa làm**: `DS-02` nói Core chỉ chuyển trạng thái cho đơn COD và từ chối `422` đơn non-COD, nên Sales phải định nghĩa transition cho `ONLINE` trước khi nối thật | Signed program matrix + Sales producer phát `GOLDEN_HOUR+ONLINE` task | real integration |
| `OD-V1-14` | **`ivr_confirmation_required` không có business source.** `grep -rln ivr_confirmation_required docs/documents/` → 0 hit. Cả OpenAPI (`enum:[true]`) và DB (`must be true`) đang gate trên một field chưa có nguồn business đã khóa. | Product/Business + Sales Core | ✅ `CLOSED` 2026-09-05 — giữ trường, định nghĩa dứt điểm: Order Core đặt `true` **chỉ cho** đơn nó muốn IVR gọi. Một cờ opt-in tường minh là thứ ngăn "đổi một câu query rồi vô tình gọi thêm ba nghìn đơn"; suy diễn ngầm không có phanh đó | Định nghĩa field + owner sign-off + producer test | real integration |
| `OD-V1-15` | **Speech variable whitelist.** Hai bộ spec active mâu thuẫn: bộ hẹp 4 biến (`specs/data/05-pii-policy.md`, `specs/ui/04-ivr-menu-config.md`, `specs/api/04-sim-adapter-contract.md`) vs bộ Target V1 cần thêm `items[]` (public_name, quantity) và `delivery_area_short` (`docs/contracts/target-v1-closure-pack/T-03-speech-summary.md`, governance §2.7). Business source `PACK-09 §9.1` hậu thuẫn bộ hẹp. **Mở rộng whitelist tự nó là một quyết định privacy.** | Product + Privacy/Legal | ✅ `CLOSED` 2026-09-05 — owner chọn **bộ rộng**: tên ngắn, mã đơn, `items[].public_name` + `quantity` (+ `unit_label`), tổng tiền, `delivery_area_short`. Lý do: một cuộc gọi xác nhận không nói đang xác nhận **cái gì** thì không phải xác nhận. Rủi ro đã cân: người nhấc máy không phải chủ đơn sẽ nghe được món hàng và phường/quận; đổi lại là khách phát hiện đơn sai trước khi hàng đi. Guard giữ nguyên: cấm địa chỉ đầy đủ, cấm chi tiết thanh toán, `delivery_area_short` không được bắt đầu bằng chữ số | Approved whitelist + PIA/privacy sign-off + cập nhật đồng bộ 3 spec | business acceptance |
| `OD-V1-16` | **Attempt policy delta vs business source.** Phase-8 ghi GH `2/[0,300]/600s`, 24/7 `3/[0,300,600]/900s`; D-10 và candidate `mock-lab-v1` ghi GH `2/[0,150]/300s`, 24/7 `2/[0,450]/900s`. W-0151 còn tìm thấy production governance gaps: row-by-row registry, thiếu signed refs/four-eyes/effective-retire/bundle atomicity; technical retry config chưa versioned; pre-dial flag không so job policy; quiet-hours/timezone chưa có contract. Current wire **đã** exact compare và trả `409`, không phải khoảng trống. | Product + Order Core + M3; Platform/M8/Release ở dòng kỹ thuật | ✅ `CLOSED` 2026-09-05 — owner supersede nguồn phase-8, lấy bộ `D-10`: GH `2/[0,150]/300s`, 24/7 `2/[0,450]/900s`, version production `gh-247-prod-v1`. Phần `D-10` không nói thì ghi thêm ở đây: khung giờ gọi `08:00–21:00` ICT (hiện **chưa có** — hôm nay hệ thống sẵn sàng gọi lúc 3 giờ sáng nếu có task), technical retry trần `1` backoff `60s`. Giữ `mock-lab-v1` nguyên tên làm bản lab | Signed `ATP-01..15` + canonical two-program version/bundle hash + M3 producer SHA/OpenAPI/CDC/shared tests + owner business supersede nguồn cũ; xem [M8-11](../../plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md) | production |
| `OD-V1-17` | **Dial-token reuse semantics.** Task mang đúng **một** `dial_token` scalar, nhưng policy cần ≥2 customer dial cộng technical retry. Năm tài liệu ghi “one-use/attempt”. Không có endpoint re-issue/refresh trong bất kỳ contract nào. Phương án: (a) `dial_tokens[]` per-attempt, (b) reissue endpoint, (c) token bundle, (d) reusable token có TTL/risk control ghi rõ. | Sales/Security/Telephony | ✅ `CLOSED` 2026-09-05 — chọn phương án (d): **token dùng lại được**, gắn cứng vào `task_id`, TTL = cửa sổ xác nhận + 60s, số lần resolve tối đa = `max_customer_attempts` + trần technical retry, mỗi lần resolve gắn một `attempt_id` khác nhau. Trần số lần resolve thay cho “one-use”: token rò rỉ vẫn không quay số được quá số lần chính sách cho phép. Loại (b)/(c) vì bắt Sales dựng endpoint mới — thứ duy nhất có thể hỏng ngay giữa lúc đang gọi | Chọn phương án + issue/resolve/reissue contract + TTL/replay/audit tests | real call |
| `OD-V1-18` | **Vị trí resolve `dial_token→E.164`.** `specs/api/04-sim-adapter-contract.md` nói adapter **không** nhận số; `P2-4` đặt resolver trong IVR. Gateway GSM/SIP thương mại quay số E.164. Trust boundary chưa được định nghĩa ở đâu. | Security + Telephony vendor | ✅ `CLOSED` 2026-09-05 — resolver nằm **trong IVR**, bên trong biên adapter telephony. Số E.164 tồn tại **chỉ trong bộ nhớ tiến trình** cho đúng một lần quay số: không ghi DB, không vào log, không vào evidence, không vào callback payload. Loại phương án để gateway tự resolve vì nó đẩy toàn bộ ánh xạ token→số sang nhà cung cấp, tức là giao họ cả danh bạ khách. **Nợ tài liệu**: `specs/api/04-sim-adapter-contract.md` đang nói ngược, phải sửa ở GĐ 2 | Sơ đồ trust boundary đã duyệt + threat model + vendor capability statement | LAB_REAL_SIM |
| `OD-V1-19` | **TTS/speech synthesis provider.** Không prompt nào implement audio thật; `P8-1` gọi `play` mà không có nguồn audio. Chọn vendor kéo theo PDPA (nội dung đơn rời mạng), cost và pronunciation acceptance. | Product + Infra + Privacy/Legal | ✅ `CLOSED` 2026-09-05 — **không dùng vendor TTS lúc chạy**. Kịch bản là cố định, chỉ 4 giá trị thay đổi: thu người thật cho phần cố định, ghép mã đơn và tiền từ ngân hàng ghi âm chữ số, thu trước tập hữu hạn vùng giao, và **bỏ tên khách khỏi lời thoại** (“Xin chào anh/chị”). Kết quả: không còn dữ liệu khách nào rời hệ thống lúc chạy, xóa sạch bài toán PDPA/DPA/data-residency thay vì đi đàm phán nó. ElevenLabs giữ nguyên cho **lab** | Vendor decision + DPA/privacy review + pronunciation acceptance set + cost model | LAB_REAL_SIM |
| `OD-V1-20` | **Production RBAC cho runtime-gate controls.** Bộ permission `DF-01` (LOCKED, 7 quyền) không có quyền nào cho phép sửa `labDestinationAllowlist` hoặc `globalDialKillSwitch`. Cần permission mới + four-eyes. | Security/Platform + Release owner | ✅ `CLOSED` 2026-09-05 — thêm `IVR_RUNTIME_GATE_ADMIN` ở tầng danger, bất đối xứng theo `W-0068`: **bật** kill switch cần một người, không cần duyệt (chiều giảm rủi ro); **tắt** kill switch hoặc mở rộng `labDestinationAllowlist` cần bốn mắt — người đề xuất và người duyệt khác nhau, ghi bảng phê duyệt append-only. **Residual**: vế bốn mắt cần người thứ hai, cùng bài toán với `OD-V1-11` | Approved permission set + four-eyes policy + negative authz tests | LAB_REAL_SIM |
| `OD-V1-21` | **GitLab platform provisioning.** ~~TV1-12 khóa GitLab CI nhưng remote duy nhất hiện tại là GitHub.~~ **Sửa `2026-08-27`: vế này đã sai từ W-0011** — GitLab project tồn tại và chính là `origin`; runner `#55115499`, Container Registry, protected branch và hosted MR pipeline đều đã PASS trong evidence W-0011. Cái hỏng suốt từ đó là **lối đẩy code**: `remote.origin.pushurl` trỏ GitHub nên GitLab không nhận commit mới (`W-0121` sửa, GitLab đang thiếu 3 commit lúc phát hiện). Cần GitLab project/mirror, Runner, Container Registry, protected branch, MR approvals, “Pipelines must succeed”, masked/protected variables. | Platform/Infra | ⏳ `SIGNED_EXCEPT_INDEPENDENT_APPROVAL` 2026-09-05 — owner ký phần cấu hình: project/runner/registry/protected branch/hosted MR pipeline đều đã PASS trong evidence W-0011. Còn đúng một vế **không ký được bằng chữ ký**: chứng minh một lần merge bị chặn cho tới khi có approval của người khác cần Premium/Ultimate **và** một reviewer thứ hai. Dự án hiện có một người | GitLab project URL + remote verification + runner identity + hosted MR pipeline + protected-branch export + registry push/pull proof + **một MR chứng minh blocked-trước/merge-sau** | P0-2 hosted evidence |

## Explicit non-decisions

- V1 notification is disabled; no notification template/event is required. Bất biến này được enforce ở `P0-4` (`v1NotificationEnabled=false` immutable guard), fail-gate 3 trong `specs/testing/08-acceptance-criteria.md`, và `IT-FAILGATE-*` ở `P5-1` — **không** chỉ dựa vào `P4-5`.
- IVR remains a standalone .NET service; Sales remains Java and owns order truth.
- Current Golden Hour callback remains compatibility-only và không được nhận kết quả 24/7.
- `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS` may be reached while all external rows remain open, but integration/production states must remain blocked.
- Internal API (`specs/api/openapi/ivr-order-confirmation.v1.yaml`) và outbound Sales callback (`order-core-ivr-callback.target-v1.yaml`) là **hai surface riêng biệt**; naming khác nhau là chủ ý và phải map bằng mapper tường minh.

## P1 — mở bởi W-0106 (định tuyến giọng theo vùng miền, 2026-08-22)

| ID | Decision/data | Owner | Current | Closure evidence | Gate |
| --- | --- | --- | --- | --- | --- |
| `OD-VOICE-01` | **Nguồn giọng production.** Đã đảo hướng 3 lần: ElevenLabs loại vì giá → vendor Việt loại vì chất lượng (`myan` không đạt, mỗi vendor chỉ có 1 giọng nữ miền Trung) → quay lại ElevenLabs Starter `$6`/tháng, vì phép tính ban đầu tính theo **số cuộc gọi** thay vì **số câu nói duy nhất**; script cố định nên phần cố định chỉ render 609 ký tự một lần. Nối tiếp `OD-V1-19` | Product + Infra + Privacy/Legal | ✅ `CLOSED` 2026-09-05 — lab giữ ElevenLabs free tier (đã duyệt 27/08). **Production không dùng vendor nào lúc chạy** theo `OD-V1-19`, nên câu hỏi ToS/DPA/data-residency không còn áp dụng; closure evidence chuyển thành hợp đồng giọng đọc + bộ clip đã thu | Gói đã mua + **xác nhận ToS về audio sinh trong kỳ trả phí** + DPA + data residency + cost model + fallback khi voice ID biến mất | production |
| `OD-VOICE-02` | **Phân miền theo tỉnh/thành.** Chia thuần theo 34 đơn vị cấp tỉnh (NQ `202/2025/QH15`), không biệt lệ; Tây Nguyên → Trung | Owner + Product | ✅ `CLOSED` 2026-08-22 | Bảng 34→3 miền + `UT-VOICE-REGION-01..03` phủ 34 tỉnh mới và 29 tên cũ | — |
| `OD-VOICE-03` | **Một template.** Giữ đúng 1 script version `v3-test-approved`; biến thể `nghìn`/`ngàn` và `linh`/`lẻ` nằm trong bộ đọc số, không nằm trong template ⇒ `TemplateHash` không đổi | Product + Privacy/Legal | ✅ `CLOSED` 2026-08-22 | `UT-SCRIPT-VI-REGION-09` chứng minh 3 miền cùng một `TemplateHash` | — |
| `OD-VOICE-04` | **Tự host / thu âm người thật thay vì thuê vendor.** Không model tiếng Việt open-source nào vừa chất lượng vừa sạch license (`viXTTS` = CPML non-commercial và Coqui đã đóng cửa 1/2024 nên không còn ai bán license; `F5-TTS` weights = CC-BY-NC). Đường sạch duy nhất là dữ liệu giọng của chính mình | Owner + Product + Legal | ✅ `CLOSED` 2026-09-05 — chọn **thu giọng người thật và sở hữu license**, không train model trên weights phi thương mại (`viXTTS` CPML, `F5-TTS` CC-BY-NC đều không dùng được). Vì phần cố định đã thu sẵn và phần biến thiên chỉ còn chữ số + vùng giao, **không cần model tự host nào cả** | Hợp đồng + license giọng voice actor; bộ clip đã thu; bằng chứng mối nối nghe mượt; model tự host (nếu dùng) Apache/MIT + train trên data của mình | production |
| `OD-VOICE-05` | **Chốt 3 giọng không qua bước nghe** — Thắm (Bắc), Zara (Trung), Giang (Nam). Cơ sở là **mô tả văn bản, không phải nghe**; không ai trong chuỗi quyết định đã nghe ba giọng đó | Owner | ✅ `CLOSED` 2026-08-22 · **owner đã nghe trong app và chốt lại `2026-08-26`** — xem ghi chú dưới bảng | ✅ **ĐÓNG ĐỦ `2026-08-26`**: voice ID đã verify trong app **và** owner đã nghe cả ba miền qua MicroSIP 8 kHz rồi chấp nhận. W-0106 chuyển `ACCEPTED` (phạm vi lab, dữ liệu fake — đúng tiền lệ W-0104) | LAB |

> `OD-VOICE-05` đóng lựa chọn, **không** đóng nghiệm thu. Chừng nào sếp chưa nghe và ký, trần
> trạng thái W-0106 là `TESTS_PASS` chứ không phải `ACCEPTED` — theo đúng tiền lệ W-0104.

### `OD-VOICE-05` — cập nhật `2026-08-26`: owner đã nghe trong app

Bước nghe bị hoãn từ `2026-08-22` nay **đã làm**. Owner render cả ba giọng trong ElevenLabs web
app, nghe, và giữ cả ba. Voice ID lấy trực tiếp từ app, không lấy từ catalog bên thứ ba.

| Miền | Giọng | Voice ID **đã verify** | Khớp bảng §5 của audition kit? |
| --- | --- | --- | --- |
| Bắc | Thắm — *Giọng Nữ Miền Bắc* | `0ggMuQ1r9f9jqBu50nJn` | ✅ khớp |
| Trung | Zara — *Warm, Natural and Expressive* | `QocxxnxEa0x8mrL2d4VT` | ✅ khớp |
| Nam | Giang — *Northern female Narrator* | `f5q6kePPoQAjCPYG6moa` | ❌ **khác** — kit ghi `X0V9HEDEuaVhVqzVPUKM` |

**Hai điều bất thường đã được nêu và owner đã quyết, ghi lại để sau này không ai phải đoán:**

1. **Giọng miền Nam mang nhãn `Northern female Narrator`.** Đây là giọng **khác** với giọng
   `Giang` trong shortlist (ID khác hẳn), và tên vendor đặt nói ngược lại vùng nó được gán.
   Owner **đã nghe và xác nhận giọng đúng chất Nam**; nhãn của vendor là đặt tên sai.
   Đây đúng là kiểu sai mà audition kit đã cảnh báo về catalog bên thứ ba — lần này bắt được
   vì owner nghe, không phải vì tra ID.

2. **Settings lệch nhau giữa ba giọng**, trong khi audition kit §3 yêu cầu giữ y hệt:

   | Giọng | Stability | Similarity | Speed | Độ dài cùng một kịch bản |
   | --- | --- | --- | --- | ---: |
   | Thắm | `0.75` | `0.75` | `1.00` | **21,16 s** |
   | Zara | `0.50` | `0.75` | `1.00` | 18,44 s |
   | Giang | `0.50` | `0.75` | `1.09` | 17,48 s |
   | *kit đề xuất* | *0.40* | *0.75* | *0.97* | — |

   Chênh lệch đo được: Thắm dài hơn Giang **21%** trên cùng một kịch bản.
   **Owner chọn giữ nguyên** — đã nghe và ưng cả ba. Ràng buộc "settings phải giống nhau" của
   kit vì vậy **không còn hiệu lực**; thay vào đó settings thật của từng giọng được ghi ở đây và
   trong `deploy/lab/asterisk/audio/manifest.txt` để truy nguồn được.

**✅ Bước nghe qua MicroSIP đã xong `2026-08-26`.** Owner nghe cả ba lượt ở 8 kHz — đúng chất
lượng đầu dây, không phải bản studio 44,1 kHz trong app — và chấp nhận cả ba giọng. Đây là bước
mà W-0104 đã có tiền lệ một cặp giọng bị **từ chối** sau khi nghe qua điện thoại, nên nó không
phải thủ tục.

Bằng chứng máy đo đi kèm: ba cuộc `IVR_CONFIRMED`, `voice_id` lần lượt
`w0106-lab-north-tham` / `-central-zara` / `-south-giang`, `voice_region_resolved=true` cả ba.

`W-0106` chuyển `TESTS_PASS → ACCEPTED`, **phạm vi đúng bằng tiền lệ W-0104: software lab, dữ
liệu fake.** Không mở quyền gọi khách thật.

**Chưa đóng `OD-VOICE-01`:** chưa xác nhận ba file này render trên gói trả phí. Nếu là free tier
thì chúng dùng được cho **lab** (dữ liệu fake, không khách nào nghe) nhưng **không** dùng được cho
production, và phải render lại sau khi mua. `manifest.txt` giữ
`w0106_production_provider_authorized=NO`.

### `OD-VOICE-01` — cập nhật `2026-08-27`: owner quyết dùng free tier cho lab

Câu hỏi của owner: *“giờ đang dev test mà, dùng cái đó được không? chừng nào lên production rồi
tính tới mua api.”* **Được** — và quyết định này không nới lỏng ràng buộc nào đang có.

| Phạm vi | Trạng thái | Vì sao |
| --- | --- | --- |
| **Lab / dev / test** | ✅ `APPROVED` `2026-08-27` | Free tier không có commercial license, nhưng lab chạy **dữ liệu fake** và `REAL_CUSTOMER_CALL_ALLOWED=NO` — không khách nào nghe, nên không có “thương mại” để mà cần license |
| **Production** | 🔴 vẫn `OPEN` | Cần gói trả phí + **xác nhận ToS về audio sinh ra trong kỳ trả phí** + DPA + data residency + cost model + fallback khi voice ID biến mất |

Ba file MP3 hiện có, cùng 12 file đoạn cố định sắp render, vì vậy là **tài sản lab**.
`manifest.txt` giữ nguyên `w0106_production_provider_authorized=NO`, và dòng đó là thứ chặn
chúng rò sang production — không phải trí nhớ của ai.

**Một rủi ro cần nói trước, vì nó có hạn sử dụng.** Thắm/Zara/Giang là **community voice**:
chủ giọng có quyền gỡ khỏi thư viện bất cứ lúc nào, và ElevenLabs không cam kết giữ hộ. Nếu một
giọng biến mất trước lúc mua gói thì thứ mất **không phải file đã render** (chúng nằm trong repo,
ghim bằng SHA-256) mà là **khả năng render thêm** — tức là buổi nghe và ký vừa xong `2026-08-26`
phải làm lại từ đầu với một giọng khác. Đây đúng là điều kiện “fallback khi voice ID biến mất”
mà bảng trên đã liệt kê, chỉ là bây giờ nó có thật chứ không còn là giả định.

Cách giảm rủi ro rẻ nhất, và tình cờ cũng là việc kế tiếp trên đường găng: **render 12 đoạn cố
định ngay bây giờ.** Toàn bộ phần cố định của cả ba miền chỉ **609 ký tự** (203 ký tự × 3), nằm
gọn trong hạn mức free tier. Làm xong thì phần văn xuôi — phần chiếm 203/266 ký tự mỗi kịch bản —
được ghim vĩnh viễn bằng hash nội dung và không còn phụ thuộc vào việc giọng còn nằm trong thư
viện hay không. Chỉ còn phần biến thiên (tên, tiền, số lượng) là cần endpoint TTS sống, và phần
đó dù sao cũng phải chờ gói trả phí.
