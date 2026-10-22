# QA0.11.1 — Chiến đấu rõ ràng

567 PASS từ chín suite chạy trên cùng build-03 0.11.1, DLL SHA256 dad565dc48620dd2e837e18ee18fe12cc5e1746ea4c0c4ecfc14e2fe8e67ae53. Mọi source C# khớp release-source-audit.json. Không cộng kết quả build chẩn đoán, sáu lượt bot hoặc các phiên bản cũ.

- playtest: 107 PASS, 60.0 FPS. Report: `outputs\Combat-Clarity\Playtest-Release\playtest-report.json`.
- campaign: 218 PASS, 60.0 FPS. Report: `outputs\Combat-Clarity\Campaign-Release\campaign-report.json`.
- vitals: 71 PASS, 60.0 FPS. Report: `outputs\Combat-Clarity\Vitals-Release\vitals-report.json`.
- rating: 37 PASS, 60.1 FPS. Report: `outputs\Combat-Clarity\Rating-Release\rating-report.json`.
- escape: 34 PASS, 60.0 FPS. Report: `outputs\Combat-Clarity\Escape-Release\escape-report.json`.
- tactical: 75 PASS, 59.4 FPS. Report: `outputs\Combat-Clarity\Tactical-Release\tactical-report.json`.
- clarity: 17 PASS, 59.2 FPS. Report: `outputs\Combat-Clarity\Clarity-Release\clarity-report.json`.
- input: 7 PASS, 59.9 FPS. Report: `outputs\Combat-Clarity\Input-Release\input-report.json`.
- stress: 1 PASS, 60.0 FPS. Report: `outputs\Combat-Clarity\Stress-Release\stress-report.json`.

Đỉnh lấy mẫu theo frame: tactical 79 projectile / 4 địch / 11 vật thể world. Đầy pool512 còn có probe riêng kiểm không tiêu ammo/buff.

Frame time toàn bộ sau warm-up1,5s (không bỏ frame chậm):
- playtest: p95=16.68ms; p99=16.68ms; max=33.32ms; >100ms=0; foreground FPS=60.01; background frames=0.
- campaign: p95=16.68ms; p99=16.68ms; max=16.69ms; >100ms=0; foreground FPS=60.02; background frames=11.
- vitals: p95=16.68ms; p99=16.68ms; max=16.68ms; >100ms=0; foreground FPS=60.02; background frames=0.
- rating: p95=16.68ms; p99=16.68ms; max=16.68ms; >100ms=0; foreground FPS=60.06; background frames=0.
- escape: p95=16.68ms; p99=16.68ms; max=100.00ms; >100ms=1; foreground FPS=59.97; background frames=0.
- tactical: p95=16.68ms; p99=16.68ms; max=121.30ms; >100ms=1; foreground FPS=59.45; background frames=0.
- clarity: p95=16.68ms; p99=16.68ms; max=90.50ms; >100ms=0; foreground FPS=59.16; background frames=0.
- input: p95=16.68ms; p99=16.68ms; max=33.36ms; >100ms=0; foreground FPS=59.97; background frames=9.
- stress: p95=16.68ms; p99=16.68ms; max=16.68ms; >100ms=0; foreground FPS=60.02; background frames=0.

GC được ghi theo thế hệ và peak managed memory; API allocation main-thread của Mono trên máy này trả0 nên ghi allocationSupported=false / bytes=-1, không coi là không cấp phát.

Tổng 567 kiểm tra thực tế. Không cộng kết quả các phiên bản cũ.

- QA dùng profile riêng. Harness hồi quy tự chọn thẻ theo cờ test; bản chơi thường và mở đủ tính năng vẫn do người chơi chọn.
- Escape có lượt chạy tự nhiên toàn tuyến với bảo vệ tàu và không bắn; kết quả chưa xác nhận cân bằng chơi tay.
- Module được kiểm qua bắn, va chạm, damage và hút quà thật bằng fixture kiểm soát. Campaign dùng hook tăng tốc mục tiêu/boss.
- Lượt Ion cơ bản dùng di chuyển/bắn thật trong cửa sổ phản công; kiểm tra súng/kiểu/chỉ số/module vẫn ở mức đầu. FPS áp dụng cho máy và điều kiện harness hiện tại.
- Clarity kiểm terminal state/laser/UI và né kéo-laser bằng Update đầy đủ, không bảo vệ tàu, lướt đang hồi. Input dùng chuột/phím thật trên Player, gồm bấm đúp và P/Esc.
- Stress có bảo vệ tàu: 3 boss +25 tàu, súng tối đa; ghi đỉnh theo frame 142 projectile /28 địch /1 world. Sau warm-up: 673 frame, 60,02FPS, p99 16,68ms, max 16,68ms, không frame >33ms. Chỉ áp dụng máy/cảnh đo hiện tại.
- Sáu lượt bot màn4 không bất tử đều thua trước phản công; không cạn đạn và không kịp dùng Q. Không dùng kết quả này để kết luận tác dụng Q hay cân bằng hull. Chưa xác nhận cân bằng chơi tay 13 màn; xem BALANCE-NOTES.md.
- Lượt UI thường có quan sát/input chậm và thua sớm; chỉ là xác nhận luồng thao tác, không phải dữ liệu người chơi. Thông số HP/damage/spawn/module của0.11.0 được giữ trong0.11.1.

ZIP được kiểm CRC và SHA256 mọi file. Không tự push GitHub.
