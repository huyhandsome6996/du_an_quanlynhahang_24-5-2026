// ============================================================
// TẦNG DAL - DatabaseHelper
// Khởi tạo SQLite và tạo các bảng nếu chưa tồn tại
// ============================================================
using Microsoft.Data.Sqlite;

namespace QuanLyNhaHang.DAL
{
    public static class DatabaseHelper
    {
        // Đường dẫn file SQLite (đặt cùng thư mục chạy app)
        private static readonly string _connectionString =
            "Data Source=nha_hang.db;";

        public static string ConnectionString => _connectionString;

        /// <summary>
        /// Tạo tất cả bảng trong SQLite khi app khởi động.
        /// Dùng "CREATE TABLE IF NOT EXISTS" để không mất data cũ.
        /// </summary>
        public static void KhoiTaoCSDL()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            string sql = @"
                -- Bật hỗ trợ khóa ngoại trong SQLite
                PRAGMA foreign_keys = ON;

                -- Bảng BÀN
                CREATE TABLE IF NOT EXISTS Ban (
                    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenBan    TEXT    NOT NULL,
                    TrangThai TEXT    NOT NULL DEFAULT 'Trống'
                );

                -- Bảng SẢN PHẨM (Menu)
                CREATE TABLE IF NOT EXISTS SanPham (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenSanPham  TEXT    NOT NULL,
                    GiaCoBan    DECIMAL NOT NULL CHECK(GiaCoBan >= 0),
                    Loai        TEXT    NOT NULL,   -- 'ThucAn' hoặc 'NuocUong'
                    DangBan     INTEGER NOT NULL DEFAULT 1,  -- 1=Đang bán, 0=Ngừng bán
                    HinhAnh     TEXT    NULL
                );

                -- Bảng HÓA ĐƠN
                CREATE TABLE IF NOT EXISTS HoaDon (
                    Id                 INTEGER PRIMARY KEY AUTOINCREMENT,
                    BanId              INTEGER NOT NULL,
                    ThoiGianTao        DATETIME NOT NULL,
                    ThoiGianThanhToan  DATETIME NULL,
                    TongTien           DECIMAL  NOT NULL DEFAULT 0,
                    TrangThai          TEXT     NOT NULL DEFAULT 'Chưa thanh toán',
                    FOREIGN KEY (BanId) REFERENCES Ban(Id)
                );

                -- Bảng CHI TIẾT HÓA ĐƠN
                CREATE TABLE IF NOT EXISTS ChiTietHoaDon (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    HoaDonId        INTEGER NOT NULL,
                    SanPhamId       INTEGER NOT NULL,
                    SoLuong         INTEGER NOT NULL CHECK(SoLuong > 0),
                    DonGiaBan       DECIMAL NOT NULL,
                    ThuocTinhThem   TEXT    NULL,
                    ThanhTien       DECIMAL NOT NULL,
                    FOREIGN KEY (HoaDonId) REFERENCES HoaDon(Id),
                    FOREIGN KEY (SanPhamId) REFERENCES SanPham(Id)
                );

                -- Bảng NGƯỜI DÙNG (Đăng nhập / Đăng ký)
                CREATE TABLE IF NOT EXISTS NguoiDung (
                    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                    TenDangNhap   TEXT    NOT NULL UNIQUE,
                    MatKhauHash   TEXT    NOT NULL,  -- Mật khẩu được băm SHA256
                    VaiTro        TEXT    NOT NULL DEFAULT 'QuanTri',
                    NgayTao       DATETIME NOT NULL
                );
            ";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();

            // Thêm cột HinhAnh vào bảng SanPham nếu đã tồn tại bảng cũ
            try
            {
                using var alterColCmd = new SqliteCommand("ALTER TABLE SanPham ADD COLUMN HinhAnh TEXT NULL;", conn);
                alterColCmd.ExecuteNonQuery();
            }
            catch { /* Cột đã tồn tại hoặc bảng mới đã có sẵn */ }

            // Thêm dữ liệu mẫu nếu bảng còn trống
            ThemDuLieuMau(conn);

            Console.WriteLine("✅ CSDL đã sẵn sàng: nha_hang.db");
        }

        /// <summary>
        /// Thêm dữ liệu mẫu ban đầu để demo ứng dụng.
        /// </summary>
        private static void ThemDuLieuMau(SqliteConnection conn)
        {
            // Kiểm tra nếu đã có bàn thì không thêm nữa
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
                    ('Nước suối Lavie',      15000, 'NuocUong', 1, 'https://images.unsplash.com/photo-1560023907-5f67b61f904d?w=400');
            ";

            using var insertCmd = new SqliteCommand(sqlMau, conn);
            insertCmd.ExecuteNonQuery();
            Console.WriteLine("✅ Đã thêm dữ liệu mẫu (6 bàn, 10 món)");
        }
    }
}
