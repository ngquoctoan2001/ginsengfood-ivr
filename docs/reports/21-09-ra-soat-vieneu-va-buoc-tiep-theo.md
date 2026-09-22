# Rà soát VieNeu và bước tiếp theo — 21/09/2026

> Cập nhật sau báo cáo: [W-0329](../evidence/W-0329/README.md) đã kiểm sáu ca DTMF tự động
> và ca không nhập phím. Owner đã chốt tiếng; Chainguard/model thật/lab đã chạy. Tải local
> tái hiện timeout10s, máy S5 và mirror/Legal vẫn còn chờ; gọi khách thật tắt. Nội dung
> dưới đây là snapshot review ban đầu, không dùng các đề xuất nghe lại làm backlog hiện tại.

**Kết luận:** phần chuyển sang VieNeu và phần làm được ngay của Lô 4 đã hoàn tất trong code. Chưa đủ bằng chứng production. Lô tiếp theo nên là **kiểm chứng model thật trên image ứng viên, rồi nghiệm thu lab có ghép đoạn**, đồng thời hoàn thiện đường kiểm tra trước triển khai.

`REAL_CUSTOMER_CALL_ALLOWED=NO` · `RELEASE_BLOCKED`.

Phạm vi: `main@e33927d03257492de93727fbdcab4b33e6f3f25e`; đọc commit, source, hồ sơ và chạy kiểm tra tập trung. Không sửa runtime/config, không commit, không tải model, không gọi điện. File có sẵn chưa commit `19-09-bao-cao-tien-do-module-8-ivr.md` không được dùng làm bằng chứng và được giữ nguyên.

## 1. Những điểm cần xử lý trước production

### F1 — Ưu tiên cao: còn thiếu bước bắt buộc kiểm chứng hồ sơ production trong đường triển khai

Không thể hiểu câu “chart tự từ chối nếu thiếu bất kỳ điều kiện nào” là chart đã kiểm đủ chữ ký, nghe duyệt và hiệu năng:

- [`_helpers.tpl`](../../deploy/helm/ivr/templates/_helpers.tpl), dòng 104–148: các approval ref chỉ bị kiểm rỗng; `performanceRef` chỉ bắt buộc khi timeout vượt 5.000 ms. Không đọc kết quả nghe 12 đoạn hoặc bằng chứng đo máy thật.
- [`tts-provenance-gate.mjs`](../../deploy/ci/scripts/tts-provenance-gate.mjs), dòng 279–286: gate cấu trúc trả thành công cả khi còn `release_blockers=LEGAL,INTERNAL_MIRROR`. Lượt kiểm 21/09 xác nhận đúng hành vi này; đây là gate cho candidate, chưa phải quyền phát hành.
- [`backend.py`](../../deploy/tts/shim/backend.py), dòng 57–119: runtime kiểm byte bundle, các hash và manifest chọn giọng, rồi nạp engine; không gọi verifier phê duyệt production.
- [`promote.gitlab-ci.yml`](../../deploy/ci/promote.gitlab-ci.yml), dòng 77–97: job production kiểm DF-03, hiện từ chối biến môi trường non-MOCK và chỉ dùng `values-prod.yaml`; chưa nạp bản nháp TTS hay chạy `verify-model.py --mode production`.

**Việc cần làm:** xác định bước triển khai có kiểm tra bắt buộc trên đúng image digest/bundle/manifest: provenance production, S1, S2, số đo S5, topology S6 và quyền phát hành. Sau đó nối vào đường triển khai đã được duyệt. Đây là khoảng trống cần đóng trước bật TTS; không phải bằng chứng hệ thống hiện đang gọi được khách — production vẫn tắt TTS và giữ MOCK/NO.

### F2 — Ưu tiên cao: chữ ký S2 chưa tự đáp ứng hợp đồng máy kiểm

[`verify-model.py`](../../deploy/tts/scripts/verify-model.py), dòng 16–27 và 85–91, đòi thẩm quyền `LEGAL_PRIVACY`, thông tin người/ngày/ref, `license_file_sha256` và mirror chính xác. [`MODELS.lock`](../../deploy/tts/models/MODELS.lock) hiện còn **13/13 license hash trống, 13/13 mirror URI trống**, hai gate `OWNER_DATA_REQUIRED`.

