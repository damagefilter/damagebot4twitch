using System.Linq;

namespace DamageBot.Commands {
    public delegate bool FallbackCommandDelegate(IMessageReceiver caller, string command, string[] args);
    
    public class FallbackCommandContainer {

        private readonly FallbackCommandDelegate commandLogic;

        
        public FallbackCommandContainer(FallbackCommandDelegate cmd) {
            this.commandLogic = cmd;
        }
        
        public bool ParseAndExecuteCommand(IMessageReceiver caller, string cmd, string[] args) {
            return commandLogic(caller, cmd, args);
        }
    }
}