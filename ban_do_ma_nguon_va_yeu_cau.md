# 🗺️ BẢN ĐỒ MÃ NGUỒN & ĐỐI CHIẾU YÊU CẦU ĐỒ ÁN

Tài liệu này dùng để đối chiếu **chính xác vị trí code (Thư mục, File, Dòng code)** đang đáp ứng các tiêu chí chấm điểm Giữa kỳ và Cuối kỳ của giảng viên. Bạn có thể dùng file này để mở code ra chỉ cho giảng viên xem lúc bảo vệ đồ án.

---

## 📌 PHẦN A: YÊU CẦU GIỮA KỲ

### 1. Tối thiểu 3-4 form ngoài form chính và form đăng nhập
- **Code ở đâu:** Thư mục `wwwroot/`
- **File cụ thể:** 
  - Sơ đồ Bàn: `index.html`
  - Quản lý Thực đơn: `menu.html` 
  - Gọi món & POS: `order.html`
  - Lịch sử doanh thu: `lichsu.html`
  - Đăng nhập/Đăng ký: `login.html`

### 2. Form quản lý quan hệ giữa 2 đối tượng (Nhiều - Nhiều)
- **Code ở đâu:** Thư mục `wwwroot/` và `DAL/`
- **File cụ thể:** 
  - Giao diện: `order.html` và `js/order.js` (Hàm `themMon()`).
  - Backend: `DAL/ChiTietHoaDonDAL.cs` - Bảng ChiTietHoaDon đóng vai trò là bảng trung gian kết nối quan hệ Nhiều-Nhiều giữa `HoaDon` và `SanPham`.

### 3. Project chạy lên được (Tính thực thi)
- **Code ở đâu:** Thư mục gốc `QuanLyNhaHang/`
- **File cụ thể:** `Program.cs`
- **Dòng code:**
  - Dòng `35`: `var app = builder.Build();` (Khởi tạo máy chủ Web API backend).
  - Dòng `555`: `_ = Task.Run(() => app.Run("http://localhost:5000"));` (Chạy luồng ngầm cho backend).
  - Dòng `564 - 599`: Khởi tạo cửa sổ Desktop `System.Windows.Forms.Form` và nhúng trình duyệt `WebView2` để hiển thị app.

### 4. Giao diện có trang trí màu sắc, biểu tượng hoàn chỉnh
- **Code ở đâu:** Thư mục `wwwroot/`
- **File cụ thể:** Các file HTML (ví dụ `index.html`)
- **Dòng code:** 
  - Dòng `12 - 32`: Thẻ `<style>` và cấu hình Tailwind CSS định nghĩa mã màu chủ đạo (Dark Mode, màu `primary` tím oải hương).
  - Dòng `11`: Import icon từ thư viện `Material Symbols Outlined`.

### 5. Có thể nhảy qua lại giữa các form (Điều hướng)
- **Code ở đâu:** Thư mục `wwwroot/`
- **File cụ thể:** Các file HTML (ví dụ `index.html`)
- **Dòng code:** Dòng `55 - 90`. Đây là khu vực thanh Navbar/Sidebar bên trái chứa các thẻ `<a>` link tới `index.html`, `menu.html`, `order.html`...

### 6. Thay biểu tượng chính (Icon/Favicon) của chương trình
- **Code ở đâu:** Desktop Icon (`Program.cs`) và Web Favicon (`Các file HTML`)
- **File & Dòng code:**
  - **Desktop App Icon:** `Program.cs` (Dòng `572 - 580`) đoạn `using var bitmap = new System.Drawing.Bitmap(iconPath); formMain.Icon = System.Drawing.Icon.FromHandle(...)`.
  - **Favicon:** `index.html` (Dòng `8`): `<link rel="icon" type="image/png" href="img/logo.png">`.

### 7. Control (Textbox, Combobox, Button) tuân theo quy tắc đặt tên
- **Code ở đâu:** Thư mục `wwwroot/` và `wwwroot/js/`
- **File cụ thể:** Tất cả HTML và JS.
- **Dòng code minh họa:**
  - `menu.html` (Dòng `235`): `id="txtTenSanPham"` (Tiền tố `txt` cho Textbox).
  - `order.html` (Dòng `242`): `id="cboBan"` (Tiền tố `cbo` cho Combobox/Select).
  - `login.html` (Dòng `127`): `id="btnDangKy"` (Tiền tố `btn` cho Button).

### 8. CSDL có bảng, cột, khóa ngoại liên kết (Relational Database)
- **Code ở đâu:** Thư mục `DAL/`
- **File cụ thể:** `DatabaseHelper.cs` (Hàm `KhoiTaoCSDL`)
- **Dòng code:** 
  - Dòng `21`: `PRAGMA foreign_keys = ON;` (Bật tính năng khóa ngoại cho SQLite).
  - Dòng `25 - 85`: Các lệnh `CREATE TABLE` chứa `FOREIGN KEY` (ví dụ: `BanId INTEGER REFERENCES Ban(Id)`).

