using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Chatbot4 {
    public class ChatbotConfig {
        
        public bool DebugMode {
            get;
            set;
        }
        
        /// <summary>
        /// Names by which the bot can feel spoken too.
        /// If for instance, your bots account name is
        /// "HerbertTheBot" it will automatically feel spoken to at this name.
        /// But you can add here nicks, such as "Herb" or "Bot" to make it all feel more natural. 
        /// </summary>
        public List<string> BotNicks {
            get;
            set;
        }
        
        /// <summary>
        /// Use the ticker message feature?
        /// </summary>
        public bool UseTicker {
            get;
            set;
        }
        
        /// <summary>
        /// Delay between ticker messages
        /// </summary>
        public int TickerDelay {
            get;
            set;
        }
        
        /// <summary>
        /// The default time format to format any timestamps in chat.
        /// </summary>
        public string TimeFormat{
            get;
            set;
        }
        
        /// <summary>
        /// Words affecting the conversation mood in a negative fashion
        /// </summary>
        public List<string> NegativeWords {
            get;
            set;
        }
        
        /// <summary>
        /// Words affecting the conversation mood in a positive fashion
        /// </summary>
        public List<string> PositiveWords {
            get;
            set;
        }
        
        public void Save() {
            File.WriteAllText("chatbot_config.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static ChatbotConfig LoadConfig() {
            if (File.Exists("chatbot_config.json")) {
                Console.WriteLine("Reading charbot config form json");
                var mod = JsonConvert.DeserializeObject<ChatbotConfig>(File.ReadAllText("chatbot_config.json"));
                if (mod.NegativeWords == null) {
                    mod.NegativeWords = new List<string>();
                }

                if (mod.PositiveWords == null) {
                    mod.PositiveWords = new List<string>();
                }

                if (mod.BotNicks == null || mod.BotNicks.Count == 0) {
                    mod.BotNicks = new List<string>();
                }
                return mod;
            }
            else {
                Console.WriteLine("Config json not found. giving new chatbot cfg");
                var mod = new ChatbotConfig();
                mod.NegativeWords = new List<string>();
                mod.PositiveWords = new List<string>();
                mod.BotNicks = new List<string>();
                return mod;
            }
            
        }
    }
}