using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DamageBot.Events.Chat;
using DamageBot.Logging;
using DamageBot.Users;

namespace Chatbot4.Ai {
    public class Conversation {
        private readonly ResponsePool responses;
        private readonly ChatbotConfig botConfig;
        private readonly IUser conversationPartner;
        private readonly Logger log;
        
        private Mood currentMood; // last known mood of this conversation.
        private int currentMoodValue; // used to evaluate which mood should be set
        private bool conversationRunning;

        private DateTime lastSpokenTo;

        private readonly Random random;
        
        public Conversation(ResponsePool pool, ChatbotConfig cfg, IUser conversationPartner) {
            log = LogManager.GetLogger(GetType());
            this.responses = pool;
            this.currentMood = Mood.Normal;
            botConfig = cfg;
            currentMoodValue = 0;
            this.conversationPartner = conversationPartner;
            this.random = new Random();
            this.lastSpokenTo = DateTime.MinValue;
        }

        public void HandleMessage(string incomingMessage, ResponseContext context) {
            var originalMood = this.currentMood;
            if (incomingMessage != null) {
                this.currentMood = EvaluateMood(incomingMessage);
            }
            
            var node = responses.FindNode(currentMood, context, conversationRunning, incomingMessage, conversationPartner);
            if (node == null) {
                // reset mood because this was likely not directed at the bot
                this.currentMood = originalMood;
                return;
            }
            if (!conversationRunning) {
                log.Info("Conversation with " + conversationPartner.Name + " is marked running.");
                conversationRunning = true;
            }
            // we use this to make the bot forget it was spoken to in order to not drag a conversation along for hours.
            if (lastSpokenTo != DateTime.MinValue && (DateTime.Now - lastSpokenTo).Minutes > 2) {
                log.Info("Conversation with " + conversationPartner.Name + " has timed out. Minutes value is " + (DateTime.Now - lastSpokenTo).Minutes);
                conversationRunning = false;
            }
            lastSpokenTo = DateTime.Now;
            SendResponse(node);
        }

        public void SendResponse(ResponseInfo node) {
            if (random.Next(0, 100) <= node.ResponseProbability) {
                Task.Run(async () => {
                    await Task.Delay(TimeSpan.FromSeconds(node.ResponseDelay));
                    new RequestChannelMessageEvent(conversationPartner.Status.Channel, node.ResponseText).Call();
                });
            }
        }

        private Mood EvaluateMood(string message) {
            int badMatches = -1 * CountMatches(message, botConfig.NegativeWords);
            int goodMatches = CountMatches(message, botConfig.PositiveWords);
            currentMoodValue += goodMatches + badMatches;
            if (currentMoodValue >= 35) {
                return Mood.Good;
            }
            else if (currentMoodValue <= -35) {
                return Mood.Bad;
            }
            else {
                return Mood.Normal;
            }
        }
        
        private int CountMatches(string msg, List<string> wordMatchList) {
            for (int i = 0; i < wordMatchList.Count; ++i) {
                wordMatchList[i] = $"({wordMatchList[i]})";
            }
            var matches = Regex.Matches(msg, string.Join("|", wordMatchList), RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return matches.Count;
        }
    }
}