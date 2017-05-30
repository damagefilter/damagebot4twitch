using System.Collections.Generic;
using DamageBot.EventSystem;

namespace DamageBot.Events.Database {
    public class UpdateEvent : Event<UpdateEvent> {
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