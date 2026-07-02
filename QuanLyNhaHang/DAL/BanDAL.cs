// ============================================================
// TẦNG DAL - BanDAL (Access + OLE DB)
// ------------------------------------------------------------
// Implement interface IBanDAL. Thao tác với bảng Ban trong file Access.
//
// Quy tắc viết DAL (bắt buộc tuân thủ để tránh lỗi):
//   1. Dùng tham số vị trí "?" (KHÔNG dùng @ten như SQL Server)
//   2. Dùng Parameters.Add(name, OleDbType.X).Value (KHÔNG AddWithValue)
//   3. Mở connection trong khối using để tự đóng
// ============================================================
using System.Data.OleDb;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    /// <summary>
    /// Lớp BanDAL — thao tác CRUD với bảng Ban trong Access.
    /// </summary>
    public class BanDAL : IBanDAL
    {
        // Chuỗi kết nối tới Access (lấy từ DatabaseHelper)
        private readonly string _conn = DatabaseHelper.ConnectionString;

        /// <summary>
        /// Lấy toàn bộ danh sách bàn (sắp xếp theo Id) để hiển thị lên sơ đồ bàn.
        /// </summary>
        public List<Ban> LayTatCa()
        {
            // Tạo list rỗng để chứa kết quả
            var ds = new List<Ban>();
            using var c = new OleDbConnection(_conn);
            c.Open();

            // SELECT 3 cột từ bảng Ban, ORDER BY Id để bàn 1-10 hiển thị đúng thứ tự
            using var cmd = new OleDbCommand("SELECT Id, TenBan, TrangThai FROM Ban ORDER BY Id", c);
            using var r = cmd.ExecuteReader();

            // Đọc từng dòng và thêm vào list
            while (r.Read())
                ds.Add(new Ban
                {
                    Id = r.GetInt32(0),
                    TenBan = r.GetString(1),
                    TrangThai = r.GetString(2)
                });
            return ds;
        }

        /// <summary>
        /// Lấy 1 bàn theo Id. Trả về null nếu không có.
        /// </summary>
        public Ban? LayTheoId(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            // WHERE Id = ? — tham số vị trí
            using var cmd = new OleDbCommand("SELECT Id, TenBan, TrangThai FROM Ban WHERE Id = ?", c);
            // OleDbType.Integer = số nguyên 4 byte (Access LONG)
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            using var r = cmd.ExecuteReader();
            if (r.Read())
                return new Ban { Id = r.GetInt32(0), TenBan = r.GetString(1), TrangThai = r.GetString(2) };
            return null;
        }

        /// <summary>
        /// Thêm bàn mới. Throw exception nếu tên bàn đã tồn tại.
        /// </summary>
        public void Them(Ban ban)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            // Bước 1: Kiểm tra trùng tên bàn
            using var chk = new OleDbCommand("SELECT COUNT(*) FROM Ban WHERE TenBan = ?", c);
            chk.Parameters.Add("@ten", OleDbType.VarWChar).Value = ban.TenBan;
            if ((int)chk.ExecuteScalar()! > 0)
                throw new Exception("Tên bàn đã tồn tại! Vui lòng đặt tên khác.");

            // Bước 2: INSERT bàn mới
            using var cmd = new OleDbCommand("INSERT INTO Ban (TenBan, TrangThai) VALUES (?, ?)", c);
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = ban.TenBan;
            cmd.Parameters.Add("@tt", OleDbType.VarWChar).Value = ban.TrangThai;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Cập nhật thông tin bàn (tên, trạng thái).
        /// Throw exception nếu tên bàn bị trùng với bàn khác.
        /// </summary>
        public void Sua(Ban ban)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            // Kiểm tra trùng tên nhưng LOẠI TRỪ bàn đang sửa (Id <> ?)
            using var chk = new OleDbCommand("SELECT COUNT(*) FROM Ban WHERE TenBan = ? AND Id <> ?", c);
            chk.Parameters.Add("@ten", OleDbType.VarWChar).Value = ban.TenBan;
            chk.Parameters.Add("@id", OleDbType.Integer).Value = ban.Id;
            if ((int)chk.ExecuteScalar()! > 0)
                throw new Exception("Tên bàn đã tồn tại! Vui lòng đặt tên khác.");

            // UPDATE — 3 tham số vị trí theo thứ tự SET..., WHERE
            using var cmd = new OleDbCommand("UPDATE Ban SET TenBan = ?, TrangThai = ? WHERE Id = ?", c);
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = ban.TenBan;
            cmd.Parameters.Add("@tt", OleDbType.VarWChar).Value = ban.TrangThai;
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = ban.Id;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Xoá bàn theo Id. Bảng HoaDon có FK với cascade delete → tự xoá theo.
        /// </summary>
        public void Xoa(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("DELETE FROM Ban WHERE Id = ?", c);
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Chỉ cập nhật trạng thái bàn ("Trống" / "Đã đặt" / "Có khách").
        /// Dùng khi: mở bàn (Trống → Có khách), đặt bàn (Trống → Đã đặt), v.v.
        /// </summary>
        public void CapNhatTrangThai(int id, string trangThai)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("UPDATE Ban SET TrangThai = ? WHERE Id = ?", c);
            cmd.Parameters.Add("@tt", OleDbType.VarWChar).Value = trangThai;
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            cmd.ExecuteNonQuery();
        }
    }
}
