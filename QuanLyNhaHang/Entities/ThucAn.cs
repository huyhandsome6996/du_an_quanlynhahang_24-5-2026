// ============================================================
// TẦNG ENTITY - Lớp ThucAn (KẾ THỪA từ SanPham)
// ------------------------------------------------------------
// OOP thể hiện ở đây:
//   - KẾ THỪA: ThucAn kế thừa toàn bộ thuộc tính từ SanPham
//   - ĐA HÌNH: Override TinhTien() với nghiệp vụ riêng của Thức ăn:
//     Nếu khách chọn "Phần lớn" → cộng thêm 50.000đ trên đơn giá
// ============================================================
namespace QuanLyNhaHang.Entities
{
    /// <summary>
    /// Lớp ThucAn — kế thừa từ SanPham.
    /// Cài đặt TinhTien() theo nghiệp vụ Thức ăn:
    ///   - "Phần lớn"  → cộng thêm 50.000đ trên mỗi phần
    ///   - Mặc định    → dùng giá gốc
    /// </summary>
    public class ThucAn : SanPham
    {
        // Hằng số phụ phí khi khách chọn "Phần lớn" (50.000đ)
        // Hằng số phải có hậu tố 'm' để chỉ decimal (không phải double)
        private const decimal PHU_PHI_PHAN_LON = 50000m;

        /// <summary>
        /// Constructor — tự động set Loai = "ThucAn" để DAL biết đây là thức ăn.
        /// </summary>
        public ThucAn()
        {
            Loai = "ThucAn";   // Gán loại ngay khi new để tránh quên
        }

        // -------------------------------------------------------
        // ĐA HÌNH: override TinhTien() từ lớp cha SanPham
        // -------------------------------------------------------
        /// <param name="soLuong">Số phần khách gọi</param>
        /// <param name="thuocTinhThem">Có thể chứa "Phần lớn" để tính phụ phí</param>
        /// <returns>Thành tiền = (Giá + Phụ phí) × Số lượng</returns>
        public override decimal TinhTien(int soLuong, string thuocTinhThem)
        {
            // Lấy giá gốc từ property của lớp cha
            decimal donGia = GiaCoBan;

            // Nếu chuỗi thuocTinhThem có chứa "Phần lớn" (không phân biệt hoa thường)
            // thì cộng thêm phụ phí 50.000đ
            if (!string.IsNullOrEmpty(thuocTinhThem) &&
                thuocTinhThem.Contains("Phần lớn", StringComparison.OrdinalIgnoreCase))
            {
                donGia += PHU_PHI_PHAN_LON;
            }

            // Thành tiền = đơn giá × số lượng
            return donGia * soLuong;
        }

        /// <summary>
        /// Đa hình: Trả về chuỗi mô tả phụ phí để hiển thị trên UI.
        /// </summary>
        public override string MoTaPhuPhi(string thuocTinhThem)
        {
            // Nếu có "Phần lớn" → trả chuỗi mô tả "+50,000đ (Phần lớn)"
            if (!string.IsNullOrEmpty(thuocTinhThem) &&
                thuocTinhThem.Contains("Phần lớn", StringComparison.OrdinalIgnoreCase))
            {
                return $"+50,000đ (Phần lớn)";
            }
            // Mặc định không có phụ phí
            return "Không có phụ phí";
        }
    }
}
