# ION FRONTIER — Chiến đấu rõ ràng · 0.11.1

Game bắn phi thuyền 2D bằng Unity 6.6 / 6000.6.2f1, giao diện tiếng Việt. Project chính: `D:\Projects\IonFrontier`.

Bản mô tả đầy đủ: [GAME-HANDBOOK-VI.md](GAME-HANDBOOK-VI.md) — gameplay, tám phi cơ, địch, 13 màn, boss, vật phẩm và năm sao.

## Chơi và mở project

- Repo private đã có đủ source và tài nguyên0.11.1. Clone repo bằng tài khoản được cấp quyền, cài Unity6000.6.2f1 qua Unity Hub rồi mở thư mục chứa `Assets`, `Packages`, `ProjectSettings`. Chờ Unity nhập tài nguyên, mở scene và nhấn Play.
- Bản Windows: `Builds/Combat-Clarity/IonFrontier.exe`. Giữ nguyên các thư mục và DLL đi kèm.
- Unity Hub → Add project from disk → chọn thư mục project này.
- Scene chính: `Assets/IonFrontier/Scenes/Flight.unity`.
- Trong Unity chọn **Ion Frontier → Build Windows** để xuất tại `Builds/Windows`.

Build/ZIP, Library, log và lưu người chơi không nằm trong Git. Báo cáo nghiệm thu được lưu riêng ở [Design/QA-0.11.1](Design/QA-0.11.1/README.md); xem [GITHUB-SYNC.md](GITHUB-SYNC.md) để biết phạm vi đồng bộ từng phần.

## Chương mở đầu

| Màn | Đợt địch | Boss cuối màn | Đạn khởi đầu | Mở khóa khi thắng |
| --- | --- | --- | --- | --- |
| 1 — Vành đai tập sự | 3 | Bão Lửa | 180 | Lôi Điểu + màn 2 |
| 2 — Hành lang đột kích | 3 | Ảnh Ma | 210 | Thiết Vệ + màn 3 |
| 3 — Thành trì hạm đội | 3 | Thiết Giáp Vương | 240 | Màn 4 / chương mở rộng |

Ba màn trên là chương mở đầu. Hạ boss màn 3 hiện mở màn 4; chỉ hạ kỳ hạm màn 13 mới hoàn tất chiến dịch.

## Sửa luật và phản hồi0.11.1

Kết quả trận được chốt một lần: thua không chuyển lại thành thắng do viên đạn/callback đến muộn. Laser của tàu bị hủy khi tàu nguồn chết/rút hoặc slot nguồn bị tái sử dụng; laser môi trường vẫn có thời gian sống riêng.

Chín mô-đun có icon vector riêng và hai ô trạng thái ở góc dưới, hiển thị tích lũy/hồi chiêu/điều kiện kiểu bắn. Thẻ súng có chỉ số trước–sau và cảnh báo mô-đun sẽ ngừng. Mảnh phân tách nhỏ/ngắn hơn. Bản0.11.0 giữ riêng làm bản thử nghiệm.

Bản0.11.1 đạt567kiểm tra thực tế trên cùng build-03; xem [báo cáo nghiệm thu](Design/QA-0.11.1/TEST-REPORT.json). Sáu lượt bot màn4 chưa tới phản công, nên chưa xác nhận cân bằng chơi tay; thông số chiến đấu được giữ.

QA mới ghi mọi frame sau warm-up1,5s, p95/p99/max, frame chậm, GC và pool; có thống kê foreground/background. Xem [triển khai0.11.1](Design/UPDATE-0.11.1-IMPLEMENTATION.md), [QA](Design/QA-0.11.1/QA-NOTES.md) và [khảo sát cân bằng](Design/QA-0.11.1/BALANCE-NOTES.md).

## Lựa chọn chiến thuật0.11.0

Bản thử nghiệm0.11.0 đã đạt542kiểm tra:107mở đầu,218chiến dịch,71HP/chỉ số,37chấm sao,34truy đuổi và75tính năng mới. Báo cáo lịch sử [0.11.0](Design/QA-0.11.1/TEST-REPORT-0.11.0.json), [phạm vi QA](outputs/Tactical-Choices/QA-NOTES.md) và [triển khai](Design/UPDATE-0.11-IMPLEMENTATION.md). QA có bảo vệ tàu và hook tăng tốc, chưa thay cho chơi tay cân bằng toàn chiến dịch.

