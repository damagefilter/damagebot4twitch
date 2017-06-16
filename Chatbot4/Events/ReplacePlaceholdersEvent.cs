using DamageBot.EventSystem;

namespace Chatbot4.Events {
    public class ReplacePlaceholdersEvent : Event<ReplacePlaceholdersEvent> {

        public string Text {
            get;
            set;
        }

        public ReplacePlaceholdersEvent(string originalText) {
            this.Text = originalText;
        }
        
    }
}