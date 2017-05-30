using System;
using System.Data.Common;
using System.Linq;
using DamageBot.Events.Database;
using DamageBot.EventSystem;
using DamageBot.Logging;

namespace DamageBot.Database {
    public class SqliteConnectionManager : IConnectionManager, IDisposable {
        private readonly DbConnection connection;
        private readonly Logger log;
        public SqliteConnectionManager(DbConnection connection) {
            this.log = LogManager.GetLogger(GetType());
            this.connection = connection;
            EventDispatcher.Instance.Register<SelectEvent>(OnSelectRequest);
            EventDispatcher.Instance.Register<InsertEvent>(OnInsertRequest);
            EventDispatcher.Instance.Register<UpdateEvent>(OnUpdateRequest);
        }
        
        public DbDataReader Read(string query) {
            var cmd = this.connection.CreateCommand();
            cmd.CommandText = query;
            return cmd.ExecuteReader();
        }

        public int Write(string query) {
            var cmd = this.connection.CreateCommand();
            cmd.CommandText = query;
            return cmd.ExecuteNonQuery();
        }

        public void Dispose() {
            try {
                this.connection.Dispose();
            }
            catch {
                log.Warn("Tried to dispose already disposed database connection.");
            }
            EventDispatcher.Instance.Unregister<SelectEvent>(OnSelectRequest);
            EventDispatcher.Instance.Unregister<InsertEvent>(OnInsertRequest);
            EventDispatcher.Instance.Unregister<UpdateEvent>(OnUpdateRequest);
        }

        private void OnSelectRequest(SelectEvent ev) {
            var b = new SqliteSelectQueryBuilder(ev);
            ev.ResultSet = this.Read(b.Build());
        }
        
        private void OnInsertRequest(InsertEvent ev) {
            var b = new SqliteInsertQueryBuilder(ev);
            ev.AffectedRows = this.Write(b.Build());
        }
        
        private void OnUpdateRequest(UpdateEvent ev) {
            var b = new SqliteUpdateQueryBuilder(ev);
            ev.AffectedRows = this.Write(b.Build());
        }
    }

    internal class SqliteSelectQueryBuilder {
        private SelectEvent data;

        public SqliteSelectQueryBuilder(SelectEvent ev) {
            this.data = ev;
        }

        public string Build() {
            var fieldList = string.Join(",", data.FieldList);
            
            var select = $"select {fieldList} from {data.TableList} where {data.WhereClause}";
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

        public string Build() {
            string fields = string.Join(",", data.DataList.Keys);
            // quote everything so nothing goes derpshit
            string values = string.Join(",", data.DataList.Values.Select(c => $"'{c}'"));
            
            return $"insert into {data.TableName} ({fields}) values ({values})";
        }
    }
    
    internal class SqliteUpdateQueryBuilder {
        private UpdateEvent data;

        public SqliteUpdateQueryBuilder(UpdateEvent ev) {
            this.data = ev;
        }

        public string Build() {
            string fields = string.Join(",", data.DataList.Keys);
            // quote everything so nothing goes derpshit
            string values = string.Join(",", data.DataList.Values.Select(c => $"'{c}'"));
            
            return $"insert into {data.TableName} ({fields}) values ({values}) where {data.WhereClause}";
        }
    }
}