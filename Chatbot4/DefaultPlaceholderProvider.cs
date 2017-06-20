using Chatbot4.Events;
using DamageBot.EventSystem;

namespace Chatbot4 {
    public class DefaultPlaceholderProvider {
        private DamageBot.DamageBot bot;
        private ChatbotConfig config;
        public DefaultPlaceholderProvider(DamageBot.DamageBot bot, ChatbotConfig cfg) {
            EventDispatcher.Instance.Register<BatchReplacePlaceholdersEvent>(OnBatchRequest);
            EventDispatcher.Instance.Register<ReplacePlaceholdersEvent>(OnSingleRequest);
            this.bot = bot;
            this.config = cfg;
        }

        private void OnBatchRequest(BatchReplacePlaceholdersEvent ev) {
            
        }

        private void OnSingleRequest(ReplacePlaceholdersEvent ev) {
            
        }
    }
}