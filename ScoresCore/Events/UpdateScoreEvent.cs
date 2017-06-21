using DamageBot.EventSystem;
using DamageBot.Users;

namespace ScoresCore.Events {
    public class UpdateScoreEvent : Event<UpdateScoreEvent> {
        /// <summary>
        /// Score value to add to current score.
        /// If you want to remove score points, make it a negative value.
        /// Simple maths. Works fantatsic.
        /// </summary>
        public int ScoreValue {
            get;
        }
        
        public IUser User {
            get;
        }

        public UpdateScoreEvent(IUser user, int score) {
            this.ScoreValue = score;
            this.User = user;
        }
    }
}