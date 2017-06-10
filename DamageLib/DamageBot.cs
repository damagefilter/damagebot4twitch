using System;
using DamageBot.Commands;
using DamageBot.Database;
using DamageBot.Events.Chat;
using DamageBot.Events.Users;
using DamageBot.EventSystem;
using DamageBot.Logging;
using DamageBot.Tasking;
using DamageBot.Users;
using TwitchLib;
using TwitchLib.Events.Client;
using TwitchLib.Models.Client;

namespace DamageBot {
    public class DamageBot {
        private BotConfig configuration;
        
        private TwitchClient twitchIrcClient;
        private readonly TaskQueue tasks;
        
        private CommandManager cmds;

        private readonly Logger log;

        public bool IsRunning => twitchIrcClient.IsConnected;

        /// <summary>
        /// </summary>
        /// <param name="cmds"></param>
        public DamageBot(CommandManager cmds) {
            log = LogManager.GetLogger(GetType());
            this.tasks = new TaskQueue();
            this.cmds = cmds;
        }

        public void SetConfiguration(BotConfig config) {
            this.configuration = config;
        }

        public void PrepareTwitch() {
            log.Info("Setting up channel user");
            var creds = new ConnectionCredentials(configuration.TwitchUsername, configuration.TwitchUserAuthKey);
            this.twitchIrcClient = new TwitchClient(creds, configuration.Channel);
            
            log.Info("Preparing API Access.");
            TwitchAPI.Settings.ClientId = configuration.ApplicationClientId;
            TwitchAPI.Settings.AccessToken = configuration.ApiAuthKey;
            
        }

        public void Connect() {
            log.Info("Connecting to IRC");
            this.twitchIrcClient.Connect();
            log.Info("Connected? " + this.twitchIrcClient.IsConnected);
            
            this.tasks.StartPolling();
            log.Info("Ready. Lets roll.");
        }

        public void Disconnect() {
            log.Info("Disconnecting bot.");
            this.twitchIrcClient.Disconnect();
        }
        
        public void InitCallbacks() {
            this.twitchIrcClient.OnJoinedChannel += OnBotJoinedChannel;
            
            this.twitchIrcClient.OnUserJoined += OnJoinedChannel;
            this.twitchIrcClient.OnMessageReceived += OnMessageReceived;
            this.twitchIrcClient.OnChatCommandReceived += OnChatCommand;
            this.twitchIrcClient.OnWhisperCommandReceived += OnWhisperCommand;
            this.twitchIrcClient.OnUserLeft += OnUserLeftChannel;

            EventDispatcher.Instance.Register<RequestChannelMessageEvent>(OnChannelMessageRequest);
            EventDispatcher.Instance.Register<RequestWhisperMessageEvent>(OnWhisperMessageRequest);
        }
        
        private void OnBotJoinedChannel(object sender, OnJoinedChannelArgs agrs) {
            this.twitchIrcClient.SendMessage("I am here! Therefore I am!");
        }
        private void OnJoinedChannel(object sender, OnUserJoinedArgs data) {
            tasks.Add(() => {
                var user = GetUser(data.Username, data.Channel);
                if (user == null) {
                    return;
                }
                new UserJoinedEvent(user, data.Channel).Call();
            });
        }
        
        private void OnUserLeftChannel(object sender, OnUserLeftArgs data) {
            tasks.Add(() => {
                var user = GetUser(data.Username, data.Channel);
                if (user == null) {
                    return;
                }
                new UserLeftEvent(user, data.Channel).Call();
            });
        }
        
        private void OnMessageReceived(object sender, OnMessageReceivedArgs data) {
            
            if (data.ChatMessage.Message.StartsWith("!")) {
                return;
            }
            tasks.Add(() => {
                var user = GetUser(data.ChatMessage);
                if (user == null) {
                    return;
                }
                new MessageReceivedEvent(data.ChatMessage.Channel, data.ChatMessage.Message, user).Call();
            });
            
        }

        private void OnChatCommand(object sender, OnChatCommandReceivedArgs data) {
            tasks.Add(() => {
                var user = GetUser(data.Command.ChatMessage);
                if (user == null) {
                    return;
                }
                try {
                    cmds.ParseCommand(user, data.Command.Command, data.Command.ArgumentsAsList.ToArray());
                }
                catch (Exception e) {
                    user.Message("Command failed: " + e.Message);
                    log.Error("Command failed: " + e.Message, e);
                }
            });
        }
        
        private void OnWhisperCommand(object sender, OnWhisperCommandReceivedArgs data) {
            this.twitchIrcClient.SendMessage($"{data.Command} whisper command received.");
        }


        private void OnChannelMessageRequest(RequestChannelMessageEvent ev) {
            this.twitchIrcClient.SendMessage(ev.Channel ?? twitchIrcClient.JoinedChannels[0].Channel, ev.Message);
        }
        
        private void OnWhisperMessageRequest(RequestWhisperMessageEvent ev) {
            this.twitchIrcClient.SendWhisper(ev.User.Username, ev.Message);
        }

        

        private IUser GetUser(ChatMessage msg) {
            var r = new RequestUserEvent(msg.Username);
            r.Call();
            var user = r.ResolvedUser;
            if (user == null) {
                log.Warn("Unable to resolve user " + msg.Username);
                return null;
            }
            var elevation = Elevation.Viewer;
            if (msg.IsBroadcaster) {
                elevation = Elevation.Broadcaster;
            }
            if (msg.IsModerator) {
                elevation = Elevation.Moderator;
            }
            user.Status = new ChatStatus(elevation, msg.Channel, msg.IsSubscriber);
            return user;
        }
        
        private IUser GetUser(string username, string channel) {
            var r = new RequestUserEvent(username);
            r.Call();
            var user = r.ResolvedUser;
            if (user == null) {
                log.Warn("Unable to resolve user " + username);
                return null;
            }
            user.Status = new ChatStatus(Elevation.Viewer, channel, false);
            return user;
        }
    }
}