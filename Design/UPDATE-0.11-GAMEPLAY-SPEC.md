# Đặc tả triển khai 0.11.0 — Lựa chọn, phối hợp và phản công

**Trạng thái:** đã triển khai bản0.11.0 và nghiệm thu542kiểm tra. Xem [báo cáo triển khai](UPDATE-0.11-IMPLEMENTATION.md), `TEST-REPORT.json` và `outputs/Tactical-Choices/QA-NOTES.md` để biết phạm vi/giới hạn. Nội dung dưới giữ làm đặc tả gốc.
**Bản nền:** ION FRONTIER 0.10.0 tại `D:\Projects\IonFrontier`.
**Ngày lập:** 21/10/2026.

## 1. Mục tiêu và phạm vi đã chọn

Người dùng đã chọn ba hướng:

1. Chọn một trong ba nâng cấp tại checkpoint để tự định hình cách chiến đấu.
2. Đội hình địch có phối hợp, tạo ra thứ tự ưu tiên và cách khắc chế.
3. Màn 4 có diễn biến chuyển từ bị truy đuổi sang chủ động phản công.

Kết quả mong muốn: trong một lượt chơi, người chơi phải đưa ra những quyết định có tác dụng nhìn thấy được. Chọn nâng cấp ảnh hưởng cách bắn; phá tàu hỗ trợ làm đội hình địch yếu đi; phản công thành công tạo khoảng thở thật trong màn 4.

Đợt này giữ chiến dịch 13 màn, tám phi cơ, các mốc mở tàu và hệ năm sao 0.10.0. Nội tại mới cho tám phi cơ, phân nhánh toàn chiến dịch, cực tính, chế độ hành trình và bộ phận phá được cho toàn bộ boss thuộc các đợt khác. Riêng tàu tinh nhuệ phản công màn 4 có hai điểm yếu đơn giản phục vụ tình huống này.

Thông số dưới đây là **giá trị khởi điểm để triển khai và chơi thử**. Sol có thể điều chỉnh nhỏ theo bằng chứng kiểm tra, ghi lại thay đổi và lý do. Không tự đổi luật mở khóa, lưu tiến trình hoặc biến mục tiêu phản công tùy chọn thành điều kiện thắng.

## 2. Những nguyên tắc của bản nền phải được giữ

- Đọc `AGENTS.md` ở project D trước khi làm. Thay đổi source, hình, tài liệu và build tại D; không đồng bộ bản C lịch sử.
- Giữ hai ô Tỏa/Xuyên/Truy đuổi; chỉ số damage/rate/multishot vẫn cộng dồn độc lập.
- Giữ 15% rơi quà của địch thường và lượng đạn đầu 180/210/240/300. Các phần thưởng bảo đảm mới phải được tính riêng, không tăng ngầm xác suất rơi.
- HP, chỉ số tám tàu, tám Q, theo dõi mục tiêu và thứ tự khiên boss tiếp tục hoạt động.
- Mọi boss chính/phụ vẫn vỡ và nổ trước khi ghi thắng. Tàu tinh nhuệ phản công không được kích hoạt kết thúc màn.
- Giữ profile v4 tại `pilot-profile-v2.json`, sao tốt nhất, mở khóa và chế độ `-ion-unlocked` dùng lưu tạm. Chưa cần đổi schema lưu vì mô-đun mới chỉ tồn tại trong lượt.
- Một thanh lớn phía trên: tiến độ ở đoạn chạy màn 4, HP boss khi vào boss. Chi tiết tàu thường/điểm yếu dùng bảng mục tiêu và nhãn nhỏ.
- Không dùng `Time.timeScale` toàn cục để làm màn chọn nâng cấp.
- Source local đang có nhiều thay đổi chưa lên GitHub. Không reset/checkout về source trên remote. Không commit/push trong đợt này khi người dùng chưa yêu cầu riêng.

## 3. Hướng A — Chọn nâng cấp tại checkpoint

### A1. Khi nào hiện lựa chọn?

Mỗi lượt có tối đa **hai lần chọn**:

| Nhóm màn | Lần chọn thứ nhất | Lần chọn thứ hai |
| --- | --- | --- |
| 1–3 | Sau khi dọn đợt 1 | Sau khi dọn đợt 2 |
| 4 | Tiến độ đạt 1.200 m / 33,3% | Tiến độ đạt 2.400 m / 66,7% |
| 5–13 | Chuyển từ chặng 1 sang chặng 2 | Chuyển từ chặng 2 sang chặng 3 |

Màn 1 không hiện lựa chọn trong phần tập lái. Màn 11 không hiện trong lúc đang chờ chọn tuyến E/R. Không mở modal lựa chọn khi boss chết, tàu đã thua hoặc đã về căn cứ.

**Ngân sách phần thưởng:**

