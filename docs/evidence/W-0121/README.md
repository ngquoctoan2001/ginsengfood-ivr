# W-0121 — Lối đẩy code không tới GitLab nên hosted CI không thể chạy

Ngày lập hồ sơ: 24/09/2026 · Claude (W-0351) · `REAL_CUSTOMER_CALL_ALLOWED=NO`.

Ngày 27/08, `remote.origin.url` lấy code từ GitLab nhưng `remote.origin.pushurl` trỏ GitHub, nên `git push`
chỉ tới GitHub và GitLab thiếu 3 commit. W-0121 đặt hai `pushurl` trên cùng remote `origin`, GitLab trước
GitHub sau, nên một lần `git push origin main` tới cả hai. Việc này chỉ đổi `.git/config`, không đổi tệp
nào trong repo.

## Phép kiểm

- Lúc làm: `git remote -v` cho 1 fetch và 2 push; `git push --dry-run` tới GitLab exit 0.
- Từ W-0292 (14/09) hosted CI đã chạy trên GitLab. Ngày 24/09, lần push `3fba1d8` tới cả GitLab lẫn GitHub,
  và `git ls-remote --heads` của hai remote cùng trả `3fba1d8`.

Trạng thái chuyển `CODE_DONE` → `EVIDENCE_SUBMITTED` ở W-0351, khi hồ sơ này được lập.
