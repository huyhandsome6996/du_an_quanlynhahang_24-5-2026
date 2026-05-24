// ============================================================
// TẦNG DAL - Interface ISanPhamDAL
// Thể hiện: Trừu tượng (Abstraction) của OOP
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các thao tác CRUD cho Sản Phẩm (Menu).
    /// </summary>
    public interface ISanPhamDAL
    {
        List<SanPham> LayTatCa();
        List<SanPham> LayDangBan(); // Chỉ lấy món đang phục vụ
        SanPham? LayTheoId(int id);
        void Them(SanPham sanPham);
        void Sua(SanPham sanPham);
        void Xoa(int id);
    }
}