- Màn 1–3: lựa chọn thay quà bảo đảm sau đợt 1 và 2. Phần thưởng sau đợt 3 giữ như hiện tại.
- Màn 4–13: giữ hộp đạn của checkpoint; lựa chọn thay món nâng chỉ số bảo đảm tại checkpoint đó.
- Quà rơi ngẫu nhiên, tiếp tế chống cạn đạn và tiếp tế trước boss tiếp tục hoạt động.
- Không mở thêm modal khi hạ tàu tinh nhuệ trong bản đầu; giữ giới hạn hai lần chọn để tránh ngắt nhịp liên tục.

### A2. Trải nghiệm màn chọn

1. Hoàn tất chuyển chặng, chốt phần thưởng/checkpoint đúng một lần.
2. Đưa game vào trạng thái lựa chọn và đóng băng toàn bộ mô phỏng chiến đấu.
3. Hiện ba thẻ nằm ngang: biểu tượng, tên, tác dụng, đánh đổi và độ phù hợp với súng hiện tại.
4. Người chơi bấm thẻ hoặc phím 1/2/3; chỉ một thẻ được nhận.
5. Hiện thông báo lựa chọn, đếm tiếp tục khoảng 0,5 giây rồi quay lại chiến đấu.

Hiển thị ngay trong modal:

- Hai kiểu bắn hiện tại và cấp của chúng.
- Những mô-đun đã chọn trong lượt.
- Với thẻ thay kiểu bắn thứ ba, ghi rõ kiểu nào sẽ bị thay.
- Với thẻ có giới hạn/điều kiện, diễn đạt bằng ngôn ngữ người chơi.

Ví dụ một thẻ:

```text
ĐẠN PHÂN MẢNH
Phát xuyên đầu tiên tạo thêm mảnh đạn nhỏ.
Đổi lại: sát thương viên chính giảm 15%.
Phù hợp với: XUYÊN GIÁP đang trang bị.
[CHỌN]
```

### A3. Quy tắc tạo ba thẻ

Ba thẻ phải khác nhau, đang dùng được và không rơi vào lựa chọn vô nghĩa vì đã đạt giới hạn.

| Ô | Vai trò ưu tiên |
| --- | --- |
| 1 | Phát triển súng: thêm/nâng một kiểu bắn hoặc tăng chỉ số chưa đầy |
| 2 | Một mô-đun chiến đấu mới tương thích với cấu hình hiện tại |
| 3 | Hỗ trợ tình thế: sửa HP khi HP <60%, đạn khi đạn <60; nếu không cần thì mô-đun hoặc chỉ số khác |

Giá trị thẻ cơ bản dùng cùng luật vật phẩm đang có: kiểu bắn kèm 20 đạn, đạn +60, sửa +30 HP, nâng damage/rate/multishot một cấp, nâng kiểu bắn theo luật hiện tại.

- Có thể chọn hỗ trợ kể cả khi checkpoint đã có hộp đạn; đây là đánh đổi chủ động của người chơi.
- Không đề xuất mô-đun cần Xuyên nếu người chơi chưa trang bị Xuyên.
- Không đề xuất lại mô-đun đã có; không có cấp mô-đun trong đợt này.
- Nếu pool hợp lệ không đủ, dùng lựa chọn tài nguyên/chỉ số hợp lệ để đủ ba thẻ khác nhau. Tình huống gần/full mọi giới hạn phải có fallback rõ như điểm thưởng; không tạo vòng lặp tìm thẻ vô hạn.
- Các mô-đun không tự chiếm hoặc thay hai ô kiểu bắn.
- Dùng RNG riêng cho lựa chọn. Mở/đóng modal không làm thay chuỗi spawn/loot chiến đấu. QA có seed cố định; seed của lượt có thể được ghi trong log để tái hiện lỗi.

### A4. Chín mô-đun đầu tiên

Tối đa hai mô-đun/lượt, nhận qua hai checkpoint. Tên và số liệu sau là cấu hình khởi điểm.

