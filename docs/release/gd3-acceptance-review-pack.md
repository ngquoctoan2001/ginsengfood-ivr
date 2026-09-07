# GĐ 3 — phiếu rà soát nghiệm thu `EVIDENCE_SUBMITTED`

> **Phiếu này không mang trạng thái.** Nguồn trạng thái duy nhất vẫn là
> `prompt/_execution/prompt-execution-tracker.md`; readiness board là gương của tracker, và đây là
> phiếu rà soát để Release owner quyết định — không phải backlog thứ ba.
>
> `MASTER-05`: **evidence đã nộp không phải evidence đã được chấp nhận. Chỉ Release owner chuyển
> sang `ACCEPTED`.** Không agent nào tự chuyển, kể cả khi mọi cổng đều xanh.

## 1. Phiếu này trả lời cái gì

Điều kiện vào Nấc 1 đòi **mọi prompt đã lên kế hoạch có bằng chứng `ACCEPTED`**. Hiện `8/203` ở
`ACCEPTED`, và nhóm gần nhất là 35 dòng `EVIDENCE_SUBMITTED` — bằng chứng đã có, đã nộp, chỉ còn
thiếu chữ ký. GĐ 3 dựng đúng cái nền để đọc 35 dòng đó: một SHA, một bộ bằng chứng, chạy lại được.

Phiếu **không** khẳng định 35 dòng này xứng đáng `ACCEPTED`. Nó nói: tính đến SHA dưới đây, các
cổng tự động bảo vệ chúng đều xanh, nên cái còn lại để quyết là **nội dung**, không phải hạ tầng.

## 2. Nền đo, tính đến `98e94dc`

Chạy trên một clone sạch tại đúng SHA này (`git clone --local`, 0 file bẩn):

| Hạng mục | Kết quả |
| --- | --- |
| `dotnet restore --locked-mode` | sạch |
| Build Release | `0 warning / 0 error` |
| Unit / Contract / Chaos / Integration | xem §4 |
| `security-scan.sh` | `SECURITY_SCAN_PASS gitleaks=8.30.0 nuget=HIGH npm=HIGH` |
| Negative control `CT-CI-04` | PASS — PAT giả vẫn bị bắt |
| Gitleaks toàn lịch sử | `no leaks found` |
| 12 validator offline | 12/12 PASS |
| `gate-status` | `GATE_STATUS_PASS` |

**Chưa có:** pipeline GitLab hosted xanh trên chính SHA này. Code đã đẩy; URL pipeline cần
credential mà môi trường này không có (§5).

## 3. 35 dòng cần quyết

Cột "Bằng chứng" chỉ nói bằng chứng **nằm ở đâu**, không nói nó đủ hay không — đó là phần của owner.

