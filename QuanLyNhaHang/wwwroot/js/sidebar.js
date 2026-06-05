document.addEventListener('DOMContentLoaded', () => {
    const sidebar = document.querySelector('.sidebar');
    if (!sidebar) return;

    // Khôi phục trạng thái từ sessionStorage
    const isExpanded = sessionStorage.getItem('sidebarExpanded') === 'true';
    if (isExpanded) {
        sidebar.classList.add('expanded');
    }

    // Toggle khi click vào sidebar (khoảng trống)
    sidebar.addEventListener('click', (e) => {
        // Nếu click vào thẻ a hoặc button, không làm thay đổi trạng thái của sidebar
        if (e.target.closest('a') || e.target.closest('button')) {
            return;
        }

        // Nếu click vào khoảng trống trong sidebar
        const expandedNow = sidebar.classList.toggle('expanded');
        sessionStorage.setItem('sidebarExpanded', expandedNow);
        e.stopPropagation(); // Ngăn sự kiện lan ra ngoài document
    });

    // Thu nhỏ khi click ra ngoài sidebar
    document.addEventListener('click', (e) => {
        if (!sidebar.contains(e.target)) {
            sidebar.classList.remove('expanded');
            sessionStorage.setItem('sidebarExpanded', 'false');
        }
    });
});
