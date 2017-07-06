using DamageBot.Commands;
using DamageBot.Di;
using DamageBot.Events.Database;
using DamageBot.Events.Stream;
using DamageBot.EventSystem;
using DamageBot.Plugins;

namespace Statistics {
    
    /// <summary>
    /// Records simple stats about users and the channel like messages sent per user,
    /// stream length and such things.
    /// </summary>
    public class StatisticsPlugin : Plugin {
        public UserStatsRecorder UserRecorder {
            get;
            private set;
        }

        private void OnStreamStart(OnStreamStartEvent ev) {
            UserRecorder.StartRecording();
        }
        
        private void OnStreamEnd(OnStreamStopEvent ev) {
            UserRecorder.StopRecording();
        }

        public override void InitResources(DependencyContainer diContainer) {
            diContainer.AddBinding(typeof(UserStatsRecorder), true);
        }

        public override void Enable(DependencyContainer diContainer) {
            this.UserRecorder = diContainer.Get<UserStatsRecorder>();
            diContainer.Get<CommandManager>().RegisterCommandsInObject(new ControlCommands(this.UserRecorder), false);
            EventDispatcher.Instance.Register<OnStreamStartEvent>(OnStreamStart);
            EventDispatcher.Instance.Register<OnStreamStopEvent>(OnStreamEnd);
            this.UserRecorder.StartRecording();
        }

        public override void InstallRoutine() {
            var ct = new CreateTableEvent();
            ct.TableName = "user_statistics";
            ct.FieldDefinitions.Add("id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
            ct.FieldDefinitions.Add("user_id INTEGER NOT NULL");
            
            ct.FieldDefinitions.Add("stat_date DATE");
            // time inside channel.
            ct.FieldDefinitions.Add("time_watching INTEGER");
            
            ct.FieldDefinitions.Add("messages_sent INTEGER");
            // messages sent in all time

            ct.FieldDefinitions.Add("FOREIGN KEY(user_id) REFERENCES users(user_id)");
            ct.Call();
        }

        public override void UpdateRoutine(string installedVersion) {
            // don't need yet
            //throw new System.NotImplementedException();
        }

        protected override PluginDescriptor InternalPreparePluginDescriptor(PluginDescriptor descriptor) {
            descriptor.Author = "damagefilter";
            descriptor.Version = "1.0";
            descriptor.Name = "Chat Statistics";
            return descriptor;
        }
    }

    public class ControlCommands {

        private readonly UserStatsRecorder userRecorder;
        public ControlCommands(UserStatsRecorder userRecorder) {
            this.userRecorder = userRecorder;
        }
        // TODO: Commands to show how long one has been watching and all that stuff
//        [Command(
//            Aliases = new []{"startrec"}, 
//            Description = "Starts the recording of statistics for this session",
//            ToolTip = "!startrec",
//            RequiredElevation = Elevation.Broadcaster
//        )]
//        public void StartStreamCommand(IMessageReceiver caller,  string[] args) {
//            userRecorder.StartRecording();
//            new RequestChannelMessageEvent(caller.Status.Channel, "Recording started.").Call();
//        }
//        
//        [Command(
//            Aliases = new []{"stoprec"}, 
//            Description = "Stops the recording of statistics for this session manually.\nOtherwise stops when broadcaster leaves channel.",
//            ToolTip = "!stoprec",
//            RequiredElevation = Elevation.Broadcaster
//        )]
//        public void StopStreamCommand(IMessageReceiver caller,  string[] args) {
//            userRecorder.StopRecording();
//        }
    }
}