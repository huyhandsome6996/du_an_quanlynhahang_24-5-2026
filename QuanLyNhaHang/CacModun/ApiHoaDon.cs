// ============================================================
// API_HOADON.CS - Hóa đơn, Gọi món, Chi tiết hóa đơn, Thanh toán
// ------------------------------------------------------------
// Đây là modun PHỨC TẠP NHẤT — quản lý quan hệ nhiều đối tượng:
//   Bàn (1) — (N) HóaDon (1) — (N) ChiTietHoaDon (N) — (1) SanPham
//
// 8 endpoint:
//   POST   /api/ban/{id}/mo                  — Mở bàn (tạo HóaDon mới, Bàn → "Có khách")
//   GET    /api/ban/{id}/hoadon              — Lấy HĐ chưa TT + chi tiết món của bàn
//   POST   /api/hoadon/{id}/them-mon         — Thêm món vào HĐ (dùng TinhTien() đa hình)
//   DELETE /api/chitiethoadon/{id}           — Xoá 1 món khỏi HĐ + tính lại tổng
//   POST   /api/ban/{id}/thanhtoan           — Thanh toán: VAT, giảm giá, PTTT, giải phóng bàn
//   GET    /api/hoadon                       — Lấy tất cả HĐ (cho trang Lịch sử)
//   GET    /api/hoadon/{id}                  — Lấy 1 HĐ + chi tiết món
//   GET    /api/hoadon/theongay              — Lọc HĐ theo khoảng ngày (cho Báo cáo)
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
    /// <summary>
    /// Lớp tĩnh ApiHoaDon — đăng ký các endpoint hóa đơn.
    /// </summary>
    public static class ApiHoaDon
    {
        /// <summary>Đăng ký tất cả API Hóa Đơn vào WebApplication.</summary>
        public static void DangKy(this WebApplication app)
        {
            // =====================================================
            // 1. POST /api/ban/{id}/mo
            // Mở bàn: tạo HóaDon mới (TrangThai="Chưa TT"), Bàn → "Có khách".
            // Dùng khi: khách mới ngồi vào bàn đang Trống.
            // =====================================================
            app.MapPost("/api/ban/{id:int}/mo", (int id, IBanDAL banDAL, IHoaDonDAL hdDAL) =>
            {
                try
                {
                    var ban = banDAL.LayTheoId(id);
                    if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
                    // Bàn đã có khách → không cho mở lại (tránh tạo 2 HĐ chưa TT cùng lúc)
                    if (ban.TrangThai == "Có khách")
                        return Results.BadRequest(new { thongBao = "Bàn đang có khách, không thể mở lại!" });

                    // Tạo object HóaDon mới
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
                    // Them() trả về Id tự tăng
                    int hoaDonId = hdDAL.Them(hoaDon);
                    // Cập nhật trạng thái bàn → "Có khách"
                    banDAL.CapNhatTrangThai(id, "Có khách");
                    return Results.Ok(new { thongBao = "Mở bàn thành công!", hoaDonId });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // =====================================================
            // 2. GET /api/ban/{id}/hoadon
            // Lấy hóa đơn CHƯA TT của bàn + danh sách món trong HĐ đó.
            // Dùng cho: khi click vào bàn có khách → hiển thị chi tiết.
            // =====================================================
            app.MapGet("/api/ban/{id:int}/hoadon", (int id, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
            {
                try
                {
                    var hoaDon = hdDAL.LayHoaDonChuaThanhToanTheoBan(id);
                    // Bàn trống hoặc đã TT → trả 404
                    if (hoaDon == null) return Results.NotFound(new { thongBao = "Bàn này hiện chưa có hóa đơn!" });
                    // Lấy danh sách món trong HĐ
                    var chiTiet = ctDAL.LayTheoHoaDon(hoaDon.Id);
                    // Trả về object ẩn danh { hoaDon, chiTiet }
                    return Results.Ok(new { hoaDon, chiTiet });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // =====================================================
            // 3. POST /api/hoadon/{id}/them-mon
            // Thêm 1 món vào hóa đơn.
            // BODY: { SanPhamId, SoLuong, ThuocTinhThem }
            //
            // ĐA HÌNH OOP được dùng ở đây:
            //   sp = spDAL.LayTheoId(sanPhamId) → trả ThucAn hoặc NuocUong
            //   sp.TinhTien(soLuong, thuocTinhThem) → gọi override của lớp con tương ứng
            //   → ThucAn: +50k nếu "Phần lớn", NuocUong: ×1.2 nếu "Lon"
            // =====================================================
            app.MapPost("/api/hoadon/{id:int}/them-mon",
                (int id, JsonElement body, ISanPhamDAL spDAL, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
            {
                try
                {
                    // Parse body
                    int sanPhamId = body.GetProperty("SanPhamId").GetInt32();
                    int soLuong = body.GetProperty("SoLuong").GetInt32();
                    // ThuocTinhThem optional — mặc định ""
                    string thuocTinhThem = body.TryGetProperty("ThuocTinhThem", out var tt) ? tt.GetString() ?? "" : "";

                    // Validate số lượng
                    if (soLuong <= 0)
                        return Results.BadRequest(new { thongBao = "Số lượng phải lớn hơn 0!" });

                    // Kiểm tra HĐ tồn tại và chưa TT
                    var hoaDon = hdDAL.LayTheoId(id);
                    if (hoaDon == null || hoaDon.TrangThai != "Chưa thanh toán")
                        return Results.NotFound(new { thongBao = "Hóa đơn không tồn tại hoặc đã thanh toán!" });

                    // Lấy sản phẩm — sp có thể là ThucAn hoặc NuocUong (đa hình)
                    var sp = spDAL.LayTheoId(sanPhamId);
                    if (sp == null) return Results.NotFound(new { thongBao = "Sản phẩm không tồn tại!" });

                    // GỌI TinhTien() — ĐA HÌNH: C# tự gọi override của lớp con
                    decimal thanhTien = sp.TinhTien(soLuong, thuocTinhThem);
                    // Đơn giá = Thành tiền / Số lượng (để hiển thị lại ở Lịch sử)
                    decimal donGiaBan = thanhTien / soLuong;

                    // Tạo object ChiTietHoaDon
                    var chiTiet = new ChiTietHoaDon
                    {
                        HoaDonId = id,
                        SanPhamId = sanPhamId,
                        SoLuong = soLuong,
                        DonGiaBan = donGiaBan,
                        ThuocTinhThem = thuocTinhThem,
                        ThanhTien = thanhTien,
                        TrangThaiMon = "DangCho"   // Vừa order → đang chờ bếp
                    };
                    ctDAL.Them(chiTiet);

                    // Cập nhật tổng tiền của hóa đơn = tổng cũ + thành tiền món mới
                    decimal tongTienMoi = hoaDon.TongTien + thanhTien;
                    hdDAL.CapNhatTongTien(id, tongTienMoi);

                    // Trả về thông tin cho UI hiển thị thông báo
                    return Results.Ok(new
                    {
                        thongBao = "Thêm món thành công!",
                        thanhTien,
                        tongTienMoi,
                        // MoTaPhuPhi() cũng đa hình — trả "+50,000đ (Phần lớn)" hoặc "+20% (Dạng Lon)"
                        moTaPhuPhi = sp.MoTaPhuPhi(thuocTinhThem)
                    });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // =====================================================
            // 4. DELETE /api/chitiethoadon/{id}
            // Xoá 1 món khỏi hóa đơn + tính lại tổng tiền.
            // Phải dùng SQL trực tiếp vì IChiTietHoaDonDAL không có hàm
            // trả về thông tin chi tiết theo Id (chỉ có theo HoaDonId).
            // =====================================================
            app.MapDelete("/api/chitiethoadon/{id:int}",
                (int id, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
            {
                try
                {
                    // Bước 1: Đọc HoaDonId và ThanhTien của dòng cần xoá
                    using var conn = new OleDbConnection(DatabaseHelper.ConnectionString);
                    conn.Open();
                    using var getCmd = new OleDbCommand(
                        "SELECT HoaDonId, ThanhTien FROM ChiTietHoaDon WHERE Id = @id", conn);
                    getCmd.Parameters.AddWithValue("@id", id);
                    using var reader = getCmd.ExecuteReader();
                    // Không tìm thấy dòng → trả 404
                    if (!reader.Read()) return Results.NotFound(new { thongBao = "Không tìm thấy món này!" });

                    int hoaDonId = reader.GetInt32(0);          // Lấy HoaDonId
                    decimal thanhTien = reader.GetDecimal(1);   // Lấy ThanhTien của món
                    reader.Close();                              // Đóng reader để chạy SQL tiếp

                    // Bước 2: Xoá dòng chi tiết
                    ctDAL.Xoa(id);

                    // Bước 3: Tính lại tổng tiền của HĐ = tổng cũ − thành tiền món vừa xoá
                    var hoaDon = hdDAL.LayTheoId(hoaDonId);
                    if (hoaDon != null)
                    {
                        // Math.Max(0, ...) để không bị âm nếu dữ liệu lệch
                        decimal tongTienMoi = hoaDon.TongTien - thanhTien;
                        hdDAL.CapNhatTongTien(hoaDonId, Math.Max(0, tongTienMoi));
                    }
                    return Results.Ok(new { thongBao = "Xóa món thành công!" });
                }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // =====================================================
            // 5. POST /api/ban/{id}/thanhtoan
            // Thanh toán hóa đơn của bàn:
            //   - Cập nhật VAT, GiamGia, PTTT
            //   - Tính lại TongTien = TongTienMon + VAT − GiamGia
            //   - Đánh dấu "Đã thanh toán", set ThoiGianThanhToan
            //   - Đổi trạng thái bàn → "Trống" (giải phóng bàn)
            // BODY: { VAT, GiamGia, PhuongThucThanhToan }
            // =====================================================
            app.MapPost("/api/ban/{id:int}/thanhtoan",
                (int id, JsonElement body, IBanDAL banDAL, IHoaDonDAL hdDAL) =>
            {
                try
                {
                    // Tìm HĐ chưa TT của bàn
                    var hoaDon = hdDAL.LayHoaDonChuaThanhToanTheoBan(id);
                    if (hoaDon == null)
                        return Results.NotFound(new { thongBao = "Bàn này chưa có hóa đơn hoặc đã thanh toán!" });

                    // Parse body — lấy VAT, GiamGia, PTTT (có giá trị mặc định nếu thiếu)
                    decimal vat = body.TryGetProperty("VAT", out var v) ? v.GetDecimal() : 0;
                    decimal giamGia = body.TryGetProperty("GiamGia", out var g) ? g.GetDecimal() : 0;
                    string pttt = body.TryGetProperty("PhuongThucThanhToan", out var p)
                        ? p.GetString() ?? "TienMat" : "TienMat";

                    // Lưu VAT, GiamGia, PTTT vào HĐ
                    hdDAL.CapNhatThanhToan(hoaDon.Id, vat, giamGia, pttt);

                    // Tính tổng cuối = Tổng món + VAT − Giảm giá (không âm)
                    decimal tongCuoi = hoaDon.TongTien + vat - giamGia;
                    hdDAL.CapNhatTongTien(hoaDon.Id, Math.Max(0, tongCuoi));

                    // Đánh dấu "Đã thanh toán" + set ThoiGianThanhToan = now
                    hdDAL.ThanhToan(hoaDon.Id);

                    // Giải phóng bàn: "Có khách" → "Trống"
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

            // =====================================================
            // 6. GET /api/hoadon — Lấy tất cả hóa đơn (cho trang Lịch sử)
            // =====================================================
            app.MapGet("/api/hoadon", (IHoaDonDAL hdDAL) =>
            {
                try { return Results.Ok(hdDAL.LayTatCa()); }
                catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
            });

            // =====================================================
            // 7. GET /api/hoadon/{id} — Lấy 1 hóa đơn + chi tiết món
            // Dùng cho: xem chi tiết 1 hóa đơn trong Lịch sử / Báo cáo.
            // =====================================================
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

            // =====================================================
            // 8. GET /api/hoadon/theongay?tuNgay=...&denNgay=...
            // Lọc hóa đơn ĐÃ TT theo khoảng ngày (cho trang Báo cáo).
            // ASP.NET tự bind query string → DateTime.
            // =====================================================
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
