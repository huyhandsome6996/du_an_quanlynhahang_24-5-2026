// ============================================================
// TẦNG DAL - Interface IHoaDonDAL
// Thể hiện: Trừu tượng (Abstraction) của OOP
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    public interface IHoaDonDAL
    {
        List<HoaDon> LayTatCa();
        HoaDon? LayTheoId(int id);
        HoaDon? LayHoaDonChuaThanhToanTheoBan(int banId);
        int Them(HoaDon hoaDon);
        void CapNhatTongTien(int hoaDonId, decimal tongTien);
        void ThanhToan(int hoaDonId);
        void CapNhatThanhToan(int hoaDonId, decimal vat, decimal giamGia, string phuongThuc);
        List<HoaDon> LayTheoKhoangNgay(DateTime tuNgay, DateTime denNgay);
    }
}
