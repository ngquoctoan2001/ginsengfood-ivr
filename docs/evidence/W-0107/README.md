# W-0107 — Việt hóa giao diện + dữ liệu · Gói bằng chứng

Trạng thái: `TESTS_PASS` — **chưa** `ACCEPTED`
Ngày: `2026-08-22`
Baseline: `main@f7c9be9` (+ WIP `W-0105`/`W-0106` chưa commit)
Plan: [`plan/ivr-orther/W-0107-vietnamese-localization-plan.md`](../../../plan/ivr-orther/W-0107-vietnamese-localization-plan.md)

---

## 1. Đã làm gì

| GĐ | Nội dung | Kết quả |
| --- | --- | --- |
| 1 | Sinh inventory enum tự động từ 3 nguồn | [`enum-inventory.md`](enum-inventory.md) — 31 enum OpenAPI + 6 CHECK constraint + 11 trường write-site |
| 2 | Từ điển dữ liệu | `admin-ui/src/i18n/enums.vi.json` — **39 họ / 212 giá trị** |
| 3 | Hạ tầng `t(params)`, `tEnum()`, `<EnumLabel>` | 3 file mới, tương thích ngược 100% |
| 4 | Áp `<EnumLabel>` lên 8 màn | 41 điểm render |
| 5 | Bộ lọc → dropdown; gộp họ trùng; sửa chuỗi Anh sót | 4 bộ lọc, 7 khoá gộp, 2 chuỗi |
| 6 | 4 lớp guard chống trôi ngược | 2 test file mới, đều mutation-test |
| 7 | Sửa assertion | 1 assertion (không phải ~18 như plan ước lượng) |
| 8 | Gói bằng chứng | file này |

### Thực thi `OD-L10N-02a` (khác plan gốc)

Plan đầu xếp `fail_closed_effect` vào nhóm "hoãn vì cần đổi contract". Rà lại
[`AdminConfigReadService.BuildDependencies`](../../../src/Ivr.Api/Application/AdminConfigReadService.cs)
cho thấy kết luận đó **sai**: trường này là **6 hằng số** khoá theo `dependency` — vốn đã là mã —
nên dịch được hoàn toàn ở frontend, **không đụng contract**.

Đã làm trong lượt này, không cần `oasdiff`, không re-pin `contract-manifest.json`:

| Trường | Xử lý |
| --- | --- |
| `fail_closed_effect` | ✅ Dịch, khoá theo `dependency` (6/6 ô) |
| `detail` — 3 dependency hằng | ✅ Dịch, khoá theo `dependency` |
| `detail` — `DIAL_KILL_SWITCH` | ✅ Dịch, chọn theo `state` |
| `detail` — `SIM_GATEWAY`, `ORDER_CORE` | ⏸ Giữ tiếng Anh **có chủ đích** — telemetry `key=value`, cùng họ với log |
| `event.effect` — `REVIEW_ITEM` | ✅ Tách `^([A-Z_]+): ([A-Z_]+)$`, dịch cả hai vế |
| `event.effect` — `CAPACITY_INCIDENT` | ⏸ Hoãn (`OD-L10N-02b`) — thiếu `hold_new_calls` trong contract |

### Sửa lỗi tự phát hiện trong `§5.3` của plan

5/7 nhãn `recommendedCoreAction` ban đầu đều mở đầu bằng "Đề nghị Core kiểm lại rồi…",
đẩy phần phân biệt xuống cuối câu — sẽ bị cắt đúng chỗ mang nghĩa trên bảng hẹp.
Đã viết lại, đưa động từ phân biệt lên đầu: "Xác nhận đơn — sau khi Core kiểm lại".

---

## 2. Kết quả kiểm thử

