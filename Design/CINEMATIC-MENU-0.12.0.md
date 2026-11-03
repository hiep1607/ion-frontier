# Menu điện ảnh 0.12.0 — Ion Frontier Next

Đã triển khai theo phương án Menu điện ảnh người dùng chọn. Project chính: `D:\Projects\IonFrontier-Next`. Bản gốc `D:\Projects\IonFrontier` giữ nguyên.

## Thay đổi

- Màn đầu còn danh sách Xuất kích / Chiến dịch / Kho tàu / Cẩm nang, một tàu lớn và tên nhiệm vụ đang chọn.
- Kho tàu có tám tàu, HP/tốc độ/damage/rate và tên/mô tả Q thật. Tàu khóa vẫn xem được, chỉ trang bị khi đủ điều kiện.
- Chiến dịch chia 13 màn thành ba nhóm hiển thị; có hình nền đúng từng màn, mô tả, boss, phần mở khóa và sao thật. Chọn màn khóa không làm đổi màn đang chơi.
- Cẩm nang có điều khiển, vật phẩm/kiểu bắn, chín mô-đun, tiêu chí sao theo màn hiện tại và hướng dẫn lại; nội dung dài cuộn được.
- Tùy chọn có âm thanh, giảm chuyển động và xác nhận thoát. Giảm chuyển động dừng trôi tàu và nền trong menu, giữ nguyên combat.
- Chuyển mục 180 ms, hover/focus rõ; Enter dùng đường submit có sẵn của Unity. Tab/Shift+Tab đi qua các hành động và giữ trong modal; Esc đóng bảng trước, quay về căn cứ hoặc mở Tùy chọn ở căn cứ. M bật/tắt âm thanh.
- Chọn tàu/màn đưa focus tới nút xác nhận; modal chặn cả raycast và callback xuất kích ở lớp dưới. Bố cục thiết kế 1600×900 tự co để nằm gọn ở 4:3/16:9/màn rộng.

## Bản chơi

Hai launcher ở gốc project đã ưu tiên `Builds/Cinematic-Menu/IonFrontier.exe`; fallback cũ giữ để quay lại nếu thiếu build mới. Bản build cũ ở Combat-Clarity và các archive vẫn giữ nguyên. Cửa sổ exe cũ đang mở không tự thay giao diện; đóng rồi mở qua launcher ở gốc Next để dùng 0.12.0.

Chơi thường vẫn dùng vùng lưu tiến trình hiện có do company/product không đổi. Launcher mở tất cả tiếp tục dùng profile GUID tạm, không ghi đè tiến trình thật.

## Kiểm tra thực tế

285 PASS trên cùng DLL cuối: 111 Menu (gồm 18 ảnh render có nội dung), 107 hồi quy chương mở đầu và 67 nền. Không có runtime error trong ba báo cáo. SHA256 DLL ở menu-report trùng build phát hành. Source/assets/settings được đối chiếu theo frozen-source; bản gốc được đối chiếu lại SHA256 riêng.

Đã xem ảnh Home, Ships, Campaign, Guide tại 1280×720, Home 1024×768 và Campaign 1680×720; có bộ ảnh đủ bốn trang tại 1280×720, 1920×1080, 1024×768, 1680×720. Đã kiểm luồng khóa/mở, tám tàu, 13 màn, thực thi pointer raycast/keyboard-submit của Unity, giữ focus trong modal, hướng dẫn lại, màn11 chọn tuyến và quay lại trận.

111 check menu là kiểm tích hợp, ảnh và điều kiện bố cục; không gọi đó là 111 lượt người chơi. Chưa chạy lại toàn bộ 218 check chiến dịch hoặc nghiệm thu cân bằng 13 màn; 567 check0.11.1 là lịch sử, không cộng vào285.

## Phương pháp ảnh và giới hạn thao tác

Player chạy ẩn làm ảnh ScreenCapture ban đầu bị đen. Ảnh nghiệm thu menu được render bằng Camera.Render của Unity vào RenderTexture, tạm chuyển Canvas từ ScreenSpaceOverlay sang ScreenSpaceCamera chỉ trong lúc chụp rồi khôi phục; không chỉnh pixel ảnh và không thay renderer khi chơi bình thường. Đã quan sát trực tiếp một lần màn đầu ở cửa sổ Player thường bằng computer-use.

Computer-use báo SendInput GetLastError=87 và sau đó chặn thao tác khi phát hiện input của người dùng. Vì vậy không tuyên bố đã chơi tay hay hoàn tất kiểm chuột/phím ở mức hệ điều hành. Bộ menu dùng raycast, pointer-click, submit và callback thật của Unity với save tạm. Đây là giới hạn kiểm chứng còn lại.

## Phạm vi source

IonMenuHud quản lý các trang menu; IonMenuShade vẽ lớp nền chuyển sắc bằng UI mesh. IonHud giữ nguyên giao diện trong trận, tách phần menu cũ và hỗ trợ focus cho bảng thông tin. IonGame chỉ đổi định tuyến phím ở Menu và profile riêng cho cờ QA; IonBackdrop tôn trọng giảm chuyển động trong Menu. IonBuild cập nhật version0.12.0; scene/ProjectSettings được Unity xuất lại. Không đổi HP, damage, tốc độ, loot, nhiệm vụ, module hoặc luật mở khóa/lưu.

Không push remote. Backup trước sửa ở LOCAL-NOTES/Before-Cinematic-Menu-0.12.0. Báo cáo nguồn tại outputs/Cinematic-Menu, bản chụp theo dõi bằng Git tại Design/QA-0.12.0.
