# Kế hoạch chi tiết đến khi hoàn thiện hệ thống

> **HISTORICAL_PLAN / SUPERSEDED — 2026-08-27:** Kế hoạch này khóa tại baseline ngày 2026-08-26.
> Các bước yêu cầu IVR tự phân loại/trusted-skip đã bị `OD-18`/`W-0123` thay thế. Không dùng các
> bước đó làm work queue hiện hành; giữ nguyên nội dung cũ để truy vết.

**Ngày lập:** 2026-08-26 · **Baseline:** `main@bdde72c`
**Phạm vi:** từ trạng thái hiện tại (nấc 0) đến `PRODUCTION_REAL_ELIGIBLE` (nấc 4).

> **Kế hoạch này không phải tracker.** Khi bắt đầu một hạng mục, **cấp Work ID thật** từ
> `NEXT_WORK_ID` (hiện `W-0120`) và chuyển trạng thái **ở tracker**, không ở đây. Mã tạm dưới đây
> (`K-xx`) chỉ để tham chiếu trong kế hoạch, **không** đụng bộ đếm Work ID.
>
> **Không có ngày cụ thể cho các mốc phụ thuộc bên ngoài.** Ghi một ngày cho việc mình không kiểm
> soát là tạo ra một cam kết giả. Các mốc đó ghi theo **điều kiện vào**, không theo lịch.

---

## 0. Bốn nấc, và điều kiện vào từng nấc

| Nấc | Tên | Điều kiện vào (không thương lượng) |
| --- | --- | --- |
| **1** | `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS` | mọi prompt đã lập kế hoạch có evidence **được chấp nhận**, không cổng nào `BLOCKED_INTERNAL` |
| **2** | `LAB_REAL_SIM_VERIFIED` | 1 SIM thật chạy xong lab protocol, có allowlist + kill-switch evidence |
| **3** | `REAL_SALES_INTEGRATION_VERIFIED` | Target V1 **đã ký** và contract test chạy trên **Sales sandbox thật** |
| **4** | `PRODUCTION_REAL_ELIGIBLE` | capacity 32 eSIM **đo được**, evidence legal/security **được chấp nhận**, `DF-03` **đã ký** |

Bốn nấc này **không thể đảo thứ tự**. Không thể nhảy sang nấc 3 khi nấc 2 chưa xong, vì contract
test trên sandbox thật vẫn cần một hệ thống chứng minh được nó gọi được.

---

## 1. Critical path — cái gì chặn cái gì

```
  ┌──────────────────────────────────────────────────────────────────────────┐
  │ ĐỢT 1 · Owner (4 việc, $6, ~1 tuần)                                       │
  │  C-1 nghe & ký giọng ─► C-2 mua Starter ─► C-3 render 12 MP3 ─┐          │
  └───────────────────────────────────────────────────────────────┼──────────┘
                                                                   ▼
  ┌──────────────────────────────────────────────────────────────────────────┐
  │ ĐỢT 2 · Dev (audio đầu-cuối, ~1 tuần)                                     │
  │  K-01 nối MP3 vào lab ─► K-02 endpoint TTS thật ─► K-03 bật segmentation  │
  │  ─► C-4 gọi 6 lượt MicroSIP × 3 miền và NGHE ─► NGHIỆM THU A1 thật       │
  └──────────────────────────────────────────────────────────────────────────┘
                                    │
        ┌───────────────────────────┼────────────────────────────┐
        ▼                           ▼                            ▼
  ┌───────────────┐  ┌──────────────────────────┐  ┌─────────────────────────┐
  │ ĐỢT 3 · Legal │  │ ĐỢT 4 · Nghiệm thu nội bộ│  │ ĐỢT 5 · Cổng ngoài      │
  │ ký kịch bản + │  │ 119 work item → ACCEPTED │  │ (song song, từ HÔM NAY) │
  │ retention     │  │ ⇒ NẤC 1                  │  │ Module 3 · Security ·   │
  │ ⇒ mở G-LEGAL  │  │                          │  │ Infra · Platform        │
  └───────────────┘  └──────────────────────────┘  └─────────────────────────┘
                                    │                            │
                                    ▼                            ▼
                    ┌────────────────────────────┐  ┌─────────────────────────┐
                    │ ĐỢT 6 · SIM thật ⇒ NẤC 2   │  │ ĐỢT 7 · Sandbox Sales   │
                    │ (chặn: G-LAB-SIM)          │  │ ⇒ NẤC 3 (chặn: G-CONTRACT│
                    └────────────────────────────┘  │  + G-AUTH)              │
                                    │                └─────────────────────────┘
                                    └──────────┬─────────────────┘
                                               ▼
                              ┌──────────────────────────────────┐
                              │ ĐỢT 8 · 32 eSIM + release ⇒ NẤC 4│
                              └──────────────────────────────────┘
```