Mỗi màn có hai lần chọn một trong ba thẻ nâng cấp. Màn1–3 chọn sau đợt1/2; màn4 chọn ở1.200/2.400m; màn5–13 chọn khi vào chặng2/3. Màn chọn đóng băng mô phỏng và đồng hồ chấm sao. Chọn bằng chuột hoặc1/2/3, sau đó trở lại trận. Các thẻ gồm phát triển súng, mô-đun phù hợp và hỗ trợ theo HP/đạn. Mô-đun có ô riêng với hai ô kiểu bắn; tối đa hai mô-đun/lượt, reset khi xuất kích lại.

| Mô-đun | Tác dụng |
| --- | --- |
| Nòng hội tụ | Góc Tỏa còn60%, sát thương đạn chính tăng15% |
| Đạn phân mảnh | Viên Xuyên giảm15% sát thương; hit thật đầu tiên của mỗi viên chính tạo hai mảnh30%, tối đa bốn mảnh/loạt |
| Sét dẫn hướng | Mỗi sáu loạt trúng, phóng sét sang một địch cách tối đa3đơn vị, gây50% sát thương viên kích hoạt |
| Động cơ phản kích | Sau lướt, ba loạt trong hai giây tăng25% sát thương |
| Buồng xung điện | Mỗi loạt thứ tám tăng35% sát thương và xuyên thêm một mục tiêu |
| Đạn dư chấn | Mỗi năm tàu thường do súng chính hạ tạo nổ bán kính1,6, gây75% sát thương viên kết liễu |
| Giáp phản ứng | Mất HP thật tạo xung xóa đạn gần; hồi tám giây |
| Nam châm tiếp tế | Phạm vi hút tăng60%, tốc độ hút tăng20%, không đổi tỷ lệ rơi |
| Bộ thu hồi đạn | Mỗi ba tàu thường do súng chính hạ hoàn ba đạn, tối đa30đạn/màn |

Đạn phụ, Q, drone và đồng minh không tự tạo chuỗi kích hoạt đệ quy. Mô-đun cần một kiểu bắn sẽ tạm ngừng nếu kiểu đó bị thay; nhặt lại kiểu sẽ hoạt động lại.

Sáu đội hình phối hợp thay một phần lượt spawn cũ: khiên+xạ thủ, kéo+laser, tàu mẹ+đội sửa, gọng kìm, tàu báo động và mìn+ngư lôi. Hạ nguồn hỗ trợ sẽ cắt đúng tác dụng; đội hình có cảnh báo và giới hạn sinh viện binh. Màn4 dùng timeline truy đuổi riêng.

## Màn4 — Thoát khỏi vành đai

Tàu tự tiến3600m ở50m/giây, khoảng72giây mô phỏng. Thanh tiến độ có hai checkpoint ở1200/2400m, mỗi mốc cho tiếp tế và lựa chọn nâng cấp. Pause và màn chọn không làm đường thoát chạy ngầm.

| Thời gian hoạt động | Diễn biến |
| --- | --- |
| 0–12s | Hai tàu bám đuôi, quạt pháo và đá có khe |
| 12–24s | Một tàu vượt lên sườn tạo hỏa lực chặn đường |
| 24–40s | Đá cắt ngang và hỏa lực hai phía luân phiên |
| 40–48s | Tàu truy kích hạng nặng vượt lên, báo cơ hội phản công |
| 48–50s | Elite vào phía trước, lộ hai động cơ |
| 50–64s | Phá hai động cơ18HP mỗi bên rồi đánh thân42HP; đá thưa hơn |
| 64–72s | Bứt tới điểm nhảy; hạ elite thành công làm giảm áp lực |
| 100% | Kẻ Nghiền Đá440HP chặn lối thoát |

Elite phản công là mục tiêu tùy chọn. Phá động cơ nào loại bỏ cụm pháo bên đó; hết cả hai động cơ thì thân mở giáp. Hạ được trước khi hết cửa sổ nhận600điểm và40đạn một lần. Nếu bỏ qua, elite rút và người chơi vẫn tới boss cuối bình thường. Elite không cấp sao/mở khóa hoặc khởi động đồng hồ boss.

Mục tiêu phụ vẫn là phá18thiênthạch bằng hỏa lực, không tính va đá. Pháo khóa hướng được báo trước khoảng0,85giây; hướng đã khóa không tiếp tục bám tàu. Khởi đầu300đạn, giữ tiếp tế chống cạn và trước boss.

## Bối cảnh riêng cho từng màn

