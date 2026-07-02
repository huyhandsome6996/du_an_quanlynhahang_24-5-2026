// ============================================================
// TẦNG DAL - DatabaseHelper
// ------------------------------------------------------------
// Lớp tĩnh (static) chứa CHUỖI KẾT NỐI tới file Access (.accdb).
// Mọi lớp DAL đều lấy chuỗi này qua DatabaseHelper.ConnectionString.
//
// CÔNG NGHỆ SỬ DỤNG:
//   - Microsoft Access 2016 (.accdb)
//   - OLE DB Provider: Microsoft.ACE.OLEDB.12.0 (cài sẵn trên Windows có Office)
//
// LƯU Ý KHI THI:
//   Nếu thầy hỏi "Chuỗi kết nối CSDL nằm ở đâu?" → trả lời:
//   "Dạ ở file DatabaseHelper.cs, dòng ConnectionString bên dưới ạ."
// ============================================================
using System.Data.OleDb;

namespace QuanLyNhaHang.DAL
{
    /// <summary>
    /// Lớp tĩnh cung cấp chuỗi kết nối và hàm kiểm tra CSDL.
    /// </summary>
    public static class DatabaseHelper
    {
        // =======================================================================
        // CHUỖI KẾT NỐI tới file Access (.accdb).
        // - Provider: Microsoft.ACE.OLEDB.12.0  → driver OLE DB cho Access 2007+
        // - Data Source: QuanLyNhaHang.accdb    → file CSDL nằm cùng thư mục với .exe
        // =======================================================================
        public const string ConnectionString =
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=QuanLyNhaHang.accdb;";

        /// <summary>
        /// Kiểm tra file .accdb có tồn tại trong thư mục chạy ứng dụng không.
        /// Schema + dữ liệu mẫu đã có sẵn trong file (không tự tạo bảng bằng code).
        /// </summary>
        public static void KhoiTaoCSDL()
        {
            // Nếu file không tồn tại → ném exception để user biết copy file vào
            if (!File.Exists("QuanLyNhaHang.accdb"))
                throw new FileNotFoundException(
                    "Không tìm thấy file QuanLyNhaHang.accdb! " +
                    "Hãy copy file CSDL vào thư mục chạy ứng dụng.");
            // File có rồi → in ra console báo đã sẵn sàng
            Console.WriteLine("✅ CSDL Access đã sẵn sàng: QuanLyNhaHang.accdb");
        }
    }
}
