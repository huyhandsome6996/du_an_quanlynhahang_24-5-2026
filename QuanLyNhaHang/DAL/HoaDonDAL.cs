// ============================================================
// TẦNG DAL - HoaDonDAL (Access + OLE DB)
// Hỗ trợ: VAT, Giảm giá, Phương thức thanh toán
// ============================================================
using System.Data.OleDb;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class HoaDonDAL : IHoaDonDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        // Câu truy vấn chung cho mọi hàm SELECT
        private const string SelectSql =
            "SELECT hd.Id, hd.BanId, b.TenBan, hd.ThoiGianTao, hd.ThoiGianThanhToan, " +
            "hd.TongTien, hd.TrangThai, hd.VAT, hd.GiamGia, hd.PhuongThucThanhToan " +
            "FROM HoaDon hd INNER JOIN Ban b ON hd.BanId = b.Id";

        // Helper: đọc 1 dòng từ reader sang object HoaDon
        private static HoaDon Doc(OleDbDataReader r) => new()
        {
            Id = r.GetInt32(0),
            BanId = r.GetInt32(1),
            TenBan = r.GetString(2),
            ThoiGianTao = r.GetDateTime(3),
            ThoiGianThanhToan = r.IsDBNull(4) ? null : r.GetDateTime(4),
            TongTien = r.GetDecimal(5),
            TrangThai = r.GetString(6),
            VAT = r.IsDBNull(7) ? 0 : r.GetDecimal(7),
            GiamGia = r.IsDBNull(8) ? 0 : r.GetDecimal(8),
            PhuongThucThanhToan = r.IsDBNull(9) ? "TienMat" : r.GetString(9)
        };

        public List<HoaDon> LayTatCa()
        {
            var ds = new List<HoaDon>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(SelectSql + " ORDER BY hd.ThoiGianTao DESC", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));
            return ds;
        }

        public HoaDon? LayTheoId(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(SelectSql + " WHERE hd.Id = ?", c);
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            using var r = cmd.ExecuteReader();
            return r.Read() ? Doc(r) : null;
        }

        public HoaDon? LayHoaDonChuaThanhToanTheoBan(int banId)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                SelectSql + " WHERE hd.BanId = ? AND hd.TrangThai = 'Chưa thanh toán'", c);
            cmd.Parameters.Add("@banId", OleDbType.Integer).Value = banId;
            using var r = cmd.ExecuteReader();
            return r.Read() ? Doc(r) : null;
        }

        public int Them(HoaDon hd)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "INSERT INTO HoaDon (BanId, ThoiGianTao, TongTien, TrangThai, VAT, GiamGia, PhuongThucThanhToan) " +
                "VALUES (?, ?, ?, ?, ?, ?, ?)", c);
            cmd.Parameters.Add("@banId", OleDbType.Integer).Value = hd.BanId;
            cmd.Parameters.Add("@tgt", OleDbType.Date).Value = hd.ThoiGianTao;
            cmd.Parameters.Add("@tongTien", OleDbType.Currency).Value = hd.TongTien;
            cmd.Parameters.Add("@tt", OleDbType.VarWChar).Value = hd.TrangThai;
            cmd.Parameters.Add("@vat", OleDbType.Currency).Value = hd.VAT;
            cmd.Parameters.Add("@giamGia", OleDbType.Currency).Value = hd.GiamGia;
            cmd.Parameters.Add("@pttt", OleDbType.VarWChar).Value = hd.PhuongThucThanhToan;
            cmd.ExecuteNonQuery();

            // Lấy Id tự tăng: dùng @@IDENTITY trên cùng connection
            using var idCmd = new OleDbCommand("SELECT @@IDENTITY", c);
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public void CapNhatTongTien(int hoaDonId, decimal tongTien)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("UPDATE HoaDon SET TongTien = ? WHERE Id = ?", c);
            cmd.Parameters.Add("@tongTien", OleDbType.Currency).Value = tongTien;
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = hoaDonId;
            cmd.ExecuteNonQuery();
        }

        public void ThanhToan(int hoaDonId)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "UPDATE HoaDon SET ThoiGianThanhToan = ?, TrangThai = 'Đã thanh toán' WHERE Id = ?", c);
            cmd.Parameters.Add("@tgtt", OleDbType.Date).Value = DateTime.Now;
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = hoaDonId;
            cmd.ExecuteNonQuery();
        }

        public void CapNhatThanhToan(int hoaDonId, decimal vat, decimal giamGia, string phuongThuc)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "UPDATE HoaDon SET VAT = ?, GiamGia = ?, PhuongThucThanhToan = ? WHERE Id = ?", c);
            cmd.Parameters.Add("@vat", OleDbType.Currency).Value = vat;
            cmd.Parameters.Add("@giamGia", OleDbType.Currency).Value = giamGia;
            cmd.Parameters.Add("@pttt", OleDbType.VarWChar).Value = phuongThuc;
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = hoaDonId;
            cmd.ExecuteNonQuery();
        }

        public List<HoaDon> LayTheoKhoangNgay(DateTime tuNgay, DateTime denNgay)
        {
            var ds = new List<HoaDon>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                SelectSql + " WHERE hd.TrangThai = 'Đã thanh toán' " +
                "AND hd.ThoiGianThanhToan >= ? AND hd.ThoiGianThanhToan <= ? " +
                "ORDER BY hd.ThoiGianThanhToan DESC", c);
            cmd.Parameters.Add("@tuNgay", OleDbType.Date).Value = tuNgay;
            cmd.Parameters.Add("@denNgay", OleDbType.Date).Value = denNgay;
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));
            return ds;
        }
    }
}
