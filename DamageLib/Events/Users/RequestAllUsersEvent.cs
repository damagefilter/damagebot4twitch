using System.Collections.Generic;
using DamageBot.EventSystem;
using DamageBot.Users;

namespace DamageBot.Events.Users {
    /// <summary>
    /// Request a normal user
    /// </summary>
    public class RequestAllUsersEvent : Event<RequestAllUsersEvent> {
        

        public List<IUser> ResolvedUsers {
            get;
            set;
        }
    }
}