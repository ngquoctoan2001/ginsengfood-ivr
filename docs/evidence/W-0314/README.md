# W-0314 — Lô 2: luồng xoá dữ liệu phủ `phone_e164`

**Ngày:** `2026-09-18` · **Baseline:** `main@079af26` · **Nguồn:** Lô 2 (`T2`) của
[bản vướng mắc `17/09`](../../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md) · Toàn: *"tiếp tục lô 2"*

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Toàn chốt ngày `18/09`

Hỏi sau khi đọc lại code (§1), trước khi sửa dòng nào. Cả bốn câu theo đề xuất.

| # | Câu hỏi | Trả lời |
| --- | --- | --- |
| 1 | Mục `8`: ba cột Sales hôm nay chỉ xoá *"khi hết hạn"* — mà `S3` không có hạn | DSAR xoá `official_contact_id`, `customer_trust_status` và cờ đi kèm `trusted_skip_allowed`. **Giữ** `customer_id` làm khoá đối chiếu, ghi vào `NotErasable` kèm lý do |
| 2 | Xoá lần hai cho cùng một đơn ném lỗi | Sửa trong lô này — test viết trước, thấy đỏ rồi mới sửa |
| 3 | `DsarService` không có lối chạy | **Không** làm trong lô này — ghi thành vướng mắc mới (`S8`) |
| 4 | Dòng đã ẩn danh trước bản sửa vẫn còn số | Migration xoá luôn; không hoàn tác |

*Suy ra từ câu 1 + câu 4, báo lại Toàn:* backfill áp **cùng định nghĩa xoá mới** cho cả `4` cột, không chỉ
số điện thoại. Dòng đã xoá không bao giờ xoá lại được (§3), nên nếu chỉ dọn số, dòng cũ giữ khoá liên hệ
và hai giá trị trust mãi mãi.

---

## 1. Đọc lại trước khi sửa — bốn điều plan chưa có

| # | Phát hiện | Bằng chứng |
| --- | --- | --- |
| 1 | **Plan trỏ nhầm thân trigger.** Bước `3` bảo dựng hàm mới *"dựa trên bản cuối ở `P2_1_TaskIntake.cs:226-274`"* — đó là thân trong **`Down`**, bản **cũ**. Làm theo thì `call_script_template_id`, `call_script_version`, `evidence_policy_version`, `privacy_policy_version` thôi được giữ bất biến, và không test nào đang có bắt được | Bản thật: `Up`, dòng `134-190`. Kiểm bằng máy: `Down` của `W-0314` trùng **từng dòng** với `Up` của `P2_1`; `Up` của `W-0314` = `Up` của `P2_1` **cộng đúng `2` dòng**, không bớt dòng nào |
| 2 | **`DsarService` không có lối chạy.** Không endpoint, không CLI, không script, không đăng ký DI — chỉ test gọi nó. Runbook bảo *"chạy `EraseAsync`"* nhưng không có gì để chạy. Vì `S3`, đây là luồng xoá **duy nhất** | Tìm cả repo: ngoài `tests/`, không file nào gọi `DsarService`/`EraseAsync` |
| 3 | **Xoá lần hai ném lỗi.** Trigger chỉ cho đặt `anonymized_at` khi nó còn NULL; câu xoá khớp mọi task của đơn, kể cả task đã xoá ⇒ lần hai đóng dấu lại task cũ, trigger chặn, **cả câu** bị huỷ. Task Sales gửi lại sau lần xoá đầu **không bao giờ xoá được** | Test đỏ trước khi sửa: `P0001: confirmation-task contract/policy/speech snapshot is immutable` |
| 4 | **`customer_trust_status` là LEGACY_READ** (`OD-18`): không quyết định gọi/bỏ qua nào đọc nó. Lý do danh mục ghi để không xoá — *"đầu vào của một quyết định"* — đã cũ | `TaskIntakeService.cs` (`ReadLegacyTrustMetadata`, comment `OD-18`) |

`gitnexus_impact`: `RetentionTargetCatalog` **LOW** · `DsarService` **LOW**, `0` caller trực tiếp — index không
theo tham chiếu hằng, nên đọc tay: `SpeechSnapshotRedactionSql` dùng ở đúng `DsarService.cs:77` và
`RetentionTargetCatalog.cs:73`, tới `RetentionJob.cs:427`.

