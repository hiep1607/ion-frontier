# Đánh giá độc lập ION FRONTIER 0.11.0

Ngày: 22/10/2026. Phạm vi: chất lượng phần triển khai của Sol, độ tin cậy của QA, khả năng đọc tình huống chiến đấu và hướng phát triển tiếp.

## Kết luận

Đây là một bản thử nghiệm có nền kỹ thuật khá tốt: ba hướng chính đã được triển khai thành cơ chế thật, có build riêng, backup, tài liệu và kiểm thử. Tuy nhiên, chưa nên coi 542 PASS là bằng chứng đã hoàn thiện toàn bộ chất lượng game. Review độc lập tìm được một lỗi chuyển thua thành thắng và một hành vi laser cần sửa/chốt luật. Bằng chứng về cân bằng chơi tay và giật khung hình còn thiếu.

Đánh giá định tính của người review: khoảng **7/10 cho một bản thử nghiệm đang phát triển**. Đây không phải điểm đo bằng chơi tay toàn bộ 13 màn. Nên làm bản 0.11.1 sửa lỗi và cải thiện phản hồi trước khi thêm nội dung lớn.

## Đã kiểm tra như thế nào

- Đọc AGENTS, đặc tả, báo cáo triển khai, source mới và các đường tích hợp damage/state/HUD/mission.
- Đối chiếu SHA256: mọi file source gốc trong release-source-audit vẫn khớp; DLL trong build phát hành khớp báo cáo. Sáu báo cáo có tổng 542 assertion PASS đúng như đã công bố.
- Đọc cách harness tạo tình huống, không chỉ đọc nhãn PASS. Xem ảnh Player màn chọn, combo đạn và động cơ elite.
- Chạy hai probe độc lập trong Unity Editor, gọi các phương thức combat thực với bước 20 ms, save riêng. Đây là tình huống dựng có kiểm soát, không phải lượt chơi tay trong Player.
- Probe tạm đã được đưa ra khỏi Assets. Source gameplay và Player phát hành không bị sửa. Bằng chứng: `outputs/Quality-Review-0.11.0/independent-probes.json`, `IonQualityReviewProbe.cs.txt`, `source-integrity.json`; log `outputs/quality-review-probe-final.log`.

## Phát hiện theo ưu tiên

### P1 — Đã thua vẫn có thể thắng và mở màn nếu boss chết ngay sau đó

**Đã tái hiện.** Tàu mình còn 5 HP, nhận đòn 10 HP; một viên đạn thân thiện đã ở gần boss còn 1 HP. Thực hiện đúng thứ tự xử lý của vòng Update: xử lý sát thương vào người chơi, rồi cập nhật viên đạn còn lại.

Kết quả: `Defeat → BossDying → StageClear`, HP cuối bằng 0; clearedStages từ 3 thành 4, vẫn ghi 2 sao. Không có exception.

Nguyên nhân: `IonGame.cs:267` gọi UpdateEnemies rồi UpdateBullets mà chưa kiểm tra trạng thái giữa hai bước. Finish(false) chỉ xóa đạn địch, nên đạn của mình vẫn còn. DamageEnemy không chặn xử lý sau Defeat; `IonBossWrecks.cs:46` đặt BossDying khi boss cuối chết và cuối chuỗi wreck gọi Finish(true).

Đường xử lý này đã có trong backup 0.10.0; **không đủ cơ sở quy nó là lỗi mới do Sol đưa vào 0.11**. Nhưng bộ nghiệm thu hiện tại chưa phát hiện được nó và bản giao vẫn có lỗi.

Hướng sửa: xác định kết quả trận một lần; kiểm tra trạng thái ngay sau bước có thể làm tàu chết; chặn callback damage/death đến muộn thay đổi kết quả đã chốt. Thêm test cùng frame: va chạm/đạn gây chết + đạn mình hạ boss; không ghi thắng/sao/mở khóa sau Defeat nếu luật ưu tiên thua được giữ.

### P2 — Laser đang nạp vẫn bắn khi tàu phát đã bị hạ

**Hành vi đã tái hiện.** Tạo đội kéo/laser, để Lăng Kính tạo cảnh báo, hạ Lăng Kính trước khi laser bắn, rồi cho mô phỏng laser chạy 65 bước × 20 ms. Nguồn đã chết nhưng HP người chơi vẫn giảm từ 100 xuống khoảng 93,6.

