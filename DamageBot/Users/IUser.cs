using System;

namespace DamageBot.Users {
    public interface IUser {
        /// <summary>
        /// This users name in a string
        /// </summary>
        string DisplayName {
            get;
        }

        /// <summary>
        /// Connected users bio.
        /// </summary>
        string Bio {
            get;
        }
        
        /// <summary>
        /// Is this user a Sub?
        /// </summary>
        bool IsSub {
            get;
        }

        /// <summary>
        /// Time connected.
        /// </summary>
        TimeSpan OnlineSince {
            get;
        }
        
        /// <summary>
        /// The day when this user first joined the chat.
        /// (When the bot was also in the chat)
        /// </summary>
        DateTime DateJoinedFirst {
            get;
        }
        
        /// <summary>
        /// The day this user has last visited the chat.
        /// </summary>
        DateTime DateLastVisited {
            get;
        }
    }
}