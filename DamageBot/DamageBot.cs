using System;
using DamageBot.Database;
using DamageBot.Di;
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

        /// <summary>
        /// </summary>
        /// <param name="config">the bot configuration</param>
        public DamageBot(BotConfig config) {
            this.configuration = config;
            this.diContainer = new DependencyContainer();
            this.pluginLoader = new PluginLoader();
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
        /// <summary>
        /// Sets the implementation for the bots database connection.
        /// Without it many features will likely fail a lot.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void BindDatabaseImplementation<T>() where T : IConnectionManager {
            this.diContainer.AddBinding(typeof(IConnectionManager), typeof(T));
        }

        public void InitIrcCallbacks() {
            this.twitchIrcClient.OnJoinedChannel += OnBotJoinedChannel;
            
            this.twitchIrcClient.OnUserJoined += OnJoinedChannel;
            this.twitchIrcClient.OnMessageReceived += OnMessageReceived;
            this.twitchIrcClient.OnUserLeft += OnUserLeftChannel;
        }

        public void LoadPlugins() {
            this.pluginLoader.LoadPlugins();
            this.pluginLoader.InitialisePluginResources(diContainer);
            diContainer.BuildAndCreateResolver();
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
            
        }
    }
}