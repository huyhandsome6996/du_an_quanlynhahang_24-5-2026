// ============================================================
// TẦNG DAL - Interface IBanDAL
// ------------------------------------------------------------
// OOP thể hiện ở đây: TRỪU TƯỢNG (Abstraction).
// Định nghĩa các thao tác CRUD (Create-Read-Update-Delete)
// cho bảng Ban. Mọi lớp DAL cụ thể (BanDAL) phải implement.
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    /// <summary>
    /// Interface định nghĩa các thao tác CRUD cho Bàn.
    /// </summary>
    public interface IBanDAL
    {
        /// <summary>Lấy toàn bộ danh sách bàn (để hiển thị lên sơ đồ bàn).</summary>
        List<Ban> LayTatCa();

        /// <summary>Lấy 1 bàn theo Id. Trả về null nếu không có.</summary>
        Ban? LayTheoId(int id);

        /// <summary>Thêm bàn mới. Throw exception nếu trùng tên.</summary>
        void Them(Ban ban);

        /// <summary>Cập nhật thông tin bàn (tên, trạng thái).</summary>
        void Sua(Ban ban);

        /// <summary>Xoá bàn theo Id. Bảng HoaDon liên quan cũng tự xoá theo (cascade).</summary>
        void Xoa(int id);

        /// <summary>Chỉ cập nhật trạng thái bàn ("Trống" / "Đã đặt" / "Có khách").</summary>
        void CapNhatTrangThai(int id, string trangThai);
    }
}
