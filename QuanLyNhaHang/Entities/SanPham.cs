// ============================================================
// TẦNG ENTITY - Lớp SanPham (Thực thể Sản Phẩm)
// Thể hiện: Đóng gói (Encapsulation) + Trừu tượng (Abstraction)
// ============================================================
namespace QuanLyNhaHang.Entities
{
    /// <summary>
    /// Lớp TRỪU TƯỢNG - không thể tạo đối tượng trực tiếp.
    /// Bắt buộc các lớp con (ThucAn, NuocUong) phải cài đặt TinhTien().
    /// </summary>
    public abstract class SanPham
    {
        // -------------------------------------------------------
        // ĐÓNG GÓI: Các trường private, truy cập qua Properties
        // -------------------------------------------------------
        private int _id;
        private string _tenSanPham = string.Empty;
        private decimal _giaCoBan;
        private bool _dangBan;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string TenSanPham
        {
            get => _tenSanPham;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Tên sản phẩm không được để trống!");
                _tenSanPham = value.Trim();
            }
        }

        // ĐÓNG GÓI: Validate giá không được âm
        public decimal GiaCoBan
        {
            get => _giaCoBan;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Giá sản phẩm không được âm!");
                _giaCoBan = value;
            }
        }

        // Loại sản phẩm: "ThucAn" hoặc "NuocUong"
        public string Loai { get; set; } = string.Empty;

        public bool DangBan
        {
            get => _dangBan;
            set => _dangBan = value;
        }

        // URL hoặc đường dẫn ảnh minh họa
        public string? HinhAnh { get; set; }

        // -------------------------------------------------------
        // TRỪU TƯỢNG: Phương thức bắt buộc lớp con phải ghi đè
        // Đây là nền tảng của Đa hình (Polymorphism)
        // -------------------------------------------------------
        /// <summary>
        /// Tính thành tiền dựa trên số lượng và thuộc tính thêm của khách.
        /// Mỗi loại (ThucAn/NuocUong) sẽ tính khác nhau → Đa hình.
        /// </summary>
        public abstract decimal TinhTien(int soLuong, string thuocTinhThem);

        /// <summary>
        /// Lấy mô tả phụ phí để hiển thị trên hóa đơn.
        /// </summary>
        public abstract string MoTaPhuPhi(string thuocTinhThem);
    }
}
