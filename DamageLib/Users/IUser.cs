using System;
using DamageBot.Commands;

namespace DamageBot.Users {
    public interface IUser : IMessageReceiver {
        long UserId {
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

        /// <summary>
        /// The status of this user during this message cycle.
        /// You CAN set a new one but it will be overridden immediately with the next request.
        /// </summary>
        new ChatStatus Status {
            get;
            set;
        }
    }
}