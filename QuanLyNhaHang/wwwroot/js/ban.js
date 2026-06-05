// ============================================================
// BAN.JS - Logic trang Quản Lý Bàn (index.html)
// ============================================================

const API = 'http://localhost:5000/api';
let danhSachBan = [];      // Lưu toàn bộ danh sách bàn
let idBanDangSua = null;   // Id bàn đang được sửa

// ---- Khởi động trang ----
document.addEventListener('DOMContentLoaded', () => {
    taiDanhSachBan();
});

// Lấy danh sách bàn từ API
async function taiDanhSachBan() {
    document.getElementById('luoiBan').innerHTML = '<div class="spinner"></div>';
    try {
        const res = await fetch(`${API}/ban`);
        danhSachBan = await res.json();
        hienThiBan(danhSachBan);
    } catch (err) {
        hienThiThongBao('Không kết nối được server! Hãy chắc chắn đã chạy dotnet run.', 'error');
        document.getElementById('luoiBan').innerHTML = '';
    }
}

// TÌM KIẾM bàn theo tên
function timKiemBan() {
    const tuKhoa = document.getElementById('txtTimKiemBan').value.trim().toLowerCase();
    if (!tuKhoa) {
        hienThiBan(danhSachBan);
        return;
    }
    const dsLoc = danhSachBan.filter(b => b.TenBan.toLowerCase().includes(tuKhoa));
    hienThiBan(dsLoc);
}

// Vẽ lưới bàn lên giao diện
function hienThiBan(dsBan) {
    const luoi = document.getElementById('luoiBan');
    const soTrong   = dsBan.filter(b => b.TrangThai === 'Trống').length;
    const soDaDat   = dsBan.filter(b => b.TrangThai === 'Đã đặt').length;
    const soCoKhach = dsBan.filter(b => b.TrangThai === 'Có khách').length;

    document.getElementById('soBanTrong').textContent   = soTrong;
    document.getElementById('soBanDaDat').textContent   = soDaDat;
    document.getElementById('soBanCoKhach').textContent = soCoKhach;

    if (dsBan.length === 0) {
        luoi.innerHTML = '<div class="empty-state flex flex-col items-center"><img src="img/chair_3d.png" class="empty-icon w-16 h-16 object-contain rounded-xl drop-shadow-md mb-2"><p>Chưa có bàn nào. Hãy thêm bàn mới!</p></div>';
        return;
    }

    luoi.innerHTML = dsBan.map(ban => {
        const laTrong = ban.TrangThai === 'Trống';
        const laDaDat = ban.TrangThai === 'Đã đặt';
        const cssClass = laTrong ? 'trong' : (laDaDat ? 'dadat' : 'cokhach');
        const icon = laTrong ? '<img src="img/chair_3d.png" class="w-12 h-12 object-contain rounded-lg border border-primary/20 shadow-md">' : (laDaDat ? '<img src="img/clock_3d.png" class="w-12 h-12 object-contain rounded-lg border border-primary/20 shadow-md">' : '<img src="img/user_3d.png" class="w-12 h-12 object-contain rounded-lg border border-primary/20 shadow-md">');
        const badgeClass = laTrong ? 'badge-trong' : (laDaDat ? 'badge-dadat' : 'badge-cokhach');
        const badgeText = laTrong ? '● Trống' : (laDaDat ? '● Đã đặt' : '● Có khách');
        return `
        <div class="ban-card ${cssClass}"
             onclick="clickVaoBan(${ban.Id})"
             title="${ban.TenBan} - ${ban.TrangThai}">
            <div class="ban-icon flex justify-center mb-2">${icon}</div>
            <div class="ban-ten">${ban.TenBan}</div>
            <div class="ban-trangthai">
                <span class="badge ${badgeClass}">
                    ${badgeText}
                </span>
            </div>
            <div style="margin-top:0.75rem; display:flex; gap:0.35rem; justify-content:center; flex-wrap:wrap;">
                <button class="btn btn-sm btn-info" onclick="event.stopPropagation(); moModalSuaBan(${ban.Id})">✏️</button>
                <button class="btn btn-sm btn-danger" onclick="event.stopPropagation(); xoaBan(${ban.Id}, '${ban.TenBan}')">🗑️</button>
            </div>
        </div>`;
    }).join('');
}

