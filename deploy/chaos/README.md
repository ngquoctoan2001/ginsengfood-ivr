# Chaos fault injection — `W-0042` · `P6-3`

## Blast radius, stated before anything else

Every upstream in `toxiproxy.staging.json` is a **container alias on a throwaway network**
(`chaos-db`, `chaos-sales`). Those names do not resolve anywhere else. This is the blast-radius
limit `P6-3` §4 asks for, and it is structural rather than procedural: there is no route from this
config to a shared environment, so no run of it can reach one by mistake.

`CHAOS-GUARD-05` fails the build if an upstream here ever names a host that is not a container
alias or loopback.

**Không chạy chaos ở production hoặc với khách thật** (`P6-3` §11). `REAL_CUSTOMER_CALL_ALLOWED=NO`
và `IVR_ADAPTER_MODE=MOCK` là điều kiện của toàn bộ Phase 6.

## Hai cách chèn lỗi, dùng cho hai thứ khác nhau

| Cách | Dùng cho | Vì sao |
| --- | --- | --- |
| **Toxiproxy** (file này) | mất kết nối DB, mạng chậm | lỗi **thật ở tầng mạng** — socket bị cắt bởi một thứ nằm giữa tiến trình và Postgres, đúng như một partition |
| **Chèn ở tầng mã** (`tests/chaos/`) | Sales không trả lời, SIM rớt cuộc | biên phụ thuộc ngoài chưa có endpoint thật để cắt; `P6-3` §5 cho phép rõ ràng cách này |

Khác biệt đáng nói vì hai cách chứng minh hai mức khác nhau, và gộp chúng lại sẽ làm bản báo cáo
game-day nghe mạnh hơn thực tế.

## Chạy

Suite chaos **tự dựng** Toxiproxy và Postgres cho mỗi lần chạy rồi xoá đi, nên file này không cần
thiết để chạy test:

```bash
dotnet test tests/chaos/Ivr.ChaosTests.csproj
```

File này dành cho một môi trường staging **chưa tồn tại**. Khi có staging (`W-0063`), nạp nó vào
Toxiproxy rồi trỏ IVR qua cổng `15432`/`15999`. Ghi ra đây thay vì mô tả như thể đã có nơi để chạy.

## Toxic dùng trong các scenario

| Toxic | Tác dụng | Scenario |
| --- | --- | --- |
| `enabled: false` trên proxy | cắt kết nối, đóng cái đang mở và từ chối cái mới | `CHAOS-DB-02`, `CHAOS-RECOVERY-04` |
| `latency` (downstream) | chậm chứ không chết | dự phòng cho kịch bản "chậm" của `ARCH-05` §1 |
