using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            set;
        }

        /// <summary>
        /// At least this much words from the secondary pool must be contained in the chatter text.
        /// </summary>
        public int RequiredSecondaryMatches {
            get;
            set;
        }

        /// <summary>
        /// Probability in percent that the bot will respond at all.
        /// </summary>
        public int ResponseProbability {
            get;
            set;
        }

        /// <summary>
        /// The delay to wait before sending out the response.
        /// Makes the bot more human-looking when it#s not firing back answers right away.
        /// </summary>
        public int RespondTime {
            get;
            set;
        }
        
        /// <summary>
        /// When the bot is engaged in a conversation with someone,
        /// this can ignore the primary words pool.
        /// Can be used so that you don't have to address the bot by its name in order to trigger a response.
        /// </summary>
        public bool CanIgnorePrimary {
            get;
            set;
        }

        /// <summary>
        /// Context for which this node is applicable.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ResponseContext Context {
            get;
            set;
        }

        /// <summary>
        /// Describes the tone of the possible responses.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Mood ResponseMood {
            get;
            set;
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
            set;
        }
        
        
    }
}