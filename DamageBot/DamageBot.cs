using DamageBot.Database;
using DamageBot.Di;
using DamageBot.Plugins;
using TwitchLib;
using TwitchLib.Events.Client;

namespace DamageBot {
    public class DamageBot {
        private DependencyContainer diContainer;
        private PluginLoader pluginLoader;
        private TwitchClient twitchIrcClient; 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="di">an empty dependncy injection container</param>
        /// <param name="loader">a plugin loader instance</param>
        /// <param name="twitchIrcClient">A primed and connected twitch client object</param>
        public DamageBot(DependencyContainer di, PluginLoader loader, TwitchClient twitchIrcClient) {
            this.diContainer = di;
            this.pluginLoader = loader;
            this.twitchIrcClient = twitchIrcClient;
        }

        /// <summary>
        /// Sets the implementation for the bots database connection.
        /// Without it many features will likely fail a lot.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void BindDatabaseImplementation<T>() where T : IConnectionManager {
            this.diContainer.AddBinding(typeof(IConnectionManager), typeof(T));
        }

        /// <summary>
        /// Initialise the twitch API which is required for more detailed stuff
        /// like getting proper user objects from names and such.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="authToken"></param>
        public void InitTwitchApi(string clientId, string authToken) {
            TwitchAPI.Settings.ClientId = clientId;
            TwitchAPI.Settings.AccessToken = authToken;
        }

        public void InitIrcCallbacks() {
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

        private void OnJoinedChannel(object sender, OnUserJoinedArgs data) {
        }
        
        private void OnUserLeftChannel(object sender, OnUserLeftArgs data) {
            
        }
        
        private void OnMessageReceived(object sender, OnMessageReceivedArgs data) {
            
        }
    }
}