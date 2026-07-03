// ============================================================
// COMMON.JS — Các hàm DÙNG CHUNG cho tất cả các trang
// ------------------------------------------------------------
// Bao gồm:
//   1. formatTien       — Định dạng số tiền (15.000đ)
//   2. formatThoiGian   — Định dạng ngày giờ theo vi-VN
//   3. moModal / dongModal — Mở/đóng modal có hiệu ứng scale
//   4. hienToast        — Thông báo nổi góc trên phải
//   5. hienThiThongBao  — Thông báo kép (nội tuyến + toast)
//   6. Auto-toggle sidebar khi click
// ============================================================

// Địa chỉ API — chỉ cần đổi ở đây nếu đổi port
// (mặc định server ASP.NET chạy ở http://localhost:5000)
const API = 'http://localhost:5000/api';

// ---------- 0. PHÂN QUYỀN (Role-Based Access Control) ----------

/**
 * Lấy vai trò của user hiện tại từ sessionStorage.
 * Trả về 'QuanTri' hoặc 'NhanVien'. Mặc định 'NhanVien' nếu chưa có.
 */
function layVaiTro() {
    return sessionStorage.getItem('vst_vai_tro') || 'NhanVien';
}

/**
 * Trả về true nếu user hiện tại là Quản trị viên.
 */
function laQuanTri() {
    return layVaiTro() === 'QuanTri';
}

/**
 * Wrapper fetch() tự gắn header "X-Vai-Tro" cho mọi lời gọi API.
 * Backend (PhanQuyen.cs) đọc header này để quyết định cho phép hay chặn.
 *
 * Cách dùng: thay fetch(...) bằng apiFetch(...), cú pháp y hệt.
 *   const res = await apiFetch(`${API}/sanpham`);
 *   const res = await apiFetch(`${API}/sanpham`, {
 *       method: 'POST',
 *       headers: { 'Content-Type': 'application/json' },
 *       body: JSON.stringify(payload)
 *   });
 *
 * Header X-Vai-Tro và X-Ten-Dang-Nhap được tự động thêm vào.
 * Nếu options.headers có Content-Type, sẽ được giữ nguyên.
 */
async function apiFetch(url, options = {}) {
    // Clone options để không mutate object gốc
    const opts = { ...options };
    // Đảm bảo có object headers
    opts.headers = { ...(opts.headers || {}) };
    // Gắn header phân quyền
    opts.headers['X-Vai-Tro'] = layVaiTro();
    opts.headers['X-Ten-Dang-Nhap'] = sessionStorage.getItem('vst_user') || '';
    return fetch(url, opts);
}

// ---------- 1. ĐỊNH DẠNG TIỀN & THỜI GIAN ----------

/**
 * Định dạng số thành chuỗi tiền tệ VNĐ.
 * Ví dụ: 45000 → "45.000 ₫"
 * @param {number} so - Số tiền cần format
 * @returns {string} Chuỗi đã format
 */
function formatTien(so) {
    // Intl.NumberFormat — API built-in của JavaScript để format số theo locale
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(so || 0);   // so || 0 — nếu so là null/undefined → dùng 0
}

/**
 * Định dạng chuỗi ISO datetime thành chuỗi ngày giờ vi-VN.
 * Ví dụ: "2026-06-15T11:30:00" → "15/06/2026, 11:30:00"
 * @param {string} chuoi - Chuỗi datetime từ API
 * @returns {string} Chuỗi đã format, hoặc '-' nếu null
 */
function formatThoiGian(chuoi) {
    // Nếu chuỗi rỗng → trả '-' để hiển thị gạch ngang
    if (!chuoi) return '-';
    // new Date(chuoi) → parse ISO → Date object
    // toLocaleString('vi-VN') → format theo kiểu Việt Nam
    return new Date(chuoi).toLocaleString('vi-VN');
}

// ---------- 2. MODAL: mở / đóng với hiệu ứng scale ----------

/**
 * Mở modal có hiệu ứng scale (nhỏ → to).
 * @param {string} id - Id của phần tử modal (vd: 'modalSanPham')
 */
function moModal(id) {
    // Tìm phần tử modal theo id
    const modal = document.getElementById(id);
    if (!modal) return;   // Không tìm thấy → thoát

    // Thêm class 'show' để modal hiện ra (display: flex)
    modal.classList.add('show');

    // Sau 50ms mới scale lên để có hiệu ứng mượt
    // (nếu scale ngay khi show sẽ bị nhiễu do display vừa đổi)
    setTimeout(() => {
        // firstElementChild = phần tử con đầu tiên (thường là card modal)
        const card = modal.firstElementChild;
        if (card) {
            // Đổi class scale-95 → scale-100 để phóng to
            card.classList.remove('scale-95');
            card.classList.add('scale-100');
        }
    }, 50);
}

