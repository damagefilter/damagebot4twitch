using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DamageBot.Events.Chat;
using DamageBot.Users;

namespace Chatbot4.Ai {
    public class Conversation {
        private readonly ResponsePool responses;
        private readonly ChatbotConfig botConfig;
        private readonly IUser conversationPartner;
        
        private Mood currentMood; // last known mood of this conversation.
        private int currentMoodValue; // used to evaluate which mood should be set
        private bool conversationRunning;

        private DateTime lastSpokenTo;

        private Random random;
        
        public Conversation(ResponsePool pool, ChatbotConfig cfg, IUser conversationPartner) {
            this.responses = pool;
            this.currentMood = Mood.Normal;
            botConfig = cfg;
            currentMoodValue = 0;
            this.conversationPartner = conversationPartner;
            this.random = new Random();
        }

        public void HandleTickerMessage() {
            
        }

        public void HandleMessage(string incomingMessage, ResponseContext context) {
            if (incomingMessage != null) {
                EvaluateMood(incomingMessage);
            }
            
            var node = responses.FindNode(currentMood, context, conversationRunning, incomingMessage, conversationPartner);
            if (node == null) {
                return;
            }
            if (!conversationRunning) {
                conversationRunning = true;
            }
            // we use this to make the bot forget it was spoken to in order to not drag a conversation along for hours.
            if ((lastSpokenTo - DateTime.Now).Minutes > 2) {
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