// ============================================================
// TẦNG ENTITY - Lớp ChiTietHoaDon (Chi tiết Hóa Đơn)
// ------------------------------------------------------------
// Mỗi 1 đối tượng ChiTietHoaDon = 1 dòng trong bảng ChiTietHoaDon
// của file Access. Đại diện cho 1 món được gọi trong 1 hóa đơn.
//
// Mối quan hệ:
//   - 1 HoaDon có nhiều ChiTietHoaDon (1—N)
//   - 1 SanPham có nhiều ChiTietHoaDon (1—N)
//   → Vậy ChiTietHoaDon là bảng TRUNG GIAN giữa HoaDon và SanPham
// ============================================================
namespace QuanLyNhaHang.Entities
{
    /// <summary>
    /// Lớp ChiTietHoaDon — đại diện cho 1 dòng món trong hóa đơn.
    /// </summary>
    public class ChiTietHoaDon
    {
        // Id — Khóa chính tự tăng
        public int Id { get; set; }

        // HoaDonId — FK tham chiếu tới HoaDon.Id
        public int HoaDonId { get; set; }

        // SanPhamId — FK tham chiếu tới SanPham.Id
        public int SanPhamId { get; set; }

        // TenSanPham — Chỉ dùng để HIỂN THỊ (lấy qua JOIN với bảng SanPham)
        public string TenSanPham { get; set; } = string.Empty;

        // SoLuong — Số phần/lon/ly khách gọi
        public int SoLuong { get; set; }

        // DonGiaBan — Đơn giá đã tính phụ phí (Phần lớn +50k / Lon ×1.2)
        // = ThanhTien / SoLuong (lưu để hiển thị lại khi xem lịch sử)
        public decimal DonGiaBan { get; set; }

        // ThuocTinhThem — Ghi chú khách yêu cầu: "Phần lớn", "Lon", "Ít cay"...
        public string ThuocTinhThem { get; set; } = string.Empty;

        // ThanhTien = DonGiaBan × SoLuong (đã được server tính bằng TinhTien())
        public decimal ThanhTien { get; set; }

        // TrangThaiMon — Trạng thái bếp/bar:
        //   "DangCho"      : Vừa gửi order, đang chờ bếp nhận
        //   "DangChuanBi"  : Bếp đang nấu
        //   "DaPhucVu"     : Đã mang ra cho khách
        public string TrangThaiMon { get; set; } = "DangCho";
    }
}
