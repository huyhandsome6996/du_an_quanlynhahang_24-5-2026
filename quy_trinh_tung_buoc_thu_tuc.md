# 🗺️ HƯỚNG DẪN LÀM LẠI DỰ ÁN TỪ ĐẦU (STEP-BY-STEP)

Tài liệu này được soạn ra để nhóm của bạn có thể cùng nhau ngồi xem, làm theo từng bước (copy/paste code) và tự tay gõ lệnh Git để hiểu rõ luồng công việc từ lúc chưa có gì đến khi hoàn thành dự án. Quy trình này bám sát vào bảng Trello (Epic/User Story/Task) mà chúng ta đã lập.

---

## 🟢 EPIC 1: XÂY DỰNG NỀN TẢNG & CƠ SỞ DỮ LIỆU

### User Story 1: Thiết lập cấu trúc lõi
Mục tiêu: Tạo bộ khung C#, cài SQLite và tạo các file cấu trúc OOP cơ bản.

**Bước 1: Tạo dự án C# mới**
- Mở Terminal (Command Prompt / PowerShell) tại thư mục mà bạn muốn lưu dự án.
- Gõ lệnh để tạo bộ khung:
  ```bash
  dotnet new web -n QuanLyNhaHang --no-restore -o ./QuanLyNhaHang
  ```
- Di chuyển vào thư mục vừa tạo:
  ```bash
  cd QuanLyNhaHang
  ```
- Cài đặt thư viện SQLite:
  ```bash
  dotnet add package Microsoft.Data.Sqlite --version 8.0.0
  ```

**Bước 2: Tạo tầng Thực thể (Entities)**
- Mở thư mục `QuanLyNhaHang` bằng VS Code (lệnh: `code .`).
- Tạo thủ công một thư mục tên là `Entities`.
- Trong `Entities`, tạo thủ công các file C#: `Ban.cs`, `SanPham.cs`, `ThucAn.cs`, `NuocUong.cs`, `HoaDon.cs`, `ChiTietHoaDon.cs`.
- *Viết code:* Dán nội dung code OOP (Kế thừa, Đóng gói) vào các file này (Bạn có thể copy từ code hiện tại của dự án).

**Bước 3: Tạo tầng Database (DAL) và khởi tạo CSDL**
- Tạo thủ công một thư mục tên là `DAL`.
- Trong `DAL`, tạo file `DatabaseHelper.cs`.
- *Viết code:* Dán code chứa các lệnh SQL `CREATE TABLE...` để phần mềm tự sinh ra file `nha_hang.db`.

**Bước 4: Lưu lịch sử phiên bản (Git Commit)**
- Mở Terminal của VS Code, gõ lần lượt:
  ```bash
  git add .
  git commit -m "Epic 1 - User Story 1: Thiet lap cau truc loi C# va SQLite"
  ```

---

## 🟢 EPIC 2: CÁC TÍNH NĂNG QUẢN LÝ CỐT LÕI

### User Story 2 & 3: Quản lý sơ đồ bàn và thực đơn
Mục tiêu: Làm chức năng Thêm/Sửa/Xóa (CRUD) cho Bàn và Sản phẩm, nối từ Database lên Giao diện Web.

**Bước 1: Hoàn thiện tầng DAL (Kết nối CSDL)**
- Tạo thư mục con `Interfaces` bên trong `DAL`.
- Tạo các file: `IBanDAL.cs`, `ISanPhamDAL.cs`.
- Trong thư mục `DAL`, tạo các file thực thi: `BanDAL.cs`, `SanPhamDAL.cs`.
- *Viết code:* Dán code chứa các câu lệnh `SELECT, INSERT, UPDATE, DELETE` vào các file DAL này.

**Bước 2: Xây dựng API trong Program.cs**
- Mở file `Program.cs`.
- *Viết code:* Đăng ký các DAL (`builder.Services.AddSingleton...`). Thêm các đoạn `app.MapGet`, `app.MapPost` để tạo đường dẫn API cho Bàn và Sản phẩm. Đừng quên gọi `DatabaseHelper.KhoiTaoCSDL()`.

**Bước 3: Thiết kế Giao diện Mặt tiền (Frontend)**
- Tạo thư mục `wwwroot` nằm ngang hàng với thư mục `DAL`.
- Tạo các file HTML thủ công: `index.html` (Sơ đồ bàn), `menu.html` (Thực đơn).
- Tạo thư mục `wwwroot/js`, thêm `ban.js`, `menu.js`.
- Tạo thư mục `wwwroot/css`, thêm `style.css`.
- *Viết code:* Dán code giao diện và Javascript gọi API (bằng `fetch`) vào.