`SpawnBeam` chỉ lưu origin/age, không lưu tàu nguồn và serial. `IonEncounters.cs:191` tạo beam độc lập; `OnEncounterKilled` không hủy beam của Lăng Kính; `IonGame.cs:535` tiếp tục kích hoạt và gây damage.

Nên chốt luật: viên đạn đã rời nòng có thể tiếp tục bay, nhưng laser còn đang nạp nên mất khi nguồn bị phá. Nếu muốn giữ laser sau khi tàu chết thì phải thể hiện nó như một hiểm họa độc lập; hiện hình và luật “phá nguồn để cắt phối hợp” chưa rõ ở tình huống này.

Hướng sửa đề xuất: beam có owner slot + serial, hủy đúng beam phụ thuộc nguồn khi nguồn chết/rút. Không ClearBeams toàn cục vì sẽ xóa cả laser của tàu khác. Test hạ nguồn trước cảnh báo, trong lúc nạp, lúc đang bắn và sau tái sử dụng slot.

### P2 — FPS trung bình đang bỏ qua các frame chậm nhất

`IonTacticalTest.cs:14` chỉ đưa frame có unscaledDeltaTime < 0,1 giây vào phép tính. Campaign và các harness khác cũng dùng cách lọc tương tự. Vì vậy một khựng 200 ms không làm xấu con số FPS được công bố.

Điều này **không chứng minh game đang giật**; nó cho thấy số 59–60 FPS chưa chứng minh được game không giật. Nên ghi tất cả frame sau một khoảng warm-up được công bố, thêm p95/p99 frame time, frame chậm nhất, số frame >33/50/100 ms và allocation/GC. Đo trong cảnh đông địch, nhiều boss và combo súng, không chỉ suite Vitals.

### P2 — Một phần QA kiểm sự kiện riêng lẻ, chưa kiểm đủ tổ hợp nguy hiểm

`TestAdvanceEncounter` chỉ tăng TickEncounters và UpdateEncounterEnemy; nó không tăng đồng thời player, projectile, beam và world theo từng bước. Test kéo/laser xác nhận có pull event và beam được tạo, nhưng chưa chứng minh người chơi né được trong trận thật khi lướt đang hồi.

Tactical báo peak theo frame là 79 đạn / 4 địch / 10 world object. Đây không phải stress toàn ngân sách địch, vì một số địch được tạo và hạ trong cùng một bước hook trước lần lấy mẫu tiếp theo. Chạy bình thường với bảo vệ 10.000 giây cũng không xác nhận được độ khó.

Cần bổ sung: input chuột/phím thật; chết đúng frame chuyển chặng; giữ click khi chọn; pause/resume sau chọn; hạ từng nguồn hỗ trợ khi còn cảnh báo; cảnh nhiều boss + đạn nâng tối đa; các lượt màn 4 không bất tử với nhiều hull. Không cần tăng số test bằng cách chỉ kiểm thêm tên/ID.

### P3 — Phản hồi mô-đun và phân biệt hình chưa theo kịp cơ chế

- Tất cả mô-đun dùng chung icon nâng cấp trong `IonUpgradeDraft.cs:20`; đọc tên được nhưng khó nhận diện nhanh.
- HUD mô-đun là dòng chữ cỡ 12 trong `IonDraftHud.cs:14`, nhỏ ở ảnh 720p. Chưa thấy số tích lũy sét/xung, ba phát sau lướt hoặc cooldown giáp phản ứng.
- Mảnh phân tách đổi màu, damage và hit radius nhưng vẫn nhận scale sprite của Bolt súng hiện tại. Ảnh combo cho thấy mảnh hồng gần kích thước đạn chính. Nên có hình ngắn/nhỏ và trail riêng.
- Động cơ elite tái dùng sprite máy phát. Dùng được cho prototype; nên có sprite động cơ gắn với thân và trạng thái hỏng rõ hơn ở lượt polish.

Đây chủ yếu là thiếu hoàn thiện hình/UX, không phải lý do phủ nhận cơ chế mới đã chạy.

## Phần Sol làm tốt

