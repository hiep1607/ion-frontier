# Prompt giao việc cho Sol

> Đã hoàn tất bản0.11.0 ngày22/10/2026. Xem `UPDATE-0.11-IMPLEMENTATION.md`, `TEST-REPORT.json` và `outputs/Tactical-Choices/QA-NOTES.md`. Các đoạn sau là yêu cầu giao việc gốc.

Đọc đặc tả đầy đủ ở `D:\Projects\IonFrontier\Design\UPDATE-0.11-GAMEPLAY-SPEC.md`, rồi triển khai bản cập nhật ION FRONTIER0.11.0 theo ba hướng đã chốt:

1. Chọn một trong ba nâng cấp tại hai checkpoint mỗi màn, có chín mô-đun tạo thay đổi hành vi và màn chọn đóng băng toàn bộ mô phỏng.
2. Sáu đội hình địch phối hợp, có dấu hiệu rõ và cách phá tác dụng bằng cách hạ thành viên hỗ trợ; thay một phần đợt spawn cũ theo bảng phân bổ.
3. Màn4 giữ tuyến3600m/72giây hoạt động nhưng thêm tình huống chặn sườn và cửa sổ phản công tàu tinh nhuệ có hai động cơ. Hạ được nhận thưởng/khoảng thở; bỏ qua vẫn tới boss cuối bình thường.

Project chính duy nhất là `D:\Projects\IonFrontier`. Đọc `AGENTS.md` tại đó trước. Dùng code local0.10.0 làm nền; repo GitHub mới đồng bộ một phần hình ảnh, không lấy source remote đè local. Tất cả chỉnh sửa, hình, tài liệu, QA và build đặt tại D. Trao đổi và giao diện bằng tiếng Việt.

Đặc tả đã xác định luật, phạm vi, thông số đầu, điểm tích hợp, thứ tự triển khai và tiêu chí nghiệm thu. Hãy tự xử lý các lựa chọn kỹ thuật thông thường và thực hiện tới bản chơi được. Có thể chỉnh nhỏ thông số sau kiểm tra, ghi rõ thay đổi. Không biến nhiệm vụ phản công tùy chọn thành điều kiện thắng; giữ profilev4, mốc mở tàu, tám Q và hệ5sao hiện tại.

Ưu tiên các rủi ro: combat/timer chạy ngầm trong draft; nhận reward hai lần; sát thương phụ kích hoạt đệ quy; liên kết đội hình tới slot pool đã tái sử dụng; elite bị tính thành boss và kết thúc màn sớm; phá engine làm sai bộ đếm nhiệm vụ; các mục tiêu ưu tiên bị mất khiến kẹt màn.

Tạo backup local trước khi sửa lớn. Triển khai và kiểm tra theo từng phần; dùng số liệu và ảnh Player thật để kiểm UI/gameplay. Cập nhật các harness cũ đúng theo thay đổi mới, thêm kiểm tra hành vi thực cho module/formation/counterattack. Không tắt tính năng mới chỉ để test cũ qua.

Xuất bản vào `Builds/Tactical-Choices/IonFrontier.exe`, có launcher thường và mở đủ13màn/8tàu, ZIP trong `outputs`, ảnh và báo cáo QA. Giữ bản0.10.0 riêng. Cập nhật README, CAMPAIGN, GAME-HANDBOOK và AGENTS theo bản đã thực hiện. Báo cáo đúng kiểm tra đã chạy, giới hạn cân bằng còn lại và những điều chỉnh so với đặc tả.

Không tự push GitHub, không thêm các hệ ngoài ba hướng trên như shop, multiplayer, nội tại mới cho cả tám tàu hoặc roguelite hành trình. Kết quả cuối cần có link build, ảnh minh họa ba tính năng và báo cáo ngắn về thay đổi/kiểm tra.
