# W-0317 — Lô 4: phần làm được ngay của nhánh VieNeu

**Ngày:** `2026-09-18` · **Baseline:** `main@e71ff00` · **Loại:** image, cấu hình Helm nháp, tài liệu · **`0` file `.cs`**

`REAL_CUSTOMER_CALL_ALLOWED=NO`

---

## 0. Tóm tắt

Ba bước *"làm được ngay, không chờ ai"* của Lô 4 trong
[bản vướng mắc `17/09`](../../../plan/ivr-orther/vuong-mac-va-quyet-dinh-2026-09-17.md): bước `1`, `2`, `4`. Bước `3`
đã xong ở `W-0315`.

| Bước | Kết quả |
| --- | --- |
| `1` Quét lại bằng Trivy ghim | **`57`** lỗ `HIGH`/`CRITICAL`, không phải `16`: `54 HIGH` · `3 CRITICAL`, `13` đã có bản vá Debian |
| `1` Vá cái vá được | Ghim `4` gói Debian có bản vá ⇒ **`44 HIGH`, `0 CRITICAL`, `0` còn bản vá để lấy**. Đã sửa trong `Dockerfile.tts` |
| `2` Thử đổi bản cài nền | Debian distroless: `21 HIGH` ngay ở image gốc — không đủ. **Chainguard: `0`**, qua `tts-container-selftest` và `tts-helm-selftest` — **chưa đổi hẳn**, xem §3 |
| `2` Đo lỗ còn lại có bị gọi tới không | Không: tiến trình chạy thật không nạp thư viện nào của `44` lỗ còn lại |
| `4` Cấu hình production nháp | `deploy/helm/ivr/values-prod-tts.draft.yaml`. Để trống đúng `14` ô còn chờ; render chứng minh thiếu bất kỳ ô nào cũng bị từ chối |

`values-prod.yaml` vẫn `tts.enabled: false`. Không tải bundle model: việc tải cần Toàn cho phép, và các
kiểm tra của bước `2` trong kế hoạch không cần tới nó.

---

## 1. Bước `1` — quét lại

Cùng image Trivy ghim với `W-0122` (`aquasec/trivy@sha256:7cced7ca…`, `v0.73.0`), DB tải ngày `2026-09-18`.
Image build lại từ `Dockerfile.tts` tại `e71ff00` (dùng cache), `126,108,829` bytes — đúng bằng kích thước bản
`28/08`.

| Nhóm | Lỗ | Trạng thái |
| --- | ---: | --- |
| `perl-base` | `8` (`3 CRITICAL`) | `7` có bản vá `5.40.1-6+deb13u1`; `1` `fix_deferred` |
| util-linux (`bsdutils`, `libblkid1`, `liblastlog2-2`, `libmount1`, `libsmartcols1`, `libuuid1`, `login`, `mount`, `util-linux`) | `36` | `4` CVE trên `9` gói, chưa có bản vá — **mới** so với `28/08` |
| `ncurses` (`4` gói) | `4` | cùng một CVE, chưa có bản vá |
| `libpcre2-8-0` | `3` | có bản vá `10.46-1~deb13u2` — **mới** |
| `libsqlite3-0` | `2` | có bản vá `3.46.1-7+deb13u2` |
| `libsystemd0`, `libudev1` | `2` | chưa có bản vá — **mới** |
| `gzip` | `1` | có bản vá `1.13-1+deb13u1` |
| `libacl1` | `1` | chưa có bản vá |
| **Tổng** | **`57`** | `13` có bản vá |

Con số `16` của `28/08` là đúng **với DB lúc đó**. `SEC-C1` của phiếu bảo mật cũ đã hỏi đúng điều này: có lấy
DB tại thời điểm đo làm mốc không. Câu trả lời thực tế là không lấy được: DB đổi hằng ngày.

Ghim thêm `perl-base`, `gzip`, `libpcre2-8-0`, `libsqlite3-0` vào khối `apt-get` của stage runtime, như ba gói
OpenSSL đã được ghim. Build lại, quét lại: **`44 HIGH`, `0 CRITICAL`, `0` có bản vá**, `8` CVE riêng biệt.
Image `129,675,750` bytes.

