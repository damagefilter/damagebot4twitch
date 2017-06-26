using System.IO;
using Newtonsoft.Json;

namespace ScoresCore {
    public class ScoresConfig {
        
        /// <summary>
        /// Name of the score currency used.
        /// </summary>
        public string CurrencyName {
            get;
            set;
        }
        
        /// <summary>
        /// Used in the !points command. This message will be returned
        /// with placeholders filled out by correct values.
        /// Placeholders: {amount}, {currency}, {username}
        /// May vary in the future
        /// </summary>
        public string ScoreInfoMessageTemplate {
            get;
            set;
        }
        
        public string ScoreRewardedTemplate {
            get;
            set;
        }
        
        public string ScoreDeductedTemplate {
            get;
            set;
        }

        /// <summary>
        /// True: Gives score for each message.
        /// </summary>
        public bool UseMessagingScore {
            get;
            set;
        }

        /// <summary>
        /// Amount of score to hand out per message
        /// </summary>
        public int MessagingScoreAmount {
            get;
            set;
        }

        /// <summary>
        /// True: Gives scores to all people in chat for lurking.
        /// </summary>
        public bool UseLurkerScore {
            get;
            set;
        }

        /// <summary>
        /// Amount to give each time lurker score is handed out.
        /// </summary>
        public int LurkerScoreAmount {
            get;
            set;
        }

        /// <summary>
        /// Interval in minutes how often lurker score should be given.
        /// </summary>
        public int LurkerScoreInterval {
            get;
            set;
        }
        
        public void Save() {
            File.WriteAllText("scores_config.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static ScoresConfig LoadConfig() {
            if (File.Exists("scores_config.json")) {
                var mod = JsonConvert.DeserializeObject<ScoresConfig>(File.ReadAllText("scores_config.json"));
                
                return mod;
            }
            var mod2 = new ScoresConfig();
            return mod2;
        }
    }
}