| Bộ | Kết quả |
| --- | --- |
| .NET ContractTests | `22/22` PASS |
| .NET UnitTests | `420/420` PASS (+1 mới: `IT-L10N-DBENUM-04`) |
| .NET ChaosTests | `6/6` PASS |
| .NET IntegrationTests | `227/227` PASS |
| **.NET tổng** | **`675/675` PASS** |
| admin-ui vitest | **`200/200` PASS** (21 file) |
| `eslint --max-warnings 0` | PASS |
| `tsc --noEmit` | PASS |
| `next build` | PASS — 18 route |
| `dotnet format --verify-no-changes` | PASS |
| `generate-test-traceability.mjs` | `396` test có tag |

### Test mới

| TestId | Nội dung | Vị trí |
| --- | --- | --- |
| `UT-L10N-COVER-03` | Mọi enum OpenAPI console render đều có nhãn; mọi giá trị chỉ ở **một** catalogue; không họ rỗng, không nhãn trùng mã | `admin-ui/tests/unit/enum-coverage.test.ts` |
| `UT-L10N-ENUM-01` | `tEnum()` trả mã gốc + `known:false` cho giá trị lạ; phân biệt **vắng** với **chưa dịch** | cùng file |
| `UT-L10N-NOENG-05` | Chuỗi **tiếng Anh** trong prop hiển thị là vi phạm | `admin-ui/tests/unit/i18n-a11y.test.ts` |
| `IT-L10N-DBENUM-04` | Mọi `CHECK … IN (…)` trong EF model có nhãn trong `enums.vi.json` | `tests/Ivr.UnitTests/ConsoleEnumDictionaryTests.cs` |

### Mutation test — chứng minh guard có răng

Theo chuẩn đã lập ở `A-0318`/`A-0319`, mỗi guard mới được chứng minh bằng cách phá có chủ đích:

| Guard | Phép phá | Kết quả |
| --- | --- | --- |
| `UT-L10N-NOENG-05` | Thêm `aria-label="Governance overview panel"` vào `EmptyState.tsx` | **ĐỎ** — báo đúng `components/feedback/EmptyState.tsx:23` |
| `IT-L10N-DBENUM-04` | Xoá `approvalType.PRIVACY_LEGAL` khỏi từ điển | **ĐỎ** — `Failed: 1` |
| `UT-L10N-COVER-03` | (tự phát hiện trong lúc viết) `decision` và `status` dùng lại tên cho nhiều enum khác nhau | **ĐỎ** — buộc đổi sang ánh xạ 1→N |

Probe đã gỡ; cả ba xanh lại sau khi khôi phục.

---

## 3. Điều **không** làm, và vì sao

| Hạng mục | Lý do |
| --- | --- |
| `order_state` | `NT-3` — Order Core sở hữu, contract khai `Opaque enum` (`D-02`). Miễn trừ **tường minh** trong `enum-coverage.test.ts` kèm giải thích, để người sau không tưởng là bug rồi bịa từ điển. |
| `detail` telemetry (2 ô) | Dòng chẩn đoán `key=value`, dịch ra mất khả năng grep và lệch với log. |
| `event.effect` CAPACITY_INCIDENT | `OD-L10N-02b` — cần thêm `hold_new_calls` vào contract. |
| CSV export | `NT-5` — giữ mã gốc. Test `reports-screen.test.ts:313` khoá `PROGRAM,GOLDEN_HOUR,11,6,…` **vẫn xanh, không sửa một ký tự**. |
| Audit log, evidence | `NT-5` — không đụng. |
| `CHECK` cho các cột enum còn lại | `W-0115` — đã triển khai, xem [`docs/evidence/W-0115/`](../W-0115/). |
| `script_variant` trong báo cáo | Giá trị là version id (`v3-test-approved`), không phải enum. Ép qua từ điển sẽ báo mọi dòng "chưa dịch" — cảnh báo giả. |

---

## 4. Ảnh hưởng

`gitnexus impact` trên `t()`: **CRITICAL** — 75 symbol, 70 caller trực tiếp, 25 flow, 3 module.

