// ============================================================
// LICHSU.JS — Logic trang Lịch Sử Hóa Đơn (lichsu.html)
//
// Chức năng:
//   1. Tải danh sách toàn bộ hóa đơn
//   2. Hiển thị 3 ô thống kê nhanh (tổng HĐ, đã thanh toán, doanh thu)
//   3. Click "Xem" → mở modal chi tiết món trong hóa đơn
//   4. In hóa đơn
// ============================================================

let hoaDonDangXem = null;   // Lưu hóa đơn đang xem để in

// ---------- KHỞI ĐỘNG ----------
document.addEventListener('DOMContentLoaded', taiLichSu);

// ---------- 1. TẢI LỊCH SỬ HÓA ĐƠN ----------
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

// ---------- 2. CẬP NHẬT 3 Ô THỐNG KÊ ----------
function capNhatThongKe(ds) {
    document.getElementById('tongHoaDon').textContent = ds.length;
    const daThanhToan = ds.filter(h => h.TrangThai === 'Đã thanh toán');
    document.getElementById('daThanhToan').textContent = daThanhToan.length;
    const tongDoanhThu = daThanhToan.reduce((t, h) => t + h.TongTien, 0);
    document.getElementById('tongDoanhThu').textContent = formatTien(tongDoanhThu);
}

// ---------- 3. HIỂN THỊ BẢNG LỊCH SỬ ----------
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

// ---------- 4. XEM CHI TIẾT HÓA ĐƠN ----------
async function xemChiTiet(id) {
    document.getElementById('chiTietHDNoidung').innerHTML = '<div class="spinner"></div>';
    moModal('modalChiTietHD');

    try {
        const res = await fetch(`${API}/hoadon/${id}`);
        const { hoaDon: hd, chiTiet } = await res.json();
        hoaDonDangXem = hd;

        document.getElementById('chiTietHDTieuDe').textContent = `Hóa Đơn #${hd.Id} - ${hd.TenBan}`;

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

// ---------- 5. IN HÓA ĐƠN ----------
function inHoaDon() {
    if (!hoaDonDangXem) return;
    window.print();
}
