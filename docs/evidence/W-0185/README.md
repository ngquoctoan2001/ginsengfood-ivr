# W-0185 — B3 VieNeu TTS, real-SIM lab và production telephony evidence validator

Ngày: 2026-09-04

Baseline audit: main@8ed62e93f5ec0ff7a4c694181ac73ee04f1eb34b.

Trạng thái: **TESTS_PASS_LOCAL / OFFLINE_EVIDENCE_VALIDATOR_READY /
REAL_SIM_AND_PRODUCTION_EVIDENCE_NOT_RECEIVED**.

## 1. Kết quả audit

Phần local của VieNeu-TTS không còn là khoảng trống adapter:

- source/model/voice đã pin; ba giọng Bắc/Trung/Nam đã được Owner chọn;
- shim trả raw mono 16-bit audio/L16 8 kHz và fail closed khi provenance/acceptance sai;
- fixed catalog có 12/12 file và local container/Compose/Helm/safety gate đã có;
- software lab W-0104 chỉ dùng fake data + MicroSIP, không chứng minh SIM/PSTN/carrier.

Các bằng chứng B3 chưa tồn tại vẫn là:

1. đúng sáu lượt 2 fake order × 3 miền qua route Asterisk/MicroSIP 8 kHz trên target hardware;
2. tám scenario one-SIM LAB-01..LAB-08, gồm DTMF, no-input, no-answer, rejected và kill switch;
3. exact vendor/model/SKU/firmware/VoLTE, SIM/carrier và số đích do nội bộ sở hữu;
4. media topology, caller ID, RFC4733, recording-off, CDR↔attempt, allowlist và secret custody;
5. retention/purge/rollback drill;
6. target-hardware performance, capacity recalibration, internal mirror và procurement decision;
7. Legal/Privacy, Security, Platform, Telephony, Procurement, Product và Release approval.

Vì các mục trên cần thiết bị, SIM, vendor artifact và chữ ký thật, W-0185 không tự tạo kết quả
PASS cho chúng. REAL_CUSTOMER_CALL_ALLOWED=NO giữ nguyên.

## 2. Phần local đã bổ sung

CLI deploy/ci/scripts/b3-telephony-evidence-validator.mjs cung cấp một điểm intake fail-closed,
metadata-only cho hồ sơ B3 khi external input được gửi tới.

Validator khóa:

- chín source contract/provenance file hiện tại bằng SHA-256;
- exact candidate gồm full IVR commit, TTS image digest, model bundle, fixed catalog và config hash;
- canonical candidate hash và canonical bundle hash, đều phải khớp pin reviewer độc lập;
- đúng sáu TTS call theo thứ tự Bắc/Trung/Nam × fake order A/B, DTMF 1/0, result tương ứng;
- đúng tám one-SIM scenario LAB-01..LAB-08;
- exact model/SKU/firmware/VoLTE và topology
  TTS→IVR worker→Asterisk→SIP gateway→SIM/PSTN→owner device;
- resolver ở telephony boundary, IVR không thấy E.164 và không giữ mapping key;
- recording tắt, allowlist + kill switch đã kiểm, credential từ secret store, CDR dùng opaque ID;
- retention/purge/rollback và production vẫn default disabled;
- mười artifact role và bảy sign-off role đúng thẩm quyền, signer khác verifier;
- pin artifact truyền ngoài file bằng mười lần --expected-artifact ROLE=SHA256;
- input regular/non-symlink trong repo, UTF-8 không BOM, tối đa 512 KiB, không duplicate JSON key;
- từ chối phone/email/dialable URI/private key/credential-like material trong JSON.

Output B3_TELEPHONY_EVIDENCE_PASS chỉ có nghĩa **eligible for evidence review**. Nó không tự
đặt LAB_REAL_SIM_VERIFIED, PRODUCTION_REAL_ELIGIBLE hoặc cho phép pilot/cutover.

## 3. Template và cách dùng

Template: `docs/evidence/W-0185/b3-telephony-evidence.template.json`.

Kiểm template pending:

~~~text
node deploy/ci/scripts/b3-telephony-evidence-validator.mjs --check-template docs/evidence/W-0185/b3-telephony-evidence.template.json
~~~

Template cố ý ở PENDING_EXTERNAL_INPUT, mọi kết quả quan sát còn PENDING/false; nó hợp lệ
về hình dạng nhưng bị từ chối nếu đưa vào --input.

Khi nhận đủ hồ sơ thật:

1. copy template sang file mới, chỉ dùng alias như LAB-A; không dán số thật, audio, transcript,
   CDR raw hoặc credential;
2. điền kết quả sáu TTS call và tám real-SIM scenario từ quan sát thật;
3. điền mười artifact ref/hash, bảy chữ ký và exact candidate;
4. tính candidate_sha256 trên object candidate sau khi bỏ chính field đó, canonical JSON
   key-sort; tính bundle_sha256 tương tự trên toàn bundle sau khi bỏ field hash;
5. reviewer lấy candidate/bundle/artifact pin từ trust source độc lập;
6. chạy --input cùng --expected-bundle-sha, --expected-candidate-sha, --expected-ivr-sha,
   --expected-tts-image-digest và mười --expected-artifact ROLE=SHA256.

Danh sách exact artifact role nằm sẵn trong template và CLI usage. Không được lấy expected hash từ
chính completed bundle.

## 4. Local verification

- Node syntax: PASS.
- Validator self-test: 1 valid fixture, 34 refusal cases, exact 6 TTS calls,
  8 real-SIM scenarios, 10 artifacts và 7 sign-offs.
- Pending template: valid-not-ready; completed intake: refused.
- Artifact manifest: 13/13 source/script/template/CI/evidence pin PASS.
- Không gọi network, không dựng SIM call, không đọc/lưu audio, không sửa runtime/adapter/config.
- W-0122 regression không cần Docker PASS: provenance, audition, voice-acceptance và fixed-render.
  Helm/container/converter rerun hiện ENV_BLOCKED / NOT_RUN: đã khởi động Docker Desktop nhưng
  backend không trả lời bounded CLI probe; không tái gán kết quả lịch sử và không coi đây là
  regression của CLI W-0185.

## 5. Boundary và bước tiếp theo

W-0122 vẫn BLOCKED_EXTERNAL. Bước kế tiếp có giá trị là lấy exact model/SKU + thiết bị/SIM,
cho phép chỉ gọi LAB-A do Owner kiểm soát, rồi chạy theo
[lab-runbook.md](../W-0122/lab-runbook.md) và
[one-sim-lab-plan.md](../../lab/one-sim-lab-plan.md). Sau đó điền bundle và để reviewer chạy
validator với pin độc lập.

Cho đến khi đủ artifact/chữ ký và pilot/cutover riêng:

- LAB_REAL_SIM_VERIFIED=NO;
- PRODUCTION_REAL_ELIGIBLE=NO;
- REAL_CUSTOMER_CALL_ALLOWED=NO.
