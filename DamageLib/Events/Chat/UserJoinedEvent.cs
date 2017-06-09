using DamageBot.EventSystem;
using DamageBot.Users;

namespace DamageBot.Events.Chat {
    public class UserJoinedEvent : Event<UserJoinedEvent> {
        public IUser User {
            get;
        }

        public string Channel {
            get;
        }

        public UserJoinedEvent(IUser user, string joinedChannel) {
            this.User = user;
            this.Channel = joinedChannel;
        }
    }
}