// ============================================================
// TẦNG DAL - Interface IBanDAL
// Thể hiện: Trừu tượng (Abstraction) của OOP
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các thao tác CRUD cho Bàn.
    /// Mọi lớp DAL phải implement interface này → Trừu tượng hóa.
    /// </summary>
    public interface IBanDAL
    {
        List<Ban> LayTatCa();
        Ban? LayTheoId(int id);
        void Them(Ban ban);
        void Sua(Ban ban);
        void Xoa(int id);
        void CapNhatTrangThai(int id, string trangThai);
    }
}