Trong khi đó, [quyết định 17/09, mục S2](../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md) chuyển việc nhận rủi ro về Sếp, do công ty không có đội Legal/Security riêng. Vì vậy “Sếp ký rồi điền legalRef” chưa đủ: cần chốt chữ ký ấy được biểu diễn thế nào trong hợp đồng provenance, và bằng chứng nào đáp ứng phần license hoặc ngoại lệ được phê duyệt. Không điền hash giả hoặc tự gắn thẩm quyền.

Khi cập nhật Legal/mirror, byte `MODELS.lock` đổi: phải cập nhật chuỗi hash trong `voices.json`, provenance gate, B3/template, bản nháp Helm và **lấy lại xác nhận cho manifest giọng ràng vào lock mới**. Không sửa âm thầm artifact Owner đã ký. Các ràng buộc nằm ở [`tts-voice-acceptance-lib.mjs`](../../deploy/ci/scripts/tts-voice-acceptance-lib.mjs), dòng 18–26, và [`b3-telephony-evidence-validator.mjs`](../../deploy/ci/scripts/b3-telephony-evidence-validator.mjs), dòng 19–29.

### F3 — Ưu tiên vừa: lệnh khởi động lab chưa chờ TTS sẵn sàng trước preflight

[`Start-FreeSoftphoneLab.ps1`](../../deploy/lab/Start-FreeSoftphoneLab.ps1), dòng 82–99, dùng `compose up -d`, mở MicroSIP rồi có thể gọi ngay qua `-InvokePreflightCall`. [`Invoke-FreeSoftphoneCall.ps1`](../../deploy/lab/Invoke-FreeSoftphoneCall.ps1), dòng 31–46, chỉ chờ SIP đăng ký. Overlay có healthcheck nhưng hai script không chờ kết quả này.

**Hệ quả có thể xảy ra khi cold start:** SIP đã online trong lúc model chưa nạp xong; task thử gặp TTS chưa sẵn sàng. Chưa tái hiện cuộc gọi lỗi trong lượt review này. **Sửa trước buổi lab:** chờ `/health/ready=200` với timeout hữu hạn, làm permission probe; thiếu điều kiện thì dừng trước gửi task. Đây cũng là yêu cầu đang ghi trong [lab runbook](../evidence/W-0122/lab-runbook.md).

### F4 — Cần đưa rõ vào nghiệm thu: ghép đoạn và nơi chứa audio production

- [`docker-compose.vieneu-tts.yml`](../../docker-compose.vieneu-tts.yml), dòng 39–46, hiện **Segmentation=false**. Gọi thành công với cấu hình này chỉ chứng minh đọc cả câu; chưa nghiệm thu 12 đoạn cố định và mối nối.
- 12 WAV hiện có đã kiểm lại checksum và PCM mono 8 kHz. Lab cài chúng ở thư mục sounds gốc qua [`entrypoint.sh`](../../deploy/lab/asterisk/entrypoint.sh), dòng 32–42.
- [Bản nháp production](../../deploy/helm/ivr/values-prod-tts.draft.yaml) tham chiếu `sound:generated/ivr-seg-*`. [Chart worker](../../deploy/helm/ivr/templates/deployment-worker.yaml), dòng 238–247, chỉ gắn PVC có sẵn; không chép 12 WAV vào đó và không triển khai đầu đọc Asterisk.

**Việc cần làm:** bật catalog trong cấu hình lab nghiệm thu có kiểm soát để nghe mối nối; trong S5/S6 phải có bước đưa đúng 12 file vào media sink, kiểm checksum/quyền đọc và chứng minh từng media reference phát được. Render Helm thành công chưa chứng minh các file đó hiện diện.

## 2. Các commit gần nhất và đính chính phản hồi cũ

| Commit ngày 18/09 | Nội dung đã đối chiếu |
| --- | --- |
| `9af20d3` — W-0315 | VieNeu là engine thật duy nhất; bỏ STATIC_FILE/clip composer; endpoint loopback; cập nhật lab và tài liệu |
| `a6b7063` — W-0316 | Cập nhật các quyết định 17/09; phần S2 chưa ký vẫn để mở |
| `e71ff00` — W-0315 follow-up | Sửa các chỗ sót trong kế hoạch lab/hồ sơ, ghim lại B3; không thay runtime |
| `3d0111a` — W-0317 | **Lô 4 phần làm được ngay đã xong**: vá image Debian, thử Chainguard, tạo Helm draft |
| `e33927d` — W-0318 | **Lô 5 phần agent đã xong**: tiêu chí + danh sách đề nghị nghiệm thu; Toàn chưa duyệt từng đợt |

