-- ============================================================
-- SCRIPT SQL TẠO CƠ SỞ DỮ LIỆU ACCESS CHO ĐỒ ÁN QUẢN LÝ NHÀ HÀNG
-- Môn: Lập trình hướng đối tượng
-- Đề tài: Hệ thống Quản lý Nhà hàng
-- ============================================================
--
-- Cách sử dụng:
-- 1. Mở Microsoft Access → New → Blank Database → đặt tên QuanLyNhaHang.accdb
-- 2. Vào tab "Database Tools" → "Visual Basic" → Immediate Window (Ctrl+G)
-- 3. Dán script dưới đây (chia thành từng phần) và nhấn Enter để chạy
-- 4. Hoặc: dùng file QuanLyNhaHang.accdb đã được tạo sẵn (file .accdb
--    trong cùng folder này, tạo bằng Jackcess 5.1.0)
--
-- Lưu ý: Access SQL có vài khác biệt so với SQL Server/MySQL:
--   - Dùng AUTOINCREMENT thay vì AUTO_INCREMENT
--   - Dùng LONGTEXT (hoặc MEMO) cho văn bản dài
--   - Dùng MONEY / CURRENCY cho tiền tệ
--   - Dùng SHORT DATE_TIME cho ngày giờ
--   - Dùng YESNO cho boolean
--   - Quan hệ (Relationship) phải tạo qua DAO/ADO hoặc UI, không thể qua DDL thuần
-- ============================================================

-- ============================================================
-- PHẦN 1: TẠO CÁC BẢNG (DDL)
-- ============================================================

-- Bảng 1: NguoiDung (Tài khoản đăng nhập)
-- Mật khẩu lưu PLAIN-TEXT (không băm SHA-256) — đồ án đơn giản hoá
CREATE TABLE NguoiDung (
    Id              AUTOINCREMENT PRIMARY KEY,
    TenDangNhap     VARCHAR(50)  NOT NULL UNIQUE,
    MatKhau         VARCHAR(100) NOT NULL,    -- plain-text, không băm
    VaiTro          VARCHAR(20)  NOT NULL DEFAULT 'QuanTri',
    NgayTao         DATETIME     NOT NULL
);

-- Bảng 2: Ban (Bàn ăn)
CREATE TABLE Ban (
    Id              AUTOINCREMENT PRIMARY KEY,
    TenBan          VARCHAR(20)  NOT NULL,
    TrangThai       VARCHAR(20)  NOT NULL DEFAULT 'Trống'
);

-- Bảng 3: SanPham (Sản phẩm - thức ăn + nước uống)
CREATE TABLE SanPham (
    Id              AUTOINCREMENT PRIMARY KEY,
    TenSanPham      VARCHAR(100) NOT NULL,
    GiaCoBan        MONEY        NOT NULL,
    Loai            VARCHAR(20)  NOT NULL,  -- 'ThucAn' hoặc 'NuocUong'
    DangBan         YESNO        NOT NULL DEFAULT -1,  -- True = đang bán
    HinhAnh         VARCHAR(255) NULL
);

-- Bảng 4: HoaDon (Hóa đơn)
CREATE TABLE HoaDon (
    Id                  AUTOINCREMENT PRIMARY KEY,
    BanId               LONG        NOT NULL,
    ThoiGianTao         DATETIME    NOT NULL,
    ThoiGianThanhToan   DATETIME    NULL,
    TongTien            MONEY       NOT NULL DEFAULT 0,
    TrangThai           VARCHAR(30) NOT NULL DEFAULT 'Chưa thanh toán',
    VAT                 MONEY       NOT NULL DEFAULT 0,
    GiamGia             MONEY       NOT NULL DEFAULT 0,
    PhuongThucThanhToan VARCHAR(20) NOT NULL DEFAULT 'TienMat',
    CONSTRAINT FK_HoaDon_Ban FOREIGN KEY (BanId) REFERENCES Ban(Id)
        ON UPDATE CASCADE ON DELETE CASCADE
);

