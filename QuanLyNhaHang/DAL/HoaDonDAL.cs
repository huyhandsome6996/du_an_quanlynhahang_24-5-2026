// ============================================================
// TẦNG DAL - HoaDonDAL
// Implement interface IHoaDonDAL: thao tác với bảng HoaDon
// ============================================================
using Microsoft.Data.Sqlite;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class HoaDonDAL : IHoaDonDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        // Lấy toàn bộ lịch sử hóa đơn (kèm tên bàn)
        public List<HoaDon> LayTatCa()
        {
            var ds = new List<HoaDon>();
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = @"
                SELECT hd.Id, hd.BanId, b.TenBan, hd.ThoiGianTao,
                       hd.ThoiGianThanhToan, hd.TongTien, hd.TrangThai
                FROM HoaDon hd
                INNER JOIN Ban b ON hd.BanId = b.Id
                ORDER BY hd.ThoiGianTao DESC";

            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                ds.Add(DocTuReader(reader));

            return ds;
        }

        // Lấy 1 hóa đơn theo Id
        public HoaDon? LayTheoId(int id)
        {
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = @"
                SELECT hd.Id, hd.BanId, b.TenBan, hd.ThoiGianTao,
                       hd.ThoiGianThanhToan, hd.TongTien, hd.TrangThai
                FROM HoaDon hd
                INNER JOIN Ban b ON hd.BanId = b.Id
                WHERE hd.Id = @id";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
                return DocTuReader(reader);

            return null;
        }

        // Lấy hóa đơn chưa thanh toán của 1 bàn cụ thể
        public HoaDon? LayHoaDonChuaThanhToanTheoBan(int banId)
        {
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = @"
                SELECT hd.Id, hd.BanId, b.TenBan, hd.ThoiGianTao,
                       hd.ThoiGianThanhToan, hd.TongTien, hd.TrangThai
                FROM HoaDon hd
                INNER JOIN Ban b ON hd.BanId = b.Id
                WHERE hd.BanId = @banId AND hd.TrangThai = 'Chưa thanh toán'
                LIMIT 1";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@banId", banId);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
                return DocTuReader(reader);

            return null;
        }

        // Tạo hóa đơn mới, trả về Id vừa tạo
        public int Them(HoaDon hoaDon)
        {
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = @"
                INSERT INTO HoaDon (BanId, ThoiGianTao, TongTien, TrangThai)
                VALUES (@banId, @tgt, @tongTien, @tt);
                SELECT last_insert_rowid();";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@banId", hoaDon.BanId);
            cmd.Parameters.AddWithValue("@tgt", hoaDon.ThoiGianTao.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@tongTien", hoaDon.TongTien);
            cmd.Parameters.AddWithValue("@tt", hoaDon.TrangThai);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Cập nhật tổng tiền sau khi thêm/xóa món
        public void CapNhatTongTien(int hoaDonId, decimal tongTien)
        {
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = "UPDATE HoaDon SET TongTien = @tongTien WHERE Id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tongTien", tongTien);
            cmd.Parameters.AddWithValue("@id", hoaDonId);
            cmd.ExecuteNonQuery();
        }

        // Thanh toán hóa đơn: cập nhật thời gian và trạng thái
        public void ThanhToan(int hoaDonId)
        {
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = @"
                UPDATE HoaDon
                SET ThoiGianThanhToan = @tgtt, TrangThai = 'Đã thanh toán'
                WHERE Id = @id";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tgtt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@id", hoaDonId);
            cmd.ExecuteNonQuery();
        }

        // Helper: đọc dữ liệu từ reader sang object HoaDon
        private HoaDon DocTuReader(SqliteDataReader reader)
        {
            return new HoaDon
            {
                Id = reader.GetInt32(0),
                BanId = reader.GetInt32(1),
                TenBan = reader.GetString(2),
                ThoiGianTao = DateTime.Parse(reader.GetString(3)),
                ThoiGianThanhToan = reader.IsDBNull(4)
                    ? null
                    : DateTime.Parse(reader.GetString(4)),
                TongTien = reader.GetDecimal(5),
                TrangThai = reader.GetString(6)
            };
        }
    }
}
