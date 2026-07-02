// ============================================================
// PROGRAM.CS - ĐIỂM VÀO (ENTRY POINT) của ứng dụng
// ------------------------------------------------------------
// File này chỉ TỔNG HỢP các modun (mỗi modun nằm trong 1 file .cs
// riêng tại folder /CacModun). Mọi logic được tách ra để:
//   1. Dễ bảo trì — sửa 1 modun không ảnh hưởng modun khác
//   2. Dễ trình bày khi bảo vệ OOP — chỉ cần mở 1 file nhỏ
//
// QUY TRÌNH KHỞI ĐỘNG ỨNG DỤNG (3 bước):
//   Bước 1: KhoiTaoUngDung.KhoiTao()  → tạo WebApplication + DI + kiểm tra CSDL
//   Bước 2: ApiXxx.DangKy(app)         → đăng ký 5 nhóm API endpoints
//   Bước 3: KhoiDongDesktop.Chay(app)  → mở cửa sổ WinForms + WebView2
// ============================================================
using QuanLyNhaHang.CacModun;

// Bước 1: Khởi tạo WebApplication + DI + kiểm tra file CSDL Access
var app = KhoiTaoUngDung.KhoiTao(args);

// Bước 2: Đăng ký 5 nhóm API (mỗi nhóm trong 1 file riêng tại /CacModun)
ApiTaiKhoan.DangKy(app);   // Đăng ký / Đăng nhập (POST /api/auth/...)
ApiBan.DangKy(app);        // CRUD Bàn + Đặt bàn (GET/POST/PUT/DELETE /api/ban/...)
ApiSanPham.DangKy(app);    // CRUD Sản phẩm (GET/POST/PUT/DELETE /api/sanpham/...)
ApiHoaDon.DangKy(app);     // Mở bàn / Gọi món / Thanh toán (POST /api/ban/{id}/mo, ...)
ApiBaoCao.DangKy(app);     // Báo cáo doanh thu + Top món bán chạy (GET /api/baocao/...)

// Bước 3: Mở cửa sổ desktop WinForms chứa WebView2 trỏ vào server :5000
// KHÔNG mở trình duyệt web bên ngoài.
KhoiDongDesktop.Chay(app);
