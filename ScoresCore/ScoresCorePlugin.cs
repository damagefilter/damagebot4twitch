using DamageBot.Di;
using DamageBot.Events.Database;
using DamageBot.Plugins;

namespace ScoresCore {
    /// <summary>
    /// This here plugin provides an API to implement suer scoring.
    /// Like 
    /// </summary>
    public class ScoresCorePlugin : Plugin {
        public override void InitResources(DependencyContainer diContainer) {
            throw new System.NotImplementedException();
        }

        public override void Enable(DependencyContainer diContainer) {
            
        }

        public override void InstallRoutine() {
            var ct = new CreateTableEvent();
            ct.TableName = "user_score";
            ct.FieldDefinitions.Add("id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
            ct.FieldDefinitions.Add("user_id INTEGER NOT NULL");
            ct.FieldDefinitions.Add("score INT NOT NULL");
            ct.Call();
        }

        public override void UpdateRoutine(string installedVersion) {
            
        }

        protected override PluginDescriptor InternalPreparePluginDescriptor(PluginDescriptor descriptor) {
            descriptor.Author = "damagefilter";
            descriptor.Version = "1.0";
            descriptor.Name = "Scores Core";
        }
    }
}