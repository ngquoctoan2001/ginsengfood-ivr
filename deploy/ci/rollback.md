# Rollback — `W-0045` · `P7-3` §8

> **Chưa lần nào chạy.** Không có runner, registry hay credential cluster cho môi trường thật
> (`W-0061`, `W-0063` — cả hai `BLOCKED_EXTERNAL`). Đây là quy trình đã viết, **không phải** quy
> trình đã diễn tập. `P7-3` §10 nói rõ YAML và mô phỏng local không được gọi là deploy proof, và
> tài liệu này không tự nhận khác.

## 1. Ba lớp rollback, và lớp nào tự động

| Lớp | Kích hoạt | Ai làm |
| --- | --- | --- |
| `helm --atomic` | rollout không khoẻ trong `--timeout` | **tự động**, ngay trong lệnh upgrade |
| `after_script` của job | job thất bại **sau** khi upgrade trả về (ví dụ smoke đỏ) | **tự động**, trong CI |
| `rollback_prod` | quyết định của người sau khi đã deploy xong | **thủ công**, protected environment |

Hai lớp đầu tồn tại vì chúng bắt hai loại hỏng khác nhau. `--atomic` bắt "rollout không lên nổi".
`after_script` bắt trường hợp nguy hiểm hơn: **Kubernetes coi rollout là khoẻ trong khi dịch vụ trả
lời sai** — pod `Ready`, probe xanh, nhưng smoke sau deploy đỏ.

## 2. Rollback thủ công

```bash
helm history ivr --namespace ivr-prod --max 10
helm rollback ivr <REVISION> --namespace ivr-prod --wait --timeout 10m
kubectl -n ivr-prod rollout status deploy/ivr-ivr-api --timeout=5m
```

`<REVISION>` **phải nêu rõ**. `helm rollback` không tham số lùi đúng một bước, mà "một bước" là
tương đối với lịch sử tại thời điểm đó — nếu có hai lần deploy chồng nhau thì nó không lùi về chỗ
người vận hành đang nghĩ tới.

## 3. Migration **không** lùi cùng release

Đây là điều quan trọng nhất trong tài liệu này.

`helm rollback` đưa **manifest** về revision cũ. Nó **không** hoàn tác migration mà Job
`pre-upgrade` đã áp — schema đã đổi rồi. Lùi image về bản cũ trong khi schema đã tiến lên nghĩa là
chạy code cũ trên schema mới.

Vì vậy migration phải **tương thích lùi một bậc**: thêm cột nullable, không đổi tên và không xoá cột
trong cùng một release với code dùng nó. Đây là ràng buộc lên **cách viết migration**, không phải
lên rollback. Ghi ở đây vì đó là cái bẫy mà rollback tự động dễ làm người ta quên.

Bản đầu của tài liệu này (`W-0045`) kết đoạn trên bằng *"và nó chưa có test nào ép"*. Câu đó
**hết đúng từ `W-0046`**, khi `IT-MIGRATE-03` trong `progressive-selftest.mjs` bắt đầu quét mã
nguồn migration, và không ai quay lại sửa. Ghi lại ở đây thay vì lặng lẽ xoá: một tài liệu tự nhận
"chưa có gì canh" trong khi đã có là loại sai nguy hiểm hơn im lặng.

`W-0114` mở rộng phần đó. `UT-SCHEMA-BACKCOMPAT-01` đọc `Up` của **mọi** migration qua
`Migration.UpOperations` — mô hình thao tác có kiểu, không phải văn bản — và từ chối bảy dạng mà
release liền trước không sống nổi: xoá cột, xoá bảng, đổi tên cột, đổi tên bảng, thêm cột
`NOT NULL` không default, `AlterColumn` **thu hẹp** (siết nullable, rút ngắn, đổi kiểu), và thêm
ràng buộc mới (unique hoặc `CHECK`) lên cột **đã có từ trước**.

Ba dạng cuối là thứ `IT-MIGRATE-03` không thấy. Ngược lại, `IT-MIGRATE-03` chạy trong image node
không cần .NET nên đỏ sớm hơn và rẻ hơn — nên **cả hai đều giữ**. Khác biệt duy nhất về phán quyết
là `AlterColumn`: quét văn bản thấy lời gọi chứ không thấy tham số nên phải chặn mọi `AlterColumn`,
còn đọc thao tác thì phân biệt được nới rộng (an toàn theo chiều này) với thu hẹp.

Cổng có **danh sách miễn trừ**, và nó đang rỗng. Rỗng vì mọi migration trong repo hiện đã thuần bổ
sung, không phải vì mặc định là bỏ qua: cách hợp lệ để xoá một cột là expand/contract — thêm, ship
code thôi đọc cột cũ, rồi mới xoá ở release sau — và release cuối cùng ấy cần chỗ để nói ra điều
đó. Danh sách nằm ngay trong file test chứ không phải một JSON bên cạnh, để việc thêm miễn trừ hiện
lên trong review là "có người sửa cổng tương thích schema".

## 3a. Chiều còn lại — code mới trên schema cũ

Chiều ngược lại xảy ra ở khoảng giữa lúc Job `pre-upgrade` bắt đầu và lúc nó xong, và ở mọi lần
deploy bỏ hook hoặc trỏ nhầm database chưa migrate. Ở đó binary mới **không thể** đọc schema cũ —
EF gọi tên cột chưa tồn tại nên câu đọc đầu tiên là `42703 undefined_column`. Điều phải giữ không
phải "vẫn chạy" mà là **hỏng đúng một dạng mà rolling deploy sống được**:

| Phải | Vì sao |
| --- | --- |
| `/health/ready` trả `503` `schema_behind` | pod báo Healthy ở đây sẽ nhận traffic rồi trả lỗi cho người gọi |
| `/health/live` và `/health/startup` vẫn `200` | liveness đỏ làm **crash-loop** mọi replica; `--atomic` cần rollout **đứng**, không cần nó giãy |
| readiness tự xanh lại khi migration xong, **không cần restart** | API không được báo là hook đã xong; nếu readiness chốt cứng thì rollout đứng vì một schema đã đúng |

`IT-SCHEMA-NEWCODE-01` dựng schema tiến từ rỗng đến migration liền trước — không lùi bằng `Down`,
vì schema mà deploy thật gặp là schema do release trước dựng lên — rồi khởi động **đúng entry point
của bản ship** trên đó. `IT-SCHEMA-NEWCODE-02` khoá tiền đề: đúng một bậc, bậc đó là migration mới
nhất, và migration đó có thao tác thật.

## 4. Evidence không bị rollback

`ci-artifacts/cd/` giữ digest và effective values của lần deploy **đã xảy ra**, kể cả khi nó bị lùi.
Rollback là một sự kiện mới, không phải một cái tẩy: hồ sơ phải trả lời được "lúc đó cái gì đang
chạy", chứ không chỉ "bây giờ cái gì đang chạy".

## 5. Sau khi rollback

1. Ghi lại revision đã lùi về và lý do.
2. Kiểm `helm get values ivr -n <ns>` xác nhận `realCustomerCallAllowed: false` vẫn giữ — rollback
   đưa về **cấu hình cũ**, và cấu hình cũ cũng phải nằm ở đáy ladder.
3. Kiểm alert `IvrDownstreamFailClosedSpike` và `IvrCallbackRevalidateLatencyBreach` (`W-0041`) đã
   tắt chưa; nếu chưa thì rollback chưa giải quyết được nguyên nhân.
