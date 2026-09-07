# W-0213 — `environment` scope một loại approval, và trơ với hai loại kia

Ngày: 2026-09-07 · Baseline: `main@f271218` · Trạng thái: **TESTS_PASS** cho phần ghim và tài liệu;
**OWNER_DECISION_REQUIRED** cho câu hỏi thật.

Mục 0.5 là mục cuối của nhóm 0. Nó cũng là mục duy nhất mà **tiền đề của chính worklist bị sai**, và
việc đầu tiên phải làm là sửa điều đó.

## 1. Cái tôi viết sai

Worklist ghi tiêu đề *"Ngữ nghĩa approval theo môi trường **chưa có thật trong code**"*. Sai. Codex
(F02) viết đúng phạm vi hơn — nó nói về `AnyLiveAsync` — còn tôi khái quát thành cả hệ thống
approval. Đọc code thì ngữ nghĩa đó **có thật**, và ở nơi quan trọng nhất nó còn được thi hành **hai
lớp**:

| Loại approval | Đọc bởi | Lọc `environment`? |
| --- | --- | --- |
| `FEATURE_FLAG_CHANGE` | `PostgresFourEyesApprovalVerifier.VerifyAsync` | **Có, hai lớp** |
| `RUNTIME_GATE_ADMIN` | `RuntimeGateApprovalReader.AnyLiveAsync` | Không |
| `PRODUCTION_CALL` | `AnyLiveAsync` | Không (không migration nào seed) |

Hai lớp của `FEATURE_FLAG_CHANGE`:

1. `AND environment = {3}` — predicate cột, tham số là `before.Environment`.
2. `AND change_fingerprint = {2}` — mà `RuntimeGateFingerprint.Of()` đặt `snapshot.Environment`
   **làm trường đầu tiên** của chuỗi canonical.

Nên một approval cấp cho lab **không thể** dùng cho production, kể cả khi ai đó xài lại
`approval_reference`: fingerprint đã khác trước khi predicate cột được chạm tới.

## 2. Cái thật sự sai — cột được ghi cho ba loại, đọc cho một

`environment` có mặt trên mọi row. `W0195` seed `RUNTIME_GATE_ADMIN` với `environment = NULL`.
Constraint `ck_ivr_runtime_gate_approvals_change_binding` bắt buộc `environment IS NOT NULL` **chỉ
cho** `FEATURE_FLAG_CHANGE` — tức schema *biết* sự khác biệt nhưng không nói ra.

Hệ quả: ai chèn một row `RUNTIME_GATE_ADMIN` với `environment='lab'` sẽ tin mình đã giới hạn quyền
quản trị về lab. Không hề. Gate vẫn trả `true` ở mọi môi trường.

## 3. Một tầng nữa, tìm được vì test đỏ

Bản test đầu tiên của tôi định chứng minh tính trơ bằng cách `UPDATE ... SET environment = 'lab'`
trên row đã seed rồi khẳng định gate vẫn mở. Nó **đỏ**, và câu trả lời hay hơn giả định của tôi:

```text
P0001: a granted runtime gate approval is immutable; only revocation may change
```

Trigger append-only từ chối thẳng. Nghĩa là **không ai thu hẹp được một admin grant sau khi đã cấp**
— kể cả người có quyền vào database. Cách duy nhất là revoke rồi cấp lại, mà cấp lại cũng không giới
hạn được gì vì reader không đọc cột.

Hai nửa đó — *không sửa được* và *sửa cũng vô nghĩa* — mới là hình dạng đầy đủ của vấn đề, và nửa
thứ nhất chỉ lộ ra khi chạy thật.

## 4. Đã làm gì

**Không sửa một dòng hành vi nào.** Admin gate có nên scope theo môi trường hay không là câu hỏi của
Security/Platform, không phải của M8.

### 4.1. `IT-GATE-APPROVAL-10`

`tests/Ivr.IntegrationTests/RuntimeGateApprovalTests.cs`, trên Postgres thật:

