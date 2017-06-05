using DamageBot.Users;

namespace DamageBot.Commands {
    public interface IMessageReceiver {

        /// <summary>
        /// Sends a message to this MessageReceiver.
        /// You may use unity multi markup thingamybobs
        /// </summary>
        /// <param name="message"></param>
        void Message(string message);

        /// <summary>
        /// Returns true if this IMessageReceiver has at least the given elevation level.
        /// False otherwise
        /// </summary>
        /// <param name="elevationLevel"></param>
        /// <returns></returns>
        bool HasPermission(Elevation elevationLevel);

        /// <summary>
        /// Chat status of this message receiver
        /// </summary>
        ChatStatus Status {
            get;
        }
    }
}