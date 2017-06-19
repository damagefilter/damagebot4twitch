using System.Collections.Generic;
using DamageBot.EventSystem;

namespace DamageBot.Events.Database {
    public class InsertEvent : Event<InsertEvent> {
        /// <summary>
        /// List of tables to select from as a string.
        /// It may contain join commands.
        /// </summary>
        public string TableName {
            get;
            set;
        }

        /// <summary>
        /// The list of fields to update and the value to set them to.
        /// Make sure that the provided object has a unique and correct string representation
        /// if it's not a number.
        /// </summary>
        public Dictionary<string, object> DataList {
            get;
            set;
        }

        /// <summary>
        /// Affected rows for the specified insert operation
        /// </summary>
        public long AffectedRows {
            get;
            set;
        }

        /// <summary>
        /// Set after the call.
        /// Returns the LAST insert ID. If you inserted multilple rows in one statement - just so you know
        /// </summary>
        public long LastInsertId {
            get;
            set;
        }

        public InsertEvent() {
            DataList = new Dictionary<string, object>();
        }
    }
}