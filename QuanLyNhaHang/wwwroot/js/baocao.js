// ============================================================
// BAOCAO.JS - Logic trang Báo Cáo Thống Kê (baocao.html)
// ============================================================

const API = 'http://localhost:5000/api';
let hoaDonDangXem = null;

document.addEventListener('DOMContentLoaded', () => {
    // Thiết lập ngày mặc định: từ đầu tháng đến nay
    const homNay = new Date();
    const dauThang = new Date(homNay.getFullYear(), homNay.getMonth(), 1);
    document.getElementById('dtpTuNgay').value = dauThang.toISOString().split('T')[0];
    document.getElementById('dtpDenNgay').value = homNay.toISOString().split('T')[0];

    taiBaoCaoDoanhThu();
    taiLichSu();
    taiMonBanChay();
});

// ---- Tải báo cáo doanh thu tổng hợp ----
async function taiBaoCaoDoanhThu() {
    try {
        const res = await fetch(`${API}/baocao/doanhthu`);
        if (!res.ok) throw new Error('Lỗi phản hồi server');
        const data = await res.json();

        document.getElementById('tongDoanhThu').textContent = formatTien(data.tongDoanhThu || 0);
        document.getElementById('doanhThuHomNay').textContent = formatTien(data.doanhThuHomNay || 0);
        document.getElementById('doanhThuThangNay').textContent = formatTien(data.doanhThuThangNay || 0);
        document.getElementById('tongHoaDon').textContent = data.tongHoaDon || 0;
    } catch (err) {
        hienToast('Lỗi tải báo cáo doanh thu!', 'error');
    }
}

// ---- Tải danh sách hóa đơn (lịch sử) ----
async function taiLichSu() {
    document.getElementById('bangDoanhThu').innerHTML =
        '<tr><td colspan="7" class="text-center" style="padding:2rem;"><div class="spinner"></div></td></tr>';
    try {
        const res = await fetch(`${API}/hoadon`);
        const ds = await res.json();
        hienThiBangDoanhThu(ds);
    } catch {
        document.getElementById('bangDoanhThu').innerHTML =
            '<tr><td colspan="7"><div class="alert alert-error">⚠️ Lỗi kết nối server!</div></td></tr>';
    }
}

// ---- Lọc hóa đơn theo khoảng ngày ----
async function locTheoNgay() {
    const tuNgay = document.getElementById('dtpTuNgay').value;
    const denNgay = document.getElementById('dtpDenNgay').value;

    if (!tuNgay || !denNgay) {
        hienToast('Vui lòng chọn đầy đủ Từ ngày và Đến ngày!', 'error');
        return;
    }

    if (new Date(tuNgay) > new Date(denNgay)) {
        hienToast('Từ ngày không được lớn hơn Đến ngày!', 'error');
        return;
    }

    document.getElementById('bangDoanhThu').innerHTML =
        '<tr><td colspan="7" class="text-center" style="padding:2rem;"><div class="spinner"></div></td></tr>';

    try {
        const res = await fetch(`${API}/hoadon/theongay?tuNgay=${tuNgay}&denNgay=${denNgay}`);
        if (!res.ok) throw new Error('Lỗi phản hồi server');
        const ds = await res.json();
        hienThiBangDoanhThu(ds);

        // Tính tổng doanh thu trong khoảng ngày lọc
        const tongTien = ds
            .filter(h => h.TrangThai === 'Đã thanh toán')
            .reduce((t, h) => t + h.TongTien, 0);
        document.getElementById('tongDoanhThuLoc').textContent =
            ds.length > 0 ? `Tổng: ${formatTien(tongTien)}` : '';

        hienToast(`Đã lọc ${ds.length} hóa đơn từ ${tuNgay} đến ${denNgay}`, 'success');
    } catch {
        document.getElementById('bangDoanhThu').innerHTML =
            '<tr><td colspan="7"><div class="alert alert-error">⚠️ Lỗi lọc hóa đơn theo ngày!</div></td></tr>';
    }
}

