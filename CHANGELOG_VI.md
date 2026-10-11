# Danh sách thay đổi

## Sửa các script gameplay hiện có (giữ file và GUID)

- Assets/Scripts/Player/PlayerMovement.cs — thay lớp rỗng bằng input, Rigidbody, ngắm chuột và animation.
- Assets/Scripts/Player/PlayerShooting.cs — tốc độ bắn, kiểm tra trạng thái game/UI, raycast vật cản/quái, kiểm tra đoạn thân–đầu nòng, particles/line/light/audio.
- Assets/Scripts/Player/PlayerHealth.cs — máu, chống nhận sát thương dồn trong 0,35 giây, âm thanh, chết; bỏ tự restart theo animation event.
- Assets/Scripts/Enemy/EnemyMovement.cs — NavMeshAgent, tìm lại đường, dừng theo trạng thái.
- Assets/Scripts/Enemy/EnemyAttack.cs — kiểm tra khoảng cách/tường/hồi chiêu/máu.
- Assets/Scripts/Enemy/EnemyHealth.cs — máu, xác chết, dừng agent/collider, thông báo chết một lần.
- Assets/Scripts/Managers/EnemyManager.cs — thay sinh vô hạn bằng 10 đợt, boss, kiểm tra điểm sinh, đếm quái, hồi máu giữa đợt.
- Assets/Scripts/Managers/ScoreManager.cs — điểm từng lượt và kỷ lục PlayerPrefs.
- Assets/Scripts/Managers/GameOverManager.cs — trì hoãn màn hình kết quả để thấy animation chết.

## Thêm

- Assets/Scripts/Managers/GameManager.cs — trạng thái game, pause/resume, restart/menu/quit.
- Assets/Scripts/Managers/AudioManager.cs — nhạc và hiệu ứng, hai nhóm âm lượng.
- Assets/Scripts/Managers/ArenaNavigation.cs — nạp/tháo dữ liệu NavMesh đã bake.
- Assets/Scripts/Player/CameraFollow.cs — camera góc chéo theo Player.
- Assets/Scripts/UI/GameUI.cs — tạo UI lưu vào scene và cập nhật/bind sự kiện khi chạy.
- Assets/Editor/NightmareSetup.cs — dựng scene/prefab/controller/NavMesh, validate, rebake và build Windows.
- Assets/Editor/NightmareSmokeTest.cs — bài thử tích hợp Play Mode không dùng package ngoài.
- Metadata .meta cho file/thư mục mới.
- README_VI.md, CHANGELOG_VI.md, Verification/* và .gitignore.

## Điều chỉnh tài nguyên phụ trong bản sao

- Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs: bỏ tự khởi động layout tutorial; menu đọc tài liệu vẫn còn.
- Di chuyển Assets/Profiles/SurvivalShooterProfile.asset và .meta sang Reference/LegacyAssets: profile cũ tham chiếu script hậu kỳ bị thiếu trong project nguồn, không dùng cho game mới. Không thêm Post Processing và không đổi pipeline.

## Tái sử dụng

Environment.prefab, Lights.prefab, Player/Zombunny/ZomBear/Hellephant FBX, toàn bộ material/texture liên quan, GunParticles/HitParticles, LineRenderMaterial, 9 file WAV và nhạc Background Music. Giữ nguyên thông báo bản quyền gốc. Các script cùng chức năng được sửa tại chỗ, không tạo bộ Player/Enemy thứ hai.

## Chỉ được sinh/sửa sau khi chạy công cụ Editor

- Assets/NightmareGenerated/: scene, 5 prefab, 4 controller, các clip sao chép nếu khả dụng và ArenaNavMesh.asset.
- Metadata importer 4 nhân vật: tắt Optimize Game Objects để gắn đầu nòng và kiểm tra các xương dùng animation chung; không đổi FBX gốc.
- ProjectSettings/TagManager.asset: giữ Floor 8; thêm NightmareWorld 13, NightmarePlayer 14, NightmareEnemy 15.
- ProjectSettings/EditorBuildSettings.asset: thay scene tutorial không tồn tại bằng scene game.
- Product name, company và độ phân giải mặc định trong PlayerSettings.
- Kiểm tra collision layer cũng được thiết lập lúc runtime để không phụ thuộc trạng thái Editor.

ProjectVersion.txt, Packages/manifest.json và render pipeline vẫn giữ nguyên. Danh sách so sánh hash thực tế nằm trong Verification/source-audit.json.

## Cập nhật 11/10/2026: sinh quái và ngắm

Đã sửa lỗi NavMeshPath khởi tạo trong constructor gây dừng sinh quái; chuyển sang Awake. Hiện con trỏ chuột, thêm tâm ngắm vàng lớn có viền đen và ngắm ngoài biên sàn. Đã biên dịch với thư viện Unity 2018.4 thành công; chưa chạy lại Play Mode do lỗi khởi chạy Windows từ công cụ. Xem Verification/FIX_2026-10-11.md. Bản scene người dùng đã tạo được giữ nguyên.