// Xử lý khi click vào 1 bàn
async function clickVaoBan(banId) {
    const ban = danhSachBan.find(b => b.Id === banId);
    if (!ban) return;

    const modal = document.getElementById('modalChiTietBan');
    document.getElementById('chiTietBanTieuDe').textContent = ban.TenBan;

    if (ban.TrangThai === 'Trống') {
        // Bàn trống: hiển thị nút mở bàn
        document.getElementById('chiTietBanNoidung').innerHTML = `
            <div class="empty-state flex flex-col items-center">
                <img src="img/chair_3d.png" class="w-20 h-20 object-contain rounded-2xl drop-shadow-md mb-3 border border-primary/20">
                <p style="margin-bottom:0.5rem; color:var(--mau-chu);">${ban.TenBan} hiện đang <strong style="color:var(--mau-xanh)">Trống</strong></p>
                <p class="text-nhat">Nhấn "Mở Bàn" để tạo hóa đơn mới đón khách.</p>
            </div>`;
        document.getElementById('chiTietBanFooter').innerHTML = `
            <button class="btn btn-secondary" onclick="dongModal('modalChiTietBan')">Hủy</button>
            <button class="btn btn-primary btn-lg flex items-center justify-center gap-1" onclick="moBan(${ban.Id})">
                <img src="img/add_3d.png" class="w-4 h-4 object-cover rounded-sm inline-block mr-1"> Mở Bàn Đón Khách
            </button>`;
    } else if (ban.TrangThai === 'Đã đặt') {
        // Bàn đã đặt: hiển thị nút Mở Bàn + Hủy Đặt
        document.getElementById('chiTietBanNoidung').innerHTML = `
            <div class="empty-state flex flex-col items-center">
                <img src="img/clock_3d.png" class="w-20 h-20 object-contain rounded-2xl drop-shadow-md mb-3 border border-primary/20">
                <p style="margin-bottom:0.5rem; color:var(--mau-chu);">${ban.TenBan} hiện đang <strong style="color:#fbbf24">Đặt trước</strong></p>
                <p class="text-nhat">Bàn đã được đặt trước. Nhấn "Mở Bàn" để đón khách hoặc "Hủy Đặt" để hủy.</p>
            </div>`;
        document.getElementById('chiTietBanFooter').innerHTML = `
            <button class="btn btn-danger" onclick="huyDatBan(${ban.Id})">Hủy Đặt</button>
            <button class="btn btn-primary btn-lg flex items-center justify-center gap-1" onclick="moBan(${ban.Id})">
                <img src="img/add_3d.png" class="w-4 h-4 object-cover rounded-sm inline-block mr-1"> Mở Bàn Đón Khách
            </button>`;
    } else {
        // Bàn có khách: tải hóa đơn và hiển thị
        document.getElementById('chiTietBanNoidung').innerHTML = '<div class="spinner"></div>';
        document.getElementById('chiTietBanFooter').innerHTML = '';
        moModal('modalChiTietBan');
        await hienThiHoaDonCuaBan(ban.Id);
        return;
    }

    moModal('modalChiTietBan');
}

// Hiển thị hóa đơn của bàn đang có khách
async function hienThiHoaDonCuaBan(banId) {
    try {
        const res = await fetch(`${API}/ban/${banId}/hoadon`);
        if (!res.ok) {
            document.getElementById('chiTietBanNoidung').innerHTML =
                '<p class="text-center text-nhat" style="padding:2rem;">Không tìm thấy hóa đơn.</p>';
            return;
        }
        const data = await res.json();
        const { hoaDon, chiTiet } = data;

        const danhSachMon = chiTiet.length === 0
            ? '<p class="text-nhat text-center" style="padding:1rem;">Chưa có món nào được gọi.</p>'
            : `<div class="table-wrapper">
                <table>
                    <thead>
                        <tr><th>Món</th><th>Ghi Chú</th><th>SL</th><th>Thành Tiền</th></tr>
                    </thead>
                    <tbody>
                        ${chiTiet.map(ct => `
                            <tr>
                                <td>${ct.TenSanPham}</td>
                                <td><span class="text-nhat">${ct.ThuocTinhThem || '-'}</span></td>
                                <td class="text-center">${ct.SoLuong}</td>
                                <td class="text-chinh fw-bold">${formatTien(ct.ThanhTien)}</td>
                            </tr>`).join('')}
                    </tbody>
                </table>
               </div>`;

        document.getElementById('chiTietBanNoidung').innerHTML = `
            <p class="text-nhat mb-1">Mở lúc: ${formatThoiGian(hoaDon.ThoiGianTao)}</p>
            ${danhSachMon}
            <div class="tong-tien-box">
                <div class="tong-tien-row tong">
                    <span class="flex items-center gap-1"><img src="img/money_3d.png" class="w-4 h-4 object-cover rounded-sm inline-block"> Tổng cộng:</span>
                    <span>${formatTien(hoaDon.TongTien)}</span>
                </div>
            </div>`;

        document.getElementById('chiTietBanFooter').innerHTML = `
            <a href="order.html" class="btn btn-info flex items-center gap-1 justify-center"><img src="img/pos_3d.png" class="w-4 h-4 object-cover inline-block"> Gọi thêm món</a>
            <button class="btn btn-success flex items-center gap-1 justify-center" onclick="thanhToanNhanhTuModal(${banId})">
                <img src="img/check_3d.png" class="w-4 h-4 object-cover inline-block"> Thanh Toán Ngay
            </button>`;
    } catch (err) {
        document.getElementById('chiTietBanNoidung').innerHTML =
            `<div class="alert alert-error">⚠️ Lỗi tải hóa đơn: ${err.message}</div>`;
    }
}

