using System.Collections.Generic;
using System.Text.RegularExpressions;
using DamageBot.Users;

namespace Chatbot4.Ai {
    public class Conversation {
        private ResponsePool responses;
        private Mood currentMood;
        private readonly ChatbotConfig botConfig;
        private int currentMoodValue;
        private bool conversationRunning;
        
        public Conversation(ResponsePool pool, ChatbotConfig cfg) {
            this.responses = pool;
            this.currentMood = Mood.Normal;
            botConfig = cfg;
            currentMoodValue = 0;
        }

        public void HandleMessage(string incomingMessage, IUser sendingUser, ResponseContext context) {
            EvaluateMood(incomingMessage);
            var node = responses.FindNode(currentMood, context, conversationRunning, incomingMessage, sendingUser);
            if (node == null) {
                return;
            }
            if (!conversationRunning) {
                conversationRunning = true;
            }
            // TODO: is delay > 0 schedule sending message otherwise send directly
        }

        private void EvaluateMood(string message) {
            int badMatches = -1 * CountMatches(message, botConfig.NegativeWords);
            int goodMatches = CountMatches(message, botConfig.PositiveWords);
            currentMoodValue += goodMatches + badMatches;
            if (currentMoodValue >= 35) {
                currentMood = Mood.Good;
            }
            else if (currentMoodValue <= -35) {
                currentMood = Mood.Bad;
            }
            else {
                currentMood = Mood.Normal;
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