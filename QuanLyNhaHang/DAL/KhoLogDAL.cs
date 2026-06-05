// ============================================================
// TẦNG DAL - KhoLogDAL
// Nhật ký nhập/xuất kho nguyên liệu
// ============================================================
using Microsoft.Data.Sqlite;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class KhoLogDAL : IKhoLogDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        public List<KhoLog> LayTatCa()
        {
            var ds = new List<KhoLog>();
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = @"SELECT k.Id, k.Loai, k.NguyenLieuId, n.TenNguyenLieu,
                               k.SoLuong, k.DonGia, k.ThoiGian, k.LyDo
                               FROM KhoLog k
                               INNER JOIN NguyenLieu n ON k.NguyenLieuId = n.Id
                               ORDER BY k.ThoiGian DESC";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    ds.Add(DocTuReader(reader));
                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTatCa KhoLog: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        public List<KhoLog> LayTheoNguyenLieu(int nguyenLieuId)
        {
            var ds = new List<KhoLog>();
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = @"SELECT k.Id, k.Loai, k.NguyenLieuId, n.TenNguyenLieu,
                               k.SoLuong, k.DonGia, k.ThoiGian, k.LyDo
                               FROM KhoLog k
                               INNER JOIN NguyenLieu n ON k.NguyenLieuId = n.Id
                               WHERE k.NguyenLieuId = @nlId
                               ORDER BY k.ThoiGian DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nlId", nguyenLieuId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    ds.Add(DocTuReader(reader));
                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTheoNguyenLieu KhoLog: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        public void Them(KhoLog log)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = @"INSERT INTO KhoLog (Loai, NguyenLieuId, SoLuong, DonGia, ThoiGian, LyDo)
                               VALUES (@loai, @nlId, @soLuong, @donGia, @thoiGian, @lyDo)";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@loai", log.Loai);
                cmd.Parameters.AddWithValue("@nlId", log.NguyenLieuId);
                cmd.Parameters.AddWithValue("@soLuong", log.SoLuong);
                cmd.Parameters.AddWithValue("@donGia", log.DonGia);
                cmd.Parameters.AddWithValue("@thoiGian", log.ThoiGian.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@lyDo", (object?)log.LyDo ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi Them KhoLog: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        private KhoLog DocTuReader(SqliteDataReader reader)
        {
            return new KhoLog
            {
                Id = reader.GetInt32(0),
                Loai = reader.GetString(1),
                NguyenLieuId = reader.GetInt32(2),
                TenNguyenLieu = reader.GetString(3),
                SoLuong = reader.GetDecimal(4),
                DonGia = reader.GetDecimal(5),
                ThoiGian = DateTime.Parse(reader.GetString(6)),
                LyDo = reader.IsDBNull(7) ? null : reader.GetString(7)
            };
        }
    }
}
