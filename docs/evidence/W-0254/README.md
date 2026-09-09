# W-0254 — Sửa tài liệu M3 đọc, và ghim để nó không lệch lại

Ngày: 2026-09-09 · Baseline: `main@c0e6609` · Trạng thái: **TESTS_PASS**.

Owner yêu cầu dọn tài liệu rồi chuẩn bị tích hợp Module 3. Dọn xong `W-0253` thì rõ rằng phần khó
không nằm ở rác: **code sạch** (0 warning, 0 `NotImplementedException`, TODO duy nhất nằm trong mã
NSwag sinh ra) và **tài liệu liền link** (5 link gãy trên 669 tệp markdown). Vấn đề là `IR-06` —
tài liệu **duy nhất** M3 dựng theo — đang nói những điều **không còn đúng**.

## 1. Nó bảo M3 sinh client từ một contract cũ

`draft.24` là bản có **breaking change**. `IR-06` vẫn trỏ `draft.23` ở **năm** chỗ, gồm cả dòng
checklist *"Sinh lại client từ OpenAPI `1.0.0-draft.23`"*. `W-0250` sửa bảng field và bảng mã lỗi
trong `IR-06` nhưng **bỏ sót các con trỏ phiên bản** — sót của chính tôi.

M3 làm theo sẽ sinh client từ spec cũ, rồi gửi payload mà `draft.24` từ chối ở tầng schema.

## 2. Nó bảo M3 chờ những đội không tồn tại

Đây mới là thứ chặn thật. Đối chiếu `IR-06 §9` với `specs/_review/open-decisions-register.md`:

> **23 trong 28** quyết định đã `CLOSED`. Nhưng `§9` vẫn ghi 5 dòng đang chờ *Security*, *Platform*,
> *Telephony*, *Privacy/Legal* — và **ô ký cuối tài liệu** còn chừa hai dòng cho *Security/Platform*
> và *Privacy/Legal* ký.

Không có đội nào trong số đó. Cast thật: **owner IVR**, **dev Module 3**, cộng bên ngoài **thật sự**
là nhà mạng cho trunk thoại và một ý kiến pháp lý nếu owner muốn mua.

Hậu quả cụ thể: `IR-06` nói *"Auth profile + sandbox credential — Security/Platform —
`BLOCKED_EXTERNAL`"*, trong khi `OD-V1-07` **đã `CLOSED` ngày 2026-09-05** do chính owner ký (JWT
khóa bất đối xứng, JWKS, TTL ≤ 10 phút, scope `ivr.task.write`, mTLS hoãn). M3 đọc `IR-06` sẽ ngồi
chờ một đội không có, cho một quyết định đã ký bốn ngày trước.

Tương tự `dial_token`: `§9` ghi `OWNER_DECISION_REQUIRED`, thực tế `OD-V1-05` + `OD-V1-17` +
`OD-V1-18` đều đã đóng `2026-09-05` (vế TTL chốt lại `2026-09-09`, `W-0246`).

## 3. Ba mục treo trên một quorum không tồn tại — việc owner cần quyết

Trong 5 mục "còn mở" của register, chỉ **hai** là việc thật:

| Mục | Thực chất |
| --- | --- |
| `OD-V1-09` giao thức SIM lab | **việc thật** — đã ký giao thức, chờ SIM/nhà mạng |
| `OD-V1-10` 32 eSIM capacity | **việc thật** — cố ý chưa ký, vì 32 là giả định chưa đo |
| `OD-V1-11` script/legal/retention | owner **đã ký nội dung**; treo vì chờ *Legal/Privacy* |
| `OD-V1-21` GitLab provisioning | owner **đã ký**; treo vì chờ *independent approver* |
| `OD-V1-23` opt-out boundary | owner **đã ký**; treo vì chờ *Product + CRM + Legal/Privacy* |

Ba dòng cuối treo trên **cùng một thứ**. Đã ghi thành `§9a` trong `IR-06` kèm hai đường đi, để owner
chọn: **(1)** tuyên bố quorum là chính mình cho cả ba, hoặc **(2)** mua ý kiến pháp lý ngoài cho
riêng `OD-V1-11` — mục duy nhất mà chữ ký ngoài có giá trị thật.

Quan trọng: cho tới khi owner chọn, **ba dòng ấy không chặn M3**. Chúng không chạm contract intake
hay callback ở `§3`/`§4`.

## 4. Sửa gì trong `IR-06`

