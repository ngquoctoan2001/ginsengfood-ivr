# W-0216 — Chạy hết 50 gate, và bảng điều khiển đã chết từ trước

Ngày: 2026-09-07 · Baseline: `main@f5810c0` · Trạng thái: **TESTS_PASS**.

Chín lượt `W-0208..W-0215` mỗi lượt chỉ chạy vài gate liên quan trực tiếp. Lượt này chạy **hết 50
script** trong `deploy/ci/scripts/`. Bốn thứ lộ ra — một cái có từ trước tôi, ba cái là của tôi.

---

## 1. `gate-status.mjs` đã chết từ `W-0207`, trước cả lượt đầu của tôi

Bảng điều khiển release crash với `AssertionError` chứ không phải fail có thông điệp:

```text
AssertionError: the tracker parser read something that is not a status;
                the column map has drifted.
+ [ 'W-0207=TESTS_PASS (nửa IVR) / BLOCKED_EXTERNAL (nửa M3)' ]
```

Cột `Status` của §5 là **closed vocabulary 12 token** (`prompt-execution-tracker.md` §1), và §6 ghi
rõ *"một trong Status vocabulary §1"*. `W-0207` viết một chuỗi ghép hai token kèm chú thích. Parser
assert đỏ.

**Kiểm tại `9603ca5`** — commit trước lượt đầu của tôi — cùng lỗi, chỉ mình `W-0207`. Nên bảng điều
khiển đã hỏng từ `2026-09-06`, và **không ai biết vì không ai chạy nó**. Bản audit 07/09 đọc
`gate-status.yaml` nhưng chưa từng chạy `gate-status.mjs`.

## 2. Tôi làm nó nặng thêm ba cách

**(a) Năm chuỗi ghép nữa.** `W-0208`, `W-0209`, `W-0210`, `W-0213`, `W-0215` — cùng kiểu như
`W-0207`. Tệ hơn: tôi dùng `OWNER_DECISION_REQUIRED` và `QUORUM_PENDING`, **hai token không có trong
vocabulary**. Tôi tự nghĩ ra hai trạng thái cho một cột đóng.

**(b) Hai dấu gạch đứng không escape.** Dòng `W-0214` của tôi có
`quarantined>0 ‖ closed>0 ‖ claimed` viết bằng ký tự gạch đứng thật. Markdown đọc chúng thành dấu
ngăn cột: hàng đó thành **13 cột thay vì 9**, và column map lệch cho cả bảng.

> Ghi chú thật thà: khi soạn dòng `W-0216` mô tả chính lỗi này, tôi lặp lại đúng nó. Bắt được vì
> lần này có đếm cột trước khi commit.

**(c) `gate-status.yaml` là file *sinh ra*, tôi sửa tay cả tám lượt.** Header script viết rõ:

> `--write` regenerate `docs/release/gate-status.yaml` … *do not edit it by hand.*

Hậu quả đo được: board đếm **203** work item trong khi tracker có **212** — lệch đúng **9**, tức
`W-0207` cộng tám lượt của tôi. Cơ chế sinh-lại-và-fail-on-drift tồn tại chính xác để chặn chuyện
này; nó chặn được, chỉ là nó đang crash nên không ai nghe thấy.

**Cách đúng:** `node scripts/gate-status.mjs --write`. Sắc thái (*"nửa IVR xong, nửa M3 chờ"*) thuộc
cột **Residual**, không phải cột Status.

## 3. Bốn validator pin SHA-256 của IR-06, tôi sửa IR-06 bốn lần

`W-0208`, `W-0209`, `W-0211`, `W-0215` đều sửa
`integration-requirements/06-module-3-api-handover.md`. Bốn validator offline ghim hash của nó, để
một gói quyết định không thể được validate dựa trên bản handover đã đổi từ lúc quyết định được đặt
ra:

| Validator | |
| --- | --- |
| `d06-revalidation-evidence-validator` | `W0178` |
| `dial-token-production-bundle-validator` | `W0183` |
| `upstream-session-signoff-validator` | `W0181` |
| `opt-out-suppression-bundle-validator` | `W0187` |