| Khẳng định | |
| --- | --- |
| approval lab verify được thay đổi lab | scope hoạt động |
| **không** verify được cùng thay đổi ở production | scope thật sự chặn |
| fingerprint lab ≠ fingerprint production | chặn từ lớp thứ nhất |
| `UPDATE environment` trên row đã cấp → `PostgresException` | không thu hẹp được |
| revoke → gate đóng | revoke mới là công tắc |
| cấp lại row `RUNTIME_GATE_ADMIN` với `environment='lab'` → gate **vẫn mở** | cột trơ |

### 4.2. Ghi vào nơi người ta sẽ đọc

`RuntimeGateApprovalKinds` — hai khối doc mới: `FeatureFlagChange` nói nó là loại **duy nhất** được
scope và scope hai lớp; `RuntimeGateAdmin` nói `environment` của nó được ghi và **không bao giờ được
đọc**, trỏ sang khối kia. Đây là chỗ một người tra cứu loại approval sẽ nhìn vào.

### 4.3. Sửa hai đề xuất không thi hành được (mục B4)

Bản audit gốc đề xuất *"gate seed theo environment (chỉ dev/lab)"* và *"thêm test khẳng định prod
không có `RUNTIME_GATE_ADMIN` sau migration"*.

- Đề xuất thứ nhất hỏng theo **hai** cách độc lập: reader không đọc cột, và trigger không cho sửa.
- Đề xuất thứ hai sai tiền đề: row seed mang `environment=NULL` và áp mọi môi trường **theo thiết
  kế**, nên không tồn tại trạng thái "prod không có" để khẳng định.

Cái thay thế được là `IT-GATE-APPROVAL-10`: ghim rằng cột trơ, để không ai tưởng nó là công tắc.

## 5. Vì sao mức nguy hiểm thấp hơn bản audit ngụ ý

`FeatureFlagAdminService:74-79` từ chối **mọi** risk increase ở production, **vô điều kiện**, và
đứng **sau** bước four-eyes. Có thêm approval row cũng không làm HTTP API cho phép tắt kill switch
hay mở allowlist ở prod (Codex F02, tôi verify lại). Không seed `PRODUCTION_CALL`. Và mỗi thay đổi
cụ thể vẫn bị buộc vào đúng môi trường của nó qua fingerprint.

Nên đây là **một cái bẫy tài liệu/schema**, không phải một đường vòng qua gate.

## 6. Kiểm chứng

```text
dotnet test Ivr.sln                          895/895 PASS, 0 failed, 0 skipped
                                             (Unit 594 · Integration 269 · Contract 24 · Chaos 8)
dotnet test --filter IT-GATE-APPROVAL-10     1/1 PASS
contract-freeze-verifier.mjs                 CONTRACT_FREEZE=PASS
generate-test-traceability.mjs --check       TEST_TRACEABILITY_CURRENT=554  (553→554)
docs-selftest.mjs                            API_DOCS_SELFTEST_PASS
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1=DRAFT`.

## 7. Còn lại — Security/Platform

**Câu hỏi:** admin gate có **cần** scope theo môi trường không?

- **Nếu có:** phải sửa **contract + reader + test**, không chỉ đổi giá trị cột. Và phải giữ được
  khả năng giảm rủi ro khẩn cấp một người (`unconditionalRiskReduction`) — thứ `W-0192` đã sửa đúng
  một lần rồi, đừng làm hỏng lại.
- **Nếu không:** ghi thẳng vào `OD-V1-20` rằng cột cố ý trơ cho `RUNTIME_GATE_ADMIN` và
  `PRODUCTION_CALL`, để lần sau không ai mở lại câu hỏi này từ đầu.

Ghi chú: `PRODUCTION_CALL` cùng cảnh ngộ nhưng chưa cấp bách — không migration nào seed nó, và nó là
cổng cuối trước khi một khách hàng thật nghe chuông.
