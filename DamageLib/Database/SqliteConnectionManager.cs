using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using DamageBot.Events.Database;
using DamageBot.EventSystem;
using DamageBot.Logging;

namespace DamageBot.Database {
    public class SqliteConnectionManager : IConnectionManager, IDisposable {
        private readonly DbConnection connection;
        private DbTransaction transaction;
        private readonly Logger log;
        
        public SqliteConnectionManager() {
            this.log = LogManager.GetLogger(GetType());
            if (!File.Exists("damagebot.sqlite")) {
                log.Info("Creating new database and connecting.");
                this.connection = new SQLiteConnection("Data Source=damagebot.sqlite;Version=3;");
                this.connection.Open();
                this.BuildSchema();
            }
            else {
                log.Info("Connecting to existing database.");
                this.connection = new SQLiteConnection("Data Source=damagebot.sqlite;Version=3;");
                this.connection.Open();
            }
            
            log.Info("Registering query request listeners");
            EventDispatcher.Instance.Register<SelectEvent>(OnSelectRequest);
            EventDispatcher.Instance.Register<InsertEvent>(OnInsertRequest);
            EventDispatcher.Instance.Register<UpdateEvent>(OnUpdateRequest);
            EventDispatcher.Instance.Register<CreateTableEvent>(OnCreateTableRequest);
            EventDispatcher.Instance.Register<DeleteEvent>(OnDeleteRequest);
            log.Info("Creating new database.");
        }

        // TODO Move this into some external initialisation step
        private void BuildSchema() {
            var cmd = this.connection.CreateCommand();

            cmd.CommandText = @"
CREATE TABLE users (
  user_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  username VARCHAR(255) NOT NULL UNIQUE,
  twitch_id VARCHAR(255) NOT NULL,
  first_joined DATETIME NOT NULL,
  last_joined DATETIME NOT NULL
)";
            cmd.ExecuteNonQuery();
            
            cmd.CommandText = @"
CREATE TABLE plugins(
  plugin_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
  plugin_name VARCHAR(255) NOT NULL,
  plugin_author VARCHAR(255) NOT NULL,
  plugin_version VARCHAR(255) NOT NULL
)";
            cmd.ExecuteNonQuery();
            
            cmd.CommandText = @"
CREATE INDEX author_version_idx ON plugins(plugin_name, plugin_author)";
            cmd.ExecuteNonQuery();
        }

        public DbDataReader Read(string query) {
            var cmd = this.connection.CreateCommand();
            cmd.CommandText = query;
            if (this.transaction != null) {
                cmd.Transaction = this.transaction;
            }
            var reader = cmd.ExecuteReader();
            cmd.Dispose();
            return reader;
        }

        public long Write(DbCommand command) {
            command.Connection = connection;
            if (this.transaction != null) {
                command.Transaction = this.transaction;
            }
            long rows;
            if (command.CommandText.ToLower().StartsWith("insert")) {
                rows = (int)(this.connection as SQLiteConnection).LastInsertRowId;
            }
            else {
                rows = command.ExecuteNonQuery();
            }
            command.Dispose();
            return rows;
        }

        public void BeginTransaction() {
            transaction?.Dispose();
            transaction = connection.BeginTransaction();
        }

        public void Commit() {
            transaction?.Commit();
            transaction?.Dispose();
            transaction = null;
        }

        public void Rollback() {
            transaction?.Rollback();
        }

        public void Dispose() {
            try {
                this.connection.Dispose();
            }
            catch {
                log.Warn("Tried to dispose already disposed database connection.");
            }
            log.Info("Disposing connection manager");
            EventDispatcher.Instance.Unregister<SelectEvent>(OnSelectRequest);
            EventDispatcher.Instance.Unregister<InsertEvent>(OnInsertRequest);
            EventDispatcher.Instance.Unregister<UpdateEvent>(OnUpdateRequest);
            EventDispatcher.Instance.Unregister<CreateTableEvent>(OnCreateTableRequest);
            EventDispatcher.Instance.Unregister<DeleteEvent>(OnDeleteRequest);
        }