Nguồn đầy đủ: [W-0315](../evidence/W-0315/README.md), [W-0316](../evidence/W-0316/README.md), [W-0317](../evidence/W-0317/README.md), [W-0318](../evidence/W-0318/README.md).

**Số CVE:** đã đọc và tính lại từ bốn JSON Trivy gốc trong `artifacts/sbom`; hash khớp các tiền tố W-0317 ghi. Đây là **snapshot 18/09**, không phải quét mới 21/09:

| Image trong báo cáo quét | HIGH | CRITICAL | Có bản vá |
| --- | ---: | ---: | ---: |
| Debian trước vá | 54 | 3 | 13 |
| Debian sau vá — Dockerfile đang dùng | 44 | 0 | 0 |
| Distroless Debian — chỉ image nền | 21 | 0 | 0 |
| Chainguard thử nghiệm — toàn image TTS | 0 | 0 | 0 |

44 là số finding theo gói, gồm 8 CVE riêng biệt theo W-0317. **Chưa chuyển sang Chainguard:** Python đổi 3.12 → 3.14, chưa sinh tiếng thật trên candidate này. “Không thư viện nào của 44 finding được nạp” chỉ là phép đo import không có model của W-0317 §2; không chứng minh toàn bộ quá trình inference không thể chạm tới chúng. “0 CVE” cũng chỉ đúng ở lần quét đó.

Một chỗ tài liệu đang dùng vẫn ghi 16: [phiếu cho Sếp](../../plan/ivr-orther/phieu-quyet-dinh-cho-sep-2026-09-17.md), dòng 146. Khi chuẩn bị ký S2, dùng scan mới trên digest chọn phát hành và sửa số liệu trong phiếu này.

## 3. Thứ tự thực hiện đề xuất

| Thứ tự | Việc cụ thể | Đầu vào/người phụ trách | Xong khi |
| --- | --- | --- | --- |
| **1 — lô kỹ thuật kế tiếp** | Bổ sung chờ readiness/permission trước gọi lab; chuẩn bị phép thử so sánh Debian đã vá với Chainguard bằng **cùng bundle thật** | Agent; Docker hoạt động; bundle đã kiểm hoặc Toàn cho phép fetch nonprod theo lock | Nạp 13/13 artifact đúng size/hash; ba giọng được chấp nhận đều sinh PCM 8 kHz; ghi image ID/digest, latency và trạng thái lỗi; không dùng deterministic-test làm bằng chứng model |
| **2** | Chạy nghiệm thu có ghép đoạn: 2 đơn giả × 3 miền; nghe đủ 12 đoạn và mọi mối nối; kiểm DTMF, media round-trip và rollback | S1 chỉ định người nghe; Toàn vận hành software lab | 6/6 cuộc gọi có kết quả và xác nhận người nghe; rollback về đọc cả câu vẫn dùng VieNeu; có gói kết quả ràng vào candidate |
| **3 — song song chuẩn bị** | Chốt cách đưa S2 vào provenance theo F2; chọn Debian hoặc Chainguard sau khi có kết quả model thật; quét lại đúng image và lập mirror | Sếp ký S2; Toàn/S5 cung cấp nơi lưu image/model | Có người/ngày/ref và dữ liệu artifact thật; xử lý rõ yêu cầu license; chuỗi hash/manifest mới hợp lệ; không còn blocker provenance production |
| **4** | Đo cold/warm latency, RAM/CPU và tải trên máy dự kiến chạy; triển khai thử media sink gồm 12 WAV | S5; S6 cho đầu tổng đài/topology | Đạt baseline 5 giây/request và ngân sách trước quay còn 20% dư theo cấu hình; số đo bao gồm nhiều cuộc gọi, không chỉ một câu smoke; reference phát được, Asterisk đọc được/không ghi được |
| **5** | Tích hợp kiểm tra production bắt buộc theo F1; hoàn tất 14 ô nháp; diễn tập rollout/rollback trên môi trường được duyệt | Agent + S5/S6 + release owner | Production verifier PASS trên bundle thật; các negative case thiếu phê duyệt/measurement bị chặn; exact candidate có test/gate/deploy evidence |
| **6** | Toàn nghiệm thu theo Lô 5, cập nhật trạng thái có căn cứ; chỉ bật production theo quyết định phát hành | Toàn và các chữ ký còn thiếu | Các điều kiện phát hành thực tế đã đủ; software-lab PASS không được đổi thành real-trunk/production PASS |

