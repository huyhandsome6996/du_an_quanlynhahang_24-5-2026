// ============================================================
// KHOITAOUNG DUNG.CS - Cấu hình ban đầu cho ứng dụng
// Đăng ký DI cho các DAL, CORS, JSON serializer, tạo CSDL
// ============================================================
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using QuanLyNhaHang.DAL;
using QuanLyNhaHang.DAL.Interfaces;

namespace QuanLyNhaHang.CacModun
{
    public static class KhoiTaoUngDung
    {
        // Khởi tạo WebApplication: cấu hình DI, CORS, JSON, tạo CSDL
        public static WebApplication KhoiTao(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Cho phép frontend (HTML/JS) gọi API từ same-origin
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            // Giữ nguyên tên thuộc tính C# khi serialize sang JSON (không camelCase)
            builder.Services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = null;
                options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            });

            // === Đăng ký các DAL vào DI (mỗi DAL đều implement Interface tương ứng) ===
            builder.Services.AddSingleton<IBanDAL, BanDAL>();
            builder.Services.AddSingleton<ISanPhamDAL, SanPhamDAL>();
            builder.Services.AddSingleton<IHoaDonDAL, HoaDonDAL>();
            builder.Services.AddSingleton<IChiTietHoaDonDAL, ChiTietHoaDonDAL>();
            builder.Services.AddSingleton<INguoiDungDAL, NguoiDungDAL>();

            var app = builder.Build();

            // Kiểm tra file CSDL Access (.accdb) tồn tại
            DatabaseHelper.KhoiTaoCSDL();

            app.UseCors();
            app.UseDefaultFiles();   // Mặc định trả về index.html khi gọi /
            app.UseStaticFiles();    // Phục vụ file tĩnh trong wwwroot

            return app;
        }
    }
}