---

## 2. Bước `2` — lỗ còn lại có bị gọi tới không

Theo cách `W-0185` đã đo, nhưng đo trên image mới. Chạy trong image, với user runtime, `--network none`,
`--read-only`, `--cap-drop ALL`: nạp module entrypoint `shim.server`, class engine
`vieneu._v3_turbo_engine.onnx_runtime_lite.OnnxV3LiteEngine` và `5` thư viện native (`numpy`, `onnxruntime`,
`sea_g2p`, `soxr`, `tokenizers`), rồi đọc `/proc/self/maps`.

| Đo | Kết quả |
| --- | --- |
| Shared object đã map | `46` |
| Trong đó thuộc gói còn lỗ (`libuuid`, `libblkid`, `libmount`, `libsmartcols`, `libsystemd`, `libudev`, `libncurses`, `libtinfo`, `libacl`, `liblastlog`, `libperl`) | **`0`** |
| `subprocess` được import | Không |
| Module `vieneu` đã nạp | `5`, không có `serve.py` |

**Giới hạn:** không có bundle model nên engine chưa khởi tạo session ONNX thật. Nạp model có thể map thêm
thư viện; `W-0185` đo lúc có model cũng không thấy luồng nào gọi tới các gói này.

---

## 3. Bước `2` — thử đổi bản cài nền

| Image | `HIGH`/`CRITICAL` | Ghi chú |
| --- | ---: | --- |
| Hiện tại, Debian `13.6`, trước khi vá | `57` | |
| Hiện tại, sau khi ghim `4` gói | `44` | **Đang dùng** — `Dockerfile.tts` |
| `gcr.io/distroless/python3-debian13` (image gốc, chưa thêm gì) | `21` | Chính gói Python của Debian (`3` CVE × `4` gói), `libexpat1`, `libuuid1`, `ncurses`. Không đủ để bỏ rủi ro 3 |
| `cgr.dev/chainguard/python` (Wolfi), toàn bộ image `ivr-tts` | **`0`** | Cả gói hệ điều hành lẫn gói Python |

Image thử trên nền Chainguard (Dockerfile ở cuối file này, **không** nằm trong `deploy/`):

| Kiểm | Kết quả |
| --- | --- |
| Build với đúng `runtime-requirements.lock` | `Resolved 24 packages`, `Installed 24 packages` — mọi hash khớp, có đủ wheel cho Python `3.14.7` |
| `tts-container-selftest` | `15/15` test của shim · `TTS_CONTAINER_CONTRACT_PASS` · non-root `1654:1654` · không cổng · không mạng |
| Nạp thư viện | `numpy 2.3.4`, `onnxruntime 1.24.4`, `soxr 1.0.0`, `tokenizers 0.22.2`, `sea_g2p`, engine `vieneu` |
| Có gì trong image | Không có `/bin/sh`, `/bin/bash`, `perl`, `gzip`, `apk`, `uv` |
| Kích thước | `105,760,872` bytes — nhỏ hơn bản Debian `20` MB |
| Đo như §2 | `45` shared object, `0` thuộc gói trong danh sách, `subprocess` không được import |

**Vì sao chưa đổi hẳn** — hai điều không phải việc tôi tự làm được:

1. **Chưa có lượt đọc thật.** Python đổi từ `3.12` sang `3.14`. Test của shim và việc nạp thư viện đều qua,
   nhưng chưa tạo được một câu nói bằng model thật: bundle (`~211 MB`, Hugging Face, revision đã ghim, hash
   trong `MODELS.lock`) không có trên máy này, và tải nó cần Toàn cho phép.
2. **Phụ thuộc một nhà cung cấp ngoài.** Image miễn phí của Chainguard chỉ có tag `latest`, và phải ghim theo
   digest. Muốn build lại được về sau, image cần được chép vào kho bản cài nội bộ (`S5`). Đổi hay không nằm ở
   rủi ro 3 của `S2`: Sếp **ký** với `44` lỗ mức cao, hoặc **cho đổi** và rủi ro 3 biến mất.

