# 📓 NHẬT KÝ HOẠT ĐỘNG - DỰ ÁN QUẢN LÝ NHÀ HÀNG

> **Dự án:** Ứng dụng Quản Lý Nhà Hàng (Desktop - C# + SQLite + HTML)  
> **Nhóm:** OOP Project  
> **Ngày bắt đầu:** 24/05/2026  

---

## 📅 Ngày 24/05/2026

### 🕐 13:21 — Khởi động dự án, lên kế hoạch tổng thể

**Công việc:**
- Phân tích yêu cầu ERD từ giảng viên
- Thiết kế cấu trúc thư mục 3 tầng (Entity → DAL → Presentation)
- Lên danh sách API endpoints cần làm
- Thiết kế cách áp dụng 4 tính chất OOP vào dự án

**Quyết định thiết kế:**
- `SanPham` sẽ là abstract class (Trừu tượng)
- `ThucAn` và `NuocUong` kế thừa `SanPham` (Kế thừa)
- `TinhTien()` là phương thức abstract → override khác nhau (Đa hình)
- Các DAL đều implement Interface tương ứng (Trừu tượng + Đóng gói)

---

### 🕐 13:22 — Khởi tạo project C#

**Lệnh terminal:**
```bash
dotnet new web -n QuanLyNhaHang --no-restore -o ./QuanLyNhaHang
dotnet add package Microsoft.Data.Sqlite --version 8.0.0
```

**Kết quả:** Project tạo thành công, SQLite package được cài đặt.

---

### 🕐 13:23 — Tạo tầng Entity (Entities/)

**Các file tạo mới:**
- `Entities/SanPham.cs` — Abstract class, có property validate (Đóng gói)
- `Entities/ThucAn.cs` — Kế thừa SanPham, override TinhTien(): +50,000đ khi "Phần lớn"
- `Entities/NuocUong.cs` — Kế thừa SanPham, override TinhTien(): x1.2 khi chọn "Lon"
- `Entities/Ban.cs` — Thực thể bàn ăn
- `Entities/HoaDon.cs` — Thực thể hóa đơn
- `Entities/ChiTietHoaDon.cs` — Thực thể chi tiết hóa đơn

**OOP áp dụng:**
- **Đóng gói:** `GiaCoBan` có setter validate không được âm; `TenSanPham` validate không rỗng
- **Trừu tượng:** `TinhTien()` và `MoTaPhuPhi()` là phương thức abstract
- **Kế thừa:** ThucAn, NuocUong extends SanPham

---

### 🕐 13:24 — Tạo tầng DAL - Interfaces

**Các file tạo mới:**
- `DAL/Interfaces/IBanDAL.cs`
- `DAL/Interfaces/ISanPhamDAL.cs`
- `DAL/Interfaces/IHoaDonDAL.cs`
- `DAL/Interfaces/IChiTietHoaDonDAL.cs`

**OOP áp dụng:** Tất cả DAL implement interface → **Trừu tượng hóa**

---

### 🕐 13:24 — Tạo tầng DAL - Implementations

**Các file tạo mới:**
- `DAL/DatabaseHelper.cs` — Khởi tạo SQLite, tạo bảng, seed dữ liệu mẫu (6 bàn, 10 món)
- `DAL/BanDAL.cs` — Implement IBanDAL: CRUD bàn
- `DAL/SanPhamDAL.cs` — Implement ISanPhamDAL: CRUD sản phẩm, **dùng Factory Pattern tạo ThucAn/NuocUong từ DB**
- `DAL/HoaDonDAL.cs` — Implement IHoaDonDAL: quản lý hóa đơn
- `DAL/ChiTietHoaDonDAL.cs` — Implement IChiTietHoaDonDAL: quản lý chi tiết hóa đơn

---

### 🕐 13:26 — Tạo Program.cs (API Routes)

**Công việc:**
- Đăng ký Dependency Injection cho các DAL
- Map toàn bộ API endpoints (15 routes)
- Xử lý Đa hình: khi thêm món, gọi `sp.TinhTien()` — ThucAn và NuocUong tính khác nhau
- App chạy trên `http://localhost:5000`

**Danh sách API endpoints đã tạo:**
```
GET  /api/ban                     - Lấy tất cả bàn
POST /api/ban                     - Thêm bàn
PUT  /api/ban/{id}                - Sửa bàn
DEL  /api/ban/{id}                - Xóa bàn
GET  /api/sanpham                 - Lấy tất cả sản phẩm
GET  /api/sanpham/dangban         - Lấy sản phẩm đang bán
POST /api/sanpham                 - Thêm sản phẩm
PUT  /api/sanpham/{id}            - Sửa sản phẩm
DEL  /api/sanpham/{id}            - Xóa sản phẩm
POST /api/ban/{id}/mo             - Mở bàn (tạo hóa đơn)
GET  /api/ban/{id}/hoadon         - Hóa đơn hiện tại của bàn
POST /api/hoadon/{id}/them-mon    - Thêm món vào hóa đơn
DEL  /api/chitiethoadon/{id}      - Xóa món khỏi hóa đơn
POST /api/ban/{id}/thanhtoan      - Thanh toán & đóng bàn
GET  /api/hoadon                  - Lịch sử hóa đơn
GET  /api/hoadon/{id}             - Chi tiết 1 hóa đơn
```

---

### 🕐 13:27 — Tạo giao diện HTML (wwwroot/)

**Các file tạo mới:**
- `wwwroot/css/style.css` — CSS toàn cục, dark theme sang trọng, màu vàng đồng
- `wwwroot/index.html` — Trang Quản Lý Bàn (lưới bàn trực quan)
- `wwwroot/menu.html` — Trang Thực Đơn (bảng CRUD)
- `wwwroot/order.html` — Trang Gọi Món (layout 2 cột)
- `wwwroot/lichsu.html` — Trang Lịch Sử Hóa Đơn

---

### 🕐 13:30 — Tạo JavaScript logic

**Các file tạo mới:**
- `wwwroot/js/ban.js` — Logic quản lý bàn
- `wwwroot/js/menu.js` — Logic thực đơn CRUD
- `wwwroot/js/order.js` — Logic gọi món & thanh toán
- `wwwroot/js/lichsu.js` — Logic lịch sử & thống kê

---

### 🕐 13:33 — Kiểm tra build

**Lệnh terminal:**
```bash
dotnet build --no-restore
```

**Kết quả:**
```
Build succeeded.
0 Warning(s)
0 Error(s)
```

---

### 🕐 13:34 — Triển khai Gitflow & Commit theo User Story

Chúng tôi tiến hành chia nhỏ dự án thành các **User Story** và **Task** cụ thể, tạo nhánh `feature/` riêng biệt, commit bằng tiếng Việt chi tiết rồi merge `--no-ff` vào nhánh `develop`. Cuối cùng gộp về `main` và gắn tag release.

**Nhánh & Lịch sử Commit chi tiết:**

1. **User Story 1: Thiết lập môi trường dự án & SQLite**
   - Nhánh: `feature/setup-moi-truong`
   - Commit: `Tính năng: Thiết lập môi trường dự án C# và CSDL SQLite ban đầu`

2. **User Story 2: Xây dựng lớp thực thể OOP**
   - Nhánh: `feature/entities-oop`
   - Commit: `Tính năng: Xây dựng các lớp thực thể áp dụng đầy đủ 4 tính chất OOP`

3. **User Story 3: Triển khai tầng DAL & Interfaces**
   - Nhánh: `feature/dal-layer`
   - Commit: `Tính năng: Triển khai tầng DAL và Interfaces kết nối SQLite`

4. **User Story 4: Xây dựng API Endpoints Backend**
   - Nhánh: `feature/api-backend`
   - Commit: `Tính năng: Xây dựng hệ thống Web API phục vụ Frontend HTML`

5. **User Story 5: Thiết kế Giao diện Frontend HTML/CSS/JS**
   - Nhánh: `feature/frontend-ui`
   - Commit: `Tính năng: Thiết kế giao diện HTML/CSS/JS đa trang sang trọng`

6. **User Story 6: Tài liệu báo cáo & Nhật ký**
   - Nhánh: `feature/tai-lieu-du-an`
   - Commit: `Tài liệu: Cập nhật nhật ký hoạt động chi tiết dự án`

7. **Hoàn thiện Release v1.0**
   - Nhánh: Gộp `develop` vào `main`
   - Tag: `v1.0-Final-Release`
   - Push toàn bộ nhánh và tag lên GitHub: `git push origin develop`, `git push origin main`, `git push origin --tags`

---

## 📋 Ghi Chú Kỹ Thuật Quan Trọng

### Cách chạy ứng dụng
```bash
cd QuanLyNhaHang
dotnet run
# Mở trình duyệt: http://localhost:5000
```

### Giải thích OOP cho giảng viên

| Tính chất | Áp dụng ở đâu | Ví dụ cụ thể |
|-----------|---------------|--------------|
| **Đóng gói** | `SanPham.cs` - Property setter | `GiaCoBan` validate >= 0, throw Exception nếu âm |
| **Kế thừa** | `ThucAn.cs`, `NuocUong.cs` | Cả 2 đều `:SanPham`, dùng lại `Id`, `TenSanPham`, `GiaCoBan` |
| **Đa hình** | `TinhTien()` trong ThucAn/NuocUong | ThucAn "Phần lớn" +50k, NuocUong "Lon" x1.2 |
| **Trừu tượng** | `IBanDAL`, `ISanPhamDAL`, v.v. | Không thể tạo `new SanPham()` trực tiếp |

### Cấu trúc thư mục
```
QuanLyNhaHang/
├── Entities/       ← Tầng 1: Lớp thực thể
├── DAL/            ← Tầng 2: Data Access Layer
│   └── Interfaces/ ← Các Interface (Trừu tượng)
├── wwwroot/        ← Tầng 3: Giao diện HTML/JS
│   ├── css/
│   └── js/
└── Program.cs      ← API Routes (Entry Point)
```

---

### 🕐 14:50 — Nâng cấp giao diện Bistro Elite (Dark Mode & Lavender)

**Công việc:**
- Chuyển đổi toàn bộ giao diện 4 trang (`index.html`, `menu.html`, `order.html`, `lichsu.html`) sang phong cách Bistro Elite sang trọng, tinh tế.
- Sử dụng bảng màu: Nền tối huyền bí `#0b1326`, Container `#171f33`, màu nhấn chính tím oải hương `#c0c1ff`.
- Áp dụng hiệu ứng kính mờ `glass-card` thời thượng cùng Sidebar bên trái rộng 72px cố định.
- Tối ưu hóa CSS để đảm bảo mọi phần tử HTML render động từ các file JavaScript riêng biệt (`ban.js`, `menu.js`, `order.js`, `lichsu.js`) hiển thị đẹp mắt, ăn khớp với thiết kế mới mà không làm thay đổi hay ảnh hưởng đến logic nghiệp vụ cũ.

