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
    /// Interface định nghĩa các thao tác cho Người Dùng (đăng nhập / đăng ký / quản lý).
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
        /// Dùng cho: đăng ký tài khoản quản trị đầu tiên hoặc Quản trị viên tạo tài khoản nhân viên.
        /// </summary>
        void Them(NguoiDung nguoiDung);

        /// <summary>
        /// Kiểm tra trong bảng NguoiDung đã có dòng nào chưa.
        /// Dùng cho: quyết định form đăng nhập hay form đăng ký khi mở app.
        /// </summary>
        bool KiemTraCoNguoiDung();

        // =====================================================
        // CÁC HÀM DÀNH CHO QUẢN TRỊ VIÊN (Quản lý tài khoản)
        // =====================================================

        /// <summary>
        /// Lấy danh sách tất cả người dùng. Dùng cho trang Quản lý tài khoản.
        /// KHÔNG trả về trường MatKhau để tránh lộ mật khẩu qua JSON.
        /// </summary>
        List<NguoiDung> LayTatCa();

        /// <summary>
        /// Lấy người dùng theo Id. Dùng cho việc xoá / reset mật khẩu.
        /// </summary>
        NguoiDung? LayTheoId(int id);

        /// <summary>
        /// Cập nhật mật khẩu mới (plain-text). Dùng cho reset mật khẩu.
        /// </summary>
        void CapNhatMatKhau(int id, string matKhauMoi);

        /// <summary>
        /// Xoá 1 người dùng theo Id.
        /// Lưu ý: lớp DAL không kiểm tra ràng buộc nghiệp vụ
        /// (không xoá chính mình, không xoá QuanTri cuối cùng) —
        /// việc đó nằm ở tầng API.
        /// </summary>
        void Xoa(int id);

        /// <summary>
        /// Đếm số lượng QuanTri hiện có. Dùng để chặn xoá QuanTri cuối cùng.
        /// </summary>
        int DemSoQuanTri();
    }
}
