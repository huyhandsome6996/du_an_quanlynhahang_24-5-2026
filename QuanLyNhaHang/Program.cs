// ============================================================
// PROGRAM.CS - Entry Point của ứng dụng
// File này CHỈ TỔNG HỢP các modun (mỗi modun nằm trong 1 file .cs
// riêng tại folder /CacModun). Mọi logic được tách ra để dễ bảo trì
// và dễ trình bày khi bảo vệ OOP.
// ============================================================
using QuanLyNhaHang.CacModun;

// 1) Khởi tạo WebApplication + DI + CSDL
var app = KhoiTaoUngDung.KhoiTao(args);

// 2) Đăng ký các nhóm API (mỗi nhóm trong 1 file riêng)
ApiTaiKhoan.DangKy(app);
ApiBan.DangKy(app);
ApiSanPham.DangKy(app);
ApiHoaDon.DangKy(app);
ApiBaoCao.DangKy(app);

// 3) Khởi chạy desktop WinForms + WebView2 (KHÔNG mở trình duyệt)
KhoiDongDesktop.Chay(app);
