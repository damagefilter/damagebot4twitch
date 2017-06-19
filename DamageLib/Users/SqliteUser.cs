using System;
using DamageBot.Events.Chat;

namespace DamageBot.Users {
    
    public class SqliteUser : IUser {
        public long UserId {
            get;
            set;
        }

        public string TwitchId {
            get;
            set;
        }

        public string Username {
            get;
            set;
        }

        public DateTime FirstJoined {
            get;
            set;
        }

        public DateTime LastJoined {
            get;
            set;
        }

        public ChatStatus Status {
            get;
            set;
        }

        public SqliteUser(string userName) {
            this.Username = userName;
        }

        public void Message(string message) {
            new RequestWhisperMessageEvent(message, this).Call();
        }

        public bool HasPermission(Elevation elevationLevel) {
            return this.Status.UserElevation >= elevationLevel;
        }
    }
}