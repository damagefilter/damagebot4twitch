using System;
using TwitchLib.Models.API.v5.Users;

namespace DamageBot.Users {
    /// <summary>
    /// Default chat user.
    /// Use this as reference for your custom behaviour pertaining to usesr
    /// </summary>
    public class ChatUser : IUser {
        private readonly DateTime timeConnected;

        public string DisplayName {
            get;
            set;
        }

        public string Bio {
            get;
            set;
        }

        public TimeSpan OnlineSince => DateTime.Now - timeConnected;

        public string TwitchId {
            get;
            set;
        }
        
        public DateTime DateJoinedFirst {
            get;
            set;
        }

        public DateTime DateLastVisited {
            get;
            set;
        }

        public DateTime DateLastRemoteUpdate {
            get;
            set;
        }

        public string LocalId {
            get;
            set;
        }
        
        public ChatUser() {
            this.timeConnected = DateTime.Now;
        }
    }
}