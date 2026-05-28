# DỰ ÁN QUẢN LÝ NHÀ HÀNG - VUA SƯ TỬ 🦁

> **Mô tả:** Đây là hệ thống phần mềm quản lý nhà hàng hiện đại, được xây dựng bằng C# (Backend) kết nối với cơ sở dữ liệu SQLite và giao diện (Frontend) viết bằng HTML/CSS/JS thuần theo phong cách Glassmorphism sang trọng.

---

## 📂 CẤU TRÚC CÂY THƯ MỤC VÀ Ý NGHĨA TỪNG FILE
*(Giải thích cặn kẽ chi tiết - Rất dễ hiểu)*

Bạn hãy tưởng tượng dự án này như một **Nhà Hàng ngoài đời thực**. Mỗi thư mục đóng một vai trò cụ thể:

```text
du_an_quanlynhahang_24-5-2026/
│
├── QuanLyNhaHang/                   <-- Thư mục gốc chứa toàn bộ mã nguồn của phần mềm
│   │
│   ├── Entities/                    <-- KHU VỰC "BẢN VẼ" (Lớp thực thể)
│   │   ├── Ban.cs                   (Bản vẽ cấu tạo của 1 cái Bàn)
│   │   ├── SanPham.cs               (Bản vẽ chung của Sản Phẩm - Món ăn/Nước uống)
│   │   ├── ThucAn.cs                (Món ăn - Kế thừa từ Sản Phẩm)
│   │   ├── NuocUong.cs              (Thức uống - Kế thừa từ Sản Phẩm)
│   │   ├── HoaDon.cs                (Bản vẽ cấu tạo của tờ Hóa đơn tính tiền)
│   │   └── ChiTietHoaDon.cs         (Bản vẽ cấu tạo của 1 dòng ghi món trên hóa đơn)
│   │
│   ├── DAL/                         <-- KHU VỰC "KHO LƯU TRỮ" (Data Access Layer)
│   │   ├── DatabaseHelper.cs        (Người thợ xây: Tự động tạo file nha_hang.db và xây các bảng)
│   │   ├── BanDAL.cs                (Thủ kho chuyên quản lý việc Thêm/Sửa/Xóa dữ liệu Bàn)
│   │   ├── SanPhamDAL.cs            (Thủ kho chuyên quản lý dữ liệu Thực đơn)
│   │   ├── HoaDonDAL.cs             (Thủ kho chuyên lưu trữ Hóa đơn)
│   │   └── Interfaces/              (Bảng nội quy quy định các thủ kho phải làm gì)
│   │
│   ├── wwwroot/                     <-- KHU VỰC "MẶT TIỀN / SẢNH KHÁCH" (Giao diện)
│   │   ├── login.html               (Cánh cửa bảo vệ: Form tạo mật khẩu và đăng nhập)
│   │   ├── index.html               (Sảnh chính: Sơ đồ các Bàn)
│   │   ├── menu.html                (Cuốn menu: Nơi quản lý món ăn)
│   │   ├── order.html               (Quầy phục vụ: Nơi bấm chọn món và tính tiền)
│   │   ├── lichsu.html              (Sổ ghi chép: Xem lại các hóa đơn đã thanh toán)
│   │   ├── css/                     (Tủ quần áo: Chứa file style.css để làm đẹp giao diện)
│   │   ├── img/                     (Nơi treo tranh ảnh: Chứa logo.png con sư tử)
│   │   └── js/                      (Kịch bản hành động: Chứa ban.js, menu.js quy định nút bấm)
│   │
│   └── Program.cs                   <-- "NGƯỜI QUẢN LÝ CHUNG" (Trái tim của phần mềm)
│                                    (File chạy đầu tiên. Khởi động CSDL, mở server và kết nối Giao diện với Kho)
│
├── README.md                        <-- CUỐN SÁCH HƯỚNG DẪN SỬ DỤNG (Chính là file bạn đang đọc)
├── nhat_ki_hoat_dong.md             <-- SỔ GHI CHÉP QUÁ TRÌNH LÀM DỰ ÁN
└── yeu_cau_can_dat_tu_thay.md       <-- BÁO CÁO ĐỐI CHIẾU YÊU CẦU GIẢNG VIÊN
```

---

## 🚀 HƯỚNG DẪN CÁCH CHẠY PHẦN MỀM

Vì đây là dự án C#, cách chạy vô cùng đơn giản:

1. Mở màn hình **Terminal (Command Prompt / PowerShell)**.
2. Dùng lệnh `cd QuanLyNhaHang` để đi vào thư mục gốc của code.
3. Gõ lệnh `dotnet run` và ấn Enter.
4. Mở trình duyệt web (Chrome, Edge, Cốc Cốc) và truy cập vào địa chỉ: `http://localhost:5000`
5. Lần đầu tiên vào, hệ thống sẽ yêu cầu bạn **Tạo một tài khoản Quản trị**. Sau đó đăng nhập bằng tài khoản vừa tạo để sử dụng!

---

*Đồ án Môn học Lập trình Hướng đối tượng (OOP)*
*Phiên bản: 1.0 (Vua Sư Tử)*