| Work ID | Phạm vi | Bằng chứng |
| --- | --- | --- |
| `W-0001` | Planning realignment | bản ghi hoàn thành trong tracker (§8) |
| `W-0057` | `P11-1` | [`docs/evidence/W-0057/`](../evidence/W-0057/) |
| `W-0058` | `P11-2` | [`docs/evidence/W-0058/`](../evidence/W-0058/) |
| `W-0059` | `P11-3` | [`docs/evidence/W-0059/`](../evidence/W-0059/) |
| `W-0060` | `P11-4` | [`docs/evidence/W-0060/`](../evidence/W-0060/) |
| `W-0062` | Red-team remediation | bản ghi hoàn thành trong tracker (§8) |
| `W-0067` | Fix: PII regex control char | bản ghi hoàn thành trong tracker (§8) |
| `W-0068` | Fix: kill-switch immutability | bản ghi hoàn thành trong tracker (§8) |
| `W-0069` | Fix: P2-1 ↔ P2-7 dependency | bản ghi hoàn thành trong tracker (§8) |
| `W-0070` | Fix: nối W-0064/65/66 downstream | bản ghi hoàn thành trong tracker (§8) |
| `W-0071` | Fix: cross-table CHECK | bản ghi hoàn thành trong tracker (§8) |
| `W-0072` | Fix: `order_state` + policy `delivery_area_short` | bản ghi hoàn thành trong tracker (§8) |
| `W-0073` | Fix: PII gate case + artifact topology | bản ghi hoàn thành trong tracker (§8) |
| `W-0074` | Fix: dependency wildcard sweep | bản ghi hoàn thành trong tracker (§8) |
| `W-0075` | Doc-map regeneration | bản ghi hoàn thành trong tracker (§8) |
| `W-0076` | Fix: PII pattern locale-independence | bản ghi hoàn thành trong tracker (§8) |
| `W-0077` | Fix: direct dependency Meta/tracker drift | bản ghi hoàn thành trong tracker (§8) |
| `W-0093` | Phase 1/2 governance and evidence truth remediation | [`docs/evidence/W-0093/`](../evidence/W-0093/) |
| `W-0102` | Phase 3 §10 live evidence capture (Origin=`UNPLANNED`) | [`docs/evidence/W-0102/`](../evidence/W-0102/) |
| `W-0133` | Escalation Owner: đơn vị volume + độ dài phiên Giờ Vàng (Origin=`UNPLANNED`, B1 sub-task 2) | [`docs/evidence/W-0133/`](../evidence/W-0133/) |
| `W-0142` | M8-01 capacity calibration preflight và data-intake handoff (Origin=`UNPLANNED`, owner yêu cầu tiếp tục 2026-08-29) | [`docs/evidence/W-0142/`](../evidence/W-0142/) |
| `W-0143` | M8-03 admin audit/capacity surface reconciliation và M3 handoff (Origin=`UNPLANNED`, owner yêu cầu tiếp tục 2026-08-29) | [`docs/evidence/W-0143/`](../evidence/W-0143/) |
| `W-0145` | M8-05 program/result contract sign-off và factual reconciliation (Origin=`UNPLANNED`, owner yêu cầu tiếp tục 2026-09-03) | [`docs/evidence/W-0145/`](../evidence/W-0145/) |
| `W-0146` | M8-06 upstream session trace contract reconciliation và sign-off handoff (Origin=`UNPLANNED`, owner yêu cầu tiếp tục 2026-09-03) | [`docs/evidence/W-0146/`](../evidence/W-0146/) |
| `W-0147` | M8-07 Target V1 shared callback readiness audit và external handoff (Origin=`UNPLANNED`, owner yêu cầu tiếp tục 2026-09-03) | [`docs/evidence/W-0147/`](../evidence/W-0147/) |
| `W-0148` | M8-08 opt-out feedback-loop contract reconciliation và decision handoff (Origin=`UNPLANNED`, owner yêu cầu tiếp tục 2026-09-03) | [`docs/evidence/W-0148/`](../evidence/W-0148/) |
| `W-0149` | M8-09 revoke/recall/freshness contract reconciliation và decision handoff (Origin=`UNPLANNED`, owner yêu cầu tiếp tục 2026-09-03) | [`docs/evidence/W-0149/`](../evidence/W-0149/) |
| `W-0150` | M8-10 contact/dial-token production-path contract reconciliation và decision handoff (Origin=`UNPLANNED`, owner yêu cầu 2026-09-03) | [`docs/evidence/W-0150/`](../evidence/W-0150/) |
| `W-0151` | M8-11 attempt-policy production contract reconciliation và decision handoff (Origin=`UNPLANNED`, owner yêu cầu 2026-09-03) | [`docs/evidence/W-0151/`](../evidence/W-0151/) |
| `W-0152` | C5 external-decision provenance và dispatch-ready consolidation (Origin=`UNPLANNED`, owner yêu cầu tiếp tục việc tự làm được 2026-09-03) | [`docs/evidence/W-0152/`](../evidence/W-0152/) |
| `W-0153` | C5 external decision copy/paste dispatch-message kit (Origin=`UNPLANNED`, owner yêu cầu tiếp tục 2026-09-03) | [`docs/evidence/W-0153/`](../evidence/W-0153/) |
| `W-0154` | B1 capacity calibration unified data-intake bundle và D-06 handoff (Origin=`UNPLANNED`, owner yêu cầu tiếp tục 2026-09-03) | [`docs/evidence/W-0154/`](../evidence/W-0154/) |
| `W-0160` | B1 monotonic ledger-checkpoint registry contract (Origin=`UNPLANNED`, owner yêu cầu 2026-09-03) | [`docs/evidence/W-0160/`](../evidence/W-0160/) |
| `W-0166` | Reconcile current-state wording and counters in the 03/09 Module 8 worklist (Origin=`UNPLANNED`, owner yêu cầu tiếp tục 2026-09-03) | [`docs/evidence/W-0166/`](../evidence/W-0166/) |
| `W-0175` | R0 remote-head, hosted-CI và clean-checkout reproducibility audit (Origin=`UNPLANNED`, owner yêu cầu tiếp tục R0 2026-09-04) | [`docs/evidence/W-0175/`](../evidence/W-0175/) |
## 4. Kết quả suite trên `98e94dc`

