using System;
using System.Threading;
using DamageBot.Database;
using DamageBot.Logging;

namespace DamageBot {

    
    internal class Program {
        public static void Main(string[] args) {
            LogManager.ConfigureLogger();
            BotConfig cfg = BotConfig.LoadConfig();
            if (string.IsNullOrEmpty(cfg.ApiAuthKey)) {
                SetupProcess(cfg);
            }
            var bot = new DamageBot(cfg);
            bot.PrepareTwitch();
            bot.BindDatabaseImplementation<SqliteConnectionManager>();
            bot.InitCallbacks();
            bot.InitDiContainer();
            bot.PrepareDatabase();
            bot.InitCommands();
            bot.LoadPlugins();

            while (true) {
                Thread.Sleep(3000);
                Console.Write("... ");
            }
        }

        private static void SetupProcess(BotConfig cfg) {
            AuthenticationHandler authHandler = new AuthenticationHandler(cfg);
            Console.WriteLine("Firstly, lets get connected to twitch.");
            while (string.IsNullOrEmpty(cfg.ApplicationClientId)) {
                Console.WriteLine("Give me the client ID. (You get from registering a new app at twitch)");
                string input = Console.ReadLine();
                input = input?.Trim();
                if (string.IsNullOrEmpty(input)) {
                    Console.WriteLine("Try again.");
                    continue;
                }
                cfg.ApplicationClientId = input;
            }
            
            while (string.IsNullOrEmpty(cfg.ApplicationClientSecret)) {
                Console.WriteLine("Give me the application secret. (You get from registering a new app at twitch)");
                string input = Console.ReadLine();
                input = input?.Trim();
                if (string.IsNullOrEmpty(input)) {
                    Console.WriteLine("Try again.");
                    continue;
                }
                cfg.ApplicationClientSecret = input;
            }
            
            authHandler.BeginAuthProcess();
            Console.WriteLine("You will now be taken to twitch to authorize me acting on your behalf.");
            Console.WriteLine("Once you landed on the page that says \"success\" you can come back to this console window.");
            Console.WriteLine("Hit enter to go to twitch now.");
            Console.Read();
            System.Diagnostics.Process.Start("https://api.twitch.tv/kraken/oauth2/authorize" + 
                                             $"?client_id={cfg.ApplicationClientId}" + 
                                             "&redirect_uri=http://localhost:8080/handle" + 
                                             "&response_type=code" + 
                                             "&scope=chat_login channel_editor channel_feed_edit channel_feed_read channel_subscriptions");
            while (authHandler.WaitingForAuthToken) {
                // waiting ....
                Thread.Sleep(3000);
                Console.Write("... ");
            }
            Console.WriteLine("Okay I got the thing. Thanks for authorizing me.");
            authHandler.StopProcess();
            
            Console.WriteLine("Now that we have this out of the way, lets see which account to use for the chat bot functionality.");
            // Firstly, prepare the configuration
            while (string.IsNullOrEmpty(cfg.TwitchUsername)) {
                Console.WriteLine("Give me the account name.");
                string input = Console.ReadLine();
                input = input?.Trim();
                if (string.IsNullOrEmpty(input)) {
                    Console.WriteLine("Try again.");
                    continue;
                }
                cfg.TwitchUsername = input;
            }
            
            while (string.IsNullOrEmpty(cfg.TwitchUserAuthKey)) {
                Console.WriteLine("Give me the accounts auth token (you get frm OAuth app via Twitch).");
                string input = Console.ReadLine();
                input = input?.Trim();
                if (string.IsNullOrEmpty(input)) {
                    Console.WriteLine("Try again.");
                    continue;
                }
                cfg.TwitchUserAuthKey = input;
            }
            
            while (string.IsNullOrEmpty(cfg.Channel)) {
                Console.WriteLine("Give me the channel I should join.");
                string input = Console.ReadLine();
                input = input?.Trim();
                if (string.IsNullOrEmpty(input)) {
                    Console.WriteLine("Try again.");
                    continue;
                }
                cfg.Channel = input;
            }
            
            Console.WriteLine("All done. Gratz.");
            cfg.Save();
        }
    }
}