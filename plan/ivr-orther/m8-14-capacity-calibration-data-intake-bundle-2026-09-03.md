# M8-14 — Capacity calibration unified data-intake bundle

> Work ID: `W-0154`  
> Ngày lập: `2026-09-03`  
> Trạng thái: `LOCAL_INTAKE_BUNDLE_READY / EXTERNAL_DATA_NOT_RECEIVED / CALIBRATION_NOT_RUN / MODEL_UNCALIBRATED / NO_GATE_PROMOTION`

## 1. Mục tiêu và ranh giới

Gói này chuẩn hóa một lần nhận dữ liệu chung để B1 có thể được hiệu chỉnh bằng dữ liệu thật, có provenance và chữ ký phù hợp. Bốn nhóm đầu vào bắt buộc là:

1. `TIMING` — thời gian chiếm dụng và cooldown của từng attempt từ lab thật;
2. `ARRIVAL` — arrival curve đủ chi tiết của các programme mục tiêu;
3. `POLICY_OUTCOME` — attempt policy đã ký và phân phối outcome/retry thực tế;
4. `INFRA_RESERVE` — topology, quota, reserve và bằng chứng failure/failover đa kênh.

Gói này **không**:

- tạo placeholder dưới `docs/evidence/W-0008/`;
- coi template, mock, synthetic hoặc estimate là dữ liệu hiệu chỉnh production;
- thay đổi `40/50/60`, session window, arrival curve, attempt policy, reserve factor hoặc channel count;
- sửa runtime, scheduler, capacity model, config hay production gate;
- suy diễn rằng dữ liệu đã được gửi, nhận, xác minh hoặc phê duyệt.

## 2. Nguồn được khóa SHA-256

| Nguồn | SHA-256 |
|---|---|
| `docs/evidence/W-0142/README.md` | `eeed64d945b71de41dc60443ac7bac0a970a5d379d220a954929df6da112058c` |
| `docs/contracts/telephony-procurement-pack/lab-acceptance-report-template.md` | `5b7ab1e0b1a796f7c1e0bb8643fefb313a76afa626a6200bf17519e306c2dbaa` |
| `docs/contracts/telephony-procurement-pack/R-03-esim32-package.md` | `e4129c3f8daa72ce7c1db7ce925f2bdd56afcae6b6cb02a2f13a1b72313dbf66` |
| `plan/ivr-orther/m8-11-attempt-policy-production-decision-pack-2026-09-03.md` | `6dcf7516ba4af0f2746eacb8240618d19a4bf4828aba90abd89e3a8b6a8640a1` |
| `docs/contracts/target-v1-closure-pack/T-09-attempt-policy.md` | `f9be81414aa6aa66e2fb401e422b389081fd09528c2e179477ea5133647e2945` |
| `docs/capacity-model.md` | `d96a2a7177dba508274625b181f30fb722f7f1ebe84be1f1edbe1fdfb017ef81` |
| `docs/review/2026-08-28-capacity-volume-unit-and-session-length.md` | `9ee9e880ce8d8b5ca2d75f104c1f7a3e3316590c9dfaa42a19a276ea9b5dc893` |

Mọi submission phải ghi rõ SHA-256 của artifact giao nhận. Nếu một nguồn khóa ở trên đổi byte, người nhận phải dừng và đối chiếu lại trước khi dùng.

## 3. Envelope chung cho mọi submission

Mỗi nhóm dữ liệu phải có một manifest đi kèm với tối thiểu các trường:

| Trường | Yêu cầu |
|---|---|
| `submission_id` | ID duy nhất, bất biến |
| `data_group` | Một trong `TIMING`, `ARRIVAL`, `POLICY_OUTCOME`, `INFRA_RESERVE` |
| `artifact_path` | Tên/path artifact được bàn giao |
| `artifact_sha256` | SHA-256 của đúng byte artifact |
| `schema_version` | Phiên bản schema được dùng |
| `source_system` | Hệ thống hoặc thiết bị tạo dữ liệu |
| `source_version` | Version/query/firmware tương ứng |
| `observation_start_utc`, `observation_end_utc` | Khoảng quan sát UTC |
| `timezone_context` | Múi giờ nghiệp vụ nếu có |
| `record_count` | Số record sau filter |
| `filtering_rule` | Quy tắc include/exclude, không để trống |
| `pii_statement` | Xác nhận dữ liệu đã loại PII/secret/token |
| `signer_identity` | Danh tính người ký/attest |
| `signer_role`, `signer_org` | Vai trò và tổ chức có thẩm quyền |
| `authority_source` | Nguồn chứng minh quyền ký/quyền cung cấp |
| `signed_at` | Thời điểm ký có timezone |
| `limitations` | Thiếu sót, bias, known gaps; ghi `NONE` nếu không có |

Không chấp nhận manifest thiếu hash, thiếu observation window, thiếu filter hoặc không xác định được nguồn và thẩm quyền.

