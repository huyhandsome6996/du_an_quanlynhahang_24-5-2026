// ============================================================
// BAN.JS — Logic trang Sơ Đồ Bàn (index.html)
// ------------------------------------------------------------
// Chức năng chính:
//   1. Tải danh sách bàn từ API
//   2. Tìm kiếm bàn theo tên
//   3. Hiển thị bàn dưới dạng lưới (3 trạng thái: Trống / Đã đặt / Có khách)
//   4. Click vào bàn → mở modal chi tiết
//   5. Thêm / Sửa / Xoá bàn
//   6. Mở bàn (tạo hóa đơn mới)
//   7. Thanh toán nhanh từ modal
// ============================================================

// Biến toàn cục của trang
let danhSachBan = [];      // Cache toàn bộ danh sách bàn (để lọc/tìm kiếm không cần gọi API lại)
let idBanDangSua = null;   // Id bàn đang sửa (null = đang ở chế độ "Thêm mới")

// ---------- KHỞI ĐỘNG ----------
// Khi trang tải xong → gọi taiDanhSachBan để load dữ liệu
document.addEventListener('DOMContentLoaded', () => {
    // Nếu là quản trị thì mới hiện nút "Thêm Bàn Mới"
    if (sessionStorage.getItem('vst_role') === 'QuanTri') {
        const btnThemBan = document.getElementById('btnThemBan');
        if (btnThemBan) btnThemBan.style.display = 'flex';
    }
    taiDanhSachBan();
});

// ---------- 1. TẢI DANH SÁCH BÀN TỪ API ----------
async function taiDanhSachBan() {
    // Hiện spinner trong lúc chờ API
    document.getElementById('luoiBan').innerHTML = '<div class="col-span-full flex justify-center py-12"><div class="spinner"></div></div>';
    try {
        // Gọi GET /api/ban — server trả về mảng JSON
        const res = await fetch(`${API}/ban`);
        danhSachBan = await res.json();   // Parse JSON → gán vào biến toàn cục
        hienThiBan(danhSachBan);          // Hiển thị lên UI
    } catch {
        // Lỗi kết nối → hiện thông báo
        hienThiThongBao('Không kết nối được server! Hãy chạy dotnet run.', 'error');
        document.getElementById('luoiBan').innerHTML = '';
    }
}

// ---------- 2. TÌM KIẾM BÀN THEO TÊN ----------
function timKiemBan() {
    // Lấy từ khoá + chuyển thường để search không phân biệt hoa thường
    const tuKhoa = document.getElementById('txtTimKiemBan').value.trim().toLowerCase();
    if (!tuKhoa) return hienThiBan(danhSachBan);   // Không có từ khoá → hiện tất cả
    // Lọc những bàn có TenBan chứa từ khoá
    const dsLoc = danhSachBan.filter(b => b.TenBan.toLowerCase().includes(tuKhoa));
    hienThiBan(dsLoc);
}

