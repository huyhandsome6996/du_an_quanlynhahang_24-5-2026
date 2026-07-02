# DỰ ÁN QUẢN LÝ NHÀ HÀNG - VUA SƯ TỬ 🦁

> **Mô tả:** Đây là hệ thống phần mềm quản lý nhà hàng hiện đại, được xây dựng bằng C# (Backend) kết nối với cơ sở dữ liệu **Microsoft Access (.accdb)** qua OLE DB Provider và giao diện (Frontend) viết bằng HTML/CSS/JS thuần theo phong cách Glassmorphism sang trọng.

---

## 📂 CẤU TRÚC CÂY THƯ MỤC VÀ Ý NGHĨA TỪNG FILE
*(Giải thích cặn kẽ chi tiết - Rất dễ hiểu)*

Bạn hãy tưởng tượng dự án này như một **Nhà Hàng ngoài đời thực**. Mỗi thư mục đóng một vai trò cụ thể:

```text
du_an_quanlynhahang_24-5-2026/
│
├── QuanLyNhaHang/                   <-- Thư mục gốc chứa toàn bộ mã nguồn của phần mềm
│   │
│   ├── Entities/                    <-- KHU VỰC "BẢN VẼ" (Lớp thực thể)
│   │   ├── Ban.cs                   (Bản vẽ cấu tạo của 1 cái Bàn)
│   │   ├── SanPham.cs               (Bản vẽ chung của Sản Phẩm - Món ăn/Nước uống)
│   │   ├── ThucAn.cs                (Món ăn - Kế thừa từ Sản Phẩm)
│   │   ├── NuocUong.cs              (Thức uống - Kế thừa từ Sản Phẩm)
│   │   ├── HoaDon.cs                (Bản vẽ cấu tạo của tờ Hóa đơn tính tiền)
│   │   └── ChiTietHoaDon.cs         (Bản vẽ cấu tạo của 1 dòng ghi món trên hóa đơn)
│   │
│   ├── DAL/                         <-- KHU VỰC "KHO LƯU TRỮ" (Data Access Layer)
│   │   ├── DatabaseHelper.cs        (Người gác cổng: Quản lý chuỗi kết nối tới file QuanLyNhaHang.accdb)
│   │   ├── BanDAL.cs                (Thủ kho chuyên quản lý việc Thêm/Sửa/Xóa dữ liệu Bàn)
│   │   ├── SanPhamDAL.cs            (Thủ kho chuyên quản lý dữ liệu Thực đơn)
│   │   ├── HoaDonDAL.cs             (Thủ kho chuyên lưu trữ Hóa đơn)
│   │   └── Interfaces/              (Bảng nội quy quy định các thủ kho phải làm gì)
│   │
│   ├── wwwroot/                     <-- KHU VỰC "MẶT TIỀN / SẢNH KHÁCH" (Giao diện)
│   │   ├── login.html               (Cánh cửa bảo vệ: Form tạo mật khẩu và đăng nhập)
│   │   ├── index.html               (Sảnh chính: Sơ đồ các Bàn)
│   │   ├── menu.html                (Cuốn menu: Nơi quản lý món ăn)
│   │   ├── order.html               (Quầy phục vụ: Nơi bấm chọn món và tính tiền)
│   │   ├── lichsu.html              (Sổ ghi chép: Xem lại các hóa đơn đã thanh toán)
│   │   ├── css/                     (Tủ quần áo: Chứa file style.css để làm đẹp giao diện)
│   │   ├── img/                     (Nơi treo tranh ảnh: Chứa logo.png con sư tử)
│   │   └── js/                      (Kịch bản hành động: Chứa ban.js, menu.js quy định nút bấm)
│   │
│   └── Program.cs                   <-- "NGƯỜI QUẢN LÝ CHUNG" (Trái tim của phần mềm)
│                                    (File chạy đầu tiên. Khởi động CSDL, mở server và kết nối Giao diện với Kho)
│
├── README.md                        <-- CUỐN SÁCH HƯỚNG DẪN SỬ DỤNG (Chính là file bạn đang đọc)
├── BaoCaoDoAnCuoiKi_QuanLyNhaHang_HoQuangHuy.docx   <-- Báo cáo đồ án cuối kì (nộp cho thầy)
├── KeHoachSlidePowerPoint_QuanLyNhaHang_HoQuangHuy.docx   <-- Kế hoạch slide PowerPoint để bảo vệ
└── access_db/                      <-- Folder chứa file .accdb gốc + script SQL/Java tham khảo
```

