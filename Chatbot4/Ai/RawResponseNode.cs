using System.Collections.Generic;

namespace Chatbot4.Ai {
    
    /// <summary>
    /// This is the full and complete, unsorted thing of nodes
    /// as read from the json file. Highly unoptimised and shit for querying but we'll get there.
    /// This is just for easy editing the bot chatter.
    /// </summary>
    public class RawResponseNode {
        /// <summary>
        /// At least this much words from the primary word pool must be contained in the chatter text
        /// </summary>
        public int RequiredPrimaryMatches {
            get;
            private set;
        }

        /// <summary>
        /// At least this much words from the secondary pool must be contained in the chatter text.
        /// </summary>
        public int RequiredSecondaryMatches {
            get;
            private set;
        }

        /// <summary>
        /// Probability in percent that the bot will respond at all.
        /// </summary>
        public int ResponseProbability {
            get;
            private set;
        }

        /// <summary>
        /// The delay to wait before sending out the response.
        /// Makes the bot more human-looking when it#s not firing back answers right away.
        /// </summary>
        public int RespondTime {
            get;
            private set;
        }
        
        /// <summary>
        /// When the bot is engaged in a conversation with someone,
        /// this can ignore the primary words pool.
        /// Can be used so that you don't have to address the bot by its name in order to trigger a response.
        /// </summary>
        public bool CanIgnorePrimary {
            get;
            private set;
        }

        /// <summary>
        /// Context for which this node is applicable.
        /// </summary>
        public ResponseContext Context {
            get;
            private set;
        }
        
        /// <summary>
        /// Describes the tone of the possible responses.
        /// </summary>
        public Mood ResponseMood {
            get;
            private set;
        }
        
        public List<string> PrimaryWordPool {
            get;
            set; // This is public because response pool can override elements in here
        }
        
        public List<string> SecondaryWordPool {
            get;
            set; // This is public because response pool can override elements in here
        }

        public List<string> Answers {
            get;
            private set;
        }
        
        
    }
}