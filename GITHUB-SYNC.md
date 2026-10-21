# Đồng bộ GitHub từng phần — 21/10/2026

Repo riêng tư: hiep1607/ion-frontier, nhánh main. Nguồn chính: D:\Projects\IonFrontier, bản local 0.10.0.

Trước đợt này có **326 tệp mới/đã đổi** so với commit khởi đầu `d4fcd9c`. Đợt này đồng bộ **158 tệp sẵn có (48.5% theo số tệp, khoảng 80.03 MiB)** và thêm tài liệu phạm vi này. Tính theo số tệp Git quản lý; không tính build, Library, log, bản ZIP hay ghi chú riêng đã bị ignore.

## Đã đưa lên trong đợt này

1. Bộ Fleet: 8 phi cơ người chơi, 7 địch thường và 3 boss nền; giữ nguyên PNG và Unity GUID/import metadata.
2. HostileFleet: 12 địch mới và 16 boss riêng, đầy đủ PNG và metadata.
3. Prompt, manifest, biên nhận tạo hình, gallery và script nhập hình của các bộ tàu.
4. Atlas gốc, font tiếng Việt và giấy phép OFL.
5. Tài liệu nguồn hình và bản ghi phạm vi đồng bộ này.

## Còn ở máy, chưa đồng bộ

**168 tệp** còn lại: code gameplay/HUD/kiểm thử mới (13 màn, HP/chỉ số, truy đuổi màn 4, 5 sao), FleetLibrary asset/script, scene và công cụ build, ProjectSettings, nền 13 khu vực, mô hình vật thể nhiệm vụ, tài liệu chiến dịch và ảnh/bộ đóng gói các bản mới.

README chính trên GitHub vẫn mô tả bản khởi đầu; README đầy đủ 0.10.0 ở máy sẽ đi cùng đợt gameplay. Đợt hiện tại là bản sao lưu tài nguyên từng phần, chưa phải bản game 0.10.0 hoàn chỉnh có thể clone và chạy ngay. Các PNG không được sửa; bộ code mới trên máy không bị thay thế bởi bản khởi đầu.

## Kiểm tra phạm vi

- Chia thành 5 commit theo nhóm, không đưa build/export/cache/lưu người chơi hoặc thông tin xác thực lên repo.
- Kiểm tra cặp PNG/meta, GUID, SHA-256 theo manifest (nếu có) và tài liệu giấy phép font trước khi push.
- Không chạy Unity/game cho tác vụ đồng bộ file.
