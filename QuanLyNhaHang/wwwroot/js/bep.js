// ============================================================
// BEP.JS - Logic trang Màn Hình Bếp (bep.html)
// ============================================================

const API = 'http://localhost:5000/api';

// Tự động tải dữ liệu khi trang sẵn sàng
document.addEventListener('DOMContentLoaded', () => {
    taiTatCa();
    // Tự động làm mới mỗi 5 giây
    setInterval(taiTatCa, 5000);
});

// ---- TẢI DỮ LIỆU ----

async function taiMonDangCho() {
    try {
        const res = await fetch(`${API}/bep/dangcho`);
        if (!res.ok) throw new Error('Lỗi server');
        const ds = await res.json();
        hienThiDangCho(ds);
    } catch (err) {
        console.error('Lỗi tải đơn đang chờ:', err);
        hienThiDangCho([]);
    }
}

async function taiMonDangChuanBi() {
    try {
        const res = await fetch(`${API}/bep/dangchuanbi`);
        if (!res.ok) throw new Error('Lỗi server');
        const ds = await res.json();
        hienThiDangChuanBi(ds);
    } catch (err) {
        console.error('Lỗi tải đơn đang chế biến:', err);
        hienThiDangChuanBi([]);
    }
}

function taiTatCa() {
    taiMonDangCho();
    taiMonDangChuanBi();
}

// ---- HIỂN THỊ DỮ LIỆU ----

