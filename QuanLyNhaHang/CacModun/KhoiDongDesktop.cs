// ============================================================
// KHOIDONGDESKTOP.CS - Khởi chạy ứng dụng Desktop (WebView2)
// - Chạy ASP.NET server ngầm ở http://localhost:5000
// - Mở cửa sổ WinForms chứa WebView2 trỏ vào server đó
// => dotnet run sẽ mở cửa sổ desktop, KHÔNG mở trình duyệt web
// ============================================================
using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.Builder;
using Microsoft.Web.WebView2.WinForms;

namespace QuanLyNhaHang.CacModun
{
    public static class KhoiDongDesktop
    {
        public static void Chay(WebApplication app)
        {
            Console.WriteLine("🍽️  ỨNG DỤNG QUẢN LÝ NHÀ HÀNG (DESKTOP MODE)");
            Console.WriteLine("=================================");

            // 1) Chạy web server ngầm trên port 5000
            _ = Task.Run(() => app.Run("http://localhost:5000"));

            // 2) Mở cửa sổ desktop WinForms + WebView2 hiển thị giao diện
            var uiThread = new Thread(() =>
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var formMain = new Form
                {
                    Text = "🍽️ Hệ Thống Quản Lý Nhà Hàng (Desktop App)",
                    Width = 1350,
                    Height = 850,
                    StartPosition = FormStartPosition.CenterScreen
                };

                // Đặt icon cho cửa sổ (logo riêng, không phải icon WinForm mặc định)
                try
                {
                    string iconPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo.png");
                    if (File.Exists(iconPath))
                    {
                        using var bitmap = new Bitmap(iconPath);
                        formMain.Icon = Icon.FromHandle(bitmap.GetHicon());
                    }
                }
                catch { /* Bỏ qua nếu không tìm thấy icon */ }

                var webView = new WebView2
                {
                    Dock = DockStyle.Fill
                };
                formMain.Controls.Add(webView);

                formMain.Load += async (s, e) =>
                {
                    try
                    {
                        await Task.Delay(1200); // Đợi server ngầm khởi động xong
                        await webView.EnsureCoreWebView2Async();
                        webView.Source = new Uri("http://localhost:5000");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Không thể tải giao diện WebView2: {ex.Message}",
                            "Lỗi Giao Diện",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                };

                Application.Run(formMain);
            });

            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();
            uiThread.Join();
        }
    }
}
