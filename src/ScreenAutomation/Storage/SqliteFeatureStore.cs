using System;
using Microsoft.Data.Sqlite;
using ScreenAutomation.Core;

namespace ScreenAutomation.Storage
{
    /// <summary>
    /// Single-connection SQLite store. For tests, pass ":memory:" and call Init().
    /// </summary>
    public sealed class SqliteFeatureStore : IFeatureStore, IDisposable
    {
        private readonly SqliteConnection _conn;

        /// <param name="pathOrConnectionString">
        /// Use ":memory:" for tests, a file path like "automation.db", or a full SQLite connection string.
        /// </param>
        public SqliteFeatureStore(string pathOrConnectionString = ":memory:")
        {
            var _connString = pathOrConnectionString.Contains('=')
                ? pathOrConnectionString
                : (pathOrConnectionString == ":memory:"
                    ? "Data Source=:memory:"
                    : $"Data Source={pathOrConnectionString}");
            _conn = new SqliteConnection(_connString);
        }

        public void Init()
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            using var cmd = _conn.CreateCommand();
            cmd.CommandText =
            """
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS elements (
                id        TEXT PRIMARY KEY,
                kind      TEXT NOT NULL,
                canonical TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS observations (
                ts         REAL NOT NULL,
                window_id  TEXT NOT NULL,
                element_id TEXT NOT NULL,
                x          INTEGER NOT NULL,
                y          INTEGER NOT NULL,
                w          INTEGER NOT NULL,
                h          INTEGER NOT NULL,
                state      TEXT NOT NULL,
                conf       REAL NOT NULL,
                text       TEXT NULL,
                FOREIGN KEY(element_id) REFERENCES elements(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS signals (
                ts    REAL NOT NULL,
                key   TEXT NOT NULL,
                value TEXT NOT NULL
            );
            """;
            cmd.ExecuteNonQuery();
        }

        public void UpsertElement(string id, string kind, string canonical)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText =
            """
            INSERT INTO elements (id, kind, canonical)
            VALUES ($id, $kind, $canonical)
            ON CONFLICT(id) DO UPDATE SET
                kind = excluded.kind,
                canonical = excluded.canonical;
            """;
            cmd.Parameters.AddWithValue("$id", id);
            cmd.Parameters.AddWithValue("$kind", kind);
            cmd.Parameters.AddWithValue("$canonical", canonical);
            cmd.ExecuteNonQuery();
        }

        public void InsertObservation(
            double ts, string windowId, string elementId,
            int x, int y, int w, int h, string state, double conf, string? text)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText =
            """
            INSERT INTO observations
              (ts, window_id, element_id, x, y, w, h, state, conf, text)
            VALUES
              ($ts, $win, $el, $x, $y, $w, $h, $state, $conf, $text);
            """;
            cmd.Parameters.AddWithValue("$ts", ts);
            cmd.Parameters.AddWithValue("$win", windowId);
            cmd.Parameters.AddWithValue("$el", elementId);
            cmd.Parameters.AddWithValue("$x", x);
            cmd.Parameters.AddWithValue("$y", y);
            cmd.Parameters.AddWithValue("$w", w);
            cmd.Parameters.AddWithValue("$h", h);
            cmd.Parameters.AddWithValue("$state", state);
            cmd.Parameters.AddWithValue("$conf", conf);
            cmd.Parameters.AddWithValue("$text", (object?)text ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void InsertSignal(double ts, string key, string value)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO signals(ts, key, value) VALUES($ts, $k, $v);";
            cmd.Parameters.AddWithValue("$ts", ts);
            cmd.Parameters.AddWithValue("$k", key);
            cmd.Parameters.AddWithValue("$v", value);
            cmd.ExecuteNonQuery();
        }

        public void Dispose() => _conn.Dispose();
    }
}