Bản0.8.0 thay cảnh sao đổi màu bằng 13 tranh nền riêng, đồng bộ kịch bản. Hành tinh, công trình và phế tích đặt ở rìa; vùng giữa tối để nhìn rõ đạn và mục tiêu. Sao/vật thể xa chạy nhiều lớp, màn7 có vệt sao nhanh, màn8 sương trôi, màn9 lõi năng lượng nhịp chậm, màn11 đổi điểm nhấn theo tuyến E/R, màn12 có dòng vật chất đỏ và màn13 là nội vực kỳ hạm.

| Màn | Bối cảnh |
| --- | --- |
| 1 | Quỹ đạo huấn luyện xanh, hành tinh và bến tập sự |
| 2 | Khe bụi hổ phách với trụ cổng đổ vỡ |
| 3 | Cửa thành trì sắt, đèn đỏ và ụ hangar |
| 4 | Nghĩa địa chiến hạm, xác tàu và vành đai đá |
| 5 | Hành lang sơ tán cạnh hành tinh đêm và cảng dân sự |
| 6 | Vành trạm chắn quỹ đạo, kết cấu cơ khí trắng xám |
| 7 | Đường hầm siêu tốc xanh vàng và vệt sáng kéo dài |
| 8 | Sương tím, mặt trăng vỡ và tín hiệu cứu hộ xa |
| 9 | Mạng ba trạm lõi xanh lục nối bằng đường năng lượng |
| 10 | Vành đai khai mỏ bỏ hoang, máy móc đồng và kho mìn |
| 11 | Ngã ba: cảng tiếp tế xanh và trạm cứu hộ tím |
| 12 | Rìa không gian đỏ đang sụp đổ, phế tích và vết nứt |
| 13 | Nội vực Đế Hạm, vòm giáp đen và lò phản ứng tím |

Nền dịu và chuyển động chậm hơn trong trận boss. Pause dừng cả sao/sương/phế tích; đổi kích thước cửa sổ tự căn phủ kín ảnh. Tại căn cứ, chọn màn đổi cả cảnh xem trước. Các chi tiết ở hậu cảnh là phong cảnh; vật thể nhiệm vụ tương tác vẫn có vòng/nhãn ở lớp chiến đấu.

Tranh gốc: `Assets/IonFrontier/Art/SectorBackgrounds`. Prompt/manifest/ảnh tổng hợp: `ArtSource/SectorBackgrounds`. `IonBackdrop.cs` quản lý pool cố định 174 renderer, bật số lớp phù hợp từng khu vực.

## Hạm đội và nhiệm vụ mới

Bản 0.6.0 đã thêm **12 hình tàu thường, 16 hình boss**, tích hợp hành vi riêng. Mỗi màn có một mẫu boss chính khác nhau. Màn 5/9 có ba boss thật; màn 11 có hai tàu Song Sát khác hình với khiên luân phiên và cuồng nộ.

Màn 4 là chạy thoát có tiến độ; màn 5–13 chia thành ba chặng: phá cổng, chặn đội ưu tiên, sửa/hộ tống, cứu trinh sát, phá nhiễu, chiếm vòng hoặc né laser. Không còn chờ đủ 150–180 giây sau khi xong mục tiêu. Xem **[kịch bản từng màn](CAMPAIGN.md)**.

Tất cả boss đều cháy, nghiêng, vỡ sáu mảnh của chính thân tàu và nổ lõi. Nếu còn boss khác, giao tranh tiếp tục trong lúc xác tàu sụp đổ. Chỉ ghi thắng sau khi mọi boss nổ xong.

Chuyển chặng có tiếp tế đạn/chỉ số. Tiếp tế định kỳ ở giây 25 rồi mỗi 32 giây ở màn 5–13; màn 4 chỉ có checkpoint và chống cạn đạn. Trước boss +120 đạn, 1 Q và hồi 20 HP. Quà ngẫu nhiên giữ 15%, tối đa hai kiểu bắn và chỉ số cộng dồn giữ nguyên.

## HP và đặc tính phi cơ

Bản 0.7.0 dùng HP dạng số thực thay cho năm lớp chịu đòn. Đạn nhẹ, pháo nặng, ngư lôi, va chạm, laser và vùng sụp đổ gây lượng sát thương khác nhau. Laser/vùng đỏ trừ HP theo thời gian; lướt và các đại chiêu bảo vệ vẫn cho miễn sát thương trong khoảng tác dụng. Sửa tàu hồi 30 HP, không vượt HP tối đa. Trước boss hồi 20 HP; Q của Thiết Vệ cũng hồi 20 HP.

