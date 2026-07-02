// ============================================================
// KHOIDONGDESKTOP.CS - KHỞI CHẠY ỨNG DỤNG DESKTOP (WebView2)
// ------------------------------------------------------------
// Mô hình hoạt động:
//   1. Chạy ASP.NET server ngầm ở http://localhost:5000
//   2. Mở cửa sổ WinForms chứa WebView2 (giống Chrome) trỏ vào server
//   → Người dùng thấy cửa sổ desktop như app bình thường,
//     nhưng thực chất UI là HTML/CSS/JS, backend là ASP.NET API.
//
// Lý do dùng mô hình này:
//   - Lợi dụng skill Web (HTML/CSS/JS) để làm giao diện đẹp
//   - Backend vẫn dùng C#/.NET mạnh về OOP + truy cập CSDL
//   - Không cần cài đặt IIS hay server riêng — app tự chứa
// ============================================================
using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.Web.WebView2.WinForms;

namespace QuanLyNhaHang.CacModun
{
    /// <summary>
    /// Lớp tĩnh KhoiDongDesktop — khởi chạy server + mở cửa sổ desktop.
    /// </summary>
    public static class KhoiDongDesktop
    {
        /// <summary>
        /// Khởi chạy server ngầm + mở cửa sổ WinForms chứa WebView2.
        /// Hàm này BLOCKING — không trả về cho tới khi user đóng cửa sổ.
        /// </summary>
        public static void Chay(WebApplication app)
        {
            // In thông báo ra console (chỉ thấy khi chạy dotnet run)
            Console.WriteLine("🍽️  ỨNG DỤNG QUẢN LÝ NHÀ HÀNG (DESKTOP MODE)");
            Console.WriteLine("=================================");

            // 1) Chạy web server ngầm trên port 5000 (chạy nền bằng Task.Run)
            _ = Task.Run(() => app.Run("http://localhost:5000"));

            // 2) Mở cửa sổ desktop WinForms + WebView2 trên 1 thread UI riêng
            //    WinForms yêu cầu UI chạy trên thread STA (Single-Threaded Apartment)
            var uiThread = new Thread(() =>
            {
                // Các thiết lập bắt buộc cho WinForms trên .NET 10
                Application.SetHighDpiMode(HighDpiMode.SystemAware);   // Hỗ trợ DPI cao (4K)
                Application.EnableVisualStyles();                       // Dùng style hiện đại của Windows
                Application.SetCompatibleTextRenderingDefault(false);   // Dùng GDI+ (mặc định)

                // Tạo cửa sổ chính
                var formMain = new Form
                {
                    Text = "🍽️ Hệ Thống Quản Lý Nhà Hàng (Desktop App)",
                    Width = 1350,                                              // Rộng 1350px
                    Height = 850,                                              // Cao 850px
                    StartPosition = FormStartPosition.CenterScreen            // Mở giữa màn hình
                };

                // Đặt icon cho cửa sổ (logo riêng, không phải icon WinForm mặc định)
                try
                {
                    // Đường dẫn tới file logo.png trong wwwroot
                    string iconPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo.png");
                    if (File.Exists(iconPath))
                    {
                        // Chuyển PNG → Bitmap → Icon (WinForms chỉ nhận Icon)
                        using var bitmap = new Bitmap(iconPath);
                        formMain.Icon = Icon.FromHandle(bitmap.GetHicon());
                    }
                }
                catch { /* Bỏ qua nếu không tìm thấy icon — không crash app */ }

                // Tạo WebView2 — control hiển thị trang web (giống Chrome)
                var webView = new WebView2
                {
                    Dock = DockStyle.Fill    // Lấp đầy toàn bộ form
                };
                formMain.Controls.Add(webView);

                // Sự kiện Load: khi form đã hiện ra → tải trang web vào WebView2
                formMain.Load += async (s, e) =>
                {
                    try
                    {
                        // Đợi 1200ms để server ngầm kịp khởi động
                        await Task.Delay(1200);
                        // Khởi tạo WebView2 runtime (cần cài WebView2 Runtime trên Windows)
                        await webView.EnsureCoreWebView2Async();
                        // Trỏ WebView2 tới server localhost:5000
                        webView.Source = new Uri("http://localhost:5000");
                    }
                    catch (Exception ex)
                    {
                        // Nếu WebView2 lỗi (thiếu runtime) → báo người dùng
                        MessageBox.Show(
                            $"Không thể tải giao diện WebView2: {ex.Message}",
                            "Lỗi Giao Diện",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                };

                // Bắt đầu message loop của WinForms — BLOCKING
                Application.Run(formMain);
            });

            // Đặt thread là STA (bắt buộc cho WinForms)
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();     // Khởi chạy thread UI
            uiThread.Join();      // Đợi thread UI kết thúc (khi user đóng cửa sổ)
        }
    }
}
