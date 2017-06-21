using System;
using DamageBot.Events.Database;

namespace ScoresCore {
    public class Score {
        public int Id {
            get;
            set;
        }

        public int UserId {
            get;
            set;
        }

        public int ScoreValue {
            get;
            set;
        }
        
        public void Save() {
            if (this.Id > 0) {
                Update();
            }
            else {
                Insert();
            }
        }

        private void Insert() {
            var insert = new InsertEvent();
            insert.TableName = "user_statistics";
            insert.DataList.Add("user_id", this.userId);
            insert.DataList.Add("stat_date", this.statDate);
            insert.DataList.Add("time_watching", (this.statDate - DateTime.UtcNow).Seconds);
            insert.DataList.Add("messages_sent", this.messagesSent);
            insert.Call();
        }

        private void Update() {
            var update = new UpdateEvent();
            update.TableName = "user_statistics";
            update.DataList.Add("time_watching", timeWatched.Seconds + (this.statDate - DateTime.UtcNow).Seconds);
            update.DataList.Add("messages_sent", this.messagesSent);
            update.Call();
        }

    }
}