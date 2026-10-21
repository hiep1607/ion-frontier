# Hạm đội địch 0.6.0

28 PNG nguyên bản tạo bằng built-in ImageGen, transparent_background=true, không hậu xử lý raster bằng script. Thành phẩm ở `../../Assets/IonFrontier/Art/HostileFleet/`.

- N01–N12: 12 địch thường với dáng thân và hành vi riêng.
- B01–B13: boss chính màn 1–13.
- B14: Kìm Hãm, hai tàu hộ vệ màn 5.
- B15: Lõi Vệ, hai tàu đối tác màn 9.
- B16: Song Sát Lam, đối tác màn 11.

`Receipts/` lưu prompt, thiết kế và đường dẫn thành phẩm. Biên nhận N12 được dựng lại từ mô tả của lệnh tạo sau khi quá trình ghi receipt bị gián đoạn; PNG gốc được giữ nguyên. `manifest.json` chứa đường dẫn tương đối, GUID Unity, kích thước, tỷ lệ alpha trong suốt và SHA-256 của cả 28 file. `import_fleet.py` chỉ kiểm tra ảnh và tạo metadata/tham chiếu Unity, không sửa nội dung PNG.

Unity nhập RGBA không nén, tối đa 1024 px, pivot giữa, 100 pixel/unit, Full Rect; hình gốc có kích thước lớn hơn và giữ nguyên trên đĩa. Các ảnh gallery chụp trực tiếp bằng Unity được sao chép vào đây sau QA.
