// ============================================================
// TẦNG DAL - SanPhamDAL
// Implement interface ISanPhamDAL: thao tác CRUD với bảng SanPham
// Thể hiện Đa hình: Dựa vào cột 'Loai' để tạo đúng object ThucAn/NuocUong
// Sử dụng try-catch-finally + conn.Close() trong finally
// ============================================================
using Microsoft.Data.Sqlite;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class SanPhamDAL : ISanPhamDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        // -------------------------------------------------------
        // ĐA HÌNH: Đọc từ DB và tạo đúng loại object (ThucAn/NuocUong)
        // -------------------------------------------------------
        private SanPham DocTuReader(SqliteDataReader reader)
        {
            string loai = reader.GetString(4);

            // Factory Pattern kết hợp Đa hình: tạo object phù hợp theo Loai
            SanPham sp = loai == "ThucAn" ? new ThucAn() : new NuocUong();

            sp.Id = reader.GetInt32(0);
            sp.TenSanPham = reader.GetString(1);
            sp.GiaCoBan = reader.GetDecimal(2);
            sp.DangBan = reader.GetInt32(3) == 1;
            sp.Loai = loai;
            sp.HinhAnh = reader.IsDBNull(5) ? null : reader.GetString(5);

            return sp;
        }

        // Lấy toàn bộ sản phẩm
        public List<SanPham> LayTatCa()
        {
            var ds = new List<SanPham>();
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = "SELECT Id, TenSanPham, GiaCoBan, DangBan, Loai, HinhAnh FROM SanPham ORDER BY Loai, TenSanPham";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                    ds.Add(DocTuReader(reader));

                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTatCa SanPham: {ex.Message}");
                throw;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Lấy chỉ các món đang phục vụ (DangBan = 1)
        public List<SanPham> LayDangBan()
        {
            var ds = new List<SanPham>();
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = "SELECT Id, TenSanPham, GiaCoBan, DangBan, Loai, HinhAnh FROM SanPham WHERE DangBan = 1 ORDER BY Loai, TenSanPham";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                    ds.Add(DocTuReader(reader));

                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayDangBan SanPham: {ex.Message}");
                throw;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Lấy 1 sản phẩm theo Id
        public SanPham? LayTheoId(int id)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = "SELECT Id, TenSanPham, GiaCoBan, DangBan, Loai, HinhAnh FROM SanPham WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                    return DocTuReader(reader);

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTheoId SanPham: {ex.Message}");
                throw;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Thêm sản phẩm mới
        public void Them(SanPham sanPham)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                // Kiểm tra trùng tên sản phẩm
                string sqlCheck = "SELECT COUNT(*) FROM SanPham WHERE TenSanPham = @ten";
                using var checkCmd = new SqliteCommand(sqlCheck, conn);
                checkCmd.Parameters.AddWithValue("@ten", sanPham.TenSanPham);
                long count = (long)(checkCmd.ExecuteScalar() ?? 0);
                if (count > 0)
                    throw new Exception("Tên sản phẩm đã tồn tại! Vui lòng đặt tên khác.");

                string sql = @"INSERT INTO SanPham (TenSanPham, GiaCoBan, Loai, DangBan, HinhAnh)
                               VALUES (@ten, @gia, @loai, @dangBan, @hinhAnh)";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ten", sanPham.TenSanPham);
                cmd.Parameters.AddWithValue("@gia", sanPham.GiaCoBan);
                cmd.Parameters.AddWithValue("@loai", sanPham.Loai);
                cmd.Parameters.AddWithValue("@dangBan", sanPham.DangBan ? 1 : 0);
                cmd.Parameters.AddWithValue("@hinhAnh", (object?)sanPham.HinhAnh ?? DBNull.Value);
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

        // Sửa thông tin sản phẩm
        public void Sua(SanPham sanPham)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                // Kiểm tra trùng tên sản phẩm (trừ chính nó)
                string sqlCheck = "SELECT COUNT(*) FROM SanPham WHERE TenSanPham = @ten AND Id != @id";
                using var checkCmd = new SqliteCommand(sqlCheck, conn);
                checkCmd.Parameters.AddWithValue("@ten", sanPham.TenSanPham);
                checkCmd.Parameters.AddWithValue("@id", sanPham.Id);
                long count = (long)(checkCmd.ExecuteScalar() ?? 0);
                if (count > 0)
                    throw new Exception("Tên sản phẩm đã tồn tại! Vui lòng đặt tên khác.");

                string sql = @"UPDATE SanPham
                               SET TenSanPham = @ten, GiaCoBan = @gia,
                                   Loai = @loai, DangBan = @dangBan, HinhAnh = @hinhAnh
                               WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ten", sanPham.TenSanPham);
                cmd.Parameters.AddWithValue("@gia", sanPham.GiaCoBan);
                cmd.Parameters.AddWithValue("@loai", sanPham.Loai);
                cmd.Parameters.AddWithValue("@dangBan", sanPham.DangBan ? 1 : 0);
                cmd.Parameters.AddWithValue("@hinhAnh", (object?)sanPham.HinhAnh ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", sanPham.Id);
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

        // Xóa sản phẩm
        public void Xoa(int id)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = "DELETE FROM SanPham WHERE Id = @id";
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
    }
}
