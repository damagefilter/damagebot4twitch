using System;
using DamageBot.EventSystem;
using TwitchLib.Models.API.v5.Users;

namespace DamageBot.Events.Users {
    public class RequestTwitchUserEvent : Event<RequestTwitchUserEvent> {
        
        public string UserName {
            get;
        }

        public User ResolvedUser {
            get;
            set;
        }
        public RequestTwitchUserEvent(string name) {
            this.UserName = name;
        }
    }
}