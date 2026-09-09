# W-0251 — Tách bản ghi đóng băng khỏi pin sống, và dựng thứ chạy được cái gate

Ngày: 2026-09-09 · Baseline: `main@6952e60` · Trạng thái: **TESTS_PASS**.

`W-0250` để lại hai gate đỏ không phải của lượt đó và một câu hỏi: sửa thế nào khi cách duy nhất để
gate xanh lại là **ghi đè một bản ghi đã đóng**. Lượt này trả lời, rồi trả lời luôn câu hỏi lớn hơn
đứng sau nó — tại sao đỏ suốt ba mươi work item mà không ai thấy.

## 1. Đo trước, kết luận sau

Ba manifest cùng tên `artifact-sha256.txt`, cùng 18 dòng, khác vai. Đối chiếu với HEAD:

| Manifest | Khớp HEAD | Ai đọc nó |
| --- | ---: | --- |
| `docs/evidence/W-0152/` | **11/18** | không script nào |
| `docs/evidence/W-0170/` | **17/18** | **bốn** script gate |
| `docs/evidence/W-0186/` | 4/10 | không script nào |

Con số này là toàn bộ luận điểm:

- **W-0170 đang là pin sống trá hình.** 17/18 khớp HEAD không phải ngẫu nhiên — nó bị buộc phải khớp,
  vì bốn script so nó với file thật. Dòng lệch duy nhất là `m8-05`.
- **W-0152 là bản ghi đóng băng thật, và nó *đúng* khi cũ.** Bảy dòng không khớp vì sáu file ấy đã
  đổi hợp lệ giữa hai work item. "Sửa" nó sẽ là làm hỏng một bản ghi lịch sử.

Hai vai đó không thể ở chung một tệp: một cái phải theo HEAD, một cái phải **không** theo.

## 2. Tách

| Vai | Ở đâu | Sửa được không |
| --- | --- | --- |
| Bản ghi đóng băng | `docs/evidence/W-0170/artifact-sha256.txt` | **Không** — và commit này không đụng một byte |
| Pin sống | `deploy/ci/pins/external-decision-artifacts.sha256` (**mới**) | Có, cùng commit với source |

Pin sống sinh ra từ chính bản ghi ấy, sửa đúng **một** dòng: `m8-05-…md` từ `6525d2df…` sang
`a03fc6ca…` — nội dung `W-0217` đã ghi. Mười bảy dòng còn lại khớp sẵn, và đó chính là bằng chứng
tệp kia đang bị dùng sai vai.

Tệp pin mang **header tự giải thích** nói nó là gì và tại sao không phải bản ghi evidence, nên bốn
parser được sửa để bỏ qua dòng `#`. Người mở tệp lần sau không phải suy ra vai của nó từ đường dẫn.

`git diff` xác nhận `artifact-sha256.txt` của **cả W-0152 và W-0170** không đổi một byte. Thứ đổi
trong pack là `decision-closure-input.template.json` — input template dẫn xuất, không phải chứng
thực.

W-0186 nay lệch 6/10 **vì chính lượt này**, và đó là hành vi đúng: không script nào đọc nó, nó là
bản ghi, bản ghi được phép cũ đi.

## 3. Pin của pin — dây chuyền phải gỡ theo thứ tự

Closure validator ghim **bytes của hai script kia**, nên sửa response validator làm closure đỏ, sửa
routing lại làm closure đỏ lần nữa. Bốn lượt re-pin nối nhau, mỗi lượt một thông báo khác:

```text
m8-05 lệch manifest          -> tách pin, sửa dòng m8-05        -> W0165 PASS
response-validator lệch      -> re-pin trong closure validator  -> lộ lỗi kế
source.artifact_manifest_path-> ba template trỏ sang pin mới    -> lộ lỗi kế
routing-validator lệch       -> re-pin closure + template       -> W0170 PASS
```

Bảy gate `external-decision` nay xanh cả bảy: `W0164`, `W0165`, `W0170`, `W0179`, `W0184` cộng hai
selftest phụ thuộc.

## 4. Câu hỏi lớn hơn: tại sao không ai thấy suốt ba mươi work item

Không phải vì ai bỏ qua. **Không gì chạy chúng.**

Đếm bằng cách bắc cầu mọi đường gọi — job trong `deploy/ci/*.yml`, npm script trong hai
`package.json`, và `import` giữa các script:

> **17 script** trong `deploy/ci/scripts/` không tới được từ **bất kỳ** job CI, npm script hay script
> nào khác.

Chúng chỉ chạy khi có người quét tay cả thư mục. Và quét tay **đọc sai** chúng: phần lớn in một dòng
`Usage:` rồi thoát khác 0 khi gọi không tham số — đúng bài học `W-0221`, nơi một lượt quét đọc bốn
dòng usage thành bốn lần pass.

Nên "gate nói đúng chuyện gì đang xảy ra" cần hai thứ mà `for f in *.mjs` không có: **argv** mỗi gate
thật sự cần, và **token** nó in khi thật sự pass.

## 5. `gate-sweep.mjs` + `gate-invocations.json`

| | |
| --- | --- |
| `deploy/ci/gate-invocations.json` | mỗi gate: `argv` + `expect`, hoặc `sweepable: false` kèm **lý do** và **ai chạy nó** |
| `deploy/ci/scripts/gate-sweep.mjs` | chạy các entry có `argv`; **exit 0 chưa phải phán quyết** — phải thấy `expect` trong output |
| `quality-gate.gitlab-ci.yml` job `gate_sweep` | `allow_failure: false` |
| `pnpm --dir deploy/ci test:gates` | chạy tại chỗ; `test:gates:list` in bảng phân loại |

