// ============================================================
// TẦNG ENTITY - Lớp NuocUong (KẾ THỪA từ SanPham)
// ------------------------------------------------------------
// OOP thể hiện ở đây:
//   - KẾ THỪA: NuocUong kế thừa toàn bộ thuộc tính từ SanPham
//   - ĐA HÌNH: Override TinhTien() với nghiệp vụ riêng của Nước uống:
//     Nếu khách chọn "Lon" → giá × 1.2 (đắt hơn 20%)
// ============================================================
namespace QuanLyNhaHang.Entities
{
    /// <summary>
    /// Lớp NuocUong — kế thừa từ SanPham.
    /// Cài đặt TinhTien() theo nghiệp vụ Nước uống:
    ///   - "Lon" → giá × 1.2 (đắt hơn 20% so với ly)
    ///   - Mặc định (Ly) → dùng giá gốc
    /// </summary>
    public class NuocUong : SanPham
    {
        // Hệ số nhân giá khi khách chọn dạng "Lon" (1.2 = đắt hơn 20%)
        private const decimal HE_SO_LON = 1.2m;

        /// <summary>
        /// Constructor — tự động set Loai = "NuocUong" để DAL biết đây là nước uống.
        /// </summary>
        public NuocUong()
        {
            Loai = "NuocUong";   // Gán loại ngay khi new để tránh quên
        }

        // -------------------------------------------------------
        // ĐA HÌNH: override TinhTien() từ lớp cha SanPham
        // -------------------------------------------------------
        /// <param name="soLuong">Số lượng khách gọi</param>
        /// <param name="thuocTinhThem">Có thể chứa "Lon" để tính phụ phí</param>
        /// <returns>Thành tiền = (Giá × Hệ số) × Số lượng</returns>
        public override decimal TinhTien(int soLuong, string thuocTinhThem)
        {
            // Lấy giá gốc từ property của lớp cha
            decimal donGia = GiaCoBan;

            // Nếu chuỗi thuocTinhThem có chứa "Lon" → nhân giá với hệ số 1.2
            if (!string.IsNullOrEmpty(thuocTinhThem) &&
                thuocTinhThem.Contains("Lon", StringComparison.OrdinalIgnoreCase))
            {
                donGia = donGia * HE_SO_LON;
            }

            // Thành tiền = đơn giá × số lượng
            return donGia * soLuong;
        }

        /// <summary>
        /// Đa hình: Trả về chuỗi mô tả phụ phí để hiển thị trên UI.
        /// </summary>
        public override string MoTaPhuPhi(string thuocTinhThem)
        {
            // Nếu có "Lon" → trả chuỗi mô tả "+20% (Dạng Lon)"
            if (!string.IsNullOrEmpty(thuocTinhThem) &&
                thuocTinhThem.Contains("Lon", StringComparison.OrdinalIgnoreCase))
            {
                return "+20% (Dạng Lon)";
            }
            // Mặc định không có phụ phí
            return "Không có phụ phí";
        }
    }
}