**Đọc sơ đồ này theo một câu:** đợt 1 → 2 là việc **duy nhất** hoàn toàn trong tay đội IVR + owner;
đợt 5 phải **bắt đầu ngay hôm nay** vì nó là nhánh dài nhất; đợt 6 → 8 chỉ khởi động được khi
cổng ngoài mở.

---

## 2. ĐỢT 1 — Owner mở khoá audio (~1 tuần, `$6`)

**Mục tiêu:** có 12 file MP3 đoạn cố định để lab đọc được đơn thật.

| # | Việc | Ai | Đầu ra | Chặn bởi |
| --- | --- | --- | --- | --- |
| **C-2** | Mua **ElevenLabs Starter `$6`** + **đọc và trích dẫn ToS** về audio sinh trong kỳ trả phí | Owner | gói đã mua + trích ToS trong `OD-VOICE-01` | — |
| **C-1** | Nghe 3 giọng (Thắm/Bắc, Zara/Trung, Giang/Nam) **trong app** và **ký** | Owner | chữ ký trong `OD-VOICE-05` | C-2 |
| **C-3** | Render **12 MP3** = 4 câu cố định × 3 miền | Owner | 12 file MP3 | C-1 |

> ⚠️ **Thứ tự đã đảo so với bản đầu: mua TRƯỚC, nghe SAU.** Không có file mẫu nào của ba giọng
> này tồn tại — `OD-VOICE-05` chốt giọng **không qua bước nghe**, dựa trên mô tả văn bản. Nghĩa là
> muốn nghe thì phải render, mà render ở free tier tạo ra audio **không có commercial license**
> (`R17`). Mua trước rồi mới mở app thì mọi file sinh ra trong phiên đó đều dùng được, kể cả file
> anh vừa nghe và ưng. `$6` cho 30.000 credits, một lượt audition tốn ~300 — nghe thử bao nhiêu
> lần cũng không đáng kể.

