# W-0214 — Cái bẫy khung giờ, dòng log còn thiếu, và ba nhánh không phải rác

Ngày: 2026-09-07 · Baseline: `main@4bd633c` · Trạng thái: **TESTS_PASS** cho X2 và X3;
**OWNER_DECISION_REQUIRED** cho X1.

Ba mục nhóm X — những việc không có trong bản audit gốc. Hai mục đóng được ngay; mục thứ ba hoá ra
có tiền đề sai và **không nên** thi hành.

---

## X2 — `docker-compose.e2e.yml` vẫn dính bẫy khung giờ ✅

`W-0203` đã sửa đúng cái bẫy này cho profile `LocalMockE2E`, kèm lý do viết thẳng trong file:
*"a rehearsal whose result depends on what time it was started is not a rehearsal"*. Nhưng đó là
harness `tools/dev/Invoke-LocalMockE2E.mjs`. `docker-compose.e2e.yml` có commit cuối `7195ba8` ngày
**20/08** và không set `CallingWindow` gì cả — nên stack smoke chạy ngoài `08:00–21:00` giờ VN vẫn
sẽ FAIL lại y hệt đợt kiểm 06/09.

Đã thêm vào `ivr-worker`:

```yaml
Ivr__Scheduler__CallingWindow__Enabled: "true"
Ivr__Scheduler__CallingWindow__UtcOffsetMinutes: "420"
Ivr__Scheduler__CallingWindow__StartMinuteOfLocalDay: "0"
Ivr__Scheduler__CallingWindow__EndMinuteOfLocalDay: "1440"
```

**Giữ `Enabled=true` và nới ra cả ngày, không tắt.** Tắt gate thì smoke thôi không còn chạy qua đoạn
code ra quyết định nữa — mà đó chính là đoạn cần được phủ. `0..1440` giữ quyết định vẫn chạy và luôn
trả lời "mở".

**Không kết luận thay cho server.** Kết quả local không xoá được lịch sử smoke trên server tại một
baseline khác; chưa đọc config/log triển khai thật thì chưa xác nhận được nguyên nhân tuyệt đối
(Codex F09).

---

## X3 — Worker im lặng khi cửa sổ giờ đóng ✅

`SchedulerJobHost` chỉ log khi `quarantined > 0 || closed > 0 || claimed`. Cửa sổ đóng thì cả ba
đều bằng 0, nên **không một dòng nào** được ghi. Triệu chứng duy nhất nhìn thấy được là job hết hạn
vài giờ sau, không attempt nào — đọc y hệt một dialler hỏng.

Điều làm việc này đáng chú ý: **câu chữ đã có sẵn**. `CallingWindowDecision.Describe()` từ `W-0198`
sinh ra đúng câu *"outside the calling window at HH:mm local; opens …"*. Nhưng
`grep '\.Describe()' src/` cho **0 kết quả** — chỉ một test gọi nó. Thông điệp tồn tại, không ai
phát ra.

Đã sửa:

- `SchedulerRunResult` thêm `DateTimeOffset? CallingWindowOpensAt` (tham số optional, source-
  compatible — gitnexus impact `LOW`, 6 điểm chạm, không process nào gãy).
- `SchedulerRuntime` điền `window.OpensAt` ở đúng nhánh cửa sổ đóng.
- `SchedulerJobHost` log **hai chiều chuyển trạng thái**: `2312` khi đóng (kèm `OpensAt`), `2313`
  khi mở lại.

**Chỉ log lúc đổi trạng thái, không log mỗi vòng.** Loop này quay mỗi `PollIntervalMilliseconds` —
100ms dưới profile `LocalMockE2E` — nên một dòng mỗi vòng sẽ chôn vùi chính cái đêm nó cần giải
thích, dưới sáu trăm dòng giống nhau mỗi phút. Biến trạng thái khởi tạo `null` để **lần đóng đầu
tiên vẫn tự khai báo**.

Ghim bằng hai test:

| Test | Khẳng định |
| --- | --- |
| `UT-SCH-WINDOW-07` | cửa sổ đóng lúc 03:00 → `OpensAt` = 08:00 cùng ngày |
| `UT-SCH-WINDOW-08` | cửa sổ mở lúc 10:00 → `OpensAt` là `null` |

