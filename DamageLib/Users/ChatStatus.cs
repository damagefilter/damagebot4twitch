namespace DamageBot.Users {
    /// <summary>
    /// Contains information about the last messages chat status.
    /// Since it is sent through twitch on every message it would appear
    /// to make no sense to store it anywhere.
    /// Also, while you CAN change the values here it's actually read-only.
    /// This will be renewed on ever message.
    /// </summary>
    public struct ChatStatus {
        
        /// <summary>
        /// Who is the guy?
        /// A viewer, a moderator or the broadcaster himself?
        /// </summary>
        public Elevation UserElevation {
            get;
            set;
        }

        /// <summary>
        /// Is this person a subscriber?
        /// </summary>
        public bool IsSubscriber {
            get;
            set;
        }
        
        /// <summary>
        /// Is this the channel operator (broadcaster)?
        /// </summary>
        public bool IsBroadcaster => this.UserElevation == Elevation.Broadcaster;

        public string Channel {
            get;
            set;
        }

        public ChatStatus(Elevation el, string channel, bool isSub) {
            this.UserElevation = el;
            this.IsSubscriber = isSub;
            this.Channel = channel;
        }
    }
}