Manifest nghiệm thu giọng ràng vào hash của `runtime-requirements.lock` và `MODELS.lock`. Cả hai file không đổi
trong lô này, nên manifest vẫn khớp với cả hai image.

---

## 4. Bước `4` — cấu hình production nháp

`deploy/helm/ivr/values-prod-tts.draft.yaml`. Không gate nào đọc file này — mọi gate đọc
`values-<môi trường>.yaml` theo tên. `values-prod.yaml` không đổi.

**Điền sẵn:**

- ba giọng Owner chọn ngày `28/08` (`OD-VOICE-06`);
- catalog `12` đoạn cố định, lấy từ `deploy/lab/asterisk/audio/segments-appsettings.json`;
- hash `MODELS.lock`, trùng `model_lock_sha256` trong manifest nghiệm thu;
- tên ConfigMap nghiệm thu giọng và lệnh tạo nó từ đúng một nguồn;
- `voiceAcceptanceRef`.

**Để trống**, mỗi ô ghi ai điền: `governance.executionMode`, `image`, `modelBundle.existingClaim`, `mediaSink`,
bốn tham chiếu phê duyệt, `resources`.

Render bằng `alpine/helm:3.16.3`, như `tts-helm-selftest`:

| Lượt | Kết quả |
| --- | --- |
| Nháp đặt lên `values-prod.yaml` | **Bị từ chối** — ô đầu tiên chart kiểm: `executionMode` |
| Nháp + `14` giá trị `TEST_ONLY` cho đúng các ô trống | **Render được**: `785` dòng, có sidecar `vieneu-tts`, giọng `v3t-north-ngoc-linh`, ConfigMap `ivr-tts-voice-acceptance` |
| Bỏ ra **một** trong `14` giá trị đó, lần lượt | **Bị từ chối `14/14`**, `10` thông báo khác nhau |

Vậy danh sách ô trống trong đầu file nháp là **đủ và không thừa**.

---

## 5. Kiểm chứng

| Hạng mục | Kết quả |
| --- | --- |
| Ghim hash | `0` chỗ ghim trên các file chạm (kiểm trước khi sửa) |
| Report Trivy (git bỏ qua) | `artifacts/sbom/w0317-trivy-current.json` `f3a5b20d…` · `w0317-trivy-patched.json` `85d4be66…` · `w0317-trivy-chainguard-trial.json` `f10d1d11…` · `w0317-trivy-distroless-debian13-base.json` `f4af0c21…` |
| Gate sweep (Git Bash) | **`GATE_SWEEP_PASS 40/40`** — `tts-container-selftest` build lại image từ `Dockerfile.tts` đã vá; `dr-selftest` `163.6s` |
| `gate-status.mjs --write` rồi kiểm | `GATE_STATUS_PASS` — `11` gate, `305` work item, `6` OD mở |
| Quét PII `docs/evidence` | `PII_SCAN_PASS files=389` |
| Gitleaks trên các dòng thêm vào | `no leaks found` — `zricethezav/gitleaks:v8.30.0`, `.gitleaks.toml` của repo, trên một cây nháp chỉ chứa các dòng thêm vào |
| `detect_changes` | risk `low`, `0` luồng bị ảnh hưởng — không symbol .NET nào đổi |

Không chạy `dotnet test`: lô này không chạm mã .NET, không chạm test.

Image nền đã kéo về để thử: `gcr.io/distroless/python3-debian13@sha256:8ee21484…`,
`cgr.dev/chainguard/python@sha256:3402da06…` (runtime) và `@sha256:073219a5…` (build).

---

## 6. Còn lại

- **Một lượt đọc thật** trên image mới khi có bundle model — trước khi đổi sang Chainguard, và cũng là lượt
  lab VieNeu còn nợ từ `W-0315`.
