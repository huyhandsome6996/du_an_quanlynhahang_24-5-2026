// ============================================================
// BAOCAO.JS — Logic trang Báo Cáo Thống Kê (baocao.html)
// ------------------------------------------------------------
// Chức năng:
//   1. Tải 4 ô thống kê doanh thu (tổng / hôm nay / tháng / số HĐ)
//   2. Lọc hóa đơn theo khoảng ngày
//   3. Hiển thị bảng doanh thu theo khoảng ngày
//   4. Top 10 món bán chạy (kèm thanh tiến độ + huy chương)
//   5. Xem chi tiết + in hóa đơn
// ============================================================

let hoaDonDangXem = null;   // Lưu hóa đơn đang xem để in

// ---------- KHỞI ĐỘNG ----------
document.addEventListener('DOMContentLoaded', () => {
    // Mặc định lọc từ đầu tháng đến nay
    const homNay = new Date();
    const dauThang = new Date(homNay.getFullYear(), homNay.getMonth(), 1);
    // toISOString().split('T')[0] → format YYYY-MM-DD cho input[type=date]
    document.getElementById('dtpTuNgay').value = dauThang.toISOString().split('T')[0];
    document.getElementById('dtpDenNgay').value = homNay.toISOString().split('T')[0];

    taiBaoCaoDoanhThu();   // Tải 4 ô thống kê
    taiLichSu();            // Tải bảng hóa đơn
    taiMonBanChay();        // Tải top 10 món bán chạy
});

// ---------- 1. TẢI 4 Ô THỐNG KÊ DOANH THU ----------
async function taiBaoCaoDoanhThu() {
    try {
        // GET /api/baocao/doanhthu — trả về object { tongDoanhThu, tongHoaDon, ... }
        const res = await apiFetch(`${API}/baocao/doanhthu`);
        if (!res.ok) throw new Error('Lỗi phản hồi server');
        const data = await res.json();

        // Cập nhật 4 ô hiển thị
        document.getElementById('tongDoanhThu').textContent     = formatTien(data.tongDoanhThu || 0);
        document.getElementById('doanhThuHomNay').textContent   = formatTien(data.doanhThuHomNay || 0);
        document.getElementById('doanhThuThangNay').textContent = formatTien(data.doanhThuThangNay || 0);
        document.getElementById('tongHoaDon').textContent       = data.tongHoaDon || 0;
    } catch {
        hienToast('Lỗi tải báo cáo doanh thu!', 'error');
    }
}

// ---------- 2. TẢI DANH SÁCH HÓA ĐƠN (tất cả) ----------
async function taiLichSu() {
    document.getElementById('bangDoanhThu').innerHTML =
        '<tr><td colspan="7" class="text-center" style="padding:2rem;"><div class="spinner"></div></td></tr>';
    try {
        const res = await apiFetch(`${API}/hoadon`);
        const ds = await res.json();
        hienThiBangDoanhThu(ds);
    } catch {
        document.getElementById('bangDoanhThu').innerHTML =
            '<tr><td colspan="7"><div class="alert alert-error">⚠️ Lỗi kết nối server!</div></td></tr>';
    }
}

// ---------- 3. LỌC HÓA ĐƠN THEO KHOẢNG NGÀY ----------
async function locTheoNgay() {
    const tuNgay  = document.getElementById('dtpTuNgay').value;
    const denNgay = document.getElementById('dtpDenNgay').value;

    // Validate: phải chọn cả 2 ngày
    if (!tuNgay || !denNgay) {
        hienToast('Vui lòng chọn đầy đủ Từ ngày và Đến ngày!', 'error');
        return;
    }
    // Validate: Từ ngày không được lớn hơn Đến ngày
    if (new Date(tuNgay) > new Date(denNgay)) {
        hienToast('Từ ngày không được lớn hơn Đến ngày!', 'error');
        return;
    }

    // Hiện spinner
    document.getElementById('bangDoanhThu').innerHTML =
        '<tr><td colspan="7" class="text-center" style="padding:2rem;"><div class="spinner"></div></td></tr>';

    try {
        // GET /api/hoadon/theongay?tuNgay=...&denNgay=...
        // Server lọc các HĐ ĐÃ thanh toán trong khoảng [tuNgay, denNgay]
        const res = await apiFetch(`${API}/hoadon/theongay?tuNgay=${tuNgay}&denNgay=${denNgay}`);
        if (!res.ok) throw new Error('Lỗi phản hồi server');
        const ds = await res.json();
        hienThiBangDoanhThu(ds);
        hienToast(`Đã lọc ${ds.length} hóa đơn từ ${tuNgay} đến ${denNgay}`, 'success');
    } catch {
        document.getElementById('bangDoanhThu').innerHTML =
            '<tr><td colspan="7"><div class="alert alert-error">⚠️ Lỗi lọc hóa đơn theo ngày!</div></td></tr>';
    }
}

