// ============================================================
// PROGRAM.CS - Entry Point của ứng dụng
// V2: Thêm API Quản lý Kho, Báo cáo thống kê, Bếp, Đặt bàn, VAT/Giảm giá/PTTT
// ============================================================
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QuanLyNhaHang.DAL;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

// --- Đăng ký các DAL vào Dependency Injection ---
builder.Services.AddSingleton<IBanDAL, BanDAL>();
builder.Services.AddSingleton<ISanPhamDAL, SanPhamDAL>();
builder.Services.AddSingleton<IHoaDonDAL, HoaDonDAL>();
builder.Services.AddSingleton<IChiTietHoaDonDAL, ChiTietHoaDonDAL>();
builder.Services.AddSingleton<INguoiDungDAL, NguoiDungDAL>();
builder.Services.AddSingleton<INguyenLieuDAL, NguyenLieuDAL>();
builder.Services.AddSingleton<IKhoLogDAL, KhoLogDAL>();

var app = builder.Build();

DatabaseHelper.KhoiTaoCSDL();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = null,
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

// ============================================================
// HÀM TIỆN ÍCH: BĂM MẬT KHẨU SHA256
// ============================================================
static string BamSHA256(string matKhau)
{
    using var sha256 = SHA256.Create();
    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(matKhau));
    var sb = new StringBuilder();
    foreach (byte b in bytes)
        sb.Append(b.ToString("x2"));
    return sb.ToString();
}

// ============================================================
// API - ĐĂNG KÝ / ĐĂNG NHẬP
// ============================================================

