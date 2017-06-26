using System;
using System.Collections.Generic;
using DamageBot.Commands;
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
        public TwitchClient TwitchIrcClient {
            get;
            set;
        }

        private readonly TaskQueue tasks;
        
        private readonly CommandManager cmds;

        private readonly Logger log;

        public bool IsRunning => TwitchIrcClient.IsConnected;

        public BotConfig Configuration {
            get;
            private set;
        }

        /// <summary>
        /// </summary>
        /// <param name="cmds"></param>
        public DamageBot(CommandManager cmds) {
            log = LogManager.GetLogger(GetType());
            this.tasks = new TaskQueue();
            this.cmds = cmds;
        }

        public void SetConfiguration(BotConfig config) {
            this.Configuration = config;
        }

        public void PrepareTwitch() {
            log.Info("Setting up channel user");
            var creds = new ConnectionCredentials(Configuration.TwitchUsername, Configuration.TwitchUserAuthKey);
            this.TwitchIrcClient = new TwitchClient(creds, Configuration.Channel);
            
            log.Info("Preparing API Access.");
            TwitchAPI.Settings.ClientId = Configuration.ApplicationClientId;
            TwitchAPI.Settings.AccessToken = Configuration.ApiAuthKey;
            
        }

        public void Connect() {
            log.Info("Connecting to IRC");
            this.TwitchIrcClient.Connect();
            log.Info("Connected? " + this.TwitchIrcClient.IsConnected);
            
            this.tasks.StartPolling();
            log.Info("Ready. Lets roll.");
        }

        public void Disconnect() {
            log.Info("Disconnecting bot.");
            this.TwitchIrcClient.Disconnect();
        }
        
        public void InitCallbacks() {
            this.TwitchIrcClient.OnJoinedChannel += OnBotJoinedChannel;
            
            this.TwitchIrcClient.OnUserJoined += OnJoinedChannel;
            this.TwitchIrcClient.OnMessageReceived += OnMessageReceived;
            this.TwitchIrcClient.OnChatCommandReceived += OnChatCommand;
            this.TwitchIrcClient.OnWhisperCommandReceived += OnWhisperCommand;
            this.TwitchIrcClient.OnUserLeft += OnUserLeftChannel;

            EventDispatcher.Instance.Register<RequestChannelMessageEvent>(OnChannelMessageRequest);
            EventDispatcher.Instance.Register<RequestWhisperMessageEvent>(OnWhisperMessageRequest);
        }

        private void OnBotJoinedChannel(object sender, OnJoinedChannelArgs agrs) {
            this.TwitchIrcClient.SendMessage("I am here! Therefore I am!");
        }
        private void OnJoinedChannel(object sender, OnUserJoinedArgs data) {
            tasks.Add(() => {
                var user = GetUser(data.Username, null, data.Channel);
                if (user == null) {
                    return;
                }
                new UserJoinedEvent(user, data.Channel).Call();
            });
        }
        
        private void OnUserLeftChannel(object sender, OnUserLeftArgs data) {
            tasks.Add(() => {
                var user = GetUser(data.Username, null, data.Channel);
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
            this.TwitchIrcClient.SendMessage($"{data.Command} whisper command received.");
        }


        private void OnChannelMessageRequest(RequestChannelMessageEvent ev) {
            this.TwitchIrcClient.SendMessage(ev.Channel ?? TwitchIrcClient.JoinedChannels[0].Channel, ev.Message);
        }
        
        private void OnWhisperMessageRequest(RequestWhisperMessageEvent ev) {
            this.TwitchIrcClient.SendWhisper(ev.User.Name, ev.Message);
        }

        

        private IUser GetUser(ChatMessage msg) {
            var r = new RequestUserEvent(msg.Username, msg.UserId);
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
        
        private IUser GetUser(string username, string twitchId, string channel) {
            var r = new RequestUserEvent(username, twitchId);
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