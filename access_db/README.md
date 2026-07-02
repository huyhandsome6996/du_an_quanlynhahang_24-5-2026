# 📁 Cơ sở dữ liệu Microsoft Access – Đồ án Quản lý Nhà hàng

Môn: **Lập trình hướng đối tượng** | Đề tài: **Hệ thống Quản lý Nhà hàng**

## 📋 Nội dung folder

| File | Mô tả |
|------|-------|
| `QuanLyNhaHang.accdb` | **File Access 2016 chính** – mở trực tiếp bằng MS Access 2016/2019/365. Đã có sẵn 5 bảng, 3 quan hệ khóa ngoại và dữ liệu mẫu. |
| `QuanLyNhaHang_Access.sql` | Script SQL tham khảo – cùng cấu trúc, có thể chạy trong Access Query Designer để tạo lại database từ đầu. |
| `tao_access.java` | Mã nguồn Java (dùng Jackcess 5.1.0) – đã tạo ra file `.accdb` ở trên. Mỗi dòng đều có ghi chú tiếng Việt. |
| `libs/jackcess.jar` + `libs/commons-logging.jar` | Thư viện Jackcess dùng để tạo file Access trên nền Java (không cần cài MS Access). |

## 🗄️ Cấu trúc cơ sở dữ liệu (5 bảng)

```
┌──────────────┐        ┌──────────────┐
│   NguoiDung  │        │     Ban      │
├──────────────┤        ├──────────────┤
│ Id (PK)      │        │ Id (PK)      │
│ TenDangNhap  │        │ TenBan       │
│ MatKhau      │        │ TrangThai    │
│ VaiTro       │        └──────┬───────┘
│ NgayTao      │               │
└──────────────┘               │ 1
                               │
                               │ N
                       ┌───────▼────────┐
                       │    HoaDon      │
                       ├────────────────┤
                       │ Id (PK)        │
                       │ BanId (FK)     │
                       │ ThoiGianTao    │
                       │ ThoiGianTT     │
                       │ TongTien       │
                       │ TrangThai      │
                       │ VAT            │
                       │ GiamGia        │
                       │ PTThanhToan    │
                       └───────┬────────┘
                               │ 1
                               │
                               │ N
                       ┌───────▼────────┐      ┌──────────────┐
                       │ ChiTietHoaDon  │      │   SanPham    │
                       ├────────────────┤      ├──────────────┤
                       │ Id (PK)        │      │ Id (PK)      │
                       │ HoaDonId (FK)  │      │ TenSanPham   │
                       │ SanPhamId (FK) │◄────►│ GiaCoBan     │
                       │ SoLuong        │      │ Loai         │
                       │ DonGiaBan      │      │ DangBan      │
                       │ ThuocTinhThem  │      │ HinhAnh      │
                       │ ThanhTien      │      └──────────────┘
                       │ TrangThaiMon   │
                       └────────────────┘
```

## 🔗 3 quan hệ khóa ngoại (có cascade)

| Quan hệ | Bảng cha → Bảng con | Cascade |
|---------|---------------------|---------|
| `HoaDon_Ban` | `Ban.Id` → `HoaDon.BanId` | Update + Delete |
| `CTHD_HoaDon` | `HoaDon.Id` → `ChiTietHoaDon.HoaDonId` | Update + Delete |
| `CTHD_SanPham` | `SanPham.Id` → `ChiTietHoaDon.SanPhamId` | Update + Delete |

## 📊 Dữ liệu mẫu có sẵn

| Bảng | Số dòng | Mô tả |
|------|---------|-------|
| `NguoiDung` | 3 | admin (QuanTri) / huy (QuanTri) / nhanvien1 (NhanVien) |
| `Ban` | 10 | Bàn 1 – Bàn 10 (3 bàn đang có khách) |
| `SanPham` | 12 | 6 thức ăn + 6 nước uống (10 đang bán, 2 ngừng bán) |
| `HoaDon` | 5 | 5 hóa đơn đã thanh toán từ 15–18/06/2026 |
| `ChiTietHoaDon` | 14 | Từng món trong mỗi hóa đơn, có thuộc tính Phần lớn / Lon |

## 🔑 Tài khoản đăng nhập (mật khẩu PLAIN-TEXT)

