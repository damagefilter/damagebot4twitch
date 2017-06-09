using DamageBot.EventSystem;
using DamageBot.Users;

namespace DamageBot.Events.Chat {
    public class UserLeftEvent : Event<UserLeftEvent> {
        public IUser User {
            get;
        }

        public string Channel {
            get;
        }

        public UserLeftEvent(IUser user, string joinedChannel) {
            this.User = user;
            this.Channel = joinedChannel;
        }
    }
}