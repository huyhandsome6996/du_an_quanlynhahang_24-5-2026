// ============================================================
// TẦNG ENTITY - Lớp ChiTietHoaDon (Thực thể Chi Tiết Hóa Đơn)
// ============================================================
namespace QuanLyNhaHang.Entities
{
    public class ChiTietHoaDon
    {
        public int Id { get; set; }
        public int HoaDonId { get; set; }
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; } = string.Empty; // Để hiển thị UI
        public int SoLuong { get; set; }
        // Lưu giá tại thời điểm gọi (phòng nhà hàng đổi giá sau)
        public decimal DonGiaBan { get; set; }
        // Ghi chú của khách: "Phần lớn", "Không hành", "Lon"...
        public string ThuocTinhThem { get; set; } = string.Empty;
        // ThanhTien = (DonGiaBan + phụ phí) * SoLuong
        public decimal ThanhTien { get; set; }
    }
}