**Bước 4: Lưu phiên bản (Git Commit)**
- Chạy thử để kiểm tra: `dotnet run` -> Mở link `http://localhost:5000`.
- Nếu ổn, lưu lại:
  ```bash
  git add .
  git commit -m "Epic 2 - User Story 2 & 3: Xay dung tinh nang quan ly Ban va Thuc don"
  ```

---

## 🟢 EPIC 3: NGHIỆP VỤ BÁN HÀNG (POS)

### User Story 4 & 5: Gọi món, Thanh toán và Lịch sử
Mục tiêu: Ghép nối Hóa đơn và Sản phẩm, làm màn hình Order 2 cột và màn hình Lịch sử.

**Bước 1: Bổ sung Hóa đơn vào DAL và API**
- Trong `DAL`, tạo `IHoaDonDAL.cs`, `IChiTietHoaDonDAL.cs` (Interface) và `HoaDonDAL.cs`, `ChiTietHoaDonDAL.cs`.
- Mở `Program.cs`, thêm các API Mở Bàn, Thêm Món, Thanh Toán, Lấy Lịch Sử Hóa Đơn.

**Bước 2: Làm giao diện POS và Lịch sử**
- Trong `wwwroot`, tạo `order.html` và `lichsu.html`.
- Trong `wwwroot/js`, tạo `order.js` và `lichsu.js`.
- *Viết code:* Dán code xử lý chọn món, tính tổng tiền, in danh sách hóa đơn vào đây.

**Bước 3: Lưu phiên bản (Git Commit)**
- Chạy lại app bằng `dotnet run` để test thử việc gọi món.
- Lưu lại:
  ```bash
  git add .
  git commit -m "Epic 3 - User Story 4 & 5: Hoan thien chuc nang Goi mon, Thanh toan va Lich su"
  ```

---

## 🟢 EPIC 4: TRẢI NGHIỆM NGƯỜI DÙNG (UX) & BẢO MẬT

### User Story 6, 7 & 8: Giao diện Vua Sư Tử, Auth Guard, Tài liệu
Mục tiêu: Khoác áo mới cho ứng dụng (Dark/Lavender), thêm bảo mật Đăng nhập và soạn tài liệu.

**Bước 1: Nâng cấp Giao diện & Thêm Form Đăng nhập**
- Tạo thư mục `wwwroot/img` và dán file `logo.png` vào.
- Tạo file `wwwroot/login.html`. Dán code Form tạo tài khoản (sử dụng LocalStorage).
- Chỉnh sửa `index.html`, `menu.html`, `order.html`, `lichsu.html`: 
  - Thêm thẻ `<script>if(!sessionStorage.getItem('vst_logged_in')) window.location.href='login.html';</script>` lên đầu file.
  - Sửa lại các chữ "Bistro Elite" thành "Vua Sư Tử". Đổi CSS.

**Bước 2: Đóng gói Script khởi động (Tuỳ chọn)**
- Ở thư mục gốc (bên ngoài thư mục `QuanLyNhaHang`), tạo file `ChayUngDung.bat`.
- Dán code chạy lệnh khởi động tự động.

**Bước 3: Làm tài liệu nộp thầy**
- Tạo thủ công các file: `README.md`, `nhat_ki_hoat_dong.md`, `trello.md`, `yeu_cau_can_dat_tu_thay.md`, `ghi_tru_cac_file_dc_tao_ra_nhu_nao.md`.
- Copy nội dung tài liệu chuẩn vào.

**Bước 4: Chốt hạ và đẩy lên mạng (Push Git)**
- Hoàn thành toàn bộ dự án, ta gom lại lần cuối:
  ```bash
  git add .
  git commit -m "Epic 4 - User Story 6, 7 & 8: Nang cap UX Vua Su Tu, Dang nhap va Soan tai lieu"
  ```
- Đẩy toàn bộ công sức của nhóm lên GitHub:
  ```bash
  git branch -M main
  git push -u origin main
  ```
*(Lưu ý: Bạn có thể thay đổi tên nhánh main/develop tùy thuộc vào nhánh bạn đang làm việc).*

---
🎉 **CHÚC MỪNG NHÓM BẠN ĐÃ TỰ TAY BUILD XONG DỰ ÁN TỪ A-Z!** 🎉
