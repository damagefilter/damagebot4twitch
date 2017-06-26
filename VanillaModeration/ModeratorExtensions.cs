using System;
using DamageBot.Users;
using TwitchLib.Extensions.Client;

namespace VanillaModeration {
    /// <summary>
    /// Extension class to add moderation features to the DamageBot
    /// </summary>
    public static class ModeratorExtensions {
        /// <summary>
        /// Basically a temporary ban on a user.
        /// This will also clear the chat messges for given user.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="user"></param>
        /// <param name="time"></param>
        /// <param name="message"></param>
        public static void TimeoutUser(this DamageBot.DamageBot bot, IUser user, TimeSpan time, string message = "") {
            bot.TwitchIrcClient.TimeoutUser(user.Status.Channel, user.Name, time, message);
        }
        
        /// <summary>
        /// Perma-ban a user
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="user"></param>
        /// <param name="message"></param>
        public static void BanUser(this DamageBot.DamageBot bot, IUser user, string message = "") {
            bot.TwitchIrcClient.BanUser(user.Status.Channel, user.Name, message);
        }
        
        /// <summary>
        /// Lift a perma ban or a timeout for the specified user.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="user"></param>
        public static void UnbanUser(this DamageBot.DamageBot bot, IUser user) {
            bot.TwitchIrcClient.UnbanUser(user.Name);
        }

        /// <summary>
        /// Clear the chat history for the given user
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="user"></param>
        public static void ClearChatForUser(this DamageBot.DamageBot bot, IUser user) {
            // Here's what's happening here:
            // The Timout command will also issue a CLEARCHAT for the timeouted user.
            // Effectively purging the guys chat history.
            // By making the timeout 1 second it's basically like no timeout at all.
            // so effectively, just purge this users messages.
            bot.TwitchIrcClient.TimeoutUser(user.Status.Channel, user.Name, TimeSpan.FromSeconds(1));
        }
    }
}