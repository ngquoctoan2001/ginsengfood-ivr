# ARCH-05 — Resilience (Failure / Fail-closed / Retry / Cache)

Trạng thái: `SRS_DRAFT` · Sinh bởi: `p08` · Nguồn: `phase-8/02 §10`, `/16`; D-04/D-06/DO-06; MASTER-04.
Nguyên tắc tối cao: **fail-closed** — source-of-truth/policy/ops không khả dụng ⇒ **không gọi khách thật / không confirm**.

## 1. Failure matrix (bám phase-8/02 §10)
| Hệ thống down | Before attempt | During attempt | During callback |
| --- | --- | --- | --- |
| **Order Core** | Không tạo task mới | Tiếp tục call đã dispatch an toàn; callback retry bounded | Retry bounded / admin review |
| **Ops Sellable Gate** (Core gọi) | **Không dispatch** (fail-closed DO-06) | Nếu call đã có result, Core revalidate trước khi hành động | Core **block** nếu không revalidate được |
| **Trust/Contact resolver** | Hold task / review | Không đổi contact giữa cuộc gọi | Core quyết với source hiện có |
| **CRM do-not-call** (DC-01) | **Không dispatch** nếu không xác định opt-out | Không đổi giữa cuộc gọi | Core quyết; IR-CRM-01 bổ sung rich fields |
| **Evidence Registry** | Không final-callback nếu thiếu evidence | Technical exception nếu ghi evidence fail | Hold / admin review |
| **SIM Gateway** | `IVR_TECHNICAL_EXCEPTION` | Technical exception, **không** no-answer (P0-IVR-004) | N/A |
| **Admin Web** | Không ảnh hưởng vận hành | Không ảnh hưởng | Không ảnh hưởng |

## 2. Fail-closed cụ thể
- Ops `non-2xx / timeout / /health/ready=503` khi revalidate → coi "không xác thực được blocker" → không dispatch/không confirm (DO-06).
- Thiếu evidence policy/version, script chưa duyệt, phone unknown → reject/hold (không đoán).
- `REAL_CUSTOMER_CALL_ALLOWED=NO` (mặc định) → mọi dispatch = dry-run/mock (DF-03/DT-01).

## 3. Retry (bounded, kỹ thuật)
- **Callback retry** (D-04): chỉ khi timeout/5xx/`TECHNICAL_RETRY_ALLOWED`; **cùng idempotency key**; không tạo result mới, không tăng customer attempt, không đổi result status, không bypass stale guard; bounded (count/backoff ⏳ OD-10).
- **SIM technical retry**: `is_counted_customer_attempt=false`; tách khỏi customer attempt (≤2, D-10).
- Không retry vô hạn → hết retry: admin review.

## 4. Circuit breaker
- **Ops sellable gate** (phía Order Core): breaker mở khi lỗi liên tục → **fail-closed** (không dispatch), không "tạm bỏ qua blocker".
- **Core callback** (phía IVR): breaker mở → hold callback + admin review; không duplicate transition.
- **SIM channel**: `fail_count≥3/10′` → auto-disable + alert (docx §10).

## 5. Caching — quy tắc (P0)
| Được cache (TTL ngắn) | KHÔNG cache |
| --- | --- |
| Script template/version (approved) | ❌ **Sellable/Sale-Lock/Recall blocker** — phải realtime lúc revalidate |
| Program policy config (window/spacing D-10) | ❌ Order state (revalidate ở Core) |
| Permission/role lookup (DF-01) | ❌ do-not-call/opt-out ở thời điểm dispatch |
| `phone_validation_status` (trong task) | ❌ availability tồn kho critical |

- ⚠️ **Snapshot `sellable_status[]` trong task KHÔNG phải cache** — là ảnh chụp point-in-time để pre-dispatch block; **chân lý = Core revalidate realtime** (DO-02/DO-03). Không dùng snapshot thay revalidate.

## 6. Idempotency là lớp chống duplicate
- Task/callback duplicate → trả kết quả cũ (DF-04); webhook ops dedupe `EventId` (DO-04).
