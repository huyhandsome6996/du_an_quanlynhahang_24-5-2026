// ============================================================
// TẦNG DAL - NguoiDungDAL
// Implement interface INguoiDungDAL: thao tác với bảng NguoiDung
// Mật khẩu được băm SHA256 trước khi lưu vào CSDL
// ============================================================
using Microsoft.Data.Sqlite;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class NguoiDungDAL : INguoiDungDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        // Kiểm tra hệ thống đã có người dùng nào chưa
        public bool KiemTraCoNguoiDung()
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = "SELECT COUNT(*) FROM NguoiDung";
                using var cmd = new SqliteCommand(sql, conn);
                long count = (long)(cmd.ExecuteScalar() ?? 0);
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi kiểm tra người dùng: {ex.Message}");
                return false;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Lấy người dùng theo tên đăng nhập
        public NguoiDung? LayTheoTenDangNhap(string tenDangNhap)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = "SELECT Id, TenDangNhap, MatKhauHash, VaiTro, NgayTao FROM NguoiDung WHERE TenDangNhap = @ten";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ten", tenDangNhap);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new NguoiDung
                    {
                        Id = reader.GetInt32(0),
                        TenDangNhap = reader.GetString(1),
                        MatKhauHash = reader.GetString(2),
                        VaiTro = reader.GetString(3),
                        NgayTao = DateTime.Parse(reader.GetString(4))
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lấy người dùng: {ex.Message}");
                return null;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Thêm người dùng mới (mật khẩu đã được băm từ trước)
        public void Them(NguoiDung nguoiDung)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                // Kiểm tra trùng tên đăng nhập trước khi thêm
                string sqlCheck = "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = @ten";
                using var checkCmd = new SqliteCommand(sqlCheck, conn);
                checkCmd.Parameters.AddWithValue("@ten", nguoiDung.TenDangNhap);
                long count = (long)(checkCmd.ExecuteScalar() ?? 0);
                if (count > 0)
                    throw new Exception("Tên đăng nhập đã tồn tại! Vui lòng chọn tên khác.");

                string sql = @"INSERT INTO NguoiDung (TenDangNhap, MatKhauHash, VaiTro, NgayTao) 
                               VALUES (@ten, @hash, @vaiTro, @ngay)";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ten", nguoiDung.TenDangNhap);
                cmd.Parameters.AddWithValue("@hash", nguoiDung.MatKhauHash);
                cmd.Parameters.AddWithValue("@vaiTro", nguoiDung.VaiTro);
                cmd.Parameters.AddWithValue("@ngay", nguoiDung.NgayTao.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw; // Re-throw để Program.cs xử lý
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }
    }
}
