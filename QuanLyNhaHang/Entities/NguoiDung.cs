// ============================================================
// TẦNG ENTITY - Lớp NguoiDung (Thực thể Người Dùng)
// ------------------------------------------------------------
// Lớp này đóng vai trò MODEL — chứa các thuộc tính của 1 tài khoản
// đăng nhập trong hệ thống. Mỗi 1 dòng trong bảng NguoiDung của
// file Access sẽ được ánh xạ thành 1 đối tượng NguoiDung.
//
// OOP thể hiện ở đây:
//   - ĐÓNG GÓI: các trường private _x, truy cập qua Properties
//   - Lớp dữ liệu thuần — không chứa logic nghiệp vụ
// ============================================================
namespace QuanLyNhaHang.Entities
{
    /// <summary>
    /// Lớp NguoiDung — đại diện cho 1 tài khoản đăng nhập hệ thống.
    /// Có 2 vai trò: "QuanTri" (toàn quyền) và "NhanVien" (chỉ phục vụ).
    /// </summary>
    public class NguoiDung
    {
        // Id — Khóa chính tự tăng trong Access, dùng để tham chiếu nội bộ
        public int Id { get; set; }

        // TenDangNhap — Tên đăng nhập, UNIQUE, người dùng tự chọn khi đăng ký
        public string TenDangNhap { get; set; } = string.Empty;

        // MatKhau — Mật khẩu lưu PLAIN-TEXT (không băm).
        // Đơn giản hoá cho đồ án nhỏ, học sinh có thể xem/sửa trực tiếp
        // trong file Access mà không cần tính lại SHA-256.
        public string MatKhau { get; set; } = string.Empty;

        // VaiTro — Phân quyền: "QuanTri" (admin) hoặc "NhanVien" (thu ngân/phục vụ)
        public string VaiTro { get; set; } = "QuanTri";

        // NgayTao — Thời điểm tạo tài khoản, mặc định = now
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
