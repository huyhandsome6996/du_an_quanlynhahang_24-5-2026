// ============================================================
// TẦNG DAL - Interface INguoiDungDAL
// ------------------------------------------------------------
// OOP thể hiện ở đây: TRỪU TƯỢNG (Abstraction).
// Interface định nghĩa "CÁI GÌ" cần làm, không quan tâm "LÀM THẾ NÀO".
// Lớp NguoiDungDAL sẽ implement interface này để chỉ ra cách thức cụ thể.
//
// Lợi ích:
//   - Nếu sau này muốn đổi từ Access sang MySQL/SQLite, chỉ cần tạo
//     NguoiDungDALMySQL : INguoiDungDAL, không phải sửa code ở tầng API.
//   - Trong DI: services.AddSingleton<INguoiDungDAL, NguoiDungDAL>()
//     → khi cần đổi implementation chỉ sửa 1 dòng.
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các thao tác cho Người Dùng (đăng nhập / đăng ký).
    /// </summary>
    public interface INguoiDungDAL
    {
        /// <summary>
        /// Tìm người dùng theo tên đăng nhập. Trả về null nếu không tìm thấy.
        /// Dùng cho: đăng nhập (lấy mật khẩu ra so sánh).
        /// </summary>
        NguoiDung? LayTheoTenDangNhap(string tenDangNhap);

        /// <summary>
        /// Thêm 1 người dùng mới vào bảng NguoiDung.
        /// Dùng cho: đăng ký tài khoản quản trị đầu tiên.
        /// </summary>
        void Them(NguoiDung nguoiDung);

        /// <summary>
        /// Kiểm tra trong bảng NguoiDung đã có dòng nào chưa.
        /// Dùng cho: quyết định form đăng nhập hay form đăng ký khi mở app.
        /// </summary>
        bool KiemTraCoNguoiDung();
    }
}
