// ============================================================
// TẠO FILE ACCESS (.accdb) CHO ĐỒ ÁN QUẢN LÝ NHÀ HÀNG
// Môn: Lập trình hướng đối tượng
// Đề tài: Hệ thống Quản lý Nhà hàng
//
// SỬ DỤNG THƯ VIỆN JACKCESS 5.1.0 (Java) — không cần MS Access cài đặt
// Tạo file .accdb (Access 2016 format) với:
//   - 5 bảng: NguoiDung, Ban, SanPham, HoaDon, ChiTietHoaDon
//   - Khóa chính + khóa ngoại + index
//   - Dữ liệu mẫu (3 user, 10 bàn, 12 sản phẩm, 5 hóa đơn, 14 chi tiết)
//
// ⚠️ PHIÊN BẢN NÀY LƯU MẬT KHẨU PLAIN-TEXT (KHÔNG BĂM SHA-256)
//    Lý do: Đồ án nhỏ, học sinh cần tự thêm/sửa tài khoản trực tiếp
//    trong Access nên dùng plain text cho dễ nhìn và dễ quản lý.
// ============================================================
import io.github.spannm.jackcess.*;       // Thư viện Jackcess để tạo file Access
import io.github.spannm.jackcess.impl.*;  // Các class nội bộ của Jackcess
import java.io.File;                      // Class File để thao tác file
import java.util.*;                       // Calendar, Date, List, Map...

public class tao_access {

    static Database db;                   // Biến static giữ kết nối tới file Access đang tạo

