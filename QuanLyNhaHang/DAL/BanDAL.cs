// ============================================================
// TẦNG DAL - BanDAL
// Implement interface IBanDAL: thao tác CRUD với bảng Ban
// ============================================================
using Microsoft.Data.Sqlite;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class BanDAL : IBanDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        // Lấy toàn bộ danh sách bàn
        public List<Ban> LayTatCa()
        {
            var dsBan = new List<Ban>();
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = "SELECT Id, TenBan, TrangThai FROM Ban ORDER BY Id";
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                dsBan.Add(new Ban
                {
                    Id = reader.GetInt32(0),
                    TenBan = reader.GetString(1),
                    TrangThai = reader.GetString(2)
                });
            }
            return dsBan;
        }

        // Lấy 1 bàn theo Id
        public Ban? LayTheoId(int id)
        {
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = "SELECT Id, TenBan, TrangThai FROM Ban WHERE Id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new Ban
                {
                    Id = reader.GetInt32(0),
                    TenBan = reader.GetString(1),
                    TrangThai = reader.GetString(2)
                };
            }
            return null;
        }

        // Thêm bàn mới
        public void Them(Ban ban)
        {
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = "INSERT INTO Ban (TenBan, TrangThai) VALUES (@ten, @tt)";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ten", ban.TenBan);
            cmd.Parameters.AddWithValue("@tt", ban.TrangThai);
            cmd.ExecuteNonQuery();
        }

        // Sửa thông tin bàn
        public void Sua(Ban ban)
        {
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = "UPDATE Ban SET TenBan = @ten, TrangThai = @tt WHERE Id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ten", ban.TenBan);
            cmd.Parameters.AddWithValue("@tt", ban.TrangThai);
            cmd.Parameters.AddWithValue("@id", ban.Id);
            cmd.ExecuteNonQuery();
        }

        // Xóa bàn (chỉ xóa được khi không có hóa đơn liên quan)
        public void Xoa(int id)
        {
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = "DELETE FROM Ban WHERE Id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // Cập nhật trạng thái bàn: "Trống" hoặc "Có khách"
        public void CapNhatTrangThai(int id, string trangThai)
        {
            using var conn = new SqliteConnection(_conn);
            conn.Open();

            string sql = "UPDATE Ban SET TrangThai = @tt WHERE Id = @id";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@tt", trangThai);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
