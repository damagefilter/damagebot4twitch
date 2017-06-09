using System.Runtime.InteropServices;
using DamageBot.Commands;
using DamageBot.Di;
using DamageBot.Events.Database;
using DamageBot.Plugins;
using DamageBot.Users;

namespace Statistics {
    public class StatisticsPlugin : Plugin {
        
        public override void InitResources(DependencyContainer diContainer) {
            diContainer.AddBinding(typeof(UserStatsRecorder), true);
        }

        public override void Enable(DependencyContainer diContainer) {
            var userStatsRecorder = diContainer.Get<UserStatsRecorder>();
            diContainer.Get<CommandManager>().RegisterCommandsInObject(new ControlCommands(userStatsRecorder), false);
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
            
            ct = new CreateTableEvent();
            ct.TableName = "stream_statistics";
            ct.FieldDefinitions.Add("id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
            // When was this session recorded
            ct.FieldDefinitions.Add("session_started DATETIME NOT NULL");
            // How long did the stream go (time in seconds)
            ct.FieldDefinitions.Add("session_length TEXT NOT NULL");

            ct.FieldDefinitions.Add("user_id INTEGER NOT NULL");
            ct.FieldDefinitions.Add("FOREIGN KEY(user_id) REFERENCES users(user_id)");
            ct.Call();
        }

        public override void UpdateRoutine(string installedVersion) {
            // don't need yet
            //throw new System.NotImplementedException();
        }
    }

    public class ControlCommands {

        private readonly UserStatsRecorder userRecorder;
        public ControlCommands(UserStatsRecorder userRecorder) {
            this.userRecorder = userRecorder;
        }
        [Command(
            Aliases = new []{"startrec"}, 
            Description = "Starts the recording of statistics for this session",
            ToolTip = "!startrec",
            RequiredElevation = Elevation.Broadcaster
        )]
        public void StartStreamCommand(IMessageReceiver caller,  string[] args) {
            userRecorder.StartRecording();
        }
        
        [Command(
            Aliases = new []{"stoprec"}, 
            Description = "Stops the recording of statistics for this session manually.\nOtherwise stops when broadcaster leaves channel.",
            ToolTip = "!stoprec",
            RequiredElevation = Elevation.Broadcaster
        )]
        public void StopStreamCommand(IMessageReceiver caller,  string[] args) {
            userRecorder.StopRecording();
        }
    }
}