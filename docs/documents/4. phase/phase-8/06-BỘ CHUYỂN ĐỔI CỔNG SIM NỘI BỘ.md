# IVR-06 - Internal SIM Gateway Adapter

Trạng thái: `SRS_BASELINE`  
Phase: 8 - IVR Order Confirmation  
Mục tiêu: Khóa ranh giới kỹ thuật của Internal SIM Gateway Adapter.

## 1. Mục tiêu

Internal SIM Gateway Adapter là lớp duy nhất được phép giao tiếp với SIM Gateway phần cứng/API. Adapter chỉ dial, phát script, capture call status/DTMF và báo lỗi kỹ thuật. Adapter không biết business state và không được cập nhật order.

## 2. Nguồn tham chiếu

| Nguồn | Vai trò |
| --- | --- |
| `docs/documents/2. pack/09-PACK-09-IVR-ORDER-CONFIRMATION.md` | Deployment model và SIM capacity. |
| `docs/documents/4. phase/phase-8/ivr-input-baseline.md` | SIM Gateway, call execution, DTMF capture. |
| `docs/documents/3. tech/10-TECH-09-IVR-ORDER-CONFIRMATION-AUTO-CALL-VERIFICATION-ANTI-FAKE-ORDER-CONTROL.md` | Technical failure không phải customer fault. |

## 3. Trách nhiệm của adapter

Adapter được phép:

- Quản lý SIM channel pool.
- Reserve/release SIM channel.
- Dial official contact token/phone theo task hợp lệ.
- Phát approved call script.
- Capture call status.
- Capture DTMF.
- Báo technical error code.
- Ghi SIM health metrics.
- Trả raw adapter result về IVR Runtime.

Adapter không được:

- Tạo/cập nhật/hủy order.
- Gửi SMS hoặc notification.
- Tự retry ngoài scheduler policy.
- Tự xác định khách trusted/untrusted.
- Tự bỏ qua blocker.
- Ghi evidence accepted/pass.

## 4. SIM channel model

| Rule | Yêu cầu |
| --- | --- |
| `ONE_SIM_ONE_ACTIVE_CALL` | Một SIM chỉ xử lý một outbound call active. |
| `CHANNEL_HEALTH_REQUIRED` | Chỉ SIM healthy mới được reserve. |
| `CHANNEL_RELEASE_REQUIRED` | Mọi call kết thúc/lỗi phải release channel. |
| `IN_FLIGHT_AUDIT_REQUIRED` | Mọi in-flight call có audit/correlation. |

## 5. Dữ liệu vào của adapter

Adapter chỉ nhận từ IVR Runtime:

- `call_job_id`.
- `attempt_id`.
- `correlation_id`.
- `sim_channel_id` nếu đã reserve.
- `phone_adapter_token` hoặc dialing material được privacy/security duyệt.
- `call_script_template_id`.
- `call_script_version`.
- `allowed_script_variables`.
- `attempt_deadline`.

## 6. Dữ liệu ra của adapter

Adapter trả:

- `adapter_result_id`.
- `attempt_id`.
- `sim_channel_id`.
- `call_started_at`.
- `call_ended_at`.
- `call_duration_seconds`.
- `raw_call_status`.
- `raw_dtmf_key`.
- `technical_error_code` nếu có.
- `adapter_trace_ref`.

Raw result chưa được dùng trực tiếp cho Order Core. Phải qua Result Normalizer ở `IVR-07`.

## 7. Phân loại lỗi kỹ thuật

| Error | Ý nghĩa | Counted customer attempt? |
| --- | --- | --- |
| `SIM_GATEWAY_ERROR` | Gateway/hardware lỗi | Không |
| `SIM_CHANNEL_FAILURE` | SIM lỗi hoặc mất channel | Không |
| `SERVER_ERROR` | Server/adaptor lỗi | Không |
| `AUDIO_PLAYBACK_ERROR` | Không phát được script | Không |
| `DTMF_CAPTURE_ERROR` | Không capture được phím | Không |
| `SCHEDULER_ERROR` | Lỗi trước dispatch | Không |
| `INTERNAL_CALLBACK_ERROR` | Lỗi callback Core | Không |
| `EVIDENCE_WRITE_ERROR` | Không ghi được evidence | Không |

## 8. Retry kỹ thuật

Technical retry phải:

- Có giới hạn.
- Không kéo dài program window.
- Không tăng customer attempt count.
- Không gọi duplicate callback.
- Có evidence/incident metrics.

`Owner Decision Required`:

- `MAX_TECHNICAL_RETRY_PER_ATTEMPT`.
- `TECHNICAL_RETRY_BACKOFF_SECONDS`.
- Mapping `BUSY`, `REJECTED`, `UNREACHABLE`, `CALL_DROPPED` theo tín hiệu SIM Gateway thực tế.
- Production SIM Gateway hardware/API protocol.

## 9. Giám sát sức khỏe hệ thống

Adapter phải expose trạng thái:

