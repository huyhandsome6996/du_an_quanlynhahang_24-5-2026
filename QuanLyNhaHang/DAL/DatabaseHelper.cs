// ============================================================
// TẦNG DAL - DatabaseHelper
// Khởi tạo SQLite và tạo các bảng nếu chưa tồn tại
// V2: Thêm bảng NguyenLieu, KhoLog; Thêm cột VAT, GiamGia, PhuongThucThanhToan, TrangThaiMon
// ============================================================
using Microsoft.Data.Sqlite;

namespace QuanLyNhaHang.DAL
{
    public static class DatabaseHelper
    {
        private static readonly string _connectionString =
            "Data Source=nha_hang.db;";

        public static string ConnectionString => _connectionString;

        public static void KhoiTaoCSDL()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            string sql = @"
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS Ban (
                    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenBan    TEXT    NOT NULL,
                    TrangThai TEXT    NOT NULL DEFAULT 'Trống'
                );

                CREATE TABLE IF NOT EXISTS SanPham (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenSanPham  TEXT    NOT NULL,
                    GiaCoBan    DECIMAL NOT NULL CHECK(GiaCoBan >= 0),
                    Loai        TEXT    NOT NULL,
                    DangBan     INTEGER NOT NULL DEFAULT 1,
                    HinhAnh     TEXT    NULL
                );

                CREATE TABLE IF NOT EXISTS HoaDon (
                    Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                    BanId              INTEGER NOT NULL,
                    ThoiGianTao        DATETIME NOT NULL,
                    ThoiGianThanhToan  DATETIME NULL,
                    TongTien           DECIMAL  NOT NULL DEFAULT 0,
                    TrangThai          TEXT     NOT NULL DEFAULT 'Chưa thanh toán',
                    VAT                DECIMAL  NOT NULL DEFAULT 0,
                    GiamGia            DECIMAL  NOT NULL DEFAULT 0,
                    PhuongThucThanhToan TEXT    NOT NULL DEFAULT 'TienMat',
                    FOREIGN KEY (BanId) REFERENCES Ban(Id)
                );

                CREATE TABLE IF NOT EXISTS ChiTietHoaDon (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    HoaDonId        INTEGER NOT NULL,
                    SanPhamId       INTEGER NOT NULL,
                    SoLuong         INTEGER NOT NULL CHECK(SoLuong > 0),
                    DonGiaBan       DECIMAL NOT NULL,
                    ThuocTinhThem   TEXT    NULL,
                    ThanhTien       DECIMAL NOT NULL,
                    TrangThaiMon    TEXT    NOT NULL DEFAULT 'DangCho',
                    FOREIGN KEY (HoaDonId) REFERENCES HoaDon(Id),
                    FOREIGN KEY (SanPhamId) REFERENCES SanPham(Id)
                );

                CREATE TABLE IF NOT EXISTS NguoiDung (
                    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenDangNhap   TEXT    NOT NULL UNIQUE,
                    MatKhauHash   TEXT    NOT NULL,
                    VaiTro        TEXT    NOT NULL DEFAULT 'QuanTri',
                    NgayTao       DATETIME NOT NULL
                );

                CREATE TABLE IF NOT EXISTS NguyenLieu (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenNguyenLieu   TEXT    NOT NULL UNIQUE,
                    DonVi           TEXT    NOT NULL,
                    SoLuongTon      DECIMAL NOT NULL DEFAULT 0,
                    MucToiThieu     DECIMAL NOT NULL DEFAULT 0,
                    GhiChu          TEXT    NULL
                );

                CREATE TABLE IF NOT EXISTS KhoLog (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    Loai            TEXT    NOT NULL,
                    NguyenLieuId    INTEGER NOT NULL,
                    SoLuong         DECIMAL NOT NULL,
                    DonGia          DECIMAL NOT NULL DEFAULT 0,
                    ThoiGian        DATETIME NOT NULL,
                    LyDo            TEXT    NULL,
                    FOREIGN KEY (NguyenLieuId) REFERENCES NguyenLieu(Id)
                );
            ";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();