---

## 2. Thay đổi

| Nơi | Đổi gì |
| --- | --- |
| `RetentionTargetCatalog.SpeechSnapshotRedactionSql` | Thêm `phone_e164`, `official_contact_id`, `customer_trust_status`, `trusted_skip_allowed` → `NULL`. **Một** câu cho cả DSAR và retention, như trước |
| `DsarService` | Câu xoá thêm `AND anonymized_at IS NULL` · dry-run đếm **cùng** điều kiện · `NotErasable` thêm `customer_id` (`3` → `4`) |
| Migration `20260918005920_W0314ErasureReachesTheNumber` | **(1)** Backfill: dòng đã có `anonymized_at` → `4` cột kia về `NULL`, chạy **trước** khi thay hàm. **(2)** Hàm trigger: `phone_e164` vào danh sách bất biến, nhánh ẩn danh đòi `phone_e164 IS NULL`. `Down` trả lại đúng thân cũ, không trả lại số. Không `ALTER TABLE` |
| `PersonalDataInventory` | Lời khai `customer_id`, `customer_trust_status`, `official_contact_id`, `phone_e164` sửa theo hành vi thật; **thêm** `trusted_skip_allowed` — tên cột không khớp bộ lọc theo tên nên trước giờ lọt danh mục |
| Tài liệu | `data-inventory.md` · `dsar-runbook.md` (bốn giới hạn · chạy lại an toàn · §3.4 rollout · §5 chưa có lối chạy) · `ivr-pdpa-legal-basis-pack.md` · `retention-period-proposal.md` (`9` → `11` cột) · `pia.md` R-10 và `release-compliance-checklist.md` T-06 thôi nói DSAR đã chạy được — nay ghi *"chưa có lối chạy"* |

---

## 3. Thiết kế — vì sao hẹp như vậy

**Trigger chỉ thêm `phone_e164`**, đúng như plan. Ba cột Sales không vào trigger: hôm nay chúng không nằm
trong danh sách bất biến và không luồng nào sửa chúng sau intake (chỉ `TaskIntakeService` ghi, lúc tạo
dòng). Việc chúng bị xoá được giữ bằng câu SQL, gate đối chiếu và test. Thêm vào trigger sẽ nới phạm vi từ
chối lúc rollout ra mọi task có khoá liên hệ, để đổi lấy một bảo đảm mà gate đã có.

**Backfill trước, rồi mới thay hàm.** Hàm cũ không liệt kê các cột này nên `UPDATE` đi qua được. Sau khi
thay thì không: dòng đã xoá không đổi được gì nữa.

**Rollout, theo hướng đóng.** Pod cũ chạy câu xoá thiếu `phone_e164`; hàm mới **từ chối** câu đó trên
dòng có số — không dòng nào bị đánh dấu *"đã xoá"* khi chưa xoá. Trên dòng chỉ có token, câu cũ xoá đủ
nên vẫn đi qua (`COMP-DSAR-11`). Runbook §3.4: không chạy xoá trong lúc pod cũ và mới cùng chạy.

**Một câu SQL cho hai luồng**, giữ nguyên. Retention (`speech_snapshot`) và DSAR dùng chung hằng; cả hai
đều có test riêng trên Postgres thật (`IT-RET-PII-08`, `COMP-DSAR-08`).

---

## 4. Test — đỏ trước, xanh sau

Chạy trên code **chưa sửa** trước, rồi sửa, rồi chạy lại. Postgres `16` thật (Testcontainers).

