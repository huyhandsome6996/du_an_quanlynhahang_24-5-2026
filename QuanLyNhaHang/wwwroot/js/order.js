// ============================================================
// ORDER.JS — Logic trang Gọi Món & Thanh Toán (order.html)
// ------------------------------------------------------------
// Đây là FORM QUẢN LÝ QUAN HỆ 2 ĐỐI TƯỢNG:
//   - Hóa Đơn (1) ←→ (N) Chi Tiết Hóa Đơn ←→ (1) Sản Phẩm
//
// Quy trình:
//   1. Chọn bàn → nếu bàn trống thì mở bàn (tạo hóa đơn mới)
//   2. Chọn món từ thực đơn → nhập số lượng + ghi chú → thêm vào hóa đơn
//   3. Nhập giảm giá → tính VAT (10%) → bấm "Thanh Toán"
// ============================================================

let danhSachMenu = [];     // Cache thực đơn (chỉ món đang bán)
let monDangChon = null;    // Món đang được chọn để thêm (object)
let hoaDonHienTai = null;  // Hóa đơn đang mở của bàn đã chọn

// Gợi ý ghi chú nhanh theo loại món (để user click thay vì gõ)
const GOI_Y_THUC_AN   = ['Phần lớn', 'Không hành', 'Ít cay', 'Không cay'];
const GOI_Y_NUOC_UONG = ['Lon', 'Ly', 'Ít đá', 'Nhiều đá', 'Không đường'];

// ---------- KHỞI ĐỘNG ----------
document.addEventListener('DOMContentLoaded', async () => {
    await taiDanhSachBan();   // Tải dropdown danh sách bàn
    await taiMenu();           // Tải thực đơn (chỉ món đang bán)

    // Nếu URL có ?banId=... (từ trang Sơ đồ bàn bấm "Gọi thêm món") → tự chọn bàn đó
    const params = new URLSearchParams(window.location.search);
    const banIdTuUrl = params.get('banId');
    if (banIdTuUrl) {
        const cboBan = document.getElementById('cboBan');
        // Kiểm tra xem banId có trong dropdown không (some() trả true nếu có phần tử thoả mãn)
        if (cboBan && [...cboBan.options].some(o => o.value === banIdTuUrl)) {
            cboBan.value = banIdTuUrl;
            await chonBan();   // Trigger sự kiện chọn bàn
        }
    }
});

// ---------- 1. TẢI DANH SÁCH BÀN VÀO DROPDOWN ----------
async function taiDanhSachBan() {
    try {
        const res = await fetch(`${API}/ban`);
        const dsBan = await res.json();
        const select = document.getElementById('cboBan');
        // Option đầu tiên = placeholder
        select.innerHTML = '<option value="">-- Chọn bàn cần phục vụ --</option>';
        // Thêm từng bàn vào dropdown
        dsBan.forEach(b => {
            const opt = document.createElement('option');
            opt.value = b.Id;
            opt.textContent = `${b.TenBan} (${b.TrangThai})`;   // Hiển thị cả trạng thái
            select.appendChild(opt);
        });
    } catch {
        hienThiThongBao('Không kết nối được server!', 'error');
    }
}

// ---------- 2. TẢI THỰC ĐƠN (chỉ món đang bán) ----------
async function taiMenu() {
    try {
        const res = await fetch(`${API}/sanpham/dangban`);
        danhSachMenu = await res.json();
        hienThiMenu(danhSachMenu);
    } catch (err) {
        console.error(err);
    }
}

// ---------- 3. HIỂN THỊ THỰC ĐƠN ----------
function hienThiMenu(ds) {
    const kv = document.getElementById('menuDanhSach');
    if (!ds.length) {
        kv.innerHTML = '<p class="text-nhat text-center" style="padding:1rem;">Không có món nào.</p>';
        return;
    }
    // Tạo HTML cho từng món — click vào → chonMon(Id)
    kv.innerHTML = ds.map(sp => {
        const imgUrl = sp.HinhAnh || 'img/logo.png';
        return `
        <div class="mon-item" onclick="chonMon(${sp.Id})">
            <div class="w-20 h-20 rounded-full overflow-hidden shadow-inner border-2 border-primary/20">
                <img src="${imgUrl}" alt="${sp.TenSanPham}" class="w-full h-full object-cover" onerror="this.src='img/logo.png'">
            </div>
            <div class="mon-ten" title="${sp.TenSanPham}">${sp.TenSanPham}</div>
            <span class="badge ${sp.Loai === 'ThucAn' ? 'badge-thucan' : 'badge-nuocuong'} text-[10px] uppercase tracking-widest px-2 py-0.5">
                ${sp.Loai === 'ThucAn' ? '🍖 Thức ăn' : '🥤 Nước uống'}
            </span>
            <span class="mon-gia">${formatTien(sp.GiaCoBan)}</span>
        </div>`;
    }).join('');
}

