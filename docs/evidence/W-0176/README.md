# W-0176 — Final external-decision provenance pin rotation

Ngày: `2026-09-04`

Baseline: `main@c213bf7663708dfca7184bf443e66d6552e2daea` + shared WIP
`W-0169..W-0175` được bảo toàn.

Trạng thái: **`TESTS_PASS_LOCAL / CURRENT_PIN_CHAIN_VALID /
BLOCKED_EXTERNAL / 0_OF_5_DISPATCHED / NO_GATE_PROMOTION`**

## 1. Root cause đã xác minh

Trước W-0176, current manifest có đúng 18 record và chỉ drift một member:

- M8-07 expected `72ddb92347fc88fad8607d2f9ceef40546274f828642a041fe021049c6a7e426`;
- M8-07 actual `c4bb79fa8b06c0f06a8b959b698084f9d02444a5cb1a25e14413b87ae74c1aa0`.

Đây là content change hợp lệ từ W-0173/W-0174, không phải lỗi thuật toán validator. W-0164 vẫn
xanh vì nó chỉ kiểm nguồn dispatch; W-0165 đọc từng manifest member nên từ chối M8-07, và W-0170
từ chối downstream do prerequisite W-0165 đỏ.

## 2. Rotation theo dependency

Pin được xoay theo đúng thứ tự, không sửa schema hay business rule:

1. cập nhật M8-07 hash trong M8-12;
2. tính lại M8-12 rồi cập nhật hai member M8-07/M8-12 trong manifest;
3. tính lại manifest rồi cập nhật mọi message D-01..D-05 trong M8-13;
4. xoay source pins của W-0164/W-0165 và hai template;
5. tính lại hai validator rồi xoay source pins W-0170 và closure template.

| Artifact hiện hành | SHA-256 |
| --- | --- |
| M8-07 | `c4bb79fa8b06c0f06a8b959b698084f9d02444a5cb1a25e14413b87ae74c1aa0` |
| M8-12 | `9da8e5698bc99df73338b3d6886e61f18c93e492431d07cb730074f6ef3aa499` |
| Manifest 18 member | `3352479690e424b88138654b1a91aa5c55908b19d47ee63870795b113e616471` |
| M8-13 | `95632b90ab99df6892ba6f0a231e9429c5021fd200d982609c52584e1b920ca3` |
| W-0164 validator | `e70eb8b90e2a5697219f375baab7e6c0d6cb7d58053310ca8fd47caf07180d45` |
| W-0164 template | `5fc84bee77beff876511cc41737123d7362806590e6015018b723ffdfd95abeb` |
| W-0165 validator | `1d14a46eeceb4a59586e23cd84668be50836831ef71c3a50c53a85386d72e1dc` |
| W-0165 template | `5e19f8a9342135ecb503050cc208262c8b4081b2a96007d381124c191d6437da` |
| W-0170 validator | `8ef4881404fce45d0905cdf74a763938919138ad2ca99520b60aaff2cba3bda0` |
| W-0170 template | `611c2481faf4d1b4741b8468a3977203286c6c6875e4bdc9db85f3166a7d2a1d` |

## 3. Verification

| Gate | Kết quả |
| --- | --- |
| Node syntax, ba validator | **PASS** |
| W-0164 | **PASS** `template=1 valid=2 refusals=19` |
| W-0165 | **PASS** `template=1 valid=2 refusals=27` |
| W-0170 | **PASS** `valid=1 refusals=21` |
| Ba pending template | **PASS valid-not-ready** |
| Pending template chạy `--input` | **REFUSED `3/3`** |
| Current manifest | **PASS `18/18`, drift `0`** |
| Test traceability | **PASS `485`** |
| PII scan, deliverables | **PASS `11/11`** |
| PII scanner self-test | **PASS `CT-CI-06..06h`** |

Các SHA trong bảng trên là snapshot lịch sử đúng tại thời điểm W-0176. Detached candidate W-0177
sau đó phát hiện hai RFQ trong manifest còn phụ thuộc CRLF của shared Windows checkout. W-0177
canonicalize hai member đó sang LF và cập nhật current chain; dùng bảng current trong
[W-0170](../W-0170/README.md), không dùng snapshot W-0176 để dispatch.
| API docs self-test | **PASS `14` generated artifacts** |
| CI config self-test | **PASS** |
| PII deliverables | **PASS `11` text files** |
| PII scanner self-test | **PASS `CT-CI-06..06h`** |
| Gate/readiness mirror | **PASS `11` gates / `174` work items / `23` open decisions / production=false** |
| Scoped `git diff --check` | **PASS** |

Các positive self-test chỉ dùng synthetic alias/hash. Không có recipient, response, receipt hoặc
authority evidence thật được tạo trong W-0176.

Ba validator chứa các negative fixture có chủ đích để chứng minh PII-like input bị từ chối. Vì vậy
PII deliverable scan chỉ bao phủ evidence, template và manifest; scanner self-test độc lập xác nhận
negative controls vẫn hoạt động. Kết quả đó không được diễn giải thành việc validator source không có
chuỗi mô phỏng dùng cho refusal test.

Ba validator source cố ý chứa email/phone/address giả trong refusal fixtures, nên quét thẳng source
phải phát hiện các dòng đó. PII verdict ở trên áp dụng cho 11 artifact giao nhận; scanner self-test
chứng minh các fixture bị từ chối đúng, không xóa đối chứng âm để làm gate xanh.

## 4. Phạm vi ảnh hưởng

GitNexus impact trước edit cho ba `SOURCE_PINS`: **LOW**, mỗi symbol có `0` direct caller,
`0` process và `0` module. Thay đổi chỉ xoay exact SHA-256 và template source block; không đụng
scheduler, callback runtime, DB, outbound connector hoặc delivery guard.

## 5. Ranh giới và phần còn lại

- W-0164/W-0165/W-0170 local chain hiện tự nhất quán, nhưng external dispatch vẫn `0/5`.
- Không có recipient authority, approved destination, ticket/audit system-of-record hoặc receipt
  thật; không được ghi `SENT`, `DELIVERED` hay `APPROVED`.
- Không có production sign-off; `TARGET_CONTRACT_V1=DRAFT` và
  `REAL_CUSTOMER_CALL_ALLOWED=NO`.
- Current shared tree chưa phải immutable candidate. Hosted CI và Docker-backed Integration/Chaos
  vẫn là gate độc lập.

## 6. Bước tiếp theo

Review và đóng gói W-0176 pin set cùng W-0175 LF rules thành một exact candidate; chạy lại ba
validator từ detached clean worktree. Sau đó mới dùng routing template khi Module 8 Owner/chief
auditor cung cấp recipient alias, authority ref, approved destination và receipt system-of-record.

## 7. Exact-checkout correction — W-0177

Bảng hash ở §2 là snapshot trước khi W-0177 pin thêm `R-00`/`R-06` thành LF. Exact checkout
`973df3c` phát hiện manifest vẫn giữ SHA của byte CRLF cũ nên W-0165/W-0170 đỏ. W-0177 xoay lại
toàn bộ chain theo byte LF canonical; xem [evidence W-0177](../W-0177/README.md). Không dùng các hash
§2 cho dispatch cycle mới.

## 8. Supersession — W-0186

Sau commit `8ed62e9` xóa source set và W-0180 làm T-09 đổi byte hợp lệ, W-0186 đã restore/re-pin
current chain. Dùng bảng current tại [W-0170 §7](../W-0170/README.md#7-current-head-restore-và-pin-rotation--w-0186),
không dùng snapshot §2/§7 của tài liệu này để mở dispatch cycle mới.