| `TestId` | Kiểm gì | Trước khi sửa | Sau |
| --- | --- | --- | --- |
| `COMP-PII-02` (unit, `2` test) | Gate đối chiếu: mọi cột danh mục khai *"Replaced with a redacted value"* ⇔ có trong câu SQL, **hai chiều** · mọi cột khai *"Retained"* phải có tên trong `NotErasable` | 🔴 `phone_e164` được hứa mà không có trong câu SQL | 🟢 |
| `COMP-DSAR-08` | Xoá bỏ số và khoá Sales, giữ `customer_id`; đơn khác không bị chạm | 🔴 sau khi xoá, câu SQL §6 vẫn thấy **`1`** dòng có số | 🟢 |
| `COMP-DSAR-09` | Xoá lần hai tới được task tới sau lần đầu; task cũ giữ nguyên dấu; dry-run đếm đúng; lần ba trả `0` và vẫn có audit | 🔴 `P0001 … immutable` | 🟢 |
| `COMP-DSAR-10` | Số của task đã nhận **không sửa được** | 🔴 `UPDATE` đi qua | 🟢 |
| `COMP-DSAR-11` | Câu xoá cũ (pod cũ) bị từ chối **đúng** trên dòng có số, đi qua trên dòng chỉ có token | 🔴 không bị chặn | 🟢 |
| `COMP-DSAR-12` | Migration dọn dòng đã xoá kiểu cũ, **không** chạm dòng đang sống | 🔴 chưa có migration | 🟢 |
| `IT-RET-PII-08` | Retention cũng xoá số và khoá Sales | 🔴 `phone_e164` còn nguyên | 🟢 |
| `COMP-DSAR-02` (sửa một dòng) | `NotErasable` có `4` mục | 🔴 `3` | 🟢 |

---

## 5. Đột biến — `8/8` bị bắt

Mỗi đột biến là một chỗ sửa đúng một lần xuất hiện; build lại; chạy test phải đỏ; trả file về và kiểm
**SHA-256 trùng** bản gốc (`8/8`).

| # | Đục | Test đỏ |
| --- | --- | --- |
| `M1` | Câu SQL bỏ `phone_e164 = NULL` | `COMP-PII-02` (gate) · `COMP-DSAR-08` · `IT-RET-PII-08` |
| `M2` | Trigger bỏ `phone_e164` khỏi danh sách bất biến | `COMP-DSAR-10` |
| `M3` | Nhánh ẩn danh bỏ điều kiện `phone_e164 IS NULL` | `COMP-DSAR-11` |
| `M4` | Câu xoá bỏ `AND anonymized_at IS NULL` | `COMP-DSAR-09` |
| `M5` | Migration không backfill | `COMP-DSAR-12` |
| `M6` | Dry-run đếm cả task đã xoá | `COMP-DSAR-09` |
| `M7` | `NotErasable` không còn nêu tên `customer_id` | `COMP-PII-02` |
| `M8` | Danh mục thôi hứa xoá `official_contact_id` — **chiều ngược** của gate | `COMP-PII-02` |

`M7` không làm `COMP-DSAR-02` đỏ, và đúng vậy: đột biến đổi chữ, không đổi số mục.

---

## 6. Điều kiện *"Xong khi"* — câu SQL và kết quả

> *"Sau DSAR, câu SQL trên DB thật không tìm thấy số điện thoại dạng đọc được nào của đơn đó."*

Quét **cả dòng** chứ không theo danh sách cột, vì danh sách cột chính là thứ đã sai. Bỏ `id` vì UUID ngẫu
nhiên có thể chứa mười chữ số liền nhau.

```sql
SELECT count(*)::int
FROM ivr_confirmation_tasks t
WHERE t.order_code = @order_code
  AND (to_jsonb(t) - 'id')::text ~ '(^|[^0-9])(\+?84|0)[0-9]{9}([^0-9]|$)';
```

| Tình huống | Trước | Sau | Test |
| --- | --- | --- | --- |
| DSAR trên đơn có số | `1` | **`0`** | `COMP-DSAR-08` |
| Đơn khác, cùng lúc | `1` | `1` — không bị chạm | `COMP-DSAR-08` |
| Đơn đã xoá kiểu cũ (còn số), rồi chạy migration | `1` | **`0`** | `COMP-DSAR-12` |
| Đơn đang sống, qua migration | `1` | `1` — backfill không chạm | `COMP-DSAR-12` |
| Hai lần xoá, task thứ hai tới sau lần đầu | — | **`0`** | `COMP-DSAR-09` |

**Gate đối chiếu:** xanh; đỏ khi đục theo **cả hai chiều** (`M1` xuôi, `M8` ngược).

### Chạy từ ngoài — trên DB mà image thật đã migrate

