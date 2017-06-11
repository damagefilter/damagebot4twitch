using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VanillaModeration {
    public class ModerationConfig {
        
        /// <summary>
        /// If true the bot will "clearchat" a users message history,
        /// including the posted link. 
        /// </summary>
        public bool DisallowLinks {
            get;
            set;
        }
        
        /// <summary>
        /// List of bad words that shouldn't be said in chat.
        /// Will also clearchat for the offending user.
        /// </summary>
        public List<string> BadWords {
            get;
            set;
        }
        
        /// <summary>
        /// How many offenses (bad words / links) is the user allowed to do
        /// before a ban is issued?
        /// </summary>
        public int OffenseThreshold {
            get;
            set;
        }

        /// <summary>
        /// Should the bot auto-ban or timeout when rule breaking is detected?
        /// </summary>
        public bool BanOnOffense {
            get;
            set;
        }

        /// <summary>
        /// The time of bans issued.
        /// If less or equal to zero perma-bans are issued.
        /// </summary>
        public int DefaultBanTime {
            get;
            set;
        }
        
        public void Save() {
            File.WriteAllText("moderation_config.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static ModerationConfig LoadConfig() {
            if (File.Exists("moderation_config.json")) {
                var mod = JsonConvert.DeserializeObject<ModerationConfig>(File.ReadAllText("moderation_config.json"));
                if (mod.BadWords == null) {
                    mod.BadWords = new List<string>();
                }
                return mod;
            }
            var mod2 = new ModerationConfig();
            mod2.BadWords = new List<string>();
            return mod2;
        }
    }
}