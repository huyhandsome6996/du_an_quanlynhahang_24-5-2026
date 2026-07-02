# 📝 GHI CHÚ NGUỒN GỐC TẠO RA CÁC FILE VÀ THƯ MỤC TRONG DỰ ÁN

Tài liệu này giúp bạn nắm rõ các thành phần trong dự án được tạo ra bằng cách nào: **Bằng câu lệnh Terminal** (Tự động) hay **Bằng cách thủ công** (Tạo file bằng tay/Code thủ công).

---

## 💻 1. CÁC THƯ MỤC VÀ FILE TẠO BẰNG LỆNH TERMINAL (TỰ ĐỘNG)

Đây là những thành phần được sinh ra khi chạy lệnh hoặc do hệ thống tự sinh trong quá trình chạy chương trình:

*   **Thư mục `QuanLyNhaHang/` (Thư mục gốc của code C#):**
    *   *Lệnh tạo ra:* `dotnet new web -n QuanLyNhaHang -o ./QuanLyNhaHang`
    *   *Mô tả:* Lệnh khởi tạo một dự án Web API trống của .NET.
*   **File `QuanLyNhaHang/QuanLyNhaHang.csproj` (File cấu hình dự án C#):**
    *   *Lệnh tạo ra:* Tự động sinh bởi lệnh `dotnet new web` ở trên, sau đó được cập nhật tự động khi chạy lệnh cài đặt thư viện SQLite: `dotnet add package Microsoft.Data.Sqlite`.
*   **File `QuanLyNhaHang/Program.cs`:**
    *   *Lệnh tạo ra:* Tự sinh ra ban đầu bởi lệnh `dotnet new web`, nhưng sau đó chúng ta đã **mở ra và viết thêm code thủ công** để định nghĩa các API.
*   **Thư mục `QuanLyNhaHang/Properties/` (Chứa file `launchSettings.json`):**
    *   *Lệnh tạo ra:* Tự động sinh bởi lệnh `dotnet new web` để cấu hình cổng mạng (cổng chạy cổng 5000).
*   **Các thư mục `QuanLyNhaHang/bin/` và `QuanLyNhaHang/obj/`:**
    *   *Lệnh tạo ra:* Tự động sinh ra khi ta chạy lệnh biên dịch `dotnet build` hoặc chạy thử `dotnet run`. Chúng chứa các file thực thi đã biên dịch từ C#.
*   **File `QuanLyNhaHang/nha_hang.db` (Cơ sở dữ liệu SQLite):**
    *   *Lệnh tạo ra:* Tự động tạo ra bởi code C# lúc ứng dụng khởi động. Khi chạy lệnh `dotnet run`, chương trình sẽ tự kiểm tra xem file này tồn tại chưa, nếu chưa sẽ tự sinh ra file cơ sở dữ liệu này.
*   **Thư mục ẩn `.git/` và file `.gitignore`:**
    *   *Lệnh tạo ra:* Được sinh ra khi khởi tạo kho lưu trữ Git bằng lệnh `git init`.

---

## ✍️ 2. CÁC THƯ MỤC VÀ FILE TẠO BẰNG TAY (THỦ CÔNG)

Đây là những thư mục và file do chính chúng ta tạo ra bằng cách nhấp chuột phải chọn "New File / New Folder" rồi viết code vào bên trong:

### A. Trong Thư Mục Backend C# (`QuanLyNhaHang/`):
*   **Thư mục `Entities/`:** Được tạo thủ công để làm tầng mô tả thực thể.
    *   *Các file bên trong:* `Ban.cs`, `SanPham.cs`, `ThucAn.cs`, `NuocUong.cs`, `HoaDon.cs`, `ChiTietHoaDon.cs` đều được viết code bằng tay.
*   **Thư mục `DAL/` (Data Access Layer):** Được tạo thủ công để quản lý kết nối CSDL.
    *   *Các file bên trong:* `DatabaseHelper.cs`, `BanDAL.cs`, `SanPhamDAL.cs`, `HoaDonDAL.cs`, `ChiTietHoaDonDAL.cs` đều được viết thủ công.
    *   *Thư mục con `DAL/Interfaces/`:* Chứa các interface `IBanDAL.cs`, `ISanPhamDAL.cs`... viết bằng tay.

### B. Trong Thư Mục Giao Diện (`QuanLyNhaHang/wwwroot/`):
*   **Thư mục `wwwroot/`:** Tạo thủ công để chứa mặt tiền ứng dụng.
*   **Các file HTML giao diện:** `login.html`, `index.html`, `menu.html`, `order.html`, `lichsu.html` được tạo và viết bằng tay.
*   **Thư mục `wwwroot/css/`:** Chứa file `style.css` (Style trang trí) được viết bằng tay.
*   **Thư mục `wwwroot/js/`:** Chứa các kịch bản hành động `ban.js`, `menu.js`, `order.js`, `lichsu.js` được viết bằng tay.
*   **Thư mục `wwwroot/img/`:** Được tạo thủ công để chứa file logo.
    *   *File `logo.png`:* Được tạo ra nhờ công cụ AI sinh ảnh (Image Generator) và được lưu thủ công vào thư mục này.

### C. Ở Thư Mục Gốc Dự Án:
*   **File `ChayUngDung.bat`:** Script khởi động nhanh 1-click tạo bằng tay.
*   **Các file tài liệu Markdown báo cáo:**
    *   `README.md` (Hướng dẫn dự án).
    *   `nhat_ki_hoat_dong.md` (Nhật ký cập nhật).
    *   `yeu_cau_can_dat_tu_thay.txt` & `yeu_cau_can_dat_tu_thay.md` (Báo cáo nộp thầy).
    *   `trello.md` (Bảng phân rã công việc).
    *   `ghi_tru_cac_file_dc_tao_ra_nhu_nao.md` (Chính là file ghi chú này).