| Bộ | Kết quả |
| --- | --- |
| `Ivr.UnitTests` | `587/587` |
| `Ivr.ContractTests` | `24/24` |
| `Ivr.ChaosTests` | `8/8` |
| `Ivr.IntegrationTests` | `266/266` |

> Clone sạch **phải** chạy `npm --prefix deploy/ci ci` trước khi test. Không có nó,
> `ApiBehaviorMatrixTests` đỏ vì `api-behavior-matrix.mjs` không import được `yaml` — nó **fail chứ
> không tự skip**, đúng như evidence `W-0197` tuyên bố. Đó là hành vi đúng, không phải lỗi.

## 5. Cái phiếu này không mua được

**Pipeline GitLab hosted xanh trên `98e94dc`.** Code đã đẩy tới cả `origin` (GitLab) và `github`.
Phần còn thiếu là **đọc kết quả**: API trả `404 Project Not Found` khi gọi ẩn danh vì project riêng
tư, và môi trường này không có `GITLAB_TOKEN`, không có `glab`. Credential đẩy code nằm trong
Windows Credential Manager và **không được rút ra để dùng làm token API** — xử lý token dạng thô
không nằm trong việc tôi làm.

Hai lối cho owner:
1. Mở `https://gitlab.com/nqt20102001/ginsengfood-ivr/-/commit/98e94dc/pipelines` và dán URL job.
2. Đặt `GITLAB_TOKEN` (scope `read_api`) vào môi trường; khi đó agent đọc được trạng thái pipeline
   qua biến môi trường mà không bao giờ thấy giá trị.

Đây đúng là thứ `W-0121` chờ từ đầu: `W-0121` ghi "lối đẩy code không tới GitLab nên hosted CI không
thể chạy". **Lối đẩy đã thông.** Cái chưa có là bằng chứng pipeline, không phải khả năng chạy.

## 6. Vì sao 174 dòng không tự chuyển được

`MASTER-05` và readiness board §2 nói cùng một câu: chỉ Release owner chuyển `ACCEPTED`. Mọi bản ghi
hoàn thành trong tracker đều ghi `Review/acceptance by: Claude self-review ... status limited to
TESTS_PASS`. Chuyển 174 dòng từ phía agent sẽ đặt vào bảng một sự chấp thuận **chưa từng có** — và
một readiness board nói sai đúng chỗ đó thì tệ hơn là không có bảng nào.

Nên điều kiện vào Nấc 1 hiện đứng ở:

| Vế | Trạng thái |
| --- | --- |
| không còn `BLOCKED_INTERNAL` | ✅ `0` |
| mọi prompt có bằng chứng `ACCEPTED` | ❌ `8/203` |
