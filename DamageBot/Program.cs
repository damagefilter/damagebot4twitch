using System;
using System.Threading.Tasks;
using DamageBot.Logging;

namespace DamageBot {

    class Thing {
        public async Task<string> ReturnsThisString(string toReturn) {
            await Task.Delay(1000);
            return toReturn;
        }
    }
    
    internal class Program {
        public static void Main(string[] args) {
//            LogManager.ConfigureLogger();
            var thing = new Thing();
            Console.WriteLine("Starting. Calling async thing.");
            Task<string> val = thing.ReturnsThisString("Async Return");
            Console.WriteLine(val.Result);
            Console.WriteLine("End");
            Console.Read();

        }
    }
}