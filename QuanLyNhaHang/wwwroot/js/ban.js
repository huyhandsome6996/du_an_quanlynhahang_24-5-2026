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
    const soCoKhach = dsBan.filter(b => b.TrangThai === 'Có khách').length;

    document.getElementById('soBanTrong').textContent   = soTrong;
    document.getElementById('soBanCoKhach').textContent = soCoKhach;

    if (dsBan.length === 0) {
        luoi.innerHTML = '<div class="empty-state"><span class="empty-icon">🪑</span><p>Chưa có bàn nào. Hãy thêm bàn mới!</p></div>';
        return;
    }

    luoi.innerHTML = dsBan.map(ban => {
        const laTrong = ban.TrangThai === 'Trống';
        return `
        <div class="ban-card ${laTrong ? 'trong' : 'cokhach'}"
             onclick="clickVaoBan(${ban.Id})"
             title="${ban.TenBan} - ${ban.TrangThai}">
            <div class="ban-icon">${laTrong ? '🪑' : '👥'}</div>
            <div class="ban-ten">${ban.TenBan}</div>
            <div class="ban-trangthai">
                <span class="badge ${laTrong ? 'badge-trong' : 'badge-cokhach'}">
                    ${laTrong ? '● Trống' : '● Có khách'}
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
            <div class="empty-state">
                <span class="empty-icon">🪑</span>
                <p style="margin-bottom:0.5rem; color:var(--mau-chu);">${ban.TenBan} hiện đang <strong style="color:var(--mau-xanh)">Trống</strong></p>
                <p class="text-nhat">Nhấn "Mở Bàn" để tạo hóa đơn mới đón khách.</p>
            </div>`;
        document.getElementById('chiTietBanFooter').innerHTML = `
            <button class="btn btn-secondary" onclick="dongModal('modalChiTietBan')">Hủy</button>
            <button class="btn btn-primary btn-lg" onclick="moBan(${ban.Id})">
                🚀 Mở Bàn Đón Khách
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
                    <span>💰 Tổng cộng:</span>
                    <span>${formatTien(hoaDon.TongTien)}</span>
                </div>
            </div>`;

        document.getElementById('chiTietBanFooter').innerHTML = `
            <a href="order.html" class="btn btn-info">🛒 Gọi thêm món</a>
            <button class="btn btn-success" onclick="thanhToanNhanhTuModal(${banId})">
                ✅ Thanh Toán Ngay
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
        const res = await fetch(`${API}/ban/${banId}/thanhtoan`, { method: 'POST' });
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
function hienThiThongBao(noiDung, loai = 'success') {
    const kv = document.getElementById('thongBaoKhuVuc');
    kv.innerHTML = `<div class="alert alert-${loai}">${noiDung}</div>`;
    setTimeout(() => kv.innerHTML = '', 4000);
}

function formatTien(so) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(so);
}

function formatThoiGian(chuoi) {
    if (!chuoi) return '-';
    return new Date(chuoi).toLocaleString('vi-VN');
}

// MODAL BEHAVIOR: KHÔNG cho đóng khi click bên ngoài (ShowDialog)
// Chỉ đóng khi nhấn nút Đóng/Hủy bên trong modal
function moModal(id) {
    document.getElementById(id).classList.add('show');
}
// => KHÔNG thêm event click vào overlay
