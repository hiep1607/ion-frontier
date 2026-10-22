# ION FRONTIER 0.11.1 — Chiến đấu rõ ràng

Project chính: `D:\Projects\IonFrontier`. Bản0.11.0 giữ tại `Builds/Tactical-Choices` làm bản thử nghiệm. Backup trước sửa: `LOCAL-NOTES/Before-Combat-Clarity-0.11.1`.

## Thay đổi theo review

### Kết quả trận được chốt một lần

`runResolved` bảo vệ kết quả. Vòng cập nhật dừng sau bước có thể làm tàu chết; projectile, world damage và callback boss không tiếp tục thay đổi kết quả đã chốt. Test tái hiện chính tình huống review: tàu còn5HP nhận10damage, viên đạn đang gần boss1HP; sau cập nhật đạn, callback boss và chuỗi wreck vẫn giữ Defeat,0HP,không mở màn/ghi sao.

### Laser gắn với nguồn

Beam của tàu lưu tham chiếu nguồn và serial. Hạ/rút nguồn hủy đúng laser đó, cả lúc nạp và đang bắn. Tái sử dụng slot hoặc serial không hợp lệ cũng dọn beam cũ. Laser môi trường không gắn tàu vẫn hoạt động; hạ một nguồn không ClearBeams toàn cục. Áp dụng cả Lăng Kính thường, đội kéo/laser và boss.

Damage laser được tính đúng phần thời gian đang khai hỏa sau1,15s cảnh báo, ở mức40HP/s. Tắt laser theo nguồn cũng áp dụng cho beam đang bắn vì đó là hỏa lực duy trì từ tàu; đạn đã rời nòng vẫn bay theo luật cũ.

### Mô-đun dễ đọc hơn

- Chín icon vector riêng, cùng cách dựng UI với sao hiện có, không cần bitmap mới.
- Hai ô mô-đun ở hai góc dưới, nền trong để giữ khoảng nhìn giữa màn. Hiện số loạt tích lũy của Sét/Xung, số kill của Dư chấn/Thu hồi, ba loạt sau lướt, thời gian buff và cooldown giáp.
- Khi thiếu kiểu bắn, hiện rõ cần Tỏa/Xuyên/Truy đuổi.
- Thẻ súng xem trước sát thương, nhịp và số viên trước–sau; báo mô-đun sẽ ngừng nếu kiểu bị thay. Preview dùng bản sao loadout, không sửa trạng thái thật.
- Mảnh đạn ngắn/nhỏ hơn, màu hồng và halo riêng, vẫn giữ damage/cap/nguồn như0.11.0.
- Bộ đếm tiếp tục sau chọn dùng thời gian UI thật0,5s, không kéo dài theo cap thời gian mô phỏng khi FPS thấp.

Giữ13màn/8tàu,9mô-đun,6đội hình,hai lựa chọn/màn,profilev4,mốc mở tàu,năm sao,loot15%,ammo và damage cơ bản. Đợt này tập trung sửa luật và phản hồi; kết luận cân bằng dựa trên các lượt thử được ghi riêng.

## QA và phép đo

Kết quả cuối đọc tại `TEST-REPORT.json` và `outputs/Combat-Clarity/QA-NOTES.md`, chỉ tính các report thực sự chạy. Source/DLL được khóa theo `release-source-audit.json`; build log `build-03.log`.

- Clarity: terminal state, damage world sau thua, laser nguồn chết/nạp/bắn/rút/slot reuse, không xóa beam khác, damage theo thời gian, callback ba thẻ, pause sau nhận thẻ, resume không bắn nhầm, preview và HUD counters.
- Input riêng: chuột cả ba thẻ, bấm đúp, phím1/2/3,P/Esc trên Player qua computer-use. Không chỉ gọi API chọn thẻ.
- Tránh laser: vòng Update đầy đủ, không bảo vệ tàu, di chuyển thường ra khỏi làn đã khóa khi lướt đang hồi.
- Stress: ba boss và25tàu thường với súng tối đa, Tỏa/Xuyên + Nòng hội tụ/Phân mảnh, chạy12s theo Update thật. Đây là fixture hiệu năng vượt mật độ thông thường, có bảo vệ tàu và không dùng để kết luận độ khó.
- Hồi quy: opening,campaign,vitals,rating,escape,tactical.

