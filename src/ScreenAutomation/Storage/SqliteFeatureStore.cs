namespace ScreenAutomation.Storage
{
    using System;
    using Microsoft.Data.Sqlite;
    using ScreenAutomation.Core;

    public sealed class SqliteFeatureStore : IFeatureStore
    {
        private readonly string _pathOrConn;

        public SqliteFeatureStore(string pathOrConn) => _pathOrConn = pathOrConn;

        private SqliteConnection Open()
        {
            // Allow :memory: for tests, or a file path for prod
            var cs = _pathOrConn.Contains('=') ? _pathOrConn : $"Data Source={_pathOrConn}";
            var con = new SqliteConnection(cs);
            con.Open();
            return con;
        }

        public void Init()
        {
            using var con = Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS elements(
                id TEXT PRIMARY KEY, kind TEXT, canonical_name TEXT,
                first_seen_ts REAL, last_seen_ts REAL);
              CREATE TABLE IF NOT EXISTS observations(
                ts REAL, window_id TEXT, element_id TEXT, x INT, y INT, w INT, h INT, state TEXT, conf REAL, text TEXT);
              CREATE TABLE IF NOT EXISTS signals(
                ts REAL, key TEXT, value TEXT);";
            cmd.ExecuteNonQuery();
        }

        public void UpsertElement(string id, string kind, string canonical)
        {
            using var con = Open();
            var tx = con.BeginTransaction();
            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
            var cmd1 = con.CreateCommand();
            cmd1.CommandText = @"INSERT INTO elements(id, kind, canonical_name, first_seen_ts, last_seen_ts)
                                 VALUES ($id,$k,$c,$t,$t)
                                 ON CONFLICT(id) DO UPDATE SET last_seen_ts=$t;";
            cmd1.Parameters.AddWithValue("$id", id);
            cmd1.Parameters.AddWithValue("$k", kind);
            cmd1.Parameters.AddWithValue("$c", canonical);
            cmd1.Parameters.AddWithValue("$t", now);
            cmd1.ExecuteNonQuery();
            tx.Commit();
        }

        public void InsertObservation(double ts, string windowId, string elementId, int x, int y, int w, int h, string state, double conf, string? text)
        {
            using var con = Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO observations(ts, window_id, element_id, x, y, w, h, state, conf, text)
                                VALUES($ts,$win,$eid,$x,$y,$w,$h,$st,$cf,$tx);";
            cmd.Parameters.AddWithValue("$ts", ts);
            cmd.Parameters.AddWithValue("$win", windowId);
            cmd.Parameters.AddWithValue("$eid", elementId);
            cmd.Parameters.AddWithValue("$x", x);
            cmd.Parameters.AddWithValue("$y", y);
            cmd.Parameters.AddWithValue("$w", w);
            cmd.Parameters.AddWithValue("$h", h);
            cmd.Parameters.AddWithValue("$st", state);
            cmd.Parameters.AddWithValue("$cf", conf);
            cmd.Parameters.AddWithValue("$tx", (object?)text ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void InsertSignal(double ts, string key, string value)
        {
            using var con = Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO signals(ts, key, value) VALUES($ts,$k,$v);";
            cmd.Parameters.AddWithValue("$ts", ts);
            cmd.Parameters.AddWithValue("$k", key);
            cmd.Parameters.AddWithValue("$v", value);
            cmd.ExecuteNonQuery();
        }
    }
}