| ID | Tên | Điều kiện | Tác dụng |
| --- | --- | --- | --- |
| `focused_spread` | Nòng hội tụ | Đang có Tỏa | Góc tỏa còn 60%; sát thương đạn chính ×1,15. Đổi độ phủ lấy khả năng dồn đạn |
| `piercing_fragments` | Đạn phân mảnh | Đang có Xuyên | Đạn chính ×0,85 sát thương. Lần gây sát thương đầu có thể tạo hai mảnh nhỏ, mỗi mảnh bằng 30% sát thương gốc trước mức giảm này; tối đa bốn mảnh được sinh từ một loạt |
| `seeking_arc` | Sét dẫn hướng | Đang có Truy đuổi | Sau sáu loạt có ít nhất một hit thật, phóng một tia sang một địch khác cách mục tiêu bị trúng tối đa ba đơn vị; sát thương bằng 50% sát thương viên kích hoạt |
| `dash_capacitor` | Động cơ phản kích | Mọi cấu hình | Sau khi lướt, ba loạt bắn tiếp trong hai giây được ×1,25 sát thương; hết thời gian hoặc bắn đủ thì mất buff |
| `pulse_chamber` | Buồng xung điện | Mọi cấu hình | Mỗi loạt thứ tám đã bắn thành công có ×1,35 sát thương và thêm một lần xuyên; tối đa bốn mục tiêu kể cả khi có Xuyên |
| `aftershock_rounds` | Đạn dư chấn | Mọi cấu hình | Mỗi năm tàu thường bị súng chính hạ tạo một vụ nổ tại tàu vừa chết, bán kính 1,6; sát thương bằng 75% sát thương viên kết liễu |
| `reactive_armor` | Giáp phản ứng | Mọi cấu hình | Sau khi thực sự mất HP, phát xung xóa đạn thường của địch trong bán kính 1,8; hồi tám giây. Không hồi HP, không xóa laser và không xóa tổng DamageTaken |
| `salvage_magnet` | Nam châm tiếp tế | Mọi cấu hình | Bán kính hút quà tăng 60%, vận tốc hút tăng 20%; không tăng xác suất rơi |
| `ammo_reclaimer` | Bộ thu hồi đạn | Mọi cấu hình | Mỗi ba tàu thường do súng chính hạ hoàn lại ba đạn kho; tổng hoàn tối đa 30 đạn/màn |

**Luật phối hợp để tránh sai sát thương hoặc sinh đạn vô hạn:**

- “Đạn chính/súng chính” là đạn xuất phát từ thao tác bắn của người chơi. Đạn Q, phân thân, drone, đồng minh, mảnh phân tách và sét phụ không tự kích hoạt lại mô-đun.
- Gán nguồn sát thương và `volleyId` rõ ràng. Sét đếm một lần cho mỗi loạt có hit thật, không đếm tất cả viên/những lần xuyên của cùng loạt.
- Một viên tạo mảnh tối đa một lần; mảnh không sinh mảnh, không có Homing/Pierce và không đánh lại ngay mục tiêu đã sinh ra nó. Áp giới hạn bốn mảnh cho toàn loạt, kể cả khi nâng Đa đạn cao.
- Tính các hệ số đúng một lần. Chốt sát thương/trạng thái của projectile khi bắn để đổi loadout sau đó không sửa đạn đang bay.
- Hit bị khiên/ẩn chặn, hit vào xác hoặc mục tiêu đã chết không cộng bộ đếm hit/kill.
- Sát thương phụ vẫn phải qua cùng cổng khiên/pha của boss, không gọi giảm HP trực tiếp.
- Thay bỏ một kiểu bắn làm mô-đun yêu cầu kiểu đó chuyển sang trạng thái không hoạt động; HUD ghi lý do. Nhặt lại kiểu thì hoạt động lại, không reset hay nhân đôi bonus.
- Hết đạn không tạo “loạt thành công” để farm Buồng xung điện. Giáp phản ứng chỉ chạy sau damage thật, tối đa một lần theo hồi chiêu, kể cả laser gây damage nhiều frame.
- Mọi state/counter/hồi chiêu mô-đun reset ở `BeginRun`, dọn khi về căn cứ. Pause và màn chọn đóng băng thời gian của mô-đun.

### A5. Trạng thái dừng mô phỏng — điểm dễ sai

Nên thêm trạng thái rõ ràng như `FlightState.UpgradeChoice`, hoặc một cổng mô phỏng tập trung có ý nghĩa tương đương.

Trong modal và nhịp đếm tiếp tục phải đứng yên: tàu, địch, projectile, world object, beam, Q, xác boss, loot, timer mission, tiến độ chạy thoát, boss rating clock, hồi lướt, hồi mô-đun, target timeout và nền chuyển động. Animation của thẻ có thể dùng thời gian UI riêng.

Không chỉ phủ modal lên game đang chạy. Không chỉ dừng `UpdateEnemies` trong khi đạn/laser tiếp tục gây damage.

- P/Esc có thể mở pause phía trên modal; tiếp tục phải quay về đúng lựa chọn chưa chọn.
- Về căn cứ trong lúc chọn: hủy draft/pending reward và dọn state.
- Double click/giữ phím chỉ nhận một phần thưởng.
- Không để click chọn thẻ đồng thời thành click bắn/Q phía sau; chờ nhả click và hết nhịp tiếp tục.
- Nếu chết trong frame chạm checkpoint, trạng thái thua ưu tiên, không mở lựa chọn sau đó.
- Mỗi checkpoint có ID và cờ consumed. Gọi chuyển chặng lại hoặc resume không tạo thêm thẻ.

## 4. Hướng B — Đội hình địch phối hợp

### B1. Mục tiêu

Một đội hình phải có thể được nhận ra bằng hình, vị trí và hiệu ứng liên kết. Khi người chơi hạ đúng tàu hỗ trợ, khả năng phối hợp thật sự mất đi.

Tận dụng mẫu tàu và hành vi đã có. Đợt đầu chưa cần thêm sprite địch mới.

