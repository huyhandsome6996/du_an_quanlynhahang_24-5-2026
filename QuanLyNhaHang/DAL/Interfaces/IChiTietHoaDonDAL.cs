// ============================================================
// TẦNG DAL - Interface IChiTietHoaDonDAL
// ------------------------------------------------------------
// OOP: TRỪU TƯỢNG. Định nghĩa các thao tác với bảng ChiTietHoaDon.
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các thao tác với chi tiết hóa đơn (từng món trong HĐ).
    /// </summary>
    public interface IChiTietHoaDonDAL
    {
        /// <summary>Lấy danh sách món của 1 hóa đơn (để hiển thị lên bảng).</summary>
        List<ChiTietHoaDon> LayTheoHoaDon(int hoaDonId);

        /// <summary>Thêm 1 món vào hóa đơn (khi khách gọi món).</summary>
        void Them(ChiTietHoaDon chiTiet);

        /// <summary>Xoá 1 món khỏi hóa đơn (khi khách đổi ý).</summary>
        void Xoa(int id);

        /// <summary>Cập nhật trạng thái món (DangCho → DangChuanBi → DaPhucVu).</summary>
        void CapNhatTrangThaiMon(int id, string trangThai);

        /// <summary>Lấy các món đang chờ bếp xử lý (của hóa đơn chưa TT).</summary>
        List<ChiTietHoaDon> LayMonDangCho();

        /// <summary>Lấy các món đang được bếp chuẩn bị.</summary>
        List<ChiTietHoaDon> LayMonDangChuanBi();
    }
}
