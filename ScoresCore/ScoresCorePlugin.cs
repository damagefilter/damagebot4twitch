using DamageBot.Commands;
using DamageBot.Database;
using DamageBot.Di;
using DamageBot.Events.Chat;
using DamageBot.Events.Database;
using DamageBot.Events.Users;
using DamageBot.Plugins;
using DamageBot.Users;

namespace ScoresCore {
    /// <summary>
    /// This here plugin provides an API to implement suer scoring.
    /// Like 
    /// </summary>
    public class ScoresCorePlugin : Plugin {
        private ScoresRecorder recorder;
        private ScoresConfig cfg;
        public override void InitResources(DependencyContainer diContainer) {
        }

        public override void Enable(DependencyContainer diContainer) {
            this.cfg = ScoresConfig.LoadConfig();
            this.recorder = new ScoresRecorder(diContainer.Get<IConnectionManager>(), cfg);
            this.recorder.StartRecording();
            diContainer.Get<CommandManager>().RegisterCommandsInObject(new BasicCommands(this.recorder, this.cfg), false);
        }

        public override void InstallRoutine() {
            var ct = new CreateTableEvent();
            ct.TableName = "user_score";
            ct.FieldDefinitions.Add("id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
            ct.FieldDefinitions.Add("user_id INTEGER NOT NULL");
            ct.FieldDefinitions.Add("score INT NOT NULL");
            ct.Call();
        }

        public override void UpdateRoutine(string installedVersion) {
            
        }

        protected override PluginDescriptor InternalPreparePluginDescriptor(PluginDescriptor descriptor) {
            descriptor.Author = "damagefilter";
            descriptor.Version = "1.0";
            descriptor.Name = "Scores Core";
            return descriptor;
        }
    }

    public class BasicCommands {
        private readonly ScoresRecorder userRecorder;
        private ScoresConfig cfg;
        public BasicCommands(ScoresRecorder recorder, ScoresConfig cfg) {
            this.userRecorder = recorder;
            this.cfg = cfg;
        }
        [Command(
            Aliases = new []{"points"}, 
            Description = "Show callers points",
            ToolTip = "!points",
            RequiredElevation = Elevation.Viewer
        )]
        public void ShowPoints(IMessageReceiver caller,  string[] args) {
            if (!(caller is IUser)) {
                caller.Message("Only users can have points. You are not a user.");
                return;
            }
            var score = userRecorder.GetScoreForUser((IUser)caller);
            string msg = cfg.ScoreInfoMessageTemplate
                .Replace("{amount}", score.ScoreValue.ToString())
                .Replace("{currency}", cfg.CurrencyName)
                .Replace("{username}", caller.Name);
            new RequestChannelMessageEvent(caller.Status.Channel, msg).Call();
        }
        
        [Command(
            Aliases = new []{"add"},
            Parent = "points",
            MinParams = 2,
            Description = "Add point to someone",
            ToolTip = "!points add <username> <amount>",
            RequiredElevation = Elevation.Moderator
        )]
        public void AddPoints(IMessageReceiver caller,  string[] args) {
            var usrRequest = new RequestUserEvent(args[0], null);
            usrRequest.Call();
            var receiver = usrRequest.ResolvedUser;
            if (receiver == null) {
                new RequestChannelMessageEvent(caller.Status.Channel, $"{args[0]} is an unknown user").Call();
                return;
            }
            var score = userRecorder.GetScoreForUser(receiver);
            score.ScoreValue += int.Parse(args[1]); 
            string msg = cfg.ScoreRewardedTemplate
                .Replace("{amount}", score.ScoreValue.ToString())
                .Replace("{currency}", cfg.CurrencyName)
                .Replace("{username}", caller.Name)
                .Replace("{receiver}", receiver.Name);
            new RequestChannelMessageEvent(caller.Status.Channel, msg).Call();
        }
        
        [Command(
            Aliases = new []{"remove"},
            Parent = "points",
            MinParams = 2,
            Description = "Remove point from someone",
            ToolTip = "!points remove <username> <amount>",
            RequiredElevation = Elevation.Moderator
        )]
        public void RemovePoints(IMessageReceiver caller,  string[] args) {
            var usrRequest = new RequestUserEvent(args[0], null);
            usrRequest.Call();
            var receiver = usrRequest.ResolvedUser;
            if (receiver == null) {
                new RequestChannelMessageEvent(caller.Status.Channel, $"{args[0]} is an unknown user").Call();
                return;
            }
            var score = userRecorder.GetScoreForUser(receiver);
            var scoreAdd = int.Parse(args[1]);
            score.ScoreValue += scoreAdd; 
            string msg = cfg.ScoreDeductedTemplate
                .Replace("{amount}", scoreAdd.ToString())
                .Replace("{currency}", cfg.CurrencyName)
                .Replace("{username}", caller.Name)
                .Replace("{receiver}", receiver.Name);
            new RequestChannelMessageEvent(caller.Status.Channel, msg).Call();
        }
    }
}