// ============================================================
// TẦNG DAL - BanDAL
// Implement interface IBanDAL: thao tác CRUD với bảng Ban
// Sử dụng try-catch-finally + conn.Close() trong finally
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
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
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
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTatCa Ban: {ex.Message}");
                throw;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Lấy 1 bàn theo Id
        public Ban? LayTheoId(int id)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
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
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTheoId Ban: {ex.Message}");
                throw;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Thêm bàn mới
        public void Them(Ban ban)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                // Kiểm tra trùng tên bàn
                string sqlCheck = "SELECT COUNT(*) FROM Ban WHERE TenBan = @ten";
                using var checkCmd = new SqliteCommand(sqlCheck, conn);
                checkCmd.Parameters.AddWithValue("@ten", ban.TenBan);
                long count = (long)(checkCmd.ExecuteScalar() ?? 0);
                if (count > 0)
                    throw new Exception("Tên bàn đã tồn tại! Vui lòng đặt tên khác.");

                string sql = "INSERT INTO Ban (TenBan, TrangThai) VALUES (@ten, @tt)";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ten", ban.TenBan);
                cmd.Parameters.AddWithValue("@tt", ban.TrangThai);
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw; // Re-throw để API xử lý
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Sửa thông tin bàn
        public void Sua(Ban ban)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                // Kiểm tra trùng tên bàn (trừ chính nó)
                string sqlCheck = "SELECT COUNT(*) FROM Ban WHERE TenBan = @ten AND Id != @id";
                using var checkCmd = new SqliteCommand(sqlCheck, conn);
                checkCmd.Parameters.AddWithValue("@ten", ban.TenBan);
                checkCmd.Parameters.AddWithValue("@id", ban.Id);
                long count = (long)(checkCmd.ExecuteScalar() ?? 0);
                if (count > 0)
                    throw new Exception("Tên bàn đã tồn tại! Vui lòng đặt tên khác.");

                string sql = "UPDATE Ban SET TenBan = @ten, TrangThai = @tt WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ten", ban.TenBan);
                cmd.Parameters.AddWithValue("@tt", ban.TrangThai);
                cmd.Parameters.AddWithValue("@id", ban.Id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Xóa bàn (chỉ xóa được khi không có hóa đơn liên quan)
        public void Xoa(int id)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = "DELETE FROM Ban WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Cập nhật trạng thái bàn: "Trống" hoặc "Có khách"
        public void CapNhatTrangThai(int id, string trangThai)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = "UPDATE Ban SET TrangThai = @tt WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tt", trangThai);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }
    }
}
