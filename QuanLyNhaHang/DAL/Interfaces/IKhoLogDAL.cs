// ============================================================
// TẦNG DAL INTERFACE - IKhoLogDAL
// ============================================================
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL.Interfaces
{
    public interface IKhoLogDAL
    {
        List<KhoLog> LayTatCa();
        List<KhoLog> LayTheoNguyenLieu(int nguyenLieuId);
        void Them(KhoLog log);
    }
}
