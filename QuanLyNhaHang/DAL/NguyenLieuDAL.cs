// ============================================================
// TẦNG DAL - NguyenLieuDAL
// Quản lý nguyên liệu kho: CRUD + cảnh báo tồn kho thấp
// ============================================================
using Microsoft.Data.Sqlite;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class NguyenLieuDAL : INguyenLieuDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        public List<NguyenLieu> LayTatCa()
        {
            var ds = new List<NguyenLieu>();
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = "SELECT Id, TenNguyenLieu, DonVi, SoLuongTon, MucToiThieu, GhiChu FROM NguyenLieu ORDER BY TenNguyenLieu";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    ds.Add(DocTuReader(reader));
                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTatCa NguyenLieu: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        public NguyenLieu? LayTheoId(int id)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = "SELECT Id, TenNguyenLieu, DonVi, SoLuongTon, MucToiThieu, GhiChu FROM NguyenLieu WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return DocTuReader(reader);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTheoId NguyenLieu: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        public void Them(NguyenLieu nl)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                // Kiểm tra trùng tên
                using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM NguyenLieu WHERE TenNguyenLieu = @ten", conn);
                checkCmd.Parameters.AddWithValue("@ten", nl.TenNguyenLieu);
                long count = (long)checkCmd.ExecuteScalar()!;
                if (count > 0)
                    throw new Exception("Tên nguyên liệu đã tồn tại!");

                string sql = @"INSERT INTO NguyenLieu (TenNguyenLieu, DonVi, SoLuongTon, MucToiThieu, GhiChu)
                               VALUES (@ten, @donVi, @soLuongTon, @mucToiThieu, @ghiChu)";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ten", nl.TenNguyenLieu);
                cmd.Parameters.AddWithValue("@donVi", nl.DonVi);
                cmd.Parameters.AddWithValue("@soLuongTon", nl.SoLuongTon);
                cmd.Parameters.AddWithValue("@mucToiThieu", nl.MucToiThieu);
                cmd.Parameters.AddWithValue("@ghiChu", (object?)nl.GhiChu ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi Them NguyenLieu: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        public void Sua(NguyenLieu nl)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                // Kiểm tra trùng tên (trừ chính nó)
                using var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM NguyenLieu WHERE TenNguyenLieu = @ten AND Id != @id", conn);
                checkCmd.Parameters.AddWithValue("@ten", nl.TenNguyenLieu);
                checkCmd.Parameters.AddWithValue("@id", nl.Id);
                long count = (long)checkCmd.ExecuteScalar()!;
                if (count > 0)
                    throw new Exception("Tên nguyên liệu đã tồn tại!");

                string sql = @"UPDATE NguyenLieu SET TenNguyenLieu = @ten, DonVi = @donVi,
                               SoLuongTon = @soLuongTon, MucToiThieu = @mucToiThieu, GhiChu = @ghiChu
                               WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ten", nl.TenNguyenLieu);
                cmd.Parameters.AddWithValue("@donVi", nl.DonVi);
                cmd.Parameters.AddWithValue("@soLuongTon", nl.SoLuongTon);
                cmd.Parameters.AddWithValue("@mucToiThieu", nl.MucToiThieu);
                cmd.Parameters.AddWithValue("@ghiChu", (object?)nl.GhiChu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", nl.Id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi Sua NguyenLieu: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        public void Xoa(int id)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = "DELETE FROM NguyenLieu WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi Xoa NguyenLieu: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        public void CapNhatSoLuongTon(int id, decimal soLuongMoi)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = "UPDATE NguyenLieu SET SoLuongTon = @soLuong WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@soLuong", soLuongMoi);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi CapNhatSoLuongTon: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        public List<NguyenLieu> LayCanhBao()
        {
            var ds = new List<NguyenLieu>();
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = "SELECT Id, TenNguyenLieu, DonVi, SoLuongTon, MucToiThieu, GhiChu FROM NguyenLieu WHERE SoLuongTon <= MucToiThieu ORDER BY SoLuongTon ASC";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    ds.Add(DocTuReader(reader));
                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayCanhBao: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        private NguyenLieu DocTuReader(SqliteDataReader reader)
        {
            return new NguyenLieu
            {
                Id = reader.GetInt32(0),
                TenNguyenLieu = reader.GetString(1),
                DonVi = reader.GetString(2),
                SoLuongTon = reader.GetDecimal(3),
                MucToiThieu = reader.GetDecimal(4),
                GhiChu = reader.IsDBNull(5) ? null : reader.GetString(5)
            };
        }
    }
}
