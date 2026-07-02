// ============================================================
// API_TAIKHOAN.CS - Các endpoint Đăng ký / Đăng nhập
// ------------------------------------------------------------
// 3 endpoint:
//   GET  /api/auth/check      — Kiểm tra đã có user chưa (ẩn/hiện form đăng ký)
//   POST /api/auth/dangky     — Đăng ký tài khoản QuanTri đầu tiên
//   POST /api/auth/dangnhap   — Đăng nhập, so sánh mật khẩu PLAIN-TEXT
//
// PHIÊN BẢN NÀY KHÔNG DÙNG SHA-256:
//   Mật khẩu lưu plain-text trong Access. Học sinh có thể tự thêm/sửa
//   tài khoản trực tiếp trong Access mà không cần tính hash.
//   Đồ án nhỏ → tối giản code cho dễ học, dễ thi vấn đáp.
// ============================================================
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.CacModun
{
    /// <summary>
    /// Lớp tĩnh ApiTaiKhoan — đăng ký các endpoint tài khoản.
    /// </summary>
    public static class ApiTaiKhoan
    {
        /// <summary>
        /// Đăng ký tất cả API tài khoản vào WebApplication.
        /// Dùng extension method pattern để viết: app.DangKy() thay vì ApiTaiKhoan.DangKy(app).
        /// </summary>
        public static void DangKy(this WebApplication app)
        {
            // =====================================================
            // 1. GET /api/auth/check
            // Kiểm tra trong bảng NguoiDung đã có dòng nào chưa.
            // Frontend dùng kết quả để quyết định:
            //   - coNguoiDung=false → hiện form Đăng ký (lần đầu chạy app)
            //   - coNguoiDung=true  → hiện form Đăng nhập
            // =====================================================
            app.MapGet("/api/auth/check", (INguoiDungDAL ndDAL) =>
            {
                try
                {
                    // ndDAL được DI tự inject. KiemTraCoNguoiDung trả bool.
                    return Results.Ok(new { coNguoiDung = ndDAL.KiemTraCoNguoiDung() });
                }
                catch (Exception ex)
                {
                    // Trả về 400 Bad Request + thông báo lỗi
                    return Results.BadRequest(new { thongBao = ex.Message });
                }
            });

            // =====================================================
            // 2. POST /api/auth/dangky
            // Đăng ký tài khoản QuanTri đầu tiên.
            // Body JSON: { "TenDangNhap": "admin", "MatKhau": "admin123" }
            // =====================================================
            app.MapPost("/api/auth/dangky", (JsonElement body, INguoiDungDAL ndDAL) =>
            {
                try
                {
                    // Parse JSON body → lấy 2 trường
                    string tenDangNhap = body.GetProperty("TenDangNhap").GetString() ?? "";
                    string matKhau = body.GetProperty("MatKhau").GetString() ?? "";

                    // Validate phía server (luôn validate cả client lẫn server để an toàn)
                    if (string.IsNullOrWhiteSpace(tenDangNhap) || tenDangNhap.Length < 3)
                        return Results.BadRequest(new { thongBao = "Tên đăng nhập phải có ít nhất 3 ký tự!" });
                    if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 4)
                        return Results.BadRequest(new { thongBao = "Mật khẩu phải có ít nhất 4 ký tự!" });

                    // Tạo object NguoiDung — lưu mật khẩu PLAIN-TEXT (không băm)
                    var nguoiDung = new NguoiDung
                    {
                        TenDangNhap = tenDangNhap.Trim(),
                        MatKhau = matKhau,                  // Lưu plain-text trực tiếp
                        VaiTro = "QuanTri",                 // Tài khoản đầu tiên luôn là QuanTri
                        NgayTao = DateTime.Now
                    };
                    ndDAL.Them(nguoiDung);                  // Gọi DAL để INSERT vào DB
                    return Results.Ok(new { thongBao = "Đăng ký thành công! Vui lòng đăng nhập." });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { thongBao = ex.Message });
                }
            });

            // =====================================================
            // 3. POST /api/auth/dangnhap
            // Đăng nhập, so sánh mật khẩu PLAIN-TEXT.
            // Body JSON: { "TenDangNhap": "admin", "MatKhau": "admin123" }
            // =====================================================
            app.MapPost("/api/auth/dangnhap", (JsonElement body, INguoiDungDAL ndDAL) =>
            {
                try
                {
                    // Parse JSON body
                    string tenDangNhap = body.GetProperty("TenDangNhap").GetString() ?? "";
                    string matKhau = body.GetProperty("MatKhau").GetString() ?? "";

                    // Validate không được để trống
                    if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
                        return Results.BadRequest(new { thongBao = "Vui lòng nhập đầy đủ thông tin!" });

                    // Tìm user trong DB
                    var nguoiDung = ndDAL.LayTheoTenDangNhap(tenDangNhap.Trim());
                    // Không tìm thấy user → 401 Unauthorized
                    if (nguoiDung == null) return Results.Unauthorized();

                    // SO SÁNH MẬT KHẨU PLAIN-TEXT trực tiếp
                    // (Phiên bản cũ dùng SHA-256 → phức tạp và thừa thãi cho đồ án nhỏ)
                    if (nguoiDung.MatKhau != matKhau) return Results.Unauthorized();

                    // Đúng user + đúng mật khẩu → trả về 200 + thông tin user
                    return Results.Ok(new
                    {
                        thongBao = "Đăng nhập thành công!",
                        tenDangNhap = nguoiDung.TenDangNhap,
                        vaiTro = nguoiDung.VaiTro
                    });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { thongBao = ex.Message });
                }
            });
        }
    }
}
