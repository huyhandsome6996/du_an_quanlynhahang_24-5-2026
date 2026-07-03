// ============================================================
// API_BAN.CS - Các endpoint quản lý Bàn + Đặt bàn
// ------------------------------------------------------------
// 7 endpoint:
//   GET    /api/ban              — Lấy tất cả bàn
//   GET    /api/ban/{id}         — Lấy 1 bàn theo Id
//   POST   /api/ban              — Thêm bàn mới
//   PUT    /api/ban/{id}         — Cập nhật thông tin bàn
//   DELETE /api/ban/{id}         — Xoá bàn (không cho xoá bàn có khách)
//   POST   /api/ban/{id}/dat     — Đặt bàn trước (Trống → Đã đặt)
//   POST   /api/ban/{id}/huy-dat — Huỷ đặt bàn (Đã đặt → Trống)
//
// Tất cả endpoint đều inject IBanDAL qua DI.
// ============================================================
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.CacModun
{
    /// <summary>
    /// Lớp tĩnh ApiBan — đăng ký các endpoint quản lý Bàn.
    /// </summary>
    public static class ApiBan
    {
        /// <summary>Đăng ký tất cả API Bàn vào WebApplication.</summary>
        public static void DangKy(this WebApplication app)
        {
            // 1) GET /api/ban — Lấy tất cả bàn (cho trang Sơ đồ bàn)
            app.MapGet("/api/ban", (IBanDAL banDAL) =>
            {
                try { return Results.Ok(banDAL.LayTatCa()); }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // 2) GET /api/ban/{id} — Lấy 1 bàn theo Id
            //    {id:int} là route constraint — chỉ match nếu id là số nguyên
            app.MapGet("/api/ban/{id:int}", (int id, IBanDAL banDAL) =>
            {
                var ban = banDAL.LayTheoId(id);
                if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
                return Results.Ok(ban);
            });

            // 3) POST /api/ban — Thêm bàn mới
            //    CHỈ QUẢN TRỊ VIÊN (Use Case: "Đổi trạng thái bàn").
            //    ASP.NET tự deserialize body JSON → object Ban
            app.MapPost("/api/ban", (HttpContext ctx, Ban ban, IBanDAL banDAL) =>
            {
                var loi = PhanQuyen.YeuCauQuanTri(ctx);
                if (loi != null) return loi;

                try
                {
                    // Validate phía server
                    if (string.IsNullOrWhiteSpace(ban.TenBan))
                        return Results.BadRequest(new { thongBao = "Tên bàn không được để trống!" });
                    banDAL.Them(ban);
                    return Results.Ok(new { thongBao = "Thêm bàn thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // 4) PUT /api/ban/{id} — Cập nhật thông tin bàn
            //    CHỈ QUẢN TRỊ VIÊN.
            app.MapPut("/api/ban/{id:int}", (int id, HttpContext ctx, Ban ban, IBanDAL banDAL) =>
            {
                var loi = PhanQuyen.YeuCauQuanTri(ctx);
                if (loi != null) return loi;

                try
                {
                    ban.Id = id;        // Đảm bảo Id đúng theo URL
                    banDAL.Sua(ban);
                    return Results.Ok(new { thongBao = "Cập nhật bàn thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // 5) DELETE /api/ban/{id} — Xoá bàn
            //    KHÔNG cho xoá bàn đang có khách (để không mất hóa đơn đang mở)
            //    CHỈ QUẢN TRỊ VIÊN.
            app.MapDelete("/api/ban/{id:int}", (int id, HttpContext ctx, IBanDAL banDAL) =>
            {
                var loi = PhanQuyen.YeuCauQuanTri(ctx);
                if (loi != null) return loi;

                try
                {
                    var ban = banDAL.LayTheoId(id);
                    if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
                    // Kiểm tra trạng thái — nếu đang có khách thì chặn
                    if (ban.TrangThai == "Có khách")
                        return Results.BadRequest(new { thongBao = "Không thể xóa bàn đang có khách!" });
                    banDAL.Xoa(id);
                    return Results.Ok(new { thongBao = "Xóa bàn thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // 6) POST /api/ban/{id}/dat — Đặt bàn trước (Trống → Đã đặt)
            app.MapPost("/api/ban/{id:int}/dat", (int id, IBanDAL banDAL) =>
            {
                try
                {
                    var ban = banDAL.LayTheoId(id);
                    if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
                    // Chỉ cho đặt bàn đang Trống (bàn đã đặt/Có khách thì không cho đặt lại)
                    if (ban.TrangThai != "Trống")
                        return Results.BadRequest(new { thongBao = "Chỉ có thể đặt bàn đang trống!" });
                    banDAL.CapNhatTrangThai(id, "Đã đặt");
                    return Results.Ok(new { thongBao = "Đặt bàn thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // 7) POST /api/ban/{id}/huy-dat — Huỷ đặt bàn (Đã đặt → Trống)
            app.MapPost("/api/ban/{id:int}/huy-dat", (int id, IBanDAL banDAL) =>
            {
                try
                {
                    var ban = banDAL.LayTheoId(id);
                    if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
                    // Chỉ cho huỷ nếu bàn đang "Đã đặt"
                    if (ban.TrangThai != "Đã đặt")
                        return Results.BadRequest(new { thongBao = "Bàn này chưa được đặt!" });
                    banDAL.CapNhatTrangThai(id, "Trống");
                    return Results.Ok(new { thongBao = "Hủy đặt bàn thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });
        }
    }
}
