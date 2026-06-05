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
        // VAT 10% tự động
        public decimal VAT { get; set; } = 0;
        // Số tiền giảm giá
        public decimal GiamGia { get; set; } = 0;
        // Phương thức thanh toán: TienMat, The, QR, ChuyenKhoan
        public string PhuongThucThanhToan { get; set; } = "TienMat";
        // Trạng thái: "Chưa thanh toán" hoặc "Đã thanh toán"
        public string TrangThai { get; set; } = "Chưa thanh toán";
    }
}