`DsarService` không có lối chạy (`S8`), nên câu xoá được chạy **tay** bằng `psql` — đúng văn bản
`DsarService` sinh ra — trên DB của stack `ivr-w0314`, sau khi image `ivr-migrate` thật (EF bundle) áp
migration, seed nạp `10` fixture và `28` ví dụ đã chạy qua HTTP.

| # | Kiểm | Kết quả |
| --- | --- | --- |
| 1 | Migration cuối đã áp | `20260918005920_W0314ErasureReachesTheNumber` |
| 2 | Hàm trigger đang cài có cả hai dòng `phone_e164` | `t` · `t` |
| 3 | Task mang số (seed + ví dụ) | `2`; chọn đơn `GF-2026-GH-0005` |
| 4 | Dòng còn số đọc được của đơn đó, trước | `1` |
| 5 | Đổi số của task đã nhận | `ERROR: … snapshot is immutable` |
| 6 | Câu xoá cũ, như pod cũ chạy | `ERROR: … snapshot is immutable` |
| 7 | Câu xoá từ `W-0314` | `UPDATE 1` |
| 8 | Dòng còn số đọc được, sau | **`0`** |
| 9 | Chạy lại câu xoá | `UPDATE 0`, không lỗi |
| 10 | Task còn lại vẫn giữ số | `1` |

---

## 7. Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Toàn bộ solution | **`1184/1184`** (`789` unit · `363` integration · `24` contract · `8` chaos), `0` failed `0` skipped, `0` warning — `1176` → `1184`. Lần chạy cả solution, unit ra `788/789`: gate traceability đỏ vì bảng chưa sinh lại. Sinh lại, chạy lại cả project unit: `789/789` |
| Chạy từ ngoài | **`28/28`** ví dụ qua HTTP thật trên stack riêng `ivr-w0314`; migration áp bằng image `ivr-migrate` thật, gồm `TASK-M3-NUMBER` trọn vòng tới callback — trigger mới không chặn `UPDATE` hợp lệ nào trên luồng thật. Gỡ stack **và volume của nó** sau khi chạy; volume dev dùng chung không bị đụng |
| Gate sweep (Git Bash) | **`40/40`** chạy, `21` bỏ qua theo manifest (`dr-selftest` `136.5s`) |
| `TestId` mới | **`8`**: `COMP-PII-02` · `COMP-DSAR-08..12` · `IT-RET-PII-08` (`COMP-DSAR-02` sửa một dòng) |
| Traceability | `735` → **`743`** |
| Expand guard | Migration chỉ có thân hàm và `UPDATE` — không `ALTER TABLE`, không `DROP` |
| `gitnexus_detect_changes` | risk `low`, `0` luồng bị ảnh hưởng, `16` file. Ký hiệu production chạm: `DsarService` (`EraseAsync`, `NotErasable`), `PersonalDataInventory.Fields`, `RetentionTargetCatalog`. Migration mới chưa có trong index |

---

## 8. Còn mở

| Việc | Ở đâu |
| --- | --- |
| **`S8` — ai được xoá dữ liệu khách, và bằng công cụ gì.** Lô này làm cho lệnh xoá **đúng**; chưa có cách nào **chạy** nó ngoài test | Bản vướng mắc §2, chờ Sếp + Toàn |
| `IR-07` `A-9` (*"xoá khi khách yêu cầu"*) hứa một việc mà hôm nay chưa có lối chạy — cùng gốc với `S8` | Không sửa phiếu đã gửi (luật `14`) |
| `retention-period-proposal.md` vẫn ghi *"chờ pháp chế điền số"* — cũ từ `S3` | Lô 3 mục `1` |
| Gate `compliance-pack-selftest.mjs` vẫn kiểm **ba** giới hạn trong DB; giới hạn thứ tư (`customer_id`) được giữ ở mức code bằng `COMP-PII-02`, và đã ghi ở runbook lẫn bộ hồ sơ pháp lý | Để nguyên — ghi để biết |
| `eligibility_snapshot_json` lưu nguyên văn, có thể mang object `trust` legacy (`OD-15`). Các trường khai trong đó nói về bộ phân giải (`risk_evidence_available`, `resolver_*`), không định danh ai; xoá không chạm cột này | Ghi để biết |
