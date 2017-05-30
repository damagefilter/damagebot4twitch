using System;
using DamageBot.EventSystem;
using TwitchLib.Models.API.v5.Users;

namespace DamageBot.Events.Users {
    public class RequestTwitchUserEvent : Event<RequestTwitchUserEvent> {
        
        public string UserName {
            get;
        }

        public Action<User> Setter {
            get;
        }

        public RequestTwitchUserEvent(string name, Action<User> setterCallback) {
            this.UserName = name;
            this.Setter = setterCallback;
        }
    }
}