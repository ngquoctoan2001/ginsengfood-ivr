# W-0044 — Evidence: Kubernetes & Helm (`P7-2`)

Ngày: `2026-08-18` · Cập nhật `2026-08-19` · Trạng thái: **5/5 test §8 `PASS`**.
`IT-K8S-NETPOL-04` không còn `NOT_PROVEN` — xem §5, và **kết luận cũ ở đó là sai**.
`IT-K8S-RETENTION-05` đã đóng ở `W-0047` — xem §7

Tài liệu chart: [`deploy/helm/README.md`](../../../deploy/helm/README.md).

## 1. Chart được kiểm bằng cách **triển khai thật**, không chỉ render

Render một chart chứng minh YAML parse được. Nó **không** chứng minh pod khởi động, readiness rút
pod khỏi rotation, hay policy chặn được gì — và cả ba đều hỏng vì lý do riêng ở lần chạy đầu.

Nên `k8s-selftest.mjs` dựng một cluster k3s dùng-một-lần, nạp 4 image, triển khai chart, chạy kiểm,
rồi xoá đi.

## 2. Ba defect thật — chỉ cluster sống mới lộ

### 2.1 `$(IVR_DB_PASSWORD)` tới pod dưới dạng **văn bản nguyên si**

Kubernetes chỉ expand `$(VAR)` trong giá trị env với biến khai báo **trước đó trong cùng danh
sách**; biến khai báo sau vẫn là chữ. Tôi đặt connection string trước, nên mọi pod nhận:

```
Password=$(IVR_DB_PASSWORD)
```

và fail `28P01 password authentication failed`. Triệu chứng đọc như "database từ chối credential"
— chỉ vào bí mật, không chỉ vào thứ tự. API cũng 503 vì cùng lý do.

Sửa: đưa `IVR_DB_PASSWORD` lên **trước** connection string, kèm comment giải thích, vì người sửa
tiếp theo sẽ thấy hai biến và không có lý do gì để đoán thứ tự quan trọng.

### 2.2 `USER node` làm `runAsNonRoot` không dùng được

Kubernetes từ chối container:

```
image has non-numeric user (node), cannot verify user is non-root
```

Với securityContext siết, một USER **dạng tên** nghĩa là pod **không khởi động được**. Sửa image
console thành `USER 1000` — cùng UID, nhưng portable. `W-0043` đã kiểm non-root và vẫn xanh, vì
`docker inspect` chấp nhận tên; chỉ Kubernetes mới đòi số.

### 2.3 Migration (nối tiếp `W-0043`)

Chart giải bằng Job `pre-install`/`pre-upgrade` hook. Hook chứ không initContainer: initContainer
chạy **trên mỗi replica**, và nhiều API replica cùng áp một migration là cách đã biết để hỏng
schema. Trong cluster, Job vào trạng thái `Complete` trước khi deployment lên.

## 3. Ladder được ép lúc render, và được kiểm bằng cách **cố phá**

Guard nằm trong `_helpers.tpl` và làm `helm template` hỏng — chỗ sớm nhất có thể chặn, và chỗ duy
nhất một người đang vội không bỏ qua được.

| Vi phạm dựng lên | Kết quả |
| --- | --- |
| `realCustomerCallAllowed=true` ở `dev` | ❌ `Only pilot and prod may ever carry it, and only after a DF-03 sign-off` |
| `LAB_REAL_SIM` với allowlist rỗng | ❌ `a lab run that may dial anything is not a lab run` |
| `PRODUCTION_REAL` mà tắt kill switch | ❌ `requires the kill switch to remain enabled` |

Cả bốn env như đang ship đều render `REAL_CUSTOMER_CALL_ALLOWED="NO"` và
`IVR_EXECUTION_MODE="MOCK"`, kể cả `prod`.

## 4. `IT-K8S-GATE-02` đọc từ **pod đang chạy**, đúng như §8 đòi

§8 nói rõ: đọc effective config của pod đang chạy, **không** đọc values file. Yêu cầu đó hoá ra
không thừa chút nào — §2.1 là đúng trường hợp values file **đúng** trong khi pod chạy thứ khác.

Kiểm trên pod `api` và `worker` đang chạy: `IVR_EXECUTION_MODE=MOCK`, `REAL_CUSTOMER_CALL_ALLOWED=NO`,
`IVR_KILL_SWITCH_ENABLED=true`, và **không env nào còn `$(...)` chưa expand**.

## 5. `IT-K8S-NETPOL-04` — kết luận cũ **sai**, và sai theo kiểu đáng ghi lại

**Cập nhật `2026-08-19`: đã `PASS`.** Cluster **có** thực thi NetworkPolicy. Phần dưới giữ nguyên
lập luận cũ vì cách nó sai mới là thứ đáng đọc.

