using DamageBot.Di;

namespace DamageBot.Plugins {
    public abstract class Plugin {

        protected PluginDescriptor localDescriptor;

        /// <summary>
        /// In this step a plugin provides Resources to the Bots Di Container.
        /// No instances are made his this is purely configurational.
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