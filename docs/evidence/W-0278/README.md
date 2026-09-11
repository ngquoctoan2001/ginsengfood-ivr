# W-0278 — Một field hai từ vựng, và một hash trúng số điện thoại

Ngày: 2026-09-11 · Baseline: `main@b5210c4` · Trạng thái: **TESTS_PASS**.

Owner chốt ngày 2026-09-11: `IvrDashboardSimPanel.adapter_mode` trả **sentinel `NONE`** khi không có
channel. Contract `1.0.0-draft.26 → 1.0.0-draft.27`.

## 1. Dữ kiện khiến lựa chọn trở nên dễ

`AdminReadService.cs:650` trả `channels[0].AdapterMode` khi có channel, và `executionMode` khi không.
Điều làm nó thành lỗi rõ ràng chứ không phải sở thích: **`IvrDashboardProjection` đã có
`execution_mode` là field bắt buộc ở cấp trên**, ngay cạnh `sim`.

Nên fallback cũ trả về một giá trị **đã nằm sẵn cách đó hai dòng trong cùng response** — không thêm
thông tin nào, chỉ nhét nghĩa thứ hai vào một field khác. Nó chỉ "chạy được" vì cả hai tập cùng chứa
`MOCK`; một lượt lab ở `LAB_REAL_SIM` chưa provision channel sẽ phát ra `adapter_mode: "LAB_REAL_SIM"`.

Lịch sử dòng đó: có từ commit tạo file (`c85c22f` "save"), **không lý do nào được ghi**. Comment
trong test cho thấy phương án duy nhất từng cân nhắc là *"chuỗi rỗng"*.

## 2. Sentinel mở ra thứ `draft.26` không làm được

`W-0275` **cố ý** để field này mở vì nó mang hai từ vựng. Sau sentinel nó có một, nên đóng được:
`enum [MOCK, VENDOR, ASTERISK_ARI, NONE]` — khác `IvrSimChannel` đúng một giá trị, vì một dòng
channel luôn có adapter còn panel thì có thể không có channel nào.

Enum lập tức có tác dụng: hai assertion trong `AdminReadApiTests` phải chuyển từ chuỗi tự do sang
tập đóng sinh tự động.

## 3. `CT-CI-12` bắt được lỗi đầu tiên của nó

Sau khi bump lên `draft.27`, sweep đỏ:

```
ci-config-selftest: docs/api-changelog.md says the current contract is 1.0.0-draft.26,
                    but the spec is 1.0.0-draft.27.
```

Guard viết ở `W-0276` cho đúng tình huống này, lần đầu bắt thật — nếu không có nó, index lại lệch
thêm một nhịp nữa như ba nguồn đã từng.

## 4. Phát hiện đáng giá nhất: một hash trúng số điện thoại

Re-pin xong, `upstream-session-signoff-validator` đỏ:

```
W0181_VALIDATION_FAILED: document contains a phone-like value
```

Không phải số điện thoại nào cả. Tài liệu handover băm ra

```
4e786b45dd2b49ff1e6b252c73af027d5dbc91588761475f3da930da86cc5cbc
```

chứa **`91588761475` — mười một chữ số liên tiếp**, nằm gọn trong dải 9–15 mà bộ kiểm điện thoại đếm.
Validator screen **toàn bộ document đã serialize**, mà document đó phần lớn là hash được pin.

Nghĩa là: **một release qua được gate này hay không phụ thuộc vào việc SHA-256 của nó có tình cờ chứa
dãy 9 số hay không.** May rủi, không phải đúng sai.

Và nó giải thích một chuyện cũ: đúng lỗi này xuất hiện ở đầu phiên (`W-0267`) và tôi **đã quy cho
agent khác**. Cùng nguyên nhân, khác hash.

### 4.1 Cách sửa, và phần dư được nói ra

Khoét SHA-256 khỏi phép kiểm điện thoại, đúng khuôn `W-0260` khoét ISO date:

```js
const SHA256_HEX = /(?<![0-9a-fA-F])[0-9a-fA-F]{64}(?![0-9a-fA-F])/gu;
```

**Đúng sáu mươi tư ký tự, chốt hai đầu** — 63 hoặc 65 không phải SHA-256 và vẫn bị đếm. Chỉ khoét cho
phép kiểm điện thoại; mọi pattern khác vẫn nhìn giá trị nguyên vẹn.

Bốn ca chứng minh, và đã kiểm chúng đỏ khi gỡ carve-out:

| Ca | Kỳ vọng |
| --- | --- |
| chính hash gây lỗi | **không** bị bắt |
| 63 ký tự hex | vẫn bắt |
| 65 ký tự hex | vẫn bắt |
| hash **kèm** `+84 912 345 678` cùng chuỗi | vẫn bắt |

**Phần dư, nói ra chứ không ngụ ý**: một giá trị cố ý dựng thành đúng 64 ký tự hex có thể giấu số bên
trong. Phép kiểm này tồn tại để chặn bí mật lọt vào evidence bundle **do sơ ý** — không ai vô tình
viết số điện thoại thành một hash 64 ký tự — và một tác giả có ác ý với quyền ghi vào những tài liệu
này có cách đơn giản hơn nhiều.

## 5. Kết quả

```
Unit         670/670
Integration  283/283
Contract      24/24
Chaos          8/8
             ─────────
             985/985
```

`dotnet build Ivr.sln` — 0 warning, 0 error. **`GATE_SWEEP_PASS 39/39`**.

Impact `AdminReadService`: **LOW**, 0 execution flow.

Cascade: 5 chỗ pin tài liệu handover + `dial-token.task_oas_sha256`. Không file nào tôi đổi bị pin
bởi thứ khác.

## 6. Còn lại

- **Trạng thái CI đỏ có chủ ý từ `W-0277`** vẫn nguyên (baseline `draft.25`, changelog là placeholder).
  Owner đã gác chuyện CI; header placeholder nay là `draft.25 vs. draft.27`.
- **`docs/api/changelog/…draft.26-to-draft.27.md`** không có, cùng lý do `W-0275` §6: `oasdiff` chỉ
  nằm trong image CI.

Mock-only evidence: đầy đủ · Lab: NOT_RUN · Real integration: NOT_RUN · Production: NOT_RUN
