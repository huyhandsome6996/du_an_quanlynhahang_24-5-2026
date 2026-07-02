// ============================================================
// MENU.JS — Logic trang Thực Đơn (menu.html)
//
// Chức năng:
//   1. Tải danh sách món từ API
//   2. Lọc theo loại (Thức ăn / Nước uống)
//   3. Tìm kiếm theo tên
//   4. Thêm / Sửa / Xóa món (kèm upload ảnh Base64)
//   5. Hiển thị ghi chú đa hình OOP (Phụ phí Phần lớn / Lon)
// ============================================================

let danhSachSanPham = [];   // Cache toàn bộ sản phẩm

// ---------- KHỞI ĐỘNG ----------
document.addEventListener('DOMContentLoaded', () => {
    taiDanhSach();
    capNhatGiaoDienLoai(); // Hiện ghi chú phụ phí lần đầu
});

// ---------- 1. TẢI DANH SÁCH MÓN ----------
async function taiDanhSach() {
    document.getElementById('bangSanPham').innerHTML =
        '<div class="col-span-full flex justify-center py-12"><div class="spinner"></div></div>';
    try {
        const res = await fetch(`${API}/sanpham`);
        danhSachSanPham = await res.json();
        locTheoLoai();
    } catch {
        document.getElementById('bangSanPham').innerHTML =
            '<div class="col-span-full"><div class="alert alert-error">⚠️ Lỗi kết nối server!</div></div>';
    }
}

// ---------- 2. LỌC THEO LOẠI + TÌM KIẾM ----------
function locTheoLoai() {
    const loai = document.getElementById('cboLocLoai').value;
    let dsLoc = loai ? danhSachSanPham.filter(sp => sp.Loai === loai) : danhSachSanPham;

    // Áp dụng thêm tìm kiếm nếu có
    const tuKhoa = document.getElementById('txtTimKiemMon')?.value.trim().toLowerCase();
    if (tuKhoa) {
        dsLoc = dsLoc.filter(sp => sp.TenSanPham.toLowerCase().includes(tuKhoa));
    }
    hienThiBang(dsLoc);
}

function timKiemMon() { locTheoLoai(); }

// ---------- 3. HIỂN THỊ LƯỚI MÓN ĂN ----------
function hienThiBang(ds) {
    document.getElementById('tongSoMon').textContent = ds.length;
    const grid = document.getElementById('bangSanPham');

    if (ds.length === 0) {
        grid.innerHTML = `
            <div class="col-span-full">
                <div class="empty-state">
                    <span class="empty-icon">📋</span>
                    <p>Không có món nào. Hãy thêm món mới!</p>
                </div>
            </div>`;
        return;
    }

    grid.innerHTML = ds.map(sp => {
        const imageSrc = sp.HinhAnh || 'img/logo.png';
        return `
        <div class="glass-card product-card rounded-2xl overflow-hidden cursor-pointer flex flex-col transition-all hover:shadow-xl hover:-translate-y-1" onclick="moModalSua(${sp.Id})">
            <div class="relative h-44 w-full bg-surface-container-highest overflow-hidden border-b border-white/5">
                <img src="${imageSrc}" alt="${sp.TenSanPham}" class="w-full h-full object-cover transition-transform duration-500 hover:scale-105" onerror="this.src='img/logo.png'">
                <span class="absolute top-3 right-3 badge ${sp.DangBan ? 'badge-trong' : 'badge-cokhach'}">
                    ${sp.DangBan ? '✅ Đang bán' : '❌ Ngừng'}
                </span>
                <span class="absolute bottom-3 left-3 badge ${sp.Loai === 'ThucAn' ? 'badge-thucan' : 'badge-nuocuong'}">
                    ${sp.Loai === 'ThucAn' ? '🍖 Thức ăn' : '🥤 Nước uống'}
                </span>
            </div>
            <div class="p-5 flex-1 flex flex-col">
                <h3 class="font-bold text-base text-on-surface mb-1 line-clamp-1">${sp.TenSanPham}</h3>
                <p class="text-primary font-bold text-sm mb-4">${formatTien(sp.GiaCoBan)}</p>
                <div class="flex gap-2 justify-end mt-auto pt-4 border-t border-white/5">
                    <button class="btn btn-sm btn-info flex-1" onclick="event.stopPropagation(); moModalSua(${sp.Id})">✏️ Sửa</button>
                    <button class="btn btn-sm btn-danger flex-1" onclick="event.stopPropagation(); xoa(${sp.Id}, '${sp.TenSanPham.replace(/'/g, "\\\'")}')">🗑️ Xóa</button>
                </div>
            </div>
        </div>`;
    }).join('');
}

