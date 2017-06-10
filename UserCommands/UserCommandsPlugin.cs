using System;
using System.Linq;
using DamageBot.Commands;
using DamageBot.Di;
using DamageBot.Events.Chat;
using DamageBot.Events.Database;
using DamageBot.Logging;
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
            var ct = new CreateTableEvent();
            ct.TableName = "user_commands";
            ct.FieldDefinitions.Add("id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
            ct.FieldDefinitions.Add("alias VARCHAR(255) NOT NULL");
            ct.FieldDefinitions.Add("response TEXT NOT NULL");
            ct.IndexName = "alias_idx";
            ct.IndexFieldList = "alias";
            ct.Call();
        }

        public override void UpdateRoutine(string installedVersion) {
            // nothin net
        }

        protected override PluginDescriptor InternalPreparePluginDescriptor(PluginDescriptor descriptor) {
            descriptor.Name = "User Commands";
            descriptor.Author = "damagefilter";
            descriptor.Version = "1.0";
            return descriptor;
        }
    }

    public class Command {

        private Logger log;

        public Command() {
            log = LogManager.GetLogger(GetType());
        }
        /// <summary>
        /// This here acts as a fallback command resolver which effectively allows us
        /// to raise unknown commands (as defined per users)
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command( 
            IsFallbackHandler = true,
            Description = "Resolves a command by name and an action.",
            RequiredElevation = Elevation.Viewer
        )]
        public bool ResolverCommand(IMessageReceiver caller, string command, string[] args) {
            var select = new SelectEvent();
            select.TableList = "user_commands";
            select.FieldList.Add("response");
            select.WhereClause = $"alias = '{command}'";
            select.Call();
            if (select.ReadNext()) {
                log.Info("Resolved command " + command);
                new RequestChannelMessageEvent(caller.Status.Channel, select.GetString("response")).Call();
                return true;
            }
            return false;
        }

        [Command( 
            Aliases = new[]{"newcmd", "addcmd"}, 
            Description = "Create a new command.",
            ToolTip = "!newcmd <command name> <response text>",
            MinParams = 2,
            RequiredElevation = Elevation.Moderator
        )]
        public void RegisterCommand(IMessageReceiver caller, string[] args) {
            try {
                var insert = new InsertEvent();
                insert.TableName = "user_commands";
                insert.DataList.Add("alias", args[0]);
                insert.DataList.Add("response", string.Join(" ", args.Skip(1)));
                insert.Call();
                caller.Message($"Command {args[0]} has been added.");
                log.Info("Added command " + args[0]);
            }
            catch (Exception e) {
                caller.Message($"Command {args[0]} has not been added. Reason: {e.Message}.");
                log.Error("Not added command " + args[0], e);
            }
        }
        
        [Command( 
            Aliases = new[]{"remcmd", "delcmd"}, 
            Description = "Remove a command by name.",
            ToolTip = "!delcmd <command name>",
            MinParams = 1,
            RequiredElevation = Elevation.Moderator
        )]
        public void RemoveCommand(IMessageReceiver caller, string[] args) {
            try {
                var delete = new DeleteEvent();
                delete.TableName = "user_commands";
                delete.WhereClause = $"alias = '{args[0]}'";
                delete.Call();
                caller.Message($"Command {args[0]} has not been removed.");
                log.Info("Removed command " + args[0]);
            }
            catch (Exception e) {
                caller.Message($"Command {args[0]} has not been removed. Reason: {e.Message}.");
                log.Error("Not removed command " + args[0], e);
            }
        }
    }
}