# Performance, capacity and security/privacy report

Work `W-0037` (prompt `P5-3`) · Ngày `2026-08-18` · Chế độ `MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO`

> Mọi số dưới đây đo trên **mock SIM + PostgreSQL thật (Testcontainers)**. Không có cuộc gọi thật,
> không có PII thật, không có SIM thật. Năng lực telephony thật vẫn `BLOCKED_EXTERNAL` (`W-0008`).

## 1. Capacity và one-sim-one-call — `PT-CAP-01`

Đẩy quá năng lực có chủ ý, hai hình dạng:

| Kênh | Job đẩy vào | Lease được cấp | Kênh `RESERVED` | Job còn nguyên | Attempt tính lượt khách |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 8 | ≤ 1 | = số lease | 8 | **0** |
| 4 | 24 | ≤ 4 | = số lease | 24 | **0** |

Ba khẳng định, theo thứ tự quan trọng:

1. **`ONE_SIM_ONE_ACTIVE_CALL` giữ dưới tranh chấp.** Mọi worker đua cùng lúc; số lease không bao giờ vượt số kênh, và mỗi kênh `RESERVED` trỏ tới đúng một `active_call_job_id` **duy nhất**. Một kênh mang hai cuộc là hai khách nghe đơn của nhau — đây là assertion đáng giá nhất trong cả báo cáo.
2. **Không mất task khi quá tải.** Số job seed vào = số job còn trong DB. Cái không lấy được kênh thì **đang chờ**, không bị rơi, và không cái nào bị âm thầm đánh dấu xong.
3. **Quá tải không tiêu lượt gọi của khách.** Lease là IVR giữ chỗ, không phải một lần gọi. `is_counted_customer_attempt = 0` trên toàn bộ (`DT-02`/`DT-04`).

Chạy tuần tự sẽ không chứng minh được gì về tranh chấp, nên toàn bộ worker đua song song.

## 2. Fail-closed dưới tải — `PT-FAILCLOSED-03`

12 task đánh giá **đồng thời** với nguồn capacity ném timeout.

| Kết quả | Giá trị |
| --- | --- |
| Task đủ điều kiện dispatch | **0/12** |
| Reason code | `CAPACITY_SOURCE_UNAVAILABLE` trên cả 12 |
| Attempt được tạo | **0** |
| Job còn nguyên | 12/12 |

`DO-06`. Chế độ hỏng đáng sợ là chế độ **dễ dãi**: một nguồn capacity chậm hoặc chết không được phép đọc thành "còn nhiều chỗ". Chạy đồng thời là ca đáng thử nhất, vì một cuộc đua là nơi "không biết" dễ bị làm tròn thành "ổn" nhất.

## 3. PII lúc chạy — `SEC-PII-04`

Cổng PII của CI quét **file**. Không có gì quét thứ service **thật sự ghi xuống database** — nơi một rò rỉ sẽ đáp xuống lúc chạy: payload audit, evidence ref, hay reason của review item dựng từ một field của khách.

Sau một lượt tải 8 task, mọi dòng trong `ivr_audit_log`, `ivr_evidence`, `ivr_evidence_links`, `ivr_review_items`, `ivr_capacity_incidents` được đưa **ngược lại** qua chính `PiiGuard` mà runtime dùng để tự kiểm. Tất cả sạch.

## 4. Bảo mật — trạng thái thật

| Hạng mục §8 | Trạng thái |
| --- | --- |
| `SEC-AUTHZ-05` caller lạ / thiếu scope → 403 | **có** — `IT-API-AUTHZ-01/02`, `IT-AUTH-INGRESS-12`, `UT-AUTH-JWT-01..05` |
| `SEC-AUTHZ-05` rate limit | **KHÔNG CÓ** — xem §5 |
| `SEC-ERR-06` error không lộ stack/PII | **có** — `IT-FND-ERR-12`, envelope đã che + correlation id |
| Secret exposure | **có** — gitleaks (P0-2), `UT-AUTH-SECRET-09`, `dotnet list package --vulnerable` |

## 5. Khoảng trống chưa đóng — nêu thẳng

| Mục | Trạng thái | Vì sao |
| --- | --- | --- |
| **Rate limiting** | **CHƯA CÓ** | Chỉ tồn tại ánh xạ mã lỗi `IVR_RATE_LIMITED → 429`; **không có middleware nào phát ra nó**. Không tự thêm trong slice này: ngưỡng rate-limit cho một internal API là quyết định vận hành (ai gọi, bao nhiêu, hành vi khi vượt), và đặt một con số tuỳ tiện rồi test nó sẽ tạo bằng chứng về một chính sách chưa ai duyệt |
| **Soak 4–8h** (`PT-SOAK-02`) | **NOT_RUN** | Không chạy được trong phiên này. Không thay bằng một lượt ngắn rồi gọi là soak — leak bộ nhớ và trôi deadline là hiện tượng theo thời gian, một lượt 30 giây không nói gì về chúng |
| **k6/NBomber** | **KHÔNG DÙNG** | Nút thắt của IVR là **lease kênh trong database**, không phải HTTP throughput. Đo bằng worker đua thật trên Postgres thật đúng chỗ nghẽn hơn là bắn HTTP vào một endpoint không phải giới hạn |
| **32 kênh thật** | `BLOCKED_EXTERNAL` | `W-0008`. Mô phỏng 4 kênh chứng minh bất biến, **không** chứng minh năng lực — xem `telephony-procurement-pack` R-03 |
| **OWASP ZAP baseline** | chưa nối | Cần một service đang chạy trong pipeline; thuộc `P7-3` |

## 6. Ngưỡng: cái gì đo được, cái gì chưa

`P5-3` §4 nêu "callback revalidate 3–5s (D-04)" và "intake/scheduler latency ngưỡng". Cả hai là **ngưỡng đầu-cuối có Sales thật trong vòng lặp**. Với fake provider, con số đo được là độ trễ của chính fake — nên báo cáo này **không** ghi một con số latency nào và không tuyên bố đạt ngưỡng D-04.

Cái đo được và đã đo là **bất biến dưới tranh chấp**: one-sim-one-call, không mất task, không tiêu lượt khách, fail-closed. Đó là những thứ sai được ngay cả khi latency đẹp.

## 7. Kết luận

Không có điểm gãy nào phát hiện được ở các bất biến đã kiểm. Ba khoảng trống ở §5 là **chưa làm**, không phải **đã đạt**: rate limiting cần một quyết định vận hành, soak cần thời gian, năng lực thật cần SIM thật.