// ---------- 3. VẼ LƯỚI BÀN LÊN GIAO DIỆN ----------
function hienThiBan(dsBan) {
    const luoi = document.getElementById('luoiBan');

    // Cập nhật 3 ô thống kê nhanh (số bàn Trống / Đã đặt / Có khách)
    document.getElementById('soBanTrong').textContent   = dsBan.filter(b => b.TrangThai === 'Trống').length;
    document.getElementById('soBanDaDat').textContent   = dsBan.filter(b => b.TrangThai === 'Đã đặt').length;
    document.getElementById('soBanCoKhach').textContent = dsBan.filter(b => b.TrangThai === 'Có khách').length;

    // Trường hợp không có bàn
    if (dsBan.length === 0) {
        luoi.innerHTML = `
            <div class="col-span-full empty-state flex flex-col items-center">
                <img src="img/chair_3d.png" class="w-16 h-16 object-contain rounded-xl mb-2">
                <p>Chưa có bàn nào. Hãy thêm bàn mới!</p>
            </div>`;
        return;
    }

    // Tạo HTML cho từng bàn — dùng map + template literal
    luoi.innerHTML = dsBan.map(ban => {
        // Phân loại trạng thái để chọn CSS class
        const laTrong = ban.TrangThai === 'Trống';
        const laDaDat = ban.TrangThai === 'Đã đặt';

        // Chọn class CSS theo trạng thái (mỗi trạng thái 1 màu)
        const cssClass = laTrong ? 'trong' : (laDaDat ? 'dadat' : 'cokhach');
        const badgeClass = laTrong ? 'badge-trong' : (laDaDat ? 'badge-dadat' : 'badge-cokhach');
        const badgeText  = laTrong ? '● Trống'   : (laDaDat ? '● Đã đặt'   : '● Có khách');

        // Chọn icon theo trạng thái
        const icon = laTrong ? 'chair_3d.png' : (laDaDat ? 'click_3d.png' : 'user_3d.png');

        // Trả về HTML của 1 card bàn
        // onclick="clickVaoBan(${ban.Id})" → click vào card sẽ mở modal chi tiết
        // event.stopPropagation() ở 2 nút Sửa/Xoá để không trigger click của card
        
        // Chỉ Quản Trị mới thấy nút Sửa và Xóa bàn
        let actionButtons = '';
        if (sessionStorage.getItem('vst_role') === 'QuanTri') {
            actionButtons = `
            <div style="margin-top:0.75rem; display:flex; gap:0.35rem; justify-content:center;">
                <button class="btn btn-sm btn-info" onclick="event.stopPropagation(); moModalSuaBan(${ban.Id})">✏️</button>
                <button class="btn btn-sm btn-danger" onclick="event.stopPropagation(); xoaBan(${ban.Id}, '${ban.TenBan}')">🗑️</button>
            </div>`;
        }

        return `
        <div class="ban-card ${cssClass}" onclick="clickVaoBan(${ban.Id})" title="${ban.TenBan} - ${ban.TrangThai}">
            <div class="flex justify-center mb-2">
                <img src="img/${icon}" class="w-12 h-12 object-contain rounded-lg border border-primary/20 shadow-md">
            </div>
            <div class="ban-ten">${ban.TenBan}</div>
            <div><span class="badge ${badgeClass}">${badgeText}</span></div>
            ${actionButtons}
        </div>`;
    }).join('');   // Nối mảng thành chuỗi
}

// ---------- 4. CLICK VÀO BÀN → MỞ MODAL CHI TIẾT ----------
async function clickVaoBan(banId) {
    // Tìm bàn trong cache
    const ban = danhSachBan.find(b => b.Id === banId);
    if (!ban) return;

    // Set tiêu đề modal = tên bàn
    document.getElementById('chiTietBanTieuDe').textContent = ban.TenBan;

    // 3 nhánh xử lý theo trạng thái bàn
    if (ban.TrangThai === 'Trống') {
        // Bàn trống → hiện nút "Mở Bàn"
        document.getElementById('chiTietBanNoidung').innerHTML = `
            <div class="empty-state flex flex-col items-center">
                <img src="img/chair_3d.png" class="w-20 h-20 object-contain rounded-2xl mb-3 border border-primary/20">
                <p style="margin-bottom:0.5rem; color:var(--mau-chu);">
                    ${ban.TenBan} hiện đang <strong style="color:var(--mau-xanh)">Trống</strong>
                </p>
                <p class="text-nhat">Nhấn "Mở Bàn" để tạo hóa đơn mới đón khách.</p>
            </div>`;
        document.getElementById('chiTietBanFooter').innerHTML = `
            <button class="btn btn-secondary" onclick="dongModal('modalChiTietBan')">Hủy</button>
            <button class="btn btn-primary btn-lg flex items-center gap-1" onclick="moBan(${ban.Id})">
                <img src="img/add_3d.png" class="w-4 h-4 object-cover rounded-sm"> Mở Bàn Đón Khách
            </button>`;

    } else if (ban.TrangThai === 'Đã đặt') {
        // Bàn đã đặt → hiện 2 nút "Hủy Đặt" + "Mở Bàn"
        document.getElementById('chiTietBanNoidung').innerHTML = `
            <div class="empty-state flex flex-col items-center">
                <img src="img/click_3d.png" class="w-20 h-20 object-contain rounded-2xl mb-3 border border-primary/20">
                <p style="margin-bottom:0.5rem; color:var(--mau-chu);">
                    ${ban.TenBan} hiện đang <strong style="color:var(--mau-vang)">Đặt trước</strong>
                </p>
                <p class="text-nhat">Nhấn "Mở Bàn" để đón khách hoặc "Hủy Đặt" để hủy.</p>
            </div>`;
        document.getElementById('chiTietBanFooter').innerHTML = `
            <button class="btn btn-danger" onclick="huyDatBan(${ban.Id})">Hủy Đặt</button>
            <button class="btn btn-primary btn-lg flex items-center gap-1" onclick="moBan(${ban.Id})">
                <img src="img/add_3d.png" class="w-4 h-4 object-cover rounded-sm"> Mở Bàn Đón Khách
            </button>`;

    } else {
        // Bàn có khách → tải hóa đơn và hiển thị chi tiết
        document.getElementById('chiTietBanNoidung').innerHTML = '<div class="spinner"></div>';
        document.getElementById('chiTietBanFooter').innerHTML = '';
        moModal('modalChiTietBan');   // Mở modal trước để user thấy spinner
        await hienThiHoaDonCuaBan(ban.Id);   // Gọi API lấy hóa đơn
        return;
    }

    moModal('modalChiTietBan');
}

