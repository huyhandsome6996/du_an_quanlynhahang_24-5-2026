# ⚠️ CÁC LỖI THƯỜNG GẶP VÀ CÁCH KHẮC PHỤC TRONG DỰ ÁN

Tài liệu này ghi lại các lỗi kỹ thuật thường gặp trong quá trình phát triển và vận hành dự án **Quản Lý Nhà Hàng (mô hình Desktop App kết hợp Web API)**, cùng hướng dẫn từng bước để xử lý nhanh chóng.

---

## 💾 1. Lỗi Cấu Trúc Cơ Sở Dữ Liệu SQLite Không Tự Cập Nhật (Schema Out-of-Date)

### 📌 Hiện Tượng
Khi thêm một tính năng mới yêu cầu thêm trường dữ liệu (ví dụ: trường `HinhAnh` cho món ăn), mã nguồn đã được cập nhật thuộc tính nhưng khi chạy ứng dụng vẫn gặp lỗi không tìm thấy cột trong database, hoặc không lưu được dữ liệu mới.

### 🔍 Nguyên Nhân
* CSDL SQLite được cấu hình tự động tạo bảng thông qua câu lệnh `CREATE TABLE IF NOT EXISTS`. Lệnh này **chỉ chạy khi bảng chưa tồn tại**.
* Nếu file cơ sở dữ liệu `nha_hang.db` đã được tạo từ trước, SQLite sẽ bỏ qua khối lệnh khởi tạo bảng, dẫn đến việc thiếu các cột mới được bổ sung sau này.

### 🛠️ Cách Khắc Phục
* **Cách 1: Chạy lệnh nâng cấp cấu trúc bảng (ALTER TABLE) tự động:**
  Trong file `DAL/DatabaseHelper.cs`, chúng ta viết thêm khối lệnh chạy thử `ALTER TABLE` trong khối `try-catch` khi khởi động:
  ```csharp
  try
  {
      using var alterColCmd = new SqliteCommand("ALTER TABLE SanPham ADD COLUMN HinhAnh TEXT NULL;", conn);
      alterColCmd.ExecuteNonQuery();
  }
  catch { /* Cột đã tồn tại hoặc bảng mới đã có sẵn, bỏ qua lỗi */ }
  ```
* **Cách 2: Làm mới hoàn toàn CSDL (Áp dụng khi đang test dự án):**
  Xóa file cơ sở dữ liệu cũ `nha_hang.db` nằm ở thư mục gốc của project chạy. Khi bạn chạy lại lệnh `dotnet run`, ứng dụng sẽ tự động sinh lại file CSDL mới từ đầu với cấu trúc bảng hoàn hảo nhất.

---

## 🌐 2. Lỗi Giao Diện Desktop App Không Cập Nhật Thay Đổi (WebView2 Cache Issue)

### 📌 Hiện Tượng
Bạn chỉnh sửa mã nguồn HTML, CSS hoặc các file Javascript (`menu.js`, `order.js`...) và giao diện trên trình duyệt Web đã hiển thị đúng, nhưng khi chạy ứng dụng Desktop qua lệnh `dotnet run` thì giao diện app Desktop vẫn giữ nguyên phiên bản cũ, thiếu các nút bấm, trường nhập liệu hoặc tính năng mới.

### 🔍 Nguyên Nhân
* Ứng dụng Desktop sử dụng thành phần **WebView2 (Chromium)** để hiển thị giao diện.
* WebView2 có cơ chế lưu bộ nhớ đệm (Cache) rất chặt chẽ để tối ưu hiệu năng. Thư mục dữ liệu người dùng (`QuanLyNhaHang.exe.WebView2`) sẽ lưu trữ toàn bộ file tĩnh từ lần chạy đầu tiên. Khi bạn sửa file JS/HTML, WebView2 vẫn ưu tiên đọc file cũ từ cache thay vì tải file mới từ server Web API.

### 🛠️ Cách Khắc Phục

#### Bước 1: Sử dụng kỹ thuật Cache-Busting (Khuyên dùng)
Tại các file HTML (`index.html`, `menu.html`, `order.html`, `lichsu.html`), khi nhúng các file kịch bản JS hoặc CSS, hãy thêm tham số phiên bản `?v=...` phía sau tên file:
```html
<!-- Trước khi sửa: Dễ bị lưu cache -->
<script src="js/menu.js"></script>

<!-- Sau khi sửa: Bắt buộc WebView2 tải bản mới nhất -->
<script src="js/menu.js?v=1.2"></script>
```

#### Bước 2: Xóa thư mục Cache của WebView2
Nếu phiên bản thay đổi quá nhiều và cần làm sạch hoàn toàn trạng thái:
1. Đóng ứng dụng Desktop đang chạy.
2. Truy cập vào thư mục đầu ra của dự án (mặc định là `QuanLyNhaHang/bin/Debug/net10.0-windows/`).
3. Tìm và xóa thư mục **`QuanLyNhaHang.exe.WebView2`**.
4. Chạy lại lệnh `dotnet run` để tạo mới hoàn toàn môi trường hiển thị.

*Lưu ý nhanh:* Bạn cũng có thể dùng lệnh PowerShell sau để tắt tiến trình WebView2 bị kẹt và xóa thư mục này nhanh chóng:
```powershell
taskkill /F /IM msedgewebview2.exe; taskkill /F /IM QuanLyNhaHang.exe
Remove-Item -Recurse -Force QuanLyNhaHang\bin\Debug\net10.0-windows\QuanLyNhaHang.exe.WebView2
```

---

## 🔌 3. Lỗi Trùng Cổng Mạng (Port 5000 in Use)

### 📌 Hiện Tượng
Khi chạy lệnh `dotnet run`, chương trình báo lỗi crash và xuất hiện thông báo đại loại như `Failed to bind to address http://localhost:5000: address already in use`.

### 🔍 Nguyên Nhân
Ứng dụng Web API chạy ngầm ở cổng `5000` của bạn chưa được giải phóng hoặc đang bị chiếm dụng bởi một tiến trình chạy lỗi trước đó.

### 🛠️ Cách Khắc Phục
* **Trong Windows (PowerShell):** Tìm ID của tiến trình đang chiếm cổng 5000 và dừng nó bằng các lệnh sau:
  ```powershell
  # Tìm PID của tiến trình sử dụng cổng 5000
  netstat -ano | findstr :5000
  
  # Kết quả trả về dòng cuối cùng có dạng: TCP 127.0.0.1:5000 ... LISTENING 12264 (với 12264 là PID)
  # Dừng tiến trình đó bằng lệnh:
  taskkill /F /PID 12264
  ```
