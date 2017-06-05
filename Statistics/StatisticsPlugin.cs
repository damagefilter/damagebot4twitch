using DamageBot.Di;
using DamageBot.Events.Database;
using DamageBot.Plugins;

namespace Statistics {
    public class StatisticsPlugin : Plugin {
        public override void InitResources(DependencyContainer diContainer) {
            throw new System.NotImplementedException();
        }

        public override void Enable(DependencyContainer diContainer) {
            throw new System.NotImplementedException();
        }

        public override void InstallRoutine() {
            var ct = new CreateTableEvent();
            ct.TableName = "user_statistics";
            ct.FieldDefinitions.Add("id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
            // time inside channel.
            ct.FieldDefinitions.Add("time_watching TEXT NOT NULL");
            // messages sent in all time
            ct.FieldDefinitions.Add("messages_overall TEXT NOT NULL");
            
            // messages sent this week
            ct.FieldDefinitions.Add("messages_this_week TEXT NOT NULL");
            
            // messages sent this month
            ct.FieldDefinitions.Add("messages_this_month TEXT NOT NULL");
            
            // date on which the user was last seen.
            // Is compared against to determine messages in this week and month.
            ct.FieldDefinitions.Add("last_seen DATETIME NOT NULL");
            
            ct.FieldDefinitions.Add("user_id INTEGER NOT NULL");
            ct.FieldDefinitions.Add("FOREIGN KEY(user_id) REFERENCES users(user_id)");
            ct.Call();
            
            ct = new CreateTableEvent();
            ct.TableName = "stream_statistics";
            ct.FieldDefinitions.Add("id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
            // When was this session recorded
            ct.FieldDefinitions.Add("time_recorded DATETIME NOT NULL");

            // Max num users that session.
            ct.FieldDefinitions.Add("max_users INTEGER NOT NULL");
            
            // messages sent this session
            ct.FieldDefinitions.Add("messages INTEGER NOT NULL");
            
            // How long did the stream go (time in seconds)
            ct.FieldDefinitions.Add("session_length TEXT NOT NULL");
            
            // messages sent this month
            ct.FieldDefinitions.Add("messages_this_month TEXT NOT NULL");
            
            // date on which the user was last seen.
            // Is compared against to determine messages in this week and month.
            
            
            ct.FieldDefinitions.Add("user_id INTEGER NOT NULL");
            ct.FieldDefinitions.Add("FOREIGN KEY(user_id) REFERENCES users(user_id)");
            ct.Call();
        }

        public override void UpdateRoutine(string installedVersion) {
            throw new System.NotImplementedException();
        }
    }
}