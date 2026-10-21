# Bộ 13 phi thuyền 2D

Bộ hình riêng cho game bắn phi thuyền: **3 tàu người chơi + 7 tàu địch thường + 3 boss**. Đây là tài nguyên hình ảnh; không thay đổi code hoặc bản game.

Mỗi tàu là một PNG nền trong suốt độc lập. Góc nhìn từ trên xuống; tàu người chơi hướng lên, tàu địch và boss hướng xuống. Màu sắc và dáng tàu phân biệt từng loại. Vai trò bên dưới là ý tưởng thiết kế, chưa được lập trình vào game.

## Người chơi

1. **Phong Ưng** — bạc và xanh lam; dáng nhọn, cánh vuốt, thiên về tốc độ.
2. **Lôi Điểu** — bạc và vàng; cánh rộng, hai pháo lớn, thiên về hỏa lực.
3. **Thiết Vệ** — bạc và xanh lục; thân giáp dày, dáng gọn, thiên về phòng thủ.

## Tàu địch thường

1. **Trinh Sát** — đỏ, nhỏ và nhọn.
2. **Đánh Chặn** — cánh móc, đỏ sẫm và hổ phách.
3. **Đột Kích** — tím, thân dao mảnh và cánh cong.
4. **Oanh Tạc** — cam, thân bè và hai khoang bom.
5. **Xạ Thủ** — xanh lam, thân dài và pháo chính ở giữa.
6. **Hộ Vệ** — xanh lục, các tấm giáp lớn hai bên.
7. **Tinh Nhuệ** — đen và đỏ, bốn nhánh cánh dạng X.

## Boss

1. **Bão Lửa** — chiến hạm cam, cánh rộng và nhiều nòng pháo.
2. **Ảnh Ma** — chiến hạm tím, cánh tách và các lõi năng lượng.
3. **Thiết Giáp Vương** — đại chiến hạm thép và đỏ, giáp nhiều lớp và pháo kép.

## Sử dụng

- Mở `Xem-13-mau.html` để xem toàn bộ mẫu và tên gọi.
- Các PNG đã nằm trong `Assets/IonFrontier/Art/Fleet` của project này, kèm thiết lập Sprite (Single) và alpha.
- Đặt Texture Type thành **Sprite (2D and UI)**, Sprite Mode **Single**, bật **Alpha Is Transparency** rồi Apply.
- Tùy chỉnh Pixels Per Unit và kích thước trong scene theo game của bạn. Boss nên lớn hơn tàu thường.

Hình được tạo bằng công cụ ImageGen tích hợp. Các PNG gốc được giữ nguyên, gồm kênh alpha. Prompt được lưu trong thư mục `Prompts`. Đây là sprite 2D, không phải mô hình 3D.