- SIM healthy/degraded/disabled.
- Active call count.
- Channel reserve failure.
- DTMF capture failure rate.
- Audio playback failure rate.
- Callback failure rate.
- Capacity risk.

Disable/enable SIM phải đi qua permission trong `IVR-08`.

## 10. Tiêu chí chấp nhận

- Adapter không có quyền update order.
- Adapter không gửi notification.
- Technical failure được tách khỏi no-answer.
- SIM health và channel lifecycle có audit.
- Các owner decision kỹ thuật còn thiếu được đánh dấu rõ.

## 11. Adapter interface detail

| Operation | Input | Output | Side effect |
| --- | --- | --- | --- |
| `ReserveSimChannel` | Attempt context, queue, program. | SIM reservation or capacity failure. | Marks SIM reserved. |
| `ReleaseSimChannel` | Reservation id, final status. | Release ack. | Marks SIM idle/degraded. |
| `DialOutboundCall` | SIM reservation, dial token, script version. | Provider/call id. | Starts call. |
| `PlayScript` | Approved script template and variables. | Playback status. | Audio playback. |
| `CaptureDtmf` | Active call id. | DTMF key/status. | Captures customer input. |
| `GetCallDisposition` | Active call id. | Raw call outcome. | None. |
| `HealthCheck` | SIM channel id. | Healthy/degraded/failed. | Updates health evidence. |

## 12. SIM channel lifecycle

```text
ENABLED_IDLE
  -> RESERVED
  -> ACTIVE_CALL
  -> RELEASING
  -> ENABLED_IDLE

Any state -> DISABLED_BY_ADMIN
Any active state -> HEALTH_FAILED
HEALTH_FAILED -> DISABLED_BY_ADMIN or ENABLED_IDLE after review
```

Rules:

- `RESERVED` must have TTL.
- `ACTIVE_CALL` must have exactly one attempt.
- `DISABLED_BY_ADMIN` cannot be selected by scheduler.
- `HEALTH_FAILED` cannot auto-enable without health pass/review.

## 13. Raw outcome taxonomy

| Raw adapter outcome | Normalized category | Counted customer attempt |
| --- | --- | --- |
| Answered + DTMF `1` | `IVR_CONFIRMED` | Yes |
| Answered + DTMF `0` | `IVR_CUSTOMER_CANCELLED` | Yes |
| Ring timeout under policy | `NO_ANSWER` | Yes |
| Invalid key | `DTMF_INVALID` | Owner policy |
| DTMF timeout after answer | `DTMF_TIMEOUT` | Owner policy |
| Busy | `BUSY` | Owner Decision Required |
| Rejected | `REJECTED` | Owner Decision Required |
| Unreachable | `UNREACHABLE` | Owner Decision Required |
| SIM gateway error | `SIM_GATEWAY_ERROR` | No |
| Audio playback error | `AUDIO_PLAYBACK_ERROR` | No |
| DTMF capture error | `DTMF_CAPTURE_ERROR` | No |
| Callback/evidence write error | Technical exception | No |

## 14. Script delivery rules

Adapter may only play:

- Approved script template.
- Approved script version.
- Approved variables.

Adapter must not play:

- Full customer address.
- Full customer profile.
- Payment detail.
- Health/sensitive notes.
- CRM/AI consultation content.
- Marketing/upsell message.

## 15. Adapter security controls

| Control | Requirement |
| --- | --- |
| Credential scope | SIM credential only, no Order Core write credential. |
| Secret storage | Secret manager/config boundary, no UI/log exposure. |
| Logging | No raw phone unless explicitly approved; redact tokens. |
| Network | Internal network only. |
| Admin | Disable/enable through IVR admin API, not adapter local button. |
| Recording | Disabled unless privacy/legal owner approves. |

## 16. Adapter failure handling

| Failure | Required output |
| --- | --- |
| Cannot reserve SIM | Capacity incident or technical exception. |
| Dial fails before ring | Technical exception, not no-answer. |
| Audio fails | Technical exception, not customer attempt. |
| DTMF capture fails | Technical exception. |
| Call drops after answered | `CALL_DROPPED`, owner decision for count. |
| SIM hangs active | Health failed, admin review. |

## 17. Adapter smoke tests

| Test ID | Scenario | Expected |
| --- | --- | --- |
| IVR06-SMK-001 | Fake adapter DTMF `1` | `IVR_CONFIRMED`. |
| IVR06-SMK-002 | Fake adapter DTMF `0` | `IVR_CUSTOMER_CANCELLED`. |
| IVR06-SMK-003 | Fake no-answer | `NO_ANSWER`. |
| IVR06-SMK-004 | SIM gateway error | Technical exception, not counted. |
| IVR06-SMK-005 | Disabled SIM selected | Scheduler refuses. |
| IVR06-SMK-006 | Two calls same SIM | Second reserve fails. |
| IVR06-SMK-007 | Raw phone in logs | Test fails. |