// ---------- 4. HIỂN THỊ BẢNG DOANH THU ----------
function hienThiBangDoanhThu(ds) {
    const tbody = document.getElementById('bangDoanhThu');
    if (!ds.length) {
        tbody.innerHTML = '<tr><td colspan="7"><div class="empty-state"><span class="empty-icon">📊</span><p>Chưa có hóa đơn nào trong khoảng này.</p></div></td></tr>';
        document.getElementById('tongDoanhThuLoc').textContent = '';
        return;
    }

    // Tính tổng doanh thu trong khoảng đã lọc (chỉ tính HĐ đã TT)
    const tongTien = ds.filter(h => h.TrangThai === 'Đã thanh toán').reduce((t, h) => t + h.TongTien, 0);
    document.getElementById('tongDoanhThuLoc').textContent = `Tổng: ${formatTien(tongTien)}`;

    // Tạo HTML cho từng dòng
    tbody.innerHTML = ds.map(hd => `
        <tr>
            <td class="px-6 py-4 font-bold text-primary">#${hd.Id}</td>
            <td class="px-6 py-4 font-medium">${hd.TenBan}</td>
            <td class="px-6 py-4 text-on-surface-variant">${formatThoiGian(hd.ThoiGianTao)}</td>
            <td class="px-6 py-4 text-on-surface-variant">
                ${hd.ThoiGianThanhToan ? formatThoiGian(hd.ThoiGianThanhToan) : '<span class="text-on-surface-variant/40 italic">Chưa đóng</span>'}
            </td>
            <td class="px-6 py-4 text-right font-bold text-primary">${formatTien(hd.TongTien)}</td>
            <td class="px-6 py-4 text-center">
                <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[11px] font-bold uppercase tracking-wider border
                    ${hd.TrangThai === 'Đã thanh toán'
                        ? 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20'
                        : 'bg-primary/10 text-primary border-primary/20'}">
                    <span class="w-1.5 h-1.5 rounded-full ${hd.TrangThai === 'Đã thanh toán' ? 'bg-emerald-400' : 'bg-primary'} animate-pulse"></span>
                    ${hd.TrangThai}
                </span>
            </td>
            <td class="px-6 py-4 text-center">
                <button class="bg-white/[0.04] hover:bg-white/[0.1] border border-white/10 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-wider transition-all text-on-surface flex items-center gap-1 mx-auto"
                        onclick="xemChiTiet(${hd.Id})">
                    <img src="img/search_3d.png" alt="Search" class="w-4 h-4 object-cover rounded-sm"> Xem
                </button>
            </td>
        </tr>`).join('');
}

// ---------- 5. TẢI TOP 10 MÓN BÁN CHẠY ----------
async function taiMonBanChay() {
    document.getElementById('bangMonBanChay').innerHTML =
        '<tr><td colspan="4" class="text-center" style="padding:2rem;"><div class="spinner"></div></td></tr>';
    try {
        // GET /api/baocao/monbanchay?top=10
        const res = await apiFetch(`${API}/baocao/monbanchay?top=10`);
        if (!res.ok) throw new Error('Lỗi phản hồi server');
        const ds = await res.json();
        hienThiMonBanChay(ds);
    } catch {
        document.getElementById('bangMonBanChay').innerHTML =
            '<tr><td colspan="4"><div class="alert alert-error">⚠️ Lỗi tải món bán chạy!</div></td></tr>';
    }
}

