// ============================================================
// TẦNG DAL - NguoiDungDAL (Access + OLE DB)
// Mật khẩu lưu SHA-256, không lưu plain-text
// ============================================================
using System.Data.OleDb;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class NguoiDungDAL : INguoiDungDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        public bool KiemTraCoNguoiDung()
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("SELECT COUNT(*) FROM NguoiDung", c);
            return (int)cmd.ExecuteScalar()! > 0;
        }

        public NguoiDung? LayTheoTenDangNhap(string tenDangNhap)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand(
                "SELECT Id, TenDangNhap, MatKhauHash, VaiTro, NgayTao FROM NguoiDung WHERE TenDangNhap = ?", c);
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = tenDangNhap;
            using var r = cmd.ExecuteReader();
            if (r.Read())
                return new NguoiDung
                {
                    Id = r.GetInt32(0),
                    TenDangNhap = r.GetString(1),
                    MatKhauHash = r.GetString(2),
                    VaiTro = r.GetString(3),
                    NgayTao = r.GetDateTime(4)
                };
            return null;
        }

        public void Them(NguoiDung nd)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            // Kiểm tra trùng tên đăng nhập
            using var chk = new OleDbCommand("SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = ?", c);
            chk.Parameters.Add("@ten", OleDbType.VarWChar).Value = nd.TenDangNhap;
            if ((int)chk.ExecuteScalar()! > 0)
                throw new Exception("Tên đăng nhập đã tồn tại! Vui lòng chọn tên khác.");

            // INSERT với OleDbType tường minh để tránh "Data type mismatch"
            using var cmd = new OleDbCommand(
                "INSERT INTO NguoiDung (TenDangNhap, MatKhauHash, VaiTro, NgayTao) " +
                "VALUES (?, ?, ?, ?)", c);
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = nd.TenDangNhap;
            cmd.Parameters.Add("@hash", OleDbType.VarWChar).Value = nd.MatKhauHash;
            cmd.Parameters.Add("@vaiTro", OleDbType.VarWChar).Value = nd.VaiTro;
            cmd.Parameters.Add("@ngay", OleDbType.Date).Value = nd.NgayTao;
            cmd.ExecuteNonQuery();
        }
    }
}