### B2. Sáu tình huống chiến đấu

| ID | Thành phần | Diễn biến và cách phá |
| --- | --- | --- |
| `shield_lance` | Một Hộ Vệ/Dệt Khiên + hai Xạ Thủ | Tàu khiên đứng trước, hai xạ thủ phía sau bắn lệch nhịp. Đường liên kết chỉ rõ nguồn giảm damage. Hạ tàu khiên thì bảo vệ mất ngay |
| `tractor_crossfire` | Một Lao Kéo + một Lăng Kính | Tàu kéo báo trước, kéo lệch vị trí rồi laser nạp tại một làn. Người chơi có thể hạ tàu kéo hoặc rời làn; lực kéo phải ngừng trước lúc laser bắn |
| `carrier_workshop` | Một Tổ Ong + một Thợ Máy + hai Dao Cắt | Thợ Máy sửa riêng tàu mẹ bằng liên kết thấy được. Hạ Thợ Máy cắt hồi máu; hạ Tổ Ong cắt sinh tàu con. Tối đa bốn tàu con sống thuộc đội |
| `pincer_wings` | Hai Dao Cắt + hai Cung Điện | Hai cánh vào từ hai phía, khai hỏa lệch nhịp khoảng một giây; vòng điện buộc chọn khe nhưng không tạo hai tường kín cùng lúc |
| `alarm_escort` | Một tàu phát tín hiệu dùng sprite Lăng Kính + hai hộ tống | Tàu tín hiệu có đồng hồ sáu giây. Hạ kịp hủy gọi viện và nhận thưởng nhỏ; nếu hết thời gian chỉ gọi một lần hai Dao Cắt rồi rút |
| `mine_torpedo` | Một Rải Mốc + một Lôi Ngư | Mìn tạo vùng cản có khe rõ; ngư lôi được phóng sau đó để ép đổi làn. Bắn mìn tạo nổ dây chuyền hoặc hạ tàu ngư lôi để giảm áp lực |

Thông số khởi điểm bổ sung:

- Giữ HP theo công thức hiện tại; không nhân HP hàng loạt để làm đội hình “khó”.
- `shield_lance`: giảm damage 35% như nhóm hộ vệ hiện tại, không cộng thêm một lớp giảm trùng.
- `carrier_workshop`: sửa tàu mẹ +2 HP mỗi ba giây, không vượt MaxHP. Tắt nhịp sửa vai trò cũ cho thành viên này để tránh hồi hai lần.
- `tractor_crossfire`: lực kéo tối đa 0,6 đơn vị/giây trong một giây; ngừng ít nhất 0,4 giây trước lúc laser bắn. Giữ cảnh báo laser ít nhất 1,15 giây và lối thoát bằng lái thường.
- `alarm_escort`: một lần thưởng +20 đạn trực tiếp hoặc +300 điểm nếu kho đã đầy; không rơi bảo đảm thêm mô-đun và không tự nhân số tàu gọi viện.
- Mọi timer của đội hình đóng băng khi pause/draft.

### B3. Áp dụng vào màn nào?

| Màn | Phạm vi đợt đầu |
| --- | --- |
| 1 | Giữ hướng dẫn và các đợt cơ bản; giới thiệu modal nâng cấp nhẹ nhàng |
| 2 | Một `shield_lance` ở đợt 3 dùng Hộ Vệ/Xạ Thủ nền, nằm trong tổng số tàu của đợt |
| 3 | Một `shield_lance` hoặc `pincer_wings` ở đợt 2/3; thay một phần thành viên cũ, giữ tổng số địch gần mức cũ |
| 4 | Dùng kịch bản riêng ở mục C, không chạy thêm director đội hình tổng quát |
| 5 | `shield_lance`, `mine_torpedo` trong đoạn hộ tống |
| 6 | `carrier_workshop`, `shield_lance`; tránh chồng đội laser lên sự kiện laser giữ vòng |
| 7 | Tổ chức lại nhóm Dao Cắt/Chớp Ảnh đã có thành đánh sườn có nhịp; giữ thời gian bám tín hiệu và số mục tiêu ưu tiên |
| 8 | `alarm_escort`, `pincer_wings` trong phần chiến đấu; giữ riêng nhóm gây nhiễu bắt buộc |
| 9 | `shield_lance`, `carrier_workshop` gây áp lực quanh vùng chiếm |
| 10 | `mine_torpedo`; giới hạn tổng mìn đang hoạt động để không kín sân |
| 11 | Chọn đội phù hợp tuyến E hộ tống hoặc R cứu hộ; tránh gọi thêm đội khi chưa chọn tuyến |
| 12 | `pincer_wings`; không dùng lực kéo sát vùng sụp đổ |
| 13 | Luân phiên `carrier_workshop`, `shield_lance`, `pincer_wings` trước boss |