`IonFrameMetrics` ghi mọi frame dương sau warm-up1,5s, gồm p50/p95/p99/max, số frame>33/50/100ms, đỉnh pool và GC từng thế hệ. Có cả thống kê toàn bộ và foreground; không bỏ frame background khỏi thống kê toàn bộ. Screenshot/resizes cũng nằm trong số liệu. Player ở background có thể bị giới hạn mạnh trên máy này nên có cờ QA chờ focus trước khi bắt đầu bài test.

Mono trên máy này trả0 cho API allocation main-thread; báo cáo đánh dấu `allocationSupported=false`,bytes=-1. Có peak managed memory và GC counts; không suy diễn số0 là không cấp phát.

## Chơi thử/cân bằng

`Balance-Release/balance-report.json` ghi sáu lượt bot không bất tử màn4: hull0/1/2, có/không cấp một Q đầu lượt. Bot lái theo nguy cơ trong sân, bắn và nhận hai lựa chọn thật; không tăng tốc tuyến/HP/kill. Chỉ giữ bảo vệ2s đầu màn vốn có. Đây là khảo sát sơ bộ trên các hull, không thay cho dữ liệu người chơi đủ lớn hoặc xác nhận cân bằng13màn.

Mỗi lượt ghi distance,time,HP,DamageTaken,nguồn damage,ammo,đại chiêu được cấp và elite/boss arrival. HP lúc tới boss có tiếp tế/heal trước boss theo luật sẵn có. Một lượt thao tác gameplay qua UI được ghi tách khỏi bot; xem báo cáo chơi thử nếu có. Các số liệu quan sát không được cộng thành PASS chức năng.

## Bản chơi và nguồn

- Build: `Builds/Combat-Clarity/IonFrontier.exe`.
- Launcher thường: `Choi-game.cmd`; mở13màn/8tàu bằng save tạm: `Choi-thu-mo-tat-ca.cmd`.
- ZIP: `outputs/IonFrontier-Chien-Dau-Ro-Rang-0.11.1-Windows.zip`, kiểmCRC/hash từngfile.
- Các file chính: IonGame, IonBossWrecks, IonEncounters, IonExpandedBosses, IonHostileFleet, IonCombatModules, IonModuleIcon, IonDraftHud, IonUpgradeDraft, IonLoadout, IonCombatTelemetry, IonFrameMetrics và các harness Clarity/Balance.

Không push GitHub trong đợt này. Đặc tả tiếp theo nên dựa vào số liệu chơi thử và ưu tiên những lựa chọn người chơi ít hiểu/ít dùng.

## Nghiệm thu cuối 22/10/2026

**567 PASS trên cùng build-03:** 107 mở đầu +218 chiến dịch +71 vitals +37 rating +34 escape +75 tactical +17 clarity +7 input vật lý +1 stress. Không runtime error trong chín report. Các báo cáo chẩn đoán trước đó lưu để truy vết, không được cộng. Source/exe/DLL khớp audit lúc đóng gói.

Stress3boss+25tàu: 673frame sau warm-up,60,02FPS,p95/p99/max~16,68ms,0frame>33ms,peak142projectile/28enemies/1world,peakmanaged~9,43MB. Đây là fixture có bảo vệ tàu, không phải xác nhận độ khó hoặc hiệu năng mọi màn.

Sáu lượt bot không bất tử thua ở22,44–38,53s, trước phản công; Q được cấp chưa kịp dùng. Không cạn đạn. Lượt UI thường cũng thua sớm với thao tác chậm qua công cụ. **Giữ thông số trong0.11.1; chưa xác nhận cân bằng chơi tay.** Số liệu, giới hạn và phương án thử tiếp ở `outputs/Combat-Clarity/BALANCE-NOTES.md`.