---

## 🔍 CHI TIẾT Ý NGHĨA VÀ TÁC DỤNG CỦA TỪNG THƯ MỤC VÀ FILE

Dưới đây là mô tả chi tiết của từng phần trong cấu trúc dự án để bạn dễ dàng nắm bắt hoặc giải thích trước Hội đồng bảo vệ:

### 📐 1. Thư mục `Entities/` (Lớp Thực Thể - Tầng Core OOP)
Đây là nơi thể hiện **tính chất hướng đối tượng (OOP)** mạnh mẽ nhất thông qua việc định nghĩa các thực thể (đối tượng) có trong nhà hàng:
*   **`SanPham.cs`**: Lớp trừu tượng (`abstract class`) đại diện cho sản phẩm nói chung. Nó đóng vai trò làm lớp cha (Base Class), chứa các thuộc tính cơ bản như `Id`, `TenSanPham`, `GiaCoBan`, `DangBan` (trạng thái bán) và đặc biệt là trường **`HinhAnh`** (lưu URL ảnh minh họa).
*   **`ThucAn.cs` & `NuocUong.cs`**: Các lớp con kế thừa (`inheritance`) từ `SanPham`. Đây là minh chứng của tính Đa hình và Kế thừa trong OOP.
*   **`Ban.cs`**: Mô tả đối tượng Bàn ăn trong nhà hàng với các thông tin `Id`, `TenBan` và `TrangThai` ("Trống" hoặc "Có khách").
*   **`HoaDon.cs`**: Đại diện cho 1 hóa đơn tính tiền của bàn ăn, gồm mã hóa đơn, mã bàn, giờ mở bàn, giờ đóng bàn, trạng thái thanh toán và tổng tiền.
*   **`ChiTietHoaDon.cs`**: Đại diện cho từng dòng món ăn được gọi trên hóa đơn (mã hóa đơn, mã sản phẩm, số lượng, giá bán tại thời điểm gọi).

### 🗄️ 2. Thư mục `DAL/` (Data Access Layer - Tầng Truy Xuất Dữ Liệu)
Nhiệm vụ của tầng này là làm việc trực tiếp với cơ sở dữ liệu Microsoft Access (`QuanLyNhaHang.accdb`) qua OLE DB Provider, đọc ghi dữ liệu từ C# vào file vật lý:
*   **`DatabaseHelper.cs`**: Quản lý chuỗi kết nối OLE DB tới file `QuanLyNhaHang.accdb`, kiểm tra file CSDL tồn tại khi ứng dụng khởi động.
*   **Các lớp DAL (`BanDAL.cs`, `SanPhamDAL.cs`, `HoaDonDAL.cs`, `ChiTietHoaDonDAL.cs`)**: Thực hiện các câu lệnh SQL (`SELECT`, `INSERT`, `UPDATE`, `DELETE`) để truy vấn và cập nhật dữ liệu tương ứng.
*   **Thư mục con `Interfaces/` (`IBanDAL.cs`, `ISanPhamDAL.cs`...)**: Định nghĩa các Interface quy định các phương thức mà một lớp DAL bắt buộc phải triển khai. Điều này thể hiện **tính Trừu tượng (Abstraction)** trong thiết kế phần mềm.

