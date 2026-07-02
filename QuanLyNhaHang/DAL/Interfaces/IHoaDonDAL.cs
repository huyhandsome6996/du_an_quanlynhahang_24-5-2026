// ============================================================
// TẦNG DAL - Interface IHoaDonDAL
// ------------------------------------------------------------
// OOP: TRỪU TƯỢNG. Định nghĩa các thao tác với bảng HoaDon.
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các thao tác với hóa đơn.
    /// </summary>
    public interface IHoaDonDAL
    {
        /// <summary>Lấy tất cả hóa đơn (cho trang Lịch sử và Báo cáo).</summary>
        List<HoaDon> LayTatCa();

        /// <summary>Lấy 1 hóa đơn theo Id. Trả về null nếu không có.</summary>
        HoaDon? LayTheoId(int id);

        /// <summary>
        /// Lấy hóa đơn "Chưa thanh toán" của 1 bàn cụ thể.
        /// Mỗi bàn cùng 1 thời điểm chỉ có 1 hóa đơn chưa TT.
        /// </summary>
        HoaDon? LayHoaDonChuaThanhToanTheoBan(int banId);

        /// <summary>
        /// Thêm hóa đơn mới. Trả về Id tự tăng (dùng @@IDENTITY).
        /// </summary>
        int Them(HoaDon hoaDon);

        /// <summary>Cập nhật tổng tiền (khi thêm/xoá món).</summary>
        void CapNhatTongTien(int hoaDonId, decimal tongTien);

        /// <summary>Đánh dấu hóa đơn đã thanh toán + set thời gian thanh toán = now.</summary>
        void ThanhToan(int hoaDonId);

        /// <summary>Cập nhật VAT, giảm giá, phương thức thanh toán trước khi đóng hóa đơn.</summary>
        void CapNhatThanhToan(int hoaDonId, decimal vat, decimal giamGia, string phuongThuc);

        /// <summary>Lọc hóa đơn đã thanh toán theo khoảng ngày (cho trang Báo cáo).</summary>
        List<HoaDon> LayTheoKhoangNgay(DateTime tuNgay, DateTime denNgay);
    }
}
