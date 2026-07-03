// ============================================================
// SIDEBAR.JS — Tự động chèn sidebar vào các trang con
// ------------------------------------------------------------
// Tránh lặp lại 60 dòng code sidebar trong 5 file HTML.
// Mỗi trang chỉ cần gọi:
//   <div id="sidebar"></div>
//   <script src="js/sidebar.js"></script>
//   <script>chenSidebar('index');</script>
// ============================================================

/**
 * Chèn sidebar vào phần tử có id="sidebar".
 * @param {string} activePage — tên trang đang active
 *   ('index' | 'menu' | 'order' | 'lichsu' | 'baocao')
 */
function chenSidebar(activePage) {
    // Mảng cấu hình các nút điều hướng
    // Mỗi phần tử: href (đường dẫn), key (tên trang), icon (file ảnh), text (hiển thị)
    // Lấy quyền hạn từ bộ nhớ (mặc định coi như Nhân viên nếu lỗi)
    const role = sessionStorage.getItem('vst_role') || 'NhanVien';

    // Mảng cấu hình các nút điều hướng cơ bản (Ai cũng thấy)
    let menu = [
        { href: 'index.html',   key: 'index',   icon: 'table_3d.png',     text: 'Sơ đồ bàn' },
        { href: 'order.html',   key: 'order',   icon: 'pos_3d.png',       text: 'Gọi món' },
        { href: 'lichsu.html',  key: 'lichsu',  icon: 'bill_3d.png',      text: 'Lịch sử hóa đơn' }
    ];

    // Nếu là Quản trị viên thì nhét thêm chức năng Quản lý thực đơn và Báo cáo
    if (role === 'QuanTri') {
        menu.splice(1, 0, { href: 'menu.html',    key: 'menu',    icon: 'menu_book_3d.png', text: 'Thực đơn' });
        menu.push({ href: 'baocao.html',  key: 'baocao',  icon: 'money_3d.png',     text: 'Báo cáo' });
    }

    // Tạo HTML cho từng nút — dùng map + template literal
    // So sánh m.key === activePage để thêm class 'active' cho nút trang hiện tại
    const navHTML = menu.map(m => `
        <a href="${m.href}" class="nav-btn relative flex items-center group ${m.key === activePage ? 'active' : 'text-on-surface-variant/50 hover:text-on-surface transition-all'}" title="${m.text}">
            <img src="img/${m.icon}" alt="${m.text}"
                 class="w-9 h-9 object-cover rounded-[14px] bg-white/[0.04] border border-primary/20 p-1 group-hover:scale-110 transition-transform shadow-sm shrink-0">
            <span class="sidebar-text text-sm">${m.text}</span>
        </a>
    `).join('');   // join('') để nối mảng thành 1 chuỗi (không có dấu phẩy)

    // HTML sidebar hoàn chỉnh — bao gồm logo, đường kẻ, các nút, nút Đăng xuất
    const sidebarHTML = `
        <nav class="sidebar fixed left-0 top-0 h-screen z-50 flex flex-col py-6" id="mainSidebar">
            <!-- Logo -->
            <div class="relative group mb-6 px-3 w-full flex justify-center">
                <!-- Hiệu ứng phát sáng phía sau logo -->
                <div class="absolute inset-0 rounded-full bg-primary/15 blur-md pointer-events-none transition-all group-hover:bg-primary/25"></div>
                <img src="img/logo.png" alt="Logo"
                     class="relative w-12 h-12 rounded-full border-2 border-primary/30 object-cover shadow-[0_0_15px_rgba(212,168,83,0.2)] transition-all group-hover:scale-105 shrink-0">
                <span class="sidebar-text text-primary text-xl mt-2" style="font-family:'Playfair Display',serif;">Vua Sư Tử</span>
            </div>

            <!-- Đường kẻ trang trí -->
            <div class="w-full h-[1px] bg-gradient-to-r from-transparent via-primary/20 to-transparent mb-4"></div>

            ${navHTML}

            <!-- Khoảng trống + nút Đăng xuất ở cuối -->
            <div class="flex-1"></div>
            <!-- Nút Đăng xuất: sessionStorage.clear() → xoá trạng thái đăng nhập -->
            <button onclick="event.stopPropagation(); sessionStorage.clear(); window.location.href='login.html';"
                    class="nav-btn relative flex items-center group text-on-surface-variant/40 hover:text-red-400 transition-all"
                    title="Đăng xuất">
                <img src="img/logout_3d.png" alt="Đăng xuất"
                     class="w-9 h-9 object-cover rounded-[14px] bg-red-500/10 border border-red-500/20 p-1 group-hover:scale-110 transition-transform shadow-sm shrink-0">
                <span class="sidebar-text text-sm text-red-500">Đăng xuất</span>
            </button>
        </nav>
    `;

    // Chèn HTML vào placeholder có id="sidebar"
    const placeholder = document.getElementById('sidebar');
    if (placeholder) {
        placeholder.innerHTML = sidebarHTML;
    }
}
