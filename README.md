# ION FRONTIER

Game bắn phi thuyền 2D bằng Unity, giao diện tiếng Việt.

## Bản khởi đầu

Commit đầu tiên lưu một phần nền tảng của project:

- 5 script C# cho gameplay, giao diện, hình ảnh/âm thanh, nền không gian và kiểm tra thủ công qua cờ chạy.
- Manifest và lockfile của package Unity.
- Phiên bản Unity, cấu hình Git và các file meta tương ứng.

Scene, tài nguyên đồ họa, font, thiết lập project đầy đủ và công cụ build sẽ được bổ sung sau. Bản này chưa phải gói game hoàn chỉnh để mở và chơi ngay.

## Môi trường

- Unity 6.6 — `6000.6.2f1`.
- C# và Unity UI (uGUI).
- Mục tiêu hiện tại: Windows 64-bit.

## Source hiện có

| File | Vai trò |
| --- | --- |
| `Assets/IonFrontier/Scripts/IonGame.cs` | Trạng thái game, điều khiển, đạn, đội hình, boss, điểm số và pool |
| `Assets/IonFrontier/Scripts/IonHud.cs` | Menu, HUD và giao diện tiếng Việt |
| `Assets/IonFrontier/Scripts/IonArt.cs` | Tạo sprite từ atlas, hiệu ứng và âm thanh tổng hợp |
| `Assets/IonFrontier/Scripts/IonBackdrop.cs` | Nền vũ trụ và sao parallax |
| `Assets/IonFrontier/Scripts/IonPlaytest.cs` | Harness chỉ bật khi chạy với `-ion-playtest` |

## Hướng phát triển

Bộ hình mới dự kiến gồm 3 tàu người chơi, 7 tàu địch thường và 3 boss. Các hình và cơ chế tương ứng chưa nằm trong bản khởi đầu này.
