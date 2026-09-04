# W-0122 — Security, supply-chain và performance evidence

Ngày đo: `2026-08-27`  
Baseline source: `main@f291f44`  
Evidence class: `LOCAL/NONPROD`; không phải production acceptance

## Candidate image đã đo

| Thuộc tính | Giá trị |
| --- | --- |
| Local image | `ivr-tts:w0122-hardened` |
| Image ID/digest local | `sha256:4c76d318e24110267c908594031863cd7dbe3f31c92569c4e02b4de3ba9ba30d` |
| Kích thước unpacked | `126,109,579` bytes; giảm `20,854,618` bytes (`14.2%`) sau khi tách builder và loại tool/UI/training/test/sample khỏi runtime |
| Base | `python:3.12.14-slim-trixie@sha256:7a8b475003c4fe15a2cd4e55e5cfc2f3560bdc9333d624f24cdd6d4340fd7a17` |
| Runtime lock | 24 packages; SHA-256 `a2f18ce29167f97e1e11f9b1d9802378c6dc4997ddcfcdc99d04a54c77956304` |
| OpenSSL security delta | `libssl3t64`, `openssl`, `openssl-provider-legacy` = `3.5.7-1~deb13u2` |
| Runtime identity | `1654:1654` |
| Container boundary đã thử | read-only rootfs; `network=none`; không publish/expose port; drop `ALL`; `no-new-privileges` |

Production runtime chỉ chứa đường local preset-based ONNX inference. Dependency cho upstream web
UI/API, training và voice cloning không được đưa vào image. `HF_HUB_OFFLINE=1`; production vẫn
phải dùng model bundle từ internal mirror đã duyệt.

## Kết quả chức năng/security local

| Gate | Kết quả |
| --- | --- |
| Shim unit/negative | `15/15 PASS` — positive + mutation cho Owner voice-acceptance contract, cộng ba test mới đi thẳng vào readiness path thật (`VieNeuBackend.load()`). Đây là chính bộ test mà container selftest chạy bên trong image, không phải một lượt đếm riêng |
| Container contract | `PASS` — non-root, read-only, no network, no port |
| Runtime imports | `PASS` — NumPy, ONNX Runtime, SEA-G2P, soxr, tokenizers |
| Minimal content | `PASS` — `uv` cùng 8 nhóm upstream UI/deploy/example/training/test/reference-sample path không tồn tại trong runtime |
| Real ONNX startup | `PASS` — `/health/ready=200` trên locked nonprod model bundle |
| Real request | `PASS` — `200`, `audio/L16`, 8 kHz contract, `20,480` bytes, `3,459 ms` local observation |
| Production fail-closed | `PASS` — thiếu manifest, mount template pending hoặc bật audition trong `PRODUCTION_REAL` đều `/health/ready=503` |
| Privacy-safe log | `PASS` — chỉ `startup/request status` và latency bucket; không text/body/audio/traceback |

`3,459 ms` là một quan sát đơn trên máy dev, không phải p95 và không đại diện target hardware.
Startup không được đo như benchmark độc lập trong lượt này. Target CPU/RAM, cold/warm full-playlist p95, RSS,
queue wait, expected concurrency, request/character budget và lease/pre-dial headroom vẫn
`ENV_BLOCKED` cho tới khi Infra cung cấp môi trường đích.

## Re-scan `2026-08-28` trên image đã rebuild sau W-0126

