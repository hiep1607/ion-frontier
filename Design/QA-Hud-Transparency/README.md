# QA chỉnh HUD trong suốt

Bản chụp báo cáo gốc từ outputs/Hud-Transparency. Unity build thành công;67kiểm tra backdrop đạt, không runtime error. Đã xem ảnh Player màn4/8 và boss13: thấy cảnh phía sau bảng, chữ có độ tương phản và viền tối.

- backdrop-report.json:67check thực sự đã chạy trên DLL HUD mới.
- frame-metrics.json:phép đo của chính lượt QA, gồm frame chậm sau warm-up.
- verification.json:SHA256 IonHud/DLL và phạm vi sửa.
- current-build-update.json:xác minh cập nhật bản chơi cố định bằng cùng DLL đã test, kiểm hash toàn bộ175tệp và giữ nguyên171tệp của bản gốc trong archive.

Đường dẫn D: trong JSON mô tả máy lúc kiểm tra. Các ảnh/build/ZIP không được push.567check của bản0.11.1 gốc không chạy lại và không cộng vào67check này. Báo cáo bản gốc giữ riêng tại Design/QA-0.11.1.