            // Thêm cột mới vào bảng cũ nếu chưa tồn tại (backward compatibility)
            string[] alterCommands = new[]
            {
                "ALTER TABLE SanPham ADD COLUMN HinhAnh TEXT NULL;",
                "ALTER TABLE HoaDon ADD COLUMN VAT DECIMAL NOT NULL DEFAULT 0;",
                "ALTER TABLE HoaDon ADD COLUMN GiamGia DECIMAL NOT NULL DEFAULT 0;",
                "ALTER TABLE HoaDon ADD COLUMN PhuongThucThanhToan TEXT NOT NULL DEFAULT 'TienMat';",
                "ALTER TABLE ChiTietHoaDon ADD COLUMN TrangThaiMon TEXT NOT NULL DEFAULT 'DangCho';"
            };
            foreach (var alterSql in alterCommands)
            {
                try
                {
                    using var alterCmd = new SqliteCommand(alterSql, conn);
                    alterCmd.ExecuteNonQuery();
                }
                catch { /* Cột đã tồn tại */ }
            }

            ThemDuLieuMau(conn);
            Console.WriteLine("✅ CSDL đã sẵn sàng: nha_hang.db");
        }

        private static void ThemDuLieuMau(SqliteConnection conn)
        {
            using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Ban", conn);
            long soBan = (long)(checkCmd.ExecuteScalar() ?? 0);
            if (soBan > 0) return;

            string sqlMau = @"
                INSERT INTO Ban (TenBan) VALUES
                    ('Bàn 1'), ('Bàn 2'), ('Bàn 3'),
                    ('Bàn 4'), ('Bàn VIP 1'), ('Phòng Lạnh 1');

                INSERT INTO SanPham (TenSanPham, GiaCoBan, Loai, DangBan, HinhAnh) VALUES
                    ('Lẩu Thái hải sản',    350000, 'ThucAn',  1, 'https://images.unsplash.com/photo-1547928576-a4a3323dce9a?w=400'),
                    ('Bò bít tết',          200000, 'ThucAn',  1, 'https://images.unsplash.com/photo-1544025162-d76694265947?w=400'),
                    ('Gà nướng mật ong',    180000, 'ThucAn',  1, 'https://images.unsplash.com/photo-1598515214211-89d3e73ae83b?w=400'),
                    ('Cơm chiên dương châu',  80000, 'ThucAn',  1, 'https://images.unsplash.com/photo-1603133872878-6967b6827050?w=400'),
                    ('Salad Caesar',          75000, 'ThucAn',  1, 'https://images.unsplash.com/photo-1550304943-4f24f54ddde9?w=400'),
                    ('Nước ép dưa hấu',      40000, 'NuocUong', 1, 'https://images.unsplash.com/photo-1589733901241-5e514f26b437?w=400'),
                    ('Trà đào cam sả',       45000, 'NuocUong', 1, 'https://images.unsplash.com/photo-1556881286-fc6915169721?w=400'),
                    ('Coca Cola',            30000, 'NuocUong', 1, 'https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=400'),
                    ('Bia Tiger',            35000, 'NuocUong', 1, 'https://images.unsplash.com/photo-1608270586620-248524c67de9?w=400'),
                    ('Nước suối Lavie',      15000, 'NuocUong', 1, 'https://images.unsplash.com/photo-1560023907-5f67f61f904d?w=400');

                INSERT INTO NguyenLieu (TenNguyenLieu, DonVi, SoLuongTon, MucToiThieu, GhiChu) VALUES
                    ('Thịt bò', 'kg', 50, 10, 'Nguyên liệu chính'),
                    ('Thịt gà', 'kg', 40, 8, 'Nguyên liệu chính'),
                    ('Hải sản tổng hợp', 'kg', 20, 5, 'Cá, tôm, mực'),
                    ('Rau sạch', 'kg', 30, 5, 'Rau ăn kèm, salad'),
                    ('Gạo', 'kg', 100, 20, 'Gạo Jasmine'),
                    ('Dầu ăn', 'lít', 20, 5, 'Dầu thực vật'),
                    ('Nước mắm', 'lít', 10, 3, 'Nước mắm nhĩ'),
                    ('Đường', 'kg', 15, 5, 'Đường trắng'),
                    ('Coca Cola', 'thùng', 10, 3, '24 lon/thùng'),
                    ('Bia Tiger', 'thùng', 8, 2, '24 lon/thùng'),
                    ('Nước suối', 'thùng', 12, 3, '24 chai/thùng'),
                    ('Trà đào', 'thùng', 5, 2, '6 bình/thùng');
            ";

            using var insertCmd = new SqliteCommand(sqlMau, conn);
            insertCmd.ExecuteNonQuery();
            Console.WriteLine("✅ Đã thêm dữ liệu mẫu (6 bàn, 10 món, 12 nguyên liệu)");
        }
    }
}
