# ION FRONTIER 0.11.0 — bản triển khai

Project: `D:\Projects\IonFrontier`. Dùng source local 0.10.0; backup trước sửa ở `LOCAL-NOTES/Before-Tactical-Choices-0.11.0`. Không đồng bộ bản C: và không push GitHub.

## Ba hướng đã tích hợp

| Hướng | Hành vi trong bản chơi | Source chính |
| --- | --- | --- |
| Chọn nâng cấp | Hai checkpoint mỗi màn, ba thẻ khác nhau, nhận một thẻ. Thẻ súng, mô-đun phù hợp hoặc hỗ trợ theo HP/đạn; có fallback khi đạt giới hạn. Dừng toàn bộ mô phỏng, hỗ trợ pause/resume và chống nhận hai lần. | IonUpgradeDraft, IonDraftHud, IonCombatModules |
| Đội hình phối hợp | Sáu loại đội thay một phần lượt spawn cũ. Phá tàu nguồn để mất khiên/sửa/kéo/gọi viện. Giữ ngân sách, serial của slot, nhiệm vụ ưu tiên và khoảng nghỉ giữa đội. | IonEncounters, IonEncounterTestHooks |
| Màn 4 phản công | Giữ tuyến 3.600 m / 72 giây hoạt động. Có chặn sườn, tàu nặng vượt lên và cửa sổ 50–64 giây phá hai động cơ 18 HP, thân 42 HP. Thắng thưởng 600 điểm / 40 đạn và xác tách mảnh; bỏ qua vẫn tới boss 440 HP. | IonEscapeMission, IonCounterattack |

Thông số gameplay trong đặc tả được giữ. Không thêm shop, multiplayer hoặc hệ mở khóa mới. Mô-đun có tối đa hai ô riêng, không chiếm hai kiểu bắn; reset mỗi lượt. Loot thường vẫn 15%, profile v4, tám tàu/tám Q, mốc mở tàu và năm sao giữ nguyên.

## Những điểm tích hợp đã xử lý

- Damage, góc tỏa, xuyên và quyền kích hoạt mô-đun được chốt theo viên đạn lúc bắn. Đạn phụ không sinh chuỗi proc. Một loạt có `volley` riêng, sét đếm một lần; mảnh tối đa bốn và bỏ qua mục tiêu/động cơ nguồn.
- Pool đầy hoặc hết đạn không tiêu ammo, buff lướt hoặc nhịp Buồng xung điện. Bộ đếm hạ địch của mô-đun không lấy các kill trước khi nhận mô-đun.
- Nòng hội tụ và Đạn phân mảnh chỉ áp hệ số một lần. Nam châm tăng phạm vi 60% và nhân tốc độ hút gốc 1,2; không tăng xác suất rơi.
- Giảm damage của đội khiên không cộng chồng guard thường. Thợ máy của workshop không chạy thêm nhịp sửa cũ. Mìn màn 10 trước boss được chặn ở nguồn spawn chung để mọi loại tàu cùng tuân cap 12.
- Elite không là boss, không gọi Finish, không ghi sao/mở khóa và không khởi động boss clock. Engine/Q/AoE dùng cùng cổng giáp; thân nhận mỗi hit một lần. Replay/menu/chuyển boss dọn weakpoint, cảnh báo, mảnh và slot đang cháy.
- Modal dùng `UpgradeChoice`, không dùng Time.timeScale. Đầu vào chiến đấu bị chặn; chọn một thẻ rồi chờ 0,5 giây và nhả chuột. Background và các clock chiến đấu được đóng băng.

## Kiểm tra và nguồn bản build

Báo cáo chính thức là `TEST-REPORT.json`; số kiểm tra lấy từ report thực tế, không cộng kết quả lịch sử. Chi tiết giới hạn nằm trong `outputs/Tactical-Choices/QA-NOTES.md`.

- Năm harness hồi quy chạy trên build-06: opening 107, campaign 218, vitals 71, rating 37, escape 34. Các harness đã xử lý draft qua chế độ QA rõ ràng và kiểm đủ hai lựa chọn khi qua màn.
- Build-08 giữ nguyên mọi file gameplay của build-06, chỉ bổ sung kiểm tra Q, giữ súng Ion cơ bản không vô tình nhận thẻ checkpoint và cải thiện fixture chụp combo ở hai file TacticalTest. `qa-only-build-difference.json` xác minh bằng SHA256. Tactical-Release-Final chạy lại trên build-08.
- `build-source-audit.json` lưu hash source/build-06; `release-source-audit.json` lưu bản phát hành. Mã băm DLL gameplay được kiểm trước khi đóng ZIP, bên cạnh hash executable.
- Các lần kiểm tra đầu có lỗi fixture: beam chưa tạo, target ở vị trí entry ngoài sân và snapshot nền trước đổi theme. Các lỗi này đã sửa trong harness; log/report thất bại được giữ để đối chiếu.

QA tự động có bảo vệ tàu và hook tăng tốc mục tiêu. Lượt truy đuổi tự nhiên chạy đủ tuyến; lượt Ion cơ bản dùng di chuyển/bắn thật trong cửa sổ phản công. Đây chưa phải kết luận cân bằng chơi tay toàn bộ chiến dịch. FPS/đỉnh pool trong báo cáo được lấy mẫu theo frame của harness trên máy hiện tại.

## Ảnh Player đã xem

Ảnh trong `outputs/Tactical-Choices/Tactical-Release-Final`: màn chọn ở 1280×720 và 1680×720, combo Tỏa/Xuyên với hai mô-đun, đội khiên/xạ thủ, kéo/laser, elite hai động cơ, một động cơ hỏng, xác elite, thành công bằng Ion và hết hạn. Ảnh bảng năm sao trong `Rating-Final/02-five-star-debrief.png`.

Các thẻ, mô tả và footer nằm gọn trong cả hai tỷ lệ. Khi phản công, thanh lớn vẫn là tiến độ tuyến; HP từng động cơ nằm ở panel mục tiêu. Khi tới boss, tiến độ được thay bằng boss bar.

## Gói phát hành

- Build: `Builds/Tactical-Choices/IonFrontier.exe`.
- Launcher lưu thật: `Choi-game.cmd`; launcher mở 13 màn / 8 tàu bằng profile tạm: `Choi-thu-mo-tat-ca.cmd`.
- ZIP: `outputs/IonFrontier-Lua-Chon-Chien-Thuat-0.11.0-Windows.zip`.
- `package-verification.json` ghi CRC, SHA256 ZIP và đối chiếu hash từng file. Build/ZIP 0.10.0 giữ riêng.