`alarm_escort` chỉ dùng như địch tùy chọn ở màn nhiệm vụ, không dùng làm mục tiêu ưu tiên bắt buộc phải giết. Rút lui không được để lại `MissionMarkedAlive` hoặc điều kiện thắng bị kẹt.

### B4. Điều phối và ngân sách nguy hiểm

- Chỉ một đội phối hợp chủ đạo hoạt động tại một thời điểm; tối đa sáu thành viên trực tiếp, cộng tàu con theo giới hạn riêng.
- Thay một lượt `SpawnMissionSquad()` bằng đội phối hợp, không gọi cả hai rồi cộng mật độ.
- Chặng còn có đội ưu tiên bắt buộc thì tính chúng vào ngân sách. Nếu không đủ chỗ hoặc quá đông, hoãn đội tùy chọn; không bỏ mục tiêu bắt buộc.
- Sau một đội được xử lý, có khoảng 2–3 giây bớt áp lực trước đội lớn tiếp theo. Các mốc nhiệm vụ vẫn được phép chuyển ngay khi hoàn thành.
- Không chọn cùng một đội hai lần liên tiếp trong một màn. Chọn từ danh sách hợp lệ theo màn/chặng, dùng RNG riêng, seed tái hiện được.
- Director không hoạt động trong boss. Dọn liên kết, cảnh báo và thành viên tùy chọn khi chuyển boss/menu/replay.
- Trong cùng thời điểm chỉ có tối đa một cơ chế cưỡng ép vị trí mới như kéo/khóa làn. Kết hợp với hiểm họa nền phải để lại đường thoát.
- Không sinh tàu/bắn ngay trên người chơi. Có chuyển động vào sân và dấu báo trước hướng tấn công.

### B5. Dữ liệu cần theo dõi

Mỗi encounter có ID, loại, trạng thái, thành viên, vai trò, timer và các cờ sự kiện một lần. Tham chiếu thành viên phải kiểm tra cả slot và serial để không liên kết nhầm tàu mới khi pool tái sử dụng.

Thứ tự gợi ý: `Entering → Telegraphing → Executing → Recovering → Resolved`.

Khi tàu chỉ huy/hỗ trợ chết, đội lập tức bỏ hành vi phụ thuộc nó. Các thành viên còn lại có thể chiến đấu bằng hành vi thường; không bị đứng im vì mất leader.

Lưu ý tích hợp: bản nền chỉ gọi một số luật giáp/hỗ trợ mở rộng khi `Sector>=4`. `shield_lance` ở màn2–3 phải có tác dụng thật qua controller/đường damage phù hợp, không chỉ hiện đường liên kết. Kiểm tra đúng mức giảm35% và mất bảo vệ sau khi tàu khiên chết tại cả màn2 lẫn màn5.

## 5. Hướng C — Màn 4 có đoạn phản công

### C1. Cảm xúc và luật tiến trình

Kịch bản: **bị bám đuôi → bị ép đổi làn → phát hiện sơ hở → phản công → tới điểm nhảy bị boss chặn**.

Giữ đường thoát 3.600 m và tốc độ 50 m/giây. Đó là khoảng 72 giây mô phỏng hoạt động, không tính thời gian modal hoặc pause. Checkpoint vẫn ở 1.200 và 2.400 m. Thanh tiến độ không reset khi đổi tình huống.

### C2. Timeline đề xuất

| Thời gian hoạt động | Quãng đường | Tình huống |
| --- | --- | --- |
| 0–12s | 0–600 m | Hai tàu bám đuôi vào sớm, quạt ba viên, đá có khe như bản0.10 |
| 12–24s | 600–1.200 m | Một tàu truy kích vọt lên sườn, tạo loạt bắn chặn đường; tàu còn lại tiếp tục gây áp lực phía sau |
| Checkpoint 1 | 1.200 m | Nhận hộp đạn và chọn một trong ba nâng cấp |
| 24–40s | 1.200–2.000 m | Các đợt đá cắt ngang luân phiên, hỏa lực trái/phải có nhịp; tránh chồng kín cả hai phía |
| 40–48s | 2.000–2.400 m | Báo có tàu truy kích hạng nặng tăng tốc vượt lên; thấy nó bay ở rìa sân, chuẩn bị chặn phía trước |
| Checkpoint 2 | 2.400 m | Lựa chọn nâng cấp thứ hai, báo trước cơ hội phản công |
| 48–50s | 2.400–2.500 m | Tàu tinh nhuệ vào vị trí phía trên, lộ hai động cơ; địch phía sau rút khỏi đoạn này |
| 50–64s | 2.500–3.200 m | Cửa sổ phản công: phá hai động cơ rồi đánh thân tàu; đá thưa hơn để có thể ngắm |
| 64–72s | 3.200–3.600 m | Bứt tới điểm nhảy; mức áp lực phụ thuộc phản công thành công hay thất bại |
| 100% | 3.600 m | Dọn hiểm họa truy đuổi, báo tới nơi, rồi Kẻ Nghiền Đá xuất hiện như bản nền |

