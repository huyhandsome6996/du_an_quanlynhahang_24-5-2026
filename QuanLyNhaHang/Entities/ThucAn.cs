// ============================================================
// TẦNG ENTITY - Lớp ThucAn
// Thể hiện: Kế thừa (Inheritance) + Đa hình (Polymorphism)
// ============================================================
namespace QuanLyNhaHang.Entities
{
    /// <summary>
    /// Lớp ThucAn KẾ THỪA từ SanPham.
    /// Ghi đè TinhTien() theo nghiệp vụ riêng của Thức ăn:
    ///   - "Phần lớn" → cộng thêm 50.000đ/phần
    ///   - Mặc định → giá gốc
    /// </summary>
    public class ThucAn : SanPham
    {
        // Phụ phí khi khách chọn "Phần lớn" (50,000 VNĐ)
        private const decimal PHU_PHI_PHAN_LON = 50000m;

        public ThucAn()
        {
            Loai = "ThucAn";
        }

        // -------------------------------------------------------
        // ĐA HÌNH: Ghi đè phương thức TinhTien() từ lớp cha
        // -------------------------------------------------------
        public override decimal TinhTien(int soLuong, string thuocTinhThem)
        {
            decimal donGia = GiaCoBan;

            // Nếu khách chọn "Phần lớn" thì cộng thêm 50,000
            if (!string.IsNullOrEmpty(thuocTinhThem) &&
                thuocTinhThem.Contains("Phần lớn", StringComparison.OrdinalIgnoreCase))
            {
                donGia += PHU_PHI_PHAN_LON;
            }

            return donGia * soLuong;
        }

        public override string MoTaPhuPhi(string thuocTinhThem)
        {
            if (!string.IsNullOrEmpty(thuocTinhThem) &&
                thuocTinhThem.Contains("Phần lớn", StringComparison.OrdinalIgnoreCase))
            {
                return $"+50,000đ (Phần lớn)";
            }
            return "Không có phụ phí";
        }
    }
}
