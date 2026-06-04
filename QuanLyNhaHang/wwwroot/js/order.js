// ============================================================
// ORDER.JS - Logic trang Gọi Món & Thanh Toán (order.html)
// ============================================================

const API = 'http://localhost:5000/api';
let danhSachMenu = [];
let monDangChon = null;
let hoaDonHienTai = null;

const GOI_Y_THUC_AN   = ['Phần lớn', 'Không hành', 'Ít cay', 'Không cay'];
const GOI_Y_NUOC_UONG = ['Lon', 'Ly', 'Ít đá', 'Nhiều đá', 'Không đường'];

document.addEventListener('DOMContentLoaded', async () => {
    await taiDanhSachBan();
    await taiMenu();
});

async function taiDanhSachBan() {
    try {
        const res = await fetch(`${API}/ban`);
        const dsBan = await res.json();
        const select = document.getElementById('cboBan');
        select.innerHTML = '<option value="">-- Chọn bàn cần phục vụ --</option>';
        dsBan.forEach(b => {
            const opt = document.createElement('option');
            opt.value = b.Id;
            opt.textContent = `${b.TenBan} (${b.TrangThai})`;
            select.appendChild(opt);
        });
    } catch {
        hienThiThongBao('Không kết nối được server!', 'error');
    }
}

async function taiMenu() {
    try {
        const res = await fetch(`${API}/sanpham/dangban`);
        danhSachMenu = await res.json();
        hienThiMenu(danhSachMenu);
    } catch (err) { console.error(err); }
}

function hienThiMenu(ds) {
    const kv = document.getElementById('menuDanhSach');
    if (!ds.length) { kv.innerHTML = '<p class="text-nhat text-center" style="padding:1rem;">Không có món nào.</p>'; return; }
    kv.innerHTML = ds.map(sp => {
        const imgUrl = sp.HinhAnh ? sp.HinhAnh : 'img/logo.png';
        return `
        <div class="mon-item cursor-pointer" onclick="chonMon(${sp.Id})" style="padding: 0.5rem; gap: 0.75rem; justify-content: flex-start; align-items: center; display: flex;">
            <div class="w-12 h-12 rounded-lg overflow-hidden flex-shrink-0 bg-surface-container-highest border border-white/5">
                <img src="${imgUrl}" alt="${sp.TenSanPham}" class="w-full h-full object-cover" onerror="this.src='img/logo.png'">
            </div>
            <div class="flex-1 min-w-0">
                <div class="mon-ten text-sm font-semibold truncate text-on-surface" title="${sp.TenSanPham}">${sp.TenSanPham}</div>
                <div class="flex items-center gap-1.5 mt-1 flex-wrap">
                    <span class="badge ${sp.Loai === 'ThucAn' ? 'badge-thucan' : 'badge-nuocuong'}" style="font-size:0.6rem; padding: 0.1rem 0.4rem;">
                        ${sp.Loai === 'ThucAn' ? '🍖 Thức ăn' : '🥤 Nước uống'}
                    </span>
                    <span class="mon-gia text-primary font-bold text-xs">${formatTien(sp.GiaCoBan)}</span>
                </div>
            </div>
        </div>`;
    }).join('');
}

function locMenuTheoLoai(loai) {
    const ds = loai ? danhSachMenu.filter(sp => sp.Loai === loai) : danhSachMenu;
    hienThiMenu(ds);
    document.getElementById('btnTatCa').className   = 'btn btn-sm ' + (loai === '' ? 'btn-primary' : 'btn-secondary');
    document.getElementById('btnThucAn').className  = 'btn btn-sm ' + (loai === 'ThucAn' ? 'btn-primary' : 'btn-secondary');
    document.getElementById('btnNuocUong').className = 'btn btn-sm ' + (loai === 'NuocUong' ? 'btn-primary' : 'btn-secondary');
}

