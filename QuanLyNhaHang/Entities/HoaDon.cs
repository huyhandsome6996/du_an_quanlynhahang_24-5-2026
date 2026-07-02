// ============================================================
// TẦNG ENTITY - Lớp HoaDon (Thực thể Hóa Đơn)
// ------------------------------------------------------------
// Mỗi 1 đối tượng HoaDon = 1 dòng trong bảng HoaDon của file Access.
// Mối quan hệ:
//   - 1 Ban có thể có nhiều HoaDon (1—N)
//   - 1 HoaDon có nhiều ChiTietHoaDon (1—N)
//
// Vòng đời 1 hóa đơn:
//   1. Bàn trống → click "Mở bàn" → tạo HoaDon mới (TrangThai="Chưa thanh toán")
//   2. Khách gọi món → thêm ChiTietHoaDon, cập nhật TongTien
//   3. Khách trả tiền → cập nhật TrangThai="Đã thanh toán", ThoiGianThanhToan=now
//   4. Bàn tự chuyển về "Trống" để đón khách mới
// ============================================================
namespace QuanLyNhaHang.Entities
{
    /// <summary>
    /// Lớp HoaDon — đại diện cho 1 hóa đơn của 1 bàn.
    /// </summary>
    public class HoaDon
    {
        // Id — Khóa chính tự tăng trong Access
        public int Id { get; set; }

        // BanId — Khóa ngoại tham chiếu tới Ban.Id
        public int BanId { get; set; }

        // TenBan — Chỉ dùng để HIỂN THỊ trên UI (lấy qua JOIN với bảng Ban)
        public string TenBan { get; set; } = string.Empty;

        // ThoiGianTao — Thời điểm mở bàn (khách vừa ngồi vào)
        public DateTime ThoiGianTao { get; set; }

        // ThoiGianThanhToan — Nullable: null khi chưa thanh toán
        public DateTime? ThoiGianThanhToan { get; set; }

        // TongTien — Tổng tiền món (chưa gồm VAT, chưa trừ giảm giá)
        public decimal TongTien { get; set; } = 0;

        // VAT — Tiền thuế GTGT (thường 10% của TongTien)
        public decimal VAT { get; set; } = 0;

        // GiamGia — Số tiền giảm giá (ví dụ: khách có voucher 20.000đ)
        public decimal GiamGia { get; set; } = 0;

        // PhuongThucThanhToan — 1 trong 4: "TienMat" / "The" / "QR" / "ChuyenKhoan"
        public string PhuongThucThanhToan { get; set; } = "TienMat";

        // TrangThai — "Chưa thanh toán" hoặc "Đã thanh toán"
        public string TrangThai { get; set; } = "Chưa thanh toán";
    }
}
