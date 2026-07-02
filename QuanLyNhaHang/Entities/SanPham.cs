// ============================================================
// TẦNG ENTITY - Lớp SanPham (Thực thể Sản Phẩm - LỚP TRỪU TƯỢNG)
// ------------------------------------------------------------
// Đây là LỚP CHA trừu tượng cho 2 lớp con: ThucAn và NuocUong.
//
// OOP thể hiện ở đây:
//   - TRỪU TƯỢNG (Abstraction): Lớp abstract, không thể new SanPham() trực tiếp
//   - ĐÓNG GÓI (Encapsulation): Các trường private, truy cập qua Properties có validate
//   - ĐA HÌNH (Polymorphism): Phương thức abstract TinhTien() — mỗi lớp con
//     tự cài đặt cách tính tiền riêng (ThucAn: +50k nếu "Phần lớn";
//     NuocUong: ×1.2 nếu "Lon")
// ============================================================
namespace QuanLyNhaHang.Entities
{
    /// <summary>
    /// LỚP TRỪU TƯỢNG SanPham — Không thể khởi tạo trực tiếp.
    /// Bắt buộc phải tạo qua lớp con ThucAn hoặc NuocUong.
    /// </summary>
    public abstract class SanPham
    {
        // -------------------------------------------------------
        // ĐÓNG GÓI: Các trường private bên dưới được truy cập
        // qua Properties. Lý do: có thể thêm validate ở setter
        // (ví dụ: không cho tên rỗng, không cho giá âm).
        // -------------------------------------------------------
        private int _id;                     // Trường private chứa Id
        private string _tenSanPham = string.Empty;  // Trường private chứa tên
        private decimal _giaCoBan;            // Trường private chứa giá
        private bool _dangBan;                // Trường private chứa trạng thái bán

        // Property Id — chỉ là get/set đơn giản (không validate)
        public int Id
        {
            get => _id;
            set => _id = value;
        }

        // Property TenSanPham — Có VALIDATE: không cho rỗng
        public string TenSanPham
        {
            get => _tenSanPham;
            set
            {
                // Nếu tên rỗng hoặc chỉ khoảng trắng → ném exception
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Tên sản phẩm không được để trống!");
                // Trim khoảng trắng 2 đầu trước khi lưu
                _tenSanPham = value.Trim();
            }
        }

        // Property GiaCoBan — Có VALIDATE: không cho giá âm
        public decimal GiaCoBan
        {
            get => _giaCoBan;
            set
            {
                // Giá không được âm (cho phép = 0 để test)
                if (value < 0)
                    throw new ArgumentException("Giá sản phẩm không được âm!");
                _giaCoBan = value;
            }
        }

        // Loai — "ThucAn" hoặc "NuocUong". Lớp con sẽ set trong constructor.
        public string Loai { get; set; } = string.Empty;

        // Property DangBan — true: vẫn bán, false: ngừng bán
        public bool DangBan
        {
            get => _dangBan;
            set => _dangBan = value;
        }

        // HinhAnh — Đường dẫn URL hoặc chuỗi Base64 của ảnh món
        public string? HinhAnh { get; set; }

        // -------------------------------------------------------
        // TRỪU TƯỢNG + ĐA HÌNH:
        // Phương thức abstract — bắt buộc mỗi lớp con phải override
        // -------------------------------------------------------

        /// <summary>
        /// Tính thành tiền khi khách gọi số lượng + thuộc tính thêm.
        /// - ThucAn: nếu "Phần lớn" → cộng 50.000đ/phần
        /// - NuocUong: nếu "Lon" → giá × 1.2
        /// Đây chính là ĐA HÌNH: cùng 1 phương thức, nhiều cách thực thi.
        /// </summary>
        /// <param name="soLuong">Số lượng khách gọi</param>
        /// <param name="thuocTinhThem">Thuộc tính thêm: "Phần lớn" / "Lon" / "Không hành"...</param>
        /// <returns>Thành tiền</returns>
        public abstract decimal TinhTien(int soLuong, string thuocTinhThem);

        /// <summary>
        /// Lấy mô tả phụ phí để hiển thị lên UI (vd "+50,000đ (Phần lớn)").
        /// Đa hình: mỗi lớp con có cách mô tả riêng.
        /// </summary>
        public abstract string MoTaPhuPhi(string thuocTinhThem);
    }
}
