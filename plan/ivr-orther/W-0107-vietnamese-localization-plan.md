# W-0107 — Việt hóa toàn diện: giao diện + dữ liệu hiển thị

> **HISTORICAL_PLAN OVERLAY — 2026-08-27:** Hai nhãn trusted-skip trong bảng dưới đã được W-0123
> đổi sang ngữ nghĩa `LEGACY_READ`; enum còn để đọc history nhưng runtime không phát sinh mới.

Trạng thái tài liệu: `PLAN_EXECUTED`
Trạng thái triển khai: `TESTS_PASS` — GĐ 1–8 xong; chờ owner duyệt từ vựng (`OD-L10N-05`). Bằng chứng: `docs/evidence/W-0107/`
Ngày lập: `2026-08-22`
Baseline source đã đọc: `main@f7c9be9` (+ WIP admin-ui chưa commit, 206 file)
Origin: `UNPLANNED` — owner requested

> `NEXT_WORK_ID` trong tracker §2 đang là `W-0106`, nhưng `W-0106` đã được dùng cho
> plan regional-voice-routing. Tài liệu này **đề xuất** `W-0107`. Chưa ghi `START`,
> chưa cấp ID chính thức — cần owner duyệt §10 trước.

---

## 1. Yêu cầu và kết luận rà soát

### 1.1 Yêu cầu từ sếp

1. Việt hóa **toàn bộ giao diện**.
2. Việt hóa **dữ liệu hiển thị trong các bảng** — đầy đủ, không sót bảng nào.
3. Cơ chế phải phủ cả **các bảng/giá trị sẽ phát sinh trong tương lai**.

### 1.2 Kết luận

Yêu cầu **khả thi, rủi ro thấp về mặt code**, nhưng phạm vi thực tế **khác hẳn** cách hiểu
thông thường. Rà soát source cho thấy:

**Giao diện đã Việt hóa gần như trọn vẹn.** `admin-ui/src/i18n/vi.json` có `473` dòng /
~`460` khóa, phủ toàn bộ nav, tiêu đề, nhãn cột, nút, thông báo, mô tả và catalog mã lỗi.
Đã có hàng rào test `UI-I18N-02` chặn chuỗi tiếng Việt lọt vào component.

**Dữ liệu thì chưa Việt hóa một chút nào.** Mọi giá trị enum đổ từ API xuống được render
**thô** dưới dạng `SCREAMING_SNAKE_CASE` tại **~41 điểm** trên **8 màn hình**. Nhân viên
vận hành đang đọc `IVR_NO_ANSWER_FINAL`, `TASK_HELD_ADMIN_REVIEW`, `HELD_LEASE_RECOVERY`
trong bảng — đây mới là phần việc thật của W-0107.

Sáu phát hiện làm thay đổi phạm vi, cần owner đọc kỹ §3 và §10:

| # | Phát hiện | Hệ quả |
| --- | --- | --- |
| **F1** | UI chrome đã xong ~99%; phần còn thiếu chỉ là **2 chuỗi ASCII** (`"fail-closed"`, `aria-label="Governance"`) và **1 placeholder** (`"IVR_CONFIRMED"`) | Phạm vi dịch chuyển từ "dịch giao diện" sang "dịch **dữ liệu**". Ước lượng công sức đảo ngược so với dự đoán ban đầu. |
| **F2** | Test guard `UI-I18N-02` chỉ bắt **dấu tiếng Việt** hardcode. Nó **không** bắt chuỗi tiếng Anh hardcode, và **không** bắt enum thô | Hàng rào hiện tại cho cảm giác an toàn sai. Phải bổ sung guard thứ hai, nếu không W-0107 sẽ trôi ngược ngay sau khi merge. |
| **F3** | Mã enum **đang là định danh vận hành**: bộ lọc gõ tay `IVR_CONFIRMED`, CSV export chứa `PROGRAM,GOLDEN_HOUR,11,6,...`, audit/evidence tham chiếu chéo theo mã | **Không được thay mã bằng tiếng Việt.** Phải dịch **nhãn hiển thị**, giữ **mã** — xem `NT-1` §3. |
| **F4** | `order_state` là enum **mờ, do Order Core sở hữu** (`D-02`), IVR không có quyền và không biết trước tập giá trị | Không được lập từ điển cho `order_state`. Đây là ngoại lệ có chủ đích, không phải thiếu sót — xem `NT-3`. |
| **F5** | `t()` hiện là `t(key: MessageKey): string` — **không có tham số, không có số nhiều**. Chỗ cần chèn số đang lách bằng cách nối chuỗi (`reports.suppressedNotice` kết thúc bằng dấu `:`) | Từ điển enum cần khóa động `enum.<family>.<value>`, mà `MessageKey = keyof typeof vi` sẽ **vỡ type-safety**. Bắt buộc tách API riêng — xem §4.1, §4.2. |
| **F6** | API trả về **văn xuôi tiếng Anh** ở `dependency.detail`, `fail_closed_effect`, `error.message`, và `dtmf.meaning` (`CONFIRM`/`CANCEL`/`NOT_ENABLED`) | Có phần dữ liệu **không** sửa được từ frontend. Cần quyết định `OD-L10N-02` §10. |

---

## 2. Hiện trạng đã xác minh từ source

### 2.1 Lớp giao diện — đã Việt hóa

