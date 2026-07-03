// ============================================================================
// FILE: Ban.cs  —  Lớp "Bàn ăn" (thuộc tầng Entity)
// ============================================================================
//
// VỊ TRÍ FILE NÀY TRONG TOÀN BỘ DỰ ÁN:
//   - Dự án chia thành 3 tầng (kiến trúc 3 lớp):
//       1. Tầng Entity  ← FILE NÀY NẰM Ở ĐÂY (chứa các "đối tượng" thực tế)
//       2. Tầng DAL      ← Thao tác với cơ sở dữ liệu (folder DAL/)
//       3. Tầng GUI      ← Giao diện người dùng (folder wwwroot/)
//   - Tầng Entity là nơi định nghĩa "các vật dụng trong nhà hàng" dưới dạng
//     code C#. Mỗi lớp ở đây = 1 bảng trong file Access.
//
// VAI TRÒ CỦA LỚP Ban:
//   - Đại diện cho 1 chiếc bàn ăn trong nhà hàng.
//   - Khi user mở app, hệ thống sẽ tải danh sách bàn từ file Access → tạo ra
//     nhiều đối tượng Ban (mỗi bàn 1 đối tượng) → hiển thị lên giao diện.
//   - Mỗi bàn có 3 trạng thái:
//       "Trống"     → Bàn chưa có khách, nhân viên có thể cho khách ngồi
//       "Đã đặt"    → Khách gọi điện đặt trước, chưa tới nơi
//       "Có khách"  → Đang có khách ngồi, đã mở hóa đơn
//
// LỚP NÀY ĐƠN GIẢN VÌ:
//   - Bàn chỉ có 3 thông tin: mã số (Id), tên (TenBan), trạng thái (TrangThai)
//   - Không có logic phức tạp (không tính tiền, không đa hình...)
//   - Lớp này chủ yếu để "chứa dữ liệu" chứ không "xử lý dữ liệu"
// ============================================================================


// TỪ KHÓA "namespace" — khai báo "khu vực tên" để nhóm các lớp liên quan lại
// với nhau. Tránh xung đột tên với các thư viện ngoài.
// - "QuanLyNhaHang" là tên dự án
// - "Entities" là tên folder chứa các lớp thực thể
// → namespace đầy đủ: QuanLyNhaHang.Entities
// Khi file khác muốn dùng lớp Ban, phải viết: using QuanLyNhaHang.Entities;
namespace QuanLyNhaHang.Entities
{
    // ========================================================================
    // TỪ KHÓA "public class Ban" — khai báo 1 lớp (class) tên "Ban"
    // ========================================================================
    // - "public"  : công khai — file khác được phép dùng lớp này
    // - "class"   : từ khóa khai báo lớp (kiểu dữ liệu do người dùng tự định nghĩa)
    // - "Ban"     : tên lớp (viết hoa chữ đầu theo quy ước C#)
    //
    // Lớp (class) là "bản thiết kế" để tạo ra các đối tượng (object).
    // Ví dụ: lớp Ban là bản thiết kế bàn → từ 1 lớp có thể tạo ra nhiều bàn:
    //   Ban bàn1 = new Ban();  → tạo bàn 1
    //   Ban bàn2 = new Ban();  → tạo bàn 2
    // Mỗi đối tượng (bàn1, bàn2...) có dữ liệu riêng (Id, TenBan, TrangThai).
    // ========================================================================

    /// <summary>
    /// LỚP Ban — đại diện cho 1 chiếc bàn trong nhà hàng.
    /// Quản lý trạng thái để biết bàn nào đang trống / đã đặt / có khách.
    /// (Tag XML này dùng để VS Code hiển thị gợi ý khi rê chuột vào tên lớp)
    /// </summary>
    public class Ban
    {
        // ====================================================================
        // THUỘC TÍNH 1: Id  — Mã số của bàn (khóa chính trong file Access)
        // ====================================================================
        // Phân tích từng từ:
        //   "public"  : ai cũng truy cập được (từ file khác)
        //   "int"     : kiểu số nguyên (integer) — chỉ chứa số: 1, 2, 3...
        //               Không chứa chữ cái hay dấu thập phân
        //   "Id"      : tên biến (viết hoa chữ đầu theo quy ước property C#)
        //   "{ get; set; }" — đây là "property" (thuộc tính) tự động:
        //       get: cho phép ĐỌC giá trị (vd: int x = ban.Id;)
        //       set: cho phép GHI giá trị (vd: ban.Id = 5;)
        //
        // Tại sao dùng int?
        //   - Trong file Access, cột Id là AUTOINCREMENT (tự tăng: 1, 2, 3...)
        //   - Số nguyên phù hợp với kiểu dữ liệu này
        //
        // Ý nghĩa:
        //   - Mỗi bàn có 1 mã số duy nhất, không trùng nhau
        //   - Khi thêm bàn mới, Access tự động gán Id tiếp theo (không cần tự nhập)
        // ====================================================================
        public int Id { get; set; }


        // ====================================================================
        // THUỘC TÍNH 2: TenBan  — Tên hiển thị của bàn
        // ====================================================================
        // Phân tích:
        //   "public"           : công khai
        //   "string"           : kiểu chuỗi văn bản (vd: "Bàn 1", "Bàn VIP")
        //   "TenBan"           : tên property
        //   "{ get; set; }"    : đọc/ghi được
        //   "= string.Empty"   : gán giá trị mặc định là chuỗi rỗng ""
        //
        // Tại sao gán "= string.Empty"?
        //   - Tránh lỗi "NullReferenceException" khi dùng TenBan mà chưa gán giá trị
        //   - string.Empty tương đương "" — an toàn hơn null
        //
        // Cách dùng:
        //   Ban b = new Ban();
        //   b.TenBan = "Bàn 1";        // gán tên
        //   string ten = b.TenBan;     // đọc tên
        // ====================================================================
        public string TenBan { get; set; } = string.Empty;


        // ====================================================================
        // THUỘC TÍNH 3: TrangThai  — Trạng thái hiện tại của bàn
        // ====================================================================
        // Phân tích:
        //   "public"           : công khai
        //   "string"           : kiểu chuỗi
        //   "TrangThai"        : tên property
        //   "{ get; set; }"    : đọc/ghi được
        //   "= \"Trống\""      : giá trị mặc định khi tạo bàn mới là "Trống"
        //
        // 3 giá trị có thể có:
        //   "Trống"     → Bàn chưa có khách (mặc định khi tạo)
        //   "Đã đặt"    → Khách đặt trước qua điện thoại
        //   "Có khách"  → Đang phục vụ, đã có hóa đơn mở
        //
        // Tại sao dùng string thay vì enum?
        //   - Đồ án đơn giản, dùng string dễ so sánh trực tiếp:
        //       if (ban.TrangThai == "Trống") { ... }
        //   - Tránh phức tạp khi lưu/đọc từ file Access
        //   - Trong dự án lớn nên dùng enum để tránh lỗi chính tả
        //
        // Lưu ý: giá trị mặc định "Trống" — khi tạo bàn mới,
        //        bàn sẽ tự động ở trạng thái "Trống" (chưa có khách)
        // ====================================================================
        public string TrangThai { get; set; } = "Trống";
    }
}
