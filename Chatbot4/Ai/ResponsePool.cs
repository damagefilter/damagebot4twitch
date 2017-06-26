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

        private readonly Random random;

        public ResponsePool(ChatbotConfig cfg) {
            var rawResponseList = JsonConvert.DeserializeObject<List<RawResponseNode>>(File.ReadAllText("chatter.json"));
            PrepareResponseStructure();
            this.random = new Random();
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

        /// <summary>
        /// Find a random node for the given mood or a normal mood and given context, if none is found in the ideal mood list
        /// </summary>
        /// <param name="idealMood"></param>
        /// <param name="context"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ResponseInfo FindRandomNodeForContextAndMood(Mood idealMood, ResponseContext context, IUser user) {
            var idealNodes = this.loadedResponses[idealMood][context];
            var defaultNodes = this.loadedResponses[Mood.Normal][context];
            if (idealNodes.Count > 0) {
                var node = idealNodes[random.Next(0, idealNodes.Count - 1)];
                var replacer = new ReplacePlaceholdersEvent(GetRandomAnswer(node.Answers));
                replacer.Call();
                return new ResponseInfo(user != null ? replacer.Text.Replace("{CURRENT_USER}", user.Name) : replacer.Text, node.ResponseProbability, node.RespondTime);
            }
            if (defaultNodes.Count > 0) {
                var node = defaultNodes[random.Next(0, idealNodes.Count - 1)];
                var replacer = new ReplacePlaceholdersEvent(GetRandomAnswer(node.Answers));
                replacer.Call();
                return new ResponseInfo(user != null ? replacer.Text.Replace("{CURRENT_USER}", user.Name) : replacer.Text, node.ResponseProbability, node.RespondTime);
            }
            return null;
        }
        
        public ResponseInfo GetTickerNode() {
            var nodes = this.loadedResponses[Mood.Normal][ResponseContext.Ticker];
            
            if (nodes.Count > 0) {
                var node = nodes[random.Next(0, nodes.Count - 1)];
                var replacer = new ReplacePlaceholdersEvent(GetRandomAnswer(node.Answers));
                replacer.Call();
                return new ResponseInfo(replacer.Text, node.ResponseProbability, node.RespondTime);
            }
            return null;
        }

        /// <summary>
        /// Find a node with response information for the given message.
        /// </summary>
        /// <param name="idealMood">The ideal response mood.</param>
        /// <param name="context">The context of the given message</param>
        /// <param name="allowIgnorePrimary">Set true to skip primary word checks on nodes that allow skipping these checks. (CanIgnorePrimary)</param>
        /// <param name="userMessage">the message to find a response for</param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ResponseInfo FindNode(Mood idealMood, ResponseContext context, bool allowIgnorePrimary, string userMessage, IUser user) {
            if (context != ResponseContext.Chat) {
                return FindRandomNodeForContextAndMood(idealMood, context, user);
            }
            
            var idealNodes = this.loadedResponses[idealMood][context];
            var defaultNodes = this.loadedResponses[Mood.Normal][context];

            foreach (var node in idealNodes) {
                if ((node.CanIgnorePrimary && allowIgnorePrimary) || node.RequiredPrimaryMatches <= this.CountMatches(userMessage, node.PrimaryWordPool)) {
                    if (node.RequiredSecondaryMatches <= this.CountMatches(userMessage, node.SecondaryWordPool)) {
                        var replacer = new ReplacePlaceholdersEvent(GetRandomAnswer(node.Answers));
                        replacer.Call();
                        return new ResponseInfo(replacer.Text.Replace("{CURRENT_USER}", user.Name), node.ResponseProbability, node.RespondTime);
                    }
                }
            }
            
            foreach (var node in defaultNodes) {
                if ((node.CanIgnorePrimary && allowIgnorePrimary) || node.RequiredPrimaryMatches <= this.CountMatches(userMessage, node.PrimaryWordPool)) {
                    if (node.RequiredSecondaryMatches <= this.CountMatches(userMessage, node.SecondaryWordPool)) {
                        var replacer = new ReplacePlaceholdersEvent(GetRandomAnswer(node.Answers));
                        replacer.Call();
                        return new ResponseInfo(replacer.Text.Replace("{CURRENT_USER}", user.Name), node.ResponseProbability, node.RespondTime);
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
            return answers[random.Next(0, answers.Count - 1)];
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