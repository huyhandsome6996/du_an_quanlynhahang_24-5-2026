// ============================================================
// API_HOADON.CS - Hóa đơn, Gọi món, Chi tiết hóa đơn, Thanh toán
// Đây là form quản lý quan hệ nhiều đối tượng:
//   Bàn 1—n HóaDon 1—n ChiTietHoaDon n—1 SanPham
// ============================================================
using System.Data.OleDb;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QuanLyNhaHang.DAL;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.CacModun
{
    public static class ApiHoaDon
    {
        public static void DangKy(this WebApplication app)
        {
            // POST /api/ban/{id}/mo - Mở bàn: tạo HóaDon mới, set Bàn → "Có khách"
            app.MapPost("/api/ban/{id:int}/mo", (int id, IBanDAL banDAL, IHoaDonDAL hdDAL) =>
            {
                try
                {
                    var ban = banDAL.LayTheoId(id);
                    if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
                    if (ban.TrangThai == "Có khách")
                        return Results.BadRequest(new { thongBao = "Bàn đang có khách, không thể mở lại!" });

                    var hoaDon = new HoaDon
                    {
                        BanId = id,
                        ThoiGianTao = DateTime.Now,
                        TrangThai = "Chưa thanh toán",
                        TongTien = 0,
                        VAT = 0,
                        GiamGia = 0,
                        PhuongThucThanhToan = "TienMat"
                    };
                    int hoaDonId = hdDAL.Them(hoaDon);
                    banDAL.CapNhatTrangThai(id, "Có khách");
                    return Results.Ok(new { thongBao = "Mở bàn thành công!", hoaDonId });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // GET /api/ban/{id}/hoadon - Lấy hóa đơn chưa thanh toán của bàn + chi tiết món
            app.MapGet("/api/ban/{id:int}/hoadon", (int id, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
            {
                try
                {
                    var hoaDon = hdDAL.LayHoaDonChuaThanhToanTheoBan(id);
                    if (hoaDon == null) return Results.NotFound(new { thongBao = "Bàn này hiện chưa có hóa đơn!" });
                    var chiTiet = ctDAL.LayTheoHoaDon(hoaDon.Id);
                    return Results.Ok(new { hoaDon, chiTiet });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // POST /api/hoadon/{id}/them-mon - Thêm món vào hóa đơn
            // Sử dụng TinhTien() / MoTaPhuPhi() của SanPham (đa hình ThucAn / NuocUong)
            app.MapPost("/api/hoadon/{id:int}/them-mon", (int id, JsonElement body, ISanPhamDAL spDAL, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
            {
                try
                {
                    int sanPhamId = body.GetProperty("SanPhamId").GetInt32();
                    int soLuong = body.GetProperty("SoLuong").GetInt32();
                    string thuocTinhThem = body.TryGetProperty("ThuocTinhThem", out var tt) ? tt.GetString() ?? "" : "";

                    if (soLuong <= 0)
                        return Results.BadRequest(new { thongBao = "Số lượng phải lớn hơn 0!" });

                    var hoaDon = hdDAL.LayTheoId(id);
                    if (hoaDon == null || hoaDon.TrangThai != "Chưa thanh toán")
                        return Results.NotFound(new { thongBao = "Hóa đơn không tồn tại hoặc đã thanh toán!" });

                    var sp = spDAL.LayTheoId(sanPhamId);
                    if (sp == null) return Results.NotFound(new { thongBao = "Sản phẩm không tồn tại!" });

                    decimal thanhTien = sp.TinhTien(soLuong, thuocTinhThem);
                    decimal donGiaBan = thanhTien / soLuong;

                    var chiTiet = new ChiTietHoaDon
                    {
                        HoaDonId = id,
                        SanPhamId = sanPhamId,
                        SoLuong = soLuong,
                        DonGiaBan = donGiaBan,
                        ThuocTinhThem = thuocTinhThem,
                        ThanhTien = thanhTien,
                        TrangThaiMon = "DangCho"
                    };
                    ctDAL.Them(chiTiet);

                    decimal tongTienMoi = hoaDon.TongTien + thanhTien;
                    hdDAL.CapNhatTongTien(id, tongTienMoi);

                    return Results.Ok(new
                    {
                        thongBao = "Thêm món thành công!",
                        thanhTien,
                        tongTienMoi,
                        moTaPhuPhi = sp.MoTaPhuPhi(thuocTinhThem)
                    });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // DELETE /api/chitiethoadon/{id} - Xóa 1 món khỏi hóa đơn (cộng lại tổng tiền)
            app.MapDelete("/api/chitiethoadon/{id:int}", (int id, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
            {
                try
                {
                    using var conn = new OleDbConnection(DatabaseHelper.ConnectionString);
                    conn.Open();
                    using var getCmd = new OleDbCommand(
                        "SELECT HoaDonId, ThanhTien FROM ChiTietHoaDon WHERE Id = @id", conn);
                    getCmd.Parameters.AddWithValue("@id", id);
                    using var reader = getCmd.ExecuteReader();
                    if (!reader.Read()) return Results.NotFound(new { thongBao = "Không tìm thấy món này!" });

                    int hoaDonId = reader.GetInt32(0);
                    decimal thanhTien = reader.GetDecimal(1);
                    reader.Close();

                    ctDAL.Xoa(id);
                    var hoaDon = hdDAL.LayTheoId(hoaDonId);
                    if (hoaDon != null)
                    {
                        decimal tongTienMoi = hoaDon.TongTien - thanhTien;
                        hdDAL.CapNhatTongTien(hoaDonId, Math.Max(0, tongTienMoi));
                    }
                    return Results.Ok(new { thongBao = "Xóa món thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // POST /api/ban/{id}/thanhtoan - Thanh toán: VAT, giảm giá, PTTT, giải phóng bàn
            app.MapPost("/api/ban/{id:int}/thanhtoan", (int id, JsonElement body, IBanDAL banDAL, IHoaDonDAL hdDAL) =>
            {
                try
                {
                    var hoaDon = hdDAL.LayHoaDonChuaThanhToanTheoBan(id);
                    if (hoaDon == null)
                        return Results.NotFound(new { thongBao = "Bàn này chưa có hóa đơn hoặc đã thanh toán!" });

                    decimal vat = body.TryGetProperty("VAT", out var v) ? v.GetDecimal() : 0;
                    decimal giamGia = body.TryGetProperty("GiamGia", out var g) ? g.GetDecimal() : 0;
                    string pttt = body.TryGetProperty("PhuongThucThanhToan", out var p) ? p.GetString() ?? "TienMat" : "TienMat";

                    hdDAL.CapNhatThanhToan(hoaDon.Id, vat, giamGia, pttt);

                    decimal tongCuoi = hoaDon.TongTien + vat - giamGia;
                    hdDAL.CapNhatTongTien(hoaDon.Id, Math.Max(0, tongCuoi));
                    hdDAL.ThanhToan(hoaDon.Id);
                    banDAL.CapNhatTrangThai(id, "Trống");

                    return Results.Ok(new
                    {
                        thongBao = "Thanh toán thành công!",
                        tongTien = Math.Max(0, tongCuoi),
                        vat,
                        giamGia,
                        phuongThuc = pttt
                    });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // GET /api/hoadon - Lấy tất cả hóa đơn (cho trang Lịch sử)
            app.MapGet("/api/hoadon", (IHoaDonDAL hdDAL) =>
            {
                try { return Results.Ok(hdDAL.LayTatCa()); }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // GET /api/hoadon/{id} - Lấy 1 hóa đơn + chi tiết
            app.MapGet("/api/hoadon/{id:int}", (int id, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
            {
                try
                {
                    var hoaDon = hdDAL.LayTheoId(id);
                    if (hoaDon == null) return Results.NotFound(new { thongBao = "Không tìm thấy hóa đơn!" });
                    var chiTiet = ctDAL.LayTheoHoaDon(id);
                    return Results.Ok(new { hoaDon, chiTiet });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // GET /api/hoadon/theongay - Lọc hóa đơn theo khoảng ngày
            app.MapGet("/api/hoadon/theongay", (DateTime tuNgay, DateTime denNgay, IHoaDonDAL hdDAL) =>
            {
                try
                {
                    var ds = hdDAL.LayTheoKhoangNgay(tuNgay, denNgay);
                    return Results.Ok(ds);
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });
        }
    }
}
