using System.Collections.Generic;
using DamageBot.EventSystem;

namespace DamageBot.Events.Database {
    public class CreateTableEvent  : Event<CreateTableEvent> {
        public string TableName {
            get;
            set;
        }

        public List<string> FieldDefinitions {
            get;
            set;
        }

        
        public string IndexName {
            get;
            set;
        }
        
        public string IndexFieldList {
            get;
            set;
        }

        public CreateTableEvent() {
            FieldDefinitions = new List<string>();
        }
    }
}