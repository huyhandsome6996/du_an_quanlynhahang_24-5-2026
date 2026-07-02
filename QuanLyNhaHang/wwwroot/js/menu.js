// ============================================================
// MENU.JS — Logic trang Thực Đơn (menu.html)
// ------------------------------------------------------------
// Chức năng:
//   1. Tải danh sách món từ API
//   2. Lọc theo loại (Thức ăn / Nước uống)
//   3. Tìm kiếm theo tên
//   4. Thêm / Sửa / Xoá món (kèm upload ảnh Base64)
//   5. Hiển thị ghi chú đa hình OOP (Phụ phí Phần lớn / Lon)
// ============================================================

let danhSachSanPham = [];   // Cache toàn bộ sản phẩm (để lọc/tìm kiếm không cần gọi API lại)

// ---------- KHỞI ĐỘNG ----------
document.addEventListener('DOMContentLoaded', () => {
    taiDanhSach();          // Tải danh sách món
    capNhatGiaoDienLoai();  // Hiện ghi chú phụ phí lần đầu
});

// ---------- 1. TẢI DANH SÁCH MÓN ----------
async function taiDanhSach() {
    // Hiện spinner trong lúc chờ API
    document.getElementById('bangSanPham').innerHTML =
        '<div class="col-span-full flex justify-center py-12"><div class="spinner"></div></div>';
    try {
        // Gọi GET /api/sanpham — trả về mảng tất cả sản phẩm
        const res = await fetch(`${API}/sanpham`);
        danhSachSanPham = await res.json();   // Cache vào biến toàn cục
        locTheoLoai();                         // Hiển thị (có áp dụng bộ lọc)
    } catch {
        document.getElementById('bangSanPham').innerHTML =
            '<div class="col-span-full"><div class="alert alert-error">⚠️ Lỗi kết nối server!</div></div>';
    }
}

// ---------- 2. LỌC THEO LOẠI + TÌM KIẾM ----------
function locTheoLoai() {
    // Lấy giá trị combobox lọc (Trống / ThucAn / NuocUong)
    const loai = document.getElementById('cboLocLoai').value;
    // Lọc theo loại (nếu có)
    let dsLoc = loai ? danhSachSanPham.filter(sp => sp.Loai === loai) : danhSachSanPham;

    // Áp dụng thêm tìm kiếm nếu có
    const tuKhoa = document.getElementById('txtTimKiemMon')?.value.trim().toLowerCase();
    if (tuKhoa) {
        dsLoc = dsLoc.filter(sp => sp.TenSanPham.toLowerCase().includes(tuKhoa));
    }
    hienThiBang(dsLoc);
}

// Hàm wrapper cho input tìm kiếm (gọi lại locTheoLoai)
function timKiemMon() { locTheoLoai(); }

// ---------- 3. HIỂN THỊ LƯỚI MÓN ĂN ----------
function hienThiBang(ds) {
    // Cập nhật số món đang hiển thị
    document.getElementById('tongSoMon').textContent = ds.length;
    const grid = document.getElementById('bangSanPham');

    // Trường hợp không có món
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

    // Tạo HTML cho từng card món
    // onclick vào card → moModalSua(id) để sửa
    grid.innerHTML = ds.map(sp => {
        // Nếu sp.HinhAnh là Base64 → dùng luôn; nếu không → dùng logo.png
        const imageSrc = sp.HinhAnh || 'img/logo.png';
        return `
        <div class="glass-card product-card rounded-2xl overflow-hidden cursor-pointer flex flex-col transition-all hover:shadow-xl hover:-translate-y-1" onclick="moModalSua(${sp.Id})">
            <div class="relative h-44 w-full bg-surface-container-highest overflow-hidden border-b border-white/5">
                <img src="${imageSrc}" alt="${sp.TenSanPham}" class="w-full h-full object-cover transition-transform duration-500 hover:scale-105" onerror="this.src='img/logo.png'">
                <!-- Badge "Đang bán" / "Ngừng" -->
                <span class="absolute top-3 right-3 badge ${sp.DangBan ? 'badge-trong' : 'badge-cokhach'}">
                    ${sp.DangBan ? '✅ Đang bán' : '❌ Ngừng'}
                </span>
                <!-- Badge loại món: Thức ăn 🍖 / Nước uống 🥤 -->
                <span class="absolute bottom-3 left-3 badge ${sp.Loai === 'ThucAn' ? 'badge-thucan' : 'badge-nuocuong'}">
                    ${sp.Loai === 'ThucAn' ? '🍖 Thức ăn' : '🥤 Nước uống'}
                </span>
            </div>
            <div class="p-5 flex-1 flex flex-col">
                <h3 class="font-bold text-base text-on-surface mb-1 line-clamp-1">${sp.TenSanPham}</h3>
                <p class="text-primary font-bold text-sm mb-4">${formatTien(sp.GiaCoBan)}</p>
                <div class="flex gap-2 justify-end mt-auto pt-4 border-t border-white/5">
                    <!-- 2 nút Sửa / Xoá (stopPropagation để không trigger click của card) -->
                    <button class="btn btn-sm btn-info flex-1" onclick="event.stopPropagation(); moModalSua(${sp.Id})">✏️ Sửa</button>
                    <button class="btn btn-sm btn-danger flex-1" onclick="event.stopPropagation(); xoa(${sp.Id}, '${sp.TenSanPham.replace(/'/g, "\\\'")}')">🗑️ Xóa</button>
                </div>
            </div>
        </div>`;
    }).join('');
}

