# W-0352 — Lượt gate mở rộng: chạy bốn gate Docker để xét 18 việc

Ngày 24/09/2026 · Claude · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

**Trạng thái: ứng viên đang được kiểm; lượt collector có `--extended` chạy ở bước tiếp theo.**

## Vì sao có việc này

Sau [W-0351](../W-0351/README.md), 18 việc vẫn trượt C2. Lý do là chúng trích TestId của bốn gate mà full sweep bỏ
qua vì cần image hoặc mạng: `image-selftest.mjs`, `k8s-selftest.mjs`, `selftest-oasdiff.sh` và `security-scan.sh`.
Toàn trả lời: *“cái nào ko cần thì xóa đi, còn cái nào cần thì cho xong đi”*, sau khi đã thấy danh sách image cần tải
(khoảng 1,4 GB nén).

- **Không cần:** W-0116 trích một test của admin-ui, mà owner đã gỡ ở W-0253. Test đó được retire kèm
  `surfaceRemoved`, ghim đúng quyết định W-0253 như ở W-0351.
- **Cần:** mọi TestId còn lại là assertion của bốn gate trên. Cả bốn đều đã từng chạy được trên máy này (W-0286,
  W-0290, W-0291), nên việc này cho chúng chạy lại tại đúng commit, thay vì bỏ qua.

## Lượt gate mở rộng

- `deploy/ci/gate-invocations.json`: bốn gate có thêm danh sách `extended`, gồm tham số, token đạt, timeout, và image
  ghim khi script phải chạy trong image. `selftest-oasdiff.sh` chạy trong đúng image oasdiff mà CI dùng, ghim theo
  digest. `image-selftest.mjs` chạy hai lượt: lượt đầy đủ, và lượt có observability runtime.
- `gate-sweep.mjs --extended` chỉ chạy các lượt đó, theo thứ tự cố định. Mọi dòng gate in ra đều vào log, sau một tiền
  tố riêng, trong phần của chính gate đó. Token đạt phải đứng đầu một dòng, nên dạng `..._WITH_NOT_PROVEN` không được
  tính là đạt. Full sweep thường vẫn chạy offline và giữ nguyên tốc độ.
- `k8s-selftest.mjs --build-images`: tự build ba image mà chart triển khai, như `before_script` của job CI.
- Collector nhận thêm `--extended`, ghi lượt thứ ba vào gói bằng chứng cùng commit, cây và hash log. Thiếu `--extended`
  thì gói vẫn hợp lệ, nhưng các việc trích TestId của bốn gate ra `CHƯA KIỂM`.
- C2 chỉ tính một TestId của bốn gate khi phần log của đúng gate sở hữu nó in dòng `PASS` của TestId đó. Registry
  giữ 18 TestId, và self-test kiểm từng TestId được in bởi gate của nó, hoặc bởi module mà gate đó import. TestId của
  bốn gate không nhắc (`mentions`) được, vì chúng là claim.

## Security scan: 47 cảnh báo nhầm

Lượt đầu, `security-scan.sh` đỏ ở bước quét lịch sử git: 47 phát hiện của luật `generic-api-key` trong các commit từ
15/09 tới 23/09, sau lượt rà W-0291. Hosted CI đỏ cùng lý do từ 15/09. Từng dòng được mở tại đúng commit của nó:

| Loại | Số | Đã kiểm |
| --- | ---: | --- |
| Tên cờ `v1NotificationEnabled` trong seed data của migration EF | 35 | Dòng khớp đúng tên cờ, không có giá trị nào khác |
| Redoc hiển thị lại tên cờ đó; class CSS có chữ `token` | 3 | Dòng là thẻ `span` chứa đúng tên cờ |
| `sourceHashes` của portal | 5 | Giá trị bằng SHA-256 của file được nêu tên, tại cùng commit |
| SHA-256 của `images/ivr-api.tar` trong bộ kit W-0344/W-0345 | 4 | Cùng một giá trị ở cả bốn manifest; file tar không nằm trong repo |

Không có secret nào. 47 dòng được thêm vào `.gitleaksignore`, ghim theo commit, file, luật và số dòng như mọi ngoại lệ
trước đó; không nới luật ở mức cấu hình, không bỏ qua cả file. Hồ sơ rà: [reviewed-findings.json](reviewed-findings.json).
Mẫu đối chứng (token GitHub giả) vẫn bị bắt. Toàn có thể gỡ các dòng này nếu muốn tự rà.

## Kiểm chứng

- Self-test của danh sách (`acceptance-batches.mjs --self-test`): 96 phép kiểm C2 (trước 84). Self-test bằng chứng
  (`acceptance-evidence-selftest.mjs`): 70 phép kiểm (trước 47), gồm một lượt collector giả có `--extended` từ đầu
  tới cuối. Cả hai chạy trong full sweep.
- Bốn phép thử đột biến đều làm self-test đỏ: nhận dòng `PASS` từ gate khác, nhận dòng kết quả `FAIL`, bỏ kiểm tra
  gate có in TestId, và cho nhắc một TestId của gate mở rộng.
- Self-test bằng chứng bắt được một lỗi thật: collector ghi log của mọi lượt không phải test vào ô `sweep`, nên lượt
  thứ ba ghi đè log sweep. Đã sửa.
- Chế độ worktree: cả 18 việc chuyển từ `KHÔNG ĐẠT` sang `CHƯA KIỂM`, chờ lượt mở rộng.

## Chạy thử từng gate trước khi commit

Mỗi gate được chạy qua `gate-sweep.mjs --extended --only <gate>` trên máy này, trước lượt collector:

| Gate | Kết quả |
| --- | --- |
| `selftest-oasdiff.sh` | Đạt trong 1,7 giây, trong đúng image oasdiff ghim theo digest |
| `security-scan.sh` | Lượt đầu đỏ vì 47 cảnh báo nhầm ở trên. Sau khi rà và ghi, đạt trong 60 giây: gitleaks sạch, NuGet và npm không có lỗ hổng HIGH |
| `image-selftest.mjs` | Lượt đầy đủ đạt trong 15,6 phút: build, health, compose, quét image, SBOM, E2E 8 tác vụ. Hai image quét ra 0 HIGH, 0 CRITICAL |
| `image-selftest.mjs --observability-runtime` | Lượt đầu đỏ ở bước build image migrate: BuildKit báo “frontend grpc server closed unexpectedly” đúng lúc runner GitLab trên cùng máy chạy một job Docker. Chạy lại thì đạt, trace có đủ năm chặng qua API và worker |
| `k8s-selftest.mjs --build-images` | Đạt trong 6 phút trên cụm k3s tạm: đủ bảy assertion, `K8S_SELFTEST_PASS` không kèm `NOT_PROVEN`; network policy chặn thật, 90 giây mất database không làm worker restart |

Lượt đỏ của observability là lỗi hạ tầng, không phải lỗi của gate. Nhưng nó cho thấy lượt mở rộng không nên chạy cùng
lúc với job CI dùng Docker trên máy này. Vì vậy ứng viên chỉ được push sau khi collector xong.
