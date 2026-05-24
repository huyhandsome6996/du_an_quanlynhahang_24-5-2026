// ============================================================
// MENU.JS - Logic trang Quản Lý Thực Đơn (menu.html)
// ============================================================

const API = 'http://localhost:5000/api';
let danhSachSanPham = [];   // Cache toàn bộ sản phẩm

// ---- Khởi động ----
document.addEventListener('DOMContentLoaded', () => {
    taiDanhSach();
    capNhatGiaoDienLoai(); // Hiển thị ghi chú phụ phí ban đầu
});

// Lấy danh sách sản phẩm từ API
async function taiDanhSach() {
    document.getElementById('bangSanPham').innerHTML = `
        <tr><td colspan="6" class="text-center" style="padding:2rem;">
            <div class="spinner"></div>
        </td></tr>`;
    try {
        const res = await fetch(`${API}/sanpham`);
        danhSachSanPham = await res.json();
        locTheoLoai();
    } catch (err) {
        document.getElementById('bangSanPham').innerHTML = `
            <tr><td colspan="6" class="text-center">
                <div class="alert alert-error">⚠️ Lỗi kết nối server!</div>
            </td></tr>`;
    }
}

// Lọc theo loại và hiển thị bảng
function locTheoLoai() {
    const loai = document.getElementById('locLoai').value;
    const dsLoc = loai ? danhSachSanPham.filter(sp => sp.Loai === loai) : danhSachSanPham;
    hienThiBang(dsLoc);
}

// Vẽ bảng sản phẩm
function hienThiBang(ds) {
    document.getElementById('tongSoMon').textContent = ds.length;
    const tbody = document.getElementById('bangSanPham');

    if (ds.length === 0) {
        tbody.innerHTML = `
            <tr><td colspan="6">
                <div class="empty-state">
                    <span class="empty-icon">📋</span>
                    <p>Không có món nào. Hãy thêm món mới!</p>
                </div>
            </td></tr>`;
        return;
    }

    tbody.innerHTML = ds.map((sp, idx) => `
        <tr>
            <td>${idx + 1}</td>
            <td><strong>${sp.TenSanPham}</strong></td>
            <td>
                <span class="badge ${sp.Loai === 'ThucAn' ? 'badge-thucan' : 'badge-nuocuong'}">
                    ${sp.Loai === 'ThucAn' ? '🍖 Thức ăn' : '🥤 Nước uống'}
                </span>
            </td>
            <td class="text-chinh fw-bold">${formatTien(sp.GiaCoBan)}</td>
            <td>
                <span class="badge ${sp.DangBan ? 'badge-trong' : 'badge-cokhach'}">
                    ${sp.DangBan ? '✅ Đang bán' : '❌ Tạm ngừng'}
                </span>
            </td>
            <td>
                <div style="display:flex; gap:0.5rem;">
                    <button class="btn btn-sm btn-info" onclick="moModalSua(${sp.Id})">✏️ Sửa</button>
                    <button class="btn btn-sm btn-danger" onclick="xoa(${sp.Id}, '${sp.TenSanPham.replace(/'/g, "\\'")}')">🗑️ Xóa</button>
                </div>
            </td>
        </tr>`).join('');
}

// ---- MODAL THÊM / SỬA ----
function moModalThem() {
    document.getElementById('modalSanPhamTieuDe').textContent = 'Thêm Món Mới';
    document.getElementById('inputIdSanPham').value = '';
    document.getElementById('inputTenSanPham').value = '';
    document.getElementById('inputGiaCoBan').value = '';
    document.getElementById('selectLoai').value = 'ThucAn';
    document.getElementById('selectDangBan').value = 'true';
    capNhatGiaoDienLoai();
    document.getElementById('modalSanPham').classList.add('show');
}

function moModalSua(id) {
    const sp = danhSachSanPham.find(s => s.Id === id);
    if (!sp) return;

    document.getElementById('modalSanPhamTieuDe').textContent = `Sửa: ${sp.TenSanPham}`;
    document.getElementById('inputIdSanPham').value = sp.Id;
    document.getElementById('inputTenSanPham').value = sp.TenSanPham;
    document.getElementById('inputGiaCoBan').value = sp.GiaCoBan;
    document.getElementById('selectLoai').value = sp.Loai;
    document.getElementById('selectDangBan').value = sp.DangBan ? 'true' : 'false';
    capNhatGiaoDienLoai();
    document.getElementById('modalSanPham').classList.add('show');
}

// Cập nhật ghi chú phụ phí khi đổi loại (thể hiện Đa hình cho giảng viên hiểu)
function capNhatGiaoDienLoai() {
    const loai = document.getElementById('selectLoai').value;
    const moTa = document.getElementById('moTaPhuPhi');

    if (loai === 'ThucAn') {
        moTa.textContent = 'Thức ăn: Khách chọn "Phần lớn" → cộng thêm 50,000đ/phần.';
    } else {
        moTa.textContent = 'Nước uống: Khách chọn "Lon" → giá × 1.2 (đắt hơn 20%).';
    }
}

// Lưu sản phẩm (Thêm hoặc Sửa)
async function luuSanPham() {
    const id = document.getElementById('inputIdSanPham').value;
    const ten = document.getElementById('inputTenSanPham').value.trim();
    const gia = parseFloat(document.getElementById('inputGiaCoBan').value);
    const loai = document.getElementById('selectLoai').value;
    const dangBan = document.getElementById('selectDangBan').value === 'true';

    // --- Validation phía client ---
    if (!ten) {
        hienThiThongBao('Vui lòng nhập tên món!', 'error');
        document.getElementById('inputTenSanPham').focus();
        return;
    }
    if (isNaN(gia) || gia < 0) {
        hienThiThongBao('Giá cơ bản phải là số và không được âm!', 'error');
        document.getElementById('inputGiaCoBan').focus();
        return;
    }

    const payload = { TenSanPham: ten, GiaCoBan: gia, Loai: loai, DangBan: dangBan };
    const isEdit = id !== '';

    try {
        const url = isEdit ? `${API}/sanpham/${id}` : `${API}/sanpham`;
        const method = isEdit ? 'PUT' : 'POST';

        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        const data = await res.json();

        if (res.ok) {
            dongModal('modalSanPham');
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSach();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// Xóa sản phẩm
async function xoa(id, ten) {
    if (!confirm(`Xác nhận xóa món "${ten}"?`)) return;
    try {
        const res = await fetch(`${API}/sanpham/${id}`, { method: 'DELETE' });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSach();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---- Hàm tiện ích ----
function dongModal(id) {
    document.getElementById(id).classList.remove('show');
}

function hienThiThongBao(noiDung, loai = 'success') {
    const kv = document.getElementById('thongBaoKhuVuc');
    kv.innerHTML = `<div class="alert alert-${loai}">${noiDung}</div>`;
    setTimeout(() => kv.innerHTML = '', 4000);
}

function formatTien(so) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(so);
}

document.querySelectorAll('.modal-overlay').forEach(overlay => {
    overlay.addEventListener('click', e => {
        if (e.target === overlay) overlay.classList.remove('show');
    });
});
