using System;

namespace DamageBot.Users {
    
    public class SqliteUser : IUser {
        public int UserId {
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

        public SqliteUser(string userName) {
            this.Username = userName;
        }
    }
}