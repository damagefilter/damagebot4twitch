using DamageBot.EventSystem;
using DamageBot.Users;

namespace DamageBot.Events.Chat {
    /// <summary>
    /// Re-usable message event.
    /// Can be re-used for the same user in the same channel.
    /// </summary>
    public class MessageReceivedEvent : Event<MessageReceivedEvent> {

        public string Channel {
            get;
        }

        public string Message {
            get;
            set;
        }

        public IUser User {
            get;
        }

        public MessageReceivedEvent(string channel, string message, IUser user) {
            this.Channel = channel;
            this.Message = message;
            this.User = user;
        }
    }
}