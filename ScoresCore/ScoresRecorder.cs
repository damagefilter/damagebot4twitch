using System;
using System.Collections.Concurrent;
using System.Threading;
using DamageBot.Database;
using DamageBot.Events.Chat;
using DamageBot.EventSystem;
using DamageBot.Logging;
using DamageBot.Users;

namespace ScoresCore {
    /// <summary>
    /// This is the big thing that does the scoring.
    /// </summary>
    public class ScoresRecorder {

        private readonly ConcurrentDictionary<string, Score> scoresListCache;
        private readonly IConnectionManager dbCon;

        private bool dumperRunning;

        private readonly Logger log;
        private readonly ScoresConfig cfg;

        public ScoresRecorder(IConnectionManager dbConnection, ScoresConfig cfg) {
            scoresListCache = new ConcurrentDictionary<string, Score>();
            this.dbCon = dbConnection;
            this.log = LogManager.GetLogger(GetType());
            this.cfg = cfg;
        }
        
        public void StartRecording() {
            EventDispatcher.Instance.Register<UserJoinedEvent>(OnUserJoined);
            EventDispatcher.Instance.Register<UserLeftEvent>(OnUserLeft);
            EventDispatcher.Instance.Register<MessageReceivedEvent>(OnUserChat);
            this.dumperRunning = true;
            ThreadPool.QueueUserWorkItem(DumperTask);
            if (cfg.UseLurkerScore) {
                ThreadPool.QueueUserWorkItem((obj) => {
                    while (dumperRunning) {
                        Thread.Sleep(TimeSpan.FromMinutes(cfg.LurkerScoreInterval));
                        foreach (var score in scoresListCache.Values) {
                            score.ScoreValue += cfg.LurkerScoreAmount;
                        }
                    }
                });
            }
        }

        public void StopRecording() {
            EventDispatcher.Instance.Unregister<UserJoinedEvent>(OnUserJoined);
            EventDispatcher.Instance.Unregister<UserLeftEvent>(OnUserLeft);
            EventDispatcher.Instance.Unregister<MessageReceivedEvent>(OnUserChat);
            this.dumperRunning = false;
        }

        public Score GetScoreForUser(IUser user) {
            return this.GetScoreFromCache(user);
        }
        
        private void DumperTask(object state) {
            while (dumperRunning) {
                // sleep a minute, then dump again.
                Thread.Sleep(5000);
                Dump();
            }
        }

        private void Dump() {
//            dbCon.BeginTransaction();
            try {
                //FIXME: If we get a couple thousand records in here that's gonna get ugly
                foreach (var stat in scoresListCache.Values) {
                    stat.Save();
                }
//                dbCon.Commit();
            }
            catch (Exception e){
                log.Error("Failed to save a thing.", e);
//                dbCon.Rollback();
            }
        }
        
        public void OnUserJoined(UserJoinedEvent ev) {
            // ensures user is in cache
            GetScoreFromCache(ev.User);
        }

        public void OnUserChat(MessageReceivedEvent ev) {
            if (!cfg.UseMessagingScore) {
                return;
            }
            var score = GetScoreFromCache(ev.User);
            score.ScoreValue += cfg.MessagingScoreAmount;
        }

        private Score GetScoreFromCache(IUser user) {
            if (scoresListCache.ContainsKey(user.Name)) {
                return scoresListCache[user.Name];
            }
            var stats = Score.GetForUser(user);
            if (!scoresListCache.TryAdd(user.Name, stats)) {
                log.Error($"Failed to insert {user.Name} into watched list.");
            }
            return stats;
        }

        public void OnUserLeft(UserLeftEvent ev) {
            if (scoresListCache.ContainsKey(ev.User.Name)) {
                Score entry;
                if (!scoresListCache.TryRemove(ev.User.Name, out entry)) {
                    log.Error($"Failed to remove {ev.User.Name} from watched list.");
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