using DamageBot;
using DamageBot.Di;
using DamageBot.Plugins;

namespace Chatbot4 {
    public class ChatbotPlugin : Plugin {

        private DefaultPlaceholderProvider dpp;
        private ConversationDispatcher convos;
        private ChatbotConfig cfg;
        public override void InitResources(DependencyContainer diContainer) {
            
        }

        public override void Enable(DependencyContainer diContainer) {
            cfg = ChatbotConfig.LoadConfig();
            var bot = diContainer.Get<DamageBot.DamageBot>();
            dpp = new DefaultPlaceholderProvider(bot, cfg);
            convos = new ConversationDispatcher(cfg, bot.Configuration);
        }

        public override void InstallRoutine() {
            
        }

        public override void UpdateRoutine(string installedVersion) {
            
        }

        protected override PluginDescriptor InternalPreparePluginDescriptor(PluginDescriptor descriptor) {
            descriptor.Author = "damagefilter";
            descriptor.Name = "Chatbot";
            descriptor.Version = "4.0";
            return descriptor;
        }
    }
}