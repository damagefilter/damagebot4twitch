using System;

namespace DamageBot.Commands {
    public class CommandAttribute : Attribute {
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
        /// Optional list of permissions that are required to raise this command
        /// </summary>
        public string[] Permissions;

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