Đã cảnh báo trước khi sửa. Đánh giá: mức này phản ánh **fan-out**, không phải rủi ro ngữ nghĩa —
`params` là optional nên **không một lời gọi nào trong 70 chỗ phải sửa**, và TypeScript chặn ngay
nếu sai. Xác nhận bằng `tsc` + `200/200` test + build production.

`gitnexus detect_changes`: `risk_level: critical`, 859 symbol / 151 file. Con số này là **tổng hợp
trên toàn worktree**, bao gồm 206 file WIP của `W-0105`/`W-0106` chưa commit (persistence/auth/API
client) — đúng như `A-0316` đã ghi nhận. Tập thay đổi của riêng W-0107 là **9 file mới + 19 file sửa**,
toàn bộ ở lớp render UI, từ điển, test và tài liệu; **không sửa file `.cs` production nào**.

---

## 5. Gate còn lại

| Gate | Trạng thái |
| --- | --- |
| Owner duyệt từ vựng (`OD-L10N-05`) | Owner đã uỷ quyền cho đề xuất của Claude (2026-08-22). Bảng đối chiếu đầy đủ để rà lại bất cứ lúc nào: [`vocabulary-review.md`](vocabulary-review.md) — 4 họ trọng yếu xếp ở Phần 1 |
| `OD-L10N-01` (vị trí hiện mã) | Đã triển khai theo đề xuất: tooltip ở bảng, dòng mono ở màn chi tiết. Chờ owner xác nhận. |
| `OD-L10N-03` (thuật ngữ giữ nguyên) | Đã triển khai: `fail-closed`, `correlation id`, `idempotency key`, `kill switch` nằm trong allow-list của `UT-L10N-NOENG-05` |
| Ảnh chụp trước/sau 8 màn | Thay bằng [`vocabulary-review.md`](vocabulary-review.md) — bảng mã ↔ nhãn phục vụ việc duyệt từ vựng tốt hơn ảnh chụp. Ảnh chụp thật cần stack PostgreSQL + API + seed, để lại cho lượt UAT |
| Ghi tracker | ✅ `W-0107` cấp chính thức; `A-0321`/`A-0322`; `NEXT_WORK_ID` → `W-0108` |
| `REAL_CUSTOMER_CALL_ALLOWED` | `NO` — không đổi |

---

## 6. Ghi tracker

Mâu thuẫn `NEXT_WORK_ID` nêu ở bản trước **đã tự giải quyết**: `W-0106` được cấp chính thức
trong lúc lượt này đang chạy, nên `NEXT_WORK_ID` chuyển sang `W-0107` và số này lấy được
mà không nhảy cóc.

| Mục | Giá trị |
| --- | --- |
| Work ID | `W-0107` |
| Activity | `A-0321` (START/FINISH), `A-0322` (VALIDATION) |
| `NEXT_WORK_ID` sau lượt | `W-0108` |
| Status | `TESTS_PASS` — **không** nâng `ACCEPTED` |

Không nâng `ACCEPTED` vì acceptance thuộc về owner sau khi dùng thật, không phải thuộc
về việc test xanh. Quy tắc tracker §1.6.

---

## 7. Cách kiểm chứng lại

```bash
# Guard phủ enum
cd admin-ui && npx vitest run tests/unit/enum-coverage.test.ts tests/unit/i18n-a11y.test.ts

# Guard đọc CHECK constraint
dotnet test tests/Ivr.UnitTests/Ivr.UnitTests.csproj --filter "TestId=IT-L10N-DBENUM-04"

# Không còn enum thô ở vị trí nhãn chính
grep -rnE "cell: \([a-z]+\) => [a-z]+\.(status|queue_status|result_type|reason|decision)" admin-ui/src

# CSV export vẫn giữ mã gốc (NT-5)
grep -n "PROGRAM,GOLDEN_HOUR" admin-ui/tests/e2e/reports-screen.test.ts
```
