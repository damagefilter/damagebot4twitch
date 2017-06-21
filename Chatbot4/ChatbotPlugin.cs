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
            cfg = new ChatbotConfig();
            dpp = new DefaultPlaceholderProvider(diContainer.Get<DamageBot.DamageBot>(), cfg);
            convos = new ConversationDispatcher(cfg);
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