// ---------- 4. LỌC MENU THEO LOẠI ----------
function locMenuTheoLoai(loai) {
    // loai = '' (tất cả) / 'ThucAn' / 'NuocUong'
    const ds = loai ? danhSachMenu.filter(sp => sp.Loai === loai) : danhSachMenu;
    hienThiMenu(ds);

    // Đổi style nút active — nút đang được chọn có class 'btn-primary', các nút khác 'btn-secondary'
    document.getElementById('btnTatCa').className    = 'btn btn-sm ' + (loai === ''         ? 'btn-primary' : 'btn-secondary');
    document.getElementById('btnThucAn').className   = 'btn btn-sm ' + (loai === 'ThucAn'   ? 'btn-primary' : 'btn-secondary');
    document.getElementById('btnNuocUong').className = 'btn btn-sm ' + (loai === 'NuocUong' ? 'btn-primary' : 'btn-secondary');
}

// ---------- 5. XỬ LÝ KHI CHỌN BÀN ----------
async function chonBan() {
    const banId = document.getElementById('cboBan').value;
    const thongTin = document.getElementById('thongTinBan');
    const khuVucGoiMon = document.getElementById('khuVucGoiMon');

    // Nếu user bỏ chọn bàn (chọn option rỗng) → ẩn hết
    if (!banId) {
        thongTin.style.display = 'none';
        khuVucGoiMon.style.display = 'none';
        document.getElementById('chuaChonBan').style.display = 'block';
        document.getElementById('danhSachMon').style.display = 'none';
        return;
    }

    // Lấy thông tin chi tiết bàn
    const res = await fetch(`${API}/ban/${banId}`);
    const ban = await res.json();
    thongTin.style.display = 'block';

    const badge = document.getElementById('badgeTrangThaiBan');
    const thongBaoMoBan = document.getElementById('thongBaoMoBan');

    if (ban.TrangThai === 'Có khách') {
        // Bàn đã có khách → hiện menu gọi món + tải hóa đơn đang mở
        badge.className = 'badge badge-cokhach';
        badge.textContent = '● Có khách';
        thongBaoMoBan.textContent = '– Đang có hóa đơn mở';
        khuVucGoiMon.style.display = 'block';
        taiLaiHoaDon();   // Tải hóa đơn + chi tiết món
    } else {
        // Bàn trống (hoặc đã đặt) → hiện nút "Mở Bàn"
        badge.className = 'badge badge-trong';
        badge.textContent = '● Trống';
        thongBaoMoBan.textContent = '– Cần mở bàn trước';
        khuVucGoiMon.style.display = 'none';

        // Hiện nội dung "Bàn đang Trống" + nút "Mở Bàn Đón Khách"
        document.getElementById('chuaChonBan').innerHTML = `
            <div class="flex justify-center mb-4"><img src="img/table_3d.png" class="w-16 h-16 object-cover rounded-xl shadow-lg opacity-90"></div>
            <p style="margin-bottom:1rem;">${ban.TenBan} đang <strong style="color:var(--mau-xanh)">Trống</strong></p>
            <button class="btn btn-primary px-6 py-3 flex items-center justify-center gap-2 mx-auto" onclick="moBanVaGoiMon(${banId})">
                <img src="img/add_3d.png" class="w-5 h-5 object-cover rounded-sm"> Mở Bàn Đón Khách
            </button>`;
        document.getElementById('chuaChonBan').style.display = 'block';
        document.getElementById('danhSachMon').style.display = 'none';
    }
}

