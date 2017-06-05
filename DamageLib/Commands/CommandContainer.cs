using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DamageBot.Commands {
    public delegate void CommandDelegate(IMessageReceiver caller, string[] args);

    public class CommandContainer {
        public CommandDelegate commandLogic;
        
        public CommandAttribute MetaInfo {
            get;
        }

        private readonly List<CommandContainer> subCommands;

        private CommandContainer parent;
        public CommandContainer Parent {
            get {
                return parent;
            }
            set {
                if (this.parent != null) {
                    this.parent.subCommands.Remove(this);
                }
                this.parent = value;
                this.parent.subCommands.Add(this);
            }
        }

        public CommandContainer(CommandAttribute metaInfo, CommandDelegate cmd) {
            this.MetaInfo = metaInfo;
            this.commandLogic = cmd;
            this.subCommands = new List<CommandContainer>();
        }

        /// <summary>
        /// Parses the argument list and delegates it to the command logic.
        /// Whatever command logic is will then happen.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool ParseAndExecuteCommand(IMessageReceiver caller, string[] args) {
            if (args.Length < MetaInfo.MinParams || ((args.Length > MetaInfo.MaxParams) && (MetaInfo.MaxParams != 0))) {
                OnBadSyntax(caller, args);
                return false;
            }
            this.commandLogic(caller, args);
            return true;
        }

        /// <summary>
        /// get a sub command for the given alias.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public CommandContainer GetSubCommand(string alias) {
            for (int i = 0; i < this.subCommands.Count; ++i) {
                if (this.subCommands[i].MetaInfo.Aliases.Any(t => t == alias)) {
                    return this.subCommands[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Recursively generates a flat list of all sub commands and its children starting from the first caller
        /// </summary>
        /// <param name="list"></param>
        public void FlattenSubCommands(List<CommandContainer> list) {
            // If parent was null, this wouldn't be a subcommand.
            if (this.Parent != null) {
                list.Add(this);
            }
            for (int i = 0; i < subCommands.Count; ++i) {
                subCommands[i].FlattenSubCommands(list);
            }
        }

        /// <summary>
        /// Get the list of sub commands.
        /// </summary>
        /// <returns></returns>
        public List<CommandContainer> GetSubCommands() {
            return this.subCommands;
        }

        public bool HasSubCommand(string alias) {
            for (int i = 0; i < this.subCommands.Count; ++i) {
                if (this.subCommands[i].MetaInfo.Aliases.Any(t => t == alias)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if this command is known as the given alias.
        ///
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public bool HasAlias(string alias) {
            return this.MetaInfo.Aliases.Any(t => t == alias);
        }

        /// <summary>
        /// Check if this command is supposed to be a root command.
        /// </summary>
        /// <returns></returns>
        public bool IsRootCommand() {
            return string.IsNullOrEmpty(this.MetaInfo.Parent);
        }

        /// <summary>
        /// Try adding a sub command.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="index"></param>
        /// <param name="commandContainer"></param>
        /// <returns></returns>
        public bool TryAddSubCommand(string[] path, int index, CommandContainer commandContainer) {
            if (index >= path.Length) {
                // this is the final path node
                bool noAliasMatchForIndex = Array.IndexOf(this.MetaInfo.Aliases, path[index - 1]) == -1;
                if (noAliasMatchForIndex) {
                    return false;
                }
                commandContainer.Parent = this; // Takes care of subcommanding too
                return true;
            }
            // not the final node, keep traversing
            return HasSubCommand(path[index]) && GetSubCommand(path[index]).TryAddSubCommand(path, ++index, commandContainer);
        }

        /// <summary>
        /// Compile a help text for this command and its sub commands sorted by indent levels.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="indentLevel"></param>
        public void CompileHelp(StringBuilder sb, int indentLevel) {
            string indent = "";
            for (int i = 0; i <= indentLevel; ++i) {
                indent += "    ";
            }
            sb.AppendLine(indent + this.MetaInfo.ToolTip);
            sb.AppendLine(indent + this.MetaInfo.Description);
            for (int i = 0; i < this.subCommands.Count; ++i) {
                this.subCommands[i].CompileHelp(sb, indentLevel + 1);
            }
        }

        /// <summary>
        /// Called when argument count mismatches.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="parameters"></param>
        protected void OnBadSyntax(IMessageReceiver caller, string[] parameters) {
            StringBuilder sb = new StringBuilder();
            this.CompileHelp(sb, 0);
            caller.Message(sb.ToString());
        }
    }
}