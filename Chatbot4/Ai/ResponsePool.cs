using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Chatbot4.Events;
using DamageBot.Events.Chat;
using DamageBot.Users;
using Newtonsoft.Json;

namespace Chatbot4.Ai {
    /// <summary>
    /// Contains all the raw response nodes.
    /// Generates from them prepared response nodes and ticker nodes.
    /// </summary>
    public class ResponsePool {
        private Dictionary<Mood, Dictionary<ResponseContext, List<RawResponseNode>>> loadedResponses;

        public ResponsePool(ChatbotConfig cfg) {
            var rawResponseList = JsonConvert.DeserializeObject<List<RawResponseNode>>(File.ReadAllText("chatter.json"));
            PrepareResponseStructure();
            foreach (var node in rawResponseList) {
                
                // replace bot name placeholder with a list of all botnames
                if (node.PrimaryWordPool != null) {
                    if (node.PrimaryWordPool.Any((e) => Regex.IsMatch(e, "({BOT_NAME})"))) {
                        node.PrimaryWordPool.Remove("{BOT_NAME}");
                        node.PrimaryWordPool.AddRange(cfg.BotNicks);
                    }
                }
                
                if (node.SecondaryWordPool != null) {
                    if (node.SecondaryWordPool.Any((e) => Regex.IsMatch(e, "({BOT_NAME})"))) {
                        node.SecondaryWordPool.Remove("{BOT_NAME}");
                        node.SecondaryWordPool.AddRange(cfg.BotNicks);
                    }
                }
                loadedResponses[node.ResponseMood][node.Context].Add(node);
            }
        }

        public ResponseInfo FindTickerNode(Mood idealMood) {
            var idealNodes = this.loadedResponses[idealMood][ResponseContext.Ticker];
            var defaultNodes = this.loadedResponses[Mood.Normal][ResponseContext.Ticker];
            Random r = new Random();
            if (idealNodes.Count > 0) {
                var node = idealNodes[r.Next(0, idealNodes.Count - 1)];
                var replacer = new ReplacePlaceholdersEvent(GetRandomAnswer(node.Answers));
                replacer.Call();
                return new ResponseInfo(replacer.Text, node.ResponseProbability, node.RespondTime);
            }
            if (defaultNodes.Count > 0) {
                var node = defaultNodes[r.Next(0, idealNodes.Count - 1)];
                var replacer = new ReplacePlaceholdersEvent(GetRandomAnswer(node.Answers));
                replacer.Call();
                return new ResponseInfo(replacer.Text, node.ResponseProbability, node.RespondTime);
            }
            return null;
        }

        public ResponseInfo FindNode(Mood idealMood, ResponseContext context, bool conversationIsRunning, string userMessage, IUser user) {
            var idealNodes = this.loadedResponses[idealMood][context];
            var defaultNodes = this.loadedResponses[Mood.Normal][context];

            foreach (var node in idealNodes) {
                if ((node.CanIgnorePrimary && conversationIsRunning) || node.RequiredPrimaryMatches <= this.CountMatches(userMessage, node.PrimaryWordPool)) {
                    if (node.RequiredSecondaryMatches <= this.CountMatches(userMessage, node.SecondaryWordPool)) {
                        var replacer = new ReplacePlaceholdersEvent(GetRandomAnswer(node.Answers));
                        replacer.Call();
                        return new ResponseInfo(replacer.Text.Replace("{CURRENT_USER}", user.Username), node.ResponseProbability, node.RespondTime);
                    }
                }
            }
            
            foreach (var node in defaultNodes) {
                if ((node.CanIgnorePrimary && conversationIsRunning) || node.RequiredPrimaryMatches <= this.CountMatches(userMessage, node.PrimaryWordPool)) {
                    if (node.RequiredSecondaryMatches <= this.CountMatches(userMessage, node.SecondaryWordPool)) {
                        var replacer = new ReplacePlaceholdersEvent(GetRandomAnswer(node.Answers));
                        replacer.Call();
                        return new ResponseInfo(replacer.Text.Replace("{CURRENT_USER}", user.Username), node.ResponseProbability, node.RespondTime);
                    }
                }
            }
            return null;
        }

        private int CountMatches(string msg, List<string> wordMatchList) {
            for (int i = 0; i < wordMatchList.Count; ++i) {
                wordMatchList[i] = $"({wordMatchList[i]})";
            }
            var matches = Regex.Matches(msg, string.Join("|", wordMatchList), RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return matches.Count;
        }

        private string GetRandomAnswer(List<string> answers) {
            var r = new Random();
            return answers[r.Next(0, answers.Count - 1)];
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
                },
                {
                    ResponseContext.Ticker, new List<RawResponseNode>()
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
                },
                {
                    ResponseContext.Ticker, new List<RawResponseNode>()
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
                },
                {
                    ResponseContext.Ticker, new List<RawResponseNode>()
                }
            };
            this.loadedResponses.Add(Mood.Good, goodDict);
        }
    }
}