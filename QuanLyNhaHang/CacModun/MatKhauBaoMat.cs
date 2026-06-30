// ============================================================
// MATKHAUBAO MAT.CS - Tiện ích băm mật khẩu SHA256
// Không lưu mật khẩu thô, chỉ lưu hash để chống rò rỉ
// ============================================================
using System.Security.Cryptography;
using System.Text;

namespace QuanLyNhaHang.CacModun
{
    public static class MatKhauBaoMat
    {
        // Băm SHA256 chuỗi mật khẩu → chuỗi hex 64 ký tự
        public static string BamSHA256(string matKhau)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(matKhau));
            var sb = new StringBuilder();
            foreach (byte b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
