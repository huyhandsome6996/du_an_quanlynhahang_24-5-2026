// ============================================================
// TẦNG DAL INTERFACE - INguyenLieuDAL
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    public interface INguyenLieuDAL
    {
        List<NguyenLieu> LayTatCa();
        NguyenLieu? LayTheoId(int id);
        void Them(NguyenLieu nl);
        void Sua(NguyenLieu nl);
        void Xoa(int id);
        void CapNhatSoLuongTon(int id, decimal soLuongMoi);
        List<NguyenLieu> LayCanhBao(); // Lấy danh sách nguyên liệu dưới mức tối thiểu
    }
}
