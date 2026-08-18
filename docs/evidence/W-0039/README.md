# W-0039 — Evidence: Accessibility, i18n & cross-browser QA (`P5-5`)

Ngày: `2026-08-18` · Trạng thái đạt được: `TESTS_PASS` cho phần chạy được; `UI-XBROWSER-03` và visual-regression `NOT_RUN` — xem §5

## 1. Ba lỗi thật, không phải ba checkbox

Slice này tìm ra ba khiếm khuyết đang tồn tại trong console, mỗi cái ảnh hưởng người vận hành thật.

### (a) Số hiển thị sai ký hiệu Việt

`formatRate` dùng `toFixed(1)` → console hiện **`95.5%`** cho operator Việt Nam. `vi-VN` viết phần thập phân bằng **dấu phẩy**.

Đổi sang `Intl.NumberFormat(LOCALE)` sửa **hai** thứ, không phải một:

| | `toFixed(1)` | `Intl` |
| --- | --- | --- |
| Ký hiệu | `54.5%` | `54,5%` |
| Làm tròn `0.5455` | **`54.5`** | **`54,6`** |

Cái thứ hai đáng nói hơn. `54.55` không biểu diễn chính xác được trong nhị phân, nên `toFixed(1)` trả `54.5` — trong khi người làm tròn `54,55` về một chữ số thập phân sẽ viết `54,6`. Con số hiển thị **đã đổi**, và tôi ghi rõ điều đó thay vì lặng lẽ cập nhật assertion.

Đồng thời gộp bản sao: dashboard có hàm `percent()` riêng, giờ uỷ quyền cho `formatRate` để hai màn không trôi thành hai ký hiệu cho cùng một số.

### (b) Chuỗi tiếng Việt nằm trong component

`roles/page.tsx` giữ **9 mô tả màn hình** bằng tiếng Việt ngay trong một `Record`, và `reports/export/route.ts` giữ một thông báo lỗi. Prose nằm trong component là prose **người dịch không bao giờ thấy** và reviewer không bao giờ diff cùng phần còn lại của console.

Cả 10 chuỗi chuyển vào `vi.json`. `UI-I18N-02` quét mọi string literal chứa dấu tiếng Việt trong `src/` và đỏ nếu còn sót.

### (c) Ô boolean chỉ có ký hiệu, không có tên

9 chỗ render `✓` hoặc `—` trần. Trình đọc màn hình đọc ra không gì hữu ích, và operator quét một cột toàn ký hiệu phải tự nhớ dấu nào nghĩa gì.

Đây là **cùng một phản đối với "truyền tin chỉ bằng màu"** (WCAG 1.4.1) trong một bộ trang phục khác — và chính là finding mức High mà `W-0097` đã sửa cho badge, nhưng chưa ai áp cho ô boolean.

`BooleanCell` giữ ký hiệu (đọc nhanh) và mang kèm chữ trong `.sr-only`. Trạng thái thứ ba cũng được phân biệt: **"Chưa ghi nhận" không phải "Không"**.

## 2. Test không cry-wolf

Bản đầu của kiểm "key không dùng" báo **24 key chết**. Tôi kiểm trước khi tuyên bố defect: **cả 24 đều đang được dùng** — `error.*` giải qua ``t(`error.${code}`)`` trong `ErrorAlert`, còn `action.*`/`detail.*` đi vòng dưới dạng `messageKey` trước khi ai đó gọi `t()`.

Nếu tôi báo cáo 24 key chết đó, người đọc tiếp theo sẽ xoá chúng và làm hỏng màn lỗi.

Test cuối cùng hiểu **cả hai hình dạng**, và comment trong test nói rõ vì sao: một checker cry-wolf sẽ bị xoá, và khi đó ta mất luôn phần nó bắt đúng.

## 3. Test cũ phải sửa — và vì sao đó không phải nới lỏng

| Test | Đổi gì | Vì sao không phải nới |
| --- | --- | --- |
| `UT-UI-ROLE-04` | đọc mapping từ **catalogue** thay vì regex trên source component | Ý định giữ nguyên: "một dòng có text không còn trỏ tới màn thật". Prose chuyển chỗ thì kiểm tra **đi theo**, không bị bỏ |
| 7 assertion `%` | `46.2%` → `46,2%`, `54.5%` → `54,6%` | Chúng đang khoá **ký hiệu sai**. Sửa assertion ở đây là sửa cái đang khoá một lỗi |

Không assertion nào bị xoá hoặc làm yếu đi.

## 4. Kiểm chứng

| Lệnh | Kết quả |
| --- | --- |
| `npm test` (admin-ui) | **186/186**, 18 file (+5) |
| `npm run lint` / `typecheck` | 0 |
| `docs-selftest.mjs` | `DOC_CI_TOPOLOGY_PASS` (giờ phủ cả `ui_qa`), `API_DOCS_SELFTEST_PASS` |

Cộng với 44 assertion contrast WCAG từ `W-0097` đọc token thẳng từ `globals.css` cho **cả hai theme** — phần đó đã có và vẫn xanh.

## 5. Cái này KHÔNG chứng minh — hai mục `NOT_RUN`

| Mục §8 | Trạng thái | Vì sao |
| --- | --- | --- |
| `UI-XBROWSER-03` Chrome/Edge/Firefox + tablet | **NOT_RUN** | Pane preview trong môi trường này không composite frame (ghi từ `W-0097`/`W-0102`); không trình duyệt nào lái được. Không thêm một job CI cho việc không chạy được: nó sẽ đỏ vĩnh viễn rồi bị `allow_failure` hoá và thành đồ đạc |
| `UI-VISUAL-04` visual regression | **NOT_RUN** | Cùng lý do, cộng thêm §5 của prompt tự đánh dấu công cụ là `NEED_CONFIRMATION`. **Nửa quan trọng của nó thì đã phủ**: PII masked không phụ thuộc viewport vì HTML do server render — `UT-UI-MASK-*` và e2e đã khẳng định không có số thô trong HTML |
| axe-core | **không dùng** | axe cần DOM thật. Thay bằng kiểm cấu trúc trên **HTML server render thật** cho những điều axe kiểm được ở tầng đó: tên khả truy cập, `lang="vi"`, skip-link. Không gọi nó là axe |

Ba mục này ở phần **chưa làm**, không phải "đã đạt".

Thêm: **`UI-A11Y-01` không thay thế kiểm bằng bàn phím và trình đọc màn hình thật.** Nó khoá được cái máy đọc được từ source và HTML; nó không nói cho bạn biết tab order có hợp lý không. Đó vẫn là việc của một người, và `reviewer-guide.md` không có mục cho nó — một khoảng trống tôi nêu ở đây thay vì lấp bằng một assertion giả.
