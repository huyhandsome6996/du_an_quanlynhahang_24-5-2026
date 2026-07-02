// ============================================================
// TẦNG DAL - DatabaseHelper
// Kết nối Microsoft Access (.accdb) qua OLE DB Provider.
// File CSDL QuanLyNhaHang.accdb đã có sẵn schema + dữ liệu mẫu.
// ============================================================
using System.Data.OleDb;

namespace QuanLyNhaHang.DAL
{
    public static class DatabaseHelper
    {
        // =======================================================================
        // 🎯 CHÚ Ý KHI THI: NẾU THẦY HỎI "CHUỖI KẾT NỐI CSDL NẰM Ở ĐÂU?"
        // -> TRẢ LỜI: DẠ Ở FILE "DatabaseHelper.cs", DÒNG CHUỖI KẾT NỐI DƯỚI ĐÂY Ạ!
        // =======================================================================
        // Provider Microsoft.ACE.OLEDB.12.0 = Access 2007+ (2016/365 đều dùng được)
        public const string ConnectionString =
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=QuanLyNhaHang.accdb;";

        // Kiểm tra file .accdb tồn tại. Schema + data đã có sẵn trong file.
        public static void KhoiTaoCSDL()
        {
            if (!File.Exists("QuanLyNhaHang.accdb"))
                throw new FileNotFoundException(
                    "Không tìm thấy file QuanLyNhaHang.accdb! " +
                    "Hãy copy file CSDL vào thư mục chạy ứng dụng.");
            Console.WriteLine("✅ CSDL Access đã sẵn sàng: QuanLyNhaHang.accdb");
        }
    }
}
