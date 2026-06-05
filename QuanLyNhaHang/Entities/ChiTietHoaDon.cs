// ============================================================
// TẦNG ENTITY - Lớp ChiTietHoaDon (Chi tiết Hóa Đơn)
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
        public decimal DonGiaBan { get; set; }
        public string ThuocTinhThem { get; set; } = string.Empty;
        public decimal ThanhTien { get; set; }
        // Trạng thái món: "DangCho" / "DangChuanBi" / "DaPhucVu"
        public string TrangThaiMon { get; set; } = "DangCho";
    }
}
