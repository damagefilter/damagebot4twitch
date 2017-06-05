using System;
using DamageBot.Commands;
using DamageBot.Database;
using DamageBot.Di;
using DamageBot.Events.Chat;
using DamageBot.Events.Users;
using DamageBot.EventSystem;
using DamageBot.Logging;
using DamageBot.Plugins;
using DamageBot.Tasking;
using DamageBot.Users;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;

namespace DamageBot {
    public class DamageBot {
        private BotConfig configuration;
        
        private DependencyContainer diContainer;
        private PluginLoader pluginLoader;
        private TwitchClient twitchIrcClient;
        private TaskQueue tasks;
        
        private CommandManager cmds;

        private readonly Logger log;

        public bool IsRunning => twitchIrcClient.IsConnected;

        /// <summary>
        /// </summary>
        /// <param name="config">the bot configuration</param>
        public DamageBot(BotConfig config) {
            log = LogManager.GetLogger(GetType());
            this.configuration = config;
            this.diContainer = new DependencyContainer();
            this.pluginLoader = new PluginLoader();
            this.tasks = new TaskQueue();
            this.diContainer.AddBinding(typeof(CommandManager), typeof(CommandManager), true);
            this.diContainer.AddBinding(typeof(SqlUserFactory), typeof(SqlUserFactory), true);
            this.diContainer.AddBinding(typeof(TwitchUserApi), typeof(TwitchUserApi), true);
        }

        public void PrepareTwitch() {
            log.Info("Setting up channel user");
            var creds = new ConnectionCredentials(configuration.TwitchUsername, configuration.TwitchUserAuthKey);
            this.twitchIrcClient = new TwitchClient(creds, configuration.Channel);
            
            log.Info("Preparing API Access.");
            TwitchAPI.Settings.ClientId = configuration.ApplicationClientId;
            TwitchAPI.Settings.AccessToken = configuration.ApiAuthKey;
            
        }

        public void Connect() {
            log.Info("Connecting to IRC");
            this.twitchIrcClient.Connect();
            log.Info("Connected? " + this.twitchIrcClient.IsConnected);
            
            this.tasks.StartPolling();
            log.Info("Ready. Lets roll.");
        }

        public void Disconnect() {
            log.Info("Disconnecting bot.");
            this.twitchIrcClient.Disconnect();
        }
        
        public void InitCallbacks() {
            this.twitchIrcClient.OnJoinedChannel += OnBotJoinedChannel;
            
            this.twitchIrcClient.OnUserJoined += OnJoinedChannel;
            this.twitchIrcClient.OnMessageReceived += OnMessageReceived;
            this.twitchIrcClient.OnChatCommandReceived += OnChatCommand;
            this.twitchIrcClient.OnWhisperCommandReceived += OnWhisperCommand;
            this.twitchIrcClient.OnUserLeft += OnUserLeftChannel;

            EventDispatcher.Instance.Register<RequestChannelMessageEvent>(OnChannelMessageRequest);
            EventDispatcher.Instance.Register<RequestWhisperMessageEvent>(OnWhisperMessageRequest);
        }

        public void BuildDiContainerAndResolver() {
            diContainer.BuildAndCreateResolver();
        }

        public void EnsureSubsystems() {
            this.diContainer.Get<IConnectionManager>(); // This creates a new instance and consequently creates all the stuff with it
            this.diContainer.Get<SqlUserFactory>(); // prepares the user factory. We speak to it via events, while the di container keeps a reference
        }

        public void EnsureCommands() {
            this.cmds = this.diContainer.Get<CommandManager>();
        }
        
        /// <summary>
        /// Sets the implementation for the bots database connection.
        /// Without it many features will likely fail a lot.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void BindDatabaseImplementation<T>() where T : IConnectionManager {
            this.diContainer.AddBinding(typeof(IConnectionManager), typeof(T), true);
        }

        public void LoadPlugins() {
            this.pluginLoader.LoadPlugins();
            this.pluginLoader.InitialisePluginResources(diContainer);
        }

        public void EnablePlugin() {
            this.pluginLoader.EnsureInstallations();
            this.pluginLoader.EnablePlugins(diContainer);
        }

        private void OnBotJoinedChannel(object sender, OnJoinedChannelArgs agrs) {
            this.twitchIrcClient.SendMessage("I am here! Therefore I am!");
        }
        private void OnJoinedChannel(object sender, OnUserJoinedArgs data) {
        }
        
        private void OnUserLeftChannel(object sender, OnUserLeftArgs data) {
            
        }
        
        private void OnMessageReceived(object sender, OnMessageReceivedArgs data) {
            
            if (data.ChatMessage.Message.StartsWith("!")) {
                return;
            }
            tasks.Add(() => {
                var user = GetUser(data.ChatMessage);
                if (user == null) {
                    return;
                }
                new MessageReceivedEvent(data.ChatMessage.Channel, data.ChatMessage.Message, user).Call();
            });
            
        }

        private void OnChatCommand(object sender, OnChatCommandReceivedArgs data) {
            tasks.Add(() => {
                var user = GetUser(data.Command.ChatMessage);
                if (user == null) {
                    return;
                }
                try {
                    cmds.ParseCommand(user, data.Command.Command, data.Command.ArgumentsAsList.ToArray());
                }
                catch (Exception e) {
                    user.Message("Command failed: " + e.Message);
                    log.Error("Command failed: " + e.Message, e);
                }
            });
        }
        
        private void OnWhisperCommand(object sender, OnWhisperCommandReceivedArgs data) {
            this.twitchIrcClient.SendMessage($"{data.Command} whisper command received.");
        }


        private void OnChannelMessageRequest(RequestChannelMessageEvent ev) {
            this.twitchIrcClient.SendMessage(ev.Channel ?? twitchIrcClient.JoinedChannels[0].Channel, ev.Message);
        }
        
        private void OnWhisperMessageRequest(RequestWhisperMessageEvent ev) {
            this.twitchIrcClient.SendWhisper(ev.User.Username, ev.Message);
        }

        

        private IUser GetUser(ChatMessage msg) {
            var r = new RequestUserEvent(msg.Username);
            r.Call();
            var user = r.ResolvedUser;
            if (user == null) {
                log.Warn("Unable to resolve user " + msg.Username);
                return null;
            }
            var elevation = Elevation.Viewer;
            if (msg.IsBroadcaster) {
                elevation = Elevation.Broadcaster;
            }
            if (msg.IsModerator) {
                elevation = Elevation.Moderator;
            }
            user.Status = new ChatStatus(elevation, msg.Channel, msg.IsSubscriber);
            return user;
        }
    }
}