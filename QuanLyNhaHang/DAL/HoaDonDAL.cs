// ============================================================
// TẦNG DAL - HoaDonDAL (Access + OLE DB)
// ------------------------------------------------------------
// Implement interface IHoaDonDAL. Thao tác với bảng HoaDon.
//
// Hỗ trợ nghiệp vụ:
//   - Tạo hóa đơn khi mở bàn
//   - Cập nhật tổng tiền khi thêm/xoá món
//   - Thanh toán: set ThoiGianThanhToan, TrangThai="Đã thanh toán", VAT, GiamGia, PTTT
//   - Lọc hóa đơn theo khoảng ngày (cho trang Báo cáo)
//
// JOIN với bảng Ban để lấy TenBan hiển thị lên UI.
// ============================================================
using System.Data.OleDb;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    /// <summary>
    /// Lớp HoaDonDAL — thao tác với bảng HoaDon.
    /// </summary>
    public class HoaDonDAL : IHoaDonDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        // Câu SELECT chung cho mọi hàm Lay...
        // JOIN với Ban để lấy TenBan (hiển thị trên UI)
        // INNER JOIN = chỉ lấy dòng có Khóa ngoại khớp
        private const string SelectSql =
            "SELECT hd.Id, hd.BanId, b.TenBan, hd.ThoiGianTao, hd.ThoiGianThanhToan, " +
            "hd.TongTien, hd.TrangThai, hd.VAT, hd.GiamGia, hd.PhuongThucThanhToan " +
            "FROM HoaDon hd INNER JOIN Ban b ON hd.BanId = b.Id";

        /// <summary>
        /// Helper: Đọc 1 dòng từ DataReader → object HoaDon.
        /// Dùng cho mọi hàm SELECT để tránh lặp code.
        /// </summary>
        private static HoaDon Doc(OleDbDataReader r) => new()
        {
            Id = r.GetInt32(0),                                     // hd.Id
            BanId = r.GetInt32(1),                                  // hd.BanId
            TenBan = r.GetString(2),                                // b.TenBan (từ JOIN)
            ThoiGianTao = r.GetDateTime(3),                         // hd.ThoiGianTao
            // ThoiGianThanhToan có thể NULL (chưa TT) → phải kiểm tra IsDBNull
            ThoiGianThanhToan = r.IsDBNull(4) ? null : r.GetDateTime(4),
            TongTien = r.GetDecimal(5),                             // hd.TongTien
            TrangThai = r.GetString(6),                             // hd.TrangThai
            VAT = r.IsDBNull(7) ? 0 : r.GetDecimal(7),              // hd.VAT (có thể NULL)
            GiamGia = r.IsDBNull(8) ? 0 : r.GetDecimal(8),          // hd.GiamGia
            // Nếu NULL → mặc định "TienMat"
            PhuongThucThanhToan = r.IsDBNull(9) ? "TienMat" : r.GetString(9)
        };

        /// <summary>
        /// Lấy tất cả hóa đơn (sắp xếp mới nhất trước).
        /// Dùng cho trang Lịch sử.
        /// </summary>
        public List<HoaDon> LayTatCa()
        {
            var ds = new List<HoaDon>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            // Nối thêm ORDER BY vào SelectSql
            using var cmd = new OleDbCommand(SelectSql + " ORDER BY hd.ThoiGianTao DESC", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));
            return ds;
        }

        /// <summary>
        /// Lấy 1 hóa đơn theo Id.
        /// </summary>
        public HoaDon? LayTheoId(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(SelectSql + " WHERE hd.Id = ?", c);
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            using var r = cmd.ExecuteReader();
            return r.Read() ? Doc(r) : null;
        }

        /// <summary>
        /// Lấy hóa đơn CHƯA thanh toán của 1 bàn cụ thể.
        /// Mỗi bàn cùng 1 thời điểm chỉ có 1 hóa đơn chưa TT.
        /// </summary>
        public HoaDon? LayHoaDonChuaThanhToanTheoBan(int banId)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            // 2 điều kiện WHERE: theo BanId và theo TrangThai
            using var cmd = new OleDbCommand(
                SelectSql + " WHERE hd.BanId = ? AND hd.TrangThai = 'Chưa thanh toán'", c);
            cmd.Parameters.Add("@banId", OleDbType.Integer).Value = banId;
            using var r = cmd.ExecuteReader();
            return r.Read() ? Doc(r) : null;
        }

        /// <summary>
        /// Thêm hóa đơn mới. Trả về Id tự tăng.
        /// QUAN TRỌNG: Dùng SELECT @@IDENTITY trên CÙNG CONNECTION
        /// để lấy Id vừa được AutoNumber cấp.
        /// </summary>
        public int Them(HoaDon hd)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            // INSERT 7 cột. Id là AutoNumber.
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

            // SELECT @@IDENTITY — trả về Id AutoNumber vừa cấp cho dòng INSERT trên
            using var idCmd = new OleDbCommand("SELECT @@IDENTITY", c);
            // Convert.ToInt32 để unwrap object an toàn
            return Convert.ToInt32(idCmd.ExecuteScalar());
        }

        /// <summary>
        /// Cập nhật tổng tiền của hóa đơn.
        /// Dùng khi: thêm món (tổng += thanhTien), xoá món (tổng -= thanhTien).
        /// </summary>
        public void CapNhatTongTien(int hoaDonId, decimal tongTien)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("UPDATE HoaDon SET TongTien = ? WHERE Id = ?", c);
            cmd.Parameters.Add("@tongTien", OleDbType.Currency).Value = tongTien;
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = hoaDonId;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Đánh dấu hóa đơn đã thanh toán: set ThoiGianThanhToan = now, TrangThai = "Đã thanh toán".
        /// </summary>
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

        /// <summary>
        /// Cập nhật VAT, giảm giá, phương thức thanh toán TRƯỚC khi đóng hóa đơn.
        /// Gọi trước hàm ThanhToan().
        /// </summary>
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

        /// <summary>
        /// Lọc hóa đơn ĐÃ thanh toán theo khoảng ngày (cho trang Báo cáo).
        /// Lọc dựa trên ThoiGianThanhToan (không phải ThoiGianTao).
        /// </summary>
        public List<HoaDon> LayTheoKhoangNgay(DateTime tuNgay, DateTime denNgay)
        {
            var ds = new List<HoaDon>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            // 3 điều kiện: TrangThai, ThoiGianThanhToan >= tuNgay, <= denNgay
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