**Không cần chờ SIP trunk để làm bước 1–2 trong software lab.** S6 vẫn là phụ thuộc để chứng minh tuyến tổng đài production. Bộ B3 hiện còn 8 kịch bản real-SIM; vì hướng đã chốt là SIP trunk 8 kênh, cần xác nhận cách ánh xạ bộ nghiệm thu sang topology thật trước khi tuyên bố đóng B3, không lấy 6 MicroSIP calls thay thế phần đó.

S1 cần phân biệt: **chọn ba giọng đã được Owner ký ngày 28/08** và manifest hiện còn hợp lệ; phần chưa xong là nghe 12 đoạn cố định/mối nối. Nếu lock thay đổi ở bước 3, xử lý lại binding và xác nhận theo F2, không coi chữ ký cũ tự áp dụng.

## 4. Kiểm chứng thực hiện ngày 21/09

| Kiểm tra tại baseline đã nêu | Kết quả |
| --- | --- |
| Unit test .NET tập trung Speech, Scripts, AsteriskLabTelephonyTests; build từ source với `--no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false` | **215/215 PASS**, 0 skipped |
| Python ModelLock, RequestContract, BackendGuard, VoiceAcceptance, VieNeuBackendVerification | **12/12 PASS**; không có inference bằng model thật |
| Provenance selftest | **PASS**, 10 mutation; vẫn `LEGAL,INTERNAL_MIRROR` |
| Voice acceptance selftest + manifest Owner hiện tại | **PASS**; 7 binding mutation, 9 acceptance mutation; Bắc Ngọc Linh / Trung Ngọc Trân / Nam Mỹ Duyên |
| Audition / fixed-render selftest | **PASS**; 11 candidates, outbound denied; 6 refusal và positive tới cổng loopback đóng |
| B3 selftest / template | **PASS 1 valid + 34 refusal**; template **VALID_NOT_READY** |
| 12 WAV đang theo dõi | **12/12 checksum PASS**, PCM 16-bit little-endian, mono, 8 kHz |
| Docker/Helm/container/full sweep/Trivy mới | **NOT_RUN / ENV_BLOCKED**: không có pipe `dockerDesktopLinuxEngine`; không lấy lượt PASS 18/09 để chứng nhận HEAD hiện tại |
| Ba test Python conversion | **NOT_RUN**: Python host thiếu `soxr`; 12 test ở trên không thay thế kiểm resample/container |
| Model thật/lab call/performance máy đích | **NOT_RUN**; không tìm thấy bundle ở `artifacts/w-0122-models` hoặc file `.onnx` trong `artifacts`; không kết luận toàn máy không có model |

Log, TRX, kiểm artifact và tổng hợp scan: [thư mục bằng chứng lượt review](../../artifacts/vieneu-review-20260921/). Node spawn và Python temp ban đầu bị sandbox chặn; đã chạy lại cùng selftest ngoài sandbox và PASS. Lượt .NET đầu không tiến triển đã dừng, lượt đơn MSBuild nêu trên hoàn tất. Không sửa test để đạt kết quả.

GitNexus query/context đã dùng để tìm symbol; index tại `9af20d3`, nên kết luận lấy từ source `e33927d` đã đọc trực tiếp, không coi graph là bằng chứng của tip. Báo cáo này không thay tracker, chữ ký hoặc kết luận phát hành.

## 5. Cập nhật sau khi Owner yêu cầu tiếp tục — W-0320

Bước kỹ thuật số 1 đã hoàn tất local: preflight dùng chung cho hai script lab, bundle thật
`13/13` đúng hash, Debian và Chainguard đều qua `12/12` HTTP syntheses. Checksum audio tương ứng
giống nhau. Docker volume fixture đã kiểm quyền RW/RO và cleanup; regression `17` refusal + `4`
entry cases PASS. Chi tiết, image IDs và giới hạn: [W-0320](../evidence/W-0320/README.md).

**Ưu tiên mới trước bước 2:** worker vẫn timeout `5 giây`, trong khi câu đơn giả đo trên sidecar
mất `6,3–19,8 giây`; cả `12` lượt câu đơn đều vượt timeout. Cần đo toàn script của worker và xử lý
ngân sách thời gian/segmentation trước nghiệm thu gọi. Không tự tăng timeout production.
Các trạng thái Docker/model `NOT_RUN` trong §4 là thời điểm review ban đầu; W-0320 bổ sung bằng
chứng model local, chưa bổ sung cuộc gọi/người nghe/Trivy mới/target hardware hoặc production approval.
