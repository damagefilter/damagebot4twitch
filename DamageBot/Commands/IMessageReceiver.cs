namespace DamageBot.Commands {
    public interface IMessageReceiver {

        /// <summary>
        /// Sends a message to this MessageReceiver.
        /// You may use unity multi markup thingamybobs
        /// </summary>
        /// <param name="message"></param>
        void Message(string message);

        /// <summary>
        /// Returns true if at least one of the permissions are granted.
        /// False otherwise
        /// </summary>
        /// <param name="permissions"></param>
        /// <returns></returns>
        bool HasPermission(params string[] permissions);
    }
}