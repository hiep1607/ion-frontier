# Chỉnh HUD trong suốt sau 0.11.1

Theo ảnh feedback của người dùng: ba bảng nhiệm vụ, tiến độ đường thoát và thông tin mục tiêu che cảnh quá nhiều. Chỉ đổi `IonHud.cs`: alpha nền lần lượt22%,18%,22%; chữ giữ alpha riêng, có viền tối mảnh và chữ phụ sáng/lớn hơn. Ảnh tàu, thanh máu và thanh tiến độ giữ độ rõ. Không đổi luật combat hoặc bố cục.

Build đã kiểm tra: `Builds/Hud-Transparency/IonFrontier.exe`; launcher thường và mở toàn bộ dùng cùng cách lưu như0.11.1. Theo yêu cầu cập nhật của người dùng, bản này được chép đủ payload vào đường dẫn chơi cũ `Builds/Combat-Clarity`, nên mở exe hoặc shortcut cũ tới đó cũng nhận HUD mới. Bản0.11.1 gốc được sao lưu nguyên vẹn tại `Builds/Archive/Combat-Clarity-0.11.1-Original` trước cập nhật;0.11.0 và ZIP cũ giữ nguyên. Không phát hành ZIP mới trong đợt này.

Hai launcher ở gốc project trỏ về đường dẫn chơi cố định; nếu chưa có build local, chúng hướng dẫn build bằng Unity. Các lần chỉnh sau cần kiểm tra rồi cập nhật đường dẫn chơi cố định này, để người dùng không phải đổi thư mục build mỗi lần. Đây là quy trình xuất build sau sửa, không phải hot reload của exe đang chạy.

Đã build thành công bằng Unity6000.6.2f1. Chạy harness backdrop hiện có:67PASS, không runtime error; có ảnh13màn, các cảnh boss và resize. Đã xem ảnh Player màn4/8 và boss13 để kiểm tra cảnh xuyên nền/chữ. Kết quả nằm tại `outputs/Hud-Transparency/Visual-QA/backdrop-report.json`; ảnh tiêu biểu `gameplay-04.png`, `gameplay-08.png`, `boss-13.png`.

Đợt này không chạy lại toàn bộ567check combat; `TEST-REPORT.json` và bản chụp Design/QA-0.11.1 tiếp tục là bằng chứng của DLL phát hành0.11.1 gốc. QA giao diện và SHA256 DLL/source hiện tại được ghi riêng tại `outputs/Hud-Transparency/verification.json`.

Trong thư mục chơi đã cập nhật, TEST-REPORT và SHA256SUMS phản ánh DLL HUD mới/67check; báo cáo567check cũ được giữ riêng với tên TEST-REPORT-0.11.1-ORIGINAL.json. Báo cáo kiểm tra, hash và triển khai được đồng bộ cùng sửa HUD lên GitHub; không đưa build/ZIP lên Git.

Bản chụp QA để đọc trên GitHub: [QA-Hud-Transparency](QA-Hud-Transparency/README.md).