// ---------- 5. HIỂN THỊ HÓA ĐƠN CỦA BÀN ĐANG CÓ KHÁCH ----------
async function hienThiHoaDonCuaBan(banId) {
    try {
        // Gọi GET /api/ban/{id}/hoadon — trả về { hoaDon, chiTiet }
        const res = await fetch(`${API}/ban/${banId}/hoadon`);
        if (!res.ok) {
            // Bàn chưa có hóa đơn (lẻ) → hiện thông báo
            document.getElementById('chiTietBanNoidung').innerHTML =
                '<p class="text-nhat text-center" style="padding:2rem;">Không tìm thấy hóa đơn.</p>';
            return;
        }
        const { hoaDon, chiTiet } = await res.json();

        // Bảng chi tiết các món đã gọi
        const danhSachMon = chiTiet.length === 0
            ? '<p class="text-nhat text-center" style="padding:1rem;">Chưa có món nào được gọi.</p>'
            : `<div class="table-wrapper">
                <table>
                    <thead><tr><th>Món</th><th>Ghi Chú</th><th>SL</th><th>Thành Tiền</th></tr></thead>
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

        // Set nội dung modal: thời gian mở + bảng món + tổng tiền
        document.getElementById('chiTietBanNoidung').innerHTML = `
            <p class="text-nhat mb-1">Mở lúc: ${formatThoiGian(hoaDon.ThoiGianTao)}</p>
            ${danhSachMon}
            <div class="tong-tien-box">
                <div class="tong-tien-row tong">
                    <span class="flex items-center gap-1"><img src="img/money_3d.png" class="w-4 h-4 object-cover rounded-sm"> Tổng cộng:</span>
                    <span>${formatTien(hoaDon.TongTien)}</span>
                </div>
            </div>`;

        // Footer: 2 nút — "Gọi thêm món" (sang trang order) + "Thanh Toán Ngay"
        document.getElementById('chiTietBanFooter').innerHTML = `
            <a href="order.html?banId=${banId}" class="btn btn-info flex items-center gap-1 justify-center">
                <img src="img/pos_3d.png" class="w-4 h-4 object-cover"> Gọi thêm món
            </a>
            <button class="btn btn-success flex items-center gap-1 justify-center" onclick="thanhToanNhanhTuModal(${banId})">
                <img src="img/check_3d.png" class="w-4 h-4 object-cover"> Thanh Toán Ngay
            </button>`;
    } catch (err) {
        // Lỗi tải → hiện thông báo lỗi trong modal
        document.getElementById('chiTietBanNoidung').innerHTML =
            `<div class="alert alert-error">⚠️ Lỗi tải hóa đơn: ${err.message}</div>`;
    }
}

// ---------- 6. MỞ BÀN (tạo hóa đơn mới) ----------
async function moBan(banId) {
    try {
        // POST /api/ban/{id}/mo — server tạo HóaDon mới + set Bàn = "Có khách"
        const res = await fetch(`${API}/ban/${banId}/mo`, { method: 'POST' });
        const data = await res.json();
        if (res.ok) {
            dongModal('modalChiTietBan');              // Đóng modal
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSachBan();                           // Tải lại sơ đồ bàn
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 7. THANH TOÁN NHANH TỪ MODAL CHI TIẾT BÀN ----------
async function thanhToanNhanhTuModal(banId) {
    // Hỏi xác nhận trước khi thanh toán
    if (!confirm('Xác nhận thanh toán và đóng bàn này?')) return;
    try {
        // POST /api/ban/{id}/thanhtoan — body chứa VAT=0, GiamGia=0, PTTT=TienMat
        // (Thanh toán nhanh không áp dụng VAT/GiamGia)
        const res = await fetch(`${API}/ban/${banId}/thanhtoan`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ VAT: 0, GiamGia: 0, PhuongThucThanhToan: 'TienMat' })
        });
        const data = await res.json();
        if (res.ok) {
            dongModal('modalChiTietBan');
            hienThiThongBao(`✅ ${data.thongBao} | Tổng: ${formatTien(data.tongTien)}`, 'success');
            taiDanhSachBan();   // Refresh sơ đồ
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 8. MODAL THÊM BÀN ----------
function moModalThemBan() {
    idBanDangSua = null;   // null = chế độ Thêm mới (không phải Sửa)
    document.getElementById('modalBanTieuDe').textContent = 'Thêm Bàn Mới';
    document.getElementById('txtTenBan').value = '';
    document.getElementById('cboTrangThaiBan').value = 'Trống';   // Mặc định = Trống
    moModal('modalBan');
}

// ---------- 9. MODAL SỬA BÀN ----------
function moModalSuaBan(banId) {
    // Tìm bàn trong cache
    const ban = danhSachBan.find(b => b.Id === banId);
    if (!ban) return;
    idBanDangSua = banId;   // Lưu Id để biết đang sửa bàn nào
    document.getElementById('modalBanTieuDe').textContent = `Sửa ${ban.TenBan}`;
    document.getElementById('txtTenBan').value = ban.TenBan;
    document.getElementById('cboTrangThaiBan').value = ban.TrangThai;
    moModal('modalBan');
}

// ---------- 10. LƯU BÀN (Thêm hoặc Sửa) ----------
async function luuBan() {
    // Lấy dữ liệu từ form
    const tenBan = document.getElementById('txtTenBan').value.trim();
    const trangThai = document.getElementById('cboTrangThaiBan').value;

    // Validate tên bàn không rỗng
    if (!tenBan) {
        hienThiThongBao('Vui lòng nhập tên bàn!', 'error');
        document.getElementById('txtTenBan').focus();
        return;
    }

    // Tạo payload JSON
    const payload = { TenBan: tenBan, TrangThai: trangThai };
    // Nếu có idBanDangSua → Sửa (PUT), ngược lại → Thêm (POST)
    const isEdit = idBanDangSua !== null;

    try {
        // Chọn URL và method tùy chế độ
        const url    = isEdit ? `${API}/ban/${idBanDangSua}` : `${API}/ban`;
        const method = isEdit ? 'PUT' : 'POST';
        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        const data = await res.json();
        if (res.ok) {
            dongModal('modalBan');   // Đóng modal
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSachBan();        // Refresh danh sách
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 11. HỦY ĐẶT BÀN ----------
async function huyDatBan(banId) {
    if (!confirm('Xác nhận hủy đặt bàn này?')) return;
    try {
        // POST /api/ban/{id}/huy-dat — đổi trạng thái "Đã đặt" → "Trống"
        const res = await fetch(`${API}/ban/${banId}/huy-dat`, { method: 'POST' });
        const data = await res.json();
        if (res.ok) {
            dongModal('modalChiTietBan');
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSachBan();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}

// ---------- 12. XÓA BÀN ----------
async function xoaBan(banId, tenBan) {
    if (!confirm(`Xác nhận xóa "${tenBan}"? Không thể hoàn tác!`)) return;
    try {
        // DELETE /api/ban/{id} — server không cho xoá bàn đang có khách
        const res = await fetch(`${API}/ban/${banId}`, { method: 'DELETE' });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            taiDanhSachBan();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch {
        hienThiThongBao('Lỗi kết nối server!', 'error');
    }
}
