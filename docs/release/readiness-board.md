# Production readiness board — `W-0060` · `P11-4`

Sinh từ `prompt/_execution/prompt-execution-tracker.md` bởi `deploy/ci/scripts/gate-status.mjs`. **Không sửa tay** — CI đối
chiếu và đỏ nếu hai bên lệch.

## 1. Đây là gương, không phải backlog thứ hai

Tracker là nguồn duy nhất. Bảng này **không** có trạng thái riêng: mỗi dòng mang đúng Work ID
và trạng thái của tracker. Cách một tấm gương biến thành backlog thứ hai không phải là một
quyết định — nó là một tháng tracker đi tiếp còn bảng thì không.

**Không có phần trăm ở đâu cả.** Một con số phần trăm mời người đọc hiểu "94% xong" là gần
xong, trong khi 6% còn lại là toàn bộ những cổng **không ai đóng được**.

## 2. Bốn nấc, và nấc đang đứng

| Nấc | Điều kiện vào | Đạt chưa |
| --- | --- | --- |
| 1. `IMPLEMENTATION_COMPLETE_BEHIND_MOCKS` | every planned prompt has ACCEPTED evidence and no gate is BLOCKED_INTERNAL | ❌ |
| 2. `LAB_REAL_SIM_VERIFIED` | one real SIM has completed the lab protocol with allowlist and kill switch evidence | ❌ |
| 3. `REAL_SALES_INTEGRATION_VERIFIED` | Target V1 is signed and contract tests run against a real Sales sandbox | ❌ |
| 4. `PRODUCTION_REAL_ELIGIBLE` | 32 eSIM capacity measured, legal/security evidence accepted, DF-03 signed | ❌ |

**Đang ở nấc 0.** 8/136 work item ở trạng thái `ACCEPTED`; phần còn lại
cao nhất là `EVIDENCE_SUBMITTED`, và **evidence đã nộp không phải evidence đã được chấp nhận**
(`MASTER-05`). Chỉ Release owner chuyển sang `ACCEPTED`.

## 3. Phân bố trạng thái (đếm, không phải tỉ lệ)

| Trạng thái | Số work item |
| --- | --- |
| `TESTS_PASS` | 88 |
| `EVIDENCE_SUBMITTED` | 20 |
| `BLOCKED_EXTERNAL` | 15 |
| `ACCEPTED` | 8 |
| `DEFERRED_TARGET` | 2 |
| `N/A` | 1 |
| `CODE_DONE` | 1 |
| `IN_PROGRESS` | 1 |

## 4. Cổng còn mở

| Gate | Chủ sở hữu | Trạng thái | Đóng bằng gì |
| --- | --- | --- | --- |
| `G-CONTRACT` | Sales API/Core | `BLOCKED_EXTERNAL` | approved OpenAPI + CDC tests |
| `G-SPEECH` | Sales/Product/Privacy | `BLOCKED_EXTERNAL` | schema/examples/privacy approval |
| `G-DIAL` | Sales/Security/Telephony | `BLOCKED_EXTERNAL` | threat model/API/tests |
| `G-AUTH` | Security/Platform | `BLOCKED_EXTERNAL` | auth profile + sandbox credential/tests |
| `G-POLICY` | Product/Core | `BLOCKED_EXTERNAL` | signed policy/version |
| `G-LAB-SIM` | Infra/vendor | `BLOCKED_EXTERNAL` | lab report/allowlist/kill-switch evidence |
| `G-ESIM32` | Infra/procurement | `BLOCKED_EXTERNAL` | procurement + measured capacity/failover |
| `G-LEGAL` | Legal/Privacy | `BLOCKED_EXTERNAL` | signed review |
| `G-RELEASE` | Release owner | `BLOCKED_EXTERNAL` | accepted go/no-go/evidence |
| `G-GITLAB` | Platform/Infra | `BLOCKED_EXTERNAL` | upgrade to Premium/Ultimate + add second reviewer + prove one required approval before merge |
| `G-PLATFORM` | Platform/Infra | `BLOCKED_EXTERNAL` | provisioned endpoints + credentials + smoke |

## 5. Đầu vào go/no-go (`P11-4` §2.5)

| Đầu vào | Work ID |
| --- | --- |
| two-program Sales flow | W-0002 |
| speech payload and dial token | W-0003, W-0004 |
| callback and auth | W-0005, W-0006 |
| attempt policy | W-0007 |
| one-SIM lab | W-0008 |
| 32 eSIM production capacity | W-0008 |
| legal, security and release evidence | W-0009 |

Cả bảy đầu vào đều chưa đạt. **15** work item ở `BLOCKED_EXTERNAL`, và
**23** quyết định `OD-V1-*` còn mở.

## 6. Kill switch và rollback

| | Trạng thái |
| --- | --- |
| `REAL_CUSTOMER_CALL_ALLOWED` | `false` ở **cả 4** môi trường, ép lúc render chart |
| kill switch | bắt buộc bật khi chế độ khác `MOCK`, ép lúc render |
| rollback | `helm rollback --atomic` + `after_script`; **chưa lượt deploy nào từng chạy** |
| cắt ngang cuộc đang gọi | W-0111: Admin/Operator có `IVR_CALL_TERMINATE`; API ghi yêu cầu, worker poll (mặc định ≤500 ms) rồi gateway hang up. Đây là cơ chế riêng, không gộp vào kill switch; mới có evidence software/MOCK, chưa phải SIM/carrier UAT |

## 7. Cái bảng này KHÔNG nói

- **Không nói "xong hết prompt" là sẵn sàng go-live.** Nấc 1 còn chưa đạt, và nó là nấc thấp
  nhất trong bốn nấc.
- **Không tự bật cờ nào.** File này báo cáo; nó không đặt giá trị (`P11-4` §3).
- **Không đóng cổng ngoài bằng một báo cáo.** Chỉ artifact thật đóng được (`P11-4` §3).
- **Không kiểm chất lượng của evidence.** Nó kiểm evidence **có tồn tại** và trạng thái được
  mirror đúng; nó không đọc nội dung.
