using System;

namespace DamageBot.Commands {
    /// <summary>
    /// Thrown in CommandManager when dependencies cannot be sorted out.
    /// </summary>
    public class CommandDependencyException : Exception {
        public CommandDependencyException(string message) : base (message) {
        }

        public CommandDependencyException(string message, Exception cause) : base (message, cause) {
        }
    }
}