| Mục | Giá trị |
| --- | --- |
| Image | `ivr-tts:w0122-selftest`, ID `sha256:63fe84e6e090a79a044a495e4faff9fce1bf1e8d4c8f1de4f41ea381ad00a910` |
| Kích thước | `126,109,393` bytes — nhỏ hơn bản `2026-08-27` đúng `186` bytes, đúng bằng chênh lệch line ending của `uv.lock` |
| Trivy report | `artifacts/sbom/w0122-trivy-image-lf.json`, SHA-256 `28fa73ad4fc7a9728161e85b4dfd77e6c3d2c2c2072b551533be68841991ec80` |
| SPDX SBOM | `artifacts/sbom/w0122-ivr-tts-lf.spdx.json`, SHA-256 `77e1c65754c7a6dd94655db726732a640f0266d6f19cb6d60e838a0e7dc098e3`; `114` package entry ở lượt scan này |
| Kết quả | `13 HIGH`, `3 CRITICAL`, `0 fixable` — **không đổi**; toàn bộ thuộc Debian 13.6; Python target `0` |

`114` package khác `152` của lượt trước là do lượt scan/format khác, **không** phải image nhỏ đi; nội dung package không đổi. Reachability của 16 finding đã được đo trong chính image; structured disposition hiện hành được nhận qua [W-0185](../W-0185/README.md): entrypoint là `python -m shim.server`; shim không có đường spawn process nào; `vieneu/serve.py` — file duy nhất chứa `subprocess` — không nằm trên import path; và `subprocess` không được import ở cả hai lượt kiểm.

## SBOM và vulnerability scan

| Artifact/tool | Kết quả |
| --- | --- |
| SPDX SBOM | `artifacts/sbom/w0122-ivr-tts.spdx.json`; 152 SPDX package entries; SHA-256 `033674e7b43dc9edbd04dbf7f932f3459e62bc372f13f1c574bf37c9088508e8` |
| Trivy | `v0.73.0`, image pin `aquasec/trivy@sha256:7cced7cae583819fc7806d4cbc0dbbc7cad18b99f7d3e235192e6da8c091045c` |
| Saved candidate image | `artifacts/sbom/w0122-ivr-tts-image.tar`; SHA-256 `4dd0ab79b71e18ae1dd1ecd412e7b4f0cb1ce77969aaf05ba9d0f2583512056a` |
| Trivy report | `artifacts/sbom/w0122-trivy-image-high-critical.json`; SHA-256 `1fa6ff8ddeada90127884c2963bd9e2d9e992d684e6dcac937d05675e20db72f` |
| Final result | `13 HIGH`, `3 CRITICAL`, `0 fixable` theo DB tại thời điểm đo; tất cả thuộc Debian 13.6; Python target có `0 HIGH/CRITICAL` |
| Historical pre-hardening scan | `91 HIGH`, `9 CRITICAL`, `64 fixable`; chỉ dùng để chứng minh remediation delta, không phải kết quả candidate cuối |

Ba CRITICAL còn mở:

| CVE | Package/version | Trivy status | Fixed version |
| --- | --- | --- | --- |
| `CVE-2026-13221` | `perl-base 5.40.1-6` | `affected` | chưa có |
| `CVE-2026-42496` | `perl-base 5.40.1-6` | `fix_deferred` | chưa có |
| `CVE-2026-8376` | `perl-base 5.40.1-6` | `affected` | chưa có |

Mười ba HIGH còn lại nằm ở `ncurses`, `sqlite3`, `gzip`, `perl-base` và `libacl1`; report không
đưa ra fixed version. Không xóa cưỡng bức các package nền/essential để làm đẹp scanner vì có thể
phá tính toàn vẹn của Python base image.

## Kết luận gate

`RELEASE_BLOCKED`.

Local hardening đã xử lý toàn bộ finding có fixed version trong candidate này và loại toàn bộ
HIGH/CRITICAL ở Python dependency set. Release chỉ được xem lại khi:

1. base image/OS có bản sửa hoặc Security/Release owner ký disposition cho đúng 16 finding;
2. Trivy/SBOM được chạy lại trên image digest được đẩy vào internal registry;
3. Legal/Privacy, model internal mirror, owner voice acceptance, target performance và production
   topology đều có artifact phê duyệt riêng.

Không kết quả nào trong file này đổi `REAL_CUSTOMER_CALL_ALLOWED=NO`.
