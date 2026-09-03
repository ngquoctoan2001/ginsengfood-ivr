# W-0163 — Recipient routing input template

Trạng thái: **`PENDING_OWNER_INPUT / NOT_A_DISPATCH_RECEIPT`**

Mục đích: thu dữ liệu routing tối thiểu để Codex/Module 8 Owner có thể tiếp tục gửi riêng từng
batch `D-01..D-05`. File này không chứng minh message đã được gửi hoặc người nhận đã chấp nhận.

## Quy tắc điền

- Mỗi batch phải chỉ tới một người/nhóm có authority xác minh được; không dùng “team chung”.
- `authority_source_ref` phải là charter, role assignment, ticket hoặc approval-chain reference.
- `destination_ref` chỉ lưu alias/path/ticket destination; không ghi access token, password, email
  cá nhân, số điện thoại hoặc secret.
- `due_at` và `dispatch_authorized_at` dùng ISO-8601 có timezone.
- `dispatch_authorized_by` là người có quyền cho phép hành động gửi ra ngoài.
- Có thể điền một batch trước; các batch còn lại giữ `PENDING_OWNER_INPUT`.

## Owner input

| Batch | Actual recipient identity | Role / organization | Authority source ref | Channel kind | Destination ref | Due at | Dispatch authorized by | Authorized at | State |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `D-01` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `NOT_READY` |
| `D-02` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `NOT_READY` |
| `D-03` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `NOT_READY` |
| `D-04` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `NOT_READY` |
| `D-05` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `PENDING_OWNER_INPUT` | `NOT_READY` |

## Readiness rule

Một row chỉ chuyển `READY_FOR_HASH_RECHECK_AND_DISPATCH` khi đủ chín trường dữ liệu, authority ref
có thể kiểm và người cấp quyền gửi nằm trong scope. Sau đó W-0163 phải verify lại M8-12/manifest,
gửi đúng message D-xx và chỉ ghi receipt khi nhận được message/ticket ID + timestamp thật.
