// ============================================================
// TẦNG DAL - Interface IChiTietHoaDonDAL
// Thể hiện: Trừu tượng (Abstraction) của OOP
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    public interface IChiTietHoaDonDAL
    {
        List<ChiTietHoaDon> LayTheoHoaDon(int hoaDonId);
        void Them(ChiTietHoaDon chiTiet);
        void Xoa(int id);
        void CapNhatTrangThaiMon(int id, string trangThai);
        List<ChiTietHoaDon> LayMonDangCho(); // Món đang chờ bếp
        List<ChiTietHoaDon> LayMonDangChuanBi(); // Món đang chuẩn bị
    }
}
