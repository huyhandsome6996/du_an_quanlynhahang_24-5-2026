// ============================================================
// TẦNG ENTITY - Lớp NguyenLieu (Thực thể Nguyên Liệu Kho)
// ============================================================
namespace QuanLyNhaHang.Entities
{
    public class NguyenLieu
    {
        private int _id;
        private string _tenNguyenLieu = string.Empty;
        private string _donVi = string.Empty;
        private decimal _soLuongTon;
        private decimal _mucToiThieu;

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string TenNguyenLieu
        {
            get => _tenNguyenLieu;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Tên nguyên liệu không được để trống!");
                _tenNguyenLieu = value.Trim();
            }
        }

        public string DonVi
        {
            get => _donVi;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Đơn vị không được để trống!");
                _donVi = value.Trim();
            }
        }

        public decimal SoLuongTon
        {
            get => _soLuongTon;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Số lượng tồn không được âm!");
                _soLuongTon = value;
            }
        }

        public decimal MucToiThieu
        {
            get => _mucToiThieu;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Mức tối thiểu không được âm!");
                _mucToiThieu = value;
            }
        }

        public string? GhiChu { get; set; }
    }
}