| Phi cơ | HP | Tốc độ cơ bản | Sát thương | Nhịp bắn |
| --- | ---: | ---: | ---: | ---: |
| Phong Ưng | 100 | 10,50 | ×1,00 | ×1,00 |
| Lôi Điểu | 90 | 11,97 | ×0,92 | ×1,12 |
| Thiết Vệ | 130 | 9,24 | ×1,15 | ×0,90 |
| Hỏa Phượng | 110 | 10,08 | ×1,12 | ×0,96 |
| Ảnh Vũ | 95 | 11,55 | ×0,90 | ×1,16 |
| Từ Long | 120 | 9,87 | ×1,05 | ×0,96 |
| Tinh Vân | 100 | 11,03 | ×0,98 | ×1,08 |
| Hư Không | 85 | 12,18 | ×1,10 | ×0,92 |

Tốc độ tính theo đơn vị thế giới/giây ở trạng thái chạy thường; chuột và bàn phím dùng cùng giới hạn. Lướt cũng áp dụng hệ số tốc độ. Các nâng cấp sát thương/nhịp bắn tiếp tục nhân với hệ số riêng của tàu. Giá đạn và số viên gốc trong loạt giữ nguyên.

HP của mình vẫn ở góc dưới trái, kèm ảnh/tên tàu. Góc trên phải là bảng mục tiêu vừa bị đánh: ảnh, tên, HP chính xác hiện tại/tối đa và trạng thái khiên. Có tám giây giữ thông tin, pause đóng băng thời gian này. Tàu chết hiện 0 HP; tái sử dụng slot địch không làm bảng nhảy sang tàu mới. Chỉ có một thanh boss lớn ở phía trên; khi đánh boss phụ thanh này theo đúng boss đó. Không có thanh HP lớn trên từng tàu thường.

Chỉ Phong Ưng và màn 1 được mở lúc bắt đầu. Mốc mở tám tàu giữ nguyên: [0,1,2,5,7,9,11,13]. Mỗi tàu có đại chiêu riêng. Sao bảo toàn tàu cần còn ít nhất 80% HP tối đa; xem hệ năm sao bên dưới.

Màn 1 có bốn bước hướng dẫn: di chuyển, bắn hạ mục tiêu tập, lướt và nhặt hộp đạn. Trong hướng dẫn không có đạn địch; hoàn tất sẽ cấp lại đạn khởi đầu. Có nút bỏ qua hoặc chơi lại hướng dẫn ở căn cứ.

Thắng boss mới mở màn/tàu tiếp theo. Tiến trình được lưu cục bộ; thua hoặc chơi lại không mất mở khóa. Mỗi lần vào màn sẽ hồi đầy HP và cấp lượng đạn quy định, đặt súng về Ion cấp 1 và đại chiêu về 0.

## Thành tích tối đa 5 sao

Cả 13 màn đều chấm năm tiêu chí độc lập khi thắng, mỗi tiêu chí +1 sao:

| Sao | Điều kiện |
| --- | --- |
| Hoàn thành | Hạ toàn bộ boss và chờ chuỗi nổ cuối |
| Bảo toàn phi cơ | Còn ít nhất 80% HP tối đa |
| Né tránh hiệu quả | Tổng HP thực sự đã mất trong màn ≤50% HP tối đa; sửa tàu không xóa số này |
| Mục tiêu phụ | Theo từng màn, xem nút **Tiêu chí 5 sao** ở căn cứ hoặc khi tạm dừng |
| Hạ boss nhanh | Hạ tất cả boss trong 75s; màn 3/5/9/11/12 cho 105s, màn 13 cho 150s |

Đồng hồ boss bắt đầu khi boss xuất hiện, dừng khi boss cuối bị hạ; không tính tạm dừng và chuỗi nổ cuối. Trận nhiều boss dùng chung một đồng hồ. Màn 1–3 có mục tiêu phụ trúng không quá hai đòn, màn 4 cần phá 18 đá bằng hỏa lực; các màn sau giữ mục tiêu riêng trong CAMPAIGN.md.

Bảng cuối trận có năm ngôi sao, từng tiêu chí đạt/chưa đạt, số liệu thực tế, số sao lượt này và kỷ lục. Có thể chơi lại ngay để săn sao. Thành tích lưu theo mức cao nhất, thua hoặc chơi kém hơn không làm mất sao cũ. Profile v4 giữ nguyên các số sao và mở khóa đã có; ba sao cũ tiếp tục là ba trên năm sao, không tự cộng thưởng.