app.MapGet("/api/auth/check", (INguoiDungDAL ndDAL) =>
{
    try { return Results.Ok(new { coNguoiDung = ndDAL.KiemTraCoNguoiDung() }); }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

app.MapPost("/api/auth/dangky", (JsonElement body, INguoiDungDAL ndDAL) =>
{
    try
    {
        string tenDangNhap = body.GetProperty("TenDangNhap").GetString() ?? "";
        string matKhau = body.GetProperty("MatKhau").GetString() ?? "";
        if (string.IsNullOrWhiteSpace(tenDangNhap) || tenDangNhap.Length < 3)
            return Results.BadRequest(new { thongBao = "Tên đăng nhập phải có ít nhất 3 ký tự!" });
        if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 4)
            return Results.BadRequest(new { thongBao = "Mật khẩu phải có ít nhất 4 ký tự!" });
        string matKhauHash = BamSHA256(matKhau);
        var nguoiDung = new NguoiDung { TenDangNhap = tenDangNhap.Trim(), MatKhauHash = matKhauHash, VaiTro = "QuanTri", NgayTao = DateTime.Now };
        ndDAL.Them(nguoiDung);
        return Results.Ok(new { thongBao = "Đăng ký thành công! Vui lòng đăng nhập." });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

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
        string matKhauHash = BamSHA256(matKhau);
        if (nguoiDung.MatKhauHash != matKhauHash) return Results.Unauthorized();
        return Results.Ok(new { thongBao = "Đăng nhập thành công!", tenDangNhap = nguoiDung.TenDangNhap, vaiTro = nguoiDung.VaiTro });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

// ============================================================
// API - QUẢN LÝ BÀN (Bổ sung Đặt bàn)
// ============================================================

app.MapGet("/api/ban", (IBanDAL banDAL) =>
{
    try { return Results.Ok(banDAL.LayTatCa()); }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

app.MapGet("/api/ban/{id:int}", (int id, IBanDAL banDAL) =>
{
    var ban = banDAL.LayTheoId(id);
    if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
    return Results.Ok(ban);
});

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

app.MapPut("/api/ban/{id:int}", (int id, Ban ban, IBanDAL banDAL) =>
{
    try { ban.Id = id; banDAL.Sua(ban); return Results.Ok(new { thongBao = "Cập nhật bàn thành công!" }); }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

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

// POST /api/ban/{id}/dat - Đặt bàn (Trống → Đã đặt)
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

// ============================================================
// API - QUẢN LÝ SẢN PHẨM (MENU)
// ============================================================

app.MapGet("/api/sanpham", (ISanPhamDAL spDAL) =>
{
    try
    {
        var ds = spDAL.LayTatCa();
        var ketQua = ds.Select(sp => new { sp.Id, sp.TenSanPham, sp.GiaCoBan, sp.Loai, sp.DangBan, sp.HinhAnh });
        return Results.Ok(ketQua);
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

app.MapGet("/api/sanpham/dangban", (ISanPhamDAL spDAL) =>
{
    var ds = spDAL.LayDangBan();
    var ketQua = ds.Select(sp => new { sp.Id, sp.TenSanPham, sp.GiaCoBan, sp.Loai, sp.DangBan, sp.HinhAnh });
    return Results.Ok(ketQua);
});

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
        sp.Id = id; sp.TenSanPham = tenSanPham; sp.GiaCoBan = giaCoBan; sp.DangBan = dangBan; sp.HinhAnh = hinhAnh;
        spDAL.Sua(sp);
        return Results.Ok(new { thongBao = "Cập nhật sản phẩm thành công!" });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

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

// ============================================================
// API - QUẢN LÝ HÓA ĐƠN & GỌI MÓN (Bổ sung VAT, Giảm giá, PTTT)
// ============================================================

app.MapPost("/api/ban/{id:int}/mo", (int id, IBanDAL banDAL, IHoaDonDAL hdDAL) =>
{
    try
    {
        var ban = banDAL.LayTheoId(id);
        if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
        if (ban.TrangThai == "Có khách")
            return Results.BadRequest(new { thongBao = "Bàn đang có khách, không thể mở lại!" });
        var hoaDon = new HoaDon { BanId = id, ThoiGianTao = DateTime.Now, TrangThai = "Chưa thanh toán", TongTien = 0, VAT = 0, GiamGia = 0, PhuongThucThanhToan = "TienMat" };
        int hoaDonId = hdDAL.Them(hoaDon);
        banDAL.CapNhatTrangThai(id, "Có khách");
        return Results.Ok(new { thongBao = "Mở bàn thành công!", hoaDonId });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

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

app.MapPost("/api/hoadon/{id:int}/them-mon", (int id, JsonElement body, ISanPhamDAL spDAL, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
{
    try
    {
        int sanPhamId = body.GetProperty("SanPhamId").GetInt32();
        int soLuong = body.GetProperty("SoLuong").GetInt32();
        string thuocTinhThem = body.TryGetProperty("ThuocTinhThem", out var tt) ? tt.GetString() ?? "" : "";
        if (soLuong <= 0) return Results.BadRequest(new { thongBao = "Số lượng phải lớn hơn 0!" });
        var hoaDon = hdDAL.LayTheoId(id);
        if (hoaDon == null || hoaDon.TrangThai != "Chưa thanh toán")
            return Results.NotFound(new { thongBao = "Hóa đơn không tồn tại hoặc đã thanh toán!" });
        var sp = spDAL.LayTheoId(sanPhamId);
        if (sp == null) return Results.NotFound(new { thongBao = "Sản phẩm không tồn tại!" });
        decimal thanhTien = sp.TinhTien(soLuong, thuocTinhThem);
        decimal donGiaBan = thanhTien / soLuong;
        var chiTiet = new ChiTietHoaDon { HoaDonId = id, SanPhamId = sanPhamId, SoLuong = soLuong, DonGiaBan = donGiaBan, ThuocTinhThem = thuocTinhThem, ThanhTien = thanhTien, TrangThaiMon = "DangCho" };
        ctDAL.Them(chiTiet);
        decimal tongTienMoi = hoaDon.TongTien + thanhTien;
        hdDAL.CapNhatTongTien(id, tongTienMoi);
        return Results.Ok(new { thongBao = "Thêm món thành công!", thanhTien, tongTienMoi, moTaPhuPhi = sp.MoTaPhuPhi(thuocTinhThem) });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

app.MapDelete("/api/chitiethoadon/{id:int}", (int id, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
{
    try
    {
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseHelper.ConnectionString);
        conn.Open();
        var getCmd = new Microsoft.Data.Sqlite.SqliteCommand("SELECT HoaDonId, ThanhTien FROM ChiTietHoaDon WHERE Id = @id", conn);
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

// POST /api/ban/{id}/thanhtoan - Thanh toán (Bổ sung VAT, Giảm giá, PTTT)
app.MapPost("/api/ban/{id:int}/thanhtoan", (int id, JsonElement body, IBanDAL banDAL, IHoaDonDAL hdDAL) =>
{
    try
    {
        var hoaDon = hdDAL.LayHoaDonChuaThanhToanTheoBan(id);
        if (hoaDon == null)
            return Results.NotFound(new { thongBao = "Bàn này chưa có hóa đơn hoặc đã thanh toán!" });

        // Lấy VAT, Giảm giá, PTTT từ request body
        decimal vat = body.TryGetProperty("VAT", out var v) ? v.GetDecimal() : 0;
        decimal giamGia = body.TryGetProperty("GiamGia", out var g) ? g.GetDecimal() : 0;
        string pttt = body.TryGetProperty("PhuongThucThanhToan", out var p) ? p.GetString() ?? "TienMat" : "TienMat";

        // Cập nhật thông tin thanh toán
        hdDAL.CapNhatThanhToan(hoaDon.Id, vat, giamGia, pttt);

        // Cập nhật tổng tiền cuối cùng: TongTien + VAT - GiamGia
        decimal tongCuoi = hoaDon.TongTien + vat - giamGia;
        hdDAL.CapNhatTongTien(hoaDon.Id, Math.Max(0, tongCuoi));

        // Thanh toán hóa đơn
        hdDAL.ThanhToan(hoaDon.Id);

        // Giải phóng bàn
        banDAL.CapNhatTrangThai(id, "Trống");

        return Results.Ok(new { thongBao = "Thanh toán thành công!", tongTien = Math.Max(0, tongCuoi), vat, giamGia, phuongThuc = pttt });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

// ============================================================
// API - LỊCH SỬ HÓA ĐƠN
// ============================================================

app.MapGet("/api/hoadon", (IHoaDonDAL hdDAL) =>
{
    try { return Results.Ok(hdDAL.LayTatCa()); }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

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

// ============================================================
// API - QUẢN LÝ KHO (Nguyên Liệu + Nhập/Xuất)
// ============================================================

app.MapGet("/api/nguyenlieu", (INguyenLieuDAL nlDAL) =>
{
    try { return Results.Ok(nlDAL.LayTatCa()); }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

app.MapGet("/api/nguyenlieu/canhbao", (INguyenLieuDAL nlDAL) =>
{
    try { return Results.Ok(nlDAL.LayCanhBao()); }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

app.MapPost("/api/nguyenlieu", (JsonElement body, INguyenLieuDAL nlDAL) =>
{
    try
    {
        string ten = body.GetProperty("TenNguyenLieu").GetString() ?? "";
        string donVi = body.GetProperty("DonVi").GetString() ?? "";
        decimal soLuongTon = body.TryGetProperty("SoLuongTon", out var sl) ? sl.GetDecimal() : 0;
        decimal mucToiThieu = body.TryGetProperty("MucToiThieu", out var mt) ? mt.GetDecimal() : 0;
        string? ghiChu = body.TryGetProperty("GhiChu", out var gc) ? gc.GetString() : null;
        var nl = new NguyenLieu { TenNguyenLieu = ten, DonVi = donVi, SoLuongTon = soLuongTon, MucToiThieu = mucToiThieu, GhiChu = ghiChu };
        nlDAL.Them(nl);
        return Results.Ok(new { thongBao = "Thêm nguyên liệu thành công!" });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

app.MapPut("/api/nguyenlieu/{id:int}", (int id, JsonElement body, INguyenLieuDAL nlDAL) =>
{
    try
    {
        string ten = body.GetProperty("TenNguyenLieu").GetString() ?? "";
        string donVi = body.GetProperty("DonVi").GetString() ?? "";
        decimal soLuongTon = body.TryGetProperty("SoLuongTon", out var sl) ? sl.GetDecimal() : 0;
        decimal mucToiThieu = body.TryGetProperty("MucToiThieu", out var mt) ? mt.GetDecimal() : 0;
        string? ghiChu = body.TryGetProperty("GhiChu", out var gc) ? gc.GetString() : null;
        var nl = new NguyenLieu { Id = id, TenNguyenLieu = ten, DonVi = donVi, SoLuongTon = soLuongTon, MucToiThieu = mucToiThieu, GhiChu = ghiChu };
        nlDAL.Sua(nl);
        return Results.Ok(new { thongBao = "Cập nhật nguyên liệu thành công!" });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

app.MapDelete("/api/nguyenlieu/{id:int}", (int id, INguyenLieuDAL nlDAL) =>
{
    try { nlDAL.Xoa(id); return Results.Ok(new { thongBao = "Xóa nguyên liệu thành công!" }); }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

// POST /api/kho/nhap - Nhập kho
app.MapPost("/api/kho/nhap", (JsonElement body, INguyenLieuDAL nlDAL, IKhoLogDAL klDAL) =>
{
    try
    {
        int nlId = body.GetProperty("NguyenLieuId").GetInt32();
        decimal soLuong = body.GetProperty("SoLuong").GetDecimal();
        decimal donGia = body.TryGetProperty("DonGia", out var dg) ? dg.GetDecimal() : 0;
        string? lyDo = body.TryGetProperty("LyDo", out var ld) ? ld.GetString() : "Nhập kho";

        if (soLuong <= 0) return Results.BadRequest(new { thongBao = "Số lượng phải lớn hơn 0!" });

        var nl = nlDAL.LayTheoId(nlId);
        if (nl == null) return Results.NotFound(new { thongBao = "Không tìm thấy nguyên liệu!" });

        // Cập nhật tồn kho
        nlDAL.CapNhatSoLuongTon(nlId, nl.SoLuongTon + soLuong);

        // Ghi log nhập kho
        var log = new KhoLog { Loai = "Nhap", NguyenLieuId = nlId, TenNguyenLieu = nl.TenNguyenLieu, SoLuong = soLuong, DonGia = donGia, ThoiGian = DateTime.Now, LyDo = lyDo };
        klDAL.Them(log);

        return Results.Ok(new { thongBao = $"Nhập kho thành công! Đã thêm {soLuong} {nl.DonVi} {nl.TenNguyenLieu}" });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

// POST /api/kho/xuat - Xuất kho
app.MapPost("/api/kho/xuat", (JsonElement body, INguyenLieuDAL nlDAL, IKhoLogDAL klDAL) =>
{
    try
    {
        int nlId = body.GetProperty("NguyenLieuId").GetInt32();
        decimal soLuong = body.GetProperty("SoLuong").GetDecimal();
        string? lyDo = body.TryGetProperty("LyDo", out var ld) ? ld.GetString() : "Xuất kho";

        if (soLuong <= 0) return Results.BadRequest(new { thongBao = "Số lượng phải lớn hơn 0!" });

        var nl = nlDAL.LayTheoId(nlId);
        if (nl == null) return Results.NotFound(new { thongBao = "Không tìm thấy nguyên liệu!" });
        if (nl.SoLuongTon < soLuong) return Results.BadRequest(new { thongBao = $"Tồn kho không đủ! Hiện có: {nl.SoLuongTon} {nl.DonVi}" });

        // Cập nhật tồn kho
        nlDAL.CapNhatSoLuongTon(nlId, nl.SoLuongTon - soLuong);

        // Ghi log xuất kho
        var log = new KhoLog { Loai = "Xuat", NguyenLieuId = nlId, TenNguyenLieu = nl.TenNguyenLieu, SoLuong = soLuong, DonGia = 0, ThoiGian = DateTime.Now, LyDo = lyDo };
        klDAL.Them(log);

        return Results.Ok(new { thongBao = $"Xuất kho thành công! Đã xuất {soLuong} {nl.DonVi} {nl.TenNguyenLieu}" });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

app.MapGet("/api/kholog", (IKhoLogDAL klDAL) =>
{
    try { return Results.Ok(klDAL.LayTatCa()); }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

// ============================================================
// API - BẾP (Kitchen Display + Cập nhật trạng thái món)
// ============================================================

app.MapGet("/api/bep/dangcho", (IChiTietHoaDonDAL ctDAL) =>
{
    try { return Results.Ok(ctDAL.LayMonDangCho()); }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

app.MapGet("/api/bep/dangchuanbi", (IChiTietHoaDonDAL ctDAL) =>
{
    try { return Results.Ok(ctDAL.LayMonDangChuanBi()); }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

// PUT /api/chitiethoadon/{id}/trangthai - Cập nhật trạng thái món
app.MapPut("/api/chitiethoadon/{id:int}/trangthai", (int id, JsonElement body, IChiTietHoaDonDAL ctDAL) =>
{
    try
    {
        string trangThai = body.GetProperty("TrangThaiMon").GetString() ?? "";
        if (trangThai != "DangCho" && trangThai != "DangChuanBi" && trangThai != "DaPhucVu")
            return Results.BadRequest(new { thongBao = "Trạng thái không hợp lệ! (DangCho/DangChuanBi/DaPhucVu)" });
        ctDAL.CapNhatTrangThaiMon(id, trangThai);
        return Results.Ok(new { thongBao = "Cập nhật trạng thái món thành công!" });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

// ============================================================
// API - BÁO CÁO THỐNG KÊ
// ============================================================

// GET /api/baocao/monbanchay - Thống kê món bán chạy nhất
app.MapGet("/api/baocao/monbanchay", (int? top, ISanPhamDAL spDAL, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
{
    try
    {
        int soLuongTop = top ?? 10;
        // Lấy tất cả chi tiết hóa đơn đã thanh toán
        var dsHoaDon = hdDAL.LayTatCa().Where(h => h.TrangThai == "Đã thanh toán");
        var thongKe = new Dictionary<int, ThongKeMon>();
        foreach (var hd in dsHoaDon)
        {
            var chiTiet = ctDAL.LayTheoHoaDon(hd.Id);
            foreach (var ct in chiTiet)
            {
                if (!thongKe.ContainsKey(ct.SanPhamId))
                    thongKe[ct.SanPhamId] = new ThongKeMon { SanPhamId = ct.SanPhamId, TenSanPham = ct.TenSanPham, TongSoLuong = 0, TongDoanhThu = 0 };
                thongKe[ct.SanPhamId].TongSoLuong += ct.SoLuong;
                thongKe[ct.SanPhamId].TongDoanhThu += ct.ThanhTien;
            }
        }
        var ketQua = thongKe.Values.OrderByDescending(t => t.TongSoLuong).Take(soLuongTop).ToList();
        return Results.Ok(ketQua);
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

// GET /api/baocao/doanhthu - Thống kê doanh thu tổng
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
            doanhThuHomNay = ds.Where(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value.Date == homNay).Sum(h => h.TongTien),
            hoaDonHomNay = ds.Count(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value.Date == homNay),
            doanhThuThangNay = ds.Where(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value >= dauThang).Sum(h => h.TongTien),
            hoaDonThangNay = ds.Count(h => h.ThoiGianThanhToan.HasValue && h.ThoiGianThanhToan.Value >= dauThang)
        });
    }
    catch (Exception ex) { return Results.BadRequest(new { thongBao = ex.Message }); }
});

// Helper class cho thống kê món bán chạy
public class ThongKeMon
{
    public int SanPhamId { get; set; }
    public string TenSanPham { get; set; } = "";
    public int TongSoLuong { get; set; }
    public decimal TongDoanhThu { get; set; }
}

// ============================================================
// KHỞI CHẠY ỨNG DỤNG (DESKTOP MODE)
// ============================================================

Console.WriteLine("🍽️  ỨNG DỤNG QUẢN LÝ NHÀ HÀNG V2 (DESKTOP MODE)");
Console.WriteLine("=================================");

_ = Task.Run(() => app.Run("http://localhost:5000"));

var uiThread = new Thread(() =>
{
    System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.SystemAware);
    System.Windows.Forms.Application.EnableVisualStyles();
    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

    var formMain = new System.Windows.Forms.Form
    {
        Text = "🍽️ Hệ Thống Quản Lý Nhà Hàng (Desktop App)",
        Width = 1350,
        Height = 850,
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
    };

    try
    {
        string iconPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo.png");
        if (File.Exists(iconPath))
        {
            using var bitmap = new System.Drawing.Bitmap(iconPath);
            formMain.Icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
        }
    }
    catch { }

    var webView = new Microsoft.Web.WebView2.WinForms.WebView2
    {
        Dock = System.Windows.Forms.DockStyle.Fill
    };

    formMain.Controls.Add(webView);

    formMain.Load += async (s, e) =>
    {
        try
        {
            await Task.Delay(1200);
            await webView.EnsureCoreWebView2Async();
            webView.Source = new Uri("http://localhost:5000");
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(
                $"Không thể tải giao diện WebView2: {ex.Message}",
                "Lỗi Giao Diện",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error
            );
        }
    };

    System.Windows.Forms.Application.Run(formMain);
});

uiThread.SetApartmentState(ApartmentState.STA);
uiThread.Start();
uiThread.Join();
