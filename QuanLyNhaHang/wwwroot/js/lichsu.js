// ============================================================
// LICHSU.JS - Logic trang Lịch Sử Hóa Đơn (lichsu.html)
// ============================================================

const API = 'http://localhost:5000/api';
let hoaDonDangXem = null;

document.addEventListener('DOMContentLoaded', () => { taiLichSu(); });

async function taiLichSu() {
    document.getElementById('bangLichSu').innerHTML =
        '<tr><td colspan="7" class="text-center" style="padding:2rem;"><div class="spinner"></div></td></tr>';
    try {
        const res = await fetch(`${API}/hoadon`);
        const ds = await res.json();
        hienThiBang(ds);
        capNhatThongKe(ds);
    } catch {
        document.getElementById('bangLichSu').innerHTML =
            '<tr><td colspan="7"><div class="alert alert-error">⚠️ Lỗi kết nối server!</div></td></tr>';
    }
}

function capNhatThongKe(ds) {
    document.getElementById('tongHoaDon').textContent = ds.length;
    const daThanhToan = ds.filter(h => h.TrangThai === 'Đã thanh toán');
    document.getElementById('daThanhToan').textContent = daThanhToan.length;
    const tongDoanhThu = daThanhToan.reduce((t, h) => t + h.TongTien, 0);
    document.getElementById('tongDoanhThu').textContent = formatTien(tongDoanhThu);
}

function hienThiBang(ds) {
    const tbody = document.getElementById('bangLichSu');
    if (!ds.length) {
        tbody.innerHTML = '<tr><td colspan="7"><div class="empty-state"><span class="empty-icon">📊</span><p>Chưa có hóa đơn nào.</p></div></td></tr>';
        return;
    }
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

function inHoaDon() {
    if (!hoaDonDangXem) return;
    window.print();
}

function dongModal(id) {
    document.getElementById(id).classList.remove('show');
}

function formatTien(so) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(so);
}

function formatThoiGian(chuoi) {
    if (!chuoi) return '-';
    return new Date(chuoi).toLocaleString('vi-VN');
}

// MODAL BEHAVIOR: KHÔNG cho đóng khi click bên ngoài (ShowDialog)
// Chỉ đóng khi nhấn nút Đóng bên trong modal
function moModal(id) {
    document.getElementById(id).classList.add('show');
}
// => KHÔNG thêm event click vào overlay
