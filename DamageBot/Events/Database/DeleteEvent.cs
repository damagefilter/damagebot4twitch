using DamageBot.EventSystem;

namespace DamageBot.Events.Database {
    public class DeleteEvent : Event<DeleteEvent> {
        /// <summary>
        /// List of tables to select from as a string.
        /// It may contain join commands.
        /// </summary>
        public string TableName {
            get;
            set;
        }
        
        /// <summary>
        /// A big old where clause.
        /// Be sure this doesn't contain any non-standard sql stuffs.
        /// </summary>
        public string WhereClause {
            get;
            set;
        }
        
        /// <summary>
        /// Affected rows for the specified insert operation
        /// </summary>
        public int AffectedRows {
            get;
            set;
        }
    }
}