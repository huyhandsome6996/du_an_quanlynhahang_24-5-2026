// ============================================================
// TẦNG DAL - ChiTietHoaDonDAL
// Implement interface IChiTietHoaDonDAL
// Sử dụng try-catch-finally + conn.Close() trong finally
// ============================================================
using Microsoft.Data.Sqlite;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class ChiTietHoaDonDAL : IChiTietHoaDonDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        // Lấy tất cả món trong 1 hóa đơn (kèm tên sản phẩm)
        public List<ChiTietHoaDon> LayTheoHoaDon(int hoaDonId)
        {
            var ds = new List<ChiTietHoaDon>();
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = @"
                    SELECT ct.Id, ct.HoaDonId, ct.SanPhamId, sp.TenSanPham,
                           ct.SoLuong, ct.DonGiaBan, ct.ThuocTinhThem, ct.ThanhTien
                    FROM ChiTietHoaDon ct
                    INNER JOIN SanPham sp ON ct.SanPhamId = sp.Id
                    WHERE ct.HoaDonId = @hoaDonId
                    ORDER BY ct.Id";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@hoaDonId", hoaDonId);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ds.Add(new ChiTietHoaDon
                    {
                        Id = reader.GetInt32(0),
                        HoaDonId = reader.GetInt32(1),
                        SanPhamId = reader.GetInt32(2),
                        TenSanPham = reader.GetString(3),
                        SoLuong = reader.GetInt32(4),
                        DonGiaBan = reader.GetDecimal(5),
                        ThuocTinhThem = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        ThanhTien = reader.GetDecimal(7)
                    });
                }

                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTheoHoaDon: {ex.Message}");
                throw;
            }
            finally
            {
                // Bắt buộc đóng kết nối trong finally
                if (conn != null) conn.Close();
            }
        }

        // Thêm 1 món vào hóa đơn
        public void Them(ChiTietHoaDon chiTiet)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = @"
                    INSERT INTO ChiTietHoaDon (HoaDonId, SanPhamId, SoLuong, DonGiaBan, ThuocTinhThem, ThanhTien)
                    VALUES (@hdId, @spId, @sl, @donGia, @thuocTinh, @thanhTien)";

                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@hdId", chiTiet.HoaDonId);
                cmd.Parameters.AddWithValue("@spId", chiTiet.SanPhamId);
                cmd.Parameters.AddWithValue("@sl", chiTiet.SoLuong);
                cmd.Parameters.AddWithValue("@donGia", chiTiet.DonGiaBan);
                cmd.Parameters.AddWithValue("@thuocTinh", chiTiet.ThuocTinhThem ?? "");
                cmd.Parameters.AddWithValue("@thanhTien", chiTiet.ThanhTien);
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

        // Xóa 1 món khỏi hóa đơn
        public void Xoa(int id)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();

                string sql = "DELETE FROM ChiTietHoaDon WHERE Id = @id";
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