// ---------- 4. MODAL THÊM MÓN ----------
function moModalThem() {
    document.getElementById('modalSanPhamTieuDe').textContent = 'Thêm Món Mới';
    document.getElementById('txtIdSanPham').value = '';
    document.getElementById('txtTenSanPham').value = '';
    document.getElementById('txtGiaCoBan').value = '';
    document.getElementById('txtHinhAnh').value = '';
    document.getElementById('imgPreviewHinhAnh').src = 'img/logo.png';
    document.getElementById('txtFileHinhAnh').value = '';
    document.getElementById('cboLoai').value = 'ThucAn';
    document.getElementById('cboDangBan').value = 'true';
    capNhatGiaoDienLoai();
    moModal('modalSanPham');
}

// ---------- 5. MODAL SỬA MÓN ----------
function moModalSua(id) {
    const sp = danhSachSanPham.find(s => s.Id === id);
    if (!sp) return;

    document.getElementById('modalSanPhamTieuDe').textContent = `Sửa: ${sp.TenSanPham}`;
    document.getElementById('txtIdSanPham').value = sp.Id;
    document.getElementById('txtTenSanPham').value = sp.TenSanPham;
    document.getElementById('txtGiaCoBan').value = sp.GiaCoBan;
    document.getElementById('txtHinhAnh').value = sp.HinhAnh || '';
    document.getElementById('imgPreviewHinhAnh').src = sp.HinhAnh || 'img/logo.png';
    document.getElementById('txtFileHinhAnh').value = '';
    document.getElementById('cboLoai').value = sp.Loai;
    document.getElementById('cboDangBan').value = sp.DangBan ? 'true' : 'false';
    capNhatGiaoDienLoai();
    moModal('modalSanPham');
}

// ---------- 6. XỬ LÝ CHỌN ẢNH TỪ MÁY (convert sang Base64) ----------
function xuLyChonAnh(event) {
    const file = event.target.files[0];
    if (!file) return;

    // Giới hạn 1MB để tránh nặng CSDL Access
    if (file.size > 1 * 1024 * 1024) {
        hienThiThongBao('Vui lòng chọn ảnh nhỏ hơn 1MB!', 'error');
        event.target.value = '';
        return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
        const base64String = e.target.result;
        document.getElementById('txtHinhAnh').value = base64String;
        document.getElementById('imgPreviewHinhAnh').src = base64String;
    };
    reader.readAsDataURL(file);
}

function xoaAnhDaChon() {
    document.getElementById('txtHinhAnh').value = '';
    document.getElementById('imgPreviewHinhAnh').src = 'img/logo.png';
    document.getElementById('txtFileHinhAnh').value = '';
}

// ---------- 7. CẬP NHẬT GHI CHÚ PHỤ PHÍ (thể hiện ĐA HÌNH OOP) ----------
function capNhatGiaoDienLoai() {
    const loai = document.getElementById('cboLoai').value;
    const moTa = document.getElementById('moTaPhuPhi');
    if (loai === 'ThucAn') {
        moTa.textContent = 'Thức ăn: Khách chọn "Phần lớn" → cộng thêm 50,000đ/phần.';
    } else {
        moTa.textContent = 'Nước uống: Khách chọn "Lon" → giá × 1.2 (đắt hơn 20%).';
    }
}

// ---------- 8. LƯU SẢN PHẨM (THÊM HOẶC SỬA) ----------
async function luuSanPham() {
    const id   = document.getElementById('txtIdSanPham').value;
    const ten  = document.getElementById('txtTenSanPham').value.trim();
    const gia  = parseFloat(document.getElementById('txtGiaCoBan').value);
    const hinhAnh = document.getElementById('txtHinhAnh').value.trim();
    const loai = document.getElementById('cboLoai').value;
    const dangBan = document.getElementById('cboDangBan').value === 'true';

    // Validate phía client
    if (!ten) {
        hienThiThongBao('Vui lòng nhập tên món!', 'error');
        document.getElementById('txtTenSanPham').focus();
        return;
    }
    if (isNaN(gia) || gia < 0) {
        hienThiThongBao('Giá phải là số và không được âm!', 'error');
        document.getElementById('txtGiaCoBan').focus();
        return;
    }

    const payload = {
        TenSanPham: ten,
        GiaCoBan: gia,
        Loai: loai,
        DangBan: dangBan,
        HinhAnh: hinhAnh || null
    };
    const isEdit = id !== '';

    try {
        const url    = isEdit ? `${API}/sanpham/${id}` : `${API}/sanpham`;
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
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 9. XÓA SẢN PHẨM ----------
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
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}