Tàu đổi vị trí phải bay theo đường có thể nhìn/đoán được; không teleport từ sau lên trước ngay trên tàu mình. Ở sự kiện 12–24s, tàu bắn chặn chỉ dùng ba viên lệch hướng và không bắn đè đúng thời điểm pháo sau khai hỏa. Mọi lượt khóa ngắm vẫn có khoảng báo trước 0,85 giây.

Không cần triển khai hành lang cửa đóng/mở hoặc xác tàu có va chạm trong đợt này. Đá cắt ngang và đội hình chặn đầu đủ cho phần chuẩn bị; tập trung làm đoạn phản công rõ ràng.

### C3. Tàu tinh nhuệ phản công

Tên hiển thị gợi ý: **TRUY KÍCH HẠNG NẶNG**.

- Dùng một sprite địch hiện có phù hợp như Pháo Đài, thêm dấu hiệu động cơ/giáp và tên để phân biệt vai trò tinh nhuệ.
- Là elite nhiệm vụ, không đăng ký thành boss và không làm `BossActive`/boss rating clock khởi động.
- Thân tàu 42 HP; hai động cơ, mỗi động cơ 18 HP. Tổng sát thương cần tối thiểu 78 trước các hiệu ứng khác.
- Vị trí ổn định quanh y=3, dao động ngang chậm ±2 đơn vị; hai động cơ ở hai bên thân, đường bắn từ dưới lên tiếp cận được.
- Khi còn ít nhất một động cơ, thân tàu có giáp chặn damage; động cơ không có giáp này.
- Phá động cơ trái loại bỏ cụm pháo trái; phá phải loại bỏ cụm pháo phải. Hỏng một bên thì tàu nghiêng nhẹ và có khói tại đúng vị trí đó.
- Khi cả hai động cơ bị phá, thân tàu mở giáp và hiện dấu hiệu “LÕI ĐÃ LỘ”.
- Pháo mỗi bên bắn loạt nhỏ, nhịp nền khoảng ba giây, tốc độ4,5 và damage10. Có khóa hướng báo trước. Không thêm laser vào elite này.
- Trong cửa sổ phản công, đá giãn còn khoảng3,2giây/đợt và vẫn có khe. Không sinh lại hai tàu bám đuôi từ director cũ.

**Giao diện:**

- Thanh lớn phía trên vẫn là đường thoát.
- Hiện dòng `PHẢN CÔNG / PHÁ HAI ĐỘNG CƠ / CÒN ... GIÂY` ở panel nhiệm vụ.
- Mỗi động cơ có biểu tượng nhỏ và trạng thái sáng/hỏng; không tạo thêm hai thanh HP lớn.
- Bảng mục tiêu trên phải hiển thị tên/HP đúng phần vừa bị đánh, ví dụ `ĐỘNG CƠ TRÁI — 12/18`.
- Không tự kéo khóa mục tiêu/homing vào thân đang đóng giáp khi có động cơ hợp lệ trước mặt.

### C4. Thành công và thất bại đều đi tiếp được

**Thành công trước 64 giây:**

- Elite cháy, mất mảnh và rơi khỏi đường bay bằng hiệu ứng không kết thúc màn.
- Nhận +600 điểm và +40 đạn trực tiếp, cap600; có thông báo thưởng một lần.
- Không có thêm modal nâng cấp.
- Đoạn bứt thoát còn lại bớt hỏa lực truy kích, tạo cảm giác người chơi đã giành thế chủ động.

**Chưa hạ được tới 64 giây:**

- Elite rút khỏi sân; dọn hitbox động cơ và cảnh báo của nó.
- Có thể bắn một loạt tiễn biệt có cảnh báo trong đoạn bứt thoát, không spam kéo dài hoặc tăng HP boss cuối.
- Không nhận thưởng phản công, nhưng tiến độ tiếp tục tới100% và vẫn có thể thắng màn.

Giữ mục tiêu phụ hiện tại phá18thiênthạch bằng hỏa lực và năm tiêu chí sao0.10.0. Hạ elite là cơ hội thưởng/tạo khoảng thở, chưa phải sao bắt buộc mới. HP440 và các pha của Kẻ Nghiền Đá giữ như bản nền.

### C5. Các tương tác bắt buộc xử lý

- Q có thể phá động cơ và đánh thân khi giáp đã mở; không khóa Q một cách riêng chỉ để ép bắn thường.
- Sát thương xuyên, mảnh, sét và nổ phải xử lý đúng thứ tự động cơ/thân, không trừ cùng một hit nhiều lần vì hitbox chồng.
- Điểm yếu không được cộng vào `objectiveCount` máy phát, `EscapeRocksCleared` hoặc số boss đã hủy.
- Hạ elite không gọi nhánh `Finish(true)`, không mở màn5, không ghi sao và không đặt `State=BossDying` kết thúc trận.
- Nếu tái sử dụng hiệu ứng xác boss, phải có chế độ/callback dành cho elite, không tác động các bộ đếm và điều kiện kết thúc boss thật.
- Elite rút, chuyển boss, chết, restart hoặc về menu đều dọn động cơ/renderer/cảnh báo; slot pool được tái dùng an toàn.
- Sự kiện theo quãng đường dùng điều kiện vượt ngưỡng và cờ đã chạy; không so sánh float bằng đúng một giá trị. QA tăng tốc qua ngưỡng vẫn phải kích hoạt theo đúng thứ tự.