| Chỗ | Trước | Sau |
| --- | --- | --- |
| Bảng tham chiếu | `bản mới 1.0.0-draft.23` | `bản hiện hành 1.0.0-draft.24`, thêm dòng changelog `draft.23 → draft.24` **đánh dấu có breaking**, thêm dòng bảng nhãn enum |
| `§0` | hai dependency "cần chốt" | cả hai **đã chốt**, kèm mục *"Bắt đầu từ đâu — bốn thứ cần lấy"* và hai breaking change nêu thẳng |
| `§6` | *"OpenAPI current cho phép thiếu `phone_validation_status`"* | ghi chú `W-0250`: câu đó **đã hết đúng**; bản ghi `W-0150` giữ nguyên, không viết lại lịch sử |
| `§7` | *"Cần Security/Platform cung cấp"* | hồ sơ `OD-V1-07` **đã ký**, tách rõ *quyết định* (xong) khỏi *vận hành* (dựng issuer, cấp sandbox) |
| `§9` | 5 dòng chờ đội không tồn tại | đối chiếu từng dòng với register; thêm `§9a` |
| Ô ký | 4 dòng, 2 dòng cho vai không có | 3 dòng, chỉ người có thật, dòng thứ ba là *tuỳ chọn* cho ý kiến pháp lý mua ngoài |

## 5. Ghim lại để không lệch nữa

Con trỏ phiên bản lệch được **cả một release** vì không gì so nó với spec. `FREEZE-03` vốn đã kiểm
bảng field của `IR-06`; nay kiểm cả **con trỏ**:

```text
current-version row          bắt buộc phải có, và phải bằng version của spec
client-generation checklist  nếu có, phải bằng
fetch-the-spec note          nếu có, phải bằng
```

Chỉ kiểm **con trỏ**, không kiểm mọi chuỗi phiên bản trên trang: `IR-06` có quyền nói lịch sử —
*"`draft.21` trở về trước vẫn còn 11 endpoint"* là đúng và phải nói được — còn tên tệp changelog thì
mang hai phiên bản theo cấu tạo. Bản đầu tôi viết kiểm tất, và nó đỏ đúng vào những câu lịch sử hợp
lệ ấy; đã thu hẹp.

Token phiên bản khớp **tổng quát** chứ không theo khuôn `-draft.N`: một release bỏ hậu tố tiền-phát-
hành là rotation hợp lệ, và anchor chỉ nhận draft sẽ báo *"thiếu con trỏ"* thay vì kiểm nó. Đây là
lỗi bản đầu, phát hiện vì case `draft-suffix-dropped-while-still-draft` đỏ.

Case mới trong selftest chứng minh nó đỏ được; case rotation cũ nay mang theo cả `IR-06`, đúng tinh
thần vốn có của nó — *"thực hiện trọn vẹn một rotation hợp lệ, để thứ duy nhất còn đáng phàn nàn là
chính chuỗi phiên bản"*. Selftest **14 → 15 case**.

## 6. Kiểm chứng

```text
contract-freeze-selftest.mjs           CONTRACT_FREEZE_SELFTEST=PASS 15/15   (14 → 15)
  case mới, đảo chiều thử              current-version row -> draft.23  =>  FREEZE-03 đỏ đúng
contract-freeze-verifier.mjs           CONTRACT_FREEZE=PASS
gate-sweep.mjs                         GATE_SWEEP_PASS 39/39 (exit=0)
  bắt đúng 4 pin IR-06 phải xoay       d06 · dial-token · opt-out · upstream, mỗi cái nêu đúng tên tệp
dotnet test Ivr.sln                    952/952 PASS, 0 failed, 0 skipped
IR-06 đường dẫn                        15/15 tồn tại, 0 thiếu
IR-06 §4A endpoint                     31 dòng, **cả 31 đều có** trong spec; 6 operation không liệt kê đều mang tag `internal`
seed fixtures                          9 task hợp lệ · 13 schema_negative · 13 domain_negative
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1` vẫn `DRAFT`.

## 7. Còn lại — và M3 bắt đầu được ngay

M3 **không cần chờ gì** để bắt đầu: contract `draft.24` đã đóng băng và có gate, fixture âm/dương có
sẵn, hai breaking change đã ghi rõ ở `§0`.

| # | Việc | Ai |
| ---: | --- | --- |
| — | Chọn đường cho `§9a`: tự làm quorum, hay mua ý kiến pháp lý cho `OD-V1-11` | **owner** |
| — | Dựng issuer/JWKS thật + cấp sandbox credential cho dev M3 | owner IVR |
| — | `§9` mục 9: ai giữ ba token quản trị, vai M3 nào ánh xạ tầng nào, định dạng `X-Actor-Id` | owner + dev M3 |
| — | endpoint revoke + OAS + IR-06 — `2.5`, lượt phát hành contract sau | tôi |
| — | `4d` điền 20 `public_product_name` trong `PACK-02 §32.2` | owner + Product Master |