Kết luận cũ: *"cluster k3s ở cấu hình này không thực thi NetworkPolicy; cần Calico/Cilium"*. Nó
được ghi bốn lần qua bốn slice, và nó **không đúng**.

Cái sai là **phép đo**, không phải cluster. Đối chứng dương chạy
`kubectl run --rm -i -- wget`: tạo một pod rồi **gọi mạng ngay lập tức**. kube-router cài luật
iptables cho từng pod **sau khi pod xuất hiện**, nên một pod phóng ra khỏi cổng ngay lập tức
**thắng cuộc đua**. Log k3s vẫn ghi rõ `Starting network policy controller version v2.2.1` — bộ
điều khiển luôn ở đó; không ai chờ nó.

Sửa: pod thử **sống lâu** (`sleep 600`), và phép đo tách làm hai thời điểm.

| Bước | Vì sao |
| --- | --- |
| tạo pod **trước**, chưa có policy nào | đo được **mức nền**: pod này ra được internet |
| áp deny-all **sau đó** | giờ mới có thứ để đo tác dụng |
| **chờ** tới khi bị chặn (tối đa 60s) | "không thực thi" và "chưa thực thi kịp" trông y hệt nhau ở t=0 |

Thứ tự là điểm mấu chốt: áp policy trước rồi mới đo thì **không phân biệt được** "policy chặn nó"
với "nó vốn không có lối ra" — mà một pod không có lối ra thì bị chặn bởi **địa lý**, không
phải bởi chính sách. Bản sửa đầu tiên của tôi mắc đúng lỗi đó và cổng mới đỏ với thông báo *"the
pod has no route, and geography is not a policy"*.

Bài học giữ nguyên và mạnh hơn: **đối chứng dương là đúng, nhưng một đối chứng có điều kiện đua thì
đo thời điểm chứ không đo chính sách.** Nếu không có đối chứng, một cluster không thực thi sẽ cho
ra đúng màu xanh như một chính sách đúng. Nếu có đối chứng mà không chờ, một cluster **thực thi
được** bị ghi nhầm là không — và cả dự án đi tìm một CNI nó không cần.

## 6. `IT-K8S-PROBE-03` — đúng nửa quan trọng nhất

Scale postgres về 0 → api chuyển `0/1` và Kubernetes **rút pod khỏi Service endpoints**. Khôi phục
→ pod quay lại rotation.

Điều được khẳng định thêm và mới là điểm chính: **`restartCount` vẫn là 0**. Pod ra khỏi rotation
mà **không** bị restart — nghĩa là liveness không đi theo readiness. Nếu đi theo, một sự cố
dependency sẽ restart **mọi** pod cùng lúc, biến một downstream chậm thành một sự cố toàn hệ.

## 7. `IT-K8S-RETENTION-05` — đã đóng ở `W-0047`, sau hai lần chẩn đoán sai chỗ

Bản đầu của tôi ship một CronJob **bật sẵn** với hai biến env `IVR_RETENTION_RUN_ONCE` và
`IVR_RETENTION_DRY_RUN` — **tôi bịa cả hai tên**; key thật là `Ivr:Retention:*`, nên ứng dụng bỏ qua
chúng trong im lặng.

Và sâu hơn: worker **không có entrypoint run-once**. `RetentionJobHost` chạy đúng một lượt rồi
`return`, nhưng nó là `BackgroundService` trong host worker, còn scheduler, normalisation và callback
host giữ tiến trình sống. Pod CronJob vì thế **không bao giờ thoát** và Job bị ghi `failed`.

**`W-0047` đóng cả hai:** `Ivr__Retention__RunOnce` cộng `RetentionRunOnceHost` chạy một lượt rồi
dừng host. Trong run-once mode các hosted service khác **không được đăng ký** chứ không chỉ dừng
sau — một pod retention lỡ chạy scheduler có thể **đặt một cuộc gọi**, và một job tên "retention"
không được phép làm thế.

### 7.1 Rồi test vẫn đỏ — và lần này lỗi nằm trong chính bộ kiểm

Sau khi sửa code, `IT-K8S-RETENTION-05` **vẫn** thất bại trong cluster với:

```
relation "ivr_retention_checkpoints" does not exist
```

Cùng image chạy local với một Postgres đã migrate thì **exit 0**. Nên nguyên nhân không nằm ở
retention. Truy ra: **0 bảng `ivr_*`** trong cluster, dù Job migrate báo `Done`.

