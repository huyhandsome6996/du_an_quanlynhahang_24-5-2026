// ============================================================
// TẦNG ENTITY - Lớp NguoiDung (Thực thể Người Dùng)
// ============================================================
namespace QuanLyNhaHang.Entities
{
    public class NguoiDung
    {
        public int Id { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        // Mật khẩu đã được băm SHA256 - KHÔNG LƯU PLAIN-TEXT
        public string MatKhauHash { get; set; } = string.Empty;
        public string VaiTro { get; set; } = "QuanTri"; // QuanTri hoặc NhanVien
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
