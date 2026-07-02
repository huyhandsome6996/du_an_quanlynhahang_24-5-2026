// ============================================================
// TẦNG ENTITY - Lớp NuocUong
// Thể hiện: Kế thừa (Inheritance) + Đa hình (Polymorphism)
// ============================================================
namespace QuanLyNhaHang.Entities
{
    /// <summary>
    /// Lớp NuocUong KẾ THỪA từ SanPham.
    /// Ghi đè TinhTien() theo nghiệp vụ riêng của Nước uống:
    ///   - "Lon" → nhân giá x1.2
    ///   - Mặc định (Ly) → giá gốc
    /// </summary>
    public class NuocUong : SanPham
    {
        // Hệ số khi khách chọn dạng "Lon" (đắt hơn 20%)
        private const decimal HE_SO_LON = 1.2m;

        public NuocUong()
        {
            Loai = "NuocUong";
        }

        // -------------------------------------------------------
        // ĐA HÌNH: Ghi đè phương thức TinhTien() từ lớp cha
        // -------------------------------------------------------
        public override decimal TinhTien(int soLuong, string thuocTinhThem)
        {
            decimal donGia = GiaCoBan;

            // Nếu khách chọn "Lon" thì giá nhân thêm 20%
            if (!string.IsNullOrEmpty(thuocTinhThem) &&
                thuocTinhThem.Contains("Lon", StringComparison.OrdinalIgnoreCase))
            {
                donGia = donGia * HE_SO_LON;
            }

            return donGia * soLuong;
        }

        public override string MoTaPhuPhi(string thuocTinhThem)
        {
            if (!string.IsNullOrEmpty(thuocTinhThem) &&
                thuocTinhThem.Contains("Lon", StringComparison.OrdinalIgnoreCase))
            {
                return "+20% (Dạng Lon)";
            }
            return "Không có phụ phí";
        }
    }
}