-- Bảng 5: ChiTietHoaDon (Chi tiết từng món trong hóa đơn)
CREATE TABLE ChiTietHoaDon (
    Id              AUTOINCREMENT PRIMARY KEY,
    HoaDonId        LONG        NOT NULL,
    SanPhamId       LONG        NOT NULL,
    SoLuong         SHORT       NOT NULL,
    DonGiaBan       MONEY       NOT NULL,
    ThuocTinhThem   VARCHAR(100) NULL,
    ThanhTien       MONEY       NOT NULL,
    TrangThaiMon    VARCHAR(30) NOT NULL DEFAULT 'DangCho',
    CONSTRAINT FK_CTHD_HoaDon  FOREIGN KEY (HoaDonId)  REFERENCES HoaDon(Id)
        ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT FK_CTHD_SanPham FOREIGN KEY (SanPhamId) REFERENCES SanPham(Id)
        ON UPDATE CASCADE ON DELETE CASCADE
);

-- ============================================================
-- PHẦN 2: DỮ LIỆU MẪU (DML)
-- ============================================================

-- 2.1. 3 Tài khoản (mật khẩu PLAIN-TEXT — đơn giản hoá cho đồ án nhỏ)
-- admin     / admin123   → QuanTri
-- nhanvien1 / 123456     → NhanVien
-- huy       / huy123456  → QuanTri
-- Bạn có thể tự thêm/sửa trực tiếp trong Access mà không cần tính hash.
INSERT INTO NguoiDung (Id, TenDangNhap, MatKhau, VaiTro, NgayTao) VALUES
    (1, 'admin',     'admin123',  'QuanTri',  #2026-07-01 08:00:00#),
    (2, 'nhanvien1', '123456',    'NhanVien', #2026-07-01 08:00:00#),
    (3, 'huy',       'huy123456', 'QuanTri',  #2026-07-01 08:00:00#);

-- 2.2. 10 Bàn
INSERT INTO Ban (Id, TenBan, TrangThai) VALUES
    (1, 'Bàn 1',  'Trống'),
    (2, 'Bàn 2',  'Có khách'),
    (3, 'Bàn 3',  'Trống'),
    (4, 'Bàn 4',  'Có khách'),
    (5, 'Bàn 5',  'Trống'),
    (6, 'Bàn 6',  'Trống'),
    (7, 'Bàn 7',  'Trống'),
    (8, 'Bàn 8',  'Có khách'),
    (9, 'Bàn 9',  'Trống'),
    (10,'Bàn 10', 'Trống');

-- 2.3. 12 Sản phẩm (6 Thức ăn + 6 Nước uống)
INSERT INTO SanPham (Id, TenSanPham, GiaCoBan, Loai, DangBan, HinhAnh) VALUES
    (1,  'Cơm gà xối mỡ',     45000, 'ThucAn',   True,  '/img/com_ga.jpg'),
    (2,  'Mỳ Quảng',          40000, 'ThucAn',   True,  '/img/my_quang.jpg'),
    (3,  'Bún bò Huế',        50000, 'ThucAn',   True,  '/img/bun_bo.jpg'),
    (4,  'Phở bò',            55000, 'ThucAn',   True,  '/img/pho_bo.jpg'),
    (5,  'Cơm sườn nướng',    60000, 'ThucAn',   True,  '/img/com_suon.jpg'),
    (6,  'Gỏi cuốn tôm',      35000, 'ThucAn',   False, '/img/goi_cuon.jpg'),
    (7,  'Cà phê sữa',        25000, 'NuocUong', True,  '/img/ca_phe_sua.jpg'),
    (8,  'Trà sữa trân châu', 45000, 'NuocUong', True,  '/img/tra_sua.jpg'),
    (9,  'Nước cam ép',       35000, 'NuocUong', True,  '/img/nuoc_cam.jpg'),
    (10, 'Coca Cola',         15000, 'NuocUong', True,  '/img/coca.jpg'),
    (11, 'Trà đá',            10000, 'NuocUong', True,  '/img/tra_da.jpg'),
    (12, 'Sinh tố xoài',      40000, 'NuocUong', False, '/img/sinh_to_xoai.jpg');

-- 2.4. 5 Hóa đơn mẫu (đã thanh toán)
INSERT INTO HoaDon (Id, BanId, ThoiGianTao, ThoiGianThanhToan, TongTien, TrangThai, VAT, GiamGia, PhuongThucThanhToan) VALUES
    (1, 1, #2026-06-15 11:30:00#, #2026-06-15 12:00:00#, 118000, 'Đã thanh toán', 10727, 0, 'TienMat'),
    (2, 2, #2026-06-16 18:00:00#, #2026-06-16 19:30:00#, 125000, 'Đã thanh toán', 11364, 0, 'The'),
    (3, 4, #2026-06-17 19:00:00#, #2026-06-17 20:00:00#, 155000, 'Đã thanh toán', 14091, 0, 'QR'),
    (4, 8, #2026-06-18 12:00:00#, #2026-06-18 13:00:00#, 125000, 'Đã thanh toán', 11364, 0, 'TienMat'),
    (5, 3, #2026-06-15 11:30:00#, #2026-06-16 18:00:00#, 115000, 'Đã thanh toán', 10455, 0, 'ChuyenKhoan');

-- 2.5. 14 Chi tiết hóa đơn
INSERT INTO ChiTietHoaDon (Id, HoaDonId, SanPhamId, SoLuong, DonGiaBan, ThuocTinhThem, ThanhTien, TrangThaiMon) VALUES
    -- HD1: Cơm gà (Phần lớn +50k) + Coca Lon (x1.2) + Trà đá
    (1, 1, 1,  1, 95000, 'Phần lớn', 95000, 'DaPhucVu'),
    (2, 1, 9,  1, 15000, 'Lon',      18000, 'DaPhucVu'),
    (3, 1, 10, 1, 10000, 'Ly',       10000, 'DaPhucVu'),
    -- HD2: Cà phê + Phở x2
    (4, 2, 7,  1, 25000, 'Ly',        25000, 'DaPhucVu'),
    (5, 2, 4,  2, 55000, 'Phần thường',110000,'DaPhucVu'),
    -- HD3: Phở + Trà sữa + Coca Lon
    (6, 3, 4,  1, 55000, 'Phần thường',55000, 'DaPhucVu'),
    (7, 3, 8,  1, 45000, 'Ly',        45000, 'DaPhucVu'),
    (8, 3, 9,  1, 15000, 'Lon',       18000, 'DaPhucVu'),
    -- HD4: Cơm gà Phần lớn + Coca Lon + Trà đá
    (9, 4, 1,  1, 95000, 'Phần lớn', 95000, 'DaPhucVu'),
    (10,4, 9,  1, 15000, 'Lon',      18000, 'DaPhucVu'),
    (11,4, 10, 1, 10000, 'Ly',       10000, 'DaPhucVu'),
    -- HD5: Mỳ Quảng Phần lớn + Nước cam + Trà sữa
    (12,5, 2,  1, 90000, 'Phần lớn', 90000, 'DaPhucVu'),
    (13,5, 9,  1, 35000, 'Ly',       35000, 'DaPhucVu'),
    (14,5, 8,  1, 45000, 'Ly',       45000, 'DaPhucVu');

-- ============================================================
-- PHẦN 3: CÁC TRUY VẤN THAM KHẢO (SELECT)
-- ============================================================

-- 3.1. Liệt kê tất cả bàn kèm trạng thái
SELECT * FROM Ban ORDER BY Id;

-- 3.2. Liệt kê sản phẩm theo loại
SELECT * FROM SanPham WHERE Loai = 'ThucAn' AND DangBan = True ORDER BY TenSanPham;

-- 3.3. Doanh thu theo ngày
SELECT
    DateValue(ThoiGianThanhToan) AS Ngay,
    Count(*)                     AS SoDon,
    Sum(TongTien)                AS DoanhThu
FROM HoaDon
WHERE TrangThai = 'Đã thanh toán'
GROUP BY DateValue(ThoiGianThanhToan)
ORDER BY Ngay DESC;

-- 3.4. Chi tiết hóa đơn theo Id hóa đơn
SELECT
    CTHD.Id,
    SP.TenSanPham,
    CTHD.SoLuong,
    CTHD.DonGiaBan,
    CTHD.ThuocTinhThem,
    CTHD.ThanhTien
FROM ChiTietHoaDon AS CTHD
INNER JOIN SanPham AS SP ON CTHD.SanPhamId = SP.Id
WHERE CTHD.HoaDonId = 1;

-- 3.5. Top món bán chạy
SELECT TOP 10
    SP.TenSanPham,
    Sum(CTHD.SoLuong) AS TongSoLuong,
    Sum(CTHD.ThanhTien) AS TongDoanhThu
FROM ChiTietHoaDon AS CTHD
INNER JOIN SanPham AS SP ON CTHD.SanPhamId = SP.Id
GROUP BY SP.TenSanPham
ORDER BY TongSoLuong DESC;

-- 3.6. Doanh thu theo phương thức thanh toán
SELECT
    PhuongThucThanhToan,
    Count(*) AS SoDon,
    Sum(TongTien) AS TongTien
FROM HoaDon
WHERE TrangThai = 'Đã thanh toán'
GROUP BY PhuongThucThanhToan;
