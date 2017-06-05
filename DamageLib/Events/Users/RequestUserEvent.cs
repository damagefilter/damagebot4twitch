using DamageBot.EventSystem;
using DamageBot.Users;

namespace DamageBot.Events.Users {
    /// <summary>
    /// Request a normal user
    /// </summary>
    public class RequestUserEvent : Event<RequestUserEvent> {
        public string Username {
            get;
        }

        public IUser ResolvedUser {
            get;
            set;
        }

        public RequestUserEvent(string userName) {
            this.Username = userName;
        }
    }
}