        private void OnSelectRequest(SelectEvent ev) {
            var b = new SqliteSelectQueryBuilder(ev);
            ev.ResultSet = this.Read(b.Build());
        }
        
        private void OnInsertRequest(InsertEvent ev) {
            var b = new SqliteInsertQueryBuilder(ev);
            ev.LastInsertId = this.Write(b.Build());
        }
        
        private void OnUpdateRequest(UpdateEvent ev) {
            var b = new SqliteUpdateQueryBuilder(ev);
            ev.AffectedRows = this.Write(b.Build());
        }
        
        private void OnCreateTableRequest(CreateTableEvent ev) {
            log.Info("create table request");
            var b = new SqliteCreateTableQueryBuilder(ev);
            this.Write(b.Build());
        }
        
        private void OnDeleteRequest(DeleteEvent ev) {
            var b = new SqliteDeleteQueryBuilder(ev);
            this.Write(b.Build());
        }
    }

    internal class SqliteSelectQueryBuilder {
        private SelectEvent data;

        public SqliteSelectQueryBuilder(SelectEvent ev) {
            this.data = ev;
        }

        public string Build() {
            var fieldList = string.Join(",", data.FieldList);
            
            var select = $"select {fieldList} from {data.TableList}";
            if (!string.IsNullOrEmpty(data.WhereClause)) {
                select += $" where {data.WhereClause}";
            }
            if (data.OrderByList.Count > 0) {
                select += $" order by {string.Join(",", data.OrderByList)}";
            }
            if (data.Offset > 0) {
                select += $" offset {data.Offset}";
            }
            if (data.Limit > 0) {
                select += $" limit {data.Limit}";
            }
            return select;
        }
    }
    
    internal class SqliteInsertQueryBuilder {
        private InsertEvent data;

        public SqliteInsertQueryBuilder(InsertEvent ev) {
            this.data = ev;
        }

        public DbCommand Build() {
            string fields = string.Join(",", data.DataList.Keys);
            // quote everything so nothing goes derpshit
            var command = new SQLiteCommand();
            // prepare placeholders
            string values = string.Join(",", data.DataList.Keys.Select(c => $"@{c}"));
            command.CommandText = $"insert into {data.TableName} ({fields}) values ({values})";
            // fill placeholders
            foreach (var kvp in data.DataList) {
                command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
            }
            return command;
        }
    }
    
    internal class SqliteUpdateQueryBuilder {
        private UpdateEvent data;

        public SqliteUpdateQueryBuilder(UpdateEvent ev) {
            this.data = ev;
        }

        public DbCommand Build() {
            // quote everything so nothing goes derpshit
            var command = new SQLiteCommand();
            // prepare placeholders
            string values = string.Join(", ", data.DataList.Keys.Select(c => $"c = @{c}"));
            command.CommandText = $"update {data.TableName} set {values} where {data.WhereClause}";
            // fill placeholders
            foreach (var kvp in data.DataList) {
                command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
            }
            return command;
        }
    }
    
    internal class SqliteCreateTableQueryBuilder {
        
        private CreateTableEvent data;

        public SqliteCreateTableQueryBuilder(CreateTableEvent ev) {
            this.data = ev;
        }

        public DbCommand Build() {
            
            string fields = string.Join(",", data.FieldDefinitions);
            // quote everything so nothing goes derpshit
            var command = new SQLiteCommand();
            // prepare placeholders
            command.CommandText = $"create table {data.TableName} ({fields});";
            if (!string.IsNullOrEmpty(data.IndexName) && !string.IsNullOrEmpty(data.IndexFieldList)) {
                command.CommandText += $"create index {data.IndexName} on {data.TableName}({data.IndexFieldList});";
            }
            return command;
        }
    }
    
    internal class SqliteDeleteQueryBuilder {
        private DeleteEvent data;

        public SqliteDeleteQueryBuilder(DeleteEvent ev) {
            this.data = ev;
        }

        public DbCommand Build() {
            // quote everything so nothing goes derpshit
            var command = new SQLiteCommand();
            // prepare placeholders
            command.CommandText = $"delete from {data.TableName} where {data.WhereClause}";
            return command;
        }
    }
}