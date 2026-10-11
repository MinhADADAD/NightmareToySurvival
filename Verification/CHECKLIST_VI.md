# Checklist kiểm tra trong Unity 2018.4

Tất cả ô bên dưới ban đầu để trống vì chưa có lượt Play Mode thực tế.

- [ ] Console sạch lỗi compile, Missing Script và NullReferenceException sau import/setup.
- [ ] Main Menu hiển thị đúng tiếng Việt; Chơi/Hướng dẫn/Quay lại/Thoát hoạt động.
- [ ] WASD và phím mũi tên hoạt động; chạy chéo không nhanh hơn chạy thẳng.
- [ ] Player không xuyên đồ vật/tường và không rơi khỏi bản đồ.
- [ ] Ngắm chuột cả bốn hướng; thử khi đứng sát đồ vật; camera theo mượt.
- [ ] Giữ chuột trái bắn đúng nhịp; tia đạn, đầu nòng và tiếng súng đồng bộ.
- [ ] Đứng đối diện quái sau vật cản: viên đạn dừng ở collider, quái không mất máu.
- [ ] Sát tường, đưa nòng súng xuyên tường: không bắn trúng phía bên kia.
- [ ] Trúng quái có hit particle/âm thanh; bắn xác không cộng điểm lần nữa.
- [ ] Cả ba loại quái tìm đường quanh đồ vật, không đi xuyên vật cản hoặc mắc vào điểm sinh.
- [ ] Quái xuất hiện cách Player ít nhất 10 đơn vị và không chồng collider.
- [ ] Mỗi quái chỉ đánh theo hồi chiêu, không đánh mỗi frame hoặc xuyên tường.
- [ ] Player bắt đầu 100 máu; HUD giảm đúng, có flash/âm thanh; chết khi về 0.
- [ ] Quái chết dừng di chuyển/đánh, phát death animation nếu có, rồi chìm và bị hủy.
- [ ] Animation đi/đứng/chết khớp vận động; kiểm tra riêng ZomBear và Console cảnh báo clip.
- [ ] Khi còn quái chưa sinh, bộ đếm không về 0 sớm. Dọn sạch mới đổi đợt.
- [ ] Nghỉ đúng 5 giây giữa các đợt, có đếm ngược và hồi 15 máu không vượt 100.
- [ ] Đợt 10 có đúng một boss lớn hơn, thanh máu boss phản ánh trúng đạn.
- [ ] Boss chết mà còn quái khác thì chưa thắng; dọn sạch đợt 10 mới thắng.
- [ ] Thua/thắng hiển thị điểm, kỷ lục, Chơi lại và Về menu.
- [ ] Esc ngừng di chuyển, sát thương, sinh quái và đếm ngược; UI vẫn bấm được.
- [ ] Tiếp tục phục hồi thời gian; mất focus tự tạm dừng.
- [ ] Chơi lại từ pause/thua/thắng đặt lại 100 máu, 0 điểm, đợt đầu, quái cũ và Time.timeScale=1.
- [ ] Về menu rồi Chơi không để lại quái/coroutine/NavMesh của lượt trước.
- [ ] Nhạc và âm lượng hiệu ứng điều chỉnh độc lập, giá trị giữ sau reload/thoát.
- [ ] Điểm cao giữ sau khi đóng rồi mở game lại.
- [ ] UI ở 1280×720, 1920×1080 và 1024×768 không bị cắt/chồng; vùng giữa còn đủ để chiến đấu.
- [ ] Chạy bài thử Play Mode và đọc Verification/playmode-smoke.txt; không chỉ dựa vào việc Editor tự dừng.
- [ ] Build Windows x64 thành công; chạy EXE và lặp lại thao tác chơi/tạm dừng/chơi lại.

Ghi thêm: Unity version, độ phân giải, FPS, lỗi Console, bước tái hiện và ảnh chụp nếu có.
