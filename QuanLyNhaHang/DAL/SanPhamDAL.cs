// ============================================================
// TẦNG DAL - SanPhamDAL (Access + OLE DB)
// Đa hình: đọc cột Loai → tạo ThucAn hoặc NuocUong
// ============================================================
using System.Data.OleDb;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class SanPhamDAL : ISanPhamDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        // Factory Pattern + Đa hình: chọn class con dựa vào Loai
        private static SanPham Doc(OleDbDataReader r)
        {
            SanPham sp = r.GetString(4) == "ThucAn" ? new ThucAn() : new NuocUong();
            sp.Id = r.GetInt32(0);
            sp.TenSanPham = r.GetString(1);
            sp.GiaCoBan = r.GetDecimal(2);
            sp.DangBan = r.GetBoolean(3);
            sp.Loai = r.GetString(4);
            sp.HinhAnh = r.IsDBNull(5) ? null : r.GetString(5);
            return sp;
        }

        public List<SanPham> LayTatCa()
        {
            var ds = new List<SanPham>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "SELECT Id, TenSanPham, GiaCoBan, DangBan, Loai, HinhAnh FROM SanPham ORDER BY Loai, TenSanPham", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));
            return ds;
        }

        public List<SanPham> LayDangBan()
        {
            var ds = new List<SanPham>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "SELECT Id, TenSanPham, GiaCoBan, DangBan, Loai, HinhAnh FROM SanPham " +
                "WHERE DangBan = True ORDER BY Loai, TenSanPham", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) ds.Add(Doc(r));
            return ds;
        }

        public SanPham? LayTheoId(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "SELECT Id, TenSanPham, GiaCoBan, DangBan, Loai, HinhAnh FROM SanPham WHERE Id = ?", c);
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            using var r = cmd.ExecuteReader();
            return r.Read() ? Doc(r) : null;
        }

        public void Them(SanPham sp)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            using var chk = new OleDbCommand("SELECT COUNT(*) FROM SanPham WHERE TenSanPham = ?", c);
            chk.Parameters.Add("@ten", OleDbType.VarWChar).Value = sp.TenSanPham;
            if ((int)chk.ExecuteScalar()! > 0)
                throw new Exception("Tên sản phẩm đã tồn tại! Vui lòng đặt tên khác.");

            using var cmd = new OleDbCommand(
                "INSERT INTO SanPham (TenSanPham, GiaCoBan, Loai, DangBan, HinhAnh) " +
                "VALUES (?, ?, ?, ?, ?)", c);
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = sp.TenSanPham;
            cmd.Parameters.Add("@gia", OleDbType.Currency).Value = sp.GiaCoBan;
            cmd.Parameters.Add("@loai", OleDbType.VarWChar).Value = sp.Loai;
            cmd.Parameters.Add("@dangBan", OleDbType.Boolean).Value = sp.DangBan;
            cmd.Parameters.Add("@hinhAnh", OleDbType.VarWChar).Value = (object?)sp.HinhAnh ?? DBNull.Value;
            cmd.ExecuteNonQuery();
        }

        public void Sua(SanPham sp)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            using var chk = new OleDbCommand("SELECT COUNT(*) FROM SanPham WHERE TenSanPham = ? AND Id <> ?", c);
            chk.Parameters.Add("@ten", OleDbType.VarWChar).Value = sp.TenSanPham;
            chk.Parameters.Add("@id", OleDbType.Integer).Value = sp.Id;
            if ((int)chk.ExecuteScalar()! > 0)
                throw new Exception("Tên sản phẩm đã tồn tại! Vui lòng đặt tên khác.");

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