// Mở bàn (tạo hóa đơn mới)
async function moBan(banId) {
    try {
        const res = await fetch(`${API}/ban/${banId}/mo`, { method: 'POST' });
        const data = await res.json();
        if (res.ok) {
            dongModal('modalChiTietBan');
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSachBan();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// Thanh toán nhanh từ modal chi tiết bàn
async function thanhToanNhanhTuModal(banId) {
    if (!confirm('Xác nhận thanh toán và đóng bàn này?')) return;
    try {
        const res = await fetch(`${API}/ban/${banId}/thanhtoan`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ VAT: 0, GiamGia: 0, PhuongThucThanhToan: 'TienMat' })
        });
        const data = await res.json();
        if (res.ok) {
            dongModal('modalChiTietBan');
            hienThiThongBao(`✅ ${data.thongBao} | Tổng: ${formatTien(data.tongTien)}`, 'success');
            taiDanhSachBan();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---- THÊM BÀN ----
function moModalThemBan() {
    idBanDangSua = null;
    document.getElementById('modalBanTieuDe').textContent = 'Thêm Bàn Mới';
    document.getElementById('txtTenBan').value = '';
    document.getElementById('cboTrangThaiBan').value = 'Trống';
    moModal('modalBan');
}

function moModalSuaBan(banId) {
    const ban = danhSachBan.find(b => b.Id === banId);
    if (!ban) return;
    idBanDangSua = banId;
    document.getElementById('modalBanTieuDe').textContent = `Sửa ${ban.TenBan}`;
    document.getElementById('txtTenBan').value = ban.TenBan;
    document.getElementById('cboTrangThaiBan').value = ban.TrangThai;
    moModal('modalBan');
}

async function luuBan() {
    const tenBan = document.getElementById('txtTenBan').value.trim();
    const trangThai = document.getElementById('cboTrangThaiBan').value;

    // Validation phía client
    if (!tenBan) {
        hienThiThongBao('Vui lòng nhập tên bàn!', 'error');
        document.getElementById('txtTenBan').focus();
        return;
    }

    const payload = { TenBan: tenBan, TrangThai: trangThai };
    const isEdit = idBanDangSua !== null;

    try {
        const url = isEdit ? `${API}/ban/${idBanDangSua}` : `${API}/ban`;
        const method = isEdit ? 'PUT' : 'POST';

        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        const data = await res.json();

        if (res.ok) {
            dongModal('modalBan');
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSachBan();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// Đặt bàn (đặt trước)
async function datBan(banId) {
    try {
        const res = await fetch(`${API}/ban/${banId}/dat`, { method: 'POST' });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSachBan();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// Hủy đặt bàn
async function huyDatBan(banId) {
    if (!confirm('Xác nhận hủy đặt bàn này?')) return;
    try {
        const res = await fetch(`${API}/ban/${banId}/huy-dat`, { method: 'POST' });
        const data = await res.json();
        if (res.ok) {
            dongModal('modalChiTietBan');
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSachBan();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// Xóa bàn
async function xoaBan(banId, tenBan) {
    if (!confirm(`Xác nhận xóa "${tenBan}"? Thao tác này không thể hoàn tác!`)) return;
    try {
        const res = await fetch(`${API}/ban/${banId}`, { method: 'DELETE' });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSachBan();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch (err) {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---- Hàm tiện ích ----

// Hiển thị thông báo nội tuyến (giữ cho khu vực #thongBaoKhuVuc)
// + Toast notification nổi góc phải
function hienThiThongBao(noiDung, loai = 'success') {
    // Inline notification (bảo toàn cho học thuật)
    const kv = document.getElementById('thongBaoKhuVuc');
    if (kv) {
        kv.innerHTML = `<div class="alert alert-${loai}">${noiDung}</div>`;
        setTimeout(() => kv.innerHTML = '', 4000);
    }
    // Toast notification (UX cao cấp)
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
    
    // Icon theo loại thông báo
    const icon = loai === 'success' ? '✅' : '❌';
    toast.innerHTML = `<span style="font-size:1.1rem;">${icon}</span><span>${noiDung}</span>`;
    container.appendChild(toast);

    // Tự động biến mất sau 4 giây với hiệu ứng slide out
    setTimeout(() => {
        toast.classList.add('toast-out');
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

function formatTien(so) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(so);
}

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