    public static void main(String[] args) throws Exception {
        // File output — sẽ được tạo mới (overwrite nếu đã có)
        File outFile = new File("QuanLyNhaHang.accdb");
        if (outFile.exists()) outFile.delete();   // Xoá file cũ để tạo lại từ đầu

        System.out.println("[1/6] Đang tạo file Access mới: " + outFile.getName());
        // Tạo database mới định dạng V2016 (Access 2016/2019/365 đều mở được)
        db = DatabaseBuilder.create(Database.FileFormat.V2016, outFile);

        // ====== 1. Bảng NguoiDung (người dùng / tài khoản đăng nhập) ======
        System.out.println("[2/6] Tạo bảng NguoiDung...");
        Table tblNguoiDung = new TableBuilder("NguoiDung")
            .addColumn(new ColumnBuilder("Id", DataType.LONG).withAutoNumber(true))   // Id tự tăng
            .addColumn(new ColumnBuilder("TenDangNhap", DataType.TEXT).withLength(50*2)) // Tên đăng nhập (50 ký tự Unicode)
            .addColumn(new ColumnBuilder("MatKhau", DataType.TEXT).withLength(100*2))  // Mật khẩu PLAIN-TEXT (đơn giản hoá cho học sinh)
            .addColumn(new ColumnBuilder("VaiTro", DataType.TEXT).withLength(20*2))    // VaiTro: "QuanTri" hoặc "NhanVien"
            .addColumn(new ColumnBuilder("NgayTao", DataType.SHORT_DATE_TIME))        // Ngày tạo tài khoản
            .addIndex(new IndexBuilder("PK_NguoiDung").withPrimaryKey().withColumns("Id"))            // Khóa chính
            .addIndex(new IndexBuilder("UK_TenDangNhap").withUnique().withColumns("TenDangNhap"))     // Unique: không trùng tên đăng nhập
            .toTable(db);

        // ====== 2. Bảng Ban (bàn ăn trong nhà hàng) ======
        System.out.println("[3/6] Tạo bảng Ban...");
        Table tblBan = new TableBuilder("Ban")
            .addColumn(new ColumnBuilder("Id", DataType.LONG).withAutoNumber(true))     // Id tự tăng
            .addColumn(new ColumnBuilder("TenBan", DataType.TEXT).withLength(20*2))     // Tên bàn: "Bàn 1", "Bàn 2"...
            .addColumn(new ColumnBuilder("TrangThai", DataType.TEXT).withLength(20*2))  // Trạng thái: "Trống" / "Có khách" / "Đã đặt"
            .addIndex(new IndexBuilder("PK_Ban").withPrimaryKey().withColumns("Id"))    // Khóa chính
            .toTable(db);

        // ====== 3. Bảng SanPham (sản phẩm / món ăn & nước uống) ======
        System.out.println("[4/6] Tạo bảng SanPham...");
        Table tblSanPham = new TableBuilder("SanPham")
            .addColumn(new ColumnBuilder("Id", DataType.LONG).withAutoNumber(true))     // Id tự tăng
            .addColumn(new ColumnBuilder("TenSanPham", DataType.TEXT).withLength(100*2))// Tên món: "Cơm gà xối mỡ"...
            .addColumn(new ColumnBuilder("GiaCoBan", DataType.MONEY))                   // Giá cơ bản (kiểu tiền tệ)
            .addColumn(new ColumnBuilder("Loai", DataType.TEXT).withLength(20*2))       // Loại: "ThucAn" hoặc "NuocUong" (đa hình OOP)
            .addColumn(new ColumnBuilder("DangBan", DataType.BOOLEAN))                  // Còn bán hay không (true/false)
            .addColumn(new ColumnBuilder("HinhAnh", DataType.TEXT).withLength(255*2))   // Ảnh base64 hoặc URL
            .addIndex(new IndexBuilder("PK_SanPham").withPrimaryKey().withColumns("Id"))// Khóa chính
            .toTable(db);

        // ====== 4. Bảng HoaDon (hóa đơn của bàn) ======
        System.out.println("[5/6] Tạo bảng HoaDon...");
        Table tblHoaDon = new TableBuilder("HoaDon")
            .addColumn(new ColumnBuilder("Id", DataType.LONG).withAutoNumber(true))                  // Id tự tăng
            .addColumn(new ColumnBuilder("BanId", DataType.LONG))                                    // FK → Ban.Id
            .addColumn(new ColumnBuilder("ThoiGianTao", DataType.SHORT_DATE_TIME))                   // Thời gian mở bàn
            .addColumn(new ColumnBuilder("ThoiGianThanhToan", DataType.SHORT_DATE_TIME))             // Thời gian thanh toán (null nếu chưa)
            .addColumn(new ColumnBuilder("TongTien", DataType.MONEY))                                // Tổng tiền món
            .addColumn(new ColumnBuilder("TrangThai", DataType.TEXT).withLength(30*2))               // "Chưa thanh toán" / "Đã thanh toán"
            .addColumn(new ColumnBuilder("VAT", DataType.MONEY))                                     // Tiền VAT (10%)
            .addColumn(new ColumnBuilder("GiamGia", DataType.MONEY))                                 // Tiền giảm giá
            .addColumn(new ColumnBuilder("PhuongThucThanhToan", DataType.TEXT).withLength(20*2))     // "TienMat" / "The" / "QR" / "ChuyenKhoan"
            .addIndex(new IndexBuilder("PK_HoaDon").withPrimaryKey().withColumns("Id"))              // Khóa chính
            .addIndex(new IndexBuilder("FK_HoaDon_Ban").withColumns("BanId"))                        // Index cho FK BanId
            .toTable(db);

        // ====== 5. Bảng ChiTietHoaDon (từng món trong 1 hóa đơn) ======
        System.out.println("[6/6] Tạo bảng ChiTietHoaDon...");
        Table tblCTHD = new TableBuilder("ChiTietHoaDon")
            .addColumn(new ColumnBuilder("Id", DataType.LONG).withAutoNumber(true))                  // Id tự tăng
            .addColumn(new ColumnBuilder("HoaDonId", DataType.LONG))                                 // FK → HoaDon.Id
            .addColumn(new ColumnBuilder("SanPhamId", DataType.LONG))                                // FK → SanPham.Id
            .addColumn(new ColumnBuilder("SoLuong", DataType.INT))                                   // Số lượng gọi
            .addColumn(new ColumnBuilder("DonGiaBan", DataType.MONEY))                               // Đơn giá đã tính phụ phí (Phần lớn/Lon)
            .addColumn(new ColumnBuilder("ThuocTinhThem", DataType.TEXT).withLength(100*2))          // Ghi chú: "Phần lớn", "Lon", "Không hành"...
            .addColumn(new ColumnBuilder("ThanhTien", DataType.MONEY))                               // DonGiaBan × SoLuong
            .addColumn(new ColumnBuilder("TrangThaiMon", DataType.TEXT).withLength(30*2))            // "DangCho" / "DangChuanBi" / "DaPhucVu"
            .addIndex(new IndexBuilder("PK_ChiTietHoaDon").withPrimaryKey().withColumns("Id"))       // Khóa chính
            .addIndex(new IndexBuilder("FK_CTHD_HoaDon").withColumns("HoaDonId"))                    // Index cho FK HoaDonId
            .addIndex(new IndexBuilder("FK_CTHD_SanPham").withColumns("SanPhamId"))                  // Index cho FK SanPhamId
            .toTable(db);

        // ====== Thêm quan hệ khóa ngoại (có cascade update + delete) ======
        System.out.println("\nĐang thêm quan hệ khóa ngoại...");
        addRelationship("HoaDon_Ban", "Ban", "Id", "HoaDon", "BanId");          // 1 Ban — N HoaDon
        addRelationship("CTHD_HoaDon", "HoaDon", "Id", "ChiTietHoaDon", "HoaDonId");    // 1 HoaDon — N ChiTietHoaDon
        addRelationship("CTHD_SanPham", "SanPham", "Id", "ChiTietHoaDon", "SanPhamId"); // 1 SanPham — N ChiTietHoaDon

        // ====== DỮ LIỆU MẪU (cố ý để học sinh thấy rõ) ======
        System.out.println("\nĐang chèn dữ liệu mẫu...");

        // ---- 3 NguoiDung (mật khẩu PLAIN-TEXT, không băm) ----
        Date now = new Date();   // Thời điểm hiện tại cho NgayTao
        // admin / admin123 — Tài khoản quản trị chính
        tblNguoiDung.addRow(1, "admin", "admin123", "QuanTri", now);
        // nhanvien1 / 123456 — Tài khoản nhân viên phục vụ
        tblNguoiDung.addRow(2, "nhanvien1", "123456", "NhanVien", now);
        // huy / huy123456 — Tài khoản quản trị của sinh viên Hồ Quang Huy
        tblNguoiDung.addRow(3, "huy", "huy123456", "QuanTri", now);

        // ---- 10 Bàn ----
        String[] tenBan = {"Bàn 1","Bàn 2","Bàn 3","Bàn 4","Bàn 5","Bàn 6","Bàn 7","Bàn 8","Bàn 9","Bàn 10"};
        // Trạng thái: Bàn 2, 4, 8 đang có khách (để test chức năng thanh toán ngay)
        String[] trangThai = {"Trống","Có khách","Trống","Có khách","Trống","Trống","Trống","Có khách","Trống","Trống"};
        for (int i = 0; i < 10; i++) {
            tblBan.addRow(i + 1, tenBan[i], trangThai[i]);
        }

        // ---- 12 Sản phẩm (6 ThucAn + 6 NuocUong) ----
        // Mỗi dòng: {Tên, Giá, Loại, Đang bán, Đường dẫn ảnh}
        Object[][] sanpham = {
            {"Cơm gà xối mỡ", 45000.0, "ThucAn",   true,  "/img/com_ga.jpg"},
            {"Mỳ Quảng",       40000.0, "ThucAn",   true,  "/img/my_quang.jpg"},
            {"Bún bò Huế",     50000.0, "ThucAn",   true,  "/img/bun_bo.jpg"},
            {"Phở bò",         55000.0, "ThucAn",   true,  "/img/pho_bo.jpg"},
            {"Cơm sườn nướng", 60000.0, "ThucAn",   true,  "/img/com_suon.jpg"},
            {"Gỏi cuốn tôm",   35000.0, "ThucAn",   false, "/img/goi_cuon.jpg"},   // Đang ngừng bán
            {"Cà phê sữa",     25000.0, "NuocUong", true,  "/img/ca_phe_sua.jpg"},
            {"Trà sữa trân châu", 45000.0, "NuocUong", true,  "/img/tra_sua.jpg"},
            {"Nước cam ép",    35000.0, "NuocUong", true,  "/img/nuoc_cam.jpg"},
            {"Coca Cola",      15000.0, "NuocUong", true,  "/img/coca.jpg"},
            {"Trà đá",         10000.0, "NuocUong", true,  "/img/tra_da.jpg"},
            {"Sinh tố xoài",   40000.0, "NuocUong", false, "/img/sinh_to_xoai.jpg"}  // Đang ngừng bán
        };
        for (int i = 0; i < sanpham.length; i++) {
            tblSanPham.addRow(i + 1,
                sanpham[i][0],   // TenSanPham
                sanpham[i][1],   // GiaCoBan
                sanpham[i][2],   // Loai
                sanpham[i][3],   // DangBan
                sanpham[i][4]);  // HinhAnh
        }

        // ---- 5 Hóa đơn mẫu (đã thanh toán) — để test trang Báo cáo ----
        Calendar cal = Calendar.getInstance();
        cal.set(2026, Calendar.JUNE, 15, 11, 30, 0);  Date t1 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 15, 12, 0, 0);   Date t2 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 16, 18, 0, 0);   Date t3 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 16, 19, 30, 0);  Date t4 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 17, 19, 0, 0);   Date t5 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 17, 20, 0, 0);   Date t6 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 18, 12, 0, 0);   Date t7 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 18, 13, 0, 0);   Date t8 = cal.getTime();

        // {Id, BanId, ThoiGianTao, ThoiGianTT, TongTien, TrangThai, VAT, GiamGia, PTTT}
        Object[][] hoadon = {
            {1, 1, t1, t2, 118000.0, "Đã thanh toán", 10727.0, 0.0, "TienMat"},
            {2, 2, t3, t4, 125000.0, "Đã thanh toán", 11364.0, 0.0, "The"},
            {3, 4, t5, t6, 155000.0, "Đã thanh toán", 14091.0, 0.0, "QR"},
            {4, 8, t7, t8, 125000.0, "Đã thanh toán", 11364.0, 0.0, "TienMat"},
            {5, 3, t1, t3, 115000.0, "Đã thanh toán", 10455.0, 0.0, "ChuyenKhoan"}
        };
        for (Object[] hd : hoadon) {
            tblHoaDon.addRow(hd[0], hd[1], hd[2], hd[3], hd[4], hd[5], hd[6], hd[7], hd[8]);
        }

        // ---- Chi tiết hóa đơn (14 dòng) — mỗi dòng là 1 món trong 1 hóa đơn ----
        // {Id, HoaDonId, SanPhamId, SoLuong, DonGiaBan, ThuocTinhThem, ThanhTien, TrangThaiMon}
        Object[][] cthd = {
            // HD1: Cơm gà (Phần lớn +50k) + Coca Lon (×1.2) + Trà đá
            {1, 1, 1,  1, 95000.0, "Phần lớn", 95000.0, "DaPhucVu"},
            {2, 1, 9,  1, 15000.0, "Lon",      18000.0, "DaPhucVu"},
            {3, 1, 10, 1, 10000.0, "Ly",       10000.0, "DaPhucVu"},
            // HD2: Cà phê + Phở ×2
            {4, 2, 7,  1, 25000.0, "Ly",          25000.0,  "DaPhucVu"},
            {5, 2, 4,  2, 55000.0, "Phần thường", 110000.0, "DaPhucVu"},
            // HD3: Phở + Trà sữa + Coca Lon
            {6, 3, 4,  1, 55000.0, "Phần thường", 55000.0, "DaPhucVu"},
            {7, 3, 8,  1, 45000.0, "Ly",          45000.0, "DaPhucVu"},
            {8, 3, 9,  1, 15000.0, "Lon",         18000.0, "DaPhucVu"},
            // HD4: Cơm gà Phần lớn + Coca Lon + Trà đá
            {9,  4, 1,  1, 95000.0, "Phần lớn", 95000.0, "DaPhucVu"},
            {10, 4, 9,  1, 15000.0, "Lon",      18000.0, "DaPhucVu"},
            {11, 4, 10, 1, 10000.0, "Ly",       10000.0, "DaPhucVu"},
            // HD5: Mỳ Quảng Phần lớn + Nước cam + Trà sữa
            {12, 5, 2, 1, 90000.0, "Phần lớn", 90000.0, "DaPhucVu"},
            {13, 5, 9, 1, 35000.0, "Ly",       35000.0, "DaPhucVu"},
            {14, 5, 8, 1, 45000.0, "Ly",       45000.0, "DaPhucVu"}
        };
        for (Object[] c : cthd) {
            tblCTHD.addRow(c[0], c[1], c[2], c[3], c[4], c[5], c[6], c[7]);
        }

        db.close();   // Đóng DB để flush dữ liệu xuống file
        System.out.println("\n✅ Đã tạo file: " + outFile.getAbsolutePath());
        System.out.println("   Kích thước: " + outFile.length() + " bytes");

        // ====== Verify (mở lại để kiểm tra) ======
        System.out.println("\n=== KIỂM TRA ===");
        db = DatabaseBuilder.open(outFile);
        for (String tn : new String[]{"NguoiDung","Ban","SanPham","HoaDon","ChiTietHoaDon"}) {
            Table t = db.getTable(tn);
            int cnt = 0;
            for (Row r : t) cnt++;    // Đếm số dòng
            System.out.println("   Bảng " + tn + ": " + cnt + " dòng");
        }
        System.out.println("\n   Quan hệ:");
        for (Object rel : db.getRelationships()) {
            System.out.println("     " + rel);
        }
        db.close();
        System.out.println("\n=== HOÀN TẤT ===");
    }

    // Helper: tạo quan hệ khóa ngoại giữa 2 bảng (có cascade update + delete)
    static void addRelationship(String name, String fromTable, String fromCol,
                                String toTable, String toCol) throws Exception {
        try {
            RelationshipBuilder rb = new RelationshipBuilder(fromTable, toTable)
                .withName(name)
                .addColumns(fromCol, toCol)
                .withReferentialIntegrity()   // Bật ràng buộc toàn vẹn
                .withCascadeDeletes()         // Xoá cha → tự xoá con
                .withCascadeUpdates();        // Sửa Id cha → tự sửa Id con
            rb.toRelationship(db);
            System.out.println("   + Quan hệ " + name + ": " + toTable + "." + toCol + " → " + fromTable + "." + fromCol);
        } catch (Exception e) {
            System.out.println("   ! Quan hệ " + name + " thất bại: " + e.getMessage());
        }
    }
}