### 💻 3. Thư mục `wwwroot/` (Giao diện Frontend)
Là nơi chứa toàn bộ giao diện chạy trên trình duyệt web và WebView2 Desktop, sử dụng phong cách thiết kế **Glassmorphism (kính mờ)** hiện đại:
*   **`login.html`**: Màn hình bảo mật, bắt buộc người dùng thiết lập mật khẩu ở lần chạy đầu tiên và đăng nhập để bảo mật cơ sở dữ liệu.
*   **`index.html` (kèm `js/ban.js`)**: Sơ đồ lưới hiển thị danh sách bàn ăn theo thời gian thực (real-time). Cho phép thêm/sửa/xóa bàn và click chọn bàn để thanh toán hoặc gọi món.
*   **`menu.html` (kèm `js/menu.js`)**: Trang quản lý thực đơn món ăn và thức uống. Cho phép thêm món mới kèm theo **hình ảnh minh họa (URL)**, chỉnh sửa giá cả hoặc ẩn/hiện sản phẩm.
*   **`order.html` (kèm `js/order.js`)**: Giao diện POS bán hàng dành cho nhân viên. Hiển thị thực đơn trực quan kèm ảnh thu nhỏ giúp thao tác gọi món nhanh chóng và hỗ trợ in hóa đơn tạm tính.
*   **`lichsu.html` (kèm `js/lichsu.js`)**: Nơi xem lại toàn bộ lịch sử hóa đơn đã thanh toán, xem chi tiết hóa đơn và tính tổng doanh thu nhà hàng.

### ⚙️ 4. File cấu hình và chạy đầu tiên
*   **`Program.cs`**: File khởi chạy của ứng dụng .NET. File này làm 2 nhiệm vụ chạy song song:
    1.  Mở một máy chủ Web API (ASP.NET Core Minimal API) tại cổng `http://localhost:5000` để phục vụ các yêu cầu dữ liệu từ frontend.
    2.  Khởi tạo một luồng đơn (STA Thread) chạy giao diện Windows Forms tích hợp trình duyệt WebView2, biến ứng dụng web thành một ứng dụng Desktop chuyên nghiệp độc lập.
*   **`ChayUngDung.bat`**: File script 1-click giúp người dùng chạy nhanh dự án mà không cần gõ lệnh Terminal bằng tay.

### 📝 5. Các tài liệu hướng dẫn và báo cáo
*   **`BaoCaoDoAnCuoiKi_QuanLyNhaHang_HoQuangHuy.docx`**: Báo cáo đồ án cuối kì môn Lập trình hướng đối tượng (29 trang, đầy đủ lời cảm ơn, mục lục, 5 chương, ER diagram, UI screenshots).
*   **`KeHoachSlidePowerPoint_QuanLyNhaHang_HoQuangHuy.docx`**: Kế hoạch 17 slide PowerPoint + 10 câu hỏi Q&A gợi ý + checklist trước buổi bảo vệ.
*   **`access_db/QuanLyNhaHang_Access.sql`**: Script SQL tham khảo để tạo lại database trong Access Query Designer.
*   **`access_db/tao_access.java`**: Mã nguồn Java (dùng Jackcess) đã tạo ra file `.accdb` - tham khảo cách sinh file Access bằng code.

---

## 🚀 HƯỚNG DẪN CÁCH CHẠY PHẦN MỀM

Vì đây là dự án C#, cách chạy vô cùng đơn giản:

1. Mở màn hình **Terminal (Command Prompt / PowerShell)**.
2. Dùng lệnh `cd QuanLyNhaHang` để đi vào thư mục gốc của code.
3. Gõ lệnh `dotnet run` và ấn Enter.
4. Mở trình duyệt web (Chrome, Edge, Cốc Cốc) và truy cập vào địa chỉ: `http://localhost:5000`
5. Lần đầu tiên vào, hệ thống sẽ yêu cầu bạn **Tạo một tài khoản Quản trị**. Sau đó đăng nhập bằng tài khoản vừa tạo để sử dụng!

---

*Đồ án Môn học Lập trình Hướng đối tượng (OOP)*
*Phiên bản: 1.0 (Vua Sư Tử)*