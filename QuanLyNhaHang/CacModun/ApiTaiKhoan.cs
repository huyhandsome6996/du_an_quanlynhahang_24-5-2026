// ============================================================
// API_TAIKHOAN.CS - Các endpoint Đăng ký / Đăng nhập
// ============================================================
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.CacModun
{
    public static class ApiTaiKhoan
    {
        // Đăng ký tất cả API tài khoản vào WebApplication
        public static void DangKy(this WebApplication app)
        {
            // GET /api/auth/check - Kiểm tra đã có người dùng nào chưa (để ẩn/hiện form đăng ký)
            app.MapGet("/api/auth/check", (INguoiDungDAL ndDAL) =>
            {
                try { return Results.Ok(new { coNguoiDung = ndDAL.KiemTraCoNguoiDung() }); }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // POST /api/auth/dangky - Đăng ký tài khoản QuảnTri đầu tiên
            app.MapPost("/api/auth/dangky", (JsonElement body, INguoiDungDAL ndDAL) =>
            {
                try
                {
                    string tenDangNhap = body.GetProperty("TenDangNhap").GetString() ?? "";
                    string matKhau = body.GetProperty("MatKhau").GetString() ?? "";

                    // Validate phía server
                    if (string.IsNullOrWhiteSpace(tenDangNhap) || tenDangNhap.Length < 3)
                        return Results.BadRequest(new { thongBao = "Tên đăng nhập phải có ít nhất 3 ký tự!" });
                    if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 4)
                        return Results.BadRequest(new { thongBao = "Mật khẩu phải có ít nhất 4 ký tự!" });

                    string matKhauHash = MatKhauBaoMat.BamSHA256(matKhau);
                    var nguoiDung = new NguoiDung
                    {
                        TenDangNhap = tenDangNhap.Trim(),
                        MatKhauHash = matKhauHash,
                        VaiTro = "QuanTri",
                        NgayTao = DateTime.Now
                    };
                    ndDAL.Them(nguoiDung);
                    return Results.Ok(new { thongBao = "Đăng ký thành công! Vui lòng đăng nhập." });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // POST /api/auth/dangnhap - Đăng nhập, so sánh hash mật khẩu
            app.MapPost("/api/auth/dangnhap", (JsonElement body, INguoiDungDAL ndDAL) =>
            {
                try
                {
                    string tenDangNhap = body.GetProperty("TenDangNhap").GetString() ?? "";
                    string matKhau = body.GetProperty("MatKhau").GetString() ?? "";

                    if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
                        return Results.BadRequest(new { thongBao = "Vui lòng nhập đầy đủ thông tin!" });

                    var nguoiDung = ndDAL.LayTheoTenDangNhap(tenDangNhap.Trim());
                    if (nguoiDung == null) return Results.Unauthorized();

                    string matKhauHash = MatKhauBaoMat.BamSHA256(matKhau);
                    if (nguoiDung.MatKhauHash != matKhauHash) return Results.Unauthorized();

                    return Results.Ok(new
                    {
                        thongBao = "Đăng nhập thành công!",
                        tenDangNhap = nguoiDung.TenDangNhap,
                        vaiTro = nguoiDung.VaiTro
                    });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });
        }
    }
}
