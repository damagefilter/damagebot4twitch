using System.Collections.Generic;

namespace Chatbot4 {
    public class ChatbotConfig {
        
        public bool DebugMode {
            get;
            private set;
        }
        
        /// <summary>
        /// Names by which the bot can feel spoken too.
        /// If for instance, your bots account name is
        /// "HerbertTheBot" it will automatically feel spoken to at this name.
        /// But you can add here nicks, such as "Herb" or "Bot" to make it all feel more natural. 
        /// </summary>
        public List<string> BotNicks {
            get;
            private set;
        }
        
        /// <summary>
        /// Use the ticker message feature?
        /// </summary>
        public bool UseTicker {
            get;
            private set;
        }
        
        /// <summary>
        /// Delay between ticker messages
        /// </summary>
        public int TickerDelay {
            get;
            private set;
        }
        
        /// <summary>
        /// The default time format to format any timestamps in chat.
        /// </summary>
        public string TimeFormat{
            get;
            private set;
        }
        
        /// <summary>
        /// Words affecting the conversation mood in a negative fashion
        /// </summary>
        public List<string> NegativeWords {
            get;
            private set;
        }
        
        /// <summary>
        /// Words affecting the conversation mood in a positive fashion
        /// </summary>
        public List<string> PositiveWords {
            get;
            private set;
        }
    }
}