// ============================================================
// KHOITAOUNGDUNG.CS - CẤU HÌNH BAN ĐẦU CHO ỨNG DỤNG
// ------------------------------------------------------------
// Lớp tĩnh KhoiTaoUngDung chịu trách nhiệm:
//   1. Tạo WebApplication (server ASP.NET Core)
//   2. Cấu hình CORS (cho phép HTML/JS gọi API)
//   3. Cấu hình JSON serializer (giữ nguyên tên thuộc tính C#)
//   4. Đăng ký DI cho 5 DAL (mỗi Interface → Implementation cụ thể)
//   5. Kiểm tra file CSDL Access tồn tại
//   6. Bật Static Files (để phục vụ HTML/CSS/JS từ wwwroot)
// ============================================================
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using QuanLyNhaHang.DAL;
using QuanLyNhaHang.DAL.Interfaces;

namespace QuanLyNhaHang.CacModun
{
    /// <summary>
    /// Lớp tĩnh chứa hàm KhoiTao() — thiết lập toàn bộ ứng dụng trước khi chạy.
    /// </summary>
    public static class KhoiTaoUngDung
    {
        /// <summary>
        /// Khởi tạo WebApplication: cấu hình DI, CORS, JSON, kiểm tra CSDL.
        /// Trả về app đã sẵn sàng để đăng ký API và chạy.
        /// </summary>
        /// <param name="args">Tham số dòng lệnh (từ Program.cs)</param>
        public static WebApplication KhoiTao(string[] args)
        {
            // 1) Tạo builder từ tham số dòng lệnh (dotnet run --foo bar)
            var builder = WebApplication.CreateBuilder(args);

            // 2) Cấu hình CORS — cho phép frontend (HTML/JS) gọi API từ bất kỳ origin nào
            //    (WebView2 chạy same-origin nên không bắt buộc, nhưng để chắc chắn)
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin()    // Cho phép mọi domain
                          .AllowAnyHeader()    // Cho phép mọi Content-Type
                          .AllowAnyMethod());  // Cho phép GET/POST/PUT/DELETE
            });

            // 3) Cấu hình JSON serializer:
            //    - PropertyNamingPolicy = null → GIỮ NGUYÊN tên thuộc tính C# (vd: TenSanPham)
            //      (mặc định System.Text.Json sẽ camelCase → tenSanPham, JS sẽ không nhận được)
            //    - UnsafeRelaxedJsonEscaping → cho phép ký tự Unicode (Tiếng Việt) không bị escape
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = null;
                options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            });

            // 4) === ĐĂNG KÝ CÁC DAL VÀO DI CONTAINER ===
            //    Mỗi Interface → 1 Implementation cụ thể.
            //    Khi API cần INguoiDungDAL, DI tự inject NguoiDungDAL.
            //    → Đây chính là DEPENDENCY INJECTION (kỹ thuật OOP): tách rời
            //      Interface và Implementation, dễ thay thế khi cần.
            //    AddSingleton: chỉ tạo 1 instance duy nhất cho toàn app
            //    (DAL không chứa state, chỉ chứa chuỗi kết nối → an toàn).
            builder.Services.AddSingleton<IBanDAL, BanDAL>();
            builder.Services.AddSingleton<ISanPhamDAL, SanPhamDAL>();
            builder.Services.AddSingleton<IHoaDonDAL, HoaDonDAL>();
            builder.Services.AddSingleton<IChiTietHoaDonDAL, ChiTietHoaDonDAL>();
            builder.Services.AddSingleton<INguoiDungDAL, NguoiDungDAL>();

            // 5) Build app từ builder
            var app = builder.Build();

            // 6) Kiểm tra file CSDL Access tồn tại (ném exception nếu thiếu)
            DatabaseHelper.KhoiTaoCSDL();

            // 7) Bật CORS middleware
            app.UseCors();
            // UseDefaultFiles: khi gọi / → tự trả index.html
            app.UseDefaultFiles();
            // UseStaticFiles: phục vụ file tĩnh trong wwwroot (css, js, img, html)
            app.UseStaticFiles();

            return app;
        }
    }
}
