# Trạng thái kiểm tra — 10/10/2026

## Đã thực hiện

- Đọc Assets, Packages/manifest.json và các ProjectSettings liên quan.
- Xác nhận ProjectVersion: 2018.4.17f1, input WASD/mũi tên đã có, không có scene/Animator Controller trong project nguồn.
- Đọc toàn bộ 9 script gameplay ban đầu; kiểm tra prefab môi trường/particle, metadata animation, materials và audio.
- Tạo bản sao Assets, Packages, ProjectSettings. Không ghi vào project trong Downloads.
- Kiểm tra biên dịch tĩnh toàn bộ C# gồm công cụ Editor: PASS, exit code 0, không diagnostic cuối cùng.
- Kiểm tra biên dịch tĩnh riêng mã runtime với UNITY_EDITOR không được định nghĩa, C# 7: PASS, exit code 0, không diagnostic cuối cùng.
- Kiểm tra tham chiếu GUID asset và các đường dẫn asset mà công cụ cần.
- Kiểm tra package manifest và ProjectVersion giữ nguyên nội dung.

## Phạm vi của biên dịch tĩnh

Compiler: Roslyn đi kèm Unity 2022.3.55f1. Reference assemblies: .NET Framework 4.8 + UnityEngine/UnityEditor/UI của bản 2022.3 cài sẵn. Đây là kiểm tra C# ngoài Editor, **không mở/import/nâng cấp project**, không phải kiểm tra tương thích nhị phân với Unity 2018.4 và không phải build game. Mã mới chỉ dùng API dự định tương thích 2018.4; cần Unity 2018.4 xác nhận lần cuối.

## Chưa thực hiện

- Import/compile bằng Unity 2018.4.17f1 (chưa tìm thấy bản này tại vị trí cài Unity trên máy).
- Chạy NightmareSetup trong Editor, sinh scene/prefab/controller/NavMesh thực tế.
- Chạy Play Mode smoke test. File playmode-smoke.txt chỉ xuất hiện sau khi người dùng thực sự chạy bài thử.
- Kiểm tra hình ảnh, vị trí đầu nòng, animation ZomBear, ánh sáng và UI theo độ phân giải.
- Chơi bằng bàn phím/chuột, cân bằng độ khó và chạy đủ 10 đợt thủ công.
- Build và chạy EXE Windows.

Không coi các mục chưa thực hiện là đã đạt. File kiểm thử được cung cấp là công cụ để kiểm chứng, không phải bằng chứng đã chạy.
