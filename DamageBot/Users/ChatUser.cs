using System;
using TwitchLib.Models.API.v5.Users;

namespace DamageBot.Users {
    /// <summary>
    /// Default chat user.
    /// Use this as reference for your custom behaviour pertaining to usesr
    /// </summary>
    public class ChatUser : IUser {
        private readonly DateTime timeConnected;
        private readonly User internalUser;
        
        public string DisplayName => internalUser.DisplayName;

        public string Bio => internalUser.Bio;

        public TimeSpan OnlineSince => DateTime.Now - timeConnected;

        #region set externally by factory
        public bool IsSub {
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
        #endregion
        
        public ChatUser(User twitchUser) {
            this.timeConnected = DateTime.Now;
            this.internalUser = twitchUser;
        }
    }
}