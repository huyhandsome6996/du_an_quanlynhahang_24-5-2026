// ============================================================
// KHO.JS - Logic trang Quản Lý Kho (kho.html)
// Hệ thống Vua Sư Tử v2.0
// ============================================================

const API = 'http://localhost:5000/api';
let danhSachNguyenLieu = [];  // Lưu toàn bộ danh sách nguyên liệu
let idNLDangSua = null;       // Id nguyên liệu đang được sửa
let danhSachKhoLog = [];      // Lưu toàn bộ log nhập xuất

// ---- Khởi động trang ----
document.addEventListener('DOMContentLoaded', () => {
    taiTatCa();
});

// Tải tất cả dữ liệu cần thiết
function taiTatCa() {
    taiDanhSachNguyenLieu();
    taiCanhBao();
}

// ============================================================
// NGUYÊN LIỆU — Tải & hiển thị
// ============================================================

// Lấy danh sách nguyên liệu từ API
async function taiDanhSachNguyenLieu() {
    const tbody = document.getElementById('tbodyNguyenLieu');
    if (tbody) {
        tbody.innerHTML = '<tr><td colspan="8" class="text-center py-12"><div class="spinner"></div></td></tr>';
    }
    try {
        const res = await fetch(`${API}/nguyenlieu`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        danhSachNguyenLieu = await res.json();
        hienThiNguyenLieu(danhSachNguyenLieu);
        capNhatThongKe();
        capNhatSelectNguyenLieu();
    } catch (err) {
        hienThiThongBao('Không kết nối được server! Hãy chắc chắn đã chạy dotnet run.', 'error');
        if (tbody) {
            tbody.innerHTML = '<tr><td colspan="8" class="text-center py-8 text-on-surface-variant">⚠️ Không thể tải danh sách nguyên liệu</td></tr>';
        }
    }
}

// Hiển thị bảng nguyên liệu
function hienThiNguyenLieu(dsNL) {
    const tbody = document.getElementById('tbodyNguyenLieu');
    if (!tbody) return;

    if (dsNL.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center py-12">
                    <div class="flex flex-col items-center">
                        <img src="img/menu_book_3d.png" class="w-16 h-16 object-contain rounded-xl drop-shadow-md mb-2">
                        <p class="text-on-surface-variant">Chưa có nguyên liệu nào. Hãy thêm nguyên liệu mới!</p>
                    </div>
                </td>
            </tr>`;
        return;
    }

    tbody.innerHTML = dsNL.map((nl, idx) => {
        const laCanhBao = nl.SoLuongTon <= (nl.MucToiThieu || 0);
        const trangThai = laCanhBao
            ? '<span class="badge badge-cokhach">⚠️ Sắp hết</span>'
            : '<span class="badge badge-trong">● Đủ</span>';
        const hangCanhBao = laCanhBao ? 'style="background:rgba(248,113,113,0.04)"' : '';

        return `
        <tr ${hangCanhBao}>
            <td class="text-on-surface-variant font-semibold">${idx + 1}</td>
            <td class="font-semibold">${nl.TenNguyenLieu}</td>
            <td class="text-on-surface-variant">${nl.DonVi}</td>
            <td>
                <span class="${laCanhBao ? 'text-mau-do font-bold' : 'text-on-surface font-semibold'}">${nl.SoLuongTon}</span>
            </td>
            <td class="text-on-surface-variant">${nl.MucToiThieu || 0}</td>
            <td>${trangThai}</td>
            <td class="text-on-surface-variant text-xs max-w-[150px] truncate">${nl.GhiChu || '-'}</td>
            <td>
                <div class="flex gap-1.5">
                    <button class="btn btn-sm btn-info" onclick="moModalSuaNguyenLieu(${nl.Id})" title="Sửa">✏️</button>
                    <button class="btn btn-sm btn-danger" onclick="xoaNguyenLieu(${nl.Id}, '${nl.TenNguyenLieu.replace(/'/g, "\\'")}')" title="Xóa">🗑️</button>
                </div>
            </td>
        </tr>`;
    }).join('');
}

// Tìm kiếm nguyên liệu
function timKiemNguyenLieu() {
    const tuKhoa = document.getElementById('txtTimKiemNL').value.trim().toLowerCase();
    if (!tuKhoa) {
        hienThiNguyenLieu(danhSachNguyenLieu);
        return;
    }
    const dsLoc = danhSachNguyenLieu.filter(nl =>
        nl.TenNguyenLieu.toLowerCase().includes(tuKhoa) ||
        (nl.DonVi && nl.DonVi.toLowerCase().includes(tuKhoa)) ||
        (nl.GhiChu && nl.GhiChu.toLowerCase().includes(tuKhoa))
    );
    hienThiNguyenLieu(dsLoc);
}

// Cập nhật thống kê
function capNhatThongKe() {
    const tongNL = danhSachNguyenLieu.length;
    const soCanhBao = danhSachNguyenLieu.filter(nl => nl.SoLuongTon <= (nl.MucToiThieu || 0)).length;

    // Tổng giá trị kho — tính từ KhoLog nếu có, nếu không thì chỉ đếm số lượng
    const tongGiaTri = danhSachNguyenLieu.reduce((sum, nl) => {
        // Nếu nguyên liệu có trường DonGia thì dùng, không thì cứ hiển thị 0
        return sum + (nl.DonGia ? nl.SoLuongTon * nl.DonGia : 0);
    }, 0);

    const elTongNL = document.getElementById('statTongNL');
    const elCanhBao = document.getElementById('statCanhBao');
    const elGiaTri = document.getElementById('statGiaTri');

    if (elTongNL) elTongNL.textContent = tongNL;
    if (elCanhBao) elCanhBao.textContent = soCanhBao;
    if (elGiaTri) elGiaTri.textContent = formatTien(tongGiaTri);
}

// Cập nhật dropdown nguyên liệu cho modal Nhập/Xuất
function capNhatSelectNguyenLieu() {
    const cboNLNhap = document.getElementById('cboNLNhap');
    const cboNLXuat = document.getElementById('cboNLXuat');

    const options = danhSachNguyenLieu.map(nl =>
        `<option value="${nl.Id}">${nl.TenNguyenLieu} (${nl.SoLuongTon} ${nl.DonVi})</option>`
    ).join('');

    if (cboNLNhap) {
        cboNLNhap.innerHTML = '<option value="">-- Chọn nguyên liệu --</option>' + options;
    }
    if (cboNLXuat) {
        cboNLXuat.innerHTML = '<option value="">-- Chọn nguyên liệu --</option>' + options;
    }
}

// ============================================================
// CẢNH BÁO — Tồn kho thấp
// ============================================================

async function taiCanhBao() {
    const el = document.getElementById('khoCanhBao');
    if (!el) return;
    try {
        const res = await fetch(`${API}/nguyenlieu/canhbao`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        const dsCanhBao = await res.json();

        if (dsCanhBao.length === 0) {
            el.innerHTML = '';
            return;
        }

        el.innerHTML = `
            <div class="rounded-2xl border border-mau-do/20 bg-mau-do/[0.06] p-5 backdrop-blur-sm" style="animation: slideIn 0.3s ease">
                <div class="flex items-center gap-3 mb-3">
                    <img src="img/close_3d.png" class="w-7 h-7 object-contain">
                    <h4 class="font-bold text-mau-do text-sm uppercase tracking-wider">Cảnh báo tồn kho thấp</h4>
                    <span class="badge badge-cokhach ml-auto">${dsCanhBao.length} mục</span>
                </div>
                <div class="flex flex-wrap gap-2">
                    ${dsCanhBao.map(nl => `
                        <div class="flex items-center gap-2 bg-mau-do/[0.08] border border-mau-do/15 rounded-xl px-3 py-2 text-sm">
                            <span class="text-mau-do font-bold">${nl.TenNguyenLieu}</span>
                            <span class="text-on-surface-variant">— còn</span>
                            <span class="text-mau-do font-bold">${nl.SoLuongTon}</span>
                            <span class="text-on-surface-variant">${nl.DonVi}</span>
                            <span class="text-on-surface-variant/50">(tối thiểu: ${nl.MucToiThieu || 0})</span>
                        </div>
                    `).join('')}
                </div>
            </div>`;
    } catch (err) {
        // Không hiển thị lỗi cảnh báo — không phải vấn đề nghiêm trọng
        el.innerHTML = '';
    }
}

// ============================================================
// CRUD NGUYÊN LIỆU — Thêm / Sửa / Xóa
// ============================================================

// Mở modal thêm nguyên liệu
function moModalThemNguyenLieu() {
    idNLDangSua = null;
    document.getElementById('modalNLTieuDe').textContent = 'Thêm Nguyên Liệu';
    document.getElementById('txtTenNL').value = '';
    document.getElementById('cboDonVi').value = 'kg';
    document.getElementById('txtSoLuongTon').value = '';
    document.getElementById('txtMucToiThieu').value = '';
    document.getElementById('txtGhiChuNL').value = '';
    moModal('modalNguyenLieu');
}

// Mở modal sửa nguyên liệu
function moModalSuaNguyenLieu(nlId) {
    const nl = danhSachNguyenLieu.find(n => n.Id === nlId);
    if (!nl) return;

    idNLDangSua = nlId;
    document.getElementById('modalNLTieuDe').textContent = `Sửa ${nl.TenNguyenLieu}`;
    document.getElementById('txtTenNL').value = nl.TenNguyenLieu;
    document.getElementById('cboDonVi').value = nl.DonVi;
    document.getElementById('txtSoLuongTon').value = nl.SoLuongTon;
    document.getElementById('txtMucToiThieu').value = nl.MucToiThieu || '';
    document.getElementById('txtGhiChuNL').value = nl.GhiChu || '';
    moModal('modalNguyenLieu');
}

// Lưu (thêm hoặc sửa) nguyên liệu
async function luuNguyenLieu() {
    const tenNL = document.getElementById('txtTenNL').value.trim();
    const donVi = document.getElementById('cboDonVi').value;
    const soLuongTon = parseFloat(document.getElementById('txtSoLuongTon').value);
    const mucToiThieu = parseFloat(document.getElementById('txtMucToiThieu').value) || 0;
    const ghiChu = document.getElementById('txtGhiChuNL').value.trim();

    // Validation
    if (!tenNL) {
        hienThiThongBao('Vui lòng nhập tên nguyên liệu!', 'error');
        document.getElementById('txtTenNL').focus();
        return;
    }
    if (isNaN(soLuongTon) || soLuongTon < 0) {
        hienThiThongBao('Số lượng tồn phải là số không âm!', 'error');
        document.getElementById('txtSoLuongTon').focus();
        return;
    }
    if (!donVi) {
        hienThiThongBao('Vui lòng chọn đơn vị!', 'error');
        return;
    }

    const payload = {
        TenNguyenLieu: tenNL,
        DonVi: donVi,
        SoLuongTon: soLuongTon,
        MucToiThieu: mucToiThieu,
        GhiChu: ghiChu
    };

    const isEdit = idNLDangSua !== null;

    try {
        const url = isEdit ? `${API}/nguyenlieu/${idNLDangSua}` : `${API}/nguyenlieu`;
        const method = isEdit ? 'PUT' : 'POST';

        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const data = await res.json();

        if (res.ok) {
            dongModal('modalNguyenLieu');
            hienThiThongBao(`✅ ${isEdit ? 'Cập nhật' : 'Thêm'} nguyên liệu thành công!`, 'success');
            taiTatCa();
        } else {
            hienThiThongBao(`❌ ${data.thongBao || data.message || 'Có lỗi xảy ra!'}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// Xóa nguyên liệu
async function xoaNguyenLieu(nlId, tenNL) {
    if (!confirm(`Xác nhận xóa nguyên liệu "${tenNL}"? Thao tác này không thể hoàn tác!`)) return;
    try {
        const res = await fetch(`${API}/nguyenlieu/${nlId}`, { method: 'DELETE' });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ Đã xóa nguyên liệu "${tenNL}"`, 'success');
            taiTatCa();
        } else {
            hienThiThongBao(`❌ ${data.thongBao || data.message || 'Xóa thất bại!'}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ============================================================
// NHẬP KHO
// ============================================================

function moModalNhapKho() {
    if (danhSachNguyenLieu.length === 0) {
        hienThiThongBao('Chưa có nguyên liệu nào! Hãy thêm nguyên liệu trước.', 'error');
        return;
    }
    document.getElementById('cboNLNhap').value = '';
    document.getElementById('txtSoLuongNhap').value = '';
    document.getElementById('txtDonGiaNhap').value = '';
    document.getElementById('txtLyDoNhap').value = '';
    moModal('modalNhapKho');
}

async function nhapKho() {
    const nguyenLieuId = parseInt(document.getElementById('cboNLNhap').value);
    const soLuong = parseFloat(document.getElementById('txtSoLuongNhap').value);
    const donGia = parseFloat(document.getElementById('txtDonGiaNhap').value) || 0;
    const lyDo = document.getElementById('txtLyDoNhap').value.trim();

    // Validation
    if (!nguyenLieuId) {
        hienThiThongBao('Vui lòng chọn nguyên liệu!', 'error');
        return;
    }
    if (isNaN(soLuong) || soLuong <= 0) {
        hienThiThongBao('Số lượng nhập phải lớn hơn 0!', 'error');
        document.getElementById('txtSoLuongNhap').focus();
        return;
    }

    const payload = {
        NguyenLieuId: nguyenLieuId,
        SoLuong: soLuong,
        DonGia: donGia,
        LyDo: lyDo || 'Nhập kho'
    };

    try {
        const res = await fetch(`${API}/kho/nhap`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const data = await res.json();

        if (res.ok) {
            dongModal('modalNhapKho');
            const nl = danhSachNguyenLieu.find(n => n.Id === nguyenLieuId);
            const tenNL = nl ? nl.TenNguyenLieu : '';
            hienThiThongBao(`✅ Nhập kho thành công: +${soLuong} ${nl ? nl.DonVi : ''} ${tenNL}`, 'success');
            taiTatCa();
        } else {
            hienThiThongBao(`❌ ${data.thongBao || data.message || 'Nhập kho thất bại!'}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ============================================================
// XUẤT KHO
// ============================================================

function moModalXuatKho() {
    if (danhSachNguyenLieu.length === 0) {
        hienThiThongBao('Chưa có nguyên liệu nào! Hãy thêm nguyên liệu trước.', 'error');
        return;
    }
    document.getElementById('cboNLXuat').value = '';
    document.getElementById('txtSoLuongXuat').value = '';
    document.getElementById('txtLyDoXuat').value = '';
    document.getElementById('txtTonKhoHienTai').textContent = '';
    moModal('modalXuatKho');
}

async function xuatKho() {
    const nguyenLieuId = parseInt(document.getElementById('cboNLXuat').value);
    const soLuong = parseFloat(document.getElementById('txtSoLuongXuat').value);
    const lyDo = document.getElementById('txtLyDoXuat').value.trim();

    // Validation
    if (!nguyenLieuId) {
        hienThiThongBao('Vui lòng chọn nguyên liệu!', 'error');
        return;
    }
    if (isNaN(soLuong) || soLuong <= 0) {
        hienThiThongBao('Số lượng xuất phải lớn hơn 0!', 'error');
        document.getElementById('txtSoLuongXuat').focus();
        return;
    }
    if (!lyDo) {
        hienThiThongBao('Vui lòng nhập lý do xuất kho!', 'error');
        document.getElementById('txtLyDoXuat').focus();
        return;
    }

    // Kiểm tra tồn kho phía client
    const nl = danhSachNguyenLieu.find(n => n.Id === nguyenLieuId);
    if (nl && soLuong > nl.SoLuongTon) {
        hienThiThongBao(`Số lượng xuất (${soLuong}) vượt quá tồn kho (${nl.SoLuongTon} ${nl.DonVi})!`, 'error');
        return;
    }

    const payload = {
        NguyenLieuId: nguyenLieuId,
        SoLuong: soLuong,
        LyDo: lyDo
    };

    try {
        const res = await fetch(`${API}/kho/xuat`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const data = await res.json();

        if (res.ok) {
            dongModal('modalXuatKho');
            const tenNL = nl ? nl.TenNguyenLieu : '';
            hienThiThongBao(`✅ Xuất kho thành công: -${soLuong} ${nl ? nl.DonVi : ''} ${tenNL}`, 'success');
            taiTatCa();
        } else {
            hienThiThongBao(`❌ ${data.thongBao || data.message || 'Xuất kho thất bại!'}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ============================================================
// KHO LOG — Lịch sử nhập xuất
// ============================================================

function moModalKhoLog() {
    moModal('modalKhoLog');
    taiKhoLog();
}

async function taiKhoLog() {
    const el = document.getElementById('khoLogNoiDung');
    if (!el) return;

    el.innerHTML = '<div class="spinner"></div>';

    try {
        const res = await fetch(`${API}/kholog`);
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        danhSachKhoLog = await res.json();
        hienThiKhoLog(danhSachKhoLog);
    } catch (err) {
        el.innerHTML = '<p class="text-center text-on-surface-variant py-8">⚠️ Không thể tải lịch sử kho</p>';
    }
}

function hienThiKhoLog(dsLog) {
    const el = document.getElementById('khoLogNoiDung');
    if (!el) return;

    if (dsLog.length === 0) {
        el.innerHTML = `
            <div class="flex flex-col items-center py-12">
                <img src="img/bill_3d.png" class="w-16 h-16 object-contain rounded-xl drop-shadow-md mb-2">
                <p class="text-on-surface-variant">Chưa có lịch sử nhập xuất kho.</p>
            </div>`;
        return;
    }

    el.innerHTML = `
        <div class="table-wrapper">
            <table>
                <thead>
                    <tr>
                        <th>Thời gian</th>
                        <th>Loại</th>
                        <th>Nguyên liệu</th>
                        <th>Số lượng</th>
                        <th>Đơn giá</th>
                        <th>Thành tiền</th>
                        <th>Lý do</th>
                    </tr>
                </thead>
                <tbody>
                    ${dsLog.map(log => {
                        const laNhap = log.Loai === 'Nhap' || log.Loai === 'nhap';
                        const loaiLabel = laNhap ? 'Nhập' : 'Xuất';
                        const loaiBadge = laNhap
                            ? '<span class="badge badge-trong">↓ Nhập</span>'
                            : '<span class="badge badge-cokhach">↑ Xuất</span>';

                        return `
                        <tr>
                            <td class="text-on-surface-variant text-xs whitespace-nowrap">${formatThoiGian(log.ThoiGian || log.Ngay)}</td>
                            <td>${loaiBadge}</td>
                            <td class="font-semibold">${log.TenNguyenLieu || '-'}</td>
                            <td>
                                <span class="${laNhap ? 'text-mau-xanh' : 'text-mau-do'} font-bold">
                                    ${laNhap ? '+' : '-'}${log.SoLuong}
                                </span>
                            </td>
                            <td class="text-on-surface-variant">${log.DonGia ? formatTien(log.DonGia) : '-'}</td>
                            <td class="text-on-surface-variant">${log.DonGia ? formatTien(log.SoLuong * log.DonGia) : '-'}</td>
                            <td class="text-on-surface-variant text-xs max-w-[180px] truncate">${log.LyDo || '-'}</td>
                        </tr>`;
                    }).join('')}
                </tbody>
            </table>
        </div>`;
}

// ============================================================
// HÀM TIỆN ÍCH
// ============================================================

// Hiển thị thông báo nội tuyến + Toast notification
function hienThiThongBao(noiDung, loai = 'success') {
    // Inline notification
    const kv = document.getElementById('thongBaoKhuVuc');
    if (kv) {
        kv.innerHTML = `<div class="alert alert-${loai}">${noiDung}</div>`;
        setTimeout(() => kv.innerHTML = '', 4000);
    }
    // Toast notification
    hienToast(noiDung, loai);
}

// Toast notification — Hiển thị thông báo nổi góc phải trên cùng
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

    setTimeout(() => {
        toast.classList.add('toast-out');
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

// Định dạng tiền VND
function formatTien(so) {
    if (so === null || so === undefined || isNaN(so)) return '0₫';
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(so);
}

// Định dạng thời gian
function formatThoiGian(chuoi) {
    if (!chuoi) return '-';
    return new Date(chuoi).toLocaleString('vi-VN');
}

// MODAL — Mở/đóng modal mượt mà với hiệu ứng scale
function moModal(id) {
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
}

function dongModal(id) {
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
}