## Điều khiển

Để chơi thử với đủ 13 màn và 8 phi cơ, mở bản `Builds/Tactical-Choices/IonFrontier.exe` với tham số `-ion-unlocked`. Chế độ này dùng profile tạm riêng cho mỗi lần chạy, bỏ hướng dẫn và cấp một lần Q khi vào màn. Mở bình thường không có tham số sẽ dùng tiến trình chính.

| Thao tác | Phím |
| --- | --- |
| Chọn tàu đã mở ở căn cứ | 1–8 hoặc bấm thẻ tàu |
| Chọn màn | Bấm một trong 13 ô màn ở căn cứ |
| Chọn tuyến màn 11 | E / R hoặc bấm nút tương ứng |
| Di chuyển | Chuột, WASD hoặc phím mũi tên |
| Bắn | Giữ Space hoặc chuột trái |
| Bật/tắt tự bắn | F — mặc định tắt để giữ đạn |
| Lướt né đòn | Shift hoặc chuột phải |
| Đại chiêu riêng của tàu | Q hoặc nút trên HUD |
| Tạm dừng / tiếp tục | P / Esc |
| Bật/tắt âm thanh | M |
| Chơi lại sau thắng/thua | R |

## Ghép tối đa 2 kiểu bắn

Ion kép là kiểu khởi đầu. Nhặt Tỏa, Xuyên hoặc Truy đuổi sẽ trang bị vào tối đa **hai ô kiểu bắn**.

- **Tỏa + Xuyên:** cả loạt đạn tỏa đều xuyên tối đa 3 tàu.
- **Tỏa + Truy đuổi:** cả loạt đạn tỏa tự chỉnh hướng tìm mục tiêu.
- **Xuyên + Truy đuổi:** đạn tự tìm mục tiêu, xuyên rồi tìm tàu tiếp theo.
- Nhặt một kiểu đã sở hữu sẽ tăng cấp riêng kiểu đó, tối đa cấp 3, không chiếm thêm ô.
- Nhặt kiểu thứ ba khác hai kiểu đang có sẽ thay kiểu cũ nhất. HUD và thông báo cho biết hai kiểu đang ghép.
- Quà **Cấp kiểu bắn +1** tăng cấp cả hai kiểu đang ghép (hoặc Ion nếu chưa ghép).

## Nâng chỉ số cộng dồn độc lập

Các nâng cấp dưới đây có thể kết hợp đồng thời với bất kỳ cặp kiểu bắn nào, không chiếm hai ô:

| Quà | Mỗi lần nhặt | Cộng dồn tối đa |
| --- | --- | --- |
| Sát thương | +20% sát thương cơ sở | 10 lần, đạt ×3 |
| Nhịp bắn | +10% số loạt/giây cơ sở | 10 lần, đạt ×2 |
| Đa đạn | Thêm 1 viên trong mỗi loạt | 8 lần, cộng thêm 8 viên |

Ví dụ, nhặt 3 quà sát thương cho +60%, không làm tròn mất phần lẻ khi gây damage. Nhặt 3 quà đa đạn thêm 3 viên vào loạt tỏa/xuyên/truy đuổi đang dùng. Một loạt vẫn chỉ tốn **1 đạn**. HUD hiển thị sát thương mỗi viên, số loạt/giây và tổng số viên trong loạt.

Chỉ số được tính trên kiểu bắn đang ghép; cấp kiểu bắn và các bộ đếm được giữ khi đổi kiểu trong cùng màn. Bắt đầu màn mới đặt lại các kiểu/chỉ số; tiến trình mở khóa tàu và màn vẫn được giữ. Quà chỉ số hoặc cấp kiểu bắn đã đầy được đổi thành 30 đạn.

## Đạn và quà rơi