## 4. Nhóm A — `TIMING`

### 4.1 Row schema

```text
run_label,attempt_label,programme,execution_mode,carrier_label,scenario,disposition,started_at_utc,ended_at_utc,available_again_at_utc,occupancy_ms,cooldown_ms,full_cycle_ms,cdr_correlation_ref,gateway_model,firmware_version,codec_profile
```

### 4.2 Quy tắc chấp nhận

- `execution_mode` cho hiệu chỉnh production phải là `LAB_REAL_SIM`; mock/simulator chỉ được giữ ở tập riêng và không được trộn.
- `carrier_label` và `cdr_correlation_ref` chỉ dùng alias/correlation ID; không chứa MSISDN, customer ID, secret hoặc dial token.
- `ended_at_utc >= started_at_utc`; `available_again_at_utc >= ended_at_utc`.
- `occupancy_ms = ended_at_utc - started_at_utc`.
- `cooldown_ms = available_again_at_utc - ended_at_utc`.
- `full_cycle_ms = occupancy_ms + cooldown_ms`.
- Mỗi attempt phải truy ngược được tới CDR/log thiết bị thông qua `cdr_correlation_ref` mà không lộ PII.
- Báo cáo phải có `N`, `p50`, `p95`, `p99` theo programme/scenario/disposition và mô tả outlier policy.
- Nếu sample không đủ để bảo vệ percentile, ghi `INSUFFICIENT_SAMPLE`; không nội suy thành số production.

## 5. Nhóm B — `ARRIVAL`

### 5.1 Row schema

```text
dataset_id,programme,session_definition_id,business_timezone,bucket_start_utc,bucket_end_utc,eligible_order_count,source_query_version,eligibility_filter_version,data_quality_flag
```

### 5.2 Quy tắc chấp nhận

- `programme` phải thuộc tập programme hiện hành và đối chiếu được với owner.
- `session_definition_id`, business timezone, business calendar và effective dates phải có quyết định đã ký.
- Không mặc định `45m/2700s` hoặc bất kỳ session window nào khi owner chưa quyết định.
- Bucket phải liên tục, không overlap, không âm và đủ chi tiết để tính mọi rolling window `5m` của Golden Hour và `15m` của 24/7.
- Chỉ có tổng theo ngày/ca không đủ để hiệu chỉnh peak arrival.
- Dữ liệu phải aggregate và PII-safe; không nhận order-level PII nếu không cần thiết.
- Kèm query/hash, filter version và reconciliation tổng số record trước/sau filter.

## 6. Nhóm C — `POLICY_OUTCOME`

### 6.1 Production policy row

```text
policy_version,programme,execution_mode,max_customer_attempts,offsets_seconds,confirmation_window_seconds,effective_from_utc,retire_at_utc,bundle_sha256,product_signer,order_core_signer,m3_producer_version
```

### 6.2 Outcome distribution row

```text
dataset_id,programme,policy_version,attempt_ordinal,normalized_disposition,outcome_count,total_valid_attempts,observation_start_utc,observation_end_utc,retry_eligible,technical_retry_classification,data_quality_flag
```

### 6.3 Quy tắc chấp nhận

- Tổng `outcome_count` theo lát cắt phải khớp `total_valid_attempts`.
- Không được ẩn `UNKNOWN`, raw/unmapped disposition hoặc excluded rows; phải reconciliation riêng.
- Customer attempt và technical retry phải tách riêng, có classification rule rõ ràng.
- `policy_version` phải khớp bundle đã ký và producer version thực tế của M3.
- Dữ liệu phải đủ để tái dựng số retry phát sinh trong từng rolling window cần tính.
- `mock-lab-v1`, synthetic outcome hoặc policy chưa ký không được dùng làm input production.

## 7. Nhóm D — `INFRA_RESERVE`

### 7.1 Topology/reserve row

```text
submission_id,topology_version,vendor_model,firmware_version,carrier_scope,tested_channel_count,per_channel_concurrency,account_quota,reserve_factor,reserve_rationale,quarantine_policy_ref,failover_policy_ref,test_report_sha256,observation_start_utc,observation_end_utc
```

### 7.2 Failure scenario row

```text
scenario_id,available_channels,quarantined_channels,failed_provider_or_gateway,offered_attempts,completed_attempts,deadline_expired_attempts,recovery_seconds,result,evidence_ref
```

### 7.3 Quy tắc chấp nhận

- `reserve_factor` phải có nguồn, rationale và người có thẩm quyền xác nhận; không dùng hệ số ước đoán không provenance.
- Không được suy rộng tuyến tính từ một SIM/một kênh thành năng lực đa kênh.
- Bằng chứng phải khóa đúng topology, model, firmware, carrier scope và quota/account được thử.
- Phải mô tả provisioning, quarantine, failure injection/failure observation, recovery và điều kiện failover.
- Test report và mọi evidence ref phải có SHA-256 và không chứa credential/secret.

