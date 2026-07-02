// ============================================================
// TẦNG DAL - BanDAL (Access + OLE DB)
// ============================================================
using System.Data.OleDb;
using QuanLyNhaHang.DAL.Interfaces;
using QuanLyNhaHang.Entities;

namespace QuanLyNhaHang.DAL
{
    public class BanDAL : IBanDAL
    {
        private readonly string _conn = DatabaseHelper.ConnectionString;

        public List<Ban> LayTatCa()
        {
            var ds = new List<Ban>();
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("SELECT Id, TenBan, TrangThai FROM Ban ORDER BY Id", c);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                ds.Add(new Ban { Id = r.GetInt32(0), TenBan = r.GetString(1), TrangThai = r.GetString(2) });
            return ds;
        }

        public Ban? LayTheoId(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("SELECT Id, TenBan, TrangThai FROM Ban WHERE Id = @id", c);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            if (r.Read())
                return new Ban { Id = r.GetInt32(0), TenBan = r.GetString(1), TrangThai = r.GetString(2) };
            return null;
        }

        public void Them(Ban ban)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            // Kiểm tra trùng tên bàn
            using var chk = new OleDbCommand("SELECT COUNT(*) FROM Ban WHERE TenBan = @ten", c);
            chk.Parameters.AddWithValue("@ten", ban.TenBan);
            if ((int)chk.ExecuteScalar() > 0)
                throw new Exception("Tên bàn đã tồn tại! Vui lòng đặt tên khác.");

            using var cmd = new OleDbCommand("INSERT INTO Ban (TenBan, TrangThai) VALUES (@ten, @tt)", c);
            cmd.Parameters.AddWithValue("@ten", ban.TenBan);
            cmd.Parameters.AddWithValue("@tt", ban.TrangThai);
            cmd.ExecuteNonQuery();
        }

        public void Sua(Ban ban)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            using var chk = new OleDbCommand("SELECT COUNT(*) FROM Ban WHERE TenBan = @ten AND Id <> @id", c);
            chk.Parameters.AddWithValue("@ten", ban.TenBan);
            chk.Parameters.AddWithValue("@id", ban.Id);
            if ((int)chk.ExecuteScalar() > 0)
                throw new Exception("Tên bàn đã tồn tại! Vui lòng đặt tên khác.");

            using var cmd = new OleDbCommand("UPDATE Ban SET TenBan = @ten, TrangThai = @tt WHERE Id = @id", c);
            cmd.Parameters.AddWithValue("@ten", ban.TenBan);
            cmd.Parameters.AddWithValue("@tt", ban.TrangThai);
            cmd.Parameters.AddWithValue("@id", ban.Id);
            cmd.ExecuteNonQuery();
        }

        public void Xoa(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("DELETE FROM Ban WHERE Id = @id", c);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void CapNhatTrangThai(int id, string trangThai)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("UPDATE Ban SET TrangThai = @tt WHERE Id = @id", c);
            cmd.Parameters.AddWithValue("@tt", trangThai);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
