// ============================================================
// TẦNG DAL - ChiTietHoaDonDAL (Access + OLE DB)
// Hỗ trợ: TrangThaiMon (DangCho / DangChuanBi / DaPhucVu)
// ============================================================
using System.Data.OleDb;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class ChiTietHoaDonDAL : IChiTietHoaDonDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        // Câu truy vấn chung: JOIN SanPham để lấy tên món
        private const string SelectSql =
            "SELECT ct.Id, ct.HoaDonId, ct.SanPhamId, sp.TenSanPham, ct.SoLuong, " +
            "ct.DonGiaBan, ct.ThuocTinhThem, ct.ThanhTien, ct.TrangThaiMon " +
            "FROM ChiTietHoaDon ct INNER JOIN SanPham sp ON ct.SanPhamId = sp.Id";

        // Helper: đọc 1 dòng từ reader sang ChiTietHoaDon
        private static ChiTietHoaDon Doc(OleDbDataReader r) => new()
        {
            Id = r.GetInt32(0),
            HoaDonId = r.GetInt32(1),
            SanPhamId = r.GetInt32(2),
            TenSanPham = r.GetString(3),
            SoLuong = r.GetInt32(4),
            DonGiaBan = r.GetDecimal(5),
            ThuocTinhThem = r.IsDBNull(6) ? "" : r.GetString(6),
            ThanhTien = r.GetDecimal(7),
            TrangThaiMon = r.IsDBNull(8) ? "DangCho" : r.GetString(8)
        };

        public List<ChiTietHoaDon> LayTheoHoaDon(int hoaDonId)
        {
            var ds = new List<ChiTietHoaDon>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(SelectSql + " WHERE ct.HoaDonId = @hdId ORDER BY ct.Id", c);
            cmd.Parameters.AddWithValue("@hdId", hoaDonId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));
            return ds;
        }

        public void Them(ChiTietHoaDon ct)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "INSERT INTO ChiTietHoaDon (HoaDonId, SanPhamId, SoLuong, DonGiaBan, ThuocTinhThem, ThanhTien, TrangThaiMon) " +
                "VALUES (@hdId, @spId, @sl, @donGia, @thuocTinh, @thanhTien, @trangThai)", c);
            cmd.Parameters.AddWithValue("@hdId", ct.HoaDonId);
            cmd.Parameters.AddWithValue("@spId", ct.SanPhamId);
            cmd.Parameters.AddWithValue("@sl", ct.SoLuong);
            cmd.Parameters.AddWithValue("@donGia", ct.DonGiaBan);
            cmd.Parameters.AddWithValue("@thuocTinh", (object?)ct.ThuocTinhThem ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@thanhTien", ct.ThanhTien);
            cmd.Parameters.AddWithValue("@trangThai", ct.TrangThaiMon ?? "DangCho");
            cmd.ExecuteNonQuery();
        }

        public void Xoa(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("DELETE FROM ChiTietHoaDon WHERE Id = @id", c);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void CapNhatTrangThaiMon(int id, string trangThai)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("UPDATE ChiTietHoaDon SET TrangThaiMon = @tt WHERE Id = @id", c);
            cmd.Parameters.AddWithValue("@tt", trangThai);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // Lấy món đang chờ bếp xử lý (của hóa đơn chưa thanh toán)
        public List<ChiTietHoaDon> LayMonDangCho()
        {
            var ds = new List<ChiTietHoaDon>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "SELECT ct.Id, ct.HoaDonId, ct.SanPhamId, sp.TenSanPham, ct.SoLuong, " +
                "ct.DonGiaBan, ct.ThuocTinhThem, ct.ThanhTien, ct.TrangThaiMon " +
                "FROM (ChiTietHoaDon ct INNER JOIN SanPham sp ON ct.SanPhamId = sp.Id) " +
                "INNER JOIN HoaDon hd ON ct.HoaDonId = hd.Id " +
                "WHERE hd.TrangThai = 'Chưa thanh toán' AND ct.TrangThaiMon = 'DangCho' " +
                "ORDER BY ct.Id", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));
            return ds;
        }

        public List<ChiTietHoaDon> LayMonDangChuanBi()
        {
            var ds = new List<ChiTietHoaDon>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "SELECT ct.Id, ct.HoaDonId, ct.SanPhamId, sp.TenSanPham, ct.SoLuong, " +
                "ct.DonGiaBan, ct.ThuocTinhThem, ct.ThanhTien, ct.TrangThaiMon " +
                "FROM (ChiTietHoaDon ct INNER JOIN SanPham sp ON ct.SanPhamId = sp.Id) " +
                "INNER JOIN HoaDon hd ON ct.HoaDonId = hd.Id " +
                "WHERE hd.TrangThai = 'Chưa thanh toán' AND ct.TrangThaiMon = 'DangChuanBi' " +
                "ORDER BY ct.Id", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));
            return ds;
        }
    }
}
