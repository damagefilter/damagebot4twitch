using System;
using System.IO;
using Newtonsoft.Json;

namespace DamageBot {
    public class BotConfig {
        /// <summary>
        /// This is for the bot itself.
        /// It's provided by the person who registered a build of this bot at twitch,
        /// as new application.
        /// It's used to communicate authorization.
        /// </summary>
        public string ApplicationClientId {
            get;
            set;
        }
        
        /// <summary>
        /// This is provided by the person who registered a build of this bot at twitc.
        /// It's super secret and you gotta set it manually.
        /// It's used to communicate authorization.
        /// </summary>
        public string ApplicationClientSecret {
            get;
            set;
        }
        
        public string ApiAuthKey {
            get;
            set;
        }

        /// <summary>
        /// The account the bot will connect as to the IRC server.
        /// </summary>
        public string TwitchUsername {
            get;
            set;
        }
        
        public string Channel {
            get;
            set;
        }

        /// <summary>
        /// The password / auth token to identify with at the server.
        /// </summary>
        public string TwitchUserAuthKey {
            get;
            set;
        }

        public void Save() {
            File.WriteAllText("botconfig.json", JsonConvert.SerializeObject(this,Formatting.Indented));
            Console.WriteLine("Saved bot config");
        }

        public static BotConfig LoadConfig() {
            if (File.Exists("botconfig.json")) {
                return JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("botconfig.json"));
            }
            return new BotConfig();
        }
    }
}