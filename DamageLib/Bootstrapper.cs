using DamageBot.Commands;
using DamageBot.Database;
using DamageBot.Di;
using DamageBot.Plugins;
using DamageBot.Users;

namespace DamageBot {
    /// <summary>
    /// Its sole purpose is to bootstrap the bot correctly.
    /// You can call the Bootstrap() method or device your own bootstrap process if you need to.
    /// You probably don't though.
    /// </summary>
    public class Bootstrapper {
        private readonly DependencyContainer diContainer;
        private readonly BotConfig cfg;
        private readonly PluginLoader loader;

        public Bootstrapper(BotConfig cfg) {
            diContainer = new DependencyContainer();
            this.cfg = cfg;
            this.loader = new PluginLoader();
        }

        public Bootstrapper PrepareDi() {
            this.diContainer.AddBinding(typeof(CommandManager), true);
            this.diContainer.AddBinding(typeof(SqlUserFactory), true);
            this.diContainer.AddBinding(typeof(TwitchUserApi), true);
            this.diContainer.AddBinding(typeof(DamageBot), true);
            return this;
        }
        
        public Bootstrapper BindDatabaseImplementation<T>() where T : IConnectionManager {
            this.diContainer.AddBinding(typeof(IConnectionManager), typeof(T), true);
            return this;
        }

        public Bootstrapper LoadPlugins() {
            loader.LoadPlugins();
            loader.InitialisePluginResources(diContainer);
            return this;
        }
        
        public Bootstrapper FinaliseDi() { // call this before plugins get enabled.
            diContainer.BuildAndCreateResolver();
            return this;
        }

        /// <summary>
        /// Prepares instances of bot-systems that are spoken to via events
        /// and therefore need to be there to receive said events.
        /// </summary>
        public Bootstrapper EnsureInstance() {
            this.diContainer.Get<IConnectionManager>();
            this.diContainer.Get<SqlUserFactory>();
            return this;
        }

        public Bootstrapper EnablePlugins() {
            loader.EnablePlugins(diContainer);
            return this;
        }
        
        public Bootstrapper PrepareBot() {
            var bot = this.diContainer.Get<DamageBot>();
            bot.SetConfiguration(this.cfg);
            bot.InitCallbacks();
            bot.PrepareTwitch();
            return this;
        }

        public DamageBot GetBot() {
            return diContainer.Get<DamageBot>();
        }

        /// <summary>
        /// Runs through a series of calls to prepare all the stuff
        /// that is needed to run the bot and the plugins.
        /// </summary>
        /// <returns></returns>
        public DamageBot Bootstrap() {
            return PrepareDi().BindDatabaseImplementation<SqliteConnectionManager>().LoadPlugins().FinaliseDi().EnsureInstance().EnablePlugins().PrepareBot().GetBot();
        }
    }
}