## 8. Intake ledger hiện tại

| Nhóm | Owner/custodian cần trả | Submission | Schema check | Provenance/signature | Calibration use |
|---|---|---|---|---|---|
| `TIMING` | Lab operator + witness + Telephony | `NOT_RECEIVED` | `NOT_RUN` | `NOT_RUN` | `BLOCKED_EXTERNAL` |
| `ARRIVAL` | Business/M3 data owner | `NOT_RECEIVED` | `NOT_RUN` | `NOT_RUN` | `BLOCKED_EXTERNAL` |
| `POLICY_OUTCOME` | Product + Order Core + M3 | `NOT_RECEIVED` | `NOT_RUN` | `NOT_RUN` | `BLOCKED_EXTERNAL` |
| `INFRA_RESERVE` | Infra/Platform + Telephony | `NOT_RECEIVED` | `NOT_RUN` | `NOT_RUN` | `BLOCKED_EXTERNAL` |

Chỉ chuyển sang hiệu chỉnh khi đủ `4/4` nhóm và từng nhóm đều qua schema, provenance, hash, PII và authority checks.

## 9. Trình tự hiệu chỉnh sau khi đủ dữ liệu

1. Freeze bốn submission theo exact SHA-256 và lập immutable manifest.
2. Validate schema, completeness, time window, reconciliation, PII và signer authority.
3. Tính timing distribution từ nhóm A; không thay số nếu sample không đạt.
4. Tính rolling arrival peaks theo session/window đã ký từ nhóm B.
5. Áp attempt/outcome distribution đã ký từ nhóm C để suy ra offered attempts.
6. Áp quota, reserve và failure constraints đã chứng minh từ nhóm D.
7. Chạy capacity model trên snapshot cố định; lưu input/output/hash và sensitivity analysis.
8. Review kết quả với Product, Order Core, M3, Infra/Platform và Telephony.
9. Chỉ khi approval artifact hợp lệ tồn tại mới cập nhật calibrated evidence/gate; local PASS không thay approval.

## 10. D-06 — Nội dung yêu cầu dữ liệu có thể gửi nguyên văn

**Subject:** `[M8/B1][D-06] Yêu cầu 4/4 bộ dữ liệu có hash để hiệu chỉnh capacity production`

**Recipients:** Business/M3 data owner; Product; Order Core; M3 producer owner; Infra/Platform; Telephony; lab operator và witness.

**Message:**

> Đề nghị cung cấp đúng phần dữ liệu thuộc quyền sở hữu của anh/chị theo `M8-14 — Capacity calibration unified data-intake bundle`, Work ID `W-0154`. Bốn nhóm bắt buộc là: (A) timing attempt từ lab real-SIM; (B) arrival buckets đủ tính rolling 5m/15m với session definition đã ký; (C) production attempt policy đã ký cùng outcome/retry distribution thực tế; (D) topology/quota/reserve và failure/failover evidence đa kênh.  
>  
> Mỗi artifact phải kèm manifest có exact SHA-256, source/version, observation window UTC, record count, filter, PII statement, limitations, signer identity/role/org, authority source và signed_at. Không gửi credential, secret, dial token, MSISDN hay customer-level PII.  
>  
> Vui lòng trả artifact, manifest và signer/authority evidence vào kênh được phê duyệt; ghi rõ nhóm dữ liệu và `submission_id`. Template, mock, synthetic, tổng theo ngày hoặc số không provenance sẽ không được dùng để hiệu chỉnh production.  
>  
> IVR sẽ giữ `MODEL_UNCALIBRATED`, `CALIBRATION_NOT_RUN` và không thay đổi capacity/policy/channel count cho tới khi đủ 4/4 nhóm, validate thành công và có các approval bắt buộc.

## 11. Stop rule và trạng thái bàn giao

- Không tạo dữ liệu giả để lấp khoảng trống.
- Không tạo `docs/evidence/W-0008/` trước khi có measured/signed evidence thật.
- Không đổi model, `40/50/60`, session definition, arrival input, policy, reserve hoặc channel count ở bước intake.
- Không promote gate chỉ vì schema/template hoặc local self-test PASS.
- Nếu một nhóm chưa nhận hoặc không đạt validation, trạng thái vẫn là `BLOCKED_EXTERNAL / MODEL_UNCALIBRATED`.

Trạng thái hiện tại: `LOCAL_INTAKE_BUNDLE_READY`; cả bốn nhóm vẫn `NOT_RECEIVED`; chưa chạy calibration; chưa có approval production.

## 12. Bước tiếp theo

Gửi D-06 qua kênh được phê duyệt, thu đủ bốn submission PII-safe có exact hash, rồi mới freeze input snapshot và mở lượt validation/calibration B1.
