using DamageBot.Commands;
using DamageBot.Di;
using DamageBot.Plugins;
using DamageBot.Users;

namespace UserCommands {
    public class UserCommandsPlugin : Plugin {
        public override void InitResources(DependencyContainer diContainer) {
        }

        public override void Enable(DependencyContainer diContainer) {
            diContainer.Get<CommandManager>().RegisterCommandsInObject(new Command(), false);
        }

        public override void InstallRoutine() {
            
        }

        public override void UpdateRoutine(string installedVersion) {
        }

        public override PluginDescriptor GetDescriptor() {
            base.GetDescriptor();
            localDescriptor.Name = "UserCommands";
            localDescriptor.Author = "damagefilter";
            localDescriptor.Version = "1.0";
            return localDescriptor;
        }
    }

    public class Command {
        
        [Command( 
            IsFallbackHandler = true,
            Description = "Resolves a command by name and an action.",
            RequiredElevation = Elevation.Viewer
        )]
        public bool ResolverCommand(IMessageReceiver caller, string command, string[] args) {
            return false;
        }

        [Command( 
            Aliases = new[]{"newcmd", "addcmd"}, 
            Description = "Create a new command.",
            MinParams = 2,
            RequiredElevation = Elevation.Moderator
        )]
        public void RegisterCommand(IMessageReceiver caller, string[] args) {
            
        }
    }
}