// ---------- 6. HIỂN THỊ BẢNG MÓN BÁN CHẠY ----------
function hienThiMonBanChay(ds) {
    const tbody = document.getElementById('bangMonBanChay');
    if (!ds.length) {
        tbody.innerHTML = '<tr><td colspan="4"><div class="empty-state"><span class="empty-icon">🍽️</span><p>Chưa có dữ liệu món bán chạy.</p></div></td></tr>';
        return;
    }

    // Mảng icon huy chương cho top 1-3
    const medalIcons = ['🥇', '🥈', '🥉'];
    // Số lượng của món top 1 — để tính % thanh tiến độ
    const maxSoLuong = ds[0].TongSoLuong;

    tbody.innerHTML = ds.map((mon, i) => {
        const hang = i + 1;   // Hạng (1, 2, 3, ...)
        // Hạng 1-3: hiện huy chương; còn lại: hiện số
        const hangDisplay = hang <= 3
            ? `<span class="text-2xl">${medalIcons[i]}</span>`
            : `<span class="text-on-surface-variant font-bold">${hang}</span>`;

        // Tính % thanh tiến độ (so với top 1)
        const percent = Math.round((mon.TongSoLuong / maxSoLuong) * 100);

        return `
        <tr>
            <td class="px-6 py-4 text-center">${hangDisplay}</td>
            <td class="px-6 py-4">
                <div class="flex flex-col gap-1.5">
                    <span class="font-semibold text-on-surface">${mon.TenSanPham}</span>
                    <!-- Thanh tiến độ tương đối -->
                    <div class="w-full max-w-[200px] h-1.5 rounded-full bg-white/[0.05] overflow-hidden">
                        <div class="h-full rounded-full transition-all duration-700
                            ${hang === 1 ? 'bg-gradient-to-r from-primary to-primary-soft'
                              : hang === 2 ? 'bg-emerald-500/60'
                              : hang === 3 ? 'bg-orange-400/60'
                              : 'bg-white/20'}"
                             style="width: ${percent}%"></div>
                    </div>
                </div>
            </td>
            <td class="px-6 py-4 text-center font-bold text-on-surface">${mon.TongSoLuong.toLocaleString('vi-VN')}</td>
            <td class="px-6 py-4 text-right font-bold text-primary">${formatTien(mon.TongDoanhThu)}</td>
        </tr>`;
    }).join('');
}

// ---------- 7. XEM CHI TIẾT HÓA ĐƠN ----------
async function xemChiTiet(id) {
    document.getElementById('chiTietHDNoidung').innerHTML = '<div class="spinner"></div>';
    moModal('modalChiTietHD');

    try {
        const res = await apiFetch(`${API}/hoadon/${id}`);
        const { hoaDon: hd, chiTiet } = await res.json();
        hoaDonDangXem = hd;

        document.getElementById('chiTietHDTieuDe').textContent = `Hóa Đơn #${hd.Id} - ${hd.TenBan}`;

        // Bảng chi tiết món
        const bangMon = chiTiet.length === 0
            ? '<p class="text-nhat text-center" style="padding:1rem;">Không có món nào.</p>'
            : `<div class="table-wrapper">
                <table>
                    <thead><tr><th>Món</th><th>Ghi Chú</th><th>SL</th><th>Đơn Giá</th><th>Thành Tiền</th></tr></thead>
                    <tbody>
                        ${chiTiet.map(ct => `
                            <tr>
                                <td>${ct.TenSanPham}</td>
                                <td><span class="text-nhat">${ct.ThuocTinhThem || '-'}</span></td>
                                <td class="text-center">${ct.SoLuong}</td>
                                <td>${formatTien(ct.DonGiaBan)}</td>
                                <td class="fw-bold text-chinh">${formatTien(ct.ThanhTien)}</td>
                            </tr>`).join('')}
                    </tbody>
                </table>
               </div>`;

        document.getElementById('chiTietHDNoidung').innerHTML = `
            <div style="display:flex; gap:2rem; margin-bottom:1rem; flex-wrap:wrap;">
                <div><span class="text-nhat">Bàn:</span> <strong>${hd.TenBan}</strong></div>
                <div><span class="text-nhat">Mở lúc:</span> <strong>${formatThoiGian(hd.ThoiGianTao)}</strong></div>
                ${hd.ThoiGianThanhToan ? `<div><span class="text-nhat">Đóng lúc:</span> <strong>${formatThoiGian(hd.ThoiGianThanhToan)}</strong></div>` : ''}
                <div><span class="badge ${hd.TrangThai === 'Đã thanh toán' ? 'badge-dathanhtoan' : 'badge-chuathanhtoan'}">${hd.TrangThai}</span></div>
            </div>
            ${bangMon}
            <div class="tong-tien-box">
                <div class="tong-tien-row tong">
                    <span>💰 Tổng cộng:</span>
                    <span>${formatTien(hd.TongTien)}</span>
                </div>
            </div>`;
    } catch {
        document.getElementById('chiTietHDNoidung').innerHTML =
            '<div class="alert alert-error">⚠️ Lỗi tải chi tiết hóa đơn!</div>';
    }
}

// ---------- 8. IN HÓA ĐƠN ----------
function inHoaDon() {
    if (!hoaDonDangXem) return;
    window.print();
}
