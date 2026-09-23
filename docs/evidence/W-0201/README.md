# W-0201 — Hoà giải hai luồng đồng thời vào `main`

Ngày lập hồ sơ: 23/09/2026 · Claude (W-0349) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Việc này chưa từng có gói bằng chứng riêng. Hồ sơ này dựng lại từ các nguồn đã có trong repo; nó không
chạy lại gì và không đổi kết luận của việc gốc.

## Việc đã làm

Hai session viết song song vào cùng cây và lại trùng Work ID. W-0201 merge nhánh `worktree-gd0-fixes` với `main` (`cc12e53` phía nhánh, `4fde987` vào main) và đánh số lại các việc chưa land thành W-0198..W-0200, còn chính lượt merge là W-0201. Migration id `W0196SignedProductionAttemptPolicy` cố ý giữ nguyên vì `__EFMigrationsHistory` lưu chuỗi đó, và lý do ghi ngay trong XML doc của migration. Merge lộ một va chạm thật: validator seed của W-0196 đòi thư mục seed chứa ba file mẫu, trong khi hai test path seed của W-0193 dùng thư mục rỗng; fixture `CreateSeedFixture` tạo đủ file mà không nới luật nào. Kiểm bằng `IT-COMPROOT-SEEDPATH-03` và `IT-COMPROOT-SEEDPATH-05`, cùng build Release sạch, Unit 558/558 và Contract 24/24 trên cây đã merge.

## Nguồn

- prompt/_execution/prompt-execution-tracker.md — §5 dòng W-0201; §9 completion record 'Work ID: W-0201'; activity A-0571, A-0572
- cc12e53 (merge main vào nhánh; message ghi id cũ W-0200)
- 4fde987 (merge nhánh vào main; message ghi W-0201)
- tests/Ivr.IntegrationTests/CompositionRootTests.cs (CreateSeedFixture)
- src/Ivr.Infrastructure/Persistence/Migrations/20260905120000_W0196SignedProductionAttemptPolicy.cs (XML doc giải thích vì sao giữ tên)
- src/Ivr.Infrastructure/DevTooling/DevToolingOptions.cs (validator thư mục seed của W-0196)

## Phép kiểm C2

[Khai báo](acceptance-tests.json) nay ghi:

- Test .NET kiểm đúng thay đổi của việc này: `IT-COMPROOT-SEEDPATH-03`, `IT-COMPROOT-SEEDPATH-05`.