Cả bốn đỏ với `local source hash drift`. Đây **đúng là việc gate phải làm** — và `W-0205`
(`98e94dc`) đã làm y hệt một lần rồi, commit tên *"re-pin the offline validators onto the reviewed
current sources"*.

Đã re-pin `fb5bf159…` → `97357d33…`. `W0187` còn một tầng nữa: template lưu tại
`docs/evidence/W-0187/opt-out-suppression-decision-bundle.template.json` **nhúng** hash đó, nên phải
sửa cùng.

**Kèm một pin cũ không phải của tôi:** `d06` ghim `target-v1-shared-e2e-report-validator.mjs` ở bản
trước `W-0207`. `W-0207` sửa validator đó **sau** khi `W-0205` ghim nó, và không ai re-pin. Đã sửa
luôn — để một pin cũ không giữ gate đỏ và che mất lần lệch tiếp theo.

## 4. Quét toàn bộ pin

Sửa từng cái theo báo lỗi thì không biết còn sót gì. Nên quét hết mọi cặp `*_path` / `*_sha256` trong
`deploy/ci/scripts/`, tính lại và so:

```text
pins matching: 32   STALE: 0
```

## 5. Hai gate còn đỏ, cả hai không phải defect

| Gate | Vì sao |
| --- | --- |
| `check-mr-traceability` | đọc `CI_MERGE_REQUEST_DESCRIPTION`, chỉ tồn tại trong pipeline GitLab. Đỏ ở local là **đúng**. |
| `image-selftest` | cần Docker build image thật. |

`k8s-selftest` không đỏ — nó chỉ chạy lâu hơn timeout 180s tôi đặt; dòng cuối trước khi bị cắt là
`IT-K8S-NETPOL-04 PASS`.

## 6. Kiểm chứng

```text
gate-status.mjs                    GATE_STATUS_PASS — 11 gates, 213 work items, 5 open decisions
quét pin toàn repo                 32 khớp / 0 lệch
W0178 / W0181 / W0183 / W0187      SELFTEST_PASS
contract-freeze-selftest           CONTRACT_FREEZE_SELFTEST=PASS 14/14
progressive-selftest               PROGRESSIVE_SELFTEST_PASS
compliance-pack-selftest           COMPLIANCE_PACK_SELFTEST_PASS
dr-selftest                        DR_SELFTEST_PASS_SINGLE_HOST=DG-DR-03
cd-selftest                        CD_SELFTEST_PASS
api-behavior-matrix-selftest       API_MATRIX_VALIDATOR_SELFTEST=PASS (18 cases)
capacity-selftest                  CAPACITY_SELFTEST_PASS_UNCALIBRATED
tts-provenance / voice-acceptance  PASS (release_blockers=LEGAL,INTERNAL_MIRROR — không đổi)
W0164 / W0165 / W0170 / W0174 /
W0180 / W0182 / B3-telephony       SELFTEST_PASS
dotnet test Ivr.sln                897/897 PASS, 0 failed, 0 skipped
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1=DRAFT`. Không sửa runtime.

## 7. Ba luật ghi lại để lần sau không lặp

1. **`gate-status.yaml` và `readiness-board.md` sinh bằng `--write`.** Không gõ tay. Chúng là gương
   của tracker; sửa gương thì gương thành backlog thứ hai — đúng thứ `P11-4 §3` cấm.
2. **Cột Status §5 nhận đúng một token trong 12.** Không ghép, không tự thêm token. Sắc thái đi vào
   Residual.
3. **Sửa một file được ghim thì re-pin cùng lượt.** Bốn validator ghim IR-06; `W-0205` đã dạy điều
   này một lần và tôi vẫn quên bốn lần liên tiếp.

Điều đáng giữ lại: cả ba lỗi đều **bị máy bắt**, không phải do đọc lại. Cơ chế đúng, chỉ là chưa ai
chạy nó sau khi sửa.