## 6. Điểm tích hợp trong code hiện tại

Các tên file mới dưới đây là gợi ý tổ chức, Sol có thể đổi nếu giữ ranh giới trách nhiệm rõ.

| Phần | File liên quan | Hướng triển khai |
| --- | --- | --- |
| Chọn nâng cấp | `IonGame.cs`, `IonMissionFlow.cs`, `IonHud.cs` | Hook đúng các checkpoint; một trạng thái dừng mô phỏng; UI ba thẻ |
| Mô-đun | `IonLoadout.cs`, `IonHullStats.cs`, projectile/damage trong `IonGame.cs` | Tách dữ liệu và state mô-đun, ví dụ `IonUpgradeDraft.cs` + `IonCombatModules.cs`; nguồn damage và volley rõ ràng |
| Áp thẻ cơ bản | `ApplyPickup` hiện tại | Tái dùng phần áp hiệu ứng; không giả tạo một món pickup/cộng điểm nhặt quà nếu không có vật phẩm thật |
| Đội hình | `IonMissions.cs`, `IonHostileFleet.cs` | Director thay lượt spawn cũ; controller đội hình riêng như `IonEncounters.cs` |
| Luật giáp/sửa | `ModifyEnemyDamage`, `UpdateMission` | Không áp bảo vệ/hồi HP hai lần cho đội hình mới |
| Màn4 | `IonEscapeMission.cs` | Các phase/sự kiện theo routeDistance; khóa respawn cũ khi phản công |
| Điểm yếu elite | Damage/target tracking/world objects | Có định danh/serial và cách hiển thị riêng; mapping sprite WorldKind phải tường minh nếu thêm kind |
| Pause/nền | `IonGame.cs`, `IonBackdrop.cs`, `IonHud.cs` | Freeze đầy đủ cho state mới; resume về đúng modal |
| Sao/lưu | `IonRatings.cs`, `IonCampaign.cs` | Dừng timer đúng lúc; giữ schema và luật sao hiện tại |
| QA | Các harness đã có + harness mới | Tự chọn thẻ chỉ dưới cờ test rõ ràng, không bật tự chọn ở bản `-ion-unlocked` |

Tránh nhét toàn bộ logic mới vào một switch dài trong `IonGame.cs`. Nên có cấu hình cho mô-đun, đội hình và timeline để chỉnh cân bằng mà không sửa rải rác nhiều file.

## 7. Thứ tự thực hiện cho Sol

1. Đọc AGENTS/hồ sơ, xác minh bản local, chụp danh sách file sẽ sửa và backup trong `LOCAL-NOTES/Before-Tactical-Choices-0.11.0`.
2. Làm state lựa chọn + UI + thẻ cơ bản; xác minh pause/reset/input trước khi thêm hiệu ứng mô-đun.
3. Thêm chín mô-đun và các điểm kích hoạt có nguồn damage/volley; kiểm tra combo và pool.
4. Làm director cùng hai đội đầu `shield_lance`, `pincer_wings`; sau đó bốn đội còn lại và áp danh sách theo màn.
5. Làm timeline màn4, elite/động cơ, phản công thành công và hết hạn vẫn đi tiếp.
6. Chạy QA tích hợp, xem screenshot và thử các tình huống chơi thực; chỉnh thông số có căn cứ.
7. Cập nhật README, CAMPAIGN, GAME-HANDBOOK và AGENTS để phân biệt bản mới với0.10.0.
8. Build riêng `Builds/Tactical-Choices/IonFrontier.exe`, launcher thường/đủ tính năng, ZIP và báo cáo. Giữ build0.10.0 để đối chiếu.

## 8. Tiêu chí nghiệm thu

### Nâng cấp

- Mỗi màn có đúng hai checkpoint lựa chọn theo bảng; không lặp khi pause/restart/chuyển chặng nhanh.
- Ba thẻ khác nhau và hợp lệ, thấy rõ kiểu bị thay/đánh đổi; trạng thái súng sau chọn đúng với nội dung thẻ.
- Cả hai cách nhập chuột và1/2/3 hoạt động; không nhận hai thẻ khi double click.
- Toàn bộ combat/background/timer đứng yên trong draft; boss rating, thời gian phụ95/120s và route72s không chạy ngầm.
- Mỗi mô-đun có kiểm tra tác dụng thực, reset và điều kiện kích hoạt; không chỉ kiểm tên/ID tồn tại.
- Tỏa/Xuyên/Truy đuổi ở các tổ hợp, cấp tối đa, tám cấp đa đạn và chín mô-đun không tạo proc đệ quy hoặc vượt pool.
- Nhận thẻ không thay loot15%, không sửa profile thật trong QA, không cộng lại reward checkpoint.

