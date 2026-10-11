# Nightmare Toy Survival — hướng dẫn bàn giao

Bản sao project đã có mã game và công cụ dựng toàn bộ màn chơi. **Chưa có scene/prefab game đã được Unity sinh ra, chưa chạy Play Mode và chưa có bản Windows đã build.** Máy thực hiện chỉ có Unity 2022.3.55f1 và 6000.6.0f1; để giữ phiên bản gốc, project chưa được mở bằng hai bản này.

## Mở và chạy lần đầu

1. Cài **Unity 2018.4.17f1** từ [trang phát hành chính thức](https://unity.com/releases/editor/whats-new/2018.4.17f1). Dùng Windows Editor; cần license Unity hợp lệ để chạy Editor/batch mode.
2. Trong Unity Hub, Add project và chọn **thư mục chứa file README này**, không chọn thư mục project gốc trong Downloads. Chọn đúng 2018.4.17f1 để mở. Không chấp nhận nâng cấp bằng 2022/Unity 6.
3. Đợi import asset/package và biên dịch xong. Mở Console. Nếu có lỗi đỏ, xử lý trước khi tiếp tục; giữ nguyên thông báo để chẩn đoán, không tự nâng package/pipeline.
4. Chọn menu **Nightmare → 1. Create playable game**. Công cụ thực hiện:
   - Tạo scene `Assets/NightmareGenerated/NightmareToySurvival.unity`.
   - Dựng phòng bằng Environment.prefab, ánh sáng gốc và đèn bổ sung.
   - Tạo floor collider, biên đấu trường, layer và NavMesh từ collider.
   - Tạo Player, Zombunny, ZomBear, Hellephant, HellephantBoss prefab; gán script, collider, Rigidbody, NavMeshAgent, clip và âm thanh.
   - Tạo Animator Controller và bản sao animation clip, bỏ event tự khởi động lại màn chơi.
   - Kiểm tra điểm sinh nằm trên NavMesh, không đè vật cản và có đường tới vị trí xuất phát.
   - Tạo Canvas, menu, HUD, nút, thanh âm lượng, EventSystem và tham chiếu.
   - Lưu scene, đưa scene vào Build Settings và kiểm tra tham chiếu.
5. Chọn **Nightmare → 2. Validate scene**. Console phải báo `Scene reference validation PASS`.
6. Nhấn nút **Play** của Unity, sau đó nhấn **CHƠI** trong game. Có 5 giây chuẩn bị trước đợt đầu.

Những lần mở sau chỉ cần mở scene trên và Play. Chạy lại mục 1 khi scene đã tồn tại sẽ mở scene cũ, không ghi đè các chỉnh sửa Inspector.

Nếu cần chạy công cụ qua dòng lệnh, từ PowerShell dùng đường dẫn Unity 2018 đã cài:

```powershell
& 'C:\duong-dan-Unity-2018\Editor\Unity.exe' -batchmode -quit -projectPath 'C:\duong-dan\NightmareToySurvival' -executeMethod NightmareSetup.CreateGame -logFile 'C:\duong-dan\NightmareToySurvival\setup.log'
```

## Cách chơi

- WASD hoặc phím mũi tên: di chuyển, tốc độ đường chéo được chuẩn hóa.
- Chuột: hướng ngắm trên mặt sàn. Giữ chuột trái để bắn liên tục.
- Esc: tạm dừng/tiếp tục. Khi mất focus cửa sổ, game tự tạm dừng.
- Bắn xuyên qua khoảng trống; collider của tường/đồ vật chặn raycast. Đầu súng chọc qua tường cũng không được bắn xuyên tường.
- Quái chỉ tấn công khi đủ gần, không bị vật cản ngăn và hết thời gian hồi chiêu.
- Hạ hết quái của đợt hiện tại để nghỉ 5 giây, hồi 15 máu (tối đa 100), rồi sang đợt kế tiếp.
- Đợt 10 có boss Hellephant lớn hơn và thanh máu riêng. Chỉ thắng khi boss **và toàn bộ quái còn lại** đã chết.
- Menu và màn hình tạm dừng có thanh âm lượng nhạc nền/hiệu ứng. Điểm cao lưu bằng PlayerPrefs, tồn tại sau khi chơi lại.

## Cân bằng và chỉnh trong Inspector

Các chỉ số sau là giá trị nền. Máu quái mỗi đợt nhân với `1 + (số đợt - 1) × 0.07`.

| Đối tượng | Máu nền | Tốc độ | Sát thương | Hồi chiêu đánh | Điểm |
|---|---:|---:|---:|---:|---:|
| Player | 100 | 6 | 25 mỗi viên | Bắn mỗi 0,15 giây | — |
| Zombunny | 50 | 4,5 | 8 | 1 giây | 10 |
| ZomBear | 100 | 3,1 | 12 | 1 giây | 20 |
| Hellephant | 220 | 2 | 22 | 1 giây | 40 |
| HellephantBoss | 1.200 | 2,6 | 30 | 1,4 giây | 500 |

Boss đợt 10 có 1.956 máu sau hệ số đợt. Chiều cao mô hình được chuẩn hóa lần lượt khoảng 1,85 / 1,5 / 1,9 / 2,5 / 3,6 đơn vị. Đây là thông số thiết kế mới, không phải hành vi có sẵn của asset.

| Đợt | Zombunny | ZomBear | Hellephant | Boss |
|---|---:|---:|---:|---:|
| 1 | 5 | 0 | 0 | 0 |
| 2 | 6 | 2 | 0 | 0 |
| 3 | 7 | 3 | 0 | 0 |
| 4 | 6 | 4 | 1 | 0 |
| 5 | 8 | 4 | 2 | 0 |
| 6 | 8 | 5 | 3 | 0 |
| 7 | 10 | 6 | 3 | 0 |
| 8 | 10 | 7 | 4 | 0 |
| 9 | 12 | 8 | 4 | 0 |
| 10 | 10 | 8 | 5 | 1 |

- **Game Systems → EnemyManager**: số lượng từng đợt, thời gian nghỉ, khoảng cách sinh tối thiểu (10), hồi máu, hệ số máu.
- **Prefab quái → EnemyHealth / EnemyMovement / EnemyAttack**: máu, điểm, tốc độ, sát thương, hồi chiêu, tầm đánh.
- **Player → PlayerMovement / PlayerHealth**: tốc độ, máu, thời gian miễn sát thương ngắn (0,35 giây).
- **Player → Gun Muzzle → PlayerShooting**: sát thương, nhịp bắn, tầm bắn.
- **Main Camera → CameraFollow**: offset và độ mượt.
- **Game UI**: mọi thành phần UI được lưu trong scene, có thể xem/sửa bằng RectTransform. CanvasScaler lấy mốc 1280×720.

Khi đổi kích thước collider quái, phải bảo đảm bán kính NavMesh đã bake đủ lớn. Hiện NavMesh bake radius 1, lớn hơn collider boss 0,85; điều này ưu tiên đường rộng cho boss.

## Kiến trúc để học và mở rộng

`PlayerMovement` đọc input và di chuyển Rigidbody; `PlayerShooting` quyết định viên đạn chạm collider nào; `PlayerHealth` quản lý máu và báo thua. `EnemyMovement` chỉ tìm đường, `EnemyAttack` chỉ tính điều kiện đánh/hồi chiêu, `EnemyHealth` chỉ xử lý máu/chết và thông báo một lần.

`EnemyManager` quản lý hàng đợi sinh quái và tập quái còn sống. Bộ đếm còn lại gồm **quái chưa sinh + quái còn sống**, tránh chuyển đợt sớm. Mỗi lần sinh đều kiểm tra khoảng cách, collider, NavMesh và đường đi hoàn chỉnh. Không tìm được điểm an toàn thì chờ, hiển thị thông báo và thử lại, không sinh vào tường.

`GameManager` quản lý Menu / Playing / Paused / Won / Lost. Tất cả hành vi chiến đấu kiểm tra trạng thái trước khi thực hiện. Khi chơi lại hoặc về menu, scene được tải mới và Time.timeScale trở về 1; prefab mới khởi tạo lại máu, điểm, quái và coroutine. `ScoreManager`, `AudioManager`, `GameUI`, `GameOverManager` có trách nhiệm riêng.

`ArenaNavigation` nạp dữ liệu NavMesh đã bake khi scene chạy, tháo dữ liệu khi scene bị hủy. Không cần cài AI Navigation/NavMeshComponents ngoài project.

Player, Zombunny và Hellephant có các clip Move/Idle/Death trong metadata. Không thấy clip Hurt/Attack được khai báo: phản hồi bị thương dùng flash HUD và âm thanh. ZomBear không có clip được chia trong importer; công cụ chỉ dùng lại clip Zombunny nếu tất cả đường dẫn xương khớp. Nếu không tương thích, Console cảnh báo; gameplay vẫn chạy nhưng animation ZomBear cần bổ sung. Chưa kiểm chứng việc import clip thực tế trong Unity 2018.

## Việc cần kiểm tra trong Editor

**Chưa đánh dấu bất kỳ mục Play Mode nào là đã đạt.** Chúng cần chạy trên bản 2018.4.

1. Chạy **Nightmare → 4. Run Play Mode smoke test** khi đang dừng Play. Công cụ mở scene, vào Play, tăng tốc các đợt trong bộ nhớ để kiểm tra và tự thoát Play. Kết quả ghi `Verification/playmode-smoke.txt`; sửa đổi thông số thử không được lưu vào scene. Điểm cao trước bài thử được khôi phục khi bài thử kết thúc bình thường.
2. Bài thử kiểm tra: trạng thái đầu, raycast vào vật cản, đường đi từ các điểm sinh, agent trên NavMesh, một lần cộng điểm/giảm bộ đếm, quái chết dừng hoạt động, nhận sát thương, pause/resume, thua, chơi lại, 10 đợt/boss/thắng, chơi lại khi pause và về menu.
3. Tiếp tục checklist thủ công trong `Verification/CHECKLIST_VI.md`. Bài thử tự động không mô phỏng WASD/chuột và không kiểm tra hình ảnh/âm thanh.
4. Sau khi sửa vị trí vật cản: đặt collider mới dưới object **Toy Room - original Environment**, layer **NightmareWorld**, rồi chọn **Nightmare → 5. Rebake navigation after editing arena**. Công cụ cũng tạo lại các điểm sinh và chọn lại vị trí xuất phát. Không sửa trong Play Mode nếu muốn lưu.

Nếu mục 1 dựng game báo lỗi giữa chừng, đọc Console. Một số asset sinh có thể đã tồn tại; không xóa Assets gốc. Sửa nguyên nhân rồi chạy lại. Nếu scene đã được lưu nhưng Validate báo thiếu tham chiếu, sửa đúng field trong Inspector theo tên lỗi rồi lưu scene.

## Build Windows

1. Mở scene game, chạy Validate và checklist trước.
2. Trong File → Build Settings, chọn **PC, Mac & Linux Standalone**, Target Platform **Windows**, Architecture **x86_64**. Dùng Mono cho bài tập; nếu chọn IL2CPP phải có module Windows IL2CPP/toolchain tương ứng.
3. Chọn **Nightmare → 3. Build Windows x64**. Kết quả nằm ở `Builds/Windows/NightmareToySurvival.exe` và các file đi kèm. Có thể dùng Build Settings → Build nếu muốn chọn thư mục khác.
4. Chạy EXE, thử game từ menu đến chơi lại và kiểm tra âm thanh. Khi gửi game phải gửi **cả thư mục Windows**, không chỉ EXE.

Bản bàn giao hiện tại **không chứa EXE**, không có kết quả build thành công. Xem `Verification/STATUS.md` để biết chính xác các kiểm tra đã thực hiện.

## Tài liệu và nguồn

- `CHANGELOG_VI.md`: các script sửa/thêm và tài nguyên được tái sử dụng.
- `Verification/source-audit.json`: thống kê so với project gốc, hash phiên bản/package và tham chiếu tài nguyên.
- `Reference/LegacyAssets`: giữ profile hậu kỳ cũ bị thiếu script bên ngoài thư mục Assets; không dùng trong game mới và không thay render pipeline.
- [NavMeshBuilder.BuildNavMeshData — Unity 2018.4](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/AI.NavMeshBuilder.BuildNavMeshData.html).
- [PrefabUtility.SaveAsPrefabAsset — Unity 2018.4](https://docs.unity3d.com/2018.4/Documentation/ScriptReference/PrefabUtility.SaveAsPrefabAsset.html).

Các thông báo bản quyền tài nguyên và FontLicense của project gốc được giữ lại.