| Hạng mục | Nguồn | Trạng thái |
| --- | --- | --- |
| Catalog thông điệp | [`vi.json`](../../admin-ui/src/i18n/vi.json) — 473 dòng | ✅ Đầy đủ |
| Hàm dịch | [`lib/i18n/index.ts:13`](../../admin-ui/src/lib/i18n/index.ts#L13) | ✅ Typed, 1 locale |
| Định dạng số/tiền/ngày | `formatNumber` / `formatCurrencyVnd` / `formatDateTime`, pin `vi-VN` + `Asia/Ho_Chi_Minh` | ✅ Đúng chuẩn VN |
| Tỉ lệ phần trăm | [`lib/analytics/format.ts`](../../admin-ui/src/lib/analytics/format.ts) — `formatRate(0.955) === "95,5%"` | ✅ Dấu phẩy thập phân |
| Thuộc tính `lang` | [`app/layout.tsx`](../../admin-ui/src/app/layout.tsx) — `lang="vi"` | ✅ Có test khóa |
| Mã lỗi | 18 khóa `error.IVR_*` trong `vi.json` | ✅ Đầy đủ |

**Sót lại đúng 3 điểm:**

| Vị trí | Chuỗi | Ghi chú |
| --- | --- | --- |
| [`DependencyBadge.tsx:38`](../../admin-ui/src/components/data/DependencyBadge.tsx#L38) | `fail-closed` | Thuật ngữ kỹ thuật — cân nhắc giữ nguyên, xem `OD-L10N-03` |
| [`GovernanceNotice.tsx:19`](../../admin-ui/src/components/shell/GovernanceNotice.tsx#L19) | `aria-label="Governance"` | Screen-reader đọc tiếng Anh trong ngữ cảnh tiếng Việt — **lỗi a11y thật** |
| [`ReportFilters.tsx:57`](../../admin-ui/src/app/%28console%29/reports/ReportFilters.tsx#L57) | `placeholder="IVR_CONFIRMED"` | Sẽ tự biến mất khi đổi sang `<select>` ở §4.4 |

### 2.2 Lớp dữ liệu — chưa Việt hóa (phần việc chính)

41 điểm render enum thô, đã xác minh từng điểm.

> Bảng dưới định vị bằng **khóa cột / đường dẫn trường**, không bằng số dòng. Worktree
> hiện có 206 file WIP chưa commit và đang bị sửa đồng thời (xem `A-0319`), nên số dòng
> trôi giữa hai lần đọc. Khóa cột thì không.

| Màn hình | File | Định vị | Trường render thô |
| --- | --- | --- | --- |
| Nhật ký cuộc gọi | [`calls/page.tsx`](../../admin-ui/src/app/%28console%29/calls/page.tsx) | `CALL_COLUMNS` → `program`, `status`, `queueStatus`, `result` | `program_type`, `status`, `queue_status`, `result_type` |
| Chi tiết cuộc gọi | [`calls/[ivrCallJobId]/page.tsx`](../../admin-ui/src/app/%28console%29/calls/[ivrCallJobId]/page.tsx) | `DescriptionList` phần đầu; `attempts`/`results`/`callbacks`/`technical`/`review` blocks; `SELLABLE_COLUMNS` → `decision` | `program_type`, `order_state`, `status`, `queue_status`, `eligibility_decision`, `blocked_reasons[]`, `attempt.status`, `disposition`, `result_type`, `recommended_core_action`, `result_state`, `delivery_status`, `exception_type`, `review.reason`, `review.status`, `resolution`, `sellable.decision` |
| Chờ duyệt | [`review/page.tsx`](../../admin-ui/src/app/%28console%29/review/page.tsx) | cột `source`, `result`, `reason`, `status` | `source_type`, `result_type`, `reason`, `status` |
| Tổng quan | [`dashboard/page.tsx`](../../admin-ui/src/app/%28console%29/dashboard/page.tsx) | cột `status` (SIM), `scope`, `shortageReason`; `MetricGrid` | `sim.status`, `incident.scope`, `shortage_reason`, `adapter_mode`, `execution_mode` |
| Trạng thái tích hợp | [`integration/page.tsx`](../../admin-ui/src/app/%28console%29/integration/page.tsx) | `DEPENDENCY_COLUMNS`, `EVENT_COLUMNS` | `dependency`, `state`, `detail`, `fail_closed_effect`, `event.source`, `event.effect` |
| Cấu hình kịch bản | [`config/page.tsx`](../../admin-ui/src/app/%28console%29/config/page.tsx) | cột `status`, `approvals`, `missing`, `meaning` | `version.status`, `approval_type[]`, `missing_approvals[]`, `dtmf.meaning` |
| Báo cáo | [`BreakdownTable.tsx`](../../admin-ui/src/components/reports/BreakdownTable.tsx) | cột `key` | `row.key` (giá trị của chiều đang phân tích) |
| Tài khoản | `accounts/page.tsx` (màn hình account đã bị `W-0128` gỡ; identity thuộc Module 3) | `ACCOUNT_COLUMNS` → `role`, `status` | `role`, `status` |

### 2.3 Lớp backend

| Nguồn | Nội dung tiếng Anh | Đường đi vào UI |
| --- | --- | --- |
| [`IvrErrors.cs`](../../src/Ivr.Domain/Errors/IvrErrors.cs) | `"Authentication is required."`, `"The caller is not permitted."`, … | `error.message` → `ErrorAlert`. **Đã có lối thoát**: UI ưu tiên `t("error.<CODE>")`, chỉ rơi về `message` khi mã lạ |
| [`AdminConfigReadService.cs:103`](../../src/Ivr.Api/Application/AdminConfigReadService.cs#L103) | `DtmfKeyView("1","CONFIRM",true)`, `("0","CANCEL")`, `("9","NOT_ENABLED")` | Cột "Ý nghĩa" màn Cấu hình |
| `IvrDependencyStatus.detail` / `.fail_closed_effect` | Văn xuôi tự do do service sinh | 2 cột màn Trạng thái tích hợp |
| `IvrFailClosedEvent.effect` | Văn xuôi tự do | Bảng sự kiện fail-closed |

### 2.4 Lớp cơ sở dữ liệu

- **22 bảng** khai trong [`PersistenceModelConfiguration.cs`](../../src/Ivr.Infrastructure/Persistence/PersistenceModelConfiguration.cs).
- Enum lưu dạng `string`, **chỉ 6 bảng** có `CHECK` constraint chốt tập giá trị:
  `role`, `account.status`, `intake_outbox.status`, `execution_mode`, `script.status`, `approval_type`.
- **16 bảng còn lại không có ràng buộc tập giá trị ở DB** ⇒ không thể lấy DB làm nguồn
  sự thật duy nhất cho từ điển. Đây là lý do §6 phải quét **ba nguồn**, không phải một.

### 2.5 Hàng rào test hiện có và lỗ hổng

[`tests/unit/i18n-a11y.test.ts`](../../admin-ui/tests/unit/i18n-a11y.test.ts):

| Test | Bắt được | **Không** bắt được |
| --- | --- | --- |
| `UI-I18N-02` #1 | Chuỗi có **dấu tiếng Việt** hardcode trong `.ts`/`.tsx` | Chuỗi **tiếng Anh** hardcode; enum thô |
| `UI-I18N-02` #2 | Khóa `vi.json` chết (không ai dùng) | Khóa **thiếu** cho giá trị mới |
| `UI-I18N-02` #3 | Sai định dạng số/tiền/ngày/tỉ lệ | — |
| `UI-A11Y-01` | Glyph `✓` không có tên hỗ trợ tiếp cận | `aria-label` tiếng Anh |

**Mức phụ thuộc của test vào enum thô:** 175 lần xuất hiện trên 16 file, nhưng chỉ
**~18 lần là assertion trên text đã render** — phần còn lại là fixture. Khối lượng sửa test
nhỏ hơn nhiều so với con số 175 gợi ý.

Một assertion đặc biệt quan trọng:
`reports-screen.test.ts:313` (đã hợp nhất vào `admin-ui/tests/e2e/console-screens.test.ts`) khóa
nội dung CSV `PROGRAM,GOLDEN_HOUR,11,6,0.5455,0.8462`. Test này **phải giữ nguyên** —
nó chính là bằng chứng cho `NT-5` §3.

---

## 3. Nguyên tắc thiết kế

Năm nguyên tắc dưới đây là phần cần owner đọc kỹ nhất. Chúng quyết định hình dạng
của toàn bộ phần còn lại.

### NT-1 — Dịch **nhãn hiển thị**, không dịch **mã**

Mã enum đang gánh ba vai trò ngoài việc hiển thị: khóa lọc, cột CSV, và tham chiếu chéo
audit/evidence. Thay `IVR_CONFIRMED` bằng `Khách đã xác nhận` trong dữ liệu sẽ phá cả ba.

Quy tắc: mỗi giá trị hiển thị thành **nhãn tiếng Việt**, mã gốc đi kèm ở vị trí phụ
(`title` tooltip + `sr-only`, hoặc dòng mono nhỏ dưới nhãn ở màn chi tiết).

```
Trước:  IVR_NO_ANSWER_FINAL
Sau:    Không nghe máy — đã hết lượt     ← nhãn, cỡ chữ thường
        IVR_NO_ANSWER_FINAL              ← mã, mono, mờ, có thể copy
```

Nhân viên mới đọc được nhãn; nhân viên kỳ cựu và người dò audit vẫn thấy mã. Không ai
mất gì.

### NT-2 — Một từ điển duy nhất, ở frontend

Từ điển enum đặt tại `admin-ui/src/i18n/enums.vi.json`, tách khỏi `vi.json`.

Lý do tách: `vi.json` là **chuỗi giao diện** — người viết là designer/BA, khóa do dev đặt.
`enums.vi.json` là **từ vựng nghiệp vụ** — người duyệt phải là owner vận hành, và nội dung
của nó bị ràng buộc bởi contract. Trộn hai thứ vào một file sẽ làm test coverage ở §6
không phân biệt được "khóa giao diện chết" với "giá trị enum chưa dịch".

Không đưa từ điển lên backend. Lý do ở `OD-L10N-02` §10.

### NT-3 — `order_state` là ngoại lệ có chủ đích

`order_state` do Order Core sở hữu (`D-02`), khai trong OpenAPI là
`type: string, description: Opaque enum owned by Order Core`. IVR **không** biết tập giá trị
và **không** được đoán.

Xử lý: render nguyên văn, kèm chú thích đã có sẵn trong `vi.json`
(`detail.orderState` = "Trạng thái đơn (do Order Core sở hữu)"). Test coverage §6 phải
**miễn trừ** trường này một cách tường minh, kèm comment giải thích — nếu không, người sau
sẽ tưởng là bug và "sửa" bằng cách bịa từ điển.

### NT-4 — Giá trị lạ phải **hiện rõ**, không được im lặng

Khi API trả về giá trị chưa có trong từ điển, tuyệt đối không fallback im lặng thành chuỗi
rỗng hay dấu gạch. Hiển thị:

```
⚠ HELD_NEW_THING_2027
```

mã gốc + dấu hiệu chưa dịch, và ghi một dòng `console.warn` ở dev. Màn hình vận hành
báo "tôi không biết cái này" thì tốt hơn nhiều so với một ô trống mà người trực tưởng là
"không có dữ liệu".

Đây là điều kiện tiên quyết để §6 hoạt động: cơ chế phủ tương lai chỉ có giá trị nếu lỗi
**nhìn thấy được** ở runtime chứ không chỉ ở CI.

### NT-5 — CSV, bộ lọc, audit giữ **mã gốc**

- CSV export: cột và dòng do server sinh, đã qua k-anonymity và đã ghi audit
  ([`export/route.ts`](../../admin-ui/src/app/%28console%29/reports/export/route.ts) không thêm gì).
  **Không dịch.** Người nhận file cần mã để đối chiếu với hệ thống khác.
- Bộ lọc: giá trị gửi lên API là mã; chỉ **nhãn trong dropdown** là tiếng Việt.
- Audit/evidence: không đụng.

---

## 4. Kiến trúc đề xuất

### 4.1 Nâng cấp `t()` — thêm tham số

Hiện tại `t()` không nhận tham số, nên các chỗ cần chèn số đang nối chuỗi thủ công.
Việt hóa dữ liệu sẽ làm nhu cầu này tăng (ví dụ: "Đã ẩn 3 nhóm dưới ngưỡng").

```ts
// admin-ui/src/lib/i18n/index.ts
export function t(key: MessageKey, params?: Readonly<Record<string, string | number>>): string {
  const template = vi[key];
  if (params === undefined) return template;
  return template.replace(/\{(\w+)\}/gu, (whole, name) =>
    Object.hasOwn(params, name) ? String(params[name]) : whole,
  );
}
```

Giữ nguyên chữ ký cũ cho mọi lời gọi hiện có — `params` là optional, không có lời gọi nào
phải sửa. Placeholder không khớp thì để nguyên `{name}` chứ không xoá, để lỗi hiện ra.

**Không** thêm số nhiều (plural rules): tiếng Việt không biến đổi theo số, thêm vào chỉ
làm phức tạp mà không giải quyết vấn đề gì.

### 4.2 Từ điển enum — cấu trúc và API

```
admin-ui/src/i18n/enums.vi.json      ← từ điển, owner vận hành duyệt nội dung
admin-ui/src/lib/i18n/enum.ts        ← API tra cứu, typed
```

```jsonc
{
  "resultType": {
    "IVR_CONFIRMED": "Khách đã xác nhận",
    "IVR_CUSTOMER_CANCELLED": "Khách huỷ đơn"
  },
  "queueStatus": {
    "QUEUED": "Chờ gọi"
  }
}
```

```ts
// lib/i18n/enum.ts
import enums from "@/i18n/enums.vi.json";

export type EnumFamily = keyof typeof enums;

export interface EnumLabel {
  readonly label: string;   // nhãn tiếng Việt, hoặc chính mã khi chưa dịch
  readonly code: string;    // luôn là mã gốc
  readonly known: boolean;  // false ⇒ UI hiển thị dấu cảnh báo (NT-4)
}

export function tEnum(family: EnumFamily, value: string | undefined): EnumLabel | null {
  if (value === undefined || value === "") return null;
  const table = enums[family] as Readonly<Record<string, string>>;
  const label = table[value];
  if (label === undefined) {
    if (process.env.NODE_ENV !== "production") {
      console.warn(`[i18n] enum chua dich: ${family}.${value}`);
    }
    return { label: value, code: value, known: false };
  }
  return { label, code: value, known: true };
}
```

Tách khỏi `t()` chứ không mở rộng `t()` là có chủ đích: `MessageKey = keyof typeof vi` cho
compile-time check trên khóa giao diện. Nếu nhét enum vào cùng bảng, mọi lời gọi động
``t(`enum.${family}.${value}`)`` sẽ mất kiểm tra kiểu — đúng thứ mà comment trong
[`lib/i18n/index.ts:4`](../../admin-ui/src/lib/i18n/index.ts#L4) nói là lý do chọn 1 locale.

### 4.3 Component `<EnumLabel>`

Một component duy nhất, dùng ở cả 41 điểm:

```tsx
// components/data/EnumLabel.tsx
export function EnumLabel({ family, value, showCode = false }: EnumLabelProps) {
  const resolved = tEnum(family, value);
  if (resolved === null) return <>—</>;
  return (
    <span title={resolved.code} data-enum-code={resolved.code}>
      {resolved.known ? null : <span aria-hidden="true">⚠ </span>}
      {resolved.label}
      {showCode ? <span className={styles.code}>{resolved.code}</span> : null}
      <span className="sr-only"> ({resolved.code})</span>
    </span>
  );
}
```

`showCode` bật ở **màn chi tiết** (nơi có chỗ và người dùng cần mã), tắt ở **bảng danh sách**
(nơi cột hẹp). `data-enum-code` giữ cho E2E test tiếp tục assert theo mã, không phải sửa
theo từng lần đổi câu chữ.

### 4.4 Bộ lọc: ô text → dropdown

[`CallLogFilters.tsx`](../../admin-ui/src/app/%28console%29/calls/CallLogFilters.tsx) hiện bắt
nhân viên **gõ tay** `status`, `queue_status`, `result_type` vào ô mono. Gõ sai một ký tự thì
ra bảng rỗng mà không có lời giải thích nào.

Đổi cả ba sang `<SelectField>` lấy option từ chính từ điển enum. Đây vừa là việc Việt hóa,
vừa sửa một lỗi khả dụng có thật.

`PROGRAMS` đang hardcode nhãn trùng mã
([`CallLogFilters.tsx:25`](../../admin-ui/src/app/%28console%29/calls/CallLogFilters.tsx#L25))
cũng chuyển sang từ điển.

---

## 5. Từ điển dữ liệu — bản đề xuất

Bản dưới đây trích từ source đã xác minh. Cột "Nguồn" ghi nơi chốt tập giá trị.
**Owner vận hành cần duyệt cột "Nhãn tiếng Việt"** — đây là từ vựng nhân viên sẽ đọc hằng ngày.

### 5.1 `resultType` — Loại kết quả (11 giá trị)

Nguồn: [`ivr-order-confirmation.v1.yaml:1472`](../../specs/api/openapi/ivr-order-confirmation.v1.yaml#L1472) · [`CallResult.cs:3`](../../src/Ivr.Domain/Confirmation/CallResult.cs#L3)

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `IVR_CONFIRMED` | Khách đã xác nhận |
| `IVR_CUSTOMER_CANCELLED` | Khách huỷ đơn |
| `IVR_NO_ANSWER_ATTEMPT` | Không nghe máy — còn lượt gọi |
| `IVR_NO_ANSWER_FINAL` | Không nghe máy — đã hết lượt |
| `IVR_CONFIRMATION_WINDOW_EXPIRED` | Hết hạn cửa sổ xác nhận |
| `IVR_INVALID_PHONE_FINAL` | Số điện thoại không dùng được |
| `IVR_WRONG_INPUT` | Khách bấm sai phím |
| `IVR_TECHNICAL_EXCEPTION` | Lỗi kỹ thuật |
| `IVR_CAPACITY_EXCEPTION` | Không đủ năng lực gọi |
| `IVR_OPERATIONAL_BLOCKED` | Bị chặn do vận hành |
| `IVR_POLICY_BLOCKED` | Bị chặn do chính sách |

### 5.2 `eligibilityDecision` — Kết luận eligibility (12 giá trị)

Nguồn: [`ivr-order-confirmation.v1.yaml:1300`](../../specs/api/openapi/ivr-order-confirmation.v1.yaml#L1300)

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `TASK_ACCEPTED_CALL_JOB_CREATED` | Đã nhận — đã tạo lệnh gọi |
| `TASK_ACCEPTED_DRY_RUN_ONLY` | Đã nhận — chỉ chạy thử, không gọi thật |
| `TASK_SKIPPED_TRUSTED_CUSTOMER` | Lịch sử — đã bỏ qua theo chính sách cũ (không còn phát sinh) |
| `TASK_REJECTED_NOT_OFFICIAL_ORDER` | Từ chối — không phải đơn chính thức |
| `TASK_REJECTED_STATE_NOT_CALLABLE` | Từ chối — trạng thái đơn không cho gọi |
| `TASK_REJECTED_POLICY_MISMATCH` | Từ chối — sai chính sách gọi lại |
| `TASK_REJECTED_CONTACT_INVALID` | Từ chối — thông tin liên hệ không hợp lệ |
| `TASK_REJECTED_SCRIPT_NOT_APPROVED` | Từ chối — kịch bản chưa được duyệt |
| `TASK_REJECTED_INVALID_TRACE` | Từ chối — thiếu hoặc sai correlation id |
| `TASK_BLOCKED_OPERATIONAL` | Bị chặn — có blocker vận hành |
| `TASK_HELD_ADMIN_REVIEW` | Giữ lại — chờ quản trị duyệt |
| `TASK_HELD_POLICY_MISSING` | Giữ lại — thiếu chính sách gọi |

### 5.3 `recommendedCoreAction` — Đề xuất cho Order Core (7 giá trị)

Nguồn: [`ResultRepository.cs:438`](../../src/Ivr.Infrastructure/Repositories/ResultRepository.cs#L438) (dạng lưu trong DB)

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `REVALIDATE_AND_CONFIRM_ORDER` | Đề nghị Core kiểm lại rồi xác nhận đơn |
| `REVALIDATE_AND_CANCEL_CUSTOMER_REQUEST` | Đề nghị Core kiểm lại rồi huỷ theo yêu cầu khách |
| `NO_STATE_CHANGE_WAIT_FOR_TIMEOUT` | Không đổi trạng thái — chờ hết hạn |
| `REVALIDATE_AND_EXPIRE_CONFIRMATION` | Đề nghị Core kiểm lại rồi cho hết hạn xác nhận |
| `REVALIDATE_AND_HOLD_ADMIN_REVIEW` | Đề nghị Core kiểm lại rồi giữ chờ duyệt |
| `IGNORE_STALE_CALLBACK` | Bỏ qua — callback đã cũ |
| `BLOCK_DUE_TO_OPERATIONAL_CONSTRAINT` | Chặn do ràng buộc vận hành |

> ⚠ Lưu ý: [`TargetV1ContractMapper.cs:208`](../../src/Ivr.Infrastructure/Contracts/TargetV1ContractMapper.cs#L208)
> map cùng enum sang dạng có tiền tố `CORE_` khi gửi callback ra ngoài. Đó là **contract
> với Sales**, không phải giá trị console đọc. Từ điển chỉ phủ dạng lưu trong DB.
> Giai đoạn 1 §7 phải xác nhận lại điểm này bằng dữ liệu thật.

### 5.4 `queueStatus` / `jobStatus` — Trạng thái hàng đợi và job

Nguồn: grep write-site trong `src/Ivr.Infrastructure/Repositories/`, `Scheduling/`.
**Tập giá trị này chưa được chốt ở một nơi duy nhất** — xem cảnh báo cuối §5.

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `QUEUED` | Chờ gọi |
| `READY_FOR_SCHEDULER` | Sẵn sàng điều phối |
| `LEASED` | Đã nhận lượt điều phối |
| `LEASED_PENDING_DISPATCH` | Đã nhận lượt — chờ quay số |
| `DISPATCH_LEASED` | Đang giữ lượt quay số |
| `ACTIVE_CALL` | Đang trong cuộc gọi |
| `DISPOSITION_PENDING_NORMALIZATION` | Chờ chuẩn hoá kết quả |
| `PROVIDER_EVENT_PENDING_NORMALIZATION` | Chờ chuẩn hoá sự kiện nhà mạng |
| `RESULT_READY_FOR_CALLBACK` | Kết quả sẵn sàng gửi Core |
| `HELD_MOCK` | Giữ ở chế độ MOCK |
| `HELD_ADMIN_REVIEW` | Giữ — chờ quản trị duyệt |
| `HELD_ELIGIBILITY` | Giữ — chờ xét eligibility |
| `HELD_CAPACITY` | Giữ — thiếu năng lực |
| `HELD_CALLBACK` | Giữ — chờ callback |
| `HELD_TECHNICAL_REVIEW` | Giữ — chờ rà lỗi kỹ thuật |
| `HELD_NORMALIZATION` | Giữ — chờ chuẩn hoá |
| `HELD_LEASE_RECOVERY` | Giữ — đang khôi phục lượt |
| `CAPACITY_HELD` | Bị giữ do năng lực |
| `CAPACITY_MISSED` | Trễ deadline do năng lực |
| `CLOSED_CAPACITY` | Đóng do thiếu năng lực |
| `BLOCKED` | Bị chặn |
| `SKIPPED` | Đã bỏ qua |
| `OPEN` | Đang mở |
| `RECOVERY_REQUIRED` | Cần khôi phục |

### 5.5 `disposition` — Phân loại cuộc gọi từ nhà mạng (12 giá trị)

Nguồn: [`PostgresTelephonyDispatchStore.cs:306`](../../src/Ivr.Infrastructure/Telephony/PostgresTelephonyDispatchStore.cs#L306)
— **không phải** tên member của `SimProviderDisposition`.

> **Sửa lỗi (2026-08-28).** Bản đầu của mục này lấy nguồn là
> [`DispositionMapper.cs:71`](../../src/Ivr.Domain/Confirmation/DispositionMapper.cs#L71) và key
> theo tên member C# (`Answered`, `RingTimeout`, …). Nhưng cột `ivr_call_attempts.disposition`
> được ghi bằng `disposition.ToString().ToUpperInvariant()`, nên giá trị API thật sự trả về là
> `ANSWERED`, `RINGTIMEOUT` — viết hoa và **không** chèn dấu gạch dưới. Hệ quả: cả 11 nhãn đều
> không bao giờ khớp, mọi dòng attempt trên màn hình chi tiết cuộc gọi hiện ⚠ + mã thô.
>
> Vòng quét spec không bắt được vì `disposition` khai báo là `string` mở trong OpenAPI, và
> `enum-coverage.test.ts` khi đó chưa gọi tên family này. Nay đã có `dispositionLiterals()` áp
> đúng phép biến đổi đó thay vì chép tay danh sách — xem mục 6.

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `ANSWERED` | Khách bắt máy |
| `RINGTIMEOUT` | Đổ chuông không ai nghe |
| `BUSY` | Máy bận |
| `REJECTED` | Khách từ chối cuộc gọi |
| `UNREACHABLE` | Không liên lạc được |
| `INVALIDDESTINATION` | Số không tồn tại |
| `DROPPED` | Rớt cuộc gọi |
| `NETWORKERROR` | Lỗi mạng |
| `SIMERROR` | Lỗi SIM |
| `AUDIOERROR` | Lỗi âm thanh |
| `DTMFERROR` | Lỗi nhận phím bấm |
| `CAPACITY_EXCEPTION` | Không còn kênh gọi |

`CAPACITY_EXCEPTION` do [`ResultRepository.cs:327`](../../src/Ivr.Infrastructure/Repositories/ResultRepository.cs#L327)
ghi đè trên nhánh capacity; nó không phải một `SimProviderDisposition` ở bất kỳ dạng chữ nào.

### 5.6 `resultReason` — Lý do kết quả (11 giá trị)

Nguồn: [`DispositionMapper.cs`](../../src/Ivr.Domain/Confirmation/DispositionMapper.cs) (chuỗi truyền vào `NormalizedResult.Reason`)

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `CUSTOMER_PRESSED_1` | Khách bấm phím 1 (đồng ý) |
| `CUSTOMER_PRESSED_0` | Khách bấm phím 0 (huỷ) |
| `ANSWERED_NO_INPUT` | Bắt máy nhưng không bấm phím |
| `UNSUPPORTED_DTMF_KEY` | Bấm phím không hợp lệ |
| `WRONG_INPUT_MAX_ATTEMPTS` | Bấm sai đến hết lượt gọi |
| `RING_TIMEOUT` | Đổ chuông hết giờ |
| `BUSY` | Máy bận |
| `REJECTED_REVIEW_REQUIRED` | Khách từ chối — cần người xem lại |
| `UNREACHABLE` | Không liên lạc được |
| `INVALID_DESTINATION` | Số không tồn tại |
| `CAPACITY_UNAVAILABLE` | Không còn kênh gọi |

### 5.7 `technicalExceptionType` — Loại lỗi kỹ thuật (6 giá trị)

Nguồn: [`DispositionMapper.cs:193`](../../src/Ivr.Domain/Confirmation/DispositionMapper.cs#L193)

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `PROVIDER_DROPPED` | Nhà mạng ngắt cuộc gọi |
| `NETWORK_ERROR` | Lỗi mạng |
| `SIM_ERROR` | Lỗi SIM |
| `AUDIO_ERROR` | Lỗi âm thanh |
| `DTMF_ERROR` | Lỗi nhận phím bấm |
| `UNMAPPED_PROVIDER_DISPOSITION` | Nhà mạng trả mã lạ, chưa ánh xạ được |

> `NormalizeTechnicalCode()` cho phép nhà mạng trả **mã tuỳ ý** (chuẩn hoá về `A-Z0-9_`,
> tối đa 80 ký tự). Đây là **họ enum mở** — bắt buộc phải có `NT-4`, không thể liệt kê hết.

### 5.8 `reviewSourceType` — Nguồn mục chờ duyệt (4 giá trị)

Nguồn: grep `SourceType = "…"` trong `src/`

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `IVR_CALL_RESULT` | Kết quả cuộc gọi |
| `IVR_RESULT_CALLBACK` | Callback gửi Core |
| `ELIGIBILITY_DECISION` | Quyết định eligibility |
| `IVR_OPTOUT_PROPOSAL` | Đề xuất chặn gọi (opt-out) |

### 5.9 `reviewReason` — Lý do vào hàng chờ duyệt

> **Sửa lỗi (2026-08-28).** Tiêu đề cũ của mục này là "(họ `CALLBACK_*` / `CAPACITY_*`)", và
> bảng dưới đây đúng là chỉ liệt kê hai họ đó. Nhưng `ivr_review_items.reason` là **hợp của năm
> nguồn ghi**, không phải một taxonomy:
>
> | Nơi ghi | Giá trị đưa vào `Reason` |
> | --- | --- |
> | [`CallbackOutboxRepository.cs:266`](../../src/Ivr.Infrastructure/Persistence/Outbox/CallbackOutboxRepository.cs#L266) | `CoreResponseCode ?? LastError ?? DeliveryStatus` — họ `CALLBACK_*` |
> | [`PostgresSchedulerStore.cs:450`](../../src/Ivr.Infrastructure/Scheduling/PostgresSchedulerStore.cs#L450) | `IVR_CAPACITY_EXCEPTION` |
> | [`ResultRepository.cs:194`](../../src/Ivr.Infrastructure/Repositories/ResultRepository.cs#L194) | `NormalizedResult.Reason` — tập `resultReason` **hoặc** một mã kỹ thuật |
> | [`EligibilityRepository.cs:270`](../../src/Ivr.Infrastructure/Repositories/EligibilityRepository.cs#L270) | `Reasons[0].Code` — toàn bộ `EligibilityReasonCodes` |
> | [`SuppressionProposer.cs:93`](../../src/Ivr.Infrastructure/Crm/SuppressionProposer.cs#L93) | chuỗi ghép `CODE;channel=…;signals=N;admin_confirmed=…` |
>
> Hai nguồn giữa đã được bổ sung vào từ điển (bảng mở rộng bên dưới) và nay có
> `enum-coverage.test.ts` canh.
>
> **Chuỗi ghép của `SuppressionProposer`** đã xử lý ở phía hiển thị, không phải ở từ điển: một
> key từ điển không bao giờ khớp được một chuỗi mang dữ liệu có cấu trúc. `parseReviewReason`
> ([`lib/review/reason.ts`](../../admin-ui/src/lib/review/reason.ts)) tách `CODE` khỏi các đoạn
> `k=v`, còn [`ReviewReason`](../../admin-ui/src/components/data/ReviewReason.tsx) dịch phần mã
> qua `reviewReason` rồi đọc phần bằng chứng thành câu — *"kênh Gọi điện · 3 tín hiệu · chưa có
> admin xác nhận"*. Việc tách nằm ở call site chứ **không** ở `tEnum`: `tEnum` phục vụ hơn ba
> chục family, chỉ riêng cột này mang dữ liệu có cấu trúc, và blast radius của nó là toàn bộ
> module `Ui`. Kèm theo: family mới `suppressionChannel`, và `REVIEW_EFFECT` ở màn Trạng thái
> tích hợp được nới từ `([A-Z_]+)$` sang `(\S.*)$` — regex cũ không khớp chuỗi ghép nên cả dòng
> rơi xuống nhánh in thô.
>
> Trường hợp **duy nhất còn cố ý** rơi vào NT-4: mã kỹ thuật do nhà cung cấp trả là **tập mở** —
> `DispositionMapper.NormalizeTechnicalCode` nhận bất kỳ chuỗi upper-snake ≤80 ký tự nào. Chỉ hai
> mã do chính repo này sinh ra (`ASTERISK_RECORDING_NOT_DISABLED`,
> `ASTERISK_CHANNEL_HEALTH_NOT_READY`) là liệt kê được, và đã có nhãn.

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `CALLBACK_TIMEOUT` | Gửi callback quá hạn |
| `CALLBACK_CIRCUIT_OPEN` | Ngắt mạch bảo vệ đang mở |
| `CALLBACK_TRANSPORT_FAILURE` | Lỗi truyền tải |
| `CALLBACK_TRANSPORT_UNEXPECTED_FAILURE` | Lỗi truyền tải ngoài dự kiến |
| `CALLBACK_AUTH_REJECTED` | Core từ chối xác thực |
| `CALLBACK_PAYLOAD_INVALID` | Nội dung gửi không hợp lệ |
| `CALLBACK_PATH_BODY_MISMATCH` | Đường dẫn và nội dung không khớp |
| `CALLBACK_ACK_INVALID` | Core phản hồi ACK không hợp lệ |
| `CALLBACK_UNPROCESSABLE` | Core không xử lý được |
| `CALLBACK_UNSUPPORTED_RESPONSE` | Core trả phản hồi không hỗ trợ |
| `CALLBACK_RETRYABLE_RESPONSE` | Core báo có thể gửi lại |
| `CALLBACK_ADAPTER_SELECTION_REJECTED` | Không chọn được adapter |
| `CAPACITY_DEADLINE_UNAVAILABLE` | Không xác định được deadline năng lực |
| `CAPACITY_SOURCE_UNAVAILABLE` | Không đọc được nguồn năng lực |

Bổ sung 2026-08-28 — ba nhóm còn thiếu, mỗi nhóm truy được về đúng nơi ghi (xem
[`enums.vi.json`](../../admin-ui/src/i18n/enums.vi.json) để có bảng đầy đủ 65 giá trị):

| Nhóm | Nguồn | Số giá trị |
| --- | --- | --- |
| Lý do kết quả | `ResultRepository` ghi `NormalizedResult.Reason` — trùng tập §5.6 | 11 |
| Lỗi kỹ thuật | trùng tập §5.7, cộng 2 mã Asterisk của repo này | 8 |
| Lý do eligibility | `EligibilityRepository` ghi `Reasons[0].Code` — trùng `EligibilityReasonCodes` | 26 + `TASK_HELD_ADMIN_REVIEW` |
| Đề xuất chặn gọi | `SuppressionProposer` — trùng `OptOutReasonCodes` | 4 |

Cả bốn nhóm nay bị `enum-coverage.test.ts` canh trực tiếp từ phía C#. Bảng này **cố ý phủ dư**:
`HumanReviewRequired` là `false` với khoảng một nửa số giá trị, nhưng mô hình hoá điều kiện đó
đồng nghĩa giữ một bản sao thứ hai của nhánh rẽ trong `DispositionMapper` — một nhãn thừa tốn một
dòng JSON, một nhãn thiếu tốn một operator đang đọc mã thô giữa sự cố.

### 5.10 `sellableDecision` — Khả năng bán theo dòng (4 giá trị)

Nguồn: [`ivr-order-confirmation.v1.yaml:1187`](../../specs/api/openapi/ivr-order-confirmation.v1.yaml#L1187)

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `SELLABLE` | Bán được |
| `NOT_SELLABLE` | Không bán được |
| `BLOCKED` | Bị chặn |
| `UNKNOWN` | Chưa xác định |

### 5.11 `blockedReason` — Lý do bị chặn

| Mã | Nhãn tiếng Việt |
| --- | --- |
| `RECALL_HOLD_ACTIVE` | Đang giữ do thu hồi sản phẩm |
| `SALE_LOCK_ACTIVE` | Đang khoá bán |
| `QUALITY_HOLD_ACTIVE` | Đang giữ do chất lượng |
| `SELLABLE_SNAPSHOT_MISSING` | Thiếu ảnh chụp khả năng bán |
| `SELLABLE_SNAPSHOT_STALE` | Ảnh chụp khả năng bán đã cũ |
| `SELLABLE_STATUS_UNKNOWN` | Chưa rõ khả năng bán |
| `TRUSTED_CUSTOMER_SKIP` | Lịch sử — trusted-skip đã ngừng phát sinh |
| `BLOCKED_BY_CORE` | Order Core chặn khi kiểm lại |

### 5.12 Các họ nhỏ

| Họ | Mã → Nhãn |
| --- | --- |
| `programType` | `GOLDEN_HOUR` → Giờ vàng · `TWENTY_FOUR_SEVEN` → 24/7 |
| `paymentMethod` | `ONLINE` → Thanh toán online · `COD` → Thu hộ khi giao (COD) |
| `executionMode` | `MOCK` → Mô phỏng · `LAB_REAL_SIM` → Lab SIM thật · `PRODUCTION_REAL` → Vận hành thật |
| `scriptStatus` | `DRAFT` → Bản nháp · `IN_REVIEW` → Đang duyệt · `APPROVED` → Đã duyệt · `RETIRED` → Đã ngừng |
| `approvalType` | `MOCK_TEST` → Kiểm thử mô phỏng · `LAB` → Lab · `CONTENT` → Nội dung · `PRIVACY_LEGAL` → Pháp lý & quyền riêng tư |
| `dtmfMeaning` | `CONFIRM` → Xác nhận · `CANCEL` → Huỷ · `NOT_ENABLED` → Chưa mở |
| `dependencyState` | `UP` → Hoạt động · `DOWN` → Mất kết nối · `READY_503` → Sẵn sàng nhưng trả 503 · `NOT_WIRED` → Chưa đấu nối |
| `simStatus` | `IDLE` → Rảnh · `ACTIVE_CALL` → Đang gọi · `RESERVED` → Đã giữ chỗ · `DISABLED` → Đã tắt · `QUARANTINED` → Đang cách ly · `HEALTH_FAILED` → Health check lỗi |
| `deliveryStatus` | `READY` → Sẵn sàng gửi · `SENDING` → Đang gửi · `RETRY_PENDING` → Chờ gửi lại · `SENT` → Đã gửi · `ACKED` → Core đã nhận · `FAILED` → Gửi thất bại · `DEAD_LETTER` → Đưa vào hàng chờ xử lý tay |
| `accountRole` | `Admin` → Quản trị viên · `Operator` → Nhân viên vận hành *(đã có `accounts.admin`/`accounts.operator` trong `vi.json` — cần gộp, tránh trùng)* |
| `accountStatus` | `ACTIVE` → Đang hoạt động · `DISABLED` → Đã vô hiệu hoá · `DELETED` → Đã xoá mềm |
| `intakeOutboxStatus` | `HELD_MOCK` → Giữ ở chế độ MOCK · `READY_FOR_ELIGIBILITY` → Chờ xét eligibility · `PUBLISHED` → Đã phát hành |
| `reviewStatus` | `OPEN` → Đang mở · `RESOLVED` → Đã xử lý |

### 5.13 Đã có sẵn trong `vi.json` — cần **gộp**, không tạo mới

Các họ sau đã được dịch dưới dạng khóa giao diện. Giai đoạn 2 §7 phải **di chuyển** chúng
sang `enums.vi.json` chứ không nhân bản, nếu không sẽ có hai bản dịch lệch nhau:

| Họ | Khóa hiện tại trong `vi.json` |
| --- | --- |
| `warehouseStatus` | `reports.warehouseStatus.{COMPLETE,BACKLOG,MISMATCH,NOT_RUN}` |
| `freshnessStatus` | `reports.freshness.{FRESH,STALE,NO_DATA}` |
| `analyticsDimension` | `reports.dim{ResultType,ScriptVariant,Program}` |
| `bucket` | `reports.bucket{Day,Hour}` |
| `voiceRegion` | `detail.voiceRegion{North,Central,South,Unknown}` |
| `permission` | `roles.screen.IVR_*` (14 khóa) |

> **Cảnh báo về tính đầy đủ của §5:** các tập giá trị trên được dựng bằng cách grep
> write-site trong source. Cách này **không đảm bảo đầy đủ** — một giá trị chỉ xuất hiện
> qua biến, qua migration data, hay do nhà mạng trả về sẽ không lọt vào lưới. Đây chính là
> lý do Giai đoạn 1 §7 phải sinh inventory **tự động từ 3 nguồn**, và tại sao `NT-4` là bắt buộc.

---

## 6. Cơ chế phủ tương lai

Đây là phần trả lời trực tiếp cho vế "**và tương lai sẽ có**" trong yêu cầu. Bốn lớp,
độc lập nhau, hỏng lớp này còn lớp kia.

### 6.1 Lớp 1 — Test đọc OpenAPI (chặn ở CI)

Đi theo đúng khuôn mẫu đã có của
[`tests/unit/contract-drift.test.ts`](../../admin-ui/tests/unit/contract-drift.test.ts),
vốn đã đọc thẳng file YAML và so với `types.ts`.

```ts
// tests/unit/enum-coverage.test.ts
// Với mỗi enum khai trong ivr-order-confirmation.v1.yaml mà console có render,
// mọi giá trị phải có mục trong enums.vi.json.
// Miễn trừ tường minh: order_state (NT-3 — Core sở hữu, tập giá trị mờ).
```

Thêm một enum vào spec mà quên dịch ⇒ **CI đỏ**, kèm tên chính xác giá trị còn thiếu.

### 6.2 Lớp 2 — Test đọc `CHECK` constraint (chặn ở CI)

`PersistenceModelConfiguration.cs` có 6 `CHECK … IN (…)`. Một test .NET đọc model của EF
lúc chạy, trích tập giá trị, và so với `enums.vi.json`.

Ưu điểm so với lớp 1: bắt được các họ **không nằm trong OpenAPI** (`account.status`,
`intake_outbox.status`, `approval_type`).

Hệ quả kèm theo — và đây là phần đáng giá nhất: hiện **16/22 bảng không có ràng buộc tập
giá trị**. Test này tạo động lực bổ sung `CHECK` cho các họ đóng (`queue_status`,
`review.source_type`, `sim.status`), vừa phục vụ Việt hóa vừa siết chất lượng dữ liệu.
Đề xuất tách thành work item riêng — xem `OD-L10N-04`.

### 6.3 Lớp 3 — `NT-4` ở runtime (bắt cái CI không thấy)

Hai lớp trên chỉ phủ **enum đóng, khai báo tĩnh**. Chúng **không** phủ:

- `technical_exception_type` — nhà mạng trả mã tuỳ ý (§5.7)
- `review.reason` — sinh từ code, không có khai báo tập trung
- `order_state` — Core sở hữu (`NT-3`)
- Giá trị legacy còn nằm trong DB từ migration cũ

Với những thứ này, `⚠ + mã gốc` của `NT-4` **là** cơ chế phủ. Bổ sung một bộ đếm gửi lên
được giá trị lạ xuất hiện bao nhiêu lần mà không cần ai báo.

### 6.4 Lớp 4 — Quy trình

Thêm một dòng vào `AGENTS.md` / `CLAUDE.md`: **thêm giá trị enum mới ⇒ cập nhật
`enums.vi.json` trong cùng MR.** Rẻ, và là thứ duy nhất hoạt động khi ba lớp trên bị
người ta cố tình đi vòng.

---

## 7. Kế hoạch thực thi

| GĐ | Nội dung | Đầu ra | Ước lượng |
| --- | --- | --- | --- |
| **0** | Chốt nguyên tắc §3 + trả lời `OD-L10N-01…05` §10 | Quyết định của owner ghi vào tracker | Owner |
| **1** | Sinh inventory enum **tự động** từ 3 nguồn: OpenAPI YAML, EF model, `git grep` write-site. So khớp với dữ liệu thật trong PostgreSQL (`SELECT DISTINCT`) | `docs/evidence/W-0107/enum-inventory.md` — danh sách đầy đủ, có đánh dấu họ mở/đóng | 0,5 ngày |
| **2** | Owner vận hành duyệt bản dịch §5 (đã cập nhật theo GĐ 1) | `enums.vi.json` v1 | 0,5 ngày + owner |
| **3** | Hạ tầng: `t()` có tham số (§4.1), `tEnum()` (§4.2), `<EnumLabel>` (§4.3). **Chưa** đụng màn hình nào | 3 file mới + unit test | 0,5 ngày |
| **4** | Áp `<EnumLabel>` lên 41 điểm §2.2, theo thứ tự: Nhật ký → Chi tiết → Chờ duyệt → Tổng quan → Tích hợp → Cấu hình → Báo cáo → Tài khoản | 8 màn hình | 1,5 ngày |
| **5** | Bộ lọc text → dropdown (§4.4); gộp 6 họ trùng ở §5.13; sửa 3 chuỗi sót ở §2.1 | `CallLogFilters`, `ReportFilters`, `vi.json` gọn lại | 0,5 ngày |
| **6** | Guard: `enum-coverage.test.ts` (§6.1), test EF `CHECK` (§6.2), mở rộng `UI-I18N-02` bắt cả chuỗi tiếng Anh, đếm untranslated (§6.3) | 3 test file + 1 metric | 1 ngày |
| **7** | Sửa ~18 assertion E2E/component sang `data-enum-code`; **giữ nguyên** assertion CSV `reports-screen.test.ts:313` | Suite xanh | 0,5 ngày |
| **8** | Evidence + tracker: `docs/evidence/W-0107/`, ảnh chụp trước/sau 8 màn, ghi Activity | Gói nghiệm thu | 0,5 ngày |

**Tổng: ~5,5 ngày công** + thời gian owner duyệt từ vựng ở GĐ 2.

Thứ tự GĐ 3 → 4 tách riêng là có chủ đích: hạ tầng vào trước, một mình, dễ review và dễ
revert. GĐ 4 sau đó chỉ còn là thay thế cơ học, review nhanh.

### 7.1 Việc **không** làm trong W-0107

Ghi rõ để tránh phình phạm vi:

- **Không** dịch văn xuôi backend (`dependency.detail`, `fail_closed_effect`) — chờ `OD-L10N-02`.
- **Không** dịch `order_state` (`NT-3`).
- **Không** dịch CSV export, audit log, evidence (`NT-5`).
- **Không** đa ngôn ngữ (i18n thật). Console là **tiếng Việt duy nhất** theo `DTS-03`.
  Thêm locale thứ hai là một work item khác, lớn hơn nhiều.
- **Không** dịch nội dung kịch bản gọi — đã là tiếng Việt, thuộc `W-0104`/`W-0106`.
- **Không** thêm `CHECK` constraint cho 16 bảng — đề xuất tách, xem `OD-L10N-04`.

---

## 8. Ảnh hưởng và rủi ro

### 8.1 Blast radius

Theo quy tắc `CLAUDE.md`, phải chạy `gitnexus_impact` trước khi sửa từng symbol ở GĐ 3–5.
Đánh giá sơ bộ:

| Symbol | Rủi ro dự kiến | Ghi chú |
| --- | --- | --- |
| `t()` | **MEDIUM** | ~460 lời gọi. Nhưng `params` optional ⇒ thay đổi **tương thích ngược tuyệt đối**, không lời gọi nào phải sửa. |
| `vi.json` | LOW | Chỉ thêm/di chuyển khóa. Test #2 của `UI-I18N-02` bắt khóa chết ngay. |
| 8 file `page.tsx` | LOW | Thay đổi thuần render, không đụng data fetching hay quyền. |
| `CallLogFilters` | **MEDIUM** | Đổi input type ⇒ đổi hành vi form. Có E2E phủ. |
| Backend | **KHÔNG ĐỘNG** | W-0107 không sửa file `.cs` nào (trừ khi owner duyệt `OD-L10N-02`/`04`). |

> GitNexus index đang ở `3cd7613` trong khi `HEAD` là `f7c9be9` (`A-0302`, `A-0305` ghi nhận
> index stale và lock `.gitnexus/lbug`). **Phải `npx gitnexus analyze` thành công trước GĐ 3.**
> Nếu impact trả `UNKNOWN`, áp fail-closed như `A-0305`: dừng, không đoán.

### 8.2 Rủi ro

| # | Rủi ro | Mức | Giảm thiểu |
| --- | --- | --- | --- |
| R1 | Dịch sai từ vựng nghiệp vụ ⇒ nhân viên hiểu nhầm trạng thái đơn | **CAO** | GĐ 2 bắt buộc owner vận hành duyệt. Không dev nào tự chốt từ vựng. `NT-1` giữ mã bên cạnh nhãn nên vẫn đối chiếu được. |
| R3 | §5 sót giá trị ⇒ ô hiển thị `⚠ MÃ_LẠ` khi lên production | TRUNG BÌNH | Đúng thiết kế (`NT-4`), không phải lỗi. GĐ 1 đối chiếu `SELECT DISTINCT` từ DB thật để giảm thiểu. |
| R4 | Trôi ngược: người sau thêm enum mà quên dịch | TRUNG BÌNH | Chính là §6. Bốn lớp + quy trình. |
| R5 | Ô bảng dài ra, vỡ layout (nhãn tiếng Việt dài hơn mã) | THẤP | `showCode=false` ở bảng danh sách. Kiểm ở 1280px và 1440px trong GĐ 4. |
| R6 | Đổi filter sang dropdown làm mất khả năng lọc giá trị lạ chưa có trong từ điển | THẤP | Giữ một ô "khác…" nhập tay, hoặc dropdown cho phép giá trị tự do. Chốt ở GĐ 5. |

---

## 9. Kiểm thử và nghiệm thu

### 9.1 Test phải thêm

| ID | Nội dung |
| --- | --- |
| `UT-L10N-ENUM-01` | `tEnum()` trả `known=false` + mã gốc cho giá trị lạ; trả `null` cho `undefined`/rỗng |
| `UT-L10N-PARAM-02` | `t()` chèn tham số đúng; placeholder không khớp được giữ nguyên, không bị xoá |
| `UT-L10N-COVER-03` | Mọi enum trong OpenAPI (trừ miễn trừ tường minh) có mục trong `enums.vi.json` (§6.1) |
| `IT-L10N-DBENUM-04` | Mọi `CHECK … IN (…)` trong EF model có mục trong `enums.vi.json` (§6.2) |
| `UT-L10N-NOENG-05` | Mở rộng `UI-I18N-02`: chuỗi tiếng Anh trong prop hiển thị (`label`, `title`, `placeholder`, `aria-label`) là vi phạm |
| `UT-L10N-NODUP-06` | Không giá trị enum nào tồn tại ở **cả** `vi.json` và `enums.vi.json` (chặn R1 dạng lệch bản dịch) |
| `CT-L10N-EXPORT-07` | CSV export **vẫn** chứa mã gốc — khẳng định `NT-5`, khóa `reports-screen.test.ts:313` |

### 9.2 Tiêu chí nghiệm thu

1. 8/8 màn hình không còn `SCREAMING_SNAKE_CASE` ở vị trí nhãn chính — trừ các miễn trừ
   đã ghi ở `NT-3` và §5.7.
2. Mọi giá trị đã dịch vẫn tra cứu được mã gốc qua tooltip hoặc `sr-only`.
3. Thêm một giá trị enum giả vào OpenAPI ⇒ `UT-L10N-COVER-03` **đỏ** (mutation test, theo
   đúng chuẩn `A-0318`/`A-0319` đã lập).
4. Xoá `enums.vi.json` một mục ⇒ màn hiện `⚠ MÃ`, **không** hiện ô trống.
5. CSV export và audit log **không đổi một byte**.
6. Full suite xanh: .NET `650/650`, admin-ui lint + typecheck + test + build.
7. Ảnh chụp trước/sau 8 màn trong `docs/evidence/W-0107/`.

---

## 10. Điểm cần owner quyết

| ID | Câu hỏi | Đề xuất |
| --- | --- | --- |
| `OD-L10N-01` | Hiển thị mã gốc **ở đâu**: chỉ tooltip, hay có dòng mono nhỏ dưới nhãn ở màn chi tiết? | **Tooltip ở bảng danh sách + dòng mono ở màn chi tiết.** Bảng cần gọn, chi tiết cần tra cứu. |
| `OD-L10N-02a` | `fail_closed_effect` + `event.effect` (REVIEW_ITEM) + phần cố định của `detail` | **Làm luôn trong W-0107.** Đã rà lại source: đây **không** phải văn xuôi tự do mà là **hằng số khoá theo mã đã có sẵn**. Không cần đổi contract. Xem §11.1. |
| `OD-L10N-02b` | Phần nội suy của `detail` (`SIM_GATEWAY`, `ORDER_CORE`) + `event.effect` (CAPACITY_INCIDENT) | **Hoãn — và cân nhắc không làm.** Đây là telemetry `key=value`, gần với log hơn là văn UI. Xem §11.1. |
| `OD-L10N-03` | Thuật ngữ kỹ thuật giữ nguyên hay dịch: `fail-closed`, `correlation ID`, `idempotency`, `DTMF`, `SIM`, `adapter`, `webhook`? | **Giữ nguyên.** Đây là từ vựng chung với tài liệu vận hành và log; dịch ra sẽ làm nhân viên không tra cứu được. `vi.json` hiện đã theo hướng này (`"Correlation ID"`, `"Health fail"`). Cần owner xác nhận thành nguyên tắc chính thức. |
| `OD-L10N-04` | Bổ sung `CHECK` constraint cho 16 bảng chưa có, để §6.2 phủ được nhiều hơn? | **Tách work item riêng.** Là cải thiện chất lượng dữ liệu thật sự, nhưng cần migration + kiểm dữ liệu cũ, không thuộc bản chất việc Việt hóa. |
| `OD-L10N-05` | Ai duyệt từ vựng §5, và việc duyệt chặn ở đâu? | **Owner duyệt, nhưng chặn ở lúc merge chứ không chặn lúc build.** GĐ 3–4 không phụ thuộc câu chữ. Chỉ **4 họ** cần đọc kỹ, phần còn lại rà nhanh. Xem §11.2. |

---

## 11. Phân tích sâu hai quyết định chặn

### 11.1 `OD-L10N-02` — đã rà lại source, kết luận ban đầu **sai một nửa**

Bản plan đầu tiên xếp cả ba trường (`detail`, `fail_closed_effect`, `event.effect`) vào
cùng một nhóm "văn xuôi tự do, phải đổi contract, nên hoãn". Đọc kỹ
[`AdminConfigReadService.BuildDependencies`](../../src/Ivr.Api/Application/AdminConfigReadService.cs)
và `BuildFailClosedEvents` cho thấy **ba trường này có bản chất khác hẳn nhau**.

#### Phân loại thật

| Trường | Bản chất thật trong code | Đổi contract? | Kết luận |
| --- | --- | --- | --- |
| `fail_closed_effect` | **6 hằng số**, mỗi dependency đúng 1 câu, không nội suy gì | **Không** | Làm luôn |
| `detail` — `OPS_SELLABLE_GATE`, `CRM_DO_NOT_CALL`, `EVIDENCE_REGISTRY` | **3 hằng số** | **Không** | Làm luôn |
| `detail` — `DIAL_KILL_SWITCH` | **2 hằng số**, chọn theo `state` (đã có trong contract) | **Không** | Làm luôn |
| `event.effect` — `REVIEW_ITEM` | `$"{SourceType}: {Reason}"` — **hai mã ghép**, cả hai đã có trong §5.8 và §5.9 | **Không** | Làm luôn |
| `event.effect` — `CAPACITY_INCIDENT` | `$"{Scope}: new calls held"` / `"…open, dispatch not held"` — biến phân biệt là `hold_new_calls`, **không có trong contract** `IvrFailClosedEvent` | **Có** (hoặc parse chuỗi) | Hoãn |
| `detail` — `SIM_GATEWAY`, `ORDER_CORE` | Telemetry nội suy `provider=…; channels 3/4 enabled`, `provider=…; delivery=…; circuit=…; consecutive_transient_failures=0` | **Có** | Hoãn — và cân nhắc **không làm** |

Điểm mấu chốt: `fail_closed_effect` là **hàm thuần của `dependency`**, mà `dependency`
(`SIM_GATEWAY`, `DIAL_KILL_SWITCH`, `ORDER_CORE`, `OPS_SELLABLE_GATE`, `CRM_DO_NOT_CALL`,
`EVIDENCE_REGISTRY`) **đã là mã**. Frontend chỉ cần thêm một họ
`enums.vi.json → failClosedEffect` khoá theo `dependency` và **bỏ hẳn** chuỗi server trả về.
Không đụng API, không `oasdiff`, không re-pin manifest, không regenerate DTO.

Điều tương tự với `event.effect` của `REVIEW_ITEM`: chuỗi có dạng `^([A-Z_]+): (.+)$`,
tách ra là đúng hai mã mà §5.8/§5.9 **đang xây từ điển sẵn rồi**. Tận dụng lại, không tốn thêm.

#### Phần hoãn: chi phí thật nếu muốn làm

Cổng `CT-CONTRACT-PINNED-08` **hiện đang xanh** — đã kiểm: SHA-256 thực tế của spec
(`98d226b1…`) khớp `contract-manifest.json`, spec ở `1.0.0-draft.12`. (Ghi chú của `A-0319`
về việc cổng này đỏ đã được ai đó xử lý xong.) Nên đổi contract là **khả thi**, không bị chặn.
Nhưng giá phải trả vẫn đủ để tách riêng:

- `IvrDependencyStatus` và `IvrFailClosedEvent` đều khai `additionalProperties: false`
  ⇒ thêm field **bắt buộc** phải sửa schema, không thể lặng lẽ thêm.
- Kéo theo: regenerate `IvrServerModels.g.cs` → re-pin `contract-manifest.json` →
  `oasdiff` `draft.12 → draft.13` → rebuild portal → cập nhật `types.ts` + `contract-drift.test.ts`.
- Đây là chuỗi việc của `@ginsengfood/ivr-contracts`, không phải của người làm UI.

#### Câu hỏi thật với `detail`: **có nên dịch không?**

`provider=MOCK; channels 3/4 enabled` không phải câu văn cho người vận hành đọc — nó là
**dòng chẩn đoán dạng `key=value`**, cùng họ với log. Dịch "channels"/"enabled" sang tiếng Việt
được rất ít mà mất khả năng grep và mất sự thống nhất với log/trace.

Đề xuất: **giữ nguyên `detail` telemetry**, xử lý như `NT-1` xử lý mã — nó là dữ liệu kỹ thuật,
không phải nhãn. Nếu owner muốn dễ đọc hơn thì hướng đúng là **thêm cột phụ đã dịch** bên cạnh,
chứ không phải dịch chính chuỗi telemetry.

#### Giá trị thu được nếu chấp nhận 02a

Trên màn Trạng thái tích hợp, phần tiếng Anh nhìn thấy được gồm 2 cột × 6 dòng.
02a phủ **6/6 ô `fail_closed_effect`** và **4,5/6 ô `detail`**, cộng toàn bộ dòng
`REVIEW_ITEM` ở bảng sự kiện. Còn lại đúng 2 ô telemetry.
Chi phí: **cỡ nửa ngày**, nằm gọn trong GĐ 4 — không kéo dài 5,5 ngày.

---

### 11.2 `OD-L10N-05` — đúng về **ai**, sai về **chặn ở đâu**

Bản plan đầu ghi "điều kiện chặn của GĐ 2". Rà lại thứ tự phụ thuộc thì cách chặn đó
tốn kém mà không mua thêm được an toàn nào.

#### Vấn đề 1 — câu chữ **không** nằm trên đường găng

| Giai đoạn | Có phụ thuộc câu chữ tiếng Việt không? |
| --- | --- |
| GĐ 3 — `t()` có tham số, `tEnum()`, `<EnumLabel>` | **Không.** Chỉ cần *hình dạng* từ điển, không cần nội dung. |
| GĐ 4 — áp `<EnumLabel>` lên 41 điểm | **Không.** Code tham chiếu **khoá**, không tham chiếu **giá trị**. |
| GĐ 6 — test phủ | **Không.** Test kiểm *có mục hay không*, không kiểm dịch hay dở. |
| GĐ 7 — sửa assertion | **Không.** Assert theo `data-enum-code`. |
| Merge / deploy | **Có.** |

Sửa một giá trị trong `enums.vi.json` là thao tác **rủi ro thấp nhất trong cả plan**:
một chuỗi JSON, không đụng type, không đụng luồng, test không đổi.

⇒ Chặn đúng chỗ là **lúc merge**, không phải lúc bắt đầu build. Cho phép GĐ 3–4 chạy trên
**bản từ điển tạm** (chính §5), owner duyệt song song, chốt câu chữ trước khi merge.
Việc này gỡ ~4 ngày công khỏi lịch chờ của owner mà không giảm mức kiểm soát.

#### Vấn đề 2 — không phải họ nào cũng rủi ro như nhau

Chặn đồng loạt 30 họ coi `BUSY → "Máy bận"` ngang hàng với những chỗ dịch sai thì đổi
hành vi vận hành. Thực tế chỉ **4 họ** thuộc nhóm sau:

| Họ | Vì sao đọc sai là hỏng việc |
| --- | --- |
| `resultType` | `IVR_NO_ANSWER_ATTEMPT` và `IVR_NO_ANSWER_FINAL` khác nhau **đúng một cụm**: "còn lượt gọi" / "đã hết lượt". Đọc nhầm ⇒ escalate sai, hoặc bỏ sót đơn thật sự đã hết đường. |
| `eligibilityDecision` | Trục `TASK_REJECTED_*` (đã bỏ hẳn) và `TASK_HELD_*` (sẽ chạy tiếp) là **hai kết cục trái ngược**. Nhầm ⇒ hoặc bỏ đơn còn cứu được, hoặc ngồi chờ đơn đã chết. |
| `queueStatus` | 8 giá trị `HELD_*` khác nhau ở phần đuôi. Nhầm ⇒ gỡ sai nguyên nhân giữ hàng đợi. |
| `recommendedCoreAction` | **Lỗi trong chính bản dịch §5.3 của tôi**: 5/7 giá trị đang mở đầu bằng "Đề nghị Core kiểm lại rồi…", phần phân biệt bị đẩy xuống cuối câu. Trên bảng hẹp sẽ bị cắt đúng chỗ mang nghĩa. **Cần viết lại, đưa động từ phân biệt lên đầu.** |

Còn lại (`disposition`, `technicalExceptionType`, `deliveryStatus`, `simStatus`,
`dependencyState`, `accountStatus`, …) là ánh xạ cơ học. Dịch sai ở đó là **lỗi chính tả**,
không phải sự cố.

⇒ Rà theo hai mức: owner đọc kỹ 4 họ trên (~54 giá trị), phần còn lại rà nhanh.

#### Vấn đề 3 — `NT-1` đã hạ mức rủi ro sẵn

Vì mã gốc **luôn hiện cạnh nhãn** (tooltip ở bảng, dòng mono ở màn chi tiết), một bản dịch
sai là **thấy được và sửa được**, không âm thầm. Đây không phải cửa một chiều. Chặn cứng
toàn bộ tiến độ cho một thứ có thể sửa bằng một dòng JSON là không cân xứng.

#### Vấn đề 4 — "owner vận hành" hiện **có thể chưa tồn tại**

Bản plan đầu ghi "người trực console hằng ngày". Rà lại thì:

- `CODEOWNERS` chỉ có group giả định (`@ginsengfood/ivr-ui`, `@ginsengfood/ivr-maintainers`),
  và chính file ghi rõ *"Placeholder GitLab group paths must be verified by W-0061 before enforcement."*
- Tracker chỉ dùng một vai **`owner`** duy nhất, không tách vận hành/kỹ thuật.
- `REAL_CUSTOMER_CALL_ALLOWED=NO`; console chưa vận hành thật ⇒ **đội trực chưa hình thành**.

⇒ Nếu chưa chỉ định ai, câu trả lời trung thực là: **owner tự duyệt 4 họ trọng yếu**, và ghi
vào tracker rằng từ vựng sẽ được rà lại một lần nữa khi đội trực thật sự tiếp nhận console.
Đợi một vai chưa tồn tại thì không phải thận trọng, mà là kẹt.

---

## 12. Phụ lục — lệnh kiểm chứng

```bash
# Đếm lại điểm render enum thô
grep -rnE "cell: \([a-z]+\) => [a-z]+\.[a-z_]+,?$" admin-ui/src/app admin-ui/src/components

# Trích enum từ OpenAPI
grep -n -A15 "enum:" specs/api/openapi/ivr-order-confirmation.v1.yaml

# Trích CHECK constraint từ EF model
grep -A3 "HasCheckConstraint(" src/Ivr.Infrastructure/Persistence/PersistenceModelConfiguration.cs

# Assertion test đang bám vào enum thô
grep -rnoE "(getByText|toHaveTextContent|toContain)\([^)]*(IVR_|TASK_|GOLDEN_HOUR)" admin-ui/tests/
```

Giá trị thật đang có trong DB (chạy ở GĐ 1):

```sql
SELECT DISTINCT queue_status FROM ivr_call_jobs;
SELECT DISTINCT result_type  FROM ivr_call_results;
SELECT DISTINCT reason       FROM ivr_review_items;
SELECT DISTINCT source_type  FROM ivr_review_items;
SELECT DISTINCT status       FROM ivr_sim_channels;
```
