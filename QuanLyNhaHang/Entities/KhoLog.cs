// ============================================================
// TẦNG ENTITY - Lớp KhoLog (Nhật ký Nhập/Xuất Kho)
// ============================================================
namespace QuanLyNhaHang.Entities
{
    public class KhoLog
    {
        private int _id;
        private string _loai = string.Empty; // "Nhap" hoặc "Xuat"
        private string _tenNguyenLieu = string.Empty;
        private decimal _soLuong;
        private decimal _donGia;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        // "Nhap" hoặc "Xuat"
        public string Loai
        {
            get => _loai;
            set => _loai = value;
        }

        public int NguyenLieuId { get; set; }

        public string TenNguyenLieu
        {
            get => _tenNguyenLieu;
            set => _tenNguyenLieu = value;
        }

        public decimal SoLuong
        {
            get => _soLuong;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Số lượng phải lớn hơn 0!");
                _soLuong = value;
            }
        }

        // Đơn giá nhập (chỉ dùng khi nhập kho)
        public decimal DonGia
        {
            get => _donGia;
            set => _donGia = value;
        }

        public DateTime ThoiGian { get; set; } = DateTime.Now;
        public string? LyDo { get; set; }
    }
}
