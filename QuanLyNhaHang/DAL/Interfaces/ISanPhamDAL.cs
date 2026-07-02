// ============================================================
// TẦNG DAL - Interface ISanPhamDAL
// ------------------------------------------------------------
// OOP: TRỪU TƯỢNG. Định nghĩa các thao tác CRUD cho bảng SanPham.
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các thao tác CRUD cho Sản Phẩm (Thực đơn).
    /// </summary>
    public interface ISanPhamDAL
    {
        /// <summary>Lấy tất cả sản phẩm (kể cả món đang ngừng bán).</summary>
        List<SanPham> LayTatCa();

        /// <summary>Chỉ lấy món đang bán (DangBan = true) để hiển thị trên trang Gọi món.</summary>
        List<SanPham> LayDangBan();

        /// <summary>Lấy 1 sản phẩm theo Id. Trả về null nếu không có.</summary>
        SanPham? LayTheoId(int id);

        /// <summary>Thêm sản phẩm mới (ThucAn hoặc NuocUong). Throw nếu trùng tên.</summary>
        void Them(SanPham sanPham);

        /// <summary>Sửa sản phẩm. Throw nếu trùng tên với sản phẩm khác.</summary>
        void Sua(SanPham sanPham);

        /// <summary>Xoá sản phẩm theo Id.</summary>
        void Xoa(int id);
    }
}
