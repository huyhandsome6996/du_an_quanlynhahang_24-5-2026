// ============================================================
// PHANQUYEN.CS - Helper kiểm tra vai trò (PHÂN QUYỀN)
// ------------------------------------------------------------
// Mục đích: Đồng bộ mã nguồn với SƠ ĐỒ USE CASE.
//
// Sơ đồ Use Case phân 2 vai trò:
//   - NhanVien: Đăng nhập, Xem sơ đồ bàn, Gọi món, Thanh toán,
//               Xem lịch sử hóa đơn.
//   - QuanTri : Tất cả quyền của NhanVien + Quản lý thực đơn,
//               Quản lý tài khoản, Xem báo cáo doanh thu,
//               Đổi trạng thái bàn (thêm/sửa/xoá bàn).
//
// Cách hoạt động:
//   Frontend lưu vaiTro vào sessionStorage khi đăng nhập,
//   và gửi kèm header "X-Vai-Tro" trong mọi lời gọi API.
//   Backend đọc header này qua PhanQuyen.LayVaiTro(httpContext).
//   Nếu header thiếu/rỗng → coi là "NhanVien" (ít quyền hơn cho an toàn).
//
// LƯU Ý: Đây là cơ chế phân quyền "mềm" dành cho đồ án học tập.
//   Trong thực tế cần dùng JWT/Cookie auth middleware. Tuy nhiên
//   kiến trúc này đủ để DEMO khái niệm Role-Based Access Control.
// ============================================================
using Microsoft.AspNetCore.Http;

namespace QuanLyNhaHang.CacModun
{
    /// <summary>
    /// Lớp tĩnh PhanQuyen — tiện ích kiểm tra vai trò người dùng.
    /// </summary>
    public static class PhanQuyen
    {
        /// <summary>
        /// Đọc vai trò từ header "X-Vai-Tro" của request.
        /// Trả về "QuanTri", "NhanVien", hoặc "" nếu không có header.
        /// </summary>
        public static string LayVaiTro(HttpContext ctx)
        {
            // Header.Get trả về StringValues; nếu thiếu → trả ""
            string? v = ctx.Request.Headers["X-Vai-Tro"].FirstOrDefault();
            return string.IsNullOrWhiteSpace(v) ? "" : v!;
        }

        /// <summary>
        /// Kiểm tra request hiện tại có phải từ QuanTri không.
        /// Trả về true nếu header X-Vai-Tro == "QuanTri".
        /// </summary>
        public static bool LaQuanTri(HttpContext ctx)
        {
            return LayVaiTro(ctx) == "QuanTri";
        }

        /// <summary>
        /// Trả về 403 Forbidden kèm thông báo nếu không phải QuanTri.
        /// Dùng trong các API cần quyền quản trị.
        /// </summary>
        /// <returns>
        /// null nếu là QuanTri (cho phép đi tiếp);
        /// otherwise IResult 403 để trả về client ngay lập tức.
        /// </returns>
        public static IResult? YeuCauQuanTri(HttpContext ctx)
        {
            if (LaQuanTri(ctx)) return null;   // Đủ quyền → cho đi
            // Không đủ quyền → trả 403
            return Results.Json(
                new { thongBao = "⛔ Bạn không có quyền thực hiện thao tác này (chỉ Quản trị viên)!" },
                statusCode: StatusCodes.Status403Forbidden);
        }
    }
}
