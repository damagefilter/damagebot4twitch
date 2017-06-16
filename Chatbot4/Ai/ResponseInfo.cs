namespace Chatbot4.Ai {
    /// <summary>
    /// This comes out of the ResponsePool and contains the relevant information
    /// to answer something. The text returned is unparsed and still needs replacing content.
    /// </summary>
    public class ResponseInfo {
        public string ResponseText {
            get;
        }

        public int ResponseProbability {
            get;
        }
        
        public int ResponseDelay {
            get;
        }

        public ResponseInfo(string answer, int probability, int delay) {
            this.ResponseText = answer;
            this.ResponseProbability = probability;
            this.ResponseDelay = delay;
        }
    }
}