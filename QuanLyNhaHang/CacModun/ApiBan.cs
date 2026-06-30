// ============================================================
// API_BAN.CS - Các endpoint quản lý Bàn + Đặt bàn
// ============================================================
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.CacModun
{
    public static class ApiBan
    {
        public static void DangKy(this WebApplication app)
        {
            // GET /api/ban - Lấy tất cả bàn
            app.MapGet("/api/ban", (IBanDAL banDAL) =>
            {
                try { return Results.Ok(banDAL.LayTatCa()); }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // GET /api/ban/{id} - Lấy 1 bàn theo Id
            app.MapGet("/api/ban/{id:int}", (int id, IBanDAL banDAL) =>
            {
                var ban = banDAL.LayTheoId(id);
                if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
                return Results.Ok(ban);
            });

            // POST /api/ban - Thêm bàn mới
            app.MapPost("/api/ban", (Ban ban, IBanDAL banDAL) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(ban.TenBan))
                        return Results.BadRequest(new { thongBao = "Tên bàn không được để trống!" });
                    banDAL.Them(ban);
                    return Results.Ok(new { thongBao = "Thêm bàn thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // PUT /api/ban/{id} - Cập nhật thông tin bàn
            app.MapPut("/api/ban/{id:int}", (int id, Ban ban, IBanDAL banDAL) =>
            {
                try
                {
                    ban.Id = id;
                    banDAL.Sua(ban);
                    return Results.Ok(new { thongBao = "Cập nhật bàn thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // DELETE /api/ban/{id} - Xóa bàn (không cho xóa bàn đang có khách)
            app.MapDelete("/api/ban/{id:int}", (int id, IBanDAL banDAL) =>
            {
                try
                {
                    var ban = banDAL.LayTheoId(id);
                    if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
                    if (ban.TrangThai == "Có khách")
                        return Results.BadRequest(new { thongBao = "Không thể xóa bàn đang có khách!" });
                    banDAL.Xoa(id);
                    return Results.Ok(new { thongBao = "Xóa bàn thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // POST /api/ban/{id}/dat - Đặt bàn trước (Trống → Đã đặt)
            app.MapPost("/api/ban/{id:int}/dat", (int id, IBanDAL banDAL) =>
            {
                try
                {
                    var ban = banDAL.LayTheoId(id);
                    if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
                    if (ban.TrangThai != "Trống")
                        return Results.BadRequest(new { thongBao = "Chỉ có thể đặt bàn đang trống!" });
                    banDAL.CapNhatTrangThai(id, "Đã đặt");
                    return Results.Ok(new { thongBao = "Đặt bàn thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // POST /api/ban/{id}/huy-dat - Hủy đặt bàn (Đã đặt → Trống)
            app.MapPost("/api/ban/{id:int}/huy-dat", (int id, IBanDAL banDAL) =>
            {
                try
                {
                    var ban = banDAL.LayTheoId(id);
                    if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
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
