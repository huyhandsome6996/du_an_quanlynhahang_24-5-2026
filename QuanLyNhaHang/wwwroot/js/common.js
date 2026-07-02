// ============================================================
// COMMON.JS — Các hàm dùng chung cho tất cả các trang
// (formatTien, formatThoiGian, moModal, dongModal,
//  hienToast, hienThiThongBao)
// ============================================================

// Địa chỉ API — chỉ cần đổi ở đây nếu đổi port
const API = 'http://localhost:5000/api';

// ---------- 1. ĐỊNH DẠNG TIỀN & THỜI GIAN ----------
function formatTien(so) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(so || 0);
}

function formatThoiGian(chuoi) {
    if (!chuoi) return '-';
    return new Date(chuoi).toLocaleString('vi-VN');
}

// ---------- 2. MODAL: mở / đóng với hiệu ứng scale ----------
function moModal(id) {
    const modal = document.getElementById(id);
    if (!modal) return;
    modal.classList.add('show');
    // Sau 50ms mới scale lên để có hiệu ứng mượt
    setTimeout(() => {
        const card = modal.firstElementChild;
        if (card) {
            card.classList.remove('scale-95');
            card.classList.add('scale-100');
        }
    }, 50);
}

function dongModal(id) {
    const modal = document.getElementById(id);
    if (!modal) return;
    const card = modal.firstElementChild;
    if (card) {
        card.classList.remove('scale-100');
        card.classList.add('scale-95');
    }
    // Đợi hiệu ứng xong mới ẩn hẳn
    setTimeout(() => modal.classList.remove('show'), 150);
}

// ---------- 3. TOAST: thông báo nổi góc trên phải ----------
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

    // Tự ẩn sau 4 giây
    setTimeout(() => {
        toast.classList.add('toast-out');
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

// ---------- 4. THÔNG BÁO KÉP: nội tuyến + toast ----------
function hienThiThongBao(noiDung, loai = 'success') {
    // 1) Hiện thông báo nội tuyến trong khu vực #thongBaoKhuVuc (nếu có)
    const kv = document.getElementById('thongBaoKhuVuc');
    if (kv) {
        kv.innerHTML = `<div class="alert alert-${loai}">${noiDung}</div>`;
        setTimeout(() => kv.innerHTML = '', 4000);
    }
    // 2) Hiện toast nổi góc phải
    hienToast(noiDung, loai);
}

// ---------- 5. SIDEBAR: toggle mở rộng ----------
document.addEventListener('DOMContentLoaded', () => {
    const sidebar = document.getElementById('mainSidebar');
    if (!sidebar) return;

    // Khôi phục trạng thái từ sessionStorage
    if (sessionStorage.getItem('sidebarExpanded') === 'true') {
        sidebar.classList.add('expanded');
    }

    // Click vào khoảng trống trong sidebar → toggle mở rộng
    sidebar.addEventListener('click', (e) => {
        if (e.target.closest('a') || e.target.closest('button')) return;
        const expanded = sidebar.classList.toggle('expanded');
        sessionStorage.setItem('sidebarExpanded', expanded);
        e.stopPropagation();
    });

    // Click ra ngoài sidebar → thu gọn
    document.addEventListener('click', (e) => {
        if (!sidebar.contains(e.target)) {
            sidebar.classList.remove('expanded');
            sessionStorage.setItem('sidebarExpanded', 'false');
        }
    });
});
