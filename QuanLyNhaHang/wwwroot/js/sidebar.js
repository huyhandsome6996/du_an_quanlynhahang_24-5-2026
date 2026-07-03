// ============================================================
// SIDEBAR.JS — Tự động chèn sidebar vào các trang con
// ------------------------------------------------------------
// Tránh lặp lại 60 dòng code sidebar trong 5 file HTML.
// Mỗi trang chỉ cần gọi:
//   <div id="sidebar"></div>
//   <script src="js/sidebar.js"></script>
//   <script>chenSidebar('index');</script>
//
// PHÂN QUYỀN: Menu được lọc theo vai trò của user:
//   - Cả 2 vai trò: Sơ đồ bàn, Gọi món, Lịch sử hóa đơn
//   - Chỉ QuanTri : Thực đơn, Báo cáo, Quản lý tài khoản
// ============================================================

/**
 * Chèn sidebar vào phần tử có id="sidebar".
 * @param {string} activePage — tên trang đang active
 *   ('index' | 'menu' | 'order' | 'lichsu' | 'baocao' | 'taikhoan')
 */
function chenSidebar(activePage) {
    // Mảng cấu hình các nút điều hướng — mỗi nút có thể có thuộc tính `quyenTri`
    // để quy định: chỉ QuanTri mới thấy nút này.
    // Mặc định (không có `quyenTri`) là cả 2 vai trò đều thấy.
    const tatCaMenu = [
        { href: 'index.html',   key: 'index',   icon: 'table_3d.png',     text: 'Sơ đồ bàn' },
        { href: 'order.html',   key: 'order',   icon: 'pos_3d.png',       text: 'Gọi món' },
        { href: 'lichsu.html',  key: 'lichsu',  icon: 'bill_3d.png',      text: 'Lịch sử hóa đơn' },
        { href: 'menu.html',    key: 'menu',    icon: 'menu_book_3d.png', text: 'Thực đơn',         quyenTri: true },
        { href: 'baocao.html',  key: 'baocao',  icon: 'money_3d.png',     text: 'Báo cáo',           quyenTri: true },
        { href: 'taikhoan.html',key: 'taikhoan',icon: 'user_3d.png',      text: 'Quản lý tài khoản', quyenTri: true },
    ];

    // Lọc menu theo vai trò: nếu nút có quyenTri=true → chỉ hiện khi laQuanTri()
    const menu = tatCaMenu.filter(m => !m.quyenTri || laQuanTri());

    // Tạo HTML cho từng nút — dùng map + template literal
    // So sánh m.key === activePage để thêm class 'active' cho nút trang hiện tại
    const navHTML = menu.map(m => `
        <a href="${m.href}" class="nav-btn relative flex items-center group ${m.key === activePage ? 'active' : 'text-on-surface-variant/50 hover:text-on-surface transition-all'}" title="${m.text}">
            <img src="img/${m.icon}" alt="${m.text}"
                 class="w-9 h-9 object-cover rounded-[14px] bg-white/[0.04] border border-primary/20 p-1 group-hover:scale-110 transition-transform shadow-sm shrink-0">
            <span class="sidebar-text text-sm">${m.text}</span>
        </a>
    `).join('');   // join('') để nối mảng thành 1 chuỗi (không có dấu phẩy)

    // Hiển thị vai trò + tên user ở đầu sidebar (để user biết vai trò của mình)
    const vaiTro = layVaiTro();
    const tenUser = sessionStorage.getItem('vst_user') || '';
    // Quy ước hiển thị: QuanTri → "Quản trị", NhanVien → "Nhân viên"
    const hienThiVaiTro = vaiTro === 'QuanTri' ? 'Quản trị' : 'Nhân viên';
    // Class màu: QuanTri → vàng (primary), NhanVien → xám nhạt
    const mauVaiTro = vaiTro === 'QuanTri' ? 'text-primary' : 'text-on-surface-variant';

    // HTML sidebar hoàn chỉnh — bao gồm logo, đường kẻ, các nút, vai trò, nút Đăng xuất
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

            <!-- Khoảng trống + thông tin user + nút Đăng xuất ở cuối -->
            <div class="flex-1"></div>

            <!-- Hiển thị vai trò + tên user (chỉ hiện khi sidebar expanded) -->
            <div class="sidebar-text px-3 mb-3 text-center">
                <div class="text-[10px] uppercase tracking-widest text-on-surface-variant/50">Đăng nhập</div>
                <div class="text-sm font-bold ${mauVaiTro}">${hienThiVaiTro}</div>
                <div class="text-xs text-on-surface-variant/70 truncate">@${tenUser}</div>
            </div>

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