function hienThiDangCho(ds) {
    const khuVuc = document.getElementById('danhSachDangCho');
    const badge = document.getElementById('badgeDangCho');
    badge.textContent = ds.length;

    if (!ds.length) {
        khuVuc.innerHTML = `
            <div class="flex flex-col items-center justify-center py-16 text-center">
                <img src="img/check_3d.png" alt="Empty" class="w-16 h-16 object-cover rounded-xl opacity-30 mb-3">
                <p class="text-on-surface-variant text-sm">Không có đơn nào đang chờ.</p>
                <p class="text-on-surface-variant/50 text-xs mt-1">Bếp rảnh rỗi — mọi món đã được nhận!</p>
            </div>`;
        return;
    }

    khuVuc.innerHTML = ds.map(mon => `
        <div class="card-dangcho glass-card bg-surface-container/40 rounded-xl p-4 transition-all hover:bg-surface-container/70" data-id="${mon.Id}">
            <div class="flex justify-between items-start gap-3">
                <div class="flex-1 min-w-0">
                    <h4 class="font-bold text-on-surface text-base leading-tight truncate">${escHtml(mon.TenSanPham || 'Không tên')}</h4>
                    <div class="flex items-center gap-3 mt-2 flex-wrap">
                        <span class="inline-flex items-center gap-1 text-xs font-semibold text-amber-400 bg-amber-500/10 border border-amber-500/20 px-2.5 py-1 rounded-lg">
                            ×${mon.SoLuong || 1}
                        </span>
                        <span class="text-xs text-on-surface-variant">
                            Hóa đơn <strong class="text-primary">#${mon.HoaDonId}</strong>
                        </span>
                    </div>
                    ${mon.ThuocTinhThem ? `
                    <div class="mt-2.5 flex items-start gap-1.5">
                        <img src="img/click_3d.png" alt="Note" class="w-4 h-4 object-cover rounded-sm opacity-60 mt-0.5 shrink-0">
                        <span class="text-xs text-on-surface-variant/80 italic leading-relaxed">${escHtml(mon.ThuocTinhThem)}</span>
                    </div>` : ''}
                </div>
                <div class="shrink-0">
                    <button class="btnNhanCheBien btn btn-primary px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-wider shadow-[0_4px_15px_rgba(212,168,83,0.2)] hover:shadow-[0_6px_20px_rgba(212,168,83,0.35)] transition-all active:scale-95 flex items-center gap-1.5 border-none cursor-pointer" onclick="nhanCheBien(${mon.Id})">
                        🍳 Nhận chế biến
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

function hienThiDangChuanBi(ds) {
    const khuVuc = document.getElementById('danhSachDangChuanBi');
    const badge = document.getElementById('badgeDangChuanBi');
    badge.textContent = ds.length;

    if (!ds.length) {
        khuVuc.innerHTML = `
            <div class="flex flex-col items-center justify-center py-16 text-center">
                <img src="img/menu_book_3d.png" alt="Empty" class="w-16 h-16 object-cover rounded-xl opacity-30 mb-3">
                <p class="text-on-surface-variant text-sm">Không có món nào đang chế biến.</p>
                <p class="text-on-surface-variant/50 text-xs mt-1">Chờ nhận đơn mới từ danh sách bên trái.</p>
            </div>`;
        return;
    }

    khuVuc.innerHTML = ds.map(mon => `
        <div class="card-dangchuanbi glass-card bg-surface-container/40 rounded-xl p-4 transition-all hover:bg-surface-container/70" data-id="${mon.Id}">
            <div class="flex justify-between items-start gap-3">
                <div class="flex-1 min-w-0">
                    <h4 class="font-bold text-on-surface text-base leading-tight truncate">${escHtml(mon.TenSanPham || 'Không tên')}</h4>
                    <div class="flex items-center gap-3 mt-2 flex-wrap">
                        <span class="inline-flex items-center gap-1 text-xs font-semibold text-cyan-400 bg-cyan-500/10 border border-cyan-500/20 px-2.5 py-1 rounded-lg">
                            ×${mon.SoLuong || 1}
                        </span>
                        <span class="text-xs text-on-surface-variant">
                            Hóa đơn <strong class="text-primary">#${mon.HoaDonId}</strong>
                        </span>
                    </div>
                    ${mon.ThuocTinhThem ? `
                    <div class="mt-2.5 flex items-start gap-1.5">
                        <img src="img/click_3d.png" alt="Note" class="w-4 h-4 object-cover rounded-sm opacity-60 mt-0.5 shrink-0">
                        <span class="text-xs text-on-surface-variant/80 italic leading-relaxed">${escHtml(mon.ThuocTinhThem)}</span>
                    </div>` : ''}
                </div>
                <div class="shrink-0">
                    <button class="btnDaPhucVu btn btn-success px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-wider shadow-[0_4px_15px_rgba(74,222,128,0.2)] hover:shadow-[0_6px_20px_rgba(74,222,128,0.35)] transition-all active:scale-95 flex items-center gap-1.5 cursor-pointer" onclick="daPhucVu(${mon.Id})" style="background: linear-gradient(135deg, #4ade80, #86efac); color: #0a1628; border: none;">
                        ✅ Đã phục vụ
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

// ---- HÀNH ĐỘNG ----

async function nhanCheBien(id) {
    try {
        const res = await fetch(`${API}/chitiethoadon/${id}/trangthai`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ TrangThaiMon: 'DangChuanBi' })
        });
        if (!res.ok) {
            const data = await res.json().catch(() => ({}));
            hienThiThongBao(`❌ ${data.thongBao || 'Không thể cập nhật trạng thái!'}`, 'error');
            return;
        }
        hienThiThongBao('✅ Đã nhận chế biến!', 'success');
        taiTatCa();
    } catch (err) {
        console.error('Lỗi nhận chế biến:', err);
        hienThiThongBao('❌ Lỗi kết nối server!', 'error');
    }
}

async function daPhucVu(id) {
    try {
        const res = await fetch(`${API}/chitiethoadon/${id}/trangthai`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ TrangThaiMon: 'DaPhucVu' })
        });
        if (!res.ok) {
            const data = await res.json().catch(() => ({}));
            hienThiThongBao(`❌ ${data.thongBao || 'Không thể cập nhật trạng thái!'}`, 'error');
            return;
        }
        hienThiThongBao('✅ Đã đánh dấu phục vụ!', 'success');
        taiTatCa();
    } catch (err) {
        console.error('Lỗi đánh dấu phục vụ:', err);
        hienThiThongBao('❌ Lỗi kết nối server!', 'error');
    }
}

// ---- TIỆN ÍCH ----

// Escape HTML để tránh XSS
function escHtml(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

// Hiển thị thông báo nội tuyến + Toast notification
function hienThiThongBao(noiDung, loai = 'success') {
    // Inline notification
    const kv = document.getElementById('thongBaoKhuVuc');
    if (kv) {
        kv.innerHTML = `<div class="alert alert-${loai}">${noiDung}</div>`;
        setTimeout(() => kv.innerHTML = '', 5000);
    }
    // Toast notification (UX cao cấp)
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
    }, 5000);
}

// Định dạng tiền VNĐ
function formatTien(so) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(so);
}
