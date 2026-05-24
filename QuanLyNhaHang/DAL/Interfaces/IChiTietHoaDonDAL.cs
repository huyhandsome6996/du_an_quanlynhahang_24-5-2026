// ============================================================
// TẦNG DAL - Interface IChiTietHoaDonDAL
// Thể hiện: Trừu tượng (Abstraction) của OOP
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các thao tác cho Chi Tiết Hóa Đơn.
    /// </summary>
    public interface IChiTietHoaDonDAL
    {
        List<ChiTietHoaDon> LayTheoHoaDon(int hoaDonId);
        void Them(ChiTietHoaDon chiTiet);
        void Xoa(int id);
    }
}