// ---- Hiển thị bảng doanh thu ----
function hienThiBangDoanhThu(ds) {
    const tbody = document.getElementById('bangDoanhThu');
    if (!ds.length) {
        tbody.innerHTML = '<tr><td colspan="7"><div class="empty-state"><span class="empty-icon">📊</span><p>Chưa có hóa đơn nào trong khoảng này.</p></div></td></tr>';
        document.getElementById('tongDoanhThuLoc').textContent = '';
        return;
    }

    // Tính tổng doanh thu
    const tongTien = ds
        .filter(h => h.TrangThai === 'Đã thanh toán')
        .reduce((t, h) => t + h.TongTien, 0);
    document.getElementById('tongDoanhThuLoc').textContent = `Tổng: ${formatTien(tongTien)}`;

    tbody.innerHTML = ds.map(hd => `
        <tr>
            <td class="px-6 py-4 font-bold text-primary">#${hd.Id}</td>
            <td class="px-6 py-4 font-medium">${hd.TenBan}</td>
            <td class="px-6 py-4 text-on-surface-variant">${formatThoiGian(hd.ThoiGianTao)}</td>
            <td class="px-6 py-4 text-on-surface-variant">${hd.ThoiGianThanhToan ? formatThoiGian(hd.ThoiGianThanhToan) : '<span class="text-on-surface-variant/40 italic">Chưa đóng</span>'}</td>
            <td class="px-6 py-4 text-right font-bold text-primary">${formatTien(hd.TongTien)}</td>
            <td class="px-6 py-4 text-center">
                <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[11px] font-bold uppercase tracking-wider border ${hd.TrangThai === 'Đã thanh toán' ? 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20' : 'bg-primary/10 text-primary border-primary/20'}">
                    <span class="w-1.5 h-1.5 rounded-full ${hd.TrangThai === 'Đã thanh toán' ? 'bg-emerald-400' : 'bg-primary'} animate-pulse"></span>
                    ${hd.TrangThai}
                </span>
            </td>
            <td class="px-6 py-4 text-center">
                <button class="bg-white/[0.04] hover:bg-white/[0.1] border border-white/10 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-wider transition-all active:scale-[0.95] text-on-surface flex items-center justify-center gap-1 mx-auto" onclick="xemChiTiet(${hd.Id})">
                    <img src="img/search_3d.png" alt="Search" class="w-4 h-4 object-cover rounded-sm"> Xem
                </button>
            </td>
        </tr>`).join('');
}

// ---- Tải món bán chạy ----
async function taiMonBanChay() {
    document.getElementById('bangMonBanChay').innerHTML =
        '<tr><td colspan="4" class="text-center" style="padding:2rem;"><div class="spinner"></div></td></tr>';
    try {
        const res = await fetch(`${API}/baocao/monbanchay?top=10`);
        if (!res.ok) throw new Error('Lỗi phản hồi server');
        const ds = await res.json();
        hienThiMonBanChay(ds);
    } catch {
        document.getElementById('bangMonBanChay').innerHTML =
            '<tr><td colspan="4"><div class="alert alert-error">⚠️ Lỗi tải món bán chạy!</div></td></tr>';
    }
}

// ---- Hiển thị bảng món bán chạy ----
function hienThiMonBanChay(ds) {
    const tbody = document.getElementById('bangMonBanChay');
    if (!ds.length) {
        tbody.innerHTML = '<tr><td colspan="4"><div class="empty-state"><span class="empty-icon">🍽️</span><p>Chưa có dữ liệu món bán chạy.</p></div></td></tr>';
        return;
    }

    const medalIcons = ['🥇', '🥈', '🥉'];

    tbody.innerHTML = ds.map((mon, i) => {
        const hang = i + 1;
        let hangDisplay;
        if (hang <= 3) {
            hangDisplay = `<span class="text-2xl">${medalIcons[i]}</span>`;
        } else {
            hangDisplay = `<span class="text-on-surface-variant font-bold">${hang}</span>`;
        }

        // Thanh tiến độ tương đối
        const maxSoLuong = ds[0].TongSoLuong;
        const percent = Math.round((mon.TongSoLuong / maxSoLuong) * 100);

        return `
        <tr>
            <td class="px-6 py-4 text-center">${hangDisplay}</td>
            <td class="px-6 py-4">
                <div class="flex flex-col gap-1.5">
                    <span class="font-semibold text-on-surface">${mon.TenSanPham}</span>
                    <div class="w-full max-w-[200px] h-1.5 rounded-full bg-white/[0.05] overflow-hidden">
                        <div class="h-full rounded-full transition-all duration-700 ${hang === 1 ? 'bg-gradient-to-r from-primary to-primary-soft' : hang === 2 ? 'bg-emerald-500/60' : hang === 3 ? 'bg-orange-400/60' : 'bg-white/20'}" style="width: ${percent}%"></div>
                    </div>
                </div>
            </td>
            <td class="px-6 py-4 text-center font-bold text-on-surface">${mon.TongSoLuong.toLocaleString('vi-VN')}</td>
            <td class="px-6 py-4 text-right font-bold text-primary">${formatTien(mon.TongDoanhThu)}</td>
        </tr>`;
    }).join('');
}

