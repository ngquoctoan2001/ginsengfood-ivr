# W-0287 — Sửa toolchain CI từ bằng chứng hosted

Ngày 2026-09-14. Baseline `main@9ca529b`. Owner thực hiện: Codex theo yêu cầu tiếp tục, commit từng task. **TESTS_PASS cho phạm vi toolchain/bootstrap; toàn bộ CI chưa PASS.**

Pipeline [2842779996](https://gitlab.com/nqt20102001/ginsengfood-ivr/-/pipelines/2842779996), commit `890dfdd`, có 36 job được tạo và 7 FAIL. Runner `55115499` online khi đọc ngày 14/09. Bốn job .NET dừng ở restore: image `sdk:10.0` chứa SDK `10.0.401`, trong khi `global.json` yêu cầu `10.0.201` và `latestPatch`. Sweep dùng Node image thiếu Docker/.NET; TTS thiếu package `yaml`.

## Thay đổi

- Pin năm job SDK còn trôi về `10.0.201`, khớp `global.json`; guard kiểm tất cả fragment, cả dạng `image.name` và hidden template.
- Sweep dùng SDK này, DinD `29.6.2`, Docker CLI + Buildx, Node 24 từ image chính thức, `npm ci --no-audit --no-fund` và build `Ivr.CiPolicy` Release. Buildx cần cho `RUN --mount` trong Dockerfile TTS; thiếu nó đã tái hiện FAIL.
- Job TTS cài cả npm và dependencies đã khoá trước chín lệnh tự kiểm.
- Lệnh TTS cuối lộ pin cũ của `docs/lab/one-sim-lab-plan.md` trong template W-0185. Hash source hiện tại trùng hằng số verifier từ `98e94dc`; chỉ đồng bộ một pin template. `PENDING_EXTERNAL_INPUT` và mọi đầu vào/chữ ký PENDING giữ nguyên.
- Ghim LF cho inventory và bảng hash dùng so sánh byte. Nội dung và hash chuẩn không đổi.

## Kiểm chứng

- [Regression cấu hình](config-regression.mjs): **10 mutation bị từ chối đúng lý do + 1 cấu hình hợp lệ PASS**. Chạy từ repository root: `node docs/evidence/W-0287/config-regression.mjs`.
- Linux SDK `10.0.201` + DinD `27`: contract **24/24**, pinned-provider **1/1**, E2E **2/2**; observability **12/12**. Không sửa logic nghiệp vụ.
- Chaos đã vượt qua restore/build rồi lộ lỗi fixture: **1/8 PASS**, 7 lỗi kết nối loopback tới proxy nằm trên Docker service. Sẽ xử lý ở task kế tiếp; không báo toàn pipeline xanh.
- Review gate hiện tại vẫn có thể báo PASS khi executable SDK trả 155. Đã tái hiện trong snapshot riêng; task tiếp theo phải buộc nhận diện đúng failure.
- [Sweep Linux](gate-sweep.log): **39/39 PASS, 22 skip theo manifest**, gồm DR thật trong daemon riêng. [Job TTS](tts-job-summary.log): **9/9 lệnh hoàn tất, exit 0**, template kết thúc `VALID_NOT_READY`; không phải 9 bài gọi thật. Container shim có 15 unit test đạt; lab converter 7 refusal đạt.

Lượt đầu dùng `git archive` theo `core.autocrlf=true` đã đổi byte LF thành CRLF trong snapshot và cho 28/39; **INVALIDATED**, không phải lỗi byte đã commit. Đã xuất lại với `git -c core.autocrlf=false archive`, thư mục source mới không có build/node_modules; khi đó 38/39 đạt và lộ Buildx thiếu. Không sửa các hash hay nới validator để chữa snapshot. NuGet audit tắt trong kiểm chứng local này; security scan không được thực hiện.

GitNexus trước sửa `ci-config-selftest.mjs`/`expectedDotnetImage`: LOW, 0 caller/0 process. Chỉ sửa bootstrap/guard, không đổi runtime.

Không thay nhãn nghiệp vụ của image pause. Security scan và push/pipeline mới còn chờ chấp thuận đã hỏi. Chưa có hosted proof cho candidate mới; M3/SIM/staging/production NOT_RUN. `REAL_CUSTOMER_CALL_ALLOWED=NO`.
