// ============================================================
// TẦNG DAL - SanPhamDAL (Access + OLE DB)
// ------------------------------------------------------------
// Implement interface ISanPhamDAL. Thao tác với bảng SanPham.
//
// ĐA HÌNH (Polymorphism) thể hiện ở đây:
//   Khi đọc 1 dòng từ Access, ta dùng cột "Loai" để quyết định
//   new ThucAn() hay new NuocUong(). Mặc dù cả 2 đều gán vào
//   biến kiểu SanPham (lớp cha), nhưng khi gọi sp.TinhTien()
//   thì C# tự gọi override của lớp con tương ứng.
//   → Đây chính là đa hình lúc RUNTIME.
// ============================================================
using System.Data.OleDb;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    /// <summary>
    /// Lớp SanPhamDAL — thao tác CRUD với bảng SanPham.
    /// </summary>
    public class SanPhamDAL : ISanPhamDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        /// <summary>
        /// FACTORY METHOD + ĐA HÌNH:
        /// Đọc 1 dòng từ OleDbDataReader và tạo object ThucAn hoặc NuocUong
        /// dựa vào cột Loai. Mặc dù return kiểu SanPham (lớp cha) nhưng
        /// object thực tế là 1 trong 2 lớp con.
        /// </summary>
        private static SanPham Doc(OleDbDataReader r)
        {
            // Toán tử ?: chọn lớp con dựa vào giá trị cột Loai (cột 4)
            SanPham sp = r.GetString(4) == "ThucAn" ? new ThucAn() : new NuocUong();
            sp.Id = r.GetInt32(0);                  // Cột 0: Id
            sp.TenSanPham = r.GetString(1);         // Cột 1: TenSanPham
            sp.GiaCoBan = r.GetDecimal(2);          // Cột 2: GiaCoBan (kiểu MONEY → decimal)
            sp.DangBan = r.GetBoolean(3);           // Cột 3: DangBan (BIT → bool)
            sp.Loai = r.GetString(4);               // Cột 4: Loai (đã dùng ở trên)
            sp.HinhAnh = r.IsDBNull(5) ? null : r.GetString(5);  // Cột 5: HinhAnh (cho phép null)
            return sp;
        }

        /// <summary>
        /// Lấy tất cả sản phẩm (kể cả món đang ngừng bán), sắp xếp theo Loai + tên.
        /// </summary>
        public List<SanPham> LayTatCa()
        {
            var ds = new List<SanPham>();
            using var c = new OleDbConnection(_conn);
            c.Open();

            // SELECT 6 cột, ORDER BY Loai trước (để nhóm TẤT CẢ ThucAn rồi mới tới NuocUong),
            // sau đó ORDER BY TenSanPham (theo bảng chữ cái)
            using var cmd = new OleDbCommand(
                "SELECT Id, TenSanPham, GiaCoBan, DangBan, Loai, HinhAnh FROM SanPham ORDER BY Loai, TenSanPham", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));   // Mỗi dòng → 1 object SanPham (đa hình)
            return ds;
        }

        /// <summary>
        /// Chỉ lấy các món đang bán (DangBan = True).
        /// Dùng cho trang Gọi món — không cho khách gọi món đã ngừng bán.
        /// </summary>
        public List<SanPham> LayDangBan()
        {
            var ds = new List<SanPham>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            // WHERE DangBan = True — Access dùng True/False (không phải 1/0)
            using var cmd = new OleDbCommand(
                "SELECT Id, TenSanPham, GiaCoBan, DangBan, Loai, HinhAnh FROM SanPham " +
                "WHERE DangBan = True ORDER BY Loai, TenSanPham", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));
            return ds;
        }

        /// <summary>
        /// Lấy 1 sản phẩm theo Id. Trả về null nếu không có.
        /// </summary>
        public SanPham? LayTheoId(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "SELECT Id, TenSanPham, GiaCoBan, DangBan, Loai, HinhAnh FROM SanPham WHERE Id = ?", c);
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            using var r = cmd.ExecuteReader();
            // Toán tử ?: — đọc nếu có dòng, ngược lại trả null
            return r.Read() ? Doc(r) : null;
        }

        /// <summary>
        /// Thêm sản phẩm mới (ThucAn hoặc NuocUong). Throw nếu trùng tên.
        /// </summary>
        public void Them(SanPham sp)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            // Kiểm tra trùng tên
            using var chk = new OleDbCommand("SELECT COUNT(*) FROM SanPham WHERE TenSanPham = ?", c);
            chk.Parameters.Add("@ten", OleDbType.VarWChar).Value = sp.TenSanPham;
            if ((int)chk.ExecuteScalar()! > 0)
                throw new Exception("Tên sản phẩm đã tồn tại! Vui lòng đặt tên khác.");

            // INSERT 5 cột. Id là AutoNumber nên không cần thêm.
            using var cmd = new OleDbCommand(
                "INSERT INTO SanPham (TenSanPham, GiaCoBan, Loai, DangBan, HinhAnh) " +
                "VALUES (?, ?, ?, ?, ?)", c);
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = sp.TenSanPham;
            // OleDbType.Currency = kiểu MONEY trong Access (decimal, 4 số lẻ)
            cmd.Parameters.Add("@gia", OleDbType.Currency).Value = sp.GiaCoBan;
            cmd.Parameters.Add("@loai", OleDbType.VarWChar).Value = sp.Loai;
            // OleDbType.Boolean = kiểu BIT (True/False)
            cmd.Parameters.Add("@dangBan", OleDbType.Boolean).Value = sp.DangBan;
            // HinhAnh có thể null → phải cast sang object? ?? DBNull.Value
            cmd.Parameters.Add("@hinhAnh", OleDbType.VarWChar).Value = (object?)sp.HinhAnh ?? DBNull.Value;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Sửa sản phẩm. Throw nếu trùng tên với sản phẩm khác.
        /// </summary>
        public void Sua(SanPham sp)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            // Kiểm tra trùng tên nhưng LOẠI TRỪ sản phẩm đang sửa
            using var chk = new OleDbCommand("SELECT COUNT(*) FROM SanPham WHERE TenSanPham = ? AND Id <> ?", c);
            chk.Parameters.Add("@ten", OleDbType.VarWChar).Value = sp.TenSanPham;
            chk.Parameters.Add("@id", OleDbType.Integer).Value = sp.Id;
            if ((int)chk.ExecuteScalar()! > 0)
                throw new Exception("Tên sản phẩm đã tồn tại! Vui lòng đặt tên khác.");

            // UPDATE 5 cột + WHERE Id
            using var cmd = new OleDbCommand(
                "UPDATE SanPham SET TenSanPham = ?, GiaCoBan = ?, Loai = ?, DangBan = ?, HinhAnh = ? WHERE Id = ?", c);
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = sp.TenSanPham;
            cmd.Parameters.Add("@gia", OleDbType.Currency).Value = sp.GiaCoBan;
            cmd.Parameters.Add("@loai", OleDbType.VarWChar).Value = sp.Loai;
            cmd.Parameters.Add("@dangBan", OleDbType.Boolean).Value = sp.DangBan;
            cmd.Parameters.Add("@hinhAnh", OleDbType.VarWChar).Value = (object?)sp.HinhAnh ?? DBNull.Value;
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = sp.Id;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Xoá sản phẩm theo Id. Bảng ChiTietHoaDon có FK cascade → tự xoá theo.
        /// </summary>
        public void Xoa(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("DELETE FROM SanPham WHERE Id = ?", c);
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            cmd.ExecuteNonQuery();
        }
    }
}
