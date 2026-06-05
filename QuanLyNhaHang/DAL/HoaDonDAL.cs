// ============================================================
// TẦNG DAL - HoaDonDAL
// Implement interface IHoaDonDAL: thao tác với bảng HoaDon
// Hỗ trợ: VAT, Giảm giá, Phương thức thanh toán
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
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = @"
                    SELECT hd.Id, hd.BanId, b.TenBan, hd.ThoiGianTao,
                           hd.ThoiGianThanhToan, hd.TongTien, hd.TrangThai,
                           hd.VAT, hd.GiamGia, hd.PhuongThucThanhToan
                    FROM HoaDon hd
                    INNER JOIN Ban b ON hd.BanId = b.Id
                    ORDER BY hd.ThoiGianTao DESC";
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    ds.Add(DocTuReader(reader));
                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTatCa HoaDon: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        // Lấy 1 hóa đơn theo Id
        public HoaDon? LayTheoId(int id)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = @"
                    SELECT hd.Id, hd.BanId, b.TenBan, hd.ThoiGianTao,
                           hd.ThoiGianThanhToan, hd.TongTien, hd.TrangThai,
                           hd.VAT, hd.GiamGia, hd.PhuongThucThanhToan
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
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTheoId HoaDon: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        // Lấy hóa đơn chưa thanh toán của 1 bàn cụ thể
        public HoaDon? LayHoaDonChuaThanhToanTheoBan(int banId)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = @"
                    SELECT hd.Id, hd.BanId, b.TenBan, hd.ThoiGianTao,
                           hd.ThoiGianThanhToan, hd.TongTien, hd.TrangThai,
                           hd.VAT, hd.GiamGia, hd.PhuongThucThanhToan
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
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayHoaDonChuaThanhToan: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        // Tạo hóa đơn mới, trả về Id vừa tạo
        public int Them(HoaDon hoaDon)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = @"
                    INSERT INTO HoaDon (BanId, ThoiGianTao, TongTien, TrangThai, VAT, GiamGia, PhuongThucThanhToan)
                    VALUES (@banId, @tgt, @tongTien, @tt, @vat, @giamGia, @pttt);
                    SELECT last_insert_rowid();";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@banId", hoaDon.BanId);
                cmd.Parameters.AddWithValue("@tgt", hoaDon.ThoiGianTao.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@tongTien", hoaDon.TongTien);
                cmd.Parameters.AddWithValue("@tt", hoaDon.TrangThai);
                cmd.Parameters.AddWithValue("@vat", hoaDon.VAT);
                cmd.Parameters.AddWithValue("@giamGia", hoaDon.GiamGia);
                cmd.Parameters.AddWithValue("@pttt", hoaDon.PhuongThucThanhToan);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        // Cập nhật tổng tiền sau khi thêm/xóa món
        public void CapNhatTongTien(int hoaDonId, decimal tongTien)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = "UPDATE HoaDon SET TongTien = @tongTien WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tongTien", tongTien);
                cmd.Parameters.AddWithValue("@id", hoaDonId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        // Thanh toán hóa đơn: cập nhật thời gian, trạng thái, VAT, giảm giá, PTTT
        public void ThanhToan(int hoaDonId)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
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
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        // Cập nhật thông tin thanh toán (VAT, giảm giá, PTTT)
        public void CapNhatThanhToan(int hoaDonId, decimal vat, decimal giamGia, string phuongThuc)
        {
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = @"UPDATE HoaDon SET VAT = @vat, GiamGia = @giamGia, PhuongThucThanhToan = @pttt WHERE Id = @id";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@vat", vat);
                cmd.Parameters.AddWithValue("@giamGia", giamGia);
                cmd.Parameters.AddWithValue("@pttt", phuongThuc);
                cmd.Parameters.AddWithValue("@id", hoaDonId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        // Lấy doanh thu theo khoảng ngày
        public List<HoaDon> LayTheoKhoangNgay(DateTime tuNgay, DateTime denNgay)
        {
            var ds = new List<HoaDon>();
            SqliteConnection? conn = null;
            try
            {
                conn = new SqliteConnection(_conn);
                conn.Open();
                string sql = @"
                    SELECT hd.Id, hd.BanId, b.TenBan, hd.ThoiGianTao,
                           hd.ThoiGianThanhToan, hd.TongTien, hd.TrangThai,
                           hd.VAT, hd.GiamGia, hd.PhuongThucThanhToan
                    FROM HoaDon hd
                    INNER JOIN Ban b ON hd.BanId = b.Id
                    WHERE hd.TrangThai = 'Đã thanh toán'
                      AND hd.ThoiGianThanhToan >= @tuNgay
                      AND hd.ThoiGianThanhToan <= @denNgay
                    ORDER BY hd.ThoiGianThanhToan DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tuNgay", tuNgay.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@denNgay", denNgay.ToString("yyyy-MM-dd HH:mm:ss"));
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    ds.Add(DocTuReader(reader));
                return ds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi LayTheoKhoangNgay: {ex.Message}");
                throw;
            }
            finally
            {
                if (conn != null) conn.Close();
            }
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
                TrangThai = reader.GetString(6),
                VAT = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                GiamGia = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                PhuongThucThanhToan = reader.IsDBNull(9) ? "TienMat" : reader.GetString(9)
            };
        }
    }
}
