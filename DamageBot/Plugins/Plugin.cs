using DamageBot.Di;

namespace DamageBot.Plugins {
    public abstract class Plugin {

        protected PluginDescriptor localDescriptor;

        /// <summary>
        /// In this step a plugin provides Resources to the Bots Di Container.
        /// Called before Enable
        /// </summary>
        /// <param name="diContainer"></param>
        public abstract void InitResources(DependencyContainer diContainer);

        /// <summary>
        /// This phase is called after all plugins have finished providing their resources
        /// to the global bot Di Container.
        /// Use this to initialise whatever it is you need to initialise.
        /// </summary>
        /// <param name="diContainer"></param>
        public abstract void Enable(DependencyContainer diContainer);

        /// <summary>
        /// Called from the plugin loader when a plugin is detected the first time ever.
        /// Gives the plugin a chance to prepare resources such as tables in the database etc.
        /// Called before InitResources
        /// </summary>
        public abstract void InstallRoutine();

        /// <summary>
        /// Called from the plugin loader when this plugin was installed already
        /// but there's a version mismatch detected.
        /// Gives the plugin a chance to update resources if necessary.
        /// Called before InitResources
        /// </summary>
        /// <param name="installedVersion"></param>
        public abstract void UpdateRoutine(string installedVersion);

        /// <summary>
        /// Get a suitable descriptor from this plugin which is then used
        /// in the plugin loader to manage its state and such.
        /// How to use: Call base class, use returned descriptor to set
        /// appropritate data to the properties, return that.
        /// </summary>
        /// <returns></returns>
        public virtual PluginDescriptor GetDescriptor() {
            if (this.localDescriptor == null) {
                this.localDescriptor = new PluginDescriptor(this);
            }
            return this.localDescriptor;
        }
    }
}