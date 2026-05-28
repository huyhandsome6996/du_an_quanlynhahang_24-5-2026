# 📋 KẾ HOẠCH PHÁT TRIỂN DỰ ÁN (TRELLO BOARD)
*Mô phỏng lại quá trình chia nhỏ công việc của dự án Quản Lý Nhà Hàng - Vua Sư Tử theo chuẩn Agile/Scrum.*

---

## 🟢 EPIC 1: Xây Dựng Nền Tảng & Cơ Sở Dữ Liệu
**User Story 1: Thiết lập cấu trúc lõi**
- **Description:** Tôi là Lập trình viên, tôi muốn tạo cấu trúc CSDL SQLite và các thực thể OOP để phần mềm có nơi lưu trữ dữ liệu an toàn và hoạt động theo quy chuẩn 3 tầng.
- **Tasks (Công việc):**
  - [x] Khởi tạo dự án bằng lệnh `dotnet new web`.
  - [x] Cài đặt gói `Microsoft.Data.Sqlite`.
  - [x] Code thư mục `Entities/`: Tạo `Ban.cs`, `SanPham.cs`, `HoaDon.cs` (Áp dụng Đóng gói, Kế thừa, Đa hình).
  - [x] Code `DatabaseHelper.cs`: Viết lệnh SQL tự động sinh file `nha_hang.db` khi app chạy lần đầu.

---

## 🟢 EPIC 2: Các Tính Năng Quản Lý Cốt Lõi
**User Story 2: Quản lý sơ đồ bàn**
- **Description:** Tôi là Nhân viên phục vụ, tôi muốn xem được trạng thái các bàn (Trống/Có khách) thông qua một sơ đồ trực quan để dễ dàng sắp xếp chỗ ngồi cho khách.
- **Tasks (Công việc):**
  - [x] Backend: Viết các hàm Thêm/Xóa/Sửa/Lấy danh sách bàn trong `BanDAL.cs` và `Program.cs`.
  - [x] Frontend: Thiết kế `index.html` giao diện lưới (grid) hiển thị bàn.
  - [x] Logic JS: Viết `ban.js` để gọi API lấy bàn và đổi màu trạng thái bàn.

**User Story 3: Quản lý thực đơn nhà hàng**
- **Description:** Tôi là Quản lý nhà hàng, tôi muốn thêm món ăn mới, sửa giá, xóa món ăn để luôn cập nhật thực đơn theo mùa.
- **Tasks (Công việc):**
  - [x] Backend: Viết API quản lý Sản phẩm (Thức ăn/Nước uống).
  - [x] Frontend: Thiết kế form tạo/sửa món và danh sách món dạng thẻ (Card) tại `menu.html`.
  - [x] Logic JS: Code `menu.js` hiển thị pop-up nhập liệu.

---

## 🟢 EPIC 3: Nghiệp Vụ Bán Hàng (POS)
**User Story 4: Giao diện gọi món & Thanh toán**
- **Description:** Tôi là Nhân viên thu ngân, tôi muốn một giao diện POS (máy tính tiền) tiện lợi để mở bàn, chọn món, tính tiền và in hóa đơn thật nhanh.
- **Tasks (Công việc):**
  - [x] Backend: Cập nhật API Mở Bàn, Thêm Món Vào Hóa Đơn và Thanh Toán Đóng Bàn.
  - [x] Frontend: Xây dựng giao diện `order.html` chia làm 2 cột: Trái (Danh sách món đang bán), Phải (Hóa đơn chi tiết).
  - [x] Logic JS: Code `order.js` cho phép click thêm món, tự cập nhật Tổng tiền.

**User Story 5: Tra cứu lịch sử hóa đơn**
- **Description:** Tôi là Chủ nhà hàng, tôi muốn xem lại danh sách hóa đơn đã thu tiền để kiểm tra doanh thu trong ngày.
- **Tasks (Công việc):**
  - [x] Backend: Viết API lấy lịch sử hóa đơn trong `HoaDonDAL.cs`.
  - [x] Frontend: Tạo trang `lichsu.html` có bảng liệt kê mã hóa đơn, giờ thanh toán, số tiền.
  - [x] UI/UX: Thiết kế pop-up (Modal) khi click vào một hóa đơn thì hiện chi tiết xem bàn đó đã ăn những gì.

---

## 🟢 EPIC 4: Trải Nghiệm Người Dùng (UX) & Bảo Mật
**User Story 6: Nâng cấp giao diện cao cấp**
- **Description:** Tôi là Khách hàng (hoặc Giảng viên), tôi muốn phần mềm trông sang trọng, hiện đại, mang màu sắc chuyên nghiệp để tạo ấn tượng tốt.
- **Tasks (Công việc):**
  - [x] Đổi màu nền thành Dark Mode và màu nhấn thành Tím Oải Hương (Lavender).
  - [x] Tạo hiệu ứng kính mờ (Glass-card) cho mọi bảng biểu.
  - [x] Thay đổi phông chữ thành `Be Vietnam Pro` để gõ Tiếng Việt không bị lỗi.

**User Story 7: Bảo mật Đăng nhập**
- **Description:** Tôi là Quản trị viên, tôi muốn khi mở phần mềm phải đăng nhập thì mới được sử dụng để tránh người lạ phá hoại CSDL.
- **Tasks (Công việc):**
  - [x] Thiết kế giao diện màn hình đăng nhập độc lập (`login.html`) có Logo Vua Sư Tử.
  - [x] Code tính năng Yêu cầu tạo tài khoản (Nếu mở app lần đầu) sử dụng LocalStorage.
  - [x] Code Auth Guard: Tự động đuổi người dùng về màn hình đăng nhập nếu cố tình truy cập vào `index.html` khi chưa có quyền.