// ---------- 6. MỞ BÀN (TẠO HÓA ĐƠN) ----------
async function moBanVaGoiMon(banId) {
    try {
        // POST /api/ban/{id}/mo — server tạo HóaDon mới + set Bàn = "Có khách"
        const res = await fetch(`${API}/ban/${banId}/mo`, { method: 'POST' });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            await taiDanhSachBan();                  // Refresh dropdown
            document.getElementById('cboBan').value = banId;
            await chonBan();                          // Trigger chọn bàn lại
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 7. TẢI LẠI HÓA ĐƠN CỦA BÀN ----------
async function taiLaiHoaDon() {
    const banId = document.getElementById('cboBan').value;
    if (!banId) return;
    try {
        const res = await fetch(`${API}/ban/${banId}/hoadon`);
        if (!res.ok) {
            // Bàn không có hóa đơn chưa TT → ẩn danh sách món
            document.getElementById('chuaChonBan').style.display = 'block';
            document.getElementById('danhSachMon').style.display = 'none';
            return;
        }
        const data = await res.json();
        hoaDonHienTai = data.hoaDon;   // Lưu vào biến toàn cục để các hàm khác dùng
        hienThiHoaDon(data.hoaDon, data.chiTiet);
        capNhatHienThiThanhToan();     // Cập nhật tổng tiền / VAT / tổng cuối
    } catch {
        hienThiThongBao('Lỗi tải hóa đơn!', 'error');
    }
}

// ---------- 8. HIỂN THỊ HÓA ĐƠN ----------
function hienThiHoaDon(hd, chiTiet) {
    document.getElementById('chuaChonBan').style.display = 'none';
    document.getElementById('danhSachMon').style.display = 'block';

    const bang = document.getElementById('bangChiTietHoaDon');
    if (!chiTiet.length) {
        // Chưa có món nào → hiện empty state
        bang.innerHTML = `
            <div class="empty-state flex flex-col items-center justify-center py-8">
                <img src="img/menu_book_3d.png" class="w-14 h-14 object-cover rounded-xl mb-3 shadow-md opacity-80">
                <p class="text-on-surface-variant text-sm">Chưa có món nào. Hãy chọn món từ menu!</p>
            </div>`;
    } else {
        // Tạo bảng chi tiết món — mỗi dòng có nút Xoá
        bang.innerHTML = `
            <div class="table-wrapper">
                <table>
                    <thead><tr><th>Món</th><th>Ghi Chú</th><th>SL</th><th>Thành Tiền</th><th></th></tr></thead>
                    <tbody>
                        ${chiTiet.map(ct => `
                            <tr>
                                <td><strong>${ct.TenSanPham}</strong></td>
                                <td><span class="text-nhat">${ct.ThuocTinhThem || '-'}</span></td>
                                <td class="text-center">${ct.SoLuong}</td>
                                <td class="text-chinh fw-bold">${formatTien(ct.ThanhTien)}</td>
                                <td>
                                    <button class="bg-white/[0.05] hover:bg-red-500/20 border border-red-500/10 p-2 rounded-lg transition-all cursor-pointer"
                                            onclick="xoaMon(${ct.Id})" title="Xóa">
                                        <img src="img/close_3d.png" class="w-3.5 h-3.5 object-cover">
                                    </button>
                                </td>
                            </tr>`).join('')}
                    </tbody>
                </table>
            </div>`;
    }
    // Hiện tổng tiền hiện tại của hóa đơn
    document.getElementById('tongTienHienThi').textContent = formatTien(hd.TongTien);
}

// ---------- 9. CHỌN MỘT MÓN TỪ MENU ----------
function chonMon(sanPhamId) {
    // Tìm món trong cache
    const sp = danhSachMenu.find(m => m.Id === sanPhamId);
    if (!sp) return;
    monDangChon = sp;   // Lưu vào biến toàn cục

    // Hiện form "Thêm món" với thông tin món
    document.getElementById('tenMonDangThem').textContent = `${sp.TenSanPham} - ${formatTien(sp.GiaCoBan)}`;
    document.getElementById('txtSoLuong').value = 1;
    document.getElementById('txtThuocTinhThem').value = '';
    document.getElementById('formThemMon').classList.add('show');

    // Hiện gợi ý ghi chú theo loại món
    // (ThucAn: Phần lớn, Không hành... / NuocUong: Lon, Ly, Ít đá...)
    const goiY = sp.Loai === 'ThucAn' ? GOI_Y_THUC_AN : GOI_Y_NUOC_UONG;
    document.getElementById('goiYTuyChon').innerHTML = goiY.map(opt =>
        `<button class="btn btn-sm btn-secondary" onclick="chonGoiY('${opt}')">${opt}</button>`
    ).join('');
}

// Click vào 1 nút gợi ý → điền vào input ghi chú
function chonGoiY(text) { document.getElementById('txtThuocTinhThem').value = text; }

// Huỷ chọn món (đóng form)
function huyChonMon()   { monDangChon = null; document.getElementById('formThemMon').classList.remove('show'); }

// ---------- 10. THÊM MÓN VÀO HÓA ĐƠN ----------
async function themMon() {
    // Validate: phải có bàn + món được chọn
    if (!monDangChon || !hoaDonHienTai) {
        hienThiThongBao('Vui lòng chọn bàn và món trước!', 'error');
        return;
    }

    const soLuong = parseInt(document.getElementById('txtSoLuong').value);
    const thuocTinhThem = document.getElementById('txtThuocTinhThem').value.trim();

    if (!soLuong || soLuong <= 0) {
        hienThiThongBao('Số lượng phải lớn hơn 0!', 'error');
        document.getElementById('txtSoLuong').focus();
        return;
    }

    try {
        // POST /api/hoadon/{id}/them-mon
        // Server sẽ gọi TinhTien() của ThucAn/NuocUong (ĐA HÌNH OOP ở backend)
        // để tính thành tiền có phụ phí (Phần lớn +50k / Lon ×1.2)
        const res = await fetch(`${API}/hoadon/${hoaDonHienTai.Id}/them-mon`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                SanPhamId: monDangChon.Id,
                SoLuong: soLuong,
                ThuocTinhThem: thuocTinhThem
            })
        });
        const data = await res.json();
        if (res.ok) {
            // Hiện thông báo kèm mô tả phụ phí (đa hình: "+50,000đ (Phần lớn)" / "+20% (Dạng Lon)")
            hienThiThongBao(`✅ Đã thêm! ${data.moTaPhuPhi} | Thành tiền: ${formatTien(data.thanhTien)}`, 'success');
            huyChonMon();        // Đóng form thêm món
            taiLaiHoaDon();      // Refresh hóa đơn
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 11. XÓA 1 MÓN KHỎI HÓA ĐƠN ----------
async function xoaMon(chiTietId) {
    if (!confirm('Xóa món này khỏi hóa đơn?')) return;
    try {
        // DELETE /api/chitiethoadon/{id} — server tự tính lại tổng tiền
        const res = await fetch(`${API}/chitiethoadon/${chiTietId}`, { method: 'DELETE' });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiLaiHoaDon();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 12. THANH TOÁN ----------
async function thanhToan() {
    const banId = document.getElementById('cboBan').value;
    if (!banId) return;

    // Tính VAT 10% + giảm giá + tổng cuối
    const tongTienMon = hoaDonHienTai?.TongTien || 0;
    const vat = Math.round(tongTienMon * 0.10);                              // VAT 10% (làm tròn)
    const giamGia = parseInt(document.getElementById('txtGiamGia')?.value) || 0;
    const phuongThuc = document.getElementById('cboPhuongThuc')?.value || 'TienMat';
    const tongCuoi = Math.max(0, tongTienMon + vat - giamGia);              // Không âm

    // Hiện dialog xác nhận — chuyển mã PTTT sang tiếng Việt
    const ptText = phuongThuc === 'TienMat' ? 'Tiền mặt'
                  : phuongThuc === 'The'    ? 'Quẹt thẻ'
                  : phuongThuc === 'QR'     ? 'QR Code'
                  :                           'Chuyển khoản';

    if (!confirm(`Xác nhận thanh toán?\n\nTạm tính: ${formatTien(tongTienMon)}\nVAT (10%): ${formatTien(vat)}\nGiảm giá: ${formatTien(giamGia)}\nTỔNG CỘNG: ${formatTien(tongCuoi)}\nPhương thức: ${ptText}`)) return;

    try {
        // POST /api/ban/{id}/thanhtoan — server sẽ:
        //   1. Cập nhật VAT, GiamGia, PTTT
        //   2. Tính lại TongTien = TongTienMon + VAT - GiamGia
        //   3. Set TrangThai = "Đã thanh toán", ThoiGianThanhToan = now
        //   4. Đổi trạng thái bàn → "Trống"
        const res = await fetch(`${API}/ban/${banId}/thanhtoan`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ VAT: vat, GiamGia: giamGia, PhuongThucThanhToan: phuongThuc })
        });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ ${data.thongBao} | Đã thu: ${formatTien(data.tongTien)}`, 'success');
            // Reset trạng thái UI về "chưa chọn bàn"
            document.getElementById('cboBan').value = '';
            document.getElementById('thongTinBan').style.display = 'none';
            document.getElementById('khuVucGoiMon').style.display = 'none';
            document.getElementById('chuaChonBan').style.display = 'block';
            document.getElementById('danhSachMon').style.display = 'none';
            hoaDonHienTai = null;
            document.getElementById('txtGiamGia').value = '';
            document.getElementById('cboPhuongThuc').value = 'TienMat';
            await taiDanhSachBan();   // Refresh dropdown bàn
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 13. CẬP NHẬT HIỂN THỊ THANH TOÁN (real-time) ----------
// Gọi mỗi khi: thêm món, xoá món, đổi giảm giá → cập nhật VAT và tổng cuối ngay
function capNhatHienThiThanhToan() {
    const tongTienMon = hoaDonHienTai?.TongTien || 0;
    const vat = Math.round(tongTienMon * 0.10);
    const giamGia = parseInt(document.getElementById('txtGiamGia')?.value) || 0;
    const tongCuoi = Math.max(0, tongTienMon + vat - giamGia);

    // Hiện VAT
    const vatEl = document.getElementById('vatHienThi');
    if (vatEl) vatEl.textContent = formatTien(vat);

    // Hiện tổng cuối cùng
    const tongCuoiEl = document.getElementById('tongCuoiCung');
    if (tongCuoiEl) tongCuoiEl.textContent = formatTien(tongCuoi);
}
