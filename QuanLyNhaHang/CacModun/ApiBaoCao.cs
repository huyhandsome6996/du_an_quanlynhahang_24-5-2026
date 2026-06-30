// ============================================================
// API_BAOCAO.CS - Báo cáo thống kê (món bán chạy, doanh thu)
// ============================================================
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QuanLyNhaHang.DAL.Interfaces;

namespace QuanLyNhaHang.CacModun
{
    // Lớp phụ trợ để trả về kết quả thống kê món bán chạy
    public class ThongKeMon
    {
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; } = "";
        public int TongSoLuong { get; set; }
        public decimal TongDoanhThu { get; set; }
    }

    public static class ApiBaoCao
    {
        public static void DangKy(this WebApplication app)
        {
            // GET /api/baocao/monbanchay - Top món bán chạy (mặc định top 10)
            app.MapGet("/api/baocao/monbanchay", (int? top, ISanPhamDAL spDAL, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
            {
                try
                {
                    int soLuongTop = top ?? 10;
                    var dsHoaDon = hdDAL.LayTatCa().Where(h => h.TrangThai == "Đã thanh toán");
                    var thongKe = new Dictionary<int, ThongKeMon>();

                    foreach (var hd in dsHoaDon)
                    {
                        var chiTiet = ctDAL.LayTheoHoaDon(hd.Id);
                        foreach (var ct in chiTiet)
                        {
                            if (!thongKe.ContainsKey(ct.SanPhamId))
                                thongKe[ct.SanPhamId] = new ThongKeMon
                                {
                                    SanPhamId = ct.SanPhamId,
                                    TenSanPham = ct.TenSanPham,
                                    TongSoLuong = 0,
                                    TongDoanhThu = 0
                                };
                            thongKe[ct.SanPhamId].TongSoLuong += ct.SoLuong;
                            thongKe[ct.SanPhamId].TongDoanhThu += ct.ThanhTien;
                        }
                    }

                    var ketQua = thongKe.Values
                        .OrderByDescending(t => t.TongSoLuong)
                        .Take(soLuongTop)
                        .ToList();
                    return Results.Ok(ketQua);
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // GET /api/baocao/doanhthu - Tổng quan doanh thu (tổng / hôm nay / tháng này)
            app.MapGet("/api/baocao/doanhthu", (IHoaDonDAL hdDAL) =>
            {
                try
                {
                    var ds = hdDAL.LayTatCa().Where(h => h.TrangThai == "Đã thanh toán").ToList();
                    var homNay = DateTime.Now.Date;
                    var dauThang = new DateTime(homNay.Year, homNay.Month, 1);

                    return Results.Ok(new
                    {
                        tongDoanhThu = ds.Sum(h => h.TongTien),
                        tongHoaDon = ds.Count,
                        doanhThuHomNay = ds
                            .Where(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value.Date == homNay)
                            .Sum(h => h.TongTien),
                        hoaDonHomNay = ds.Count(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value.Date == homNay),
                        doanhThuThangNay = ds
                            .Where(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value >= dauThang)
                            .Sum(h => h.TongTien),
                        hoaDonThangNay = ds.Count(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value >= dauThang)
                    });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });
        }
    }
}
