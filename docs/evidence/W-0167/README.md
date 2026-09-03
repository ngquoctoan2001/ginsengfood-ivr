# W-0167 — Current-worktree full offline .NET suite verification

Ngày: `2026-09-03`

Baseline commit: `main@b21ec676e490`

Trạng thái: **`TESTS_PASS_LOCAL / FULL_OFFLINE_SOLUTION_PASS / NO_SOURCE_CHANGE /
EXTERNAL_GATES_UNCHANGED / NO_GATE_PROMOTION`**

## 1. Lý do mở lại local queue

W-0161 chạy toàn bộ `Ivr.IntegrationTests` (`236/236 PASS`) và W-0162 chạy toàn bộ
`Ivr.ChaosTests` (`8/8 PASS`) trên working tree hiện tại. Tuy nhiên bằng chứng full solution gần nhất
(`763/763`) thuộc W-0144 tại baseline cũ `b082ed1`; chưa có một lệnh full `Ivr.sln` bao phủ đồng
thời Contract, Unit, Integration và Chaos trên current shared tree.

Đây là phần local của task D8 trong worklist và không cần quyết định nghiệp vụ.

## 2. Phạm vi

- chạy `dotnet test Ivr.sln -c Release --no-restore`;
- ghi nguyên trạng tổng số pass/fail/skip theo từng test project;
- kiểm docs, traceability, gate mirror, Markdown map và diff hygiene;
- không sửa production source, test hoặc config để ép kết quả xanh;
- không coi local PASS là M3/shared/staging/UAT/production evidence.

## 3. Preflight

- .NET SDK: `10.0.201`;
- solution có bốn test project: Contract, Unit, Integration và Chaos;
- Docker/PostgreSQL local đã sẵn sàng từ W-0161/W-0162;
- GitNexus index: commit `b21ec67`, trạng thái up-to-date; W-0167 không sửa symbol nên impact N/A.

## 4. Kết quả

Lệnh:

```text
dotnet test Ivr.sln -c Release --no-restore --logger "console;verbosity=minimal"
```

| Project | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| `Ivr.ContractTests` | 24 | 0 | 0 |
| `Ivr.UnitTests` | 497 | 0 | 0 |
| `Ivr.IntegrationTests` | 236 | 0 | 0 |
| `Ivr.ChaosTests` | 8 | 0 | 0 |
| **Tổng** | **765** | **0** | **0** |

Integration dùng PostgreSQL/Testcontainers thật và kết thúc trong `2m59s`. Không còn container
Testcontainers/Ryuk/Toxiproxy sau test; container `local-information-platform-postgres-1` có sẵn
từ workspace khác được giữ nguyên.

## 5. Gate hỗ trợ

| Gate | Kết quả |
| --- | --- |
| Test traceability | **PASS** — `476` tagged test current |
| W-0167 PII scan | **PASS** — 1/1 Markdown |
| API docs selftest | **PASS** — 14 generated artifacts |
| Gate mirror | **PASS** — 11 gates, 165 work items, 23 open decisions, production=false |
| Markdown map | **PASS** — 655 Markdown files, 870 resolved, 199 unresolved global backlog; W-0167/target 0 unresolved |
| `git diff --check` | **PASS** — chỉ line-ending warnings của shared worktree |
| GitNexus | index `b21ec67` up-to-date; symbol impact N/A vì không sửa function/class/method |

## 6. Boundary

W-0163 và mọi external signature/artifact/shared-E2E vẫn giữ nguyên trạng thái. Production=false và
`REAL_CUSTOMER_CALL_ALLOWED=NO`.

W-0167 đóng khoảng trống full offline solution của D8 trên current tree. Nó không chứng minh M3
consumer, provider sandbox, staging/UAT, cuộc gọi khách thật hoặc production readiness.
