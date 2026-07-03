// ============================================================
// API_SANPHAM.CS - Các endpoint quản lý Sản phẩm (Thực đơn)
// ------------------------------------------------------------
// 5 endpoint:
//   GET    /api/sanpham          — Lấy tất cả sản phẩm (kể cả đã ngừng bán)
//   GET    /api/sanpham/dangban  — Chỉ lấy món đang bán
//   POST   /api/sanpham          — Thêm sản phẩm (ThucAn hoặc NuocUong)
//   PUT    /api/sanpham/{id}     — Sửa sản phẩm
//   DELETE /api/sanpham/{id}     — Xoá sản phẩm
//
// ĐA HÌNH OOP thể hiện ở POST và PUT:
//   Dựa vào cột "Loai" trong body để new ThucAn() hoặc new NuocUong().
//   Mặc dù gán vào biến SanPham (lớp cha), nhưng khi gọi TinhTien()
//   ở ApiHoaDon.cs thì override của lớp con sẽ được gọi.
// ============================================================
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;
// PhanQuyen nằm cùng namespace CacModun → không cần using thêm

namespace QuanLyNhaHang.CacModun
{
    /// <summary>
    /// Lớp tĩnh ApiSanPham — đăng ký các endpoint quản lý Sản phẩm.
    /// </summary>
    public static class ApiSanPham
    {
        /// <summary>Đăng ký tất cả API Sản phẩm vào WebApplication.</summary>
        public static void DangKy(this WebApplication app)
        {
            // 1) GET /api/sanpham — Lấy tất cả sản phẩm (cho trang Thực đơn)
            app.MapGet("/api/sanpham", (ISanPhamDAL spDAL) =>
            {
                try
                {
                    var ds = spDAL.LayTatCa();
                    // Anonymous object — chọn lọc các thuộc tính cần trả về JSON
                    // (ẩn các trường nội bộ, chỉ trả những gì UI cần)
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

            // 2) GET /api/sanpham/dangban — Chỉ lấy món đang bán (cho trang Gọi món)
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

            // 3) POST /api/sanpham — Thêm sản phẩm mới
            //    Body JSON: { TenSanPham, GiaCoBan, Loai, DangBan, HinhAnh }
            //    CHỈ QUẢN TRỊ VIÊN (Use Case: "Quản lý thực đơn").
            app.MapPost("/api/sanpham", (HttpContext ctx, JsonElement body, ISanPhamDAL spDAL) =>
            {
                // Kiểm tra quyền — trả 403 nếu là NhanVien
                var loi = PhanQuyen.YeuCauQuanTri(ctx);
                if (loi != null) return loi;

                try
                {
                    // Parse body JSON
                    string tenSanPham = body.GetProperty("TenSanPham").GetString() ?? "";
                    decimal giaCoBan = body.GetProperty("GiaCoBan").GetDecimal();
                    string loai = body.GetProperty("Loai").GetString() ?? "";
                    // DangBan optional — mặc định true
                    bool dangBan = body.TryGetProperty("DangBan", out var db) ? db.GetBoolean() : true;
                    // HinhAnh optional
                    string? hinhAnh = body.TryGetProperty("HinhAnh", out var img) ? img.GetString() : null;

                    // === ĐA HÌNH OOP ===
                    // Dựa vào Loai để chọn lớp con. Mặc dù khai báo biến kiểu SanPham
                    // (lớp cha), object thực tế là ThucAn hoặc NuocUong.
                    SanPham sp = loai == "ThucAn" ? new ThucAn() : new NuocUong();
                    sp.TenSanPham = tenSanPham;
                    sp.GiaCoBan = giaCoBan;
                    sp.DangBan = dangBan;
                    sp.HinhAnh = hinhAnh;
                    spDAL.Them(sp);    // DAL lưu xuống DB
                    return Results.Ok(new { thongBao = "Thêm sản phẩm thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // 4) PUT /api/sanpham/{id} — Sửa sản phẩm
            //    CHỈ QUẢN TRỊ VIÊN.
            app.MapPut("/api/sanpham/{id:int}", (int id, HttpContext ctx, JsonElement body, ISanPhamDAL spDAL) =>
            {
                var loi = PhanQuyen.YeuCauQuanTri(ctx);
                if (loi != null) return loi;

                try
                {
                    string tenSanPham = body.GetProperty("TenSanPham").GetString() ?? "";
                    decimal giaCoBan = body.GetProperty("GiaCoBan").GetDecimal();
                    string loai = body.GetProperty("Loai").GetString() ?? "";
                    bool dangBan = body.TryGetProperty("DangBan", out var db) ? db.GetBoolean() : true;
                    string? hinhAnh = body.TryGetProperty("HinhAnh", out var img) ? img.GetString() : null;

                    // Tạo object đa hình — cùng logic như POST
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

            // 5) DELETE /api/sanpham/{id} — Xoá sản phẩm
            //    CHỈ QUẢN TRỊ VIÊN.
            app.MapDelete("/api/sanpham/{id:int}", (int id, HttpContext ctx, ISanPhamDAL spDAL) =>
            {
                var loi = PhanQuyen.YeuCauQuanTri(ctx);
                if (loi != null) return loi;

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
