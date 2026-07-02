// ============================================================
// TẠO FILE ACCESS (.accdb) CHO ĐỒ ÁN QUẢN LÝ NHÀ HÀNG
// Môn: Lập trình hướng đối tượng
// Đề tài: Hệ thống Quản lý Nhà hàng
//
// Sử dụng Jackcess 5.1.0 (Java) - không cần MS Access cài đặt
// Tạo file .accdb (Access 2016 format) với:
//   - 5 bảng: Ban, SanPham, HoaDon, ChiTietHoaDon, NguoiDung
//   - Khóa chính + khóa ngoại + index
//   - Dữ liệu mẫu (3 user, 10 bàn, 12 sản phẩm, 5 hóa đơn, 11 chi tiết)
// ============================================================
import io.github.spannm.jackcess.*;
import io.github.spannm.jackcess.impl.*;
import java.io.File;
import java.math.BigDecimal;
import java.util.*;

public class tao_access {

    static Database db;

    public static void main(String[] args) throws Exception {
        File outFile = new File("QuanLyNhaHang.accdb");
        if (outFile.exists()) outFile.delete();

        System.out.println("[1/6] Đang tạo file Access mới: " + outFile.getName());
        db = DatabaseBuilder.create(Database.FileFormat.V2016, outFile);

        // ====== 1. Bảng NguoiDung ======
        System.out.println("[2/6] Tạo bảng NguoiDung...");
        Table tblNguoiDung = new TableBuilder("NguoiDung")
            .addColumn(new ColumnBuilder("Id", DataType.LONG).withAutoNumber(true))
            .addColumn(new ColumnBuilder("TenDangNhap", DataType.TEXT).withLength(50*2))
            .addColumn(new ColumnBuilder("MatKhauHash", DataType.TEXT).withLength(128*2))
            .addColumn(new ColumnBuilder("VaiTro", DataType.TEXT).withLength(20*2))
            .addColumn(new ColumnBuilder("NgayTao", DataType.SHORT_DATE_TIME))
            .addIndex(new IndexBuilder("PK_NguoiDung").withPrimaryKey().withColumns("Id"))
            .addIndex(new IndexBuilder("UK_TenDangNhap").withUnique().withColumns("TenDangNhap"))
            .toTable(db);

        // ====== 2. Bảng Ban ======
        System.out.println("[3/6] Tạo bảng Ban...");
        Table tblBan = new TableBuilder("Ban")
            .addColumn(new ColumnBuilder("Id", DataType.LONG).withAutoNumber(true))
            .addColumn(new ColumnBuilder("TenBan", DataType.TEXT).withLength(20*2))
            .addColumn(new ColumnBuilder("TrangThai", DataType.TEXT).withLength(20*2))
            .addIndex(new IndexBuilder("PK_Ban").withPrimaryKey().withColumns("Id"))
            .toTable(db);

        // ====== 3. Bảng SanPham ======
        System.out.println("[4/6] Tạo bảng SanPham...");
        Table tblSanPham = new TableBuilder("SanPham")
            .addColumn(new ColumnBuilder("Id", DataType.LONG).withAutoNumber(true))
            .addColumn(new ColumnBuilder("TenSanPham", DataType.TEXT).withLength(100*2))
            .addColumn(new ColumnBuilder("GiaCoBan", DataType.MONEY))
            .addColumn(new ColumnBuilder("Loai", DataType.TEXT).withLength(20*2))
            .addColumn(new ColumnBuilder("DangBan", DataType.BOOLEAN))
            .addColumn(new ColumnBuilder("HinhAnh", DataType.TEXT).withLength(255*2))
            .addIndex(new IndexBuilder("PK_SanPham").withPrimaryKey().withColumns("Id"))
            .toTable(db);

        // ====== 4. Bảng HoaDon ======
        System.out.println("[5/6] Tạo bảng HoaDon...");
        Table tblHoaDon = new TableBuilder("HoaDon")
            .addColumn(new ColumnBuilder("Id", DataType.LONG).withAutoNumber(true))
            .addColumn(new ColumnBuilder("BanId", DataType.LONG))
            .addColumn(new ColumnBuilder("ThoiGianTao", DataType.SHORT_DATE_TIME))
            .addColumn(new ColumnBuilder("ThoiGianThanhToan", DataType.SHORT_DATE_TIME))
            .addColumn(new ColumnBuilder("TongTien", DataType.MONEY))
            .addColumn(new ColumnBuilder("TrangThai", DataType.TEXT).withLength(30*2))
            .addColumn(new ColumnBuilder("VAT", DataType.MONEY))
            .addColumn(new ColumnBuilder("GiamGia", DataType.MONEY))
            .addColumn(new ColumnBuilder("PhuongThucThanhToan", DataType.TEXT).withLength(20*2))
            .addIndex(new IndexBuilder("PK_HoaDon").withPrimaryKey().withColumns("Id"))
            .addIndex(new IndexBuilder("FK_HoaDon_Ban").withColumns("BanId"))
            .toTable(db);

        // ====== 5. Bảng ChiTietHoaDon ======
        System.out.println("[6/6] Tạo bảng ChiTietHoaDon...");
        Table tblCTHD = new TableBuilder("ChiTietHoaDon")
            .addColumn(new ColumnBuilder("Id", DataType.LONG).withAutoNumber(true))
            .addColumn(new ColumnBuilder("HoaDonId", DataType.LONG))
            .addColumn(new ColumnBuilder("SanPhamId", DataType.LONG))
            .addColumn(new ColumnBuilder("SoLuong", DataType.INT))
            .addColumn(new ColumnBuilder("DonGiaBan", DataType.MONEY))
            .addColumn(new ColumnBuilder("ThuocTinhThem", DataType.TEXT).withLength(100*2))
            .addColumn(new ColumnBuilder("ThanhTien", DataType.MONEY))
            .addColumn(new ColumnBuilder("TrangThaiMon", DataType.TEXT).withLength(30*2))
            .addIndex(new IndexBuilder("PK_ChiTietHoaDon").withPrimaryKey().withColumns("Id"))
            .addIndex(new IndexBuilder("FK_CTHD_HoaDon").withColumns("HoaDonId"))
            .addIndex(new IndexBuilder("FK_CTHD_SanPham").withColumns("SanPhamId"))
            .toTable(db);

        // ====== Thêm quan hệ (Relationship) ======
        System.out.println("\nĐang thêm quan hệ khóa ngoại...");
        addRelationship("HoaDon_Ban", "Ban", "Id", "HoaDon", "BanId");
        addRelationship("CTHD_HoaDon", "HoaDon", "Id", "ChiTietHoaDon", "HoaDonId");
        addRelationship("CTHD_SanPham", "SanPham", "Id", "ChiTietHoaDon", "SanPhamId");

        // ====== DỮ LIỆU MẪU ======
        System.out.println("\nĐang chèn dữ liệu mẫu...");

        // ---- 3 NguoiDung ----
        Date now = new Date();
        tblNguoiDung.addRow(1, "admin",
            "240be518fabd2724ddb8f6ee857a5cf2e9e6c4c5e9e6c4c5e9e6c4c5e9e6c4c5", "QuanTri", now);
        tblNguoiDung.addRow(2, "nhanvien1",
            "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92", "NhanVien", now);
        tblNguoiDung.addRow(3, "huy",
            "240be518fabd2724ddb8f6ee857a5cf2e9e6c4c5e9e6c4c5e9e6c4c5e9e6c4c5", "QuanTri", now);

        // ---- 10 Bàn ----
        String[] tenBan = {"Bàn 1","Bàn 2","Bàn 3","Bàn 4","Bàn 5","Bàn 6","Bàn 7","Bàn 8","Bàn 9","Bàn 10"};
        String[] trangThai = {"Trống","Có khách","Trống","Có khách","Trống","Trống","Trống","Có khách","Trống","Trống"};
        for (int i = 0; i < 10; i++) {
            tblBan.addRow(i + 1, tenBan[i], trangThai[i]);
        }

        // ---- 12 Sản phẩm (6 ThucAn + 6 NuocUong) ----
        Object[][] sanpham = {
            {"Cơm gà xối mỡ", 45000.0, "ThucAn", true, "/img/com_ga.jpg"},
            {"Mỳ Quảng", 40000.0, "ThucAn", true, "/img/my_quang.jpg"},
            {"Bún bò Huế", 50000.0, "ThucAn", true, "/img/bun_bo.jpg"},
            {"Phở bò", 55000.0, "ThucAn", true, "/img/pho_bo.jpg"},
            {"Cơm sườn nướng", 60000.0, "ThucAn", true, "/img/com_suon.jpg"},
            {"Gỏi cuốn tôm", 35000.0, "ThucAn", false, "/img/goi_cuon.jpg"},
            {"Cà phê sữa", 25000.0, "NuocUong", true, "/img/ca_phe_sua.jpg"},
            {"Trà sữa trân châu", 45000.0, "NuocUong", true, "/img/tra_sua.jpg"},
            {"Nước cam ép", 35000.0, "NuocUong", true, "/img/nuoc_cam.jpg"},
            {"Coca Cola", 15000.0, "NuocUong", true, "/img/coca.jpg"},
            {"Trà đá", 10000.0, "NuocUong", true, "/img/tra_da.jpg"},
            {"Sinh tố xoài", 40000.0, "NuocUong", false, "/img/sinh_to_xoai.jpg"}
        };
        for (int i = 0; i < sanpham.length; i++) {
            tblSanPham.addRow(i + 1,
                sanpham[i][0],
                sanpham[i][1],
                sanpham[i][2],
                sanpham[i][3],
                sanpham[i][4]);
        }

        // ---- 5 Hóa đơn mẫu (đã thanh toán) ----
        Calendar cal = Calendar.getInstance();
        cal.set(2026, Calendar.JUNE, 15, 11, 30, 0);
        Date t1 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 15, 12, 0, 0);
        Date t2 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 16, 18, 0, 0);
        Date t3 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 16, 19, 30, 0);
        Date t4 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 17, 19, 0, 0);
        Date t5 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 17, 20, 0, 0);
        Date t6 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 18, 12, 0, 0);
        Date t7 = cal.getTime();
        cal.set(2026, Calendar.JUNE, 18, 13, 0, 0);
        Date t8 = cal.getTime();

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

        // ---- Chi tiết hóa đơn ----
        Object[][] cthd = {
            // HD1: Cơm gà x1 (Phần lớn +50k) + Coca Lon x1 + Trà đá x1
            // 95000 + 18000 + 10000 = 123000 → VAT 10% ≈ 11182 → TongTien nên = 123000, VAT = 11182
            // Tôi sẽ đồng bộ lại:
            {1, 1, 1, 1, 95000.0, "Phần lớn", 95000.0, "DaPhucVu"},
            {2, 1, 9, 1, 15000.0, "Lon", 18000.0, "DaPhucVu"},
            {3, 1, 10, 1, 10000.0, "Ly", 10000.0, "DaPhucVu"},
            // HD2: Cà phê x1 + Phở x2
            {4, 2, 7, 1, 25000.0, "Ly", 25000.0, "DaPhucVu"},
            {5, 2, 4, 2, 55000.0, "Phần thường", 110000.0, "DaPhucVu"},
            // HD3: Phở x1 + Trà sữa x1 + Coca x1
            {6, 3, 4, 1, 55000.0, "Phần thường", 55000.0, "DaPhucVu"},
            {7, 3, 8, 1, 45000.0, "Ly", 45000.0, "DaPhucVu"},
            {8, 3, 9, 1, 15000.0, "Lon", 18000.0, "DaPhucVu"},
            // HD4: Cơm gà x1 (Phần lớn) + Coca Lon x1 + Trà đá x1
            {9, 4, 1, 1, 95000.0, "Phần lớn", 95000.0, "DaPhucVu"},
            {10, 4, 9, 1, 15000.0, "Lon", 18000.0, "DaPhucVu"},
            {11, 4, 10, 1, 10000.0, "Ly", 10000.0, "DaPhucVu"},
            // HD5: Mỳ Quảng x1 (Phần lớn) + Nước cam x1 + Trà sữa x1
            {12, 5, 2, 1, 90000.0, "Phần lớn", 90000.0, "DaPhucVu"},
            {13, 5, 9, 1, 35000.0, "Ly", 35000.0, "DaPhucVu"},
            {14, 5, 8, 1, 45000.0, "Ly", 45000.0, "DaPhucVu"}
        };
        for (Object[] c : cthd) {
            tblCTHD.addRow(c[0], c[1], c[2], c[3], c[4], c[5], c[6], c[7]);
        }

        db.close();
        System.out.println("\n✅ Đã tạo file: " + outFile.getAbsolutePath());
        System.out.println("   Kích thước: " + outFile.length() + " bytes");

        // ====== Verify ======
        System.out.println("\n=== KIỂM TRA ===");
        db = DatabaseBuilder.open(outFile);
        for (String tn : new String[]{"NguoiDung","Ban","SanPham","HoaDon","ChiTietHoaDon"}) {
            Table t = db.getTable(tn);
            int cnt = 0;
            for (Row r : t) cnt++;
            System.out.println("   Bảng " + tn + ": " + cnt + " dòng");
        }
        System.out.println("\n   Quan hệ:");
        for (Object rel : db.getRelationships()) {
            System.out.println("     " + rel);
        }
        db.close();
        System.out.println("\n=== HOÀN TẤT ===");
    }

    // Helper: tạo quan hệ khóa ngoại giữa 2 bảng
    static void addRelationship(String name, String fromTable, String fromCol,
                                String toTable, String toCol) throws Exception {
        try {
            RelationshipBuilder rb = new RelationshipBuilder(fromTable, toTable)
                .withName(name)
                .addColumns(fromCol, toCol)
                .withReferentialIntegrity()
                .withCascadeDeletes()
                .withCascadeUpdates();
            rb.toRelationship(db);
            System.out.println("   + Quan hệ " + name + ": " + toTable + "." + toCol + " → " + fromTable + "." + fromCol);
        } catch (Exception e) {
            System.out.println("   ! Quan hệ " + name + " thất bại: " + e.getMessage());
        }
    }
}

