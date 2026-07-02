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
            using var cmd = new OleDbCommand("SELECT Id, TenBan, TrangThai FROM Ban WHERE Id = ?", c);
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            using var r = cmd.ExecuteReader();
            if (r.Read())
                return new Ban { Id = r.GetInt32(0), TenBan = r.GetString(1), TrangThai = r.GetString(2) };
            return null;
        }

        public void Them(Ban ban)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            using var chk = new OleDbCommand("SELECT COUNT(*) FROM Ban WHERE TenBan = ?", c);
            chk.Parameters.Add("@ten", OleDbType.VarWChar).Value = ban.TenBan;
            if ((int)chk.ExecuteScalar()! > 0)
                throw new Exception("Tên bàn đã tồn tại! Vui lòng đặt tên khác.");

            using var cmd = new OleDbCommand("INSERT INTO Ban (TenBan, TrangThai) VALUES (?, ?)", c);
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = ban.TenBan;
            cmd.Parameters.Add("@tt", OleDbType.VarWChar).Value = ban.TrangThai;
            cmd.ExecuteNonQuery();
        }

        public void Sua(Ban ban)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();

            using var chk = new OleDbCommand("SELECT COUNT(*) FROM Ban WHERE TenBan = ? AND Id <> ?", c);
            chk.Parameters.Add("@ten", OleDbType.VarWChar).Value = ban.TenBan;
            chk.Parameters.Add("@id", OleDbType.Integer).Value = ban.Id;
            if ((int)chk.ExecuteScalar()! > 0)
                throw new Exception("Tên bàn đã tồn tại! Vui lòng đặt tên khác.");

            using var cmd = new OleDbCommand("UPDATE Ban SET TenBan = ?, TrangThai = ? WHERE Id = ?", c);
            cmd.Parameters.Add("@ten", OleDbType.VarWChar).Value = ban.TenBan;
            cmd.Parameters.Add("@tt", OleDbType.VarWChar).Value = ban.TrangThai;
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = ban.Id;
            cmd.ExecuteNonQuery();
        }

        public void Xoa(int id)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("DELETE FROM Ban WHERE Id = ?", c);
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            cmd.ExecuteNonQuery();
        }

        public void CapNhatTrangThai(int id, string trangThai)
        {
            using var c = new OleDbConnection(_conn);
            c.Open();
            using var cmd = new OleDbCommand("UPDATE Ban SET TrangThai = ? WHERE Id = ?", c);
            cmd.Parameters.Add("@tt", OleDbType.VarWChar).Value = trangThai;
            cmd.Parameters.Add("@id", OleDbType.Integer).Value = id;
            cmd.ExecuteNonQuery();
        }
    }
}
