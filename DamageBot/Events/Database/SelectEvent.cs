using System;
using System.Collections.Generic;
using System.Data.Common;
using DamageBot.EventSystem;
using Newtonsoft.Json;

namespace DamageBot.Events.Database {
    /// <summary>
    /// Fire this event to make a select query at the active database.
    /// Fill the properties according to your needs.
    /// This is a very very simplified version of some kind of query builder.
    /// But for the required intents and purposes it will do just fine.
    /// As long as you don't go using sql server or oracle or any non-standard sql server thing.
    /// </summary>
    public class SelectEvent : Event<SelectEvent> {
        /// <summary>
        /// List of tables to select from as a string.
        /// It may contain join commands.
        /// Only standard sql joins allowed.
        /// </summary>
        public string TableList {
            get;
            set;
        }

        /// <summary>
        /// The list of fields to select. May contain aliases.
        /// </summary>
        public List<string> FieldList {
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
        /// A list of order-by statements.
        /// Each statement may look like so:
        /// table.myFieldname ASC
        /// or
        /// table.otherfield DESC
        /// </summary>
        public List<string> OrderByList {
            get;
            set;
        }

        public int Limit {
            get;
            set;
        }

        public int Offset {
            get;
            set;
        }

        public DbDataReader ResultSet {
            get;
            set;
        }

        public SelectEvent() {
            this.FieldList = new List<string>();
            this.OrderByList = new List<string>();
        }

        /// <summary>
        /// Advances internal pointer in result set. Reads next row.
        /// Returns true if successful, false if no more rows or error.
        /// </summary>
        /// <returns></returns>
        public bool ReadNext() {
            return this.ResultSet.Read();
        }
        
        /// <summary>
        /// Get a string from the result sets current row
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetString(string fieldName) {
            try {
                return this.ResultSet.GetString(this.ResultSet.GetOrdinal(fieldName));
            }
            catch {
                return null;
            }
        }
        
        public int GetInteger(string fieldName) {
            try {
                return this.ResultSet.GetInt32(this.ResultSet.GetOrdinal(fieldName));
            }
            catch {
                return 0;
            }
        }
        
        public float GetFloat(string fieldName) {
            try {
                return this.ResultSet.GetFloat(this.ResultSet.GetOrdinal(fieldName));
            }
            catch {
                return 0;
            }
        }
        
        public DateTime GetDateTime(string fieldName) {
            try {
                return this.ResultSet.GetDateTime(this.ResultSet.GetOrdinal(fieldName));
            }
            catch {
                return DateTime.MinValue;
            }
        }
    }
} 