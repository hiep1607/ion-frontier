# Khảo sát chơi thử / cân bằng 0.11.1

Ngày 22/10/2026. Build-03, cùng DLL với chín suite nghiệm thu. Không sửa thông số damage, HP, nhịp spawn, loot, giá trị chín mô-đun hoặc cửa sổ phản công trong đợt này. Phần sửa luật và phản hồi đã hoàn tất; **cân bằng chơi tay toàn chiến dịch chưa được xác nhận**.

## Sáu lượt màn 4 không bất tử

Nguồn: `Balance-Release/balance-report.json`. Ba hull đầu, mỗi hull một lượt không cấp Q và một lượt cấp Q. Bot dùng điều khiển di chuyển/bắn thật, không tăng tốc tuyến, không sửa HP/kill, chỉ có bảo vệ hai giây đầu theo luật game. Hai checkpoint vẫn chọn thẻ thật bằng chế độ tự chọn riêng cho harness. Các lượt kết thúc khi thua, tới boss hoặc hết giới hạn 100 giây.

| Hull | Cấp Q | Thời gian tới thua | Quãng đường | Damage đạn / đá | Đạn còn | Lựa chọn đã nhận |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| Phong Ưng | Không | 35,21s | 1.760m | 46 / 54 | 168 | 1 |
| Phong Ưng | Có | 29,05s | 1.452m | 70 / 30 | 202 | 1 |
| Lôi Điểu | Không | 32,88s | 1.644m | 50 / 40 | 162 | 1 |
| Lôi Điểu | Có | 22,44s | 1.121m | 54 / 36 | 165 | 0 |
| Thiết Vệ | Không | 38,53s | 1.926m | 60 / 70 | 107 | 1 |
| Thiết Vệ | Có | 27,25s | 1.362m | 30 / 100 | 223 | 1 |

Cả sáu lượt thua trước phản công ở giây 50 và trước boss. Không lượt nào cạn đạn; tổng HP mất là 640, gồm 310 từ đạn và 330 từ đá. Không có runtime error trong báo cáo. Frame time toàn lượt sau warm-up: mean 59,99 FPS, p95 16,72ms, p99 16,79ms, max 48,04ms; ba frame >33ms, không frame >50/100ms. Có 393 frame background và vẫn giữ trong thống kê.

**Giới hạn:** bot chọn điểm đích theo nguy cơ gần, chưa lập đường né an toàn liên tục và không chủ động tận dụng lướt. Q chỉ được bot dùng ở phản công, nên cả ba lượt được cấp Q chưa sử dụng Q. Do đó bảng không chứng minh Q yếu/mạnh hay có Q làm khó hơn. Seed lựa chọn cố định, nhưng luồng RNG combat không reset giữa lượt; sáu lượt không phải so sánh cặp cùng tình huống. Chưa thể suy ra tỷ lệ hoàn thành của người chơi hay độ khó tương đối giữa hull.

## Thử qua giao diện thường

Đã mở build thường bằng `-ion-unlocked`, chọn Phong Ưng và màn 4 trong menu, xuất kích, bật F và thử lái bằng chuột qua computer-use. Không dùng cờ harness hoặc bất tử; lưu thật không bị ghi. Quan sát ở 383/3.600m: HP 34/100, đạn 292, phá 1 đá; lượt kết thúc Defeat trước checkpoint đầu. Log: `outputs/Combat-Clarity/ui-playtest.log`.

Tần suất quan sát/input của công cụ chậm hơn điều khiển liên tục, có gián đoạn focus khi ở menu. Lượt này chỉ xác nhận luồng menu/xuất kích/tự bắn/thua hoạt động; không gán thành thử nghiệm người chơi hoặc dùng để ước lượng tỷ lệ thắng. Bài kiểm tra thao tác chọn thẻ riêng đã dùng chuột cả ba thẻ, bấm đúp, phím 1/2/3 và P/Esc, đủ 7 PASS.

## Quyết định và hướng tiếp

- Giữ thông số hiện tại trong 0.11.1. Dữ liệu báo hiệu cần kiểm tra áp lực đầu màn 4; chưa đủ cơ sở giảm damage toàn game hoặc tăng sức mạnh Q.
- Ưu tiên lượt người chơi bình thường ở màn 1/4/6/9/11/13 với hull cân bằng/nhanh/nặng; ghi nguồn chết, HP, đạn, lựa chọn thẻ, thời gian phá động cơ/thân và tới boss. Chơi màn 4 đủ lâu để so sánh thực sự dùng/giữ Q, cùng điều kiện khởi đầu.
- Nếu người chơi hiểu cảnh báo nhưng vẫn chết sớm lặp lại, thử từng thay đổi riêng: giảm chồng lấn đá với quạt truy kích trong 12 giây đầu, rồi đo lại. Giữ các nhịp chặn sườn và phản công để không làm mất kịch tính.
- Đánh giá mức dễ hiểu của icon/counter mới trước khi thêm mô-đun. Theo dõi tỷ lệ chọn và điều kiện kích hoạt; chưa thêm shop, màn hoặc mô-đun mới.

Các lượt quan sát trên không được cộng vào 567 kiểm tra chức năng. Không tuyên bố đã cân bằng đủ 13 màn.
