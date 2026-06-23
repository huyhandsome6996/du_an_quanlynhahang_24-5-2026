/**
 * ================================================================
 * COMPONENT LOADER - Hệ thống tải thành phần HTML riêng biệt
 * ================================================================
 * Mục đích: Cho phép tách giao diện HTML lớn thành nhiều file nhỏ
 *          để dễ quản lý, bảo trì và học tập.
 * 
 * Cách dùng:
 *   await loadComponent('id-phan-noi-dung', 'duong/dan/toi/file.html');
 *   await loadScript('js/file.js');
 * 
 * Lưu ý: Chỉ dùng sau khi DOM đã sẵn sàng (trong <script> ở cuối body)
 * ================================================================
 */

/**
 * Tải nội dung HTML từ file component và chèn vào phần tử chỉ định
 * @param {string} elementId - ID của thẻ div chứa placeholder
 * @param {string} componentPath - Đường dẫn tới file component HTML
 * @returns {Promise<boolean>} - true nếu tải thành công
 */
async function loadComponent(elementId, componentPath) {
    try {
        const res = await fetch(componentPath);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const html = await res.text();
        const el = document.getElementById(elementId);
        if (el) {
            el.innerHTML = html;
        }
        return true;
    } catch (e) {
        console.error(`[Component Loader] Lỗi tải component: ${componentPath}`, e);
        return false;
    }
}

/**
 * Tải file JavaScript động (sau khi component đã nạp xong)
 * @param {string} src - Đường dẫn tới file JS
 * @returns {Promise<void>}
 */
async function loadScript(src) {
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = src;
        script.onload = resolve;
        script.onerror = () => reject(new Error(`Không tải được: ${src}`));
        document.body.appendChild(script);
    });
}

/**
 * Khôi phục trạng thái sidebar (mở rộng/thu gọn) từ sessionStorage
 * Được gọi SAU khi sidebar component đã được nạp
 */
function initSidebarState() {
    const sidebar = document.getElementById('mainSidebar');
    if (sidebar && sessionStorage.getItem('sidebarExpanded') === 'true') {
        sidebar.classList.add('expanded');
    }
}

/**
 * Đánh dấu menu item đang active trên sidebar
 * @param {string} href - Đường dẫn của trang hiện tại (VD: 'index.html')
 */
function setActiveSidebarItem(href) {
    const sidebar = document.getElementById('mainSidebar');
    if (!sidebar) return;
    const links = sidebar.querySelectorAll('a.nav-btn');
    links.forEach(link => {
        link.classList.remove('active');
        link.classList.add('text-on-surface-variant/50', 'hover:text-on-surface');
        if (link.getAttribute('href') === href) {
            link.classList.add('active');
            link.classList.remove('text-on-surface-variant/50');
        }
    });
}

/**
 * Hàm mở Modal - Hiệu ứng scale mượt mà
 * Dùng chung cho tất cả các modal trong hệ thống
 * @param {string} id - ID của modal overlay
 */
window.moModal = function(id) {
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
};

/**
 * Hàm đóng Modal - Hiệu ứng scale thu nhỏ mượt mà
 * @param {string} id - ID của modal overlay
 */
window.dongModal = function(id) {
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
};