**Chưa đóng, và log không đóng hộ:** task tới ngoài giờ gọi mà confirmation window ngắn hơn khoảng
chờ tới 08:00 thì **luôn** hết hạn không gọi. Log mới làm nó *nhìn thấy được*; nó vẫn là câu phải
chốt với M3.

---

## X1 — Ba nhánh, và tại sao không nên xoá cái nào ⛔

Mục này tôi viết sai và không thi hành.

Bản trước ghi *"ahead 0 → đã merge, **chỉ chờ xóa**"*, trích `CLAUDE.md`: *"Where a stray branch
already exists, merge it into `main` and delete it."* Luật đó nói về **nhánh rác**. Đọc kỹ thì không
nhánh nào ở đây là rác.

| Nhánh | So với `main` | Thực chất |
| --- | --- | --- |
| `codex/w0128-w0129-candidate` | **ahead 1** / behind 80 | **mốc provenance cố ý** của `W-0130` |
| `codex/p03-expand-contract` | ahead 0 / behind 25 | đã merge; dẫn trong evidence `W-0197` |
| `worktree-gd0-fixes` | ahead 0 / behind 22 | đã merge vào `main` tại `ba43605` |

### Nhánh `ahead 1` tồn tại **vì** main không đủ provenance

Tracker `W-0130` ghi lý do: *"`main@2a4f45d` là mixed `save` 98 file nên **không đủ provenance**"*.
`docs/evidence/W-0130/README.md` khai đích danh:

| Branch | `codex/w0128-w0129-candidate` |
| --- | --- |
| Worktree | `Desktop/ivr-w0128-w0129-candidate` |

kèm tree hash. Nói cách khác: nhánh này ra đời **chính xác vì** lịch sử `main` có commit `save` trộn
lẫn — đúng thứ mục A3 của worklist đang than phiền. `ahead 1` là **thiết kế**, không phải nợ.

Xoá nó ⇒ commit unreachable ⇒ **đứt chuỗi bằng chứng mà `W-0130` sinh ra để giữ**.

### Hai nhánh còn lại

Xoá ref không mất commit nào — đều reachable từ `main`. Nhưng cả hai đang được tài liệu dẫn tên, và
**cả ba đều đang checkout trong worktree**: muốn xoá nhánh phải gỡ worktree trước, tức **xoá thư
mục**, mà hai trong số đó nằm **ngoài repo này** (`Desktop/ivr-p03-expand-contract`,
`Desktop/ivr-w0128-w0129-candidate`).

**Tôi không tự xoá thư mục ngoài repo được trỏ tới.** Ba câu hỏi tách rời cho owner nằm ở mục X1 của
worklist.

### Một mâu thuẫn cần owner gỡ

`CLAUDE.md` cấm tuyệt đối nhánh mới và bảo xoá nhánh lạ. `W-0130` lại cần một nhánh tồn tại lâu dài
làm mốc bằng chứng. Hai điều đó không thể cùng đúng mãi. Nếu giữ mốc thì nên **miễn trừ tường minh**
trong `CLAUDE.md`, để mỗi lượt audit sau không báo lại nó là rác — bản audit 07/09 đã báo nhầm một
lần rồi (theo chiều ngược lại: nó khẳng định *"repo chỉ có main"* ở **16** hàng).

---

## Kiểm chứng

```text
dotnet test Ivr.sln                          896/896 PASS, 0 failed, 0 skipped
                                             (Unit 595 · Integration 269 · Contract 24 · Chaos 8)
dotnet test --filter UT-SCH-WINDOW-07|08      2/2 PASS
contract-freeze-verifier.mjs                 CONTRACT_FREEZE=PASS
generate-test-traceability.mjs --check        TEST_TRACEABILITY_CURRENT=555  (554→555)
docs-selftest.mjs                            API_DOCS_SELFTEST_PASS
gitnexus impact SchedulerRunResult            LOW · 6 impacted · 0 process gãy
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1=DRAFT`. Không xoá nhánh, không gỡ worktree,
không đụng repo nào ngoài repo này.