- Đạn tối đa 600; hết đạn không thể bắn.
- Mỗi tàu thường bị hạ có **15%** cơ hội rơi một món. Các loại quà dùng trọng số; không còn quy luật cứ 3–4 mạng lại rơi và không còn quà ngẫu nhiên liên tục theo thời gian.
- Mỗi lần dọn xong một đợt có một quà tiếp tế/nâng cấp: ưu tiên đạn nếu thiếu, còn lại bổ sung kiểu bắn hoặc chỉ số.
- Khi còn dưới 35 đạn và không có hộp đạn trên sân, tiếp tế khẩn cấp có thể thả đạn; kiểm tra cách nhau 18 giây, hoặc 14 giây lúc đánh boss. Đây là cơ chế tránh kẹt màn, không phải nguồn quà ngẫu nhiên.
- Vật phẩm có hình, màu và tên riêng, được hút khi tàu tới gần. Nút **Vật phẩm** giải thích đầy đủ 10 loại.

| Quà | Nhận dạng | Tác dụng |
| --- | --- | --- |
| Đạn | Ba viên xanh ngọc | +60 loạt bắn |
| Sửa tàu | Khiên/thập xanh lam | Hồi 30 HP, không vượt HP tối đa |
| Cấp kiểu bắn | Hai mũi tên vàng | Tăng cấp cả hai kiểu đang ghép |
| Kiểu tỏa | Ba nhánh xanh lục | Ghép hiệu ứng đạn tỏa, kèm 20 đạn |
| Kiểu xuyên | Mũi nhọn tím | Ghép xuyên mục tiêu, kèm 20 đạn |
| Kiểu truy đuổi | Tâm ngắm cam | Ghép đạn tự tìm mục tiêu, kèm 20 đạn |
| Đại chiêu | Ngôi sao đỏ | Nạp đại chiêu riêng của tàu, trữ tối đa 1 |
| Sát thương | Hai nét chéo đỏ cam | +20% sát thương cơ sở |
| Nhịp bắn | Tia sét vàng nhạt | +10% nhịp bắn cơ sở |
| Đa đạn | Ba nút hồng tách nhánh | Thêm 1 viên trong loạt |

## Đại chiêu riêng cho từng tàu

Mỗi lõi đỏ nạp một lần Q, trữ tối đa 1. Ba chiêu mở đầu quét đạn địch và hủy laser tại lúc kích hoạt, không tốn đạn súng. Không thể kích hoạt chồng khi chiêu trước còn chạy; lõi nhặt được trong lúc đó được giữ lại.

| Tàu | Đại chiêu | Tác dụng |
| --- | --- | --- |
| Phong Ưng | Pháo Thiên Quang | Nạp 0,4 giây, tự ngắm một mục tiêu rồi bắn tia xuyên 1,2 giây. Tổng 90 sát thương trên mục tiêu nằm trong tia suốt thời gian bắn; xóa đạn địch chạm tia. Bảo vệ 2 giây. |
| Lôi Điểu | Lôi Liên Hoàn | 3 đợt sét, mỗi đợt nối tới tối đa 8 mục tiêu gần nhau, mỗi mục tiêu chịu 30 sát thương/đợt. Bảo vệ 2 giây. |
| Thiết Vệ | Pháo Đài Xung | Bảo vệ 4 giây, hồi 20 HP, phát 3 sóng trong bán kính 13 đơn vị, mỗi sóng gây 30 sát thương và xóa đạn/laser. |
| Hỏa Phượng | Mưa Nhiệt Hạch | 6 bom tự chọn vị trí địch, gây nổ theo vùng và để lại vùng cháy. |
| Ảnh Vũ | Ảnh Song Hành | Hai phân thân bay theo tàu, bắn đạn thật không tốn đạn kho trong 4 giây. |
| Từ Long | Xoáy Từ Phản Kích | Hút đạn địch trong 1,6 giây rồi phóng 12 viên phản kích; hút nhiều tăng sát thương. |
| Tinh Vân | Phi Đội Tinh Vân | Ba tàu yểm trợ tự ngắm, oanh kích bằng tia trong 4 giây. |
| Hư Không | Bẻ Cong Không Gian | Làm chậm địch và đạn trong bán kính 7 trong 3 giây, rồi gây nổ 90 sát thương. |

Tên chiêu được hiển thị trên HUD; căn cứ giải thích chiêu của tàu đang chọn. Tạm dừng đóng băng sát thương và diễn tiến chiêu; về căn cứ/chơi lại dọn toàn bộ hiệu ứng.

## Boss

