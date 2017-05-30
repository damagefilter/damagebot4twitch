using System.Data.Common;

namespace DamageBot.Database {
    public interface IConnectionManager {
        /// <summary>
        /// Creates a Read-Query (that's select, mostly) and executes it.
        /// Returns the data reader produced by the implementing database service.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        DbDataReader Read(string query);

        /// <summary>
        /// Does a write-query.
        /// This can be an update, an insert or a schema change.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int Write(string query);
    }
}