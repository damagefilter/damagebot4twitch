using DamageBot.EventSystem;
using DamageBot.Users;

namespace DamageBot.Events.Chat {
    
    /// <summary>
    /// Request sending a message to a channel as the bot.
    /// </summary>
    public class RequestWhisperMessageEvent : Event<RequestWhisperMessageEvent> {
        public string Message {
            get;
        }
        
        public IUser User {
            get;
        }

        public RequestWhisperMessageEvent(string message, IUser user) {
            this.Message = message;
            this.User = user;
        }
    }
}