> ✅ **Cập nhật `2026-08-26` — `W-0119`.** Phần dev của Đợt 1 đã xong: bộ hướng dẫn từng bước
> cho owner là [`docs/evidence/W-0108/segment-render-kit.md`](../../evidence/W-0108/segment-render-kit.md),
> và toàn bộ chuỗi công cụ đã được **chạy khô bằng audio giả** để 12 file thật chạy đúng ngay lần
> đầu. Lượt chạy khô tìm ra 2 lỗi, cả hai đã sửa —
> [`W-0108` §9](../../evidence/W-0108/README.md#9-kiểm-chứng-khô-chuỗi-bàn-giao-2026-08-26).
> **C-1/C-2/C-3 vẫn nguyên vẹn: chỉ owner làm được.**

**Lệnh in ra đúng 4 câu cần thu** (không phải đi tìm trong tài liệu):

```bash
pwsh ./deploy/lab/Convert-LabSegmentAudio.ps1 -ListOnly
```

**Tiêu chí nghiệm thu đợt 1:**
- Owner đã nghe cả 3 giọng — **không** ký dựa trên mô tả văn bản (`OD-VOICE-05` ghi rõ cơ sở
  quyết định trước đây là mô tả, **không ai đã nghe**).
- Gói ở trạng thái trả phí **tại thời điểm render** — free tier **không có commercial license**,
  audio audition **không** dùng được cho cuộc gọi thật (`R17`).
- Có câu trả lời bằng văn bản: **huỷ gói sau 1 tháng thì license của audio đã sinh còn hiệu lực
  không** (`R18`). Nếu không xác nhận được ⇒ **duy trì gói trả phí**, `$6`/tháng vẫn rẻ hơn mọi
  phương án khác.

> **Rủi ro nếu bỏ qua đợt này:** mọi buổi "chạy thử toàn tuyến" sau đó chỉ chứng minh được
> **kênh truyền**, không chứng minh được **nội dung**. Và một cuộc gọi thật phát audio không có
> commercial license là rủi ro pháp lý **không rollback được sau khi đã gọi**.

---

## 3. ĐỢT 2 — Audio đầu-cuối (~1 tuần, dev)

**Mục tiêu:** hai đơn khác nhau ⇒ khách nghe hai nội dung khác nhau, chứng minh bằng hash.

| # | Việc | Ai | Lệnh / file | Chặn bởi |
| --- | --- | --- | --- | --- |
| **K-01** | Chuyển 12 MP3 → PCM s16le/8 kHz/mono, loudnorm, cập nhật `SHA256SUMS` + `manifest.txt` | Dev | `pwsh ./deploy/lab/Convert-LabSegmentAudio.ps1 -SourceDirectory ...` | C-3 |
| **K-02** | Cấu hình **endpoint TTS thật** cho 3 đoạn biến thiên (endpoint + credential từ secret provider, **không hard-code vendor**) | Dev + Infra | `ConfigurableExternalTtsProvider` | C-2 |
| **K-03** | Dán khối `segments-compose-env.yml` (script tự sinh, đã thụt sẵn cho anchor `x-asterisk-lab-env`) vào compose | Dev | `docker-compose.softphone.yml` | K-01, K-02 |
| **K-04** | Test đo **cache hit trên dữ liệu thật**, không chỉ trên fixture | Dev | test mới | K-03 |
| **C-4** | **Gọi 6 lượt MicroSIP** (3 miền × phím `1`/`0`) và **NGHE** | Owner + Dev | `Invoke-FreeSoftphoneCall.ps1 -Region North\|Central\|South` | K-03 |

**Tiêu chí nghiệm thu đợt 2 — 5 điều, phải đạt cả 5:**

| # | Tiêu chí | Đo bằng |
| --- | --- | --- |
| 1 | Hai đơn khác nhau ⇒ **hai chuỗi audio khác nhau** | `PlaylistHash` (`UT-SEG-PLAYLIST-04`) — **không** so `ContentRef` đầu tiên, vì đoạn chào giống nhau |
| 2 | Thiếu một đoạn ⇒ ném lỗi có mã, **không** phát cuộc gọi thiếu nội dung | `TTS_FIXED_SEGMENT_NOT_RECORDED` (`UT-SEG-MISSING-07`) |
| 3 | Cache ấm: đơn thứ hai cùng nội dung ⇒ **0** lần gọi vendor | telemetry `ivr_tts_segments_total{kind,source}` |
| 4 | Đoạn cố định thiếu file ⇒ **fail-start**, không fail lúc đang gọi | `UT-SEG-FAILSTART-07` |
| 5 | **Nghe** đúng tên/món/số tiền của đơn tương ứng; Bắc đọc **"nghìn"**, Trung/Nam đọc **"ngàn"** | tai người, trên MicroSIP |

Bốn tiêu chí đầu **đã đạt** (W-0108). **Tiêu chí thứ 5 là thứ duy nhất còn thiếu**, và nó cần
audio thật.

> ⚠️ **Rủi ro `N-1` — đọc kỹ.** Lab hiện tại phát **một file cố định**. Một kết quả "gọi được,
> khách bấm 1, disposition đúng" chứng minh **chặng quay số**, **không** chứng minh khách nghe đúng
> đơn của mình. Tiêu chí #1 (hai đơn ⇒ hai hash) là phép thử **phân biệt được hai thứ đó**. Không
> có nó, buổi nghiệm thu sẽ nghiệm thu nhầm một thứ khác.

---

## 4. ĐỢT 5 — Cổng ngoài (bắt đầu **HÔM NAY**, chạy song song với mọi đợt khác)

Đây là nhánh **dài nhất** và không do đội IVR kiểm soát. Bắt đầu muộn một ngày là trễ một ngày ở
cuối. Toàn bộ tài liệu cần gửi **đã viết xong** — chúng chỉ đang nằm trong repo.

### 4.1 · Gửi Module 3

| # | Việc | Gửi cái gì | Đòi gì về |
| --- | --- | --- | --- |
| **K-10** | Gửi tài liệu bàn giao | [`integration-requirements/06-module-3-api-handover.md`](../../../integration-requirements/06-module-3-api-handover.md) — có payload copy-paste được + ô ký | **ngày trả lời cam kết** |
| **K-11** | Gửi closure pack | [`docs/contracts/target-v1-closure-pack/`](../../contracts/target-v1-closure-pack/README.md) — T-01..T-09 | ticket được nhận |
| **K-12** | Đòi **ma trận đã ký** (`OD-V1-13`) | T-01 | văn bản ký: `GOLDEN_HOUR+COD` có callable không? `GOLDEN_HOUR+ONLINE` có thuộc scope V1 không? |
| **K-13** | Đòi định nghĩa **`ivr_confirmation_required`** (`OD-V1-14`) | T-02 | nguồn business, ai set, khi nào, có bao giờ `false` |
| **K-14** | Đòi **OpenAPI endpoint callback generic** | T-05 + T-06 + T-08 | OpenAPI + ACK taxonomy phủ **cả hai** chương trình |
| **K-15** | Đòi quyết định **`order_state`** | IR-06 §3.7 | chọn (a) công bố state callable như dữ liệu, hoặc (b) cam kết hằng số hợp đồng |
| **K-16** | Đòi phương án **`dial_token`** (`OD-V1-17`/`18`) | T-04 + R-01 §4 | chọn a/b/c/d + chữ ký Security + sơ đồ trust boundary |
| **K-17** | Đòi **attempt policy production** (`OD-V1-16`) | T-09 | bảng đầy đủ mỗi program → attempt/offset/window; giải quyết xung đột `D-10` vs phase-8 |
| **K-18** | Đòi xác nhận **`OD-15`** | [`questions-to-module-3-od15-risk-evidence.md`](../../../plan/ivr-orther/questions-to-module-3-od15-risk-evidence.md) | ① sẽ gửi `trust.risk_evidence_available`? ② xác nhận **không** gửi `trusted_skip_allowed=false` mặc định ③ chốt tên mã `risk_flags` |

### 4.2 · Gửi Security/Platform

| # | Việc | Đòi gì |
| --- | --- | --- |
| **K-20** | Auth profile production (`OD-V1-07`) | issuer/JWKS/audience/scope/TTL + quyết định mTLS + **sandbox credential** |
| **K-21** | Chữ ký thứ hai của four-eyes (`OD-V1-20`) | Security/Platform + Release owner ký; **rồi mới** thay `PendingRuntimeGateAuthorization` |
| **K-22** | 8 mục hạ tầng `G-PLATFORM` (`W-0063`) | registry · K8s 4 env · secret store · observability backend · Grafana/Alertmanager · Argo Rollouts · warehouse · visual-regression |
| **K-23** | GitLab (`G-GITLAB`) | nâng Premium/Ultimate + reviewer thứ hai + chứng minh 1 required approval trước merge |

### 4.3 · Gửi Infra/vendor telephony

| # | Việc | Gửi cái gì | Đòi gì |
| --- | --- | --- | --- |
| **K-30** | Gửi gói RFQ | [`docs/contracts/telephony-procurement-pack/`](../../contracts/telephony-procurement-pack/README.md) — R-01..R-05 + biểu mẫu nghiệm thu lab | báo giá + capability statement |
| **K-31** | Đòi protocol/SDK (`OD-V1-09`) | R-01 | protocol docs · auth · timeout · webhook/poll semantics · version support |
| **K-32** | Đòi truth table disposition | R-01 §DT-02 | answered/busy/rejected/unreachable/invalid/dropped/network/SIM/audio/DTMF error |
| **K-33** | Đòi **1 SIM test + số allowlist** | R-02 | SIM + gateway GSM/SIP + số đích được duyệt |
| **K-34** | Đòi thông tin 32 eSIM (`OD-V1-10`) | R-03 | provisioning · concurrency/throughput đo được · failover · caller ID · cost |

### 4.4 · Gửi Legal/Privacy

| # | Việc | Gửi cái gì | Đòi gì |
| --- | --- | --- | --- |
| **K-40** | Gói pháp lý | [`docs/compliance/ivr-pdpa-legal-basis-pack.md`](../../compliance/ivr-pdpa-legal-basis-pack.md) + `ivr-retention-options.md` + `ivr-data-inventory.md` | chữ ký `DF-07` (retention) + `DT-05` (recording OFF) |
| **K-41** | Whitelist lời thoại (`OD-V1-15`) | IR-06 §3.5 | phê duyệt **mở rộng whitelist** — đây tự nó là một quyết định privacy, không phải chi tiết kỹ thuật |
| **K-42** | Kịch bản `v3-test-approved` | màn `/config` (`W-0109`) | chữ ký Content approver **và** Privacy/Legal approver — **hai người khác nhau** |

> **`W-0109` đã tạo lối thi hành cho chữ ký này.** Trước đó, lối duy nhất để chữ ký Pháp chế vào
> hệ thống là **sửa tay dữ liệu** — mất audit, mất `creator ≠ approver`, mất ý nghĩa cổng.
> Nay có API + quyền + màn hình. Nhưng lối đã mở mà chưa ai đi.

**Quyết định cần owner chốt trước khi Legal ngồi vào bàn:**

| ID | Câu hỏi | Đề xuất |
| --- | --- | --- |
| `OD-SCRIPT-01` | Content approver và Privacy/Legal approver phải là **hai người khác nhau**, nhưng hệ thống chỉ có 2 role. Thêm role thứ ba, hay ràng buộc theo `accountId`? | **Ràng buộc theo `accountId`** — thêm role thứ ba làm phình ma trận RBAC vừa khoá ở W-0105; ràng buộc "approver ≠ approver trước đó" thi hành được ngay trong `EnsureApprovalAllowed` |

---

## 5. ĐỢT 3 — Lối thi hành cho cổng Legal

**Điều kiện vào:** đợt 2 xong (Legal cần nghe được kịch bản thật, không chỉ đọc văn bản).

| # | Việc | Ai |
| --- | --- | --- |
| **K-50** | Chốt `OD-SCRIPT-01` | Owner |
| **K-51** | Nếu chọn ràng buộc `accountId`: implement "approver ≠ approver trước đó" trong `EnsureApprovalAllowed` | Dev |
| **K-52** | Tạo tài khoản riêng cho Content approver và Privacy/Legal approver | Admin |
| **K-53** | Legal duyệt kịch bản **qua màn `/config`**, không qua SQL | Legal |
| **K-54** | Legal ký `DF-07` retention + xác nhận `DT-05` recording OFF | Legal |

**Tiêu chí nghiệm thu:** chữ ký nằm trong `ivr_script_approvals` với đúng `accountId`, có audit,
và `creator ≠ approver` được ép ở tầng code — **không** phải bằng kỷ luật.

---

## 6. ĐỢT 4 — Nghiệm thu nội bộ ⇒ đạt **NẤC 1**

**Điều kiện vào:** đợt 2 xong, và evidence pack của mọi work item đã đủ.

Đây là khoảng cách **thủ tục**, không phải kỹ thuật. 77 work item ở `TESTS_PASS` và 19 ở
`EVIDENCE_SUBMITTED` cần được **reviewer/owner đọc và chấp nhận**.

| # | Việc | Ai | Ghi chú |
| --- | --- | --- | --- |
| **K-60** | ✅ **xong `2026-08-27` (`W-0121`)** — `remote.origin.pushurl` nay có hai giá trị (GitLab trước, GitHub sau), một `git push origin main` chạm cả hai | Dev + Platform | rủi ro `N-2` **chưa đóng**: sửa lối đẩy ≠ đã chạy hosted. Còn phải push thật. Protected `main` **không** chặn — fast-forward đã đi lọt `2026-08-25` (`bdde72c`); thứ chưa kiểm được là runner `#55115499` có online không |
| **K-61** | Chạy lại toàn bộ gate trên hosted runner, lưu artifact | CI | |
| **K-62** | Reviewer độc lập đọc 19 evidence pack `EVIDENCE_SUBMITTED` | Reviewer | `G-GITLAB` cần reviewer thứ hai — cùng người |
| **K-63** | Owner chuyển trạng thái sang `ACCEPTED` cho từng work item đạt | Release owner | **chỉ Release owner** làm được (`MASTER-05`) |
| **K-64** | Chạy `gate-status.mjs --write`, xác nhận readiness board mirror đúng | CI | CI đỏ nếu lệch tracker |

**Tiêu chí đạt nấc 1:** mọi prompt đã lập kế hoạch có evidence `ACCEPTED`, không cổng nào
`BLOCKED_INTERNAL`.

> **Nấc 1 KHÔNG đòi cổng ngoài phải đóng.** Nó chỉ đòi phần IVR làm được đã được chấp nhận.
> Đây là nấc **duy nhất** đội IVR + owner có thể đạt mà không cần bên thứ ba.

---

## 7. ĐỢT 6 — SIM thật ⇒ đạt **NẤC 2**

**Điều kiện vào:** `G-LAB-SIM` mở — có SIM + gateway + số allowlist + protocol vendor (K-31, K-33).

| # | Việc | Ai |
| --- | --- | --- |
| **K-70** | Implement adapter vendor thật sau `ISimGateway` (`P8-1` / `W-0048`) | Dev |
| **K-71** | Đóng khoảng trống thứ 4 của audit W-0048: kết hợp runtime `LAB_REAL_SIM` + Sales provider được validator duyệt | Dev |
| **K-72** | Nạp `LAB_DESTINATION_ALLOWLIST` = **chỉ số của chính chủ sở hữu** | Admin |
| **K-73** | Chạy lab protocol đầy đủ theo `P8-1` §3 | Dev + Owner |
| **K-74** | Viết lab acceptance report theo [biểu mẫu](../../contracts/telephony-procurement-pack/lab-acceptance-report-template.md) | Dev |
| **K-75** | `P8-2` runbook lab (`W-0049`) | Dev |

**Lab protocol phải phủ 8 kịch bản** (`IR-03` §3):
① allowlisted number bắt máy + phím `1` · ② bắt máy + phím `0` · ③ không bấm / bấm sai phím ·
④ busy/reject/unreachable nếu tái tạo được · ⑤ adapter timeout/network failure + recovery ·
⑥ kill switch ngăn dispatch mới · ⑦ **không quá một active call trên một channel** ·
⑧ log/evidence **không lộ** raw phone/full address/audio.

**Lab này KHÔNG trả lời:**
- ❌ "32 eSIM chịu được tải bao nhiêu" — một kênh không suy ra ba mươi hai kênh.
- ❌ "tích hợp Sales có đúng không" — đơn vẫn là mock.
- ❌ "gọi khách có ổn không" — chỉ gọi số của chính owner; `REAL_CUSTOMER_CALL_ALLOWED` vẫn `NO`.

---

## 8. ĐỢT 7 — Sandbox Sales ⇒ đạt **NẤC 3**

**Điều kiện vào:** `G-CONTRACT` + `G-AUTH` mở — có OpenAPI đã ký, endpoint callback generic tồn
tại, và **sandbox credential**.

| # | Việc | Ai |
| --- | --- | --- |
| **K-80** | Cấu hình `SALES_PROVIDER=TARGET_V1` trỏ sandbox thật | Dev |
| **K-81** | Chạy CDC test trên sandbox (`P4-1` / `W-0029`) | Dev |
| **K-82** | Chứng minh 5 hành vi: idempotency · stale version · state changed · blocked stock · timeout race | Dev |
| **K-83** | Chạy E2E cả **hai** chương trình qua endpoint generic | Dev |
| **K-84** | Đối chiếu: advisory `TRUST_RISK_EVIDENCE_UNAVAILABLE` **biến mất** ⇒ `OD-15` đang chạy | Dev |
| **K-85** | Tắt `CurrentGoldenHourCallbackAdapter` sau khi generic endpoint chạy | Dev |

**Tiêu chí đạt nấc 3:** Target V1 **đã ký** và contract test chạy trên Sales sandbox thật.

---

## 9. ĐỢT 8 — Production ⇒ đạt **NẤC 4**

**Điều kiện vào:** nấc 2 + nấc 3 đều đạt, `G-ESIM32` + `G-LEGAL` + `G-PLATFORM` mở.

| # | Việc | Ai |
| --- | --- | --- |
| **K-90** | Provisioning 32 eSIM, đo **throughput thực** (không dùng mặc định pilot 12 SIM) | Infra |
| **K-91** | Chứng minh failover + caller ID nhất quán | Infra + vendor |
| **K-92** | Deploy lần đầu lên hạ tầng thật; **chứng minh `helm rollback --atomic` chạy được** (rủi ro `N-5`) | Platform |
| **K-93** | `P9-1` release gate execution (`W-0050`) | Release owner |
| **K-94** | `P9-2` cutover/rollback/ops/hypercare (`W-0051`) | Release owner |
| **K-95** | `P10-5` SLA/error budget/on-call (`W-0056`) — chỉ sau K-94 | Ops |
| **K-96** | Ký `DF-03` | Release owner + security/privacy review |
| **K-97** | **Chỉ khi đó** mới xét đổi `REAL_CUSTOMER_CALL_ALLOWED` | Release owner |

**Bảy đầu vào go/no-go** (`P11-4` §2.5) — cả bảy hiện **chưa đạt**:

| Đầu vào | Work ID |
| --- | --- |
| two-program Sales flow | W-0002 |
| speech payload + dial token | W-0003, W-0004 |
| callback + auth | W-0005, W-0006 |
| attempt policy | W-0007 |
| one-SIM lab | W-0008 |
| 32 eSIM production capacity | W-0008 |
| legal, security và release evidence | W-0009 |

---

## 10. Ước lượng công sức — chỉ cho phần IVR kiểm soát

| Đợt | Nội dung | Ngày công dev | Chi phí | Phụ thuộc ngoài |
| --- | --- | ---: | ---: | --- |
| 1 | Owner mở khoá audio | 0 | **`$6`** | không |
| 2 | Audio đầu-cuối | **4–6** | — | không (sau đợt 1) |
| 3 | Lối thi hành Legal | **1–2** | — | chữ ký Legal |
| 4 | Nghiệm thu nội bộ ⇒ nấc 1 | **2–3** | — | reviewer + Release owner |
| 5 | Gửi/đòi cổng ngoài | **1–2** | — | **toàn bộ** |
| 6 | SIM thật ⇒ nấc 2 | **5–8** | SIM + gateway | `G-LAB-SIM` |
| 7 | Sandbox Sales ⇒ nấc 3 | **3–5** | — | `G-CONTRACT` + `G-AUTH` |
| 8 | Production ⇒ nấc 4 | **5–10** | 32 eSIM + hạ tầng | 5 cổng |

**Tổng phần IVR kiểm soát được (đợt 1–5): khoảng 8–13 ngày công.**
**Đợt 6–8 không ước lượng được theo lịch** — chúng phụ thuộc ngày bên khác giao artifact.

> **Con số này không phải "còn 13 ngày là xong".** Đợt 1–5 đưa hệ thống lên **nấc 1**, tức là
> "phần mềm đã được chấp nhận sau mocks". Từ nấc 1 đến nấc 4 là ba nấc nữa, và cả ba đều nằm ngoài
> tầm kiểm soát của đội IVR.

---

## 11. Thứ tự làm — nếu chỉ có một người

1. **Hôm nay:** gửi 3 gói tài liệu (K-10, K-30, K-40). Mất **nửa ngày**, mở nhánh dài nhất.
2. **Hôm nay:** owner nghe giọng + mua Starter (C-1, C-2). Mất **1 giờ**, `$6`.
3. **Tuần này:** C-3 → K-01 → K-02 → K-03 → C-4. Kết thúc bằng **buổi nghe thật**.
4. **Tuần sau:** K-50..K-54 (lối Legal) song song K-60..K-61 (hosted CI).
5. **Khi có phản hồi Module 3:** cập nhật contract nếu ma trận đổi — nhớ **4 tầng** phải sửa cùng lúc.
6. **Khi có SIM:** đợt 6.
7. **Khi có sandbox:** đợt 7.

---

## 12. Mười điều **không được làm** trong suốt kế hoạch này

1. **Không** tuyên bố `PRODUCTION_READY`, `CONTRACT_LOCKED`, hay "chỉ cấu hình là chạy" khi cổng
   ngoài còn mở.
2. **Không** đóng một cổng ngoài bằng một bản báo cáo — chỉ artifact thật đóng được.
3. **Không** coi mock evidence là lab evidence, hay lab evidence là production evidence.
4. **Không** coi lab một SIM là bằng chứng capacity 32 eSIM.
5. **Không** hard-code candidate policy `D-10` rồi dùng cho production (`R-V1-05`).
6. **Không** đổi `REAL_CUSTOMER_CALL_ALLOWED` trước khi `DF-03` được ký.
7. **Không** nới pattern PII để làm xanh một cổng — đó là **thay đổi chính sách privacy**, thuộc
   thẩm quyền owner + Privacy, không thuộc dev.
8. **Không** dùng audio sinh từ free tier cho cuộc gọi thật (không có commercial license).
9. **Không** tạo tracker/backlog thứ hai — mọi trạng thái ghi vào
   `prompt/_execution/prompt-execution-tracker.md`.
10. **Không** sửa `docs/documents/**` — đó là tài liệu gốc của business, chỉ dùng để truy nguồn.
    Nếu tài liệu business sai, owner business sửa, không phải IVR.

---

## 13. Cách đo tiến độ mà không cần ai báo cáo

| Muốn biết | Nhìn cái gì |
| --- | --- |
| Module 3 đã bật `OD-15` chưa | advisory `TRUST_RISK_EVIDENCE_UNAVAILABLE` **biến mất** khỏi log eligibility |
| Audio đã thật chưa | `PlaylistHash` của hai đơn khác nhau **khác nhau** |
| Cổng nào còn mở | `docs/release/gate-status.yaml` (CI sinh, đỏ nếu lệch tracker) |
| Nấc nào đã đạt | `docs/release/readiness-board.md` §2 |
| Gate nào đang chạy local thay vì hosted | `git remote -v` — `pushurl` có trỏ GitLab không |
| Có bao nhiêu work item đã `ACCEPTED` | tracker §5, đếm cột Status |
