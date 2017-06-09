using System;
using System.Runtime.InteropServices.ComTypes;
using DamageBot.Events.Database;

namespace Statistics {
    /// <summary>
    /// Represents anm entry in the user statistics table.
    /// </summary>
    public class UserStatEntry {
        
        private int statId;
        
        private int userId;

        private DateTime statDate;

        private TimeSpan timeWatched;
        
        private int messagesSent;

        /// <summary>
        /// Ctor for existing things in database
        /// </summary>
        /// <param name="statId"></param>
        /// <param name="userid"></param>
        /// <param name="statDate"></param>
        /// <param name="timeWatched"></param>
        /// <param name="messagesSent"></param>
        public UserStatEntry(int statId, int userid, DateTime statDate, int timeWatched, int messagesSent) {
            this.statId = statId;
            this.userId = userid;
            this.statDate = statDate;
            this.timeWatched = TimeSpan.FromSeconds(timeWatched);
            this.messagesSent = messagesSent;
        }

        public UserStatEntry(int userId) {
            this.userId = userId;
            this.statDate = DateTime.UtcNow;
        }

        public void AddMessageSent() {
            this.messagesSent++;
        }

        /// <summary>
        /// Saves this record. Updates if it already exists otherwise creates a new record.
        /// </summary>
        public void Save() {
            if (this.statId > 0) {
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

        /// <summary>
        /// Get a UserStatEntry for the given user for today.
        /// If there is no record yet for today then a new entry will be returned.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static UserStatEntry GetByUserForToday(int userId) {
            var select = new SelectEvent();
            select.TableList = "user_statistics";
            select.FieldList.Add("*");
            select.WhereClause = $"user_id = {userId} and stat_date = date('{DateTime.UtcNow.ToString("YYYY-MM-DD")}')";
            select.Call();

            if (select.ReadNext()) {
                return new UserStatEntry(
                    select.GetInteger("id"),
                    select.GetInteger("user_id"),
                    select.GetInteger("time_watching"),
                    select.GetInteger("messages_sent")
                );
            }
            else {
                return new UserStatEntry(userId);
            }
        }
    }
}