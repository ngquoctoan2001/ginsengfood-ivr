# W-0155 capacity data-intake pending templates

Các file trong thư mục này là **template cố ý không hợp lệ**. Chúng mang trạng thái
`PENDING_EXTERNAL_OWNER_DATA`, chứa placeholder và không có artifact hash thật; validator phải từ
chối nếu chạy trực tiếp trên thư mục này.

## Cách dùng

1. Sao chép toàn bộ thư mục sang một vị trí làm việc ngoài evidence tree.
2. Thay mọi placeholder trong bốn artifact JSON bằng dữ liệu aggregate/PII-safe thật.
3. Tính SHA-256 cho từng artifact sau khi hoàn tất và điền vào `bundle-manifest.json`.
4. Điền provenance, observation window, filter, signer identity/role/org, authority source và
   `signed_at` cho từng submission.
5. Bổ sung đủ cả `GOLDEN_HOUR` và `TWENTY_FOUR_SEVEN` ở timing, arrival, policy và outcome;
   template chỉ có một skeleton row để mô tả shape, không phải bộ record tối thiểu hợp lệ.
6. Chỉ đổi `status` thành `EXTERNAL_OWNER_ATTESTED` khi người có thẩm quyền thực sự attest bundle.
7. Chạy:

```powershell
node deploy/ci/scripts/capacity-data-intake-validator.mjs --bundle-dir <thu-muc-bundle>
```

`CAPACITY_DATA_INTAKE_PASS` chỉ chứng minh bundle qua kiểm tra offline. Trường
`authority=METADATA_ONLY_NOT_EXTERNALLY_VERIFIED` nhắc rằng công cụ không thể chứng minh danh tính
hay thẩm quyền ngoài đời, cũng không thay calibration, shared E2E hoặc production approval.
