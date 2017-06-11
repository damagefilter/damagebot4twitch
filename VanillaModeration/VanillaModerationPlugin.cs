using System;
using System.Text.RegularExpressions;
using DamageBot.Commands;
using DamageBot.Plugins;
using DamageBot.Di;
using DamageBot.Events.Chat;
using DamageBot.Users;

namespace VanillaModeration {
    class VanillaModerationPlugin : Plugin {
        
        public DamageBot.DamageBot Bot {
            get;
            private set;
        }

        public ModerationConfig ModConfig {
            get;
            private set;
        }

        /// <summary>
        /// This here only exists because it's listening to message events.
        /// But it doesn't need to do anything else with it.
        /// </summary>
        private UserMonitor userMon;
        
        public override void InitResources(DependencyContainer diContainer) {
            
        }

        public override void Enable(DependencyContainer diContainer) {
            Bot = diContainer.Get<DamageBot.DamageBot>();
            ModConfig = ModerationConfig.LoadConfig();
            diContainer.Get<CommandManager>().RegisterCommandsInObject(new ControlCommands(ModConfig), false);
            this.userMon = new UserMonitor(ModConfig, Bot);
        }

        public override void InstallRoutine() {
            //throw new NotImplementedException();
        }

        public override void UpdateRoutine(string installedVersion) {
            //throw new NotImplementedException();
        }

        protected override PluginDescriptor InternalPreparePluginDescriptor(PluginDescriptor descriptor) {
            descriptor.Author = "damagefilter";
            descriptor.Name = "Vanilla Moderation";
            descriptor.Version = "1.0";
            return descriptor;
        }
    }

    public class ControlCommands {
        private readonly ModerationConfig cfg;
        public ControlCommands(ModerationConfig cfg) {
            this.cfg = cfg;
        }
        [Command(
            Aliases = new []{"set_option"}, 
            Description = "Base command to set a moderation option to the big.",
            MinParams = 1,
            ToolTip = "!set_option <links|bantime|strikes>",
            RequiredElevation = Elevation.Moderator
        )]
        public void SetOptionBase(IMessageReceiver caller,  string[] args) {
            // just a container to grant a base command
        }
        
        [Command(
            Aliases = new []{"links"},
            Parent = "set_option",
            Description = "Allow or diallow links in chat.",
            ToolTip = "!set_option links <[allow|1 disallow|0]>",
            MinParams = 1,
            RequiredElevation = Elevation.Moderator
        )]
        public void SetOptionLinks(IMessageReceiver caller,  string[] args) {
            if (Regex.IsMatch(args[0], "^allow|^1")) {
                cfg.DisallowLinks = false;
                new RequestChannelMessageEvent(caller.Status.Channel, "Posting Links is now allowed!").Call();
            }
            
            else if (Regex.IsMatch(args[0], "^disallow|^0")) {
                cfg.DisallowLinks = true;
                new RequestChannelMessageEvent(caller.Status.Channel, "Posting Links is now disallowed!").Call();
            }
            else {
                caller.Message($"'{args[0]}' is not a known parameter type.");
            }
            cfg.Save();
        }
        
        [Command(
            Aliases = new []{"bantime"},
            Parent = "set_option",
            Description = "Manage how long bans should be. 0 or negative values cause perma bans.\ns=seconds, m=minutes, h=hours",
            ToolTip = "!set_option bantime <number[s|m|h]>",
            MinParams = 1,
            RequiredElevation = Elevation.Moderator
        )]
        public void SetOptionBans(IMessageReceiver caller,  string[] args) {
            if (!Regex.IsMatch(args[0], "\\d+[smhd]+")) {
                cfg.DisallowLinks = false;
                caller.Message($"{args[0]} is the wrong format. It needs to be a number and a letter (s=seconds, m=minutes, h=hours, d=days)");
                return;
            }
            var match = Regex.Match(args[0], "(\\d)+([smhd])+");
            var time = match.Groups[1].Value;
            var timeSpec = match.Groups[2].Value;
            int iTime = int.Parse(time); // Regex already checked this is a number.
            if (iTime <= 0) {
                cfg.DefaultBanTime = -1;
            }
            else {
                // regex also checked that this must be one of those
                switch (timeSpec) {
                    case "s":
                        cfg.DefaultBanTime = iTime;
                        break;
                    case "m:":
                        cfg.DefaultBanTime = TimeSpan.FromMinutes(iTime).Seconds;
                        break;
                    case "h:":
                        cfg.DefaultBanTime = TimeSpan.FromHours(iTime).Seconds;
                        break;
                    case "d:":
                        cfg.DefaultBanTime = TimeSpan.FromDays(iTime).Seconds;
                        break;
                }
            }
            cfg.Save();
            caller.Message("Ban time has been update.");
        }
        
        [Command(
            Aliases = new []{"strikes"},
            Parent = "set_option",
            Description = "Number of times a user is allowed to do an offense before banhammer drops. Numbers smaller or equal to zero turn off auto-bans",
            ToolTip = "!set_option strikes <number of strikes>",
            MinParams = 1,
            RequiredElevation = Elevation.Moderator
        )]
        public void SetOptionStrikes(IMessageReceiver caller,  string[] args) {
            if (Regex.IsMatch(args[0], "\\d")) {
                cfg.OffenseThreshold = int.Parse(args[0]);
                cfg.BanOnOffense = cfg.OffenseThreshold > 0;
                cfg.Save();
                caller.Message($"Offense threshold is now '{args[0]}'");
            }
            else {
                caller.Message($"'Offense threshold must be number but it is '{args[0]}'");
            }
        }
        
        [Command(
            Aliases = new []{"badword"},
            Description = "Add or remove bad words.",
            ToolTip = "!badword <add|remove|instaclear>",
            MinParams = 1,
            RequiredElevation = Elevation.Moderator
        )]
        public void BadWord(IMessageReceiver caller,  string[] args) {
            if (Regex.IsMatch(args[0], "\\d")) {
                cfg.OffenseThreshold = int.Parse(args[0]);
                cfg.BanOnOffense = cfg.OffenseThreshold > 0;
                cfg.Save();
                caller.Message($"Offense threshold is now '{args[0]}'");
            }
            else {
                caller.Message($"'Offense threshold must be number but it is '{args[0]}'");
            }
        }
        
        [Command(
            Aliases = new []{"add"},
            Parent = "badword",
            Description = "Add a bad word to the bots internal bad word filter",
            ToolTip = "!badword add <word [word2 word2 word4 etc]>",
            MinParams = 1,
            RequiredElevation = Elevation.Moderator
        )]
        public void AddBadWord(IMessageReceiver caller,  string[] args) {
            cfg.BadWords.AddRange(args);
            cfg.Save();
            caller.Message("Bad word filter updated.");
        }
        
        [Command(
            Aliases = new []{"remove"},
            Parent = "badword",
            Description = "Remove a bad word to the bots internal bad word filter",
            ToolTip = "!badword remove <word [word2 word2 word4 etc]>",
            MinParams = 1,
            RequiredElevation = Elevation.Moderator
        )]
        public void RemoveBadWord(IMessageReceiver caller,  string[] args) {
            cfg.BadWords.RemoveAll(e => Array.IndexOf(args, e) >= 0);
            cfg.Save();
            caller.Message("Bad word filter updated.");
        }
    }
}