- Bão Lửa: quạt đạn và loạt bắn ngắm hướng.
- Ảnh Ma: vòng đạn xoắn và loạt truy kích.
- Thiết Giáp Vương: quạt đạn dày, pháo kép, tường đạn có khe tránh và laser hạng nặng.
- Các boss có ba giai đoạn theo máu. Laser của đại boss có vạch đỏ báo trước 1,15 giây; tránh vạch trước khi tia sáng bật lên.
- Tất cả boss khi hết máu đều cháy, nghiêng/rơi, tách 6 mảnh từ chính sprite đó rồi nổ lõi. Từng xác tàu có chuỗi 4,8 giây riêng. Nếu còn boss sống, giao tranh tiếp tục; chỉ khóa trận và ghi thắng khi mọi boss nổ xong. P/Esc vẫn tạm dừng được.
- Màn 5 có Liệp Hạm + 2 Kìm Hãm; màn 9 có 3 tàu Tam Lõi với khiên luân phiên; màn 11 có Song Sát Đỏ/Lam. U Linh lộ lõi gần đèn hiệu; kỳ hạm có ba lớp hộ vệ, không bỏ qua bằng đòn quá mạnh.
- Tàu hộ vệ thường che chắn tàu gần đó; tàu oanh tạc sửa chữa đội hình theo nhịp. Đèn hiệu đã cứu và vận tải khỏe sẽ bắn yểm trợ bằng đạn thật.

## Source và tài nguyên

- `IonGame.cs`: chiến đấu, hướng dẫn, đạn, vật phẩm, các đợt địch và boss.
- `IonBackdrop.cs`: 13 tranh nền, sao/phong cảnh nhiều lớp, sương, pause và tự căn khung hình.
- `IonBackdropTest.cs`: gallery13màn, cảnh boss, hai tuyến11, resize và thử nhiều đạn.
- `IonHullStats.cs`: chỉ số tám phi cơ, HP, nguồn sát thương và theo dõi mục tiêu.
- `IonVitalsTest.cs`: kiểm tra HP, chỉ số riêng, mục tiêu và mô hình vật thể.
- `IonSpecialEffects.cs`: ba đại chiêu mở đầu.
- `IonBossWrecks.cs`: chuỗi cháy, vỡ thân và nổ cho mọi boss.
- `IonHostileFleet.cs`: danh mục địch mới, hành vi và nhóm xuất hiện.
- `IonMissionFlow.cs`: ba chặng mục tiêu và sự kiện trong từng màn.
- `IonEscapeMission.cs`: tuyến chạy thoát màn 4, thiên thạch có khe, tàu truy kích và cảnh báo pháo.
- `IonUpgradeDraft.cs`, `IonDraftHud.cs`: checkpoint, ba thẻ, khóa nhập và modal chọn nâng cấp.
- `IonCombatModules.cs`: chín mô-đun, nguồn sát thương và giới hạn kích hoạt theo loạt.
- `IonEncounters.cs`: sáu đội phối hợp, ngân sách, liên kết theo serial và cắt hỗ trợ.
- `IonCounterattack.cs`: hai động cơ, giáp thân, cửa sổ phản công và xác elite riêng.
- `IonTacticalTest.cs`: QA tích hợp cho lựa chọn, mô-đun, đội hình và hai nhánh phản công.
- `IonRatings.cs` + `IonRatingStar.cs`: năm tiêu chí, số liệu cuối trận và sao giao diện vector.
- `IonRatingTest.cs`: migration lưu cũ, 1/4/5 sao, sát thương tích lũy, đồng hồ boss và giữ kỷ lục.
- `IonEscapeTest.cs`: kiểm tra đường thoát, tiến độ, cảnh báo, va chạm và boss, gồm một lượt bay tự nhiên trọn tuyến.
- `IonExpansionWeapons.cs`: năm đại chiêu mới.
- `IonMissions.cs`: nhiệm vụ màn 4–13, vật thể có thể tương tác, tiếp tế, tuyến lựa chọn và sao thành tích.
- `IonExpandedBosses.cs`: mười trận boss mới và các hệ phòng thủ/điểm yếu.
- `IonCampaign.cs`: luật màn chơi, bảng trọng số quà và lưu tiến trình.
- `IonLoadout.cs`: hai ô ghép kiểu bắn, cấp kiểu bắn và ba bộ đếm nâng chỉ số.
- `IonHud.cs`: căn cứ, khóa/mở tàu và màn, hướng dẫn, HUD và bảng vật phẩm.
- `IonArt.cs`: hiệu ứng đạn, biểu tượng vật phẩm và âm thanh tổng hợp.
- `IonFleetLibrary.cs` + `Resources/FleetLibrary.asset`: 8 tàu người chơi, 10 sprite nền, 12 địch mới và 16 boss riêng.
- Sprite gốc: `Assets/IonFrontier/Art/Fleet`. Prompt và nguồn hình: `ArtSource/Fleet`, `ArtSource/Expansion`, `ArtSource/HostileFleet`, `ArtSource/MissionProps`.
- Font Liberation Sans và giấy phép OFL nằm trong `Resources`.
- Bảy mô hình vật thể hoàn thiện nằm ở `Assets/IonFrontier/Art/MissionProps`: đá, mìn, máy phát, đèn cứu hộ, lõi điều khiển, vận tải và móc kéo.
- Atlas bốn tàu cũ được giữ làm tài nguyên trước đó; gameplay dùng thư viện sprite riêng cho từng tàu.