- **`S2` rủi ro 3**: Sếp ký với `44` lỗ, hoặc cho đổi nền.
- Phần **Chờ** của Lô 4: đo trên máy thật (`S5`), nghe duyệt `12` đoạn (`S1`), `6` cuộc gọi thử (`S5`), ký rủi ro
  3 và 4 (`S2`), rồi mới bật `tts.enabled`.

---

## Phụ lục — Dockerfile thử trên nền Chainguard

Khác `Dockerfile.tts` ở ba chỗ:

- hai image nền;
- stage runtime không có `apt-get` và `useradd`, vì image không có shell; UID `1654` vẫn đặt bằng số;
- venv dựng bằng Python `3.14` của chính image build Chainguard.

```dockerfile
# syntax=docker/dockerfile:1.7
# W-0317 trial, kept as evidence only (not under deploy/): the same image on a Chainguard (Wolfi)
# Python base, which carries no perl, util-linux, ncurses or shell in the runtime stage.
ARG UV_IMAGE=ghcr.io/astral-sh/uv:0.8.14@sha256:d97bc3f40af096399f67e8e69e10b7735f3dbc6fed300391637ecb00f37af981
ARG BUILD_IMAGE=cgr.dev/chainguard/python@sha256:073219a510343c070f74fd1fd7100b9e599cf68eb1536be8ebd4c0b170ede589
ARG RUNTIME_IMAGE=cgr.dev/chainguard/python@sha256:3402da0629d26501855f13d490b7fa1b525e4a5db3a10af324277e5507fcb8e6

FROM ${UV_IMAGE} AS uv
FROM ${BUILD_IMAGE} AS builder

USER 0:0
COPY --from=uv /uv /usr/local/bin/uv

WORKDIR /opt/vieneu
COPY deploy/tts/runtime-requirements.lock /opt/ivr-runtime/runtime-requirements.lock
RUN --mount=type=cache,target=/root/.cache/uv \
    uv venv --python /usr/bin/python3.14 /opt/vieneu/.venv \
    && uv pip sync \
        --python /opt/vieneu/.venv/bin/python \
        --require-hashes \
        --only-binary :all: \
        /opt/ivr-runtime/runtime-requirements.lock

FROM ${RUNTIME_IMAGE} AS runtime

WORKDIR /opt/vieneu
COPY --from=builder /opt/vieneu/.venv /opt/vieneu/.venv
COPY --from=builder /opt/ivr-runtime/runtime-requirements.lock /opt/ivr-runtime/runtime-requirements.lock
COPY third_party/vieneu-tts/src/ /opt/vieneu/src/
COPY third_party/vieneu-tts/uv.lock /opt/vieneu/uv.lock
COPY third_party/vieneu-tts/LICENSE /opt/vieneu/LICENSE

WORKDIR /opt/ivr-tts
COPY deploy/tts/shim/ /opt/ivr-tts/shim/
COPY deploy/tts/models/MODELS.lock /opt/ivr-tts/models/MODELS.lock

ENV PATH="/opt/vieneu/.venv/bin:${PATH}" \
    PYTHONPATH="/opt/ivr-tts:/opt/vieneu/src" \
    PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    HF_HUB_OFFLINE=1 \
    VIE_NEU_BACKEND=vieneu-onnx \
    VIE_NEU_HOST=127.0.0.1 \
    VIE_NEU_PORT=8090 \
    VIE_NEU_BUNDLE_ROOT=/models \
    VIE_NEU_MODEL_LOCK=/opt/ivr-tts/models/MODELS.lock \
    VIE_NEU_RUNTIME_LOCK=/opt/ivr-runtime/runtime-requirements.lock \
    VIE_NEU_DEPENDENCY_LOCK=/opt/vieneu/uv.lock \
    VIE_NEU_VOICE_CONFIG=/opt/ivr-tts/shim/voices.json \
    VIE_NEU_VOICE_ACCEPTANCE_MANIFEST=/run/ivr-tts/voice-acceptance-manifest.json \
    VIE_NEU_UPSTREAM_VOICE_MANIFEST=/opt/vieneu/src/vieneu/assets/voices_v3_turbo.json

USER 1654:1654

ENTRYPOINT ["python", "-m", "shim.server"]
```
