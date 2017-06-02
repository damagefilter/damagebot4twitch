using System;

namespace DamageBot.Users {
    public interface IUser {
        int UserId {
            get;
        }
        
        string TwitchId {
            get;
        }
        
        string Username {
            get;
        }
        
        DateTime FirstJoined {
            get;
            set;
        }
        
        DateTime LastJoined {
            get;
            set;
        }
    }
}