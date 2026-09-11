# W-0277 — Để oasdiff thật sự soi cái enum, kèm một lượt CI đỏ có chủ ý

Ngày: 2026-09-11 · Baseline: `main@cc6025b` · Trạng thái: **TESTS_PASS** cục bộ · **CI đỏ có chủ ý**.

Owner chốt ngày 2026-09-11: chấp nhận một lượt `api_contract_diff` đỏ để (a) `oasdiff breaking`
thật sự soi enum `W-0275`, và (b) lấy bản changelog `draft.25 → draft.26` mà máy dev không sinh được.

## 1. Kế hoạch tôi mô tả lúc đầu **sẽ không chạy được**

Tôi đề xuất: trỏ baseline về `draft.25`, `diff -u` sẽ đỏ và in ra changelog. Kiểm lại thứ tự job
trước khi làm thì thấy nó hỏng mục đích:

```
script:
  - generate-oasdiff-changelog.sh …            # dòng 19
  - diff -u docs/… /tmp/…                      # dòng 20  ← đỏ ở đây
  …
  - oasdiff breaking … --fail-on WARN          # dòng 23  ← KHÔNG BAO GIỜ CHẠY
```

GitLab dừng job ở lệnh lỗi đầu tiên. `diff -u` đỏ thì **`oasdiff breaking` không thực thi** — tức
đúng thứ cần kiểm lại bị bỏ qua. Đổi mỗi baseline là đổi một lượt đỏ vô ích.

## 2. Ba thay đổi

**(a) `oasdiff breaking` chạy trước `diff -u`.** Đây không phải mẹo cho lượt này, mà đúng hơn về lâu
dài: breaking là **cổng thực chất**, byte-comparison là **sổ sách**. Một breaking change nên được
báo là *breaking*, không phải là *"changelog của bạn đã cũ"*. Thứ tự cũ khiến một file sổ sách lỗi
thời che mất cổng thật.

**(b) `cat` file sinh ra trước khi so sánh.** Khi `diff -u` đỏ, **log job là nơi duy nhất** bản
changelog tồn tại — `oasdiff` không cài được trên máy dev. In vô điều kiện nên không phụ thuộc việc
diff đỏ hay xanh.

**(c) Baseline IVR về `draft.25`.** Giờ `oasdiff breaking draft.25 → draft.26` **có diff thật để
soi**. Trước đó `W-0275` đẩy baseline lên `draft.26` trong cùng commit làm spec thành `draft.26`, nên
hai file giống hệt nhau và job xanh **rỗng**.

Kèm theo, `docs/api-changelog.md` khai `Baseline = draft.25`, `Current = draft.26`. `CT-CI-12` (vừa
thêm ở `W-0276`) **xanh** — đúng trường hợp nó được thiết kế cho: baseline chậm một release, tài liệu
khai đúng. Lần đầu tính chất đó được dùng thật chứ không chỉ được test.

## 3. File changelog: placeholder, không phải bản giả

`docs/api/changelog/ivr-order-confirmation.md` mang tiêu đề đúng và một thân **ghi rõ nó là
placeholder và job sẽ đỏ vì nó**.

Không viết tay nội dung: job so **byte-for-byte**, nên một bản "trông hợp lý" sẽ vừa sai vừa không
ai phát hiện cho tới lúc nó gây hại. Một placeholder tự khai là sai thì trung thực hơn một bản giả
trông đúng.

## 4. Kết quả mong đợi của lượt CI này

| Bước | Dự kiến |
| --- | --- |
| `oasdiff breaking` IVR | **Đây là câu trả lời cần tìm.** Xanh = enum không breaking; đỏ = có, và phải hoàn tác |
| `oasdiff breaking` callback | xanh (không đụng) |
| `cat /tmp/ivr-order-confirmation.md` | in bản `draft.25 → draft.26` — **commit lại nguyên văn** |
| `diff -u` IVR | **đỏ có chủ ý** — placeholder khác bản sinh |

Job sẽ **fail**, và đó là thiết kế của lượt này.

## 5. Cục bộ

`GATE_SWEEP_PASS 39/39`. Gate cục bộ không chạy `oasdiff` (`selftest-oasdiff.sh` khai
`sweepable: false` vì binary chỉ có trong image), nên chỗ đỏ duy nhất nằm ở job hosted.

Không chạm mã nguồn .NET.

## 6. Bước tiếp theo, cần người

1. Mở job `api_contract_diff`, đọc kết quả `oasdiff breaking` — **đó là thứ chưa ai biết**.
2. Chép phần `cat` vào `docs/api/changelog/ivr-order-confirmation.md`, commit; job xanh lại.
3. Nếu breaking đỏ: hoàn tác enum ở `W-0275`, hoặc đưa nó thành quyết định hợp đồng với Module 3.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
