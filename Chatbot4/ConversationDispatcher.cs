using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chatbot4.Ai;
using DamageBot;
using DamageBot.Events.Chat;
using DamageBot.Events.Users;
using DamageBot.EventSystem;
using DamageBot.Logging;
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
        private BotConfig mainCfg;
        private Conversation tickerConversation;
        private bool tickerIsRunning;

        private Logger log;

        public ConversationDispatcher(ChatbotConfig cfg, BotConfig mainCfg) {
            log = LogManager.GetLogger(GetType());
            this.conversations = new Dictionary<string, Conversation>();
            this.cfg = cfg;
            this.mainCfg = mainCfg;
            this.pool = new ResponsePool(cfg);
            EventDispatcher.Instance.Register<MessageReceivedEvent>(OnChatMessage);
            EventDispatcher.Instance.Register<UserJoinedEvent>(OnUserJoined);
            if (cfg.UseTicker) {
                var user = new RequestUserEvent(mainCfg.TwitchUsername, null);
                user.Call();
                if (user.ResolvedUser == null) {
                    log.Error("Cannot find default ticker message conversartion partner user thing.");
                }
                this.tickerConversation = new Conversation(pool, cfg, user.ResolvedUser);
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
            if (!conversations.ContainsKey(user.Name)) {
                conversations.Add(user.Name, new Conversation(this.pool, this.cfg, user));
            }
            conversations[user.Name].HandleMessage(message, context);
        }

        public void StartTicker() {
            if (!tickerIsRunning) {
                tickerIsRunning = true;
                ThreadPool.QueueUserWorkItem((obj) => {
                    Console.WriteLine("Starting message ticker.");
                    while (tickerIsRunning) {
                        Thread.Sleep(TimeSpan.FromMinutes(cfg.TickerDelay));
                        Console.WriteLine("Sending ticker message");
                        tickerConversation.SendResponse(pool.GetTickerNode());
                    }
                });
                
            }
        }
        public void StopTicker() {
            this.tickerIsRunning = false;
        }

    }
}