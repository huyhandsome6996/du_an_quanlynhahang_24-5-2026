// ============================================================
// API_BAOCAO.CS - Báo cáo thống kê (món bán chạy, doanh thu)
// ------------------------------------------------------------
// 2 endpoint:
//   GET /api/baocao/monbanchay  — Top N món bán chạy (mặc định top 10)
//   GET /api/baocao/doanhthu    — Tổng quan doanh thu (tổng / hôm nay / tháng này)
//
// Lớp phụ trợ ThongKeMon: DTO (Data Transfer Object) để trả về
// kết quả thống kê dưới dạng JSON cho frontend.
// ============================================================
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QuanLyNhaHang.DAL.Interfaces;

namespace QuanLyNhaHang.CacModun
{
    /// <summary>
    /// Lớp DTO (Data Transfer Object) — chỉ chứa dữ liệu để trả về JSON.
    /// Dùng cho endpoint /api/baocao/monbanchay.
    /// </summary>
    public class ThongKeMon
    {
        public int SanPhamId { get; set; }              // Id sản phẩm
        public string TenSanPham { get; set; } = "";    // Tên sản phẩm
        public int TongSoLuong { get; set; }            // Tổng số lượng bán ra
        public decimal TongDoanhThu { get; set; }       // Tổng doanh thu
    }

    /// <summary>
    /// Lớp tĩnh ApiBaoCao — đăng ký các endpoint báo cáo.
    /// </summary>
    public static class ApiBaoCao
    {
        /// <summary>Đăng ký tất cả API Báo cáo vào WebApplication.</summary>
        public static void DangKy(this WebApplication app)
        {
            // =====================================================
            // 1. GET /api/baocao/monbanchay?top=10
            // Trả về Top N món bán chạy nhất (sắp xếp giảm dần theo số lượng).
            // Logic:
            //   - Lấy tất cả HĐ đã thanh toán
            //   - Duyệt qua từng HĐ, lấy chi tiết món
            //   - Cộng dồn số lượng + doanh thu theo SanPhamId vào Dictionary
            //   - Sắp xếp giảm dần theo TongSoLuong, lấy Top N
            // =====================================================
            app.MapGet("/api/baocao/monbanchay",
                (HttpContext ctx, int? top, ISanPhamDAL spDAL, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
            {
                // CHỈ QUẢN TRỊ VIÊN (Use Case: "Xem báo cáo doanh thu")
                var loi = PhanQuyen.YeuCauQuanTri(ctx);
                if (loi != null) return loi;

                try
                {
                    // Nếu query không có top → mặc định 10
                    int soLuongTop = top ?? 10;

                    // Lấy danh sách HĐ đã thanh toán (Where + Linear search)
                    var dsHoaDon = hdDAL.LayTatCa().Where(h => h.TrangThai == "Đã thanh toán");

                    // Dictionary để cộng dồn: key = SanPhamId, value = ThongKeMon
                    var thongKe = new Dictionary<int, ThongKeMon>();

                    // Duyệt qua từng HĐ
                    foreach (var hd in dsHoaDon)
                    {
                        // Lấy chi tiết món của HĐ này
                        var chiTiet = ctDAL.LayTheoHoaDon(hd.Id);
                        // Duyệt qua từng món
                        foreach (var ct in chiTiet)
                        {
                            // Nếu sản phẩm chưa có trong dict → thêm mới
                            if (!thongKe.ContainsKey(ct.SanPhamId))
                                thongKe[ct.SanPhamId] = new ThongKeMon
                                {
                                    SanPhamId = ct.SanPhamId,
                                    TenSanPham = ct.TenSanPham,
                                    TongSoLuong = 0,
                                    TongDoanhThu = 0
                                };
                            // Cộng dồn số lượng và doanh thu
                            thongKe[ct.SanPhamId].TongSoLuong += ct.SoLuong;
                            thongKe[ct.SanPhamId].TongDoanhThu += ct.ThanhTien;
                        }
                    }

                    // Sắp xếp giảm dần theo TongSoLuong, lấy Top N
                    var ketQua = thongKe.Values
                        .OrderByDescending(t => t.TongSoLuong)   // Sắp giảm dần
                        .Take(soLuongTop)                         // Lấy N phần tử đầu
                        .ToList();
                    return Results.Ok(ketQua);
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // =====================================================
            // 2. GET /api/baocao/doanhthu
            // Trả về tổng quan doanh thu (4 chỉ số):
            //   - tongDoanhThu     : tổng tất cả các thời điểm
            //   - tongHoaDon       : tổng số HĐ đã TT
            //   - doanhThuHomNay   : doanh thu trong ngày hôm nay
            //   - doanhThuThangNay : doanh thu từ đầu tháng tới nay
            // =====================================================
            app.MapGet("/api/baocao/doanhthu", (HttpContext ctx, IHoaDonDAL hdDAL) =>
            {
                // CHỈ QUẢN TRỊ VIÊN
                var loi = PhanQuyen.YeuCauQuanTri(ctx);
                if (loi != null) return loi;

                try
                {
                    // Lấy tất cả HĐ đã TT (ToList để query nhiều lần)
                    var ds = hdDAL.LayTatCa().Where(h => h.TrangThai == "Đã thanh toán").ToList();
                    // Lấy ngày hôm nay (chỉ phần Date, bỏ phần giờ)
                    var homNay = DateTime.Now.Date;
                    // Ngày đầu tháng (ví dụ: 01/07/2026 00:00:00)
                    var dauThang = new DateTime(homNay.Year, homNay.Month, 1);

                    return Results.Ok(new
                    {
                        // Sum(h => h.TongTien) — tổng tiền tất cả HĐ
                        tongDoanhThu = ds.Sum(h => h.TongTien),
                        tongHoaDon = ds.Count,
                        // Doanh thu hôm nay: chỉ tính HĐ có ThoiGianThanhToan.Date == homNay
                        doanhThuHomNay = ds
                            .Where(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value.Date == homNay)
                            .Sum(h => h.TongTien),
                        // Số HĐ trong ngày hôm nay
                        hoaDonHomNay = ds.Count(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value.Date == homNay),
                        // Doanh thu từ đầu tháng tới nay
                        doanhThuThangNay = ds
                            .Where(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value >= dauThang)
                            .Sum(h => h.TongTien),
                        // Số HĐ từ đầu tháng tới nay
                        hoaDonThangNay = ds.Count(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value >= dauThang)
                    });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });
        }
    }
}
