// ============================================================
// TAIKHOAN.JS — Logic trang Quản Lý Tài Khoản (taikhoan.html)
// ------------------------------------------------------------
// Chức năng (Use Case: "Quản lý tài khoản" — chỉ QuanTri):
//   1. Tải danh sách tài khoản từ /api/taikhoan
//   2. Thêm tài khoản mới (NhanVien hoặc QuanTri)
//   3. Reset mật khẩu cho tài khoản
//   4. Xoá tài khoản (không xoá chính mình / QuanTri cuối)
// ============================================================

let idDangReset = null;       // Id tài khoản đang reset mật khẩu
let tenUserDangXoa = '';      // Tên user hiện tại (để chặn tự xoá chính mình)

// ---------- KHỞI ĐỘNG ----------
document.addEventListener('DOMContentLoaded', () => {
    taiDanhSach();
    tenUserDangXoa = sessionStorage.getItem('vst_user') || '';
});

// ---------- 1. TẢI DANH SÁCH TÀI KHOẢN ----------
async function taiDanhSach() {
    document.getElementById('bangTaiKhoan').innerHTML =
        '<tr><td colspan="5" class="text-center" style="padding:2rem;"><div class="spinner"></div></td></tr>';
    try {
        // GET /api/taikhoan — backend đã chặn NhanVien trả 403
        const res = await apiFetch(`${API}/taikhoan`);
        if (res.status === 403) {
            document.getElementById('bangTaiKhoan').innerHTML =
                '<tr><td colspan="5"><div class="alert alert-error">⛔ Bạn không có quyền xem trang này!</div></td></tr>';
            return;
        }
        if (!res.ok) throw new Error('Lỗi server');
        const ds = await res.json();
        hienThiBang(ds);
    } catch {
        document.getElementById('bangTaiKhoan').innerHTML =
            '<tr><td colspan="5"><div class="alert alert-error">⚠️ Lỗi kết nối server!</div></td></tr>';
    }
}

// ---------- 2. HIỂN THỊ BẢNG TÀI KHOẢN ----------
function hienThiBang(ds) {
    const tbody = document.getElementById('bangTaiKhoan');
    if (!ds.length) {
        tbody.innerHTML = '<tr><td colspan="5"><div class="empty-state"><span class="empty-icon">👤</span><p>Chưa có tài khoản nào.</p></div></td></tr>';
        return;
    }

    tbody.innerHTML = ds.map(nd => {
        // Hiển thị vai trò: QuanTri → badge vàng, NhanVien → badge xám
        const laQuanTri = nd.VaiTro === 'QuanTri';
        const badgeVaiTro = laQuanTri
            ? '<span class="badge badge-dadat">👑 Quản trị</span>'
            : '<span class="badge badge-trong">👤 Nhân viên</span>';

        // Nút xoá: ẩn nếu đây là chính mình (tránh tự xoá)
        const laChinhMinh = nd.TenDangNhap === tenUserDangXoa;
        const cotThaoTac = laChinhMinh
            ? '<span class="text-on-surface-variant/40 text-xs italic">Bạn</span>'
            : `
                <div class="flex gap-2 justify-center">
                    <button class="btn btn-sm btn-info" onclick="moModalReset(${nd.Id}, '${nd.TenDangNhap.replace(/'/g, "\\\\'")}')" title="Reset mật khẩu">
                        🔑 Reset
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="xoaTaiKhoan(${nd.Id}, '${nd.TenDangNhap.replace(/'/g, "\\\\'")}')" title="Xoá tài khoản">
                        🗑️ Xoá
                    </button>
                </div>`;

        return `
        <tr>
            <td class="px-6 py-4 font-bold text-primary">#${nd.Id}</td>
            <td class="px-6 py-4 font-medium">@${nd.TenDangNhap}</td>
            <td class="px-6 py-4">${badgeVaiTro}</td>
            <td class="px-6 py-4 text-on-surface-variant">${formatThoiGian(nd.NgayTao)}</td>
            <td class="px-6 py-4 text-center">${cotThaoTac}</td>
        </tr>`;
    }).join('');
}

// ---------- 3. MODAL THÊM TÀI KHOẢN ----------
function moModalThem() {
    document.getElementById('txtTenDangNhap').value = '';
    document.getElementById('txtMatKhau').value = '';
    document.getElementById('cboVaiTro').value = 'NhanVien';   // Mặc định NhanVien
    moModal('modalTaiKhoan');
}

// ---------- 4. LƯU TÀI KHOẢN MỚI ----------
async function luuTaiKhoan() {
    const ten = document.getElementById('txtTenDangNhap').value.trim();
    const mk = document.getElementById('txtMatKhau').value.trim();
    const vt = document.getElementById('cboVaiTro').value;

    // Validate client
    if (!ten || ten.length < 3) {
        hienThiThongBao('Tên đăng nhập phải có ít nhất 3 ký tự!', 'error');
        document.getElementById('txtTenDangNhap').focus();
        return;
    }
    if (!mk || mk.length < 4) {
        hienThiThongBao('Mật khẩu phải có ít nhất 4 ký tự!', 'error');
        document.getElementById('txtMatKhau').focus();
        return;
    }

    try {
        const res = await apiFetch(`${API}/taikhoan`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ TenDangNhap: ten, MatKhau: mk, VaiTro: vt })
        });
        const data = await res.json();
        if (res.ok) {
            dongModal('modalTaiKhoan');
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSach();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 5. MODAL RESET MẬT KHẨU ----------
function moModalReset(id, tenDangNhap) {
    idDangReset = id;
    document.getElementById('resetTenUser').textContent = '@' + tenDangNhap;
    document.getElementById('txtMatKhauMoi').value = '';
    moModal('modalReset');
}

// ---------- 6. XÁC NHẬN RESET MẬT KHẨU ----------
async function xacNhanReset() {
    if (idDangReset === null) return;
    const mkMoi = document.getElementById('txtMatKhauMoi').value.trim();
    if (!mkMoi || mkMoi.length < 4) {
        hienThiThongBao('Mật khẩu mới phải có ít nhất 4 ký tự!', 'error');
        return;
    }

    try {
        const res = await apiFetch(`${API}/taikhoan/${idDangReset}/matkhau`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ MatKhauMoi: mkMoi })
        });
        const data = await res.json();
        if (res.ok) {
            dongModal('modalReset');
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            idDangReset = null;
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 7. XOÁ TÀI KHOẢN ----------
async function xoaTaiKhoan(id, tenDangNhap) {
    if (!confirm(`Xác nhận xoá tài khoản "@${tenDangNhap}"?\nHành động này không thể hoàn tác!`)) return;
    try {
        const res = await apiFetch(`${API}/taikhoan/${id}`, { method: 'DELETE' });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSach();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}
