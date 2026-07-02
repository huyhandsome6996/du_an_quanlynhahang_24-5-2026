// ============================================================
// TẦNG ENTITY - Lớp Ban (Thực thể Bàn ăn)
// ------------------------------------------------------------
// Mỗi 1 đối tượng Ban = 1 dòng trong bảng Ban của file Access.
// Bàn có thể ở 1 trong 3 trạng thái:
//   - "Trống"     : Chưa có khách, có thể đón khách mới
//   - "Đã đặt"    : Khách gọi điện đặt trước, chưa tới
//   - "Có khách"  : Đang phục vụ, đã có hóa đơn mở
// ============================================================
namespace QuanLyNhaHang.Entities
{
    /// <summary>
    /// Lớp Ban — đại diện cho 1 chiếc bàn trong nhà hàng.
    /// Quản lý trạng thái để biết bàn nào đang trống / đã đặt / có khách.
    /// </summary>
    public class Ban
    {
        // Id — Khóa chính tự tăng
        public int Id { get; set; }

        // TenBan — Tên hiển thị: "Bàn 1", "Bàn 2", "Bàn VIP"...
        public string TenBan { get; set; } = string.Empty;

        // TrangThai — 1 trong 3 giá trị: "Trống" / "Đã đặt" / "Có khách"
        public string TrangThai { get; set; } = "Trống";
    }
}
