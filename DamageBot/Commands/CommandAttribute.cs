using System;
using DamageBot.Users;

namespace DamageBot.Commands {
    public class CommandAttribute : Attribute {
        
        /// <summary>
        /// Is this command a "command not found" fallback handler?
        /// If so it'll be stored in a separate list of commands that get queried
        /// when no other registered command handler can resolve the requested command.
        /// 
        /// Fallback handlers need no aliases and are not parented so you can basically omit that.
        /// 
        /// </summary>
        public bool IsFallbackHandler;
        
        /// <summary>
        /// Ways to call this command.
        /// Example: say, talk, scream, speak for a command outputting some text somewhere
        /// </summary>
        public string[] Aliases;

        /// <summary>
        /// The name of the parent command for creating
        /// sub-command structures
        /// </summary>
        public string Parent;

        /// <summary>
        /// What this command does
        /// </summary>
        public string Description;

        /// <summary>
        /// Tip shown when command parsing failes.
        /// </summary>
        public string ToolTip;

        /// <summary>
        /// Terms used for lookup when using command search
        /// </summary>
        public string[] HelpSearchTerms;
        
        /// <summary>
        /// At least this elevation level must be met to execute this command
        /// </summary>
        public Elevation RequiredElevation;

        /// <summary>
        /// min amount of parameters.
        /// 0 means no min amount
        /// </summary>
        public int MinParams;

        /// <summary>
        /// Max amount of parameters. 0 means no max amount.
        /// </summary>
        public int MaxParams;
    }
}