Tính chất giữ nó thật về lâu dài nằm ở chỗ thứ ba: **mọi tệp** trong thư mục phải có entry, chạy được
hay không chạy được đều phải khai. Thêm một gate mà quên khai thì sweep đỏ. Đó đúng là thứ lẽ ra đã
bắt được orphan đầu tiên vào ngày nó xuất hiện, thay vì để dồn thành mười bảy.

Phân loại: **39 chạy được**, **18 không** — mỗi cái một lý do cụ thể chứ không phải bỏ trống: cần
docker (`image-selftest`), cần cluster (`k8s-selftest`), cần image oasdiff ghim
(`selftest-oasdiff.sh`), là generator chứ không phải gate (`build-api-docs`, `generate-speech-segments`),
là thư viện không có lối vào (`migration-expand-guard`, `observability-runtime-selftest`,
`tts-voice-acceptance-lib` — chạy trực tiếp thì **exit 0 mà không chứng minh gì**), hoặc phán quyết
phụ thuộc lần chạy test ngay trước (`verify-api-behavior-matrix`).

## 5b. Sweep tự chứng minh nó đỏ được

Một gate không đỏ được thì không phải bằng chứng — `W-0126` viết ra luật đó, `W-0250` vừa thấy chính
`contract-freeze-selftest` vi phạm nó. Nên sweep này phải chứng minh **ba** đường đỏ, không phải khai
là có:

| Đột biến | Kết quả |
| --- | --- |
| Thêm một script không khai trong manifest | `exit=1`, chỉ đúng tên `zz-unlisted-probe.mjs` |
| Đổi tên một entry thành file không tồn tại | `exit=1`, chỉ đúng `cd-selftest.mjs` bị mất khai báo |
| Đặt `expect` thành token gate không bao giờ in | `FAIL … exit 0 but never printed …` |

Đường thứ ba là đường quan trọng nhất, vì nó chính là bài học `W-0221`: **exit 0 chưa phải phán
quyết**. Cả ba đều khôi phục sạch, `exit=0` sau đó.

`--only <gate>` thêm vào để chạy lẻ một gate lúc gỡ lỗi — nhưng nó **chỉ thu hẹp phần chạy, không thu
hẹp phần kiểm phủ**: một sweep ngừng thấy script chưa khai trong lúc ai đó đang gỡ lỗi sẽ là công cụ
sai để với tay tới.

## 5c. Hai lỗi sweep tự gây ra, sửa ngay tại chỗ

Chạy hai sweep chồng nhau thì `dr-selftest` đỏ sau 2.7s trong khi chạy lẻ nó xanh sau 136s. Nguyên
nhân thật: script dựng container docker theo **tên cố định** — `ivr-dr-primary`, `ivr-dr-standby`,
network `ivr-dr-selftest` — nên lần chạy thứ hai đâm vào lần thứ nhất. Sweep khi đó báo "gate hỏng"
trong khi sự thật là "bạn chạy nó hai lần". Đúng loại trả lời sai mà cả lượt này sinh ra để dẹp.

| Sửa | Trước | Sau |
| --- | --- | --- |
| Khoá chạy song song | lần thứ hai đỏ vô nghĩa | `GATE_SWEEP_BUSY — pid 26524 has been sweeping since …`, `exit=1` |
| Dòng lỗi | `exit 1: Node.js v24.19.0` | dòng **nêu tên vấn đề**, không phải banner phiên bản của node |

Khoá **không** thành bẫy khi có sự cố: nó ghi pid chủ, và khoá của một pid đã chết thì bị **chiếm
lại** kèm dòng nói rõ, chứ không chặn người sau. Đã thử cả hai chiều — chủ còn sống thì từ chối
(`exit=1`), chủ đã chết thì chiếm lại và chạy tiếp; khoá xoá sạch lúc thoát và nằm trong `.gitignore`.

## 6. Kiểm chứng

```text
gate-sweep.mjs                             GATE_SWEEP_PASS 39/39 run, 18 skipped by manifest (exit=0)
  ba đột biến chứng minh đỏ được           unlisted / ghost entry / expect sai -> đều exit=1
  khoá song song                           chủ sống -> GATE_SWEEP_BUSY exit=1; chủ chết -> chiếm lại
  trong đó external-decision               W0164 · W0165 · W0170 · W0179 · W0184 đều PASS
git diff docs/evidence/*/artifact-sha256   không đổi một byte
ci-config-selftest.mjs                     CI_CONFIG_SELFTEST_PASS
docs-selftest.mjs                          DOC_CI_TOPOLOGY_PASS · API_DOCS_SELFTEST_PASS
dotnet test Ivr.sln                        952/952 PASS, 0 failed, 0 skipped
gate-status.mjs                            GATE_STATUS_PASS
detect_changes                             low
```

`REAL_CUSTOMER_CALL_ALLOWED=NO`. `TARGET_CONTRACT_V1` vẫn `DRAFT`.

## 7. Còn lại

| # | Việc | Ai |
| ---: | --- | --- |
| — | ba manifest còn lại vẫn tên `artifact-sha256.txt` như nhau; đổi tên bản ghi thành `attested-sha256.txt` sẽ khiến nhầm vai khó hơn nữa | tôi, khi tiện |
| — | endpoint revoke + OAS + IR-06 — `2.5`, đi cùng `7.1`/`2.2` | tôi |
| — | `external response/authority` vẫn `NOT_RECEIVED`; lượt này không mở gate production nào | ngoài IVR |