---

## 🎯 PHẦN B: YÊU CẦU CUỐI KỲ (KIẾN TRÚC & BẢO MẬT)

### 9. Cấu trúc chuẩn 3 Tầng (3-Tier Architecture)
- **Tầng Giao diện (Presentation / UI):** Nằm toàn bộ trong thư mục `wwwroot/` (HTML, JS, CSS).
- **Tầng Xử lý Dữ liệu (Data Access Layer - DAL):** Thư mục `DAL/` (chứa các file truy xuất database như `BanDAL.cs`, `HoaDonDAL.cs`).
- **Tầng Thực thể (Entities / DTO):** Thư mục `Entities/` (chứa định nghĩa các class C# như `Ban.cs`, `SanPham.cs`).

### 10. Sử dụng Interface & Đa hình (Polymorphism)
- **Code ở đâu:** `DAL/Interfaces/` và `Entities/`
- **Dòng code minh họa:**
  - **Interface:** `Program.cs` (Dòng `29-33`): `builder.Services.AddSingleton<IBanDAL, BanDAL>();` (Áp dụng Dependency Injection với Interface).
  - **Đa hình OOP:** `Entities/SanPham.cs` có hàm `abstract decimal TinhTien()`. Lớp `ThucAn` và `NuocUong` kế thừa và viết lại (`override`) cách tính tiền khác nhau.

### 11. Validation trùng lặp dữ liệu (Nâng cao)
- **Code ở đâu:** Thư mục `DAL/`
- **File cụ thể:** `BanDAL.cs`, `SanPhamDAL.cs`, `NguoiDungDAL.cs`
- **Dòng code:** `BanDAL.cs` (Hàm `Them`, Dòng `78 - 85`). Lệnh `SELECT COUNT(*) FROM Ban WHERE TenBan = @TenBan`. Nếu tồn tại thì `throw new Exception("Tên bàn đã tồn tại!");`.

### 12. Try-Catch-Finally và đóng kết nối CSDL an toàn
- **Code ở đâu:** Thư mục `DAL/` (Trong tất cả các file DAL)
- **File cụ thể minh họa:** `BanDAL.cs`
- **Dòng code:** Trong bất kỳ hàm CRUD nào (vd dòng `90 - 110`). Có cấu trúc `SqliteConnection? conn = null; try { ... } catch { ... } finally { if (conn != null) conn.Close(); }`. Đảm bảo không bao giờ bị treo Database.

### 13. Chống tấn công SQL Injection (Parameterized Query)
- **Code ở đâu:** Thư mục `DAL/` (Trong tất cả các file DAL)
- **File cụ thể minh họa:** `ChiTietHoaDonDAL.cs`
- **Dòng code:** Dòng `21 - 25`. Sử dụng `@param` thay vì cộng chuỗi trực tiếp: `cmd.CommandText = "INSERT INTO ChiTietHoaDon (...) VALUES (@HoaDonId, @SanPhamId, ...)"; cmd.Parameters.AddWithValue("@HoaDonId", chiTiet.HoaDonId);`.

### 14. Bảo mật Mật khẩu (Hash SHA256)
- **Code ở đâu:** Thư mục gốc và `DAL/`
- **File cụ thể:** `Program.cs` và `NguoiDungDAL.cs`
- **Dòng code:**
  - `Program.cs` (Dòng `54 - 62`): Hàm `BamSHA256()` biến mật khẩu chữ thành chuỗi băm 64 ký tự hex. Không thể dịch ngược.
  - `Program.cs` (Dòng `97` và `132`): Mã hóa mật khẩu trước khi lưu vào DB hoặc khi so sánh lúc đăng nhập.

### 15. Hành vi của Form/Modal (ShowDialog)
- **Yêu cầu:** Không click ra ngoài để đóng, bắt buộc dùng nút Đóng.
- **Code ở đâu:** `wwwroot/js/`
- **File cụ thể:** `ban.js` (dòng `284`), `menu.js` (dòng `251`), `lichsu.js` (dòng `121`).
- **Dòng code:** Hàm `moModal(id)` chỉ dùng `classList.add('show')` và KHÔNG bắt sự kiện click trên lớp màn đen (overlay), buộc người dùng phải nhấn nút Hủy/Đóng có sự kiện `dongModal()` bên trong form.

### 16. Chức năng Tìm kiếm
- **Code ở đâu:** `wwwroot/`
- **File cụ thể:** `index.html` và `js/ban.js`
- **Dòng code:** 
  - `index.html` (Dòng `206`): Ô input có id `txtTimKiemBan` với sự kiện `oninput="timKiemBan()"`.
  - `js/ban.js` (Dòng `28`): Hàm `timKiemBan()` lọc realtime (Lọc ngay khi gõ) dựa vào `danhSachBan.filter()`.
