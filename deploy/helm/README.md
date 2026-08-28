# Helm chart — `W-0044` · `P7-2`

## 1. Ladder ánh xạ sang môi trường

| values | `environmentName` | `IVR_EXECUTION_MODE` | `REAL_CUSTOMER_CALL_ALLOWED` | Ghi chú |
| --- | --- | --- | --- | --- |
| `values-dev.yaml` | `dev` | `MOCK` | `false` | đáy ladder |
| `values-staging.yaml` | `staging` | `MOCK` | `false` | nơi diễn tập tải, **không** phải nơi mở mode |
| `values-lab.yaml` | `lab` | `MOCK` | `false` | env được **dựng cho** `LAB_REAL_SIM`, nhưng chưa có gateway (`W-0008`) nên chưa có gì để quay số |
| `values-prod.yaml` | `prod` | `MOCK` | `false` | vẫn `false` cho tới khi có sign-off DF-03 |

**Không file nào trong repo đặt `realCustomerCallAllowed: true`.** Đó không phải sơ ý.

## 2. Ba lằn ranh được ép lúc render, không phải lúc review

Guard nằm trong `_helpers.tpl` và làm `helm template` **hỏng** — chỗ sớm nhất có thể chặn, và là
chỗ duy nhất không thể bị bỏ qua bởi một người đang vội:

| Vi phạm | Thông báo |
| --- | --- |
| `realCustomerCallAllowed=true` ngoài lab/prod | `Only lab and prod may ever carry it, and only after a DF-03 sign-off` |
| `LAB_REAL_SIM` mà allowlist rỗng | `a lab run that may dial anything is not a lab run` |
| mode khác `MOCK` mà tắt kill switch | `requires the kill switch to remain enabled` |

Cả ba đều có kiểm âm trong `k8s-selftest.mjs`: script **cố ý phá** từng luật và đòi render hỏng
đúng lý do. Một luật ladder chưa ai thử phá là một lời bình luận.

## 3. Cài đặt

```bash
helm upgrade --install ivr deploy/helm/ivr -n ivr-dev --create-namespace -f deploy/helm/ivr/values-dev.yaml
```

Chart **tham chiếu** Secret, không mang Secret (§11). Cluster phải có sẵn `ivr-database` và
`ivr-app-secrets`; `deploy/helm/ivr/ci/bootstrap-dev.yaml` là bản dev-only cho cluster thử.

W-0128 loại `admin-ui` khỏi deployable topology. `ui.enabled=true` bị Helm từ chối: UI trong repo
chỉ là reference local, còn Module 3 sở hữu identity và UI triển khai. `ivr-app-secrets` phải có ba
key current `admin-read-token`, `admin-write-token`, `admin-danger-token`; key `*_PREVIOUS` chỉ được
khai cùng một retirement instant tuyệt đối để overlap tự đóng.

Kiểm toàn bộ như CI:

```bash
node deploy/ci/scripts/k8s-selftest.mjs
```

Script tự dựng một cluster k3s dùng-một-lần, nạp image, triển khai, rồi xoá đi.

## 4. Migration là Helm hook, không phải initContainer

`W-0043` phát hiện **không có gì chạy migration**: trên database trắng, worker chết vì thiếu bảng
và readiness chỉ *trông* như xanh vì volume cũ còn schema. Chart giải bằng Job `pre-install`/
`pre-upgrade`.

Hook chứ không initContainer vì initContainer chạy **trên mỗi replica**: nhiều API replica cùng áp
một migration là cách đã biết để hỏng schema.

## 5. HPA và trần SIM

Worker HPA **tắt mặc định**, và đó là quyết định chứ không phải thiếu sót.

Thông lượng worker bị chặn bởi **pool SIM**, không phải số pod: một SIM một cuộc gọi (DT-04). Scale
vượt pool **không** quay thêm được cuộc nào — advisory lock của scheduler (P2-3) khiến pod thừa
tranh chấp cùng lease. Nên một autoscaler nhìn CPU sẽ thêm pod không làm gì trong khi *trông như*
đã phản ứng với tải, tệ hơn không scale vì nó **giấu mất trần thật**.

Ai bật nó thì `maxReplicas` vẫn bị kẹp ở `simPoolSize` bằng `min` trong template.

## 6. Ba probe, ba câu hỏi khác nhau

| Probe | Hỏi gì | Vì sao không dùng cái khác |
| --- | --- | --- |
| `startupProbe` → `/health/startup` | boot xong chưa | cho migration/warmup thời gian mà không nới liveness |
| `readinessProbe` → `/health/ready` | nhận traffic được chưa | 503 rút pod khỏi rotation **mà không restart** — đúng phản ứng với sự cố dependency |
| `livenessProbe` → `/health/live` | tiến trình còn sống không | **cố ý không** dùng `/health/ready`: liveness phụ thuộc downstream thì một sự cố dependency restart mọi pod cùng lúc |

Worker **không có probe HTTP** vì nó không mở socket nào (`W-0043` §2). Kubernetes restart pod khi
tiến trình thoát là tín hiệu liveness duy nhất tồn tại ở đây.

## 7. NetworkPolicy

Default-deny trước, rồi mới nêu cái được đi ra. Thứ tự đó quan trọng với người đọc: một chính sách
mở lỗ trên nền permissive đọc y hệt một chính sách mở lỗ trên nền đóng, mà chỉ cái thứ hai là least
privilege.

DNS được nêu riêng vì chính sách quên nó sẽ làm hỏng **mọi** luật bên dưới theo cách trông như sai
luật khác: tên ngừng phân giải, và triệu chứng chỉ vào database.

`networkPolicy.egress.external` mặc định **rỗng** — không có egress ngoài nào cho tới khi chủ sở hữu
cung cấp endpoint thật. Không có wildcard, và `k8s-selftest.mjs` đỏ nếu ai thêm `0.0.0.0/0`.

API ingress cũng mặc định rỗng. Platform phải bật `networkPolicy.ingress.module3` với **cả** nhãn
namespace và pod của Module 3 BFF; dev selftest dùng identity giả `module3/bff` để đo positive hop.

## 8. Retention CronJob

`30 2 * * *`, `concurrencyPolicy: Forbid`, và **`dryRun: true` mặc định** (DF-07).

Xoá dữ liệu khách theo lịch là việc mà một mặc định sai **không thể hoàn tác**, nên mặc định báo
cáo cái nó *sẽ* xoá và không xoá gì. Tắt dry-run là quyết định vận hành có phê duyệt, không phải một
chỉnh sửa values.

`Forbid` chứ không `Allow`: hai lượt retention cùng xoá một tập dòng là hình dạng mất dữ liệu, không
phải vấn đề thông lượng.
