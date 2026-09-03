# W-0155 — B1 D-06 four-submission offline validator evidence

> Ngày: `2026-09-03`  
> Trạng thái: `TESTS_PASS / LOCAL_VALIDATOR_READY / EXTERNAL_SUBMISSIONS_NOT_RECEIVED / CALIBRATION_NOT_RUN / MODEL_UNCALIBRATED / NO_GATE_PROMOTION`

> Supersession note: hash validator ở §6 là exact baseline tại handoff W-0155. W-0156 mở rộng cùng
> file bằng receipt mode; current hash và delta được khóa trong `docs/evidence/W-0156/README.md`.

## 1. Kết quả

Đã tạo validator Node standalone:

- `deploy/ci/scripts/capacity-data-intake-validator.mjs`;
- không dùng network, database, secret store hoặc third-party package;
- không sửa runtime, scheduler, capacity arithmetic, attempt policy hay channel configuration;
- không tạo `docs/evidence/W-0008/`;
- bundle bình thường chỉ được chấp nhận khi status là `EXTERNAL_OWNER_ATTESTED`;
- self-test dùng nhánh nội bộ `allowTestOnly`, còn CLI bình thường từ chối `TEST_ONLY` và template
  `PENDING_EXTERNAL_OWNER_DATA`.

Lệnh chạy:

```powershell
node deploy/ci/scripts/capacity-data-intake-validator.mjs --bundle-dir <thu-muc-bundle>
```

Lệnh tự kiểm:

```powershell
node deploy/ci/scripts/capacity-data-intake-validator.mjs --self-test
```

## 2. Các guard được thực thi

### Bundle/envelope

- đúng `m8-capacity-intake-bundle.v1`, `work_id=W-0154` và exact M8-14 SHA-256;
- đúng 4/4 nhóm, không thiếu/trùng submission/group/path;
- artifact path tương đối, không `..`, không symlink, không thoát bundle root, file thường và tối đa
  50 MiB;
- artifact hash lowercase SHA-256 khớp exact bytes;
- provenance bắt buộc: source/version, observation window, timezone, filter, record count,
  signer identity/role/org, authority source, signed_at và limitations;
- `pii_statement=PII_SAFE`; chặn phone/MSISDN, email, địa chỉ dạng street, bearer/private key,
  dial token và các sensitive field name đã liệt kê;
- `record_count` phải khớp số row thật.

### `TIMING`

- đủ cả hai programme, chỉ `LAB_REAL_SIM`;
- timestamp UTC và observation-window containment;
- exact `occupancy_ms`, `cooldown_ms`, `full_cycle_ms` invariant;
- không trùng run/attempt identity và disposition normalized.

### `ARRIVAL`

- đủ cả hai programme;
- bucket UTC liên tục, không gap/overlap, tối đa 5 phút và chia được rolling 5m/15m;
- count không âm, session/query/filter version có provenance, `data_quality_flag=OK`.

### `POLICY_OUTCOME`

- đúng hai policy row production cho `GOLDEN_HOUR` và `TWENTY_FOUR_SEVEN`;
- offsets bắt đầu 0, tăng nghiêm ngặt, đủ attempt và nằm trước window expiry;
- hai programme dùng cùng canonical bundle hash;
- outcome khớp policy version, phủ đủ attempt ordinal và reconcile exact về
  `total_valid_attempts`;
- terminal attempt không được `retry_eligible`.

### `INFRA_RESERVE`

- topology/report provenance, quota, positive finite reserve value và one-SIM/one-active-call
  constraint; validator không tự chọn semantics/range của reserve factor;
- từ hai kênh thật trở lên, không cho suy rộng từ một kênh;
- tối thiểu hai scenario và bắt buộc có quarantine/failure scenario;
- channel/count math, recovery, result và evidence ref được kiểm.

## 3. Template bàn giao

Pending template nằm tại:

- [`templates/README.md`](templates/README.md);
- `templates/bundle-manifest.json`;
- `templates/timing.json`;
- `templates/arrival.json`;
- `templates/policy-outcome.json`;
- `templates/infra-reserve.json`.

Template cố ý fail closed, không phải submission hoặc evidence đã ký.

## 4. Self-test evidence

Kết quả:

```text
CAP-INTAKE-VALID-01 PASS — TEST_ONLY four-group bundle accepted only by self-test path
CAP-INTAKE-MODE-02 PASS — normal acceptance path rejects TEST_ONLY data
CAP-INTAKE-MODE-03 PASS — external status cannot hide TEST_ONLY provenance
CAP-INTAKE-TEMPLATE-04 PASS — pending template is fail-closed
CAP-INTAKE-REFUSAL PASS mutation=missing-group
CAP-INTAKE-REFUSAL PASS mutation=contract-hash
CAP-INTAKE-REFUSAL PASS mutation=artifact-hash
CAP-INTAKE-REFUSAL PASS mutation=path-traversal
CAP-INTAKE-REFUSAL PASS mutation=provenance-placeholder
CAP-INTAKE-REFUSAL PASS mutation=signer-not-alias
CAP-INTAKE-REFUSAL PASS mutation=raw-phone
CAP-INTAKE-REFUSAL PASS mutation=dial-token-field
CAP-INTAKE-REFUSAL PASS mutation=timing-invariant
CAP-INTAKE-REFUSAL PASS mutation=arrival-gap
CAP-INTAKE-REFUSAL PASS mutation=outcome-reconciliation
CAP-INTAKE-REFUSAL PASS mutation=single-channel-extrapolation
CAP-INTAKE-REFUSAL PASS mutation=infra-counts
CAP-INTAKE-REFUSAL PASS mutation=record-count
CAPACITY_DATA_INTAKE_SELFTEST_PASS valid=1 mode_guard=2 template_guard=1 refusals=14 external_submissions=0 calibration=NOT_RUN
```

## 5. Repository verification

| Kiểm tra | Kết quả |
|---|---|
| `node --check` validator | `PASS` |
| Validator self-test | `PASS — valid=1, mode_guard=2, template_guard=1, refusals=14` |
| W-0155 evidence PII scan | `PASS — 7 text files, 0 binary skipped` |
| Existing capacity self-test | `PASS — 6/6, CAPACITY_SELFTEST_PASS_UNCALIBRATED` |
| Docs self-test | `PASS — API_DOCS_GENERATED=14, API_DOCS_SELFTEST_PASS` |
| Traceability | `PASS — TEST_TRACEABILITY_CURRENT=476` |
| Gate mirror | `PASS — 11 gates, 153 work items, 23 open decisions, production=false` |
| Markdown map | `PASS — 641 Markdown files; M8-14, W-0155, template README và target worklist đều 0 unresolved` |
| Git diff whitespace check | `PASS — git diff --check` |
| GitNexus detect-changes advisory | `LOW — aggregate dirty checkout: 42 tracked files, 170 symbols, 0 affected process`; file validator mới chưa tracked nên direct review/self-test là evidence chính |

## 6. SHA-256 manifest

| File | SHA-256 |
|---|---|
| `deploy/ci/scripts/capacity-data-intake-validator.mjs` | `ce928e3be9c746657fd8fdbabd61ceec8077247c7afecb9ea56c6648913ab754` |
| `templates/README.md` | `2f7d206afc1d52fd74a879a36645959c99ef04729745ab5f7d5a502c3942de7f` |
| `templates/bundle-manifest.json` | `82fc519d3c7c559ede8e73651013930987c9393c159a7fb4ac35131daa74dab7` |
| `templates/timing.json` | `3e2ba526d3d469cefbd12799ae7668944e4b7de0f09a3bbceb3d1cb610c52c13` |
| `templates/arrival.json` | `51bb1bdf4d1efb2acfb6455f41d37039e1e95647bbf83f4207eb1a1a486ade77` |
| `templates/policy-outcome.json` | `6f612ecb2448f33f9472fc0bd7eaa4e7ec7ce218ae66d386414b1494d0b1edc4` |
| `templates/infra-reserve.json` | `6cbe4cd2166af151be05230d748d7e07059644425b7be8bb6be270cbd30d24f1` |
| M8-14 source contract | `933c55255c538987d1b86ff6d8f46b6657c68821cd00a232a55827cc751fa879` |

## 7. Giới hạn và non-inference

- Validator chỉ kiểm metadata của signer/authority; nó không xác minh danh tính, quyền ký hoặc chữ
  ký điện tử ngoài đời. Output ghi rõ `authority=METADATA_ONLY_NOT_EXTERNALLY_VERIFIED`.
- Schema/hash/PII PASS không chứng minh dữ liệu đúng nghiệp vụ, sample đủ, vendor/carrier đạt, hay
  capacity production đủ.
- Chưa có submission external nào được nhận; self-test data là synthetic và chỉ tồn tại trong thư
  mục tạm khi chạy test.
- Chưa chạy calibration, load/failover thật hoặc shared E2E.
- `REAL_CUSTOMER_CALL_ALLOWED=NO` và `production=false` giữ nguyên.

## 8. Bước tiếp theo

Module 8 Owner gửi D-06 kèm thư mục template. Khi nhận từng bundle, lưu ngoài repo/secret-safe
channel, chạy validator, ghi exact output/hash và chỉ đưa bundle qua intake ledger nếu cả 4/4 nhóm
PASS. Sau đó mới freeze input snapshot và mở calibration review.
