using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chatbot4.Ai;
using DamageBot.Events.Chat;
using DamageBot.EventSystem;
using DamageBot.Users;

namespace Chatbot4 {
    /// <summary>
    /// This here thing listens for messages and dispatches
    /// the response if there should be one.
    /// This also has a task continually running to dispatch ticker messages if applicable.
    /// </summary>
    public class ConversationDispatcher {
        private Dictionary<string, Conversation> conversations;
        private ResponsePool pool;
        private ChatbotConfig cfg;
        private Conversation tickerConversation;
        private bool tickerIsRunning;

        public ConversationDispatcher(ChatbotConfig cfg) {
            this.conversations = new Dictionary<string, Conversation>();
            this.cfg = cfg;
            this.pool = new ResponsePool(cfg);
            this.tickerConversation = new Conversation(pool, cfg, null);
            EventDispatcher.Instance.Register<MessageReceivedEvent>(OnChatMessage);
            EventDispatcher.Instance.Register<UserJoinedEvent>(OnUserJoined);
            if (cfg.UseTicker) {
                StartTicker();
            }
        }

        private void OnChatMessage(MessageReceivedEvent ev) {
            this.HandleMessageForContext(ResponseContext.Chat, ev.User, ev.Message);
        }

        private void OnUserJoined(UserJoinedEvent ev) {
            this.HandleMessageForContext(ResponseContext.Join, ev.User, null);
        }

        private void HandleMessageForContext(ResponseContext context, IUser user, string message) {
            if (!conversations.ContainsKey(user.Username)) {
                conversations.Add(user.Username, new Conversation(this.pool, this.cfg, user));
            }
            conversations[user.Username].HandleMessage(message, context);
        }

        public void StartTicker() {
            if (!tickerIsRunning) {
                Task.Run(() => {
                    DoTicker();
                });
                tickerIsRunning = true;
            }
        }

        public void StopTicker() {
            this.tickerIsRunning = false;
        }

        private async void DoTicker() {
            while (tickerIsRunning) {
                await Task.Delay(TimeSpan.FromSeconds(cfg.TickerDelay));
                tickerConversation.SendResponse(pool.GetTickerNode());
            }
        }
    }
}