Thủ phạm là `IT-K8S-PROBE-03`: nó scale postgres **về 0 rồi lên lại** để chứng minh readiness rút
pod khỏi rotation — mà postgres trong `bootstrap-dev.yaml` **không có volume**. Pod mới sinh ra với
database rỗng, và **mọi kiểm tra sau đó** chạy trên schema đã biến mất.

Một test readiness **âm thầm xoá database** là test đang kiểm thứ nó không định kiểm. Sửa bằng cách
thêm PVC cho postgres trong bootstrap. Điều này cũng giải thích luôn hiện tượng gặp lúc dựng chart:
`rollout restart` làm mất schema.

### 7.2 Trạng thái hiện tại

`IT-K8S-RETENTION-05` **PASS**: CronJob `30 2 * * *`, `concurrencyPolicy: Forbid`,
`Ivr__Retention__DryRun=true`, `Ivr__Retention__RunOnce=true`, kế thừa sàn governance — và một Job
tạo từ nó **chạy xong** trong cluster.

Dòng tổng kết của self-test giờ là **`K8S_SELFTEST_PASS`** — không còn mục nào `NOT_PROVEN`.

## 8. HPA và trần SIM

Worker HPA **tắt mặc định**. Thông lượng worker bị chặn bởi pool SIM chứ không phải số pod (DT-04);
scale vượt pool không quay thêm cuộc nào — advisory lock của scheduler (P2-3) khiến pod thừa tranh
chấp cùng lease. Một autoscaler nhìn CPU sẽ thêm pod không làm gì trong khi *trông như* đã phản ứng
với tải: tệ hơn không scale, vì nó **giấu mất trần thật**.

Ai bật thì `maxReplicas` vẫn bị kẹp ở `simPoolSize` bằng `min` trong template.

## 9. Kiểm chứng

| Kiểm tra | Kết quả |
| --- | --- |
| `IT-K8S-LINT-01` | `helm lint` 0 failed × 4 env; `kubeconform` Invalid: 0 × 4 env (12–13 object) |
| `IT-K8S-GATE-02` | 3 kiểm âm ladder đỏ đúng lý do; pod api/worker đang chạy đều MOCK/NO/kill-switch-on |
| `IT-K8S-PROBE-03` | endpoints rỗng khi DB mất, `restartCount=0`, quay lại khi DB về |
| `IT-K8S-NETPOL-04` | **PASS** (`2026-08-19`) — đối chứng dương ra được internet **trước** khi áp deny-all, rồi bị chặn sau khi luật hội tụ; pod mang selector của chart bị chặn |
| `IT-K8S-RETENTION-05` | **PASS** (đóng ở `W-0047`) — Job tạo từ CronJob chạy xong trong cluster |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` (mở rộng cho fragment k8s; kiểm âm đỏ) |
| `scan-pii.sh` | `PII_SCAN_PASS` |
| `k8s-selftest.mjs` | `K8S_SELFTEST_PASS` (`2026-08-19`); trước đó `..._WITH_NOT_PROVEN=NETPOL_ENFORCEMENT,RETENTION_EXECUTION` |

Dòng cuối là cả thiết kế lẫn kết quả: script **exit 0** nhưng **gọi tên** từng thứ chưa chứng
minh thay vì gộp thành một cờ chung. "Có gì đó chưa chứng minh" bắt người đọc tiếp theo đi mò;
nói rõ cái nào thì họ biết phải sửa gì. Và nó không phải một job đỏ vĩnh viễn rồi bị
`allow_failure` hoá thành đồ đạc — đúng cái bẫy `P5-5` đã nêu.

## 10. Cái này KHÔNG chứng minh

- **NetworkPolicy chưa được chứng minh về hành vi** (§5). Đây là khoảng trống lớn nhất của slice.
- **Chỉ `dev` được triển khai thật.** `staging`/`pilot`/`prod` mới qua lint + kubeconform + kiểm
  ladder; chưa values nào trong ba cái đó chạy trên một cluster.
- **Chưa có ExternalSecret/Vault** (§6.4). Chart **tham chiếu** Secret và không mang Secret nào, nên
  đổi sang nguồn ngoài là đổi nơi tạo Secret — nhưng lối tích hợp đó chưa dựng.
- **Chưa kiểm HPA thật sự scale.** Trần SIM được ép bằng `min` trong template và ghi trong
  annotation; chưa lượt chạy nào tạo tải để quan sát hành vi scale.
- **Retention vẫn chạy ở `dryRun=true`**, nên việc nó xoá **đúng class** (DF-07) chưa được chứng
  minh — mới chứng minh job chạy xong và mặc định không xoá gì.
- **Chưa có ingress/TLS.** Service là `ClusterIP`; lối vào từ ngoài thuộc hạ tầng platform
  (`W-0063`).
