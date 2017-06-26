using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DamageBot.Events.Chat;
using DamageBot.EventSystem;
using DamageBot.Logging;
using DamageBot.Users;

namespace VanillaModeration {
    /// <summary>
    /// Thing monitors users and their strikes.
    /// </summary>
    public class UserMonitor {
        private readonly Logger log;

        private readonly ConcurrentDictionary<string, int> offenses;
        private readonly ModerationConfig modCfg;
        private readonly DamageBot.DamageBot bot;

        public UserMonitor(ModerationConfig cfg, DamageBot.DamageBot bot) {
            this.log = LogManager.GetLogger(GetType());
            this.offenses = new ConcurrentDictionary<string, int>();
            this.modCfg = cfg;
            this.bot = bot;
            EventDispatcher.Instance.Register<UserJoinedEvent>(OnUserJoined);
            EventDispatcher.Instance.Register<UserLeftEvent>(OnUserLeft);
            EventDispatcher.Instance.Register<MessageReceivedEvent>(OnUserChat);
        }

        public void OnUserJoined(UserJoinedEvent ev) {
            if (!offenses.ContainsKey(ev.User.Name)) {
                if (!offenses.TryAdd(ev.User.Name, 0)) {
                    log.Error($"Failed to insert {ev.User.Name} into offenses watchlist. Trying again on next chat message.");
                }
            }
        }

        public void OnUserChat(MessageReceivedEvent ev) {
            if (!offenses.ContainsKey(ev.User.Name)) {
                if (!offenses.TryAdd(ev.User.Name, 0)) {
                    log.Error($"Failed to insert {ev.User.Name} into offenses watchlist. Trying again on next chat message.");
                    return;
                }
            }
            if (CheckBadWords(ev.User, ev.Message) || CheckLinks(ev.User, ev.Message)) {
                offenses[ev.User.Name]++;
                int numOffenses = offenses[ev.User.Name];
                bot.ClearChatForUser(ev.User);
                bot.TwitchIrcClient.SendMessage(ev.User.Status.Channel, $"{ev.User.Name}: That was a strike. You have {modCfg.OffenseThreshold - numOffenses} strikes before the Banhammer falls.");
            }
            if (offenses[ev.User.Name] >= modCfg.OffenseThreshold) {
                if (modCfg.DefaultBanTime > 0) {
                    bot.BanUser(ev.User);
                }
                else {
                    bot.TimeoutUser(ev.User, TimeSpan.FromSeconds(modCfg.DefaultBanTime));
                }
                offenses[ev.User.Name] = 0;
                bot.TwitchIrcClient.SendMessage(ev.User.Status.Channel, $"And the Banhammer came down on {ev.User.Name}");
            }
        }

        public void OnUserLeft(UserLeftEvent ev) {
            if (offenses.ContainsKey(ev.User.Name)) {
                int entry;
                if (!offenses.TryRemove(ev.User.Name, out entry)) {
                    log.Error($"Failed to remove {ev.User.Name} from watched list.");
                }
            }
        }
        
        private bool CheckLinks(IUser user, string message) {
            if (user.Status.IsBroadcaster || user.Status.IsModerator) {
                return false;
            }
            // Big funky url regex https://stackoverflow.com/questions/6038061/regular-expression-to-find-urls-within-a-string
            return Regex.IsMatch(message, "(http|ftp|https)://([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:/~+#-]*[\\w@?^=%&/~+#-])?");
        }

        private bool CheckBadWords(IUser user, string message) {
            if (user.Status.IsBroadcaster || user.Status.IsModerator) {
                return false;
            }
            return Regex.IsMatch(message, string.Join("|", this.modCfg.BadWords));
        }
    }
}