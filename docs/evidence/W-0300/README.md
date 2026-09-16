# W-0300 — Gỡ phê duyệt production khỏi migration schema

Ngày 16/09/2026. Baseline `e72ca84`. `REAL_CUSTOMER_CALL_ALLOWED=NO`, không đổi wire, không phát
hành contract.

Nguồn: bản giao việc 16/09 mục `B3`. Nhận xét đúng.

## Vấn đề

`W0196` chèn hai hàng `gh-247-prod-v1` với `approved_for_production = TRUE` **từ bên trong một
migration schema**. Nghĩa là phê duyệt tới **mọi** database mà schema tới — máy dev, server test,
staging, production — do chính cơ chế tạo bảng mang đi. Chạy một migration không phải là một quyết
định, và một phê duyệt xuất hiện ở đâu schema xuất hiện thì cũng không phải.

Chính hàng đó tự mâu thuẫn: `retention_class` của nó là `LEGAL_DECISION_PENDING` — khai quyết định
còn mở trong khi đã nhận phê duyệt mà quyết định ấy sẽ cấp.

`m8-11` và `specs/functional/03-scheduler-attempt-policy.md` đều đặt phê duyệt này ở **Product +
Order Core + Module 3**. `OD-V1-08` được đóng `2026-09-05` bởi **một mình owner IVR**, và phần đơn
hàng của Module 3 chưa xong nên chữ ký đối ứng chưa thể tồn tại.

## Phương án đầu bị **chính database từ chối**

Định làm `UPDATE … SET approved_for_production = FALSE`. Database không cho:

```text
Npgsql.PostgresException : P0001: attempt-policy versions are immutable; create a new version
```

`trg_ivr_attempt_policies_immutable` (`P1_2_InitialTargetV1Persistence.cs:1104-1116`) là
`BEFORE UPDATE … FOR EACH ROW` và **raise vô điều kiện** — không phân biệt cột. Đó là `W-0151` được
cưỡng chế ở tầng schema: điều khoản của một version không bao giờ được đổi dưới chân một task đã
được nhận theo nó.

**Luật đó đúng, nên bản sửa phải tôn trọng nó chứ không lách.** Hai hàng seed bị **xoá**; version
quay lại khi có người chèn nó qua một bước vận hành mang `approval_reference` thật — đúng hình dạng
`PRODUCTION_CALL` đang dùng, và đúng thứ `B3` yêu cầu.

## Xoá thì mất gì

Không mất gì:

| | |
| --- | --- |
| Các con số | Còn nguyên trong `SignedProductionAttemptPolicies`, không đụng tới |
| Provenance | Gói sign-off và decision register giữ nguyên |
| Foreign key trỏ vào bảng | **Không có** |
| Task đã nhận theo version này | **Không có** — gọi production chưa từng bật; mock và lab resolve `mock-lab-v1` |
| Tiền lệ | `W0196.Down()` xoá đúng hai hàng này sẵn rồi |

`Down()` chèn lại **đúng** thứ `W0196` đã seed, nên rollback về đúng trạng thái trước lượt này chứ
không về một nửa.

## Catalogue C# cố ý không đụng

`SignedProductionAttemptPolicies` vẫn mang `OwnerApproved`. Nó chỉ tới được qua in-memory registry,
mà `AddIvrFoundation` **ném lỗi** nếu execution mode khác `MOCK`
(`ServiceCollectionExtensions.cs:94-103`), và cả hai host production đều truyền mặc định `false`.
Nên đường đó **không bao giờ chạm `PRODUCTION_REAL`**. Lật nó nữa sẽ xoá một bản ghi quyết định và
viết lại năm test mà không đổi được gì ở runtime.

## Kiểm chứng

| Kiểm | Kết quả |
| --- | --- |
| `dotnet test Ivr.sln` | **1065/1065** pass, 0 failed, 0 skipped |
| `gitnexus_impact` trên `SignedProductionAttemptPolicies` | LOW, 0 caller runtime ngoài DI |
| Mutation: vô hiệu `Up()` | `IT-DB-POLICY-UNSIGNED-01` **ĐỎ** |
| Khôi phục `Up()` | **XANH** |
| `progressive-selftest`, `migration-expand-guard` | PASS |
| Migration SQL sinh ra | `DELETE FROM ivr_attempt_policies WHERE policy_version = 'gh-247-prod-v1';` |

`IT-DB-POLICY-UNSIGNED-01` viết là *"không hàng nào claim"* chứ không phải *"hai hàng này đã biến
mất"*. Version cụ thể không phải điểm chính — điểm chính là **một phê duyệt production không được
tới bằng migration, dù nó tên gì**. Seed lại dưới tên khác sẽ lọt một test nêu đích danh tên cũ.

Migration sinh bằng `dotnet ef migrations add` để Designer và snapshot đúng cơ chế; snapshot không
đổi vì đây là migration **chỉ dữ liệu**.

## Còn lại

`OD-V1-08` vẫn cần **Product + Order Core + Module 3** đối ký. Lượt này không thay cho ba chữ ký đó;
nó chỉ làm cho việc thiếu chữ ký **không còn tự cấp phê duyệt cho mọi database**.
