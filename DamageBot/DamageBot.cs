using System;
using DamageBot.Commands;
using DamageBot.Database;
using DamageBot.Di;
using DamageBot.Events.Chat;
using DamageBot.EventSystem;
using DamageBot.Plugins;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;

namespace DamageBot {
    public class DamageBot {
        private BotConfig configuration;
        
        private DependencyContainer diContainer;
        private PluginLoader pluginLoader;
        private TwitchClient twitchIrcClient;
        private CommandManager cmds;

        /// <summary>
        /// </summary>
        /// <param name="config">the bot configuration</param>
        public DamageBot(BotConfig config) {
            this.configuration = config;
            this.diContainer = new DependencyContainer();
            this.pluginLoader = new PluginLoader();
            this.diContainer.AddBinding(typeof(CommandManager), typeof(CommandManager), true);
        }

        public void PrepareTwitch() {
            var creds = new ConnectionCredentials(configuration.TwitchUsername, configuration.TwitchUserAuthKey);
            this.twitchIrcClient = new TwitchClient(creds, configuration.Channel);
            
            TwitchAPI.Settings.ClientId = configuration.ApplicationClientId;
            TwitchAPI.Settings.AccessToken = configuration.ApiAuthKey;
            Console.WriteLine("Connecting to IRC");
            this.twitchIrcClient.Connect();
            Console.WriteLine("Connected? " + this.twitchIrcClient.IsConnected);
            
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

        public void InitDiContainer() {
            diContainer.BuildAndCreateResolver();
        }

        public void PrepareDatabase() {
            this.diContainer.Get<IConnectionManager>(); // This creates a new instance and consequently creates all the stuff with it
        }

        public void InitCommands() {
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
            this.twitchIrcClient.SendMessage($"{data.ChatMessage.DisplayName} send a message, yo.");
        }

        private void OnChatCommand(object sender, OnChatCommandReceivedArgs data) {
            this.twitchIrcClient.SendMessage($"{data.Command.Command} chat command received.");
        }
        
        private void OnWhisperCommand(object sender, OnWhisperCommandReceivedArgs data) {
            this.twitchIrcClient.SendMessage($"{data.Command} whisper command received.");
        }

        private void OnChannelMessageRequest(RequestChannelMessageEvent ev) {
            this.twitchIrcClient.SendMessage(ev.Channel, ev.Message);
        }
        
        private void OnWhisperMessageRequest(RequestWhisperMessageEvent ev) {
            this.twitchIrcClient.SendWhisper(ev.User.Username, ev.Message);
        }
    }
}