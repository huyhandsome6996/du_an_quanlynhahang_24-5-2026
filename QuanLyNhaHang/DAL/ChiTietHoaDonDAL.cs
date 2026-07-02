// ============================================================
// TẦNG DAL - ChiTietHoaDonDAL (Access + OLE DB)
// ------------------------------------------------------------
// Implement interface IChiTietHoaDonDAL. Thao tác với bảng ChiTietHoaDon.
//
// Bảng này là bảng TRUNG GIAN giữa HoaDon và SanPham (quan hệ N—N).
// Mỗi dòng = 1 món được gọi trong 1 hóa đơn cụ thể.
//
// Hỗ trợ nghiệp vụ bếp/bar:
//   - LayMonDangCho(): lấy các món vừa order, đang chờ bếp nhận
//   - LayMonDangChuanBi(): lấy các món bếp đang nấu
//   - CapNhatTrangThaiMon(): chuyển DangCho → DangChuanBi → DaPhucVu
// ============================================================
using System.Data.OleDb;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    /// <summary>
    /// Lớp ChiTietHoaDonDAL — thao tác với bảng ChiTietHoaDon.
    /// </summary>
    public class ChiTietHoaDonDAL : IChiTietHoaDonDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        // Câu SELECT chung: JOIN với SanPham để lấy TenSanPham (hiển thị UI)
        private const string SelectSql =
            "SELECT ct.Id, ct.HoaDonId, ct.SanPhamId, sp.TenSanPham, ct.SoLuong, " +
            "ct.DonGiaBan, ct.ThuocTinhThem, ct.ThanhTien, ct.TrangThaiMon " +
            "FROM ChiTietHoaDon ct INNER JOIN SanPham sp ON ct.SanPhamId = sp.Id";

        /// <summary>
        /// Helper: Đọc 1 dòng từ DataReader → object ChiTietHoaDon.
        /// </summary>
        private static ChiTietHoaDon Doc(OleDbDataReader r) => new()
        {
            Id = r.GetInt32(0),                                  // ct.Id
            HoaDonId = r.GetInt32(1),                            // ct.HoaDonId
            SanPhamId = r.GetInt32(2),                           // ct.SanPhamId
            TenSanPham = r.GetString(3),                         // sp.TenSanPham (từ JOIN)
            SoLuong = r.GetInt32(4),                             // ct.SoLuong
            DonGiaBan = r.GetDecimal(5),                         // ct.DonGiaBan
            // ThuocTinhThem có thể NULL → mặc định ""
            ThuocTinhThem = r.IsDBNull(6) ? "" : r.GetString(6),
            ThanhTien = r.GetDecimal(7),                         // ct.ThanhTien
            // TrangThaiMon có thể NULL → mặc định "DangCho"
            TrangThaiMon = r.IsDBNull(8) ? "DangCho" : r.GetString(8)
        };

        /// <summary>
        /// Lấy danh sách món của 1 hóa đơn cụ thể.
        /// Dùng cho: hiển thị bảng món khi click vào bàn có khách.
        /// </summary>
        public List<ChiTietHoaDon> LayTheoHoaDon(int hoaDonId)
        {
            var ds = new List<ChiTietHoaDon>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(SelectSql + " WHERE ct.HoaDonId = ? ORDER BY ct.Id", c);
            cmd.Parameters.Add("@hdId", OleDbType.Integer).Value = hoaDonId;
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));
            return ds;
        }

        /// <summary>
        /// Thêm 1 món vào hóa đơn.
        /// DonGiaBan và ThanhTien đã được tính từ TinhTien() của SanPham
        /// (đa hình) ở tầng API — DAL chỉ lưu xuống DB.
        /// </summary>
        public void Them(ChiTietHoaDon ct)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "INSERT INTO ChiTietHoaDon (HoaDonId, SanPhamId, SoLuong, DonGiaBan, ThuocTinhThem, ThanhTien, TrangThaiMon) " +
                "VALUES (?, ?, ?, ?, ?, ?, ?)", c);
            cmd.Parameters.Add("@hdId", OleDbType.Integer).Value = ct.HoaDonId;
            cmd.Parameters.Add("@spId", OleDbType.Integer).Value = ct.SanPhamId;
            cmd.Parameters.Add("@sl", OleDbType.Integer).Value = ct.SoLuong;
            cmd.Parameters.Add("@donGia", OleDbType.Currency).Value = ct.DonGiaBan;
            // ThuocTinhThem có thể null
            cmd.Parameters.Add("@thuocTinh", OleDbType.VarWChar).Value = (object?)ct.ThuocTinhThem ?? DBNull.Value;
            cmd.Parameters.Add("@thanhTien", OleDbType.Currency).Value = ct.ThanhTien;
            // TrangThaiMon mặc định "DangCho" khi mới thêm
            cmd.Parameters.Add("@trangThai", OleDbType.VarWChar).Value = ct.TrangThaiMon ?? "DangCho";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Xoá 1 món khỏi hóa đơn (khi khách đổi ý).
        /// Tổng tiền sẽ được tính lại ở tầng API (ApiHoaDon.cs).
        /// </summary>
        public void Xoa(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("DELETE FROM ChiTietHoaDon WHERE Id = ?", c);
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Cập nhật trạng thái món (DangCho → DangChuanBi → DaPhucVu).
        /// Dùng cho: bếp/bar nhận món và cập nhật tiến độ.
        /// </summary>
        public void CapNhatTrangThaiMon(int id, string trangThai)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("UPDATE ChiTietHoaDon SET TrangThaiMon = ? WHERE Id = ?", c);
            cmd.Parameters.Add("@tt", OleDbType.VarWChar).Value = trangThai;
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Lấy các món đang chờ bếp xử lý (của hóa đơn chưa TT).
        /// JOIN thêm HoaDon để lọc theo TrangThai của hóa đơn.
        /// </summary>
        public List<ChiTietHoaDon> LayMonDangCho()
        {
            var ds = new List<ChiTietHoaDon>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            // 2 JOIN: ChiTietHoaDon ↔ SanPham, ChiTietHoaDon ↔ HoaDon
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

        /// <summary>
        /// Lấy các món đang được bếp chuẩn bị (TrangThaiMon = 'DangChuanBi').
        /// </summary>
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
