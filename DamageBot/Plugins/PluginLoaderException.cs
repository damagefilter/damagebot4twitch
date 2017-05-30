using System;

namespace DamageBot.Plugins {
    public class PluginLoaderException : Exception {
        public PluginLoaderException(string msg) : base(msg) {
        }
        
        public PluginLoaderException(string msg, Exception root) : base(msg, root) {
        }
    }
}