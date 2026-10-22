# Nghiệm thu ION FRONTIER 0.11.1

Bản chụp báo cáo gốc ngày22/10/2026, sao chép từ outputs/Combat-Clarity để lưu cùng source trên GitHub. Không sửa hoặc chạy lại test trong tác vụ đồng bộ Git.

- [TEST-REPORT.json](TEST-REPORT.json):567PASS từ9suite trên cùng build-03, không cộng các bản cũ.
- [QA-NOTES.md](QA-NOTES.md):phạm vi, frame time và giới hạn; mỗi thư mục *-Release có report cùng frame-metrics.json.
- [BALANCE-NOTES.md](BALANCE-NOTES.md):6lượt bot không bất tử và lượt UI thường; chưa xác nhận cân bằng chơi tay13màn.
- [release-source-audit.json](release-source-audit.json):SHA256 source/exe/DLL đã kiểm thử. Git chuẩn hóa CRLF thành LF theo.gitattributes; SHA256 source ở đây là byte local tại thời điểm build, có thể khác byte sau clone chỉ vì newline.
- [TEST-REPORT-0.11.0.json](TEST-REPORT-0.11.0.json):542PASS lịch sử, giữ riêng bản thử nghiệm0.11.0.

Đường dẫn outputs/Builds trong báo cáo gốc là đường dẫn local lúc chạy. Các file JSON được giữ nguyên; tìm report tương ứng ngay trong thư mục này. Build/ZIP/log/ảnh QA không được đưa vào repo. package-verification xác nhận ZIP local, không phải liên kết tải ZIP từ GitHub.

Unity6000.6.2f1; mở scene Assets/IonFrontier/Scenes/Flight.unity. Các cờ QA và profile riêng được mô tả trong source và tài liệu triển khai. Đợt push chỉ kiểm tra file/GUID/hash, không chạy Unity hoặc game.