### Đội hình

- Sáu đội vào sân và thực sự thực hiện cơ chế phối hợp, có thể quan sát nguồn khiên/sửa/kéo/cảnh báo.
- Hạ nguồn hỗ trợ cắt đúng tác dụng; leader chết không làm tàu còn lại đứng im.
- Kéo+laser có lối thoát bằng di chuyển thường khi lướt đang hồi.
- Tàu con/gọi viện không tăng vô hạn; hết slot thì hoãn/spawn ít theo luật, không mất objective chính.
- Không dùng slot/serial cũ để nối liên kết tới tàu mới.
- Mọi màn vẫn hoàn thành được, nhất là cứu hộ8/11, convoy5/10/11, đồng bộ9 và nhiều pha13.

### Màn4

- Có lượt chạy tự nhiên đủ3600m; sự kiện đầu, chặn sườn, hai draft, phản công và boss đúng thứ tự.
- Có lượt dùng súng Ion cơ bản hạ hai động cơ/thân trong cửa sổ phản công để chứng minh không bắt buộc một mô-đun hoặc Q cụ thể. Các nâng cấp vẫn giúp tạo thuận lợi.
- Có lượt chủ động bỏ qua elite vẫn tới100%, đánh boss và mở màn5 bình thường.
- Có lượt Q phá elite: không kết thúc màn sớm, không cộng sao/mở khóa hoặc kích hoạt boss clock sai.
- Phá động cơ từng bên thay đổi đúng cụm pháo; bảng mục tiêu hiện đúng HP/thân/động cơ.
- Tạm dừng/draft/replay/menu tại các phase không để sót hitbox, warning hoặc tham chiếu pool.
- Giữ áp lực đầu màn0.10.0; trong cửa sổ phản công phải có khoảng ngắm thực tế, không chỉ thêm elite vào giữa mật độ cũ.

### Hồi quy, hình ảnh và báo cáo

- Chạy lại các bộ cần thiết: opening regression (107mục cũ), campaign (218mục cũ), vitals (71mục cũ), rating (37mục cũ), escape (34mục cũ), sau khi cập nhật kỳ vọng thực sự bị thay đổi bởi feature mới.
- Các con số trên là baseline lịch sử; báo cáo mới ghi đúng số kiểm tra đã chạy. Không cộng các kết quả cũ chưa chạy thành PASS của0.11.
- Harness cũ cần xử lý draft bằng lựa chọn minh bạch dưới cờ QA; không tắt cả draft/đội hình/phản công để giữ test xanh.
- `EscapeTest` hiện từng đòi hai pursuer ở đoạn cuối: sửa theo timeline mới, kiểm elite/động cơ và điều kiện đi tiếp thay vì giữ assertion cũ.
- Thêm một harness tích hợp cho features mới; ưu tiên kiểm state transitions, quyền nguồn damage, success/failure, pause và save.
- Ảnh cần có: ba thẻ lựa chọn, một combo mô-đun, đội khiên/xạ thủ, đội kéo/laser, elite hai động cơ, hỏng một động cơ, phản công thắng, phản công hết hạn và bảng sao sau boss.
- Xem screenshot thực tế ở1280×720 và ít nhất một tỷ lệ khác để kiểm modal/HUD không chồng. Không cần chạy toàn bộ campaign lặp lại cho mỗi thay đổi trình bày nhỏ nếu không có rủi ro mới.
- Ghi FPS, số projectile/địch/world cao nhất và lỗi runtime. So với baseline cùng máy; không tuyên bố cân bằng chơi tay đầy đủ nếu chỉ chạy harness bảo vệ tàu.
- ZIP kiểmCRC/hash; báo cáo kèm giới hạn đã biết và nguồn build. Không tự push GitHub.

## 9. Những gì được coi là hoàn thành

Một bản0.11.0 có thể chơi từ đầu tới cuối chiến dịch với hệ lựa chọn hoạt động, sáu đội phối hợp xuất hiện theo cấu hình và màn4 có cả nhánh phản công thắng/thất bại. Người chơi nhận ra tác dụng của lựa chọn và việc phá thành viên hỗ trợ qua gameplay và hình ảnh. Build, ảnh kiểm tra, tài liệu và báo cáo cùng phản ánh trạng thái cuối.

Nếu sau chơi thử có thông số chưa cân bằng, ghi con số hiện dùng và tình huống gây vấn đề. Các lỗi kẹt màn, sai lưu, sai khiên, nhân damage, rơi mục tiêu và mô phỏng chạy ngầm khi draft phải được sửa trước khi giao bản.