// ---------- 4. MODAL THÊM MÓN ----------
function moModalThem() {
    // Đặt tiêu đề modal = "Thêm Món Mới"
    document.getElementById('modalSanPhamTieuDe').textContent = 'Thêm Món Mới';
    // Xoá toàn bộ dữ liệu cũ trong form
    document.getElementById('txtIdSanPham').value = '';
    document.getElementById('txtTenSanPham').value = '';
    document.getElementById('txtGiaCoBan').value = '';
    document.getElementById('txtHinhAnh').value = '';
    document.getElementById('imgPreviewHinhAnh').src = 'img/logo.png';
    document.getElementById('txtFileHinhAnh').value = '';
    // Mặc định loại = ThucAn, trạng thái = Đang bán
    document.getElementById('cboLoai').value = 'ThucAn';
    document.getElementById('cboDangBan').value = 'true';
    capNhatGiaoDienLoai();   // Cập nhật ghi chú phụ phí
    moModal('modalSanPham');
}

// ---------- 5. MODAL SỬA MÓN ----------
function moModalSua(id) {
    // Tìm món trong cache
    const sp = danhSachSanPham.find(s => s.Id === id);
    if (!sp) return;

    // Đổ dữ liệu vào form
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
    const file = event.target.files[0];   // Lấy file đầu tiên (chỉ chọn 1)
    if (!file) return;

    // Giới hạn 1MB để tránh nặng CSDL Access
    if (file.size > 1 * 1024 * 1024) {
        hienThiThongBao('Vui lòng chọn ảnh nhỏ hơn 1MB!', 'error');
        event.target.value = '';   // Reset input
        return;
    }

    // FileReader → đọc file thành Base64
    const reader = new FileReader();
    reader.onload = (e) => {
        const base64String = e.target.result;   // Chuỗi Base64
        // Lưu vào input hidden để gửi đi khi submit
        document.getElementById('txtHinhAnh').value = base64String;
        // Hiển thị preview
        document.getElementById('imgPreviewHinhAnh').src = base64String;
    };
    // readAsDataURL → trả về chuỗi "data:image/png;base64,..."
    reader.readAsDataURL(file);
}

// Hàm xoá ảnh đã chọn (đặt lại về logo mặc định)
function xoaAnhDaChon() {
    document.getElementById('txtHinhAnh').value = '';
    document.getElementById('imgPreviewHinhAnh').src = 'img/logo.png';
    document.getElementById('txtFileHinhAnh').value = '';
}

// ---------- 7. CẬP NHẬT GHI CHÚ PHỤ PHÍ (thể hiện ĐA HÌNH OOP) ----------
function capNhatGiaoDienLoai() {
    // Lấy loại món đang chọn trong combobox
    const loai = document.getElementById('cboLoai').value;
    const moTa = document.getElementById('moTaPhuPhi');
    // Hiện ghi chú khác nhau tùy loại
    // Đây là minh hoạ FRONT-END cho đa hình: ThucAn và NuocUong có cách tính tiền khác nhau
    // (thực tế tính tiền nằm ở C# TinhTien() của từng lớp con)
    if (loai === 'ThucAn') {
        moTa.textContent = 'Thức ăn: Khách chọn "Phần lớn" → cộng thêm 50,000đ/phần.';
    } else {
        moTa.textContent = 'Nước uống: Khách chọn "Lon" → giá × 1.2 (đắt hơn 20%).';
    }
}

// ---------- 8. LƯU SẢN PHẨM (THÊM HOẶC SỬA) ----------
async function luuSanPham() {
    // Lấy dữ liệu từ form
    const id   = document.getElementById('txtIdSanPham').value;
    const ten  = document.getElementById('txtTenSanPham').value.trim();
    const gia  = parseFloat(document.getElementById('txtGiaCoBan').value);
    const hinhAnh = document.getElementById('txtHinhAnh').value.trim();
    const loai = document.getElementById('cboLoai').value;
    // cboDangBan.value là chuỗi 'true'/'false' → convert sang bool
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

    // Tạo payload JSON — tên property PHẢI khớp với C# (PascalCase, không camelCase)
    const payload = {
        TenSanPham: ten,
        GiaCoBan: gia,
        Loai: loai,
        DangBan: dangBan,
        HinhAnh: hinhAnh || null
    };
    // Có id → Sửa (PUT), không có id → Thêm (POST)
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
            taiDanhSach();   // Refresh
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
