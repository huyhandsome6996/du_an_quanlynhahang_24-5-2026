// ============================================================
// TẦNG ENTITY - Lớp Ban (Thực thể Bàn)
// ============================================================
namespace QuanLyNhaHang.Entities
{
    public class Ban
    {
        public int Id { get; set; }
        public string TenBan { get; set; } = string.Empty;
        // Trạng thái: "Trống" hoặc "Có khách"
        public string TrangThai { get; set; } = "Trống";
    }
}
