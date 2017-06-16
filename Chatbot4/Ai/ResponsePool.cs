using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chatbot4.Events;
using Newtonsoft.Json;

namespace Chatbot4.Ai {
    /// <summary>
    /// Contains all the raw response nodes.
    /// Generates from them prepared response nodes and ticker nodes.
    /// </summary>
    public class ResponsePool {
        private Dictionary<Mood, Dictionary<ResponseContext, List<RawResponseNode>>> loadedResponses;

        public ResponsePool() {
            var rawResponseList = JsonConvert.DeserializeObject<List<RawResponseNode>>(File.ReadAllText("chatter.json"));
            PrepareResponseStructure();
            foreach (var node in rawResponseList) {
                
                // Allows for placeholders in primary and secondary word pools (such as varying bot names etc)
                if (node.PrimaryWordPool != null) {
                    var replace = new BatchReplacePlaceholdersEvent(node.PrimaryWordPool);
                    replace.Call();
                    node.PrimaryWordPool = replace.Texts;
                }
                
                if (node.SecondaryWordPool != null) {
                    var replace = new BatchReplacePlaceholdersEvent(node.SecondaryWordPool);
                    replace.Call();
                    node.SecondaryWordPool = replace.Texts;
                }
                 
                loadedResponses[node.ResponseMood][node.Context].Add(node);
            }
        }

        public ResponseInfo FindNode(Mood idealMood, ResponseContext context, string userMessage) {
            //
            return null;
        }
        
        private void PrepareResponseStructure() {
            this.loadedResponses = new Dictionary<Mood, Dictionary<ResponseContext, List<RawResponseNode>>>();
            var badDictionary = new Dictionary<ResponseContext, List<RawResponseNode>> {
                {
                    ResponseContext.Join, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Part, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Timeout, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Ban, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Chat, new List<RawResponseNode>()
                }
            };
            this.loadedResponses.Add(Mood.Bad, badDictionary);
            
            var normalDict = new Dictionary<ResponseContext, List<RawResponseNode>> {
                {
                    ResponseContext.Join, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Part, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Timeout, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Ban, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Chat, new List<RawResponseNode>()
                }
            };
            this.loadedResponses.Add(Mood.Normal, normalDict);
            
            var goodDict = new Dictionary<ResponseContext, List<RawResponseNode>> {
                {
                    ResponseContext.Join, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Part, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Timeout, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Ban, new List<RawResponseNode>()
                },
                {
                    ResponseContext.Chat, new List<RawResponseNode>()
                }
            };
            this.loadedResponses.Add(Mood.Good, goodDict);
        }
    }
}