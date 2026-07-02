// ============================================================
// TẦNG DAL - Interface INguoiDungDAL
// Thể hiện: Trừu tượng (Abstraction) của OOP
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các thao tác cho Người Dùng.
    /// </summary>
    public interface INguoiDungDAL
    {
        NguoiDung? LayTheoTenDangNhap(string tenDangNhap);
        void Them(NguoiDung nguoiDung);
        bool KiemTraCoNguoiDung();
    }
}
