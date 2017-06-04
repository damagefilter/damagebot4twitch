using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DamageBot.Logging;
using Fasterflect;

namespace DamageBot.Commands {
    /// <summary>
    /// This takes in any number of methods decorated with the CommandAttribute and
    /// creates a command tree from the information.
    /// In the end this is able to sort commands and sub commands.
    /// </summary>
    public class CommandManager {
        private Dictionary<string, CommandContainer> commands = new Dictionary<string, CommandContainer>();
        private Logger log;

        public CommandManager() {
            log = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Parses the given command and arguments.
        /// It will find the appropriate sub command and passes along the relevant arguments.
        /// The command logic is then executed or, if the --help switch exists a help text will be displayed.
        /// Help text is also displayed if the command or argument parsing failed at some point.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool ParseCommand(IMessageReceiver caller, string command, string[] args) {
            if (!this.commands.ContainsKey(command)) {
                return false;
            }
            CommandContainer baseCommand = this.commands[command];
            CommandContainer subCommand = null;
            CommandContainer tmp = null;
            int argumentIndex = 0;
            for (int i = 0; i < args.Length; ++i) {
                if (i == 0) {
                    tmp = baseCommand.GetSubCommand(args[0]);
                    if (tmp != null) {
                        subCommand = tmp;
                        ++argumentIndex;
                    }
                    continue;
                }
                if (tmp != null) {
                    if (tmp.HasSubCommand(args[argumentIndex])) {
                        tmp = tmp.GetSubCommand(args[argumentIndex]);
                        ++argumentIndex;
                    }

                    subCommand = tmp;
                }
            }

            if (subCommand == null) {
                bool hasHelp = args.Length > 0 && args[args.Length - 1] == "--help";
                if (args.Length == 0 && baseCommand.MetaInfo.MinParams > 0 || hasHelp) {
                    StringBuilder sb = new StringBuilder();
                    baseCommand.CompileHelp(sb, 0);
                    caller.Message(sb.ToString());
                    if (baseCommand.MetaInfo.MinParams > 0) {
                        return true;
                    }
                }
                if (hasHelp) {
                    return true;
                }
                return baseCommand.ParseAndExecuteCommand(caller, args);
            }
            if (args[args.Length - 1] == "--help") {
                StringBuilder sb = new StringBuilder();
                subCommand.CompileHelp(sb, 0);
                caller.Message(sb.ToString());
                return true;
            }
            return subCommand.ParseAndExecuteCommand(caller, args.Skip(argumentIndex).ToArray());
        }

        public void RegisterCommandsInObject(object listener, bool force) {
            var methods = listener.GetType().MethodsWith(Flags.InstancePublicDeclaredOnly, typeof(CommandAttribute));
            List<CommandContainer> newCommands = new List<CommandContainer>();
            // Generate a list of new commands
            for (int i = 0; i < methods.Count; ++i) {
                try {
                    // Throws on parameter mismatch
                    CommandAttribute attrib = methods[i].Attribute<CommandAttribute>();
                    CommandDelegate del = (CommandDelegate)Delegate.CreateDelegate(typeof(CommandDelegate), listener, methods[i].Name);
                    newCommands.Add(new CommandContainer(attrib, del));
                }
                catch {
                    log.Error($"Cannot register command for method {methods[i].Name}. Command signature must be (IMessageReceiver, string[])");
                }
            }
            // sort commands so that root commands come before their sub commands.
            newCommands.Sort((a, b) => {
                if (string.IsNullOrEmpty(a.MetaInfo.Parent) && string.IsNullOrEmpty(b.MetaInfo.Parent)) {
                    return 0;
                }
                if (string.IsNullOrEmpty(a.MetaInfo.Parent) && !string.IsNullOrEmpty(b.MetaInfo.Parent)) {
                    return -1;
                }
                if (!string.IsNullOrEmpty(a.MetaInfo.Parent) && string.IsNullOrEmpty(b.MetaInfo.Parent)) {
                    return 1;
                }
                int numA = a.MetaInfo.Parent.Split('.').Length;
                int numB = b.MetaInfo.Parent.Split('.').Length;
                return numA > numB ? 1 : numA < numB ? -1 : 0;
            });

            // And now finda place for the new commands inside the existing command tree.

            for (int i = 0; i < newCommands.Count; ++i) {
                try {
                    // First try to find the depending commands in the new set of commands.
                    // This throws a CommandDependencyException if not found.
                    this.SortDependencies(newCommands[i], newCommands);
                    this.UpdateCommandList(newCommands[i], force);
                }
                catch (CommandDependencyException) {
                    // try to find the dependency in the already existing commands.
                    // This may throw again which will end up as error in the log
                    this.SortDependencies(newCommands[i], this.commands.Values.ToList());
                    this.UpdateCommandList(newCommands[i], force);
                }
            }
        }

        /// <summary>
        /// Sorts out the given commands dependency on parent commands.
        /// After this the command is registered in the system and ready to go.
        ///
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="possibles"></param>
        /// <exception cref="CommandDependencyException"></exception>
        private void SortDependencies(CommandContainer cmd, List<CommandContainer> possibles) {
            if (string.IsNullOrEmpty(cmd.MetaInfo.Parent)) {
                return;
            }
            string[] commandPath = cmd.MetaInfo.Parent.Split('.');
            bool depMissing = true;
            // Check for local dependencies
            // Needs looping one more time because the list isn't keyed (and shouldn't be anyway)
            for (int i = 0; i < possibles.Count; ++i) {
                if (possibles[i].HasAlias(commandPath[0])) {
                    depMissing = !possibles[i].TryAddSubCommand(commandPath, 1, cmd);
                    break;
                }
            }

            if (depMissing) {
                string msg = $"{cmd.MetaInfo.Aliases[0]} has an unsatisfied dependency: {cmd.MetaInfo.Parent}. Adjust order of command registrations to fix this.";
                throw new CommandDependencyException(msg);
            }
        }

        /// <summary>
        /// Makes sure we have no duplicate root commands and sorts in the given command
        /// as root command, if it is, in fact, a root command
        /// </summary>
        /// <param name="com"></param>
        /// <param name="force"></param>
        private void UpdateCommandList(CommandContainer com, bool force) {
            if (!com.IsRootCommand()) {
                return;
            }
            bool hasDuplicate = false;
            StringBuilder dupes = new StringBuilder();
            for (int i = 0; i < com.MetaInfo.Aliases.Length; ++i) {
                bool currentIsDupe = false;
                if (this.commands.ContainsKey(com.MetaInfo.Aliases[i]) && !force) {
                    hasDuplicate = true;
                    currentIsDupe = true;
                    dupes.Append(com.MetaInfo.Aliases[i]).Append(" ");
                }
                if (!currentIsDupe) {
                    this.commands.Add(com.MetaInfo.Aliases[i], com);
                }
            }
            if (hasDuplicate) {
                log.Error("We've got duplicate commands: " + dupes);
            }
        }
    }
}