using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DamageBot.Di;
using DamageBot.Events.Database;
using DamageBot.Logging;

namespace DamageBot.Plugins {
    /// <summary>
    /// Reads a pre-configured path for plugin assemblies.
    /// Loads the assemblies.
    /// </summary>
    public class PluginLoader {
        // public const string pluginPath = "plugins"; // relative path to plugins (from where the exe is executed)

        private List<Plugin> plugins;

        private Logger log;

        public PluginLoader() {
            plugins = new List<Plugin>();
            log = LogManager.GetLogger(GetType());
        }

        public void LoadPlugins() {
            LoadPlugins(null);
        }

        public void LoadPlugins(string pluginPath = null) {
            // When we run as service, we must specify an absolute path because
            // the service working directory is not the directory where the actual executable is
            if (string.IsNullOrEmpty(pluginPath)) {
                pluginPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "plugins");
            }

            //var pluginPath = "plugins";
            log.Info(string.Format("Plugins directory: {0}", pluginPath));
            if (plugins != null && plugins.Count > 0) {
                throw new PluginLoaderException("Plugins appear to be loaded already.");
            }
            plugins = new List<Plugin>();
            
            if (!Directory.Exists(pluginPath)) {
                log.Info(string.Format("Plugins directory ({0}) doesn't exist. Creating new one.", pluginPath));
                Directory.CreateDirectory(pluginPath);
            }
            
            foreach (var file in Directory.EnumerateFiles(pluginPath, "*.dll")) {
                log.Info(string.Format("Attempting to load plugin dll from {0}", file));
                // load the dll
                var dll = Assembly.LoadFrom(file);

                // Scan the dll for stuff implementing Plugins.
                List<Type> pluginTypes = new List<Type>();
                Type pluginType = typeof(Plugin);
                foreach (var cls in dll.GetTypes()) {
                    if (pluginType.IsAssignableFrom(cls) && !cls.IsAbstract && !cls.IsInterface) {
                        pluginTypes.Add(cls);
                    }
                }

                foreach (var type in pluginTypes) {
                    try {
                        var instance = (Plugin)Activator.CreateInstance(type);
                        log.Info("Loaded plugin " + type);
                        plugins.Add(instance);
                    }
                    catch (Exception e) {
                        log.Error(string.Format("Loading plugin {0} failed.", type), e);
                    }
                }
            }
        }
        
        public void InitialisePluginResources(DependencyContainer di) {
            log.Info("Initialising plugin resources...");
            foreach (var plugin in plugins) {
                log.Info("Initialising " + plugin.GetDescriptor().Name);
                plugin.InitResources(di);
            }
        }

        public void EnsureInstallations() {
            foreach (Plugin plugin in plugins) {
                var descriptor = plugin.GetDescriptor();
                var select = new SelectEvent();
                select.TableList = "plugins";
                select.FieldList.Add("plugin_version");
                select.FieldList.Add("plugin_id");
                select.WhereClause = $"plugin_name = '{descriptor.Name}'";
                select.Call();
                if (select.ReadNext()) {
                    string version = select.GetString("plugin_version");
                    if (version != descriptor.Version) {
                        plugin.UpdateRoutine(version);
                        var update = new UpdateEvent();
                        update.TableName = "plugins";
                        update.DataList.Add("plugin_version", descriptor.Version);
                        update.WhereClause = $"plugin_id = '{select.GetInteger("plugin_id")}'";
                        update.Call();
                    }
                }
                else {
                    plugin.InstallRoutine();
                    var insert = new InsertEvent();
                    insert.TableName = "plugins";
                    insert.DataList.Add("plugin_name", descriptor.Name);
                    insert.DataList.Add("plugin_author", descriptor.Author);
                    insert.DataList.Add("plugin_version", descriptor.Version);
                    insert.Call();
                }
            }
           
        }

        public void EnsureUpdates() {
            
        }

        public void EnablePlugins(DependencyContainer di) {
            log.Info("Enabling plugins ...");
            foreach (var plugin in plugins) {
                log.Info("Enabling " + plugin.GetDescriptor().Name);
                plugin.Enable(di);
            }
        }
    }
}