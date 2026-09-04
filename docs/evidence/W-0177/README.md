# W-0177 — Exact local candidate freeze and verification

Ngày: `2026-09-04`
Pre-fix candidate: `973df3c50554125f7b96892d4a0ea3a84d779cc9`
Trạng thái: `IN_PROGRESS / CLEAN_CHECKOUT_PIN_DRIFT_FOUND / CANDIDATE_REBUILD_REQUIRED`.

## 1. Phạm vi

- đóng băng W-0170/W-0172..W-0176 thành commit local bất biến;
- kiểm tra từ detached clean worktree với `core.autocrlf=true`;
- chạy validator/hash/docs/security và full .NET solution gồm PostgreSQL/Chaos khi Docker sẵn sàng;
- không push, không trigger hosted CI, không dispatch external decision và không gỡ production guard.

## 2. Finding từ exact checkout

Candidate đầu gồm `f4201b1` và LF follow-up `973df3c`. Detached checkout đúng `973df3c` cho thấy:

- W-0164 và W-0174 self-test vẫn PASS;
- W-0165 từ chối `R-00-voice-gateway-rfq.md` vì artifact manifest còn pin byte CRLF cũ;
- W-0170 từ chối downstream vì prerequisite W-0165 đỏ;
- scan đủ 18 manifest member xác nhận chỉ `R-00` và `R-06` lệch;
- committed/physical blob đã là LF đúng `.gitattributes`; lỗi nằm ở hash chain chưa được xoay sau khi
  hai file được thêm vào LF policy, không phải semantic content drift.

Hai SHA-256 canonical LF:

| Artifact | SHA-256 |
| --- | --- |
| `R-00-voice-gateway-rfq.md` | `ae483fa33e5741b49a0d6c3e71bfedf51df14eec46d2a69935f45809f0e7ab37` |
| `R-06-to-trinh-mua-thiet-bi.md` | `341153903009c37fb7fcf5ea3c5bdb253a6c7cd49f908676ff7c0612e681eea4` |

## 3. Bounded remediation

Hash được xoay đúng dependency:

1. hai source row trong M8-12;
2. M8-12 và hai source row trong manifest 18 member;
3. M8-12/manifest pin trong năm message D-01..D-05 của M8-13;
4. source pins và pending template của W-0164/W-0165;
5. source pins và pending template của W-0170.

Không sửa validator algorithm/schema, source contract, runtime, DB, callback, scheduler hoặc safety
guard. GitNexus impact của ba `SOURCE_PINS` đều **LOW**, `0` direct caller, `0` process/module.

## 4. Verification đang thực hiện

| Gate | Kết quả hiện tại |
| --- | --- |
| W-0164 | **PASS** `template=1 valid=2 refusals=19` |
| W-0165 | **PASS** `template=1 valid=2 refusals=27` sau re-pin |
| W-0170 | **PASS** `valid=1 refusals=21` sau re-pin |
| W-0174 | **PASS** `valid=1 refusals=46` |
| Ba pending template | **PASS valid-not-ready**; không có routing/response/receipt thật |
| Detached clean candidate mới | **PENDING** |
| Full .NET Integration/Chaos | **PENDING** |
| Security wrapper | **PENDING** |
| Hosted GitLab CI | **AUTH_BLOCKED / NOT_RUN** |

## 5. Boundary

Finding pin drift làm `973df3c` bị loại khỏi release candidacy. Shared-tree validator PASS chỉ cho phép
tạo candidate mới; không được hồi tố ghi `973df3c` là PASS. External dispatch vẫn `0/5`, mọi
M3/Product/Security/Platform/Telephony evidence và shared E2E vẫn chưa nhận.
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

Next action: commit bounded re-pin thành candidate mới, tạo detached clean checkout mới và chạy lại
toàn bộ gate. Chỉ cập nhật `TESTS_PASS` sau khi exact SHA trả kết quả xanh.