## Lưu và kiểm tra

Tiến trình thật nằm tại `Application.persistentDataPath/pilot-profile-v2.json`; điểm cao ở `best-score.txt`.

Giữ nguyên đường dẫn lưu cũ. Profile version 2/3 được đọc và chuyển cấu trúc sang version 4, giữ màn/tàu đã mở; sao là dữ liệu bổ sung. Người đã qua màn 3 có thể vào màn 4 ngay, không bị mở sẵn năm tàu thưởng mới.

Chạy bản Windows với `-ion-playtest -ion-captures "THƯ_MỤC_KẾT_QUẢ"` để bật kiểm tra tích hợp. Harness dùng profile và điểm test riêng, tự thực hiện hướng dẫn/bắn, kiểm tra quà và dùng clear-wave để rút ngắn các chuyển màn. Đây không phải lượt chơi tay toàn bộ chiến dịch hoặc đánh giá cân bằng hoàn chỉnh.

Ảnh, log và báo cáo kiểm tra mới nhất tại `outputs/Tactical-Choices`. Harness cũ `-ion-playtest` kiểm tra hướng dẫn, ba màn đầu, ghép súng/vật phẩm và ba đại chiêu ban đầu. Harness mới `-ion-campaign-test -ion-captures "THƯ_MỤC"` kiểm tra chuyển file lưu cũ, nhiệm vụ/boss màn 4–13, hai tuyến màn 11, sao, 8 phi cơ, đại chiêu và giải phóng pool. Dùng profile/điểm test riêng; harness có tăng tốc giữ vùng/di chuyển và thao tác mục tiêu để kiểm tra chuyển trạng thái. Không dùng `-batchmode` cho Player nếu muốn chụp gameplay có hình. Kết quả không thay thế chơi tay toàn chiến dịch hoặc xác nhận cân bằng hoàn chỉnh.

Repo GitHub riêng tư đã được tạo trước đó với bản khởi đầu nhỏ. Các thay đổi gameplay hiện tại nằm trong project ổ D; chỉ push thêm khi có yêu cầu.

Bộ kiểm tra mới: `-ion-vitals-test -ion-captures "THƯ_MỤC"`; dùng profile/best riêng và kiểm tra va chạm đạn thật. Báo cáo tổng hợp nằm ở `TEST-REPORT.json`.

Kiểm tra nền: `-ion-backdrop-test -ion-captures "THƯ_MỤC"`, dùng profile/điểm riêng. Bản0.8.0 xác minh nền và chạy lại71kiểmtraHP/HUD; báo cáo chiến dịch396mục của0.7.0 được giữ riêng, không gộp thành kiểm tra mới.

Kiểm tra màn 4: `-ion-escape-test -ion-captures "THƯ_MỤC"`, dùng profile/best riêng. Có lượt bay tự nhiên khoảng 72 giây và các hook để kiểm tra nhanh mốc, pause, replay, va chạm và boss. QA bảo vệ tàu khi kiểm tra tiến trình; kết quả không xác nhận cân bằng bằng chơi tay.

Kiểm tra sao: `-ion-rating-test -ion-captures "THƯ_MỤC"`, dùng profile/best riêng. Có kiểm tra va chạm và đạn thật, cùng hooks để bố trí các mức thành tích, ngưỡng thời gian và kết thúc boss. Không phải đánh giá cân bằng bằng chơi tay.

Kiểm tra tích hợp tính năng0.11: `-ion-tactical-test -ion-captures "THƯ_MỤC"`. Các harness cũ tự chọn thẻ chỉ dưới cờ QA; bản chơi thường và `-ion-unlocked` yêu cầu người chơi chọn. Báo cáo mới phải được đọc theo TEST-REPORT.json, không cộng lại số kiểm tra lịch sử.
