// ============================================================
// TẦNG DAL - Interface IHoaDonDAL
// Thể hiện: Trừu tượng (Abstraction) của OOP
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các thao tác cho Hóa Đơn.
    /// </summary>
    public interface IHoaDonDAL
    {
        List<HoaDon> LayTatCa();
        HoaDon? LayTheoId(int id);
        HoaDon? LayHoaDonChuaThanhToanTheoBan(int banId);
        int Them(HoaDon hoaDon); // Trả về ID vừa tạo
        void CapNhatTongTien(int hoaDonId, decimal tongTien);
        void ThanhToan(int hoaDonId);
    }
}
