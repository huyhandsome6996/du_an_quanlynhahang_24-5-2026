// ============================================================
// API_SANPHAM.CS - Các endpoint quản lý Sản phẩm (Thực đơn)
// ============================================================
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.CacModun
{
    public static class ApiSanPham
    {
        public static void DangKy(this WebApplication app)
        {
            // GET /api/sanpham - Lấy tất cả sản phẩm
            app.MapGet("/api/sanpham", (ISanPhamDAL spDAL) =>
            {
                try
                {
                    var ds = spDAL.LayTatCa();
                    var ketQua = ds.Select(sp => new
                    {
                        sp.Id,
                        sp.TenSanPham,
                        sp.GiaCoBan,
                        sp.Loai,
                        sp.DangBan,
                        sp.HinhAnh
                    });
                    return Results.Ok(ketQua);
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // GET /api/sanpham/dangban - Chỉ lấy các món vẫn đang bán
            app.MapGet("/api/sanpham/dangban", (ISanPhamDAL spDAL) =>
            {
                var ds = spDAL.LayDangBan();
                var ketQua = ds.Select(sp => new
                {
                    sp.Id,
                    sp.TenSanPham,
                    sp.GiaCoBan,
                    sp.Loai,
                    sp.DangBan,
                    sp.HinhAnh
                });
                return Results.Ok(ketQua);
            });

            // POST /api/sanpham - Thêm sản phẩm (ThucAn hoặc NuocUong)
            app.MapPost("/api/sanpham", (JsonElement body, ISanPhamDAL spDAL) =>
            {
                try
                {
                    string tenSanPham = body.GetProperty("TenSanPham").GetString() ?? "";
                    decimal giaCoBan = body.GetProperty("GiaCoBan").GetDecimal();
                    string loai = body.GetProperty("Loai").GetString() ?? "";
                    bool dangBan = body.TryGetProperty("DangBan", out var db) ? db.GetBoolean() : true;
                    string? hinhAnh = body.TryGetProperty("HinhAnh", out var img) ? img.GetString() : null;

                    SanPham sp = loai == "ThucAn" ? new ThucAn() : new NuocUong();
                    sp.TenSanPham = tenSanPham;
                    sp.GiaCoBan = giaCoBan;
                    sp.DangBan = dangBan;
                    sp.HinhAnh = hinhAnh;
                    spDAL.Them(sp);
                    return Results.Ok(new { thongBao = "Thêm sản phẩm thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // PUT /api/sanpham/{id} - Sửa sản phẩm
            app.MapPut("/api/sanpham/{id:int}", (int id, JsonElement body, ISanPhamDAL spDAL) =>
            {
                try
                {
                    string tenSanPham = body.GetProperty("TenSanPham").GetString() ?? "";
                    decimal giaCoBan = body.GetProperty("GiaCoBan").GetDecimal();
                    string loai = body.GetProperty("Loai").GetString() ?? "";
                    bool dangBan = body.TryGetProperty("DangBan", out var db) ? db.GetBoolean() : true;
                    string? hinhAnh = body.TryGetProperty("HinhAnh", out var img) ? img.GetString() : null;

                    SanPham sp = loai == "ThucAn" ? new ThucAn() : new NuocUong();
                    sp.Id = id;
                    sp.TenSanPham = tenSanPham;
                    sp.GiaCoBan = giaCoBan;
                    sp.DangBan = dangBan;
                    sp.HinhAnh = hinhAnh;
                    spDAL.Sua(sp);
                    return Results.Ok(new { thongBao = "Cập nhật sản phẩm thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // DELETE /api/sanpham/{id} - Xóa sản phẩm
            app.MapDelete("/api/sanpham/{id:int}", (int id, ISanPhamDAL spDAL) =>
            {
                try
                {
                    var sp = spDAL.LayTheoId(id);
                    if (sp == null) return Results.NotFound(new { thongBao = "Không tìm thấy sản phẩm!" });
                    spDAL.Xoa(id);
                    return Results.Ok(new { thongBao = "Xóa sản phẩm thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });
        }
    }
}
