# W-0279 — Đóng lại trạng thái đỏ có chủ ý của W-0277

Ngày: 2026-09-11 · Baseline: `main@dcfa89f` · Trạng thái: **TESTS_PASS**.

Owner chốt ngày 2026-09-11: gác việc xác minh bằng `oasdiff`, đưa CI về xanh.

## 1. W-0277 đã làm gì, và giữ lại phần nào

`W-0277` cố ý làm `api_contract_diff` đỏ để lấy bản changelog mà máy dev không sinh được. Nó đổi ba
thứ; lượt này **chỉ hoàn tác hai**:

| Thay đổi của `W-0277` | Lượt này |
| --- | --- |
| baseline `draft.26 → draft.25` | **hoàn tác** → `draft.27` |
| `docs/api/changelog/ivr-order-confirmation.md` thành placeholder | **hoàn tác** → khuôn generator thật |
| **kiểm `breaking` chạy trước so-sánh changelog** | **giữ** |

Cái thứ ba không phải phần của trạng thái đỏ, nó là sửa lỗi thứ tự: trước đó một changelog cũ làm job
chết ở bước `diff` nên bước `breaking` **không bao giờ chạy** — một breaking change sẽ được báo thành
*"changelog của bạn lỗi thời"*. Hai dòng `cat /tmp/...` cũng giữ: in body ra log là chẩn đoán có ích,
và vô hại khi xanh.

## 2. Đưa về nhất quán

- Thêm `specs/api/openapi/baselines/ivr-order-confirmation.v1.0.0-draft.27.yaml`
- `docs.gitlab-ci.yml` trỏ `draft.27` ở cả hai dòng
- `docs/api-changelog.md` cột *Baseline* → `draft.27` (ràng buộc của `CT-CI-12`)
- Changelog viết lại đúng khuôn generator, lấy **byte-exact từ lịch sử git** chứ không gõ tay:

```
# API Changelog 1.0.0-draft.27 vs. 1.0.0-draft.27
<dòng trống>
No changes detected
```

Và điều làm dòng đó **đúng sự thật** chứ không phải đoán: baseline vừa tạo băm ra `7739c42582e4…`,
**giống hệt** spec hiện tại. `oasdiff` so hai file bằng nhau thì ra đúng ba dòng đó.

## 3. Cái giá, nói rõ

`draft.26` và `draft.27` — tức toàn bộ việc siết `adapter_mode`/`provider_name` thành enum — **chưa
bao giờ được `oasdiff breaking` soi**, và với cấu hình này sẽ không bao giờ.

Baseline lại bằng current, nên diff luôn rỗng. Đây đúng là lỗ hổng quy ước `W-0275` §6 đã ghi và
`draft.25` của agent khác cũng vướng. `CT-CI-12` **cho phép** baseline chậm một release chính vì lý
do đó; đưa nó về bằng nhau là lựa chọn của owner, không phải giới hạn kỹ thuật.

Muốn soi thật thì trỏ baseline về `draft.25` và commit bản changelog do image CI sinh ra.

## 4. Kết quả

`GATE_SWEEP_PASS 39/39`. Không chạm mã .NET, không file nào bị pin → **không cascade**.

Một lượt sweep trung gian báo 38/39 với `dr-selftest: spawnSync node ETIMEDOUT` ở đúng trần 180s —
nó chạy 136–156s nên sát trần; lượt sau xanh cả 39. Ghi lại vì con số 38/39 đã hiện ra.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN

## Cập nhật chỉ dấu nghiệm thu — W-0325, 21/09/2026

REAL_CUSTOMER_CALL_ALLOWED=NO

Đây là giới hạn ủy quyền hiện hành theo tracker §2 tại commit `4346f6a`, bổ sung để kiểm C1.
Kết quả, thời điểm và phạm vi kiểm chứng lịch sử ở trên giữ nguyên; mục này không xác nhận
một lượt chạy mới và không thay chữ ký nghiệm thu. Xem [hồ sơ bổ sung W-0325](../W-0325/README.md).

## Owner nghiệm thu — 23/09/2026

Toàn chỉ thị “chấp nhận nhóm A và B” theo [phiếu W-0348](../W-0348/approval-request.md) tại
`7fc9806`. W-0279 thuộc nhóm A5 và chuyển **TESTS_PASS → ACCEPTED** cho phần đã làm: Đóng trạng thái
đỏ của W-0277. Claude ghi nhận quyết định của owner, không tự cấp phê duyệt.

Còn mở, không thuộc nghiệm thu này: chỉ local. Mọi giới hạn trong cột Residual của tracker giữ
nguyên. Bằng chứng giữ đúng lượt collector tại `ca4f442` (1200/1200 test, sweep 43/43), không coi
commit tài liệu là một lượt test/sweep mới. REAL_CUSTOMER_CALL_ALLOWED=NO.