async function chonBan() {
    const banId = document.getElementById('cboBan').value;
    const thongTin = document.getElementById('thongTinBan');
    const khuVucGoiMon = document.getElementById('khuVucGoiMon');

    if (!banId) {
        thongTin.style.display = 'none';
        khuVucGoiMon.style.display = 'none';
        document.getElementById('chuaChonBan').innerHTML = '<div class="flex flex-col items-center justify-center opacity-70"><img src="img/click_3d.png" class="w-12 h-12 object-cover mb-4"></div><p>Chọn bàn để xem hóa đơn</p>';
        document.getElementById('chuaChonBan').style.display = 'block';
        document.getElementById('danhSachMon').style.display = 'none';
        return;
    }

    const res = await fetch(`${API}/ban/${banId}`);
    const ban = await res.json();

    thongTin.style.display = 'block';
    const badge = document.getElementById('badgeTrangThaiBan');
    const thongBaoMoBan = document.getElementById('thongBaoMoBan');

    if (ban.TrangThai === 'Có khách') {
        badge.className = 'badge badge-cokhach';
        badge.textContent = '● Có khách';
        thongBaoMoBan.textContent = '– Đang có hóa đơn mở';
        khuVucGoiMon.style.display = 'block';
        taiLaiHoaDon();
    } else {
        badge.className = 'badge badge-trong';
        badge.textContent = '● Trống';
        thongBaoMoBan.textContent = '– Cần mở bàn trước';
        khuVucGoiMon.style.display = 'none';
        document.getElementById('chuaChonBan').innerHTML = `
            <div class="flex justify-center mb-4"><img src="img/table_3d.png" class="w-16 h-16 object-cover rounded-xl shadow-lg opacity-90"></div>
            <p style="margin-bottom:1rem;">${ban.TenBan} đang <strong style="color:var(--mau-xanh)">Trống</strong></p>
            <button class="bg-gradient-to-r from-primary to-primary-soft hover:from-primary-hover hover:to-primary text-surface px-6 py-3 rounded-xl font-bold uppercase tracking-wider transition-all shadow-lg active:scale-95 flex items-center justify-center gap-2 mx-auto border-none cursor-pointer" onclick="moBanVaGoiMon(${banId})">
                <img src="img/add_3d.png" class="w-5 h-5 object-cover rounded-sm shadow-sm"> Mở Bàn Đón Khách
            </button>`;
        document.getElementById('chuaChonBan').style.display = 'block';
        document.getElementById('danhSachMon').style.display = 'none';
    }
}

async function moBanVaGoiMon(banId) {
    try {
        const res = await fetch(`${API}/ban/${banId}/mo`, { method: 'POST' });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ ${data.thongBao}`, 'success');
            await taiDanhSachBan();
            document.getElementById('cboBan').value = banId;
            await chonBan();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch { hienThiThongBao('Lỗi kết nối server!', 'error'); }
}

async function taiLaiHoaDon() {
    const banId = document.getElementById('cboBan').value;
    if (!banId) return;
    try {
        const res = await fetch(`${API}/ban/${banId}/hoadon`);
        if (!res.ok) {
            document.getElementById('chuaChonBan').style.display = 'block';
            document.getElementById('danhSachMon').style.display = 'none';
            return;
        }
        const data = await res.json();
        hoaDonHienTai = data.hoaDon;
        hienThiHoaDon(data.hoaDon, data.chiTiet);
    } catch { hienThiThongBao('Lỗi tải hóa đơn!', 'error'); }
}

function hienThiHoaDon(hd, chiTiet) {
    document.getElementById('chuaChonBan').style.display = 'none';
    document.getElementById('danhSachMon').style.display = 'block';

    const bang = document.getElementById('bangChiTietHoaDon');
    if (!chiTiet.length) {
        bang.innerHTML = `<div class="empty-state flex flex-col items-center justify-center py-8">
            <img src="img/menu_book_3d.png" class="w-14 h-14 object-cover rounded-xl mb-3 shadow-md opacity-80">
            <p class="text-on-surface-variant text-sm">Chưa có món nào. Hãy chọn món từ menu!</p></div>`;
    } else {
        bang.innerHTML = `<div class="table-wrapper">
            <table>
                <thead><tr><th>Món</th><th>Ghi Chú</th><th>SL</th><th>Thành Tiền</th><th></th></tr></thead>
                <tbody>${chiTiet.map(ct => `
                    <tr>
                        <td><strong>${ct.TenSanPham}</strong></td>
                        <td><span class="text-nhat">${ct.ThuocTinhThem || '-'}</span></td>
                        <td class="text-center">${ct.SoLuong}</td>
                        <td class="text-chinh fw-bold">${formatTien(ct.ThanhTien)}</td>
                        <td><button class="bg-white/[0.05] hover:bg-red-500/20 border border-red-500/10 p-2 rounded-lg transition-all active:scale-95 cursor-pointer" onclick="xoaMon(${ct.Id})" title="Xóa"><img src="img/close_3d.png" class="w-3.5 h-3.5 object-cover"></button></td>
                    </tr>`).join('')}
                </tbody>
            </table></div>`;
    }
    document.getElementById('tongTienHienThi').textContent = formatTien(hd.TongTien);
}

function chonMon(sanPhamId) {
    const sp = danhSachMenu.find(m => m.Id === sanPhamId);
    if (!sp) return;
    monDangChon = sp;
    document.getElementById('tenMonDangThem').textContent = `${sp.TenSanPham} - ${formatTien(sp.GiaCoBan)}`;
    document.getElementById('txtSoLuong').value = 1;
    document.getElementById('txtThuocTinhThem').value = '';
    document.getElementById('formThemMon').style.display = 'block';

    const goiY = sp.Loai === 'ThucAn' ? GOI_Y_THUC_AN : GOI_Y_NUOC_UONG;
    document.getElementById('goiYTuyChon').innerHTML = goiY.map(opt =>
        `<button class="btn btn-sm btn-secondary" onclick="chonGoiY('${opt}')">${opt}</button>`).join('');
}

function chonGoiY(text) { document.getElementById('txtThuocTinhThem').value = text; }
function huyChonMon()   { monDangChon = null; document.getElementById('formThemMon').style.display = 'none'; }

async function themMon() {
    if (!monDangChon || !hoaDonHienTai) {
        hienThiThongBao('Vui lòng chọn bàn và món trước!', 'error'); return;
    }
    const soLuong = parseInt(document.getElementById('txtSoLuong').value);
    const thuocTinhThem = document.getElementById('txtThuocTinhThem').value.trim();

    if (!soLuong || soLuong <= 0) {
        hienThiThongBao('Số lượng phải lớn hơn 0!', 'error');
        document.getElementById('txtSoLuong').focus(); return;
    }

    try {
        const res = await fetch(`${API}/hoadon/${hoaDonHienTai.Id}/them-mon`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ SanPhamId: monDangChon.Id, SoLuong: soLuong, ThuocTinhThem: thuocTinhThem })
        });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ Đã thêm! ${data.moTaPhuPhi} | Thành tiền: ${formatTien(data.thanhTien)}`, 'success');
            huyChonMon();
            taiLaiHoaDon();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch { hienThiThongBao('Lỗi kết nối server!', 'error'); }
}

