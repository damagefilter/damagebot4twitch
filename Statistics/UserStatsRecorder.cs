using System.Collections.Concurrent;
using System.Threading;
using DamageBot.Database;
using DamageBot.Events.Chat;
using DamageBot.EventSystem;
using DamageBot.Logging;

namespace Statistics {
    /// <summary>
    /// Does the recording of user stats.
    /// Dumps them frequently into database.
    /// </summary>
    public class UserStatsRecorder {

        private readonly IConnectionManager dbCon;
        private readonly Logger log;
        private bool dumperRunning;

        private readonly ConcurrentDictionary<string, UserStatEntry> watchedData;

        public UserStatsRecorder(IConnectionManager con) {
            this.dbCon = con;
            this.log = LogManager.GetLogger(GetType());
            this.watchedData = new ConcurrentDictionary<string, UserStatEntry>();
        }

        public void StartRecording() {
            EventDispatcher.Instance.Register<UserJoinedEvent>(OnUserJoined);
            EventDispatcher.Instance.Register<UserLeftEvent>(OnUserLeft);
            EventDispatcher.Instance.Register<MessageReceivedEvent>(OnUserChat);
            this.dumperRunning = true;
            ThreadPool.QueueUserWorkItem(DumperTask);
        }

        public void StopRecording() {
            EventDispatcher.Instance.Unregister<UserJoinedEvent>(OnUserJoined);
            EventDispatcher.Instance.Unregister<UserLeftEvent>(OnUserLeft);
            EventDispatcher.Instance.Unregister<MessageReceivedEvent>(OnUserChat);
            this.dumperRunning = false;
        }

        private void DumperTask(object state) {
            while (dumperRunning) {
                // sleep a minute, then dump again.
                Thread.Sleep(60000);
                Dump();
            }
        }

        private void Dump() {
            dbCon.BeginTransaction();
            try {
                foreach (var stat in watchedData.Values) {
                    stat.Save();
                }
                dbCon.Commit();
            }
            catch {
                dbCon.Rollback();
            }
        }

        public void OnUserJoined(UserJoinedEvent ev) {
            if (!watchedData.ContainsKey(ev.User.Username)) {
                var stats = UserStatEntry.GetByUserForToday(ev.User.UserId);
                if (!watchedData.TryAdd(ev.User.Username, stats)) {
                    log.Error($"Failed to insert {ev.User.Username} into watched list. Trying again on next chat message.");
                }
            }
        }

        public void OnUserChat(MessageReceivedEvent ev) {
            if (watchedData.ContainsKey(ev.User.Username)) {
                watchedData[ev.User.Username].AddMessageSent();
            }
            else {
                var stats = UserStatEntry.GetByUserForToday(ev.User.UserId);
                stats.AddMessageSent();
                if (!watchedData.TryAdd(ev.User.Username, stats)) {
                    log.Error($"Failed to insert {ev.User.Username} into watched list. Trying again on next chat message.");
                }
            }
        }

        public void OnUserLeft(UserLeftEvent ev) {
            if (watchedData.ContainsKey(ev.User.Username)) {
                UserStatEntry entry;
                if (!watchedData.TryRemove(ev.User.Username, out entry)) {
                    log.Error($"Failed to remove {ev.User.Username} from watched list.");
                }
                else {
                    entry.Save();
                }
            }
            if (ev.User.Status.IsBroadcaster) {
                // broadcaster left. Stop the recording of things!
                this.StopRecording();
            }
        }

    }
}