/**
 * Đóng modal có hiệu ứng scale (to → nhỏ → ẩn).
 * @param {string} id - Id của phần tử modal
 */
function dongModal(id) {
    const modal = document.getElementById(id);
    if (!modal) return;
    const card = modal.firstElementChild;
    if (card) {
        // Scale nhỏ lại trước
        card.classList.remove('scale-100');
        card.classList.add('scale-95');
    }
    // Đợi 150ms cho hiệu ứng scale xong mới ẩn hẳn
    setTimeout(() => modal.classList.remove('show'), 150);
}

// ---------- 3. TOAST: thông báo nổi góc trên phải ----------

/**
 * Hiện thông báo toast (góc trên phải, tự ẩn sau 4 giây).
 * @param {string} noiDung - Nội dung thông báo
 * @param {string} loai    - 'success' (xanh) hoặc 'error' (đỏ)
 */
function hienToast(noiDung, loai = 'success') {
    // Tìm container đã có — nếu chưa có thì tạo mới
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    // Tạo phần tử toast
    const toast = document.createElement('div');
    toast.className = `toast toast-${loai}`;   // toast-success hoặc toast-error
    // Icon khác nhau tùy loại
    const icon = loai === 'success' ? '✅' : '❌';
    // innerHTML để chèn icon + text
    toast.innerHTML = `<span style="font-size:1.1rem;">${icon}</span><span>${noiDung}</span>`;
    container.appendChild(toast);

    // Tự ẩn sau 4 giây
    setTimeout(() => {
        toast.classList.add('toast-out');   // Thêm class để chạy animation fade out
        setTimeout(() => toast.remove(), 300);   // Đợi 300ms animation xong mới remove
    }, 4000);
}

// ---------- 4. THÔNG BÁO KÉP: nội tuyến + toast ----------

/**
 * Hiện thông báo 2 chỗ cùng lúc:
 *   1. Nội tuyến trong khu vực #thongBaoKhuVuc (nếu có)
 *   2. Toast nổi góc phải
 * @param {string} noiDung - Nội dung
 * @param {string} loai    - 'success' hoặc 'error'
 */
function hienThiThongBao(noiDung, loai = 'success') {
    // 1) Hiện thông báo nội tuyến (nếu trang có #thongBaoKhuVuc)
    const kv = document.getElementById('thongBaoKhuVuc');
    if (kv) {
        kv.innerHTML = `<div class="alert alert-${loai}">${noiDung}</div>`;
        // Tự xoá sau 4 giây
        setTimeout(() => kv.innerHTML = '', 4000);
    }
    // 2) Hiện toast
    hienToast(noiDung, loai);
}

// ---------- 5. SIDEBAR: toggle mở rộng ----------

// Khi trang tải xong (DOMContentLoaded) → gắn sự kiện cho sidebar
document.addEventListener('DOMContentLoaded', () => {
    // Tìm sidebar (id="mainSidebar" được tạo bởi sidebar.js)
    const sidebar = document.getElementById('mainSidebar');
    if (!sidebar) return;   // Trang login không có sidebar → thoát

    // Khôi phục trạng thái từ sessionStorage (để giữ trạng thái khi chuyển trang)
    if (sessionStorage.getItem('sidebarExpanded') === 'true') {
        sidebar.classList.add('expanded');   // Đang mở rộng → giữ nguyên
    }

    // Click vào khoảng trống trong sidebar → toggle mở rộng/thu gọn
    sidebar.addEventListener('click', (e) => {
        // Nếu click vào link hoặc button → bỏ qua (để link hoạt động bình thường)
        if (e.target.closest('a') || e.target.closest('button')) return;
        // toggle: nếu có class expanded → bỏ, không có → thêm
        const expanded = sidebar.classList.toggle('expanded');
        // Lưu trạng thái vào sessionStorage
        sessionStorage.setItem('sidebarExpanded', expanded);
        // stopPropagation → không bubble lên document (không trigger click ngoài)
        e.stopPropagation();
    });

    // Click ra ngoài sidebar → thu gọn lại
    document.addEventListener('click', (e) => {
        // Nếu click không nằm trong sidebar → xóa class expanded
        if (!sidebar.contains(e.target)) {
            sidebar.classList.remove('expanded');
            sessionStorage.setItem('sidebarExpanded', 'false');
        }
    });
});
