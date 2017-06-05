using DamageBot.EventSystem;

namespace DamageBot.Events.Chat {
    
    /// <summary>
    /// Request sending a message to a channel as the bot.
    /// </summary>
    public class RequestChannelMessageEvent : Event<RequestChannelMessageEvent> {
        public string Channel {
            get;
        }

        public string Message {
            get;
        }

        public RequestChannelMessageEvent(string channel, string message) {
            this.Channel = channel;
            this.Message = message;
        }
    }
}