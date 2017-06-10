﻿using DamageBot.Di;

namespace DamageBot.Plugins {
    public abstract class Plugin {

        protected PluginDescriptor localDescriptor;

        /// <summary>
        /// In this step a plugin provides Resources to the Bots Di Container.
        /// Called before Enable.
        /// Di is NOT ready for reading at this stage. You may only add your injectibles here.
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
        /// Called after InitResources, before Enable but after the Di has been prepared
        /// so it is at your disposal at that point.
        /// </summary>
        public abstract void InstallRoutine();

        /// <summary>
        /// Called from the plugin loader when this plugin was installed already
        /// but there's a version mismatch detected.
        /// Gives the plugin a chance to update resources if necessary.
        /// Called after InitResources, before Enable but after the Di has been prepared
        /// so it is at your disposal at that point.
        /// </summary>
        /// <param name="installedVersion"></param>
        public abstract void UpdateRoutine(string installedVersion);
        
        /// <summary>
        /// Called on the Plugin.
        /// You get a reference to the relevant descriptor of your plugin.
        /// It's your job to fill versioning, author and name information into it correctly.
        /// Do it!
        /// It's used, for instance, to determine whether or not the plugin needs installing or updating
        /// routines called on it.
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected abstract PluginDescriptor InternalPreparePluginDescriptor(PluginDescriptor descriptor);

        /// <summary>
        /// Get a suitable descriptor from this plugin which is then used
        /// in the plugin loader to manage its state and versioning etc
        /// </summary>
        /// <returns></returns>
        public PluginDescriptor GetDescriptor() {
            if (this.localDescriptor == null) {
                this.localDescriptor = new PluginDescriptor(this);
            }
            return InternalPreparePluginDescriptor(this.localDescriptor);
        }
    }
}