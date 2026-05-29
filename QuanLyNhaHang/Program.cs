// ============================================================
// PROGRAM.CS - Entry Point của ứng dụng
// Đăng ký toàn bộ API Routes và khởi động web server
// ============================================================
using System.Text.Json;
using QuanLyNhaHang.DAL;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

var builder = WebApplication.CreateBuilder(args);

// --- Cấu hình để phục vụ file HTML tĩnh ---
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// Cấu hình JSON Options cho Minimal API (giữ nguyên PascalCase của C# Properties và hỗ trợ tiếng Việt)
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

var app = builder.Build();

// --- Khởi tạo CSDL SQLite khi app chạy ---
DatabaseHelper.KhoiTaoCSDL();

app.UseCors();
app.UseDefaultFiles();   // Phục vụ index.html mặc định
app.UseStaticFiles();    // Phục vụ CSS, JS, HTML trong wwwroot

// Cấu hình JSON trả về tiếng Việt không bị encode
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = null,
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

// ============================================================
// ====               API - QUẢN LÝ BÀN                   ====
// ============================================================

// GET /api/ban - Lấy tất cả bàn
app.MapGet("/api/ban", (IBanDAL banDAL) =>
{
    try
    {
        var dsBan = banDAL.LayTatCa();
        return Results.Ok(dsBan);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
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
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// PUT /api/ban/{id} - Sửa bàn
app.MapPut("/api/ban/{id:int}", (int id, Ban ban, IBanDAL banDAL) =>
{
    try
    {
        ban.Id = id;
        banDAL.Sua(ban);
        return Results.Ok(new { thongBao = "Cập nhật bàn thành công!" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// DELETE /api/ban/{id} - Xóa bàn
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
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// ============================================================
// ====            API - QUẢN LÝ SẢN PHẨM (MENU)          ====
// ============================================================

// GET /api/sanpham - Lấy tất cả sản phẩm
app.MapGet("/api/sanpham", (ISanPhamDAL spDAL) =>
{
    try
    {
        var ds = spDAL.LayTatCa();
        // Chuyển sang anonymous object để JSON dễ đọc hơn
        var ketQua = ds.Select(sp => new
        {
            sp.Id, sp.TenSanPham, sp.GiaCoBan, sp.Loai, sp.DangBan, sp.HinhAnh
        });
        return Results.Ok(ketQua);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// GET /api/sanpham/dangban - Lấy món đang phục vụ (cho màn hình Order)
app.MapGet("/api/sanpham/dangban", (ISanPhamDAL spDAL) =>
{
    var ds = spDAL.LayDangBan();
    var ketQua = ds.Select(sp => new
    {
        sp.Id, sp.TenSanPham, sp.GiaCoBan, sp.Loai, sp.DangBan, sp.HinhAnh
    });
    return Results.Ok(ketQua);
});

// POST /api/sanpham - Thêm sản phẩm mới
app.MapPost("/api/sanpham", (JsonElement body, ISanPhamDAL spDAL) =>
{
    try
    {
        string tenSanPham = body.GetProperty("TenSanPham").GetString() ?? "";
        decimal giaCoBan = body.GetProperty("GiaCoBan").GetDecimal();
        string loai = body.GetProperty("Loai").GetString() ?? "";
        bool dangBan = body.TryGetProperty("DangBan", out var db) ? db.GetBoolean() : true;
        string? hinhAnh = body.TryGetProperty("HinhAnh", out var img) ? img.GetString() : null;

        // ĐA HÌNH: Tạo đúng loại object dựa trên trường 'Loai'
        SanPham sp = loai == "ThucAn" ? new ThucAn() : new NuocUong();
        sp.TenSanPham = tenSanPham;  // Có validate trong setter (Encapsulation)
        sp.GiaCoBan = giaCoBan;      // Có validate >= 0 trong setter
        sp.DangBan = dangBan;
        sp.HinhAnh = hinhAnh;

        spDAL.Them(sp);
        return Results.Ok(new { thongBao = "Thêm sản phẩm thành công!" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
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
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
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
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// ============================================================
// ====         API - QUẢN LÝ HÓA ĐƠN & GỌI MÓN          ====
// ============================================================

// POST /api/ban/{id}/mo - Mở bàn (tạo hóa đơn mới)
app.MapPost("/api/ban/{id:int}/mo", (int id, IBanDAL banDAL, IHoaDonDAL hdDAL) =>
{
    try
    {
        var ban = banDAL.LayTheoId(id);
        if (ban == null) return Results.NotFound(new { thongBao = "Không tìm thấy bàn!" });
        if (ban.TrangThai == "Có khách")
            return Results.BadRequest(new { thongBao = "Bàn đang có khách, không thể mở lại!" });

        // Tạo hóa đơn mới
        var hoaDon = new HoaDon
        {
            BanId = id,
            ThoiGianTao = DateTime.Now,
            TrangThai = "Chưa thanh toán",
            TongTien = 0
        };
        int hoaDonId = hdDAL.Them(hoaDon);

        // Cập nhật trạng thái bàn thành "Có khách"
        banDAL.CapNhatTrangThai(id, "Có khách");

        return Results.Ok(new { thongBao = "Mở bàn thành công!", hoaDonId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// GET /api/ban/{id}/hoadon - Lấy hóa đơn đang mở của bàn
app.MapGet("/api/ban/{id:int}/hoadon", (int id, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
{
    try
    {
        var hoaDon = hdDAL.LayHoaDonChuaThanhToanTheoBan(id);
        if (hoaDon == null)
            return Results.NotFound(new { thongBao = "Bàn này hiện chưa có hóa đơn!" });

        var chiTiet = ctDAL.LayTheoHoaDon(hoaDon.Id);

        return Results.Ok(new { hoaDon, chiTiet });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// POST /api/hoadon/{id}/them-mon - Thêm món vào hóa đơn
app.MapPost("/api/hoadon/{id:int}/them-mon", (int id, JsonElement body,
    ISanPhamDAL spDAL, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
{
    try
    {
        int sanPhamId = body.GetProperty("SanPhamId").GetInt32();
        int soLuong = body.GetProperty("SoLuong").GetInt32();
        string thuocTinhThem = body.TryGetProperty("ThuocTinhThem", out var tt)
            ? tt.GetString() ?? "" : "";

        // Validation
        if (soLuong <= 0)
            return Results.BadRequest(new { thongBao = "Số lượng phải lớn hơn 0!" });

        // Lấy hóa đơn
        var hoaDon = hdDAL.LayTheoId(id);
        if (hoaDon == null || hoaDon.TrangThai != "Chưa thanh toán")
            return Results.NotFound(new { thongBao = "Hóa đơn không tồn tại hoặc đã thanh toán!" });

        // Lấy sản phẩm - ĐA HÌNH được thể hiện ở đây
        var sp = spDAL.LayTheoId(sanPhamId);
        if (sp == null) return Results.NotFound(new { thongBao = "Sản phẩm không tồn tại!" });

        // Gọi TinhTien() - Đa hình: ThucAn và NuocUong tính khác nhau!
        decimal thanhTien = sp.TinhTien(soLuong, thuocTinhThem);
        decimal donGiaBan = thanhTien / soLuong; // Đơn giá đã tính phụ phí

        // Thêm chi tiết hóa đơn
        var chiTiet = new ChiTietHoaDon
        {
            HoaDonId = id,
            SanPhamId = sanPhamId,
            SoLuong = soLuong,
            DonGiaBan = donGiaBan,
            ThuocTinhThem = thuocTinhThem,
            ThanhTien = thanhTien
        };
        ctDAL.Them(chiTiet);

        // Cập nhật tổng tiền hóa đơn
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
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// DELETE /api/chitiethoadon/{id} - Xóa 1 món khỏi hóa đơn
app.MapDelete("/api/chitiethoadon/{id:int}", (int id, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
{
    try
    {
        // Tìm chi tiết hóa đơn
        // Lấy danh sách tất cả chi tiết để tìm theo id
        // (Đơn giản hóa: dùng cách này để tránh thêm method mới)
        var ds = new List<ChiTietHoaDon>();

        // Xóa và tính lại tổng tiền
        // Ta cần lấy thông tin trước khi xóa
        using var conn = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseHelper.ConnectionString);
        conn.Open();

        // Lấy thông tin chi tiết trước khi xóa
        var getCmd = new Microsoft.Data.Sqlite.SqliteCommand(
            "SELECT HoaDonId, ThanhTien FROM ChiTietHoaDon WHERE Id = @id", conn);
        getCmd.Parameters.AddWithValue("@id", id);
        using var reader = getCmd.ExecuteReader();

        if (!reader.Read())
            return Results.NotFound(new { thongBao = "Không tìm thấy món này!" });

        int hoaDonId = reader.GetInt32(0);
        decimal thanhTien = reader.GetDecimal(1);
        reader.Close();

        // Xóa chi tiết
        ctDAL.Xoa(id);

        // Cập nhật lại tổng tiền hóa đơn
        var hoaDon = hdDAL.LayTheoId(hoaDonId);
        if (hoaDon != null)
        {
            decimal tongTienMoi = hoaDon.TongTien - thanhTien;
            hdDAL.CapNhatTongTien(hoaDonId, Math.Max(0, tongTienMoi));
        }

        return Results.Ok(new { thongBao = "Xóa món thành công!" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// POST /api/ban/{id}/thanhtoan - Thanh toán và đóng bàn
app.MapPost("/api/ban/{id:int}/thanhtoan", (int id, IBanDAL banDAL, IHoaDonDAL hdDAL) =>
{
    try
    {
        var hoaDon = hdDAL.LayHoaDonChuaThanhToanTheoBan(id);
        if (hoaDon == null)
            return Results.NotFound(new { thongBao = "Bàn này chưa có hóa đơn hoặc đã thanh toán!" });

        // Thanh toán hóa đơn
        hdDAL.ThanhToan(hoaDon.Id);

        // Giải phóng bàn về trạng thái "Trống"
        banDAL.CapNhatTrangThai(id, "Trống");

        return Results.Ok(new
        {
            thongBao = "Thanh toán thành công! Bàn đã được giải phóng.",
            tongTien = hoaDon.TongTien
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// ============================================================
// ====           API - LỊCH SỬ HÓA ĐƠN                  ====
// ============================================================

// GET /api/hoadon - Lấy toàn bộ lịch sử hóa đơn
app.MapGet("/api/hoadon", (IHoaDonDAL hdDAL) =>
{
    try
    {
        var ds = hdDAL.LayTatCa();
        return Results.Ok(ds);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

// GET /api/hoadon/{id} - Chi tiết 1 hóa đơn (kèm danh sách món)
app.MapGet("/api/hoadon/{id:int}", (int id, IHoaDonDAL hdDAL, IChiTietHoaDonDAL ctDAL) =>
{
    try
    {
        var hoaDon = hdDAL.LayTheoId(id);
        if (hoaDon == null) return Results.NotFound(new { thongBao = "Không tìm thấy hóa đơn!" });

        var chiTiet = ctDAL.LayTheoHoaDon(id);
        return Results.Ok(new { hoaDon, chiTiet });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { thongBao = ex.Message });
    }
});

Console.WriteLine("🍽️  ỨNG DỤNG QUẢN LÝ NHÀ HÀNG (DESKTOP MODE)");
Console.WriteLine("=================================");

// Khởi chạy Web API dưới luồng nền (Background thread)
_ = Task.Run(() => app.Run("http://localhost:5000"));

// Khởi chạy Windows Forms trên luồng STA (Single-Threaded Apartment) để tránh lỗi COM thread mode
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

    var webView = new Microsoft.Web.WebView2.WinForms.WebView2
    {
        Dock = System.Windows.Forms.DockStyle.Fill
    };

    formMain.Controls.Add(webView);

    formMain.Load += async (s, e) =>
    {
        try
        {
            // Chờ 1.2 giây để server Web API khởi động hoàn tất
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

// Thiết lập luồng STA bắt buộc cho WebView2
uiThread.SetApartmentState(ApartmentState.STA);
uiThread.Start();
uiThread.Join(); // Đợi luồng giao diện kết thúc thì tắt ứng dụng