| Hạng mục | Nhận xét |
| --- | --- |
| Bám phạm vi | Đủ ba hướng, giữ 13 màn/8 tàu, không mở thêm hệ ngoài yêu cầu |
| Damage mô-đun | Có nguồn primary, volley, snapshot khi bắn, giới hạn mảnh và kiểm tra không proc đệ quy |
| Trạng thái chọn | Có state riêng, RNG riêng, consumed checkpoint, chống nhận hai thẻ, dừng nền/timer |
| Đội hình | Có tác dụng thật của khiên/sửa/sinh con; dùng slot + serial, có cleanup và giới hạn |
| Phản công | Engine/thân tách biệt, hạ elite không gọi thắng, có nhánh timeout và wreck riêng |
| Bàn giao | Có backup, build/ZIP riêng, CRC/hash, source provenance và mô tả giới hạn QA khá trung thực |

Code được chia file theo chức năng, nhưng phần lớn state vẫn nằm trong một IonGame partial và nhiều dòng gộp quá nhiều thao tác. Khi thêm cơ chế tiếp, việc theo dõi thứ tự cập nhật và trạng thái kết thúc sẽ khó hơn. Nên tách cấu hình module/encounter khỏi hành vi, đồng thời chuẩn hóa đường kết thúc trận và nguồn damage trước; chưa cần viết lại toàn bộ engine.

## Hướng tiếp nên làm theo thứ tự

### 1. Bản 0.11.1 — Chắc luật và dễ đọc

Ưu tiên P1 thua/thắng, quyền sở hữu laser và QA frame time. Sau đó thêm icon riêng + thông tin đang tích lũy/hồi chiêu cho mô-đun; ghi rõ “cần Tỏa/Xuyên/Truy đuổi” khi mô-đun tạm ngừng.

Nghiệm thu: không đổi kết quả sau Defeat; laser phụ thuộc tàu chết được xử lý đúng luật; chuột và 1/2/3 đều chọn đúng một thẻ; không bắn ngoài ý muốn sau modal; có báo cáo p95/p99 và frame chậm nhất. Chạy hồi quy đúng các đường vừa sửa.

### 2. Bản 0.12 — Cân bằng và làm người chơi cảm nhận được lựa chọn

Chơi thử tập trung màn 1, 4, 6, 9, 11, 13, ưu tiên lượt bình thường không mở toàn bộ/bất tử. Ghi thời gian qua chặng, damage nhận, nguồn gây chết, ammo còn lại, tỷ lệ chọn từng thẻ, thời gian hạ elite và tỷ lệ hoàn thành.

So sánh ít nhất hull cân bằng, nhanh và nặng. Không kết luận từ autopilot hull0. Thử đủ trường hợp có/không có Q khi phản công: Q hiện phá hai động cơ rất mạnh và trước boss lại được nạp Q; cần xem phần thưởng có tạo quyết định đáng cân nhắc không, thay vì tự động cấm Q đánh điểm yếu.

Thẻ nên hiển thị thay đổi trực tiếp: kiểu nào bị thay, damage/nhịp/loạt trước–sau, mô-đun nào sẽ ngừng. Giữ chín mô-đun cho tới khi tác dụng của từng cái được nhìn và nghe rõ.

### 3. Đợt nội dung sau — Biến thể tình huống trong 13 màn

Ưu tiên 2–3 biến thể vào sân/mục tiêu cho các đội đang có. Ví dụ đội khiên giữ vị trí trước convoy ở lượt này, che một trạm cần phá ở lượt khác; đội báo động đi theo hai đường rút; gọng kìm đổi cánh mở trước. Giữ tín hiệu nhận biết và tổng áp lực, không chỉ cộng thêm tàu hoặc HP.

Chọn biến thể theo chặng và nhiệm vụ, tránh lặp liên tiếp; dùng seed để tái hiện. Có thể thêm một mục tiêu phụ ngắn như chặn tàu tiếp tế để đổi lấy ammo/sửa chữa, nhưng chỉ sau khi số liệu cho thấy nhịp chơi cần nó. Chưa nên ưu tiên thêm nhiều màn, nhiều mô-đun hoặc shop trước hai đợt trên.

## Quyết định đề xuất

Giữ 0.11.0 làm bản thử nghiệm. Giao Sol làm **0.11.1 theo các phát hiện P1/P2 và feedback mô-đun**, rồi chơi thử/cân bằng. Nền hiện tại đủ tốt để tiếp tục; việc cần nhất là khép các trường hợp biên và làm cơ chế hiện có dễ hiểu, đáng lựa chọn hơn.
