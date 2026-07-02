// ============================================================
// TẦNG DAL - NguoiDungDAL (Access + OLE DB)
// ------------------------------------------------------------
// Implement interface INguoiDungDAL. Truy xuất bảng NguoiDung
// trong file Access bằng OLE DB Provider.
//
// LƯU Ý QUAN TRỌNG VỀ OLE DB:
//   1. Dùng tham số vị trí "?" (KHÔNG dùng @ten như SQLite/SQL Server).
//   2. Phải dùng Parameters.Add(name, OleDbType.X).Value = ...
//      KHÔNG dùng AddWithValue() — vì AddWithValue tự đoán kiểu có thể sai
//      dẫn tới lỗi "Data type mismatch in criteria expression".
//   3. Mapping kiểu OLE DB → Access:
//      - string  → OleDbType.VarWChar (chuỗi Unicode)
//      - int     → OleDbType.Integer
//      - DateTime→ OleDbType.Date
//      - decimal → OleDbType.Currency (tiền tệ)
//      - bool    → OleDbType.Boolean
//
// MẬT KHẨU LƯU PLAIN-TEXT (không băm SHA-256) — đồ án đơn giản hoá.
// ============================================================
using System.Data.OleDb;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    /// <summary>
    /// Lớp NguoiDungDAL — thao tác với bảng NguoiDung trong file Access.
    /// </summary>
    public class NguoiDungDAL : INguoiDungDAL
    {
        // Chuỗi kết nối — lấy từ DatabaseHelper (lớp tĩnh chung)
        private readonly string _conn = DatabaseHelper.ConnectionString;

        /// <summary>
        /// Kiểm tra trong bảng NguoiDung đã có dòng nào chưa.
        /// Trả về true nếu đã có ít nhất 1 user (→ mở form đăng nhập),
        /// false nếu bảng rỗng (→ mở form đăng ký tài khoản đầu tiên).
        /// </summary>
        public bool KiemTraCoNguoiDung()
        {
            // using đảm bảo đóng connection sau khi thoát khối
            using var c = new OleDbConnection(_conn);
            c.Open();   // Mở kết nối tới Access

            // Đếm số dòng trong bảng NguoiDung
            using var cmd = new OleDbCommand("SELECT COUNT(*) FROM NguoiDung", c);
            // ExecuteScalar trả về giá trị đầu của dòng đầu (tức là số lượng)
            return (int)cmd.ExecuteScalar()! > 0;
        }

        /// <summary>
        /// Tìm người dùng theo tên đăng nhập.
        /// Trả về đối tượng NguoiDung nếu tìm thấy, ngược lại trả về null.
        /// Dùng cho: đăng nhập (lấy mật khẩu ra so sánh).
        /// </summary>
        public NguoiDung? LayTheoTenDangNhap(string tenDangNhap)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            // Câu SELECT — dùng tham số vị trí "?" (KHÔNG dùng @ten)
            using var cmd = new OleDbCommand(
                "SELECT Id, TenDangNhap, MatKhau, VaiTro, NgayTao FROM NguoiDung WHERE TenDangNhap = ?", c);

            // Truyền giá trị cho tham số vị trí đầu tiên
            // OleDbType.VarWChar = chuỗi Unicode (Access TEXT)
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = tenDangNhap;

            // ExecuteReader → trả về DataReader để duyệt kết quả
            using var r = cmd.ExecuteReader();

            // Nếu có dòng → đọc dòng đó và tạo object NguoiDung
            if (r.Read())
                return new NguoiDung
                {
                    Id = r.GetInt32(0),                  // Cột 0: Id
                    TenDangNhap = r.GetString(1),        // Cột 1: TenDangNhap
                    MatKhau = r.GetString(2),            // Cột 2: MatKhau (plain-text)
                    VaiTro = r.GetString(3),             // Cột 3: VaiTro
                    NgayTao = r.GetDateTime(4)           // Cột 4: NgayTao
                };
            // Không tìm thấy → trả null
            return null;
        }

        /// <summary>
        /// Thêm 1 người dùng mới vào bảng NguoiDung.
        /// Throw exception nếu tên đăng nhập đã tồn tại.
        /// Dùng cho: đăng ký tài khoản quản trị đầu tiên khi mới cài app.
        /// </summary>
        public void Them(NguoiDung nd)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            // Bước 1: Kiểm tra trùng tên đăng nhập (để báo lỗi thân thiện)
            using var chk = new OleDbCommand(
                "SELECT COUNT(*) FROM NguoiDung WHERE TenDangNhap = ?", c);
            chk.Parameters.Add("@ten", OleDbType.VarWChar).Value = nd.TenDangNhap;
            if ((int)chk.ExecuteScalar()! > 0)
                throw new Exception("Tên đăng nhập đã tồn tại! Vui lòng chọn tên khác.");

            // Bước 2: INSERT dòng mới
            // 4 tham số vị trí theo đúng thứ tự cột trong SQL
            using var cmd = new OleDbCommand(
                "INSERT INTO NguoiDung (TenDangNhap, MatKhau, VaiTro, NgayTao) " +
                "VALUES (?, ?, ?, ?)", c);
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = nd.TenDangNhap;
            cmd.Parameters.Add("@mk", OleDbType.VarWChar).Value = nd.MatKhau;        // plain-text
            cmd.Parameters.Add("@vaiTro", OleDbType.VarWChar).Value = nd.VaiTro;
            cmd.Parameters.Add("@ngay", OleDbType.Date).Value = nd.NgayTao;
            cmd.ExecuteNonQuery();   // Thực thi INSERT
        }
    }
}