async function xoaMon(chiTietId) {
    if (!confirm('Xóa món này khỏi hóa đơn?')) return;
    try {
        const res = await fetch(`${API}/chitiethoadon/${chiTietId}`, { method: 'DELETE' });
        const data = await res.json();
        if (res.ok) { hienThiThongBao(`✅ ${data.thongBao}`, 'success'); taiLaiHoaDon(); }
        else hienThiThongBao(`❌ ${data.thongBao}`, 'error');
    } catch { hienThiThongBao('Lỗi kết nối server!', 'error'); }
}

async function thanhToan() {
    const banId = document.getElementById('cboBan').value;
    if (!banId) return;
    if (!confirm(`Xác nhận thanh toán ${formatTien(hoaDonHienTai?.TongTien || 0)} và đóng bàn?`)) return;

    try {
        const res = await fetch(`${API}/ban/${banId}/thanhtoan`, { method: 'POST' });
        const data = await res.json();
        if (res.ok) {
            hienThiThongBao(`✅ ${data.thongBao} | Đã thu: ${formatTien(data.tongTien)}`, 'success');
            document.getElementById('cboBan').value = '';
            document.getElementById('thongTinBan').style.display = 'none';
            document.getElementById('khuVucGoiMon').style.display = 'none';
            document.getElementById('chuaChonBan').innerHTML = '<div class="flex flex-col items-center justify-center opacity-70"><img src="img/click_3d.png" class="w-12 h-12 object-cover mb-4"></div><p>Chọn bàn để xem hóa đơn</p>';
            document.getElementById('chuaChonBan').style.display = 'block';
            document.getElementById('danhSachMon').style.display = 'none';
            hoaDonHienTai = null;
            await taiDanhSachBan();
        } else {
            hienThiThongBao(`❌ ${data.thongBao}`, 'error');
        }
    } catch { hienThiThongBao('Lỗi kết nối server!', 'error'); }
}

function hienThiThongBao(noiDung, loai = 'success') {
    const kv = document.getElementById('thongBaoKhuVuc');
    kv.innerHTML = `<div class="alert alert-${loai}">${noiDung}</div>`;
    setTimeout(() => kv.innerHTML = '', 5000);
}

function formatTien(so) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(so);
}
