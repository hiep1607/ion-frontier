# Ion Frontier Next 0.12.1 — Kết quả và nhận tàu

Project làm việc: `D:\Projects\IonFrontier-Next`. Nhánh local `feature/cinematic-menu-0.12.0`. Bản gốc D:\Projects\IonFrontier giữ nguyên. Đợt này không push GitHub.

## Đã sửa

Màn kết quả cũ dùng ảnh tàu thưởng thay ảnh tàu vừa điều khiển, đồng thời vẫn viết nhận tàu khi chơi lại. Bản mới chốt RunHull lúc xuất kích và LastCompletedHull khi kết thúc; chân dung/tên tàu trên cả thắng/thua lấy đúng tàu đã dùng. Tàu mới dùng khung nhận thưởng riêng.

LastNewHull được xác định từ số tàu mở trước/sau lần thắng. Popup chỉ xuất hiện tại lần đầu qua mốc1/2/5/7/9/11/13; màn khác, thua, chơi lại hoặc chế độ đã mở tất cả không báo thưởng giả. Lưu tiến trình/sao trước khi chạy hiệu ứng. Nhấn bỏ qua hay thoát popup không làm mất phần thưởng.

Sao hiện theo năm tiêu chí, bắt đầu sau0,35giây, cách nhau0,42giây; sao đạt nhún nhẹ, phát âm tiếp tế với cao độ tăng, điểm đếm lên. Chỉ thắp đúng số sao đạt, không nháy thành năm sao giả. Có Hiện kết quả ngay/Enter; Refresh âm thanh không chạy lại chuỗi. Toàn chuỗi khoảng2,5giây.

Sau chuỗi sao, tàu mới sáng dần từ bóng tối trong0,8giây với quầng sáng mở rộng. Nút xác nhận chờ0,85giây để tránh click/Enter bỏ qua đi xuyên popup. Giữ tàu hiện tại hoặc Dùng từ chuyến sau là lựa chọn rõ ràng. Màn kết quả vẫn giữ tàu vừa lái; lựa chọn tàu mới chỉ áp dụng khi về căn cứ hoặc bắt đầu chuyến tiếp. Bỏ hành vi tự đổi sang tàu mở khóa mới nhất khi bấm màn tiếp.

Giảm chuyển động tắt nhún/zoom; âm thanh tôn trọng M/tùy chọn của game. UI co theo cửa sổ, có kiểm1280×720,1024×768,1680×720. Không đổi luật sao, mốc mở khóa, HP/damage, loot hay file lưu v4.

## Tham khảo

- [Angry Birds Space — ví dụ màn sao/kết quả](https://www.angrybirdsnest.com/angry-birds-space-solar-system-level-10-5-walkthrough/): tham khảo phân cấp sao, điểm và các nút đi tiếp.
- [Brawl Stars — phần thưởng và nhân vật mở khóa](https://support.supercell.com/brawl-stars/en/articles/starr-drops-5.html): tham khảo cách dành một khoảnh khắc riêng cho phần thưởng mới.

Đây là hướng thiết kế rút ra để áp dụng cho Ion Frontier; thời lượng/easing và bố cục là phương án riêng, không sao chép artwork/âm thanh của các game tham khảo.

## Kiểm chứng bản này

315PASS =59result+37rating+108regression+111menu, cùng DLL cuối. Report gốc ở outputs/Result-Rewards và bản chụp trong Design/QA-0.12.1. 59result gồm một luồng boss chết/chờ wreck thật để nhận tàu đầu tiên và các fixture riêng kiểm8tàu,7mốc thưởng,6màn không thưởng,1/4/5sao,thua,bỏ qua,giảm chuyển động,khóa modal,lưu vàbố cục. Fixture hạ boss/đặt điều kiện giúp kiểm giao diện, không tuyên bố chơi tay hay cân bằng toàn chiến dịch.

Đã xem ảnh thật từ UI Unity: sao đang tăng, năm sao cuối, nhận Lôi Điểu, kết quả4:3 và trang nhận thưởng. Capture đổi Canvas sang camera-mode trong lúc chụp, đặt sorting phía trên SpriteRenderer để khớp thứ tự ScreenSpaceOverlay rồi khôi phục. Không sửa pixel hoặc artwork. Kiểm chuột/phím ở cấp hệ điều hành chưa nghiệm thu; kiểm tương tác dùng callback/submit/raycast của Unity theo các suite.

Kiểm hồi quy cũ được đổi để xác nhận giữ tàu đang dùng và chọn tàu mới rõ ràng. Đã thêm reset loadout vào fixture nhặt kiểu bắn để loại ảnh hưởng loot ngẫu nhiên của đoạn combat trước; không sửa quy tắc kiểu bắn trong game. Không gộp kết quả0.12.0 hoặc567check0.11.1 vào315.

## Chạy

Hai launcher gốc Next ưu tiên Builds/Result-Rewards. Build0.12.0 ở Cinematic-Menu và các build cũ vẫn giữ nguyên. Đóng cửa sổ cũ và mở lại Choi-game.cmd. Choi-thu-mo-tat-ca.cmd đã mở sẵn tất cả nên không diễn lại thưởng lần đầu.

Backup source trước sửa: LOCAL-NOTES/Before-Result-Rewards-0.12.1. Giữ repo private, không tự push sang repo cũ public.