> ⚠️ **Quan trọng**: Phiên bản này **KHÔNG băm SHA-256** mật khẩu. Lý do:
> - Đồ án nhỏ, không cần bảo mật cao
> - Học sinh có thể **tự thêm/sửa tài khoản trực tiếp trong Access** mà không cần tính hash
> - Code C# đơn giản hơn (không còn file `MatKhauBaoMat.cs`)

| Tên đăng nhập | Mật khẩu (plain-text) | Vai trò |
|---------------|------------------------|---------|
| `admin` | `admin123` | QuanTri |
| `huy` | `huy123456` | QuanTri |
| `nhanvien1` | `123456` | NhanVien |

## ✏️ Hướng dẫn tự thêm/sửa dữ liệu trong Access

Vì mật khẩu và toàn bộ dữ liệu đều ở dạng **plain-text**, bạn có thể mở file
`QuanLyNhaHang.accdb` bằng Microsoft Access và tự thêm/sửa/xoá:

### Thêm tài khoản mới
1. Mở file `QuanLyNhaHang.accdb` bằng MS Access
2. Double-click vào bảng `NguoiDung`
3. Nhập dòng mới ở cuối bảng:
   - `Id`        : để trống (tự tăng)
   - `TenDangNhap`: ví dụ `quoca`
   - `MatKhau`    : ví dụ `quoca123` (viết thẳng, không cần băm)
   - `VaiTro`     : `QuanTri` hoặc `NhanVien`
   - `NgayTao`    : `01/07/2026`
4. Đóng Access → chạy lại app → đăng nhập bằng tài khoản mới

### Thêm món mới vào thực đơn
1. Mở bảng `SanPham` trong Access
2. Thêm dòng:
   - `Id`         : để trống
   - `TenSanPham` : `Bún chả Hà Nội`
   - `GiaCoBan`   : `55000`
   - `Loai`       : `ThucAn` hoặc `NuocUong` (**bắt buộc đúng chính xác**)
   - `DangBan`    : check = `True` (đang bán)
   - `HinhAnh`    : để trống hoặc dán URL ảnh
3. Mở app → vào trang **Thực đơn** → món mới đã có sẵn

### Thêm bàn mới
1. Mở bảng `Ban` trong Access
2. Thêm dòng: `Id` (để trống), `TenBan` (`Bàn 11`), `TrangThai` (`Trống`)
3. Mở app → vào trang **Sơ đồ bàn** → bàn mới đã xuất hiện

> 💡 **Mẹo**: Sau khi sửa file Access, hãy **đóng file lại trước khi chạy app**
> (Access khoá file khi đang mở, app sẽ không đọc được).

## 🚀 Cách mở file

### Cách 1: Mở bằng Microsoft Access (Windows)
1. Mở **Microsoft Access** (2016 trở lên)
2. File → Open → chọn `QuanLyNhaHang.accdb`
3. Nếu Access hỏi "Unsafe content" → bấm **Enable Content**
4. Xem các bảng ở tab **Tables** bên trái

### Cách 2: Mở bằng LibreOffice Base (Linux/Mac/Windows)
1. Mở **LibreOffice Base**
2. Chọn "Connect to an existing database" → loại "Microsoft Access 2007+"
3. Browse tới file `QuanLyNhaHang.accdb`
4. Bấm Finish

### Cách 3: Đọc bằng code Java (Jackcess)
```java
import io.github.spannm.jackcess.*;
Database db = DatabaseBuilder.open(new File("QuanLyNhaHang.accdb"));
Table ban = db.getTable("Ban");
for (Row r : ban) System.out.println(r);
```

## 🛠️ Tạo lại file .accdb từ đầu (nếu cần)

```bash
# Yêu cầu: Java 17+ đã cài
cd access_db
java --class-path libs/jackcess.jar:libs/commons-logging.jar tao_access.java
# → sẽ tạo lại file QuanLyNhaHang.accdb (overwrite)
```

## ✅ Kiểm tra tính hợp lệ

Sau khi mở file bằng Access, có thể chạy các truy vấn trong `QuanLyNhaHang_Access.sql` (mục PHẦN 3) để kiểm tra:

- Liệt kê bàn → 10 dòng
- Sản phẩm đang bán → 10 dòng
- Doanh thu theo ngày → 4 ngày, tổng 638.000đ
- Top món bán chạy → Phở bò, Coca Cola, Trà đá...
