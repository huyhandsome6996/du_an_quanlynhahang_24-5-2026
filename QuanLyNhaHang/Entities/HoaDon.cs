// ============================================================
// TẦNG ENTITY - Lớp HoaDon (Thực thể Hóa Đơn)
// ============================================================
namespace QuanLyNhaHang.Entities
{
    public class HoaDon
    {
        public int Id { get; set; }
        public int BanId { get; set; }
        public string TenBan { get; set; } = string.Empty; // Để hiển thị UI
        public DateTime ThoiGianTao { get; set; }
        public DateTime? ThoiGianThanhToan { get; set; } // Nullable: chưa thanh toán
        public decimal TongTien { get; set; } = 0;
        // Trạng thái: "Chưa thanh toán" hoặc "Đã thanh toán"
        public string TrangThai { get; set; } = "Chưa thanh toán";
    }
}
