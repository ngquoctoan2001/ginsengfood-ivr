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
lên rollback — và nó chưa có test nào ép. Ghi ở đây vì đó là cái bẫy mà rollback tự động dễ làm
người ta quên.

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
