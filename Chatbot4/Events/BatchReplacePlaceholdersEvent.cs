using System.Collections.Generic;
using DamageBot.EventSystem;

namespace Chatbot4.Events {
    public class BatchReplacePlaceholdersEvent : Event<BatchReplacePlaceholdersEvent> {

        public List<string> Texts {
            get;
            set;
        }

        public BatchReplacePlaceholdersEvent(List<string> originalTexts) {
            this.Texts = originalTexts;
        }
        
    }
}