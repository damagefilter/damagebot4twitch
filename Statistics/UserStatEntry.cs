using System;
using System.Runtime.InteropServices.ComTypes;
using DamageBot.Events.Database;
using DamageBot.Logging;

namespace Statistics {
    /// <summary>
    /// Represents anm entry in the user statistics table.
    /// </summary>
    public class UserStatEntry {
        
        private int statId;
        
        private int userId;

        private DateTime statDate;

        private TimeSpan startedWatching;
        
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
            this.startedWatching = TimeSpan.FromSeconds(timeWatched);
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
            this.statId = (int)insert.LastInsertId;
        }

        private void Update() {
            Console.WriteLine("Update");
            var update = new UpdateEvent();
            update.TableName = "user_statistics";
            update.DataList.Add("time_watching", startedWatching.Seconds + (DateTime.UtcNow - this.statDate).Seconds);
            update.DataList.Add("messages_sent", this.messagesSent);
            update.WhereClause = $"id = {this.statId}";
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
            select.WhereClause = $"user_id = {userId} and stat_date = date('{DateTime.UtcNow:YYYY-MM-DD}')";
            select.Call();

            if (select.ReadNext()) {
                // int statId, int userid, DateTime statDate, int startedWatching, int messagesSent
                return new UserStatEntry(
                    select.GetInteger("id"),
                    select.GetInteger("user_id"),
                    select.GetDateTime("stat_date"),
                    select.GetInteger("time_watched"),
                    select.GetInteger("messages_sent")
                );
            }
            return new UserStatEntry(userId);
        }
    }
}