// ---- Xem chi tiết hóa đơn ----
async function xemChiTiet(id) {
    document.getElementById('chiTietHDNoidung').innerHTML = '<div class="spinner"></div>';
    moModal('modalChiTietHD');

    try {
        const res = await fetch(`${API}/hoadon/${id}`);
        const data = await res.json();
        const { hoaDon: hd, chiTiet } = data;
        hoaDonDangXem = hd;

        document.getElementById('chiTietHDTieuDe').textContent =
            `Hóa Đơn #${hd.Id} - ${hd.TenBan}`;

        const bangMon = chiTiet.length === 0
            ? '<p class="text-nhat text-center" style="padding:1rem;">Không có món nào.</p>'
            : `<div class="table-wrapper">
                <table>
                    <thead><tr><th>Món</th><th>Ghi Chú</th><th>SL</th><th>Đơn Giá</th><th>Thành Tiền</th></tr></thead>
                    <tbody>${chiTiet.map(ct => `
                        <tr>
                            <td>${ct.TenSanPham}</td>
                            <td><span class="text-nhat">${ct.ThuocTinhThem || '-'}</span></td>
                            <td class="text-center">${ct.SoLuong}</td>
                            <td>${formatTien(ct.DonGiaBan)}</td>
                            <td class="fw-bold text-chinh">${formatTien(ct.ThanhTien)}</td>
                        </tr>`).join('')}
                    </tbody>
                </table></div>`;

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

// ---- In hóa đơn ----
function inHoaDon() {
    if (!hoaDonDangXem) return;
    window.print();
}

// ---- Hàm tiện ích ----

// Hiển thị thông báo nội tuyến + Toast notification
function hienThiThongBao(noiDung, loai = 'success') {
    const kv = document.getElementById('thongBaoKhuVuc');
    if (kv) {
        kv.innerHTML = `<div class="alert alert-${loai}">${noiDung}</div>`;
        setTimeout(() => kv.innerHTML = '', 4000);
    }
    hienToast(noiDung, loai);
}

// Toast notification — Hiển thị thông báo nổi góc phải trên cùng
function hienToast(noiDung, loai = 'success') {
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `toast toast-${loai}`;
    const icon = loai === 'success' ? '✅' : '❌';
    toast.innerHTML = `<span style="font-size:1.1rem;">${icon}</span><span>${noiDung}</span>`;
    container.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('toast-out');
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

function formatTien(so) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(so);
}

function formatThoiGian(chuoi) {
    if (!chuoi) return '-';
    return new Date(chuoi).toLocaleString('vi-VN');
}

// MODAL — Mở/đóng modal mượt mà với hiệu ứng scale
function moModal(id) {
    const modal = document.getElementById(id);
    if (modal) {
        modal.classList.add('show');
        setTimeout(() => {
            const child = modal.firstElementChild;
            if (child) {
                child.classList.remove('scale-95');
                child.classList.add('scale-100');
            }
        }, 50);
    }
}

function dongModal(id) {
    const modal = document.getElementById(id);
    if (modal) {
        const child = modal.firstElementChild;
        if (child) {
            child.classList.remove('scale-100');
            child.classList.add('scale-95');
        }
        setTimeout(() => {
            modal.classList.remove('show');
        }, 150);
    }
}
