namespace Chatbot4.Ai {
    /// <summary>
    /// Sets the mood for a pool of messages.
    /// You can give Good-Mood messages a more positive tone
    /// while bad-Mood messages can have a bad tone.
    /// 
    /// During a conversation the bot will evaluate by a set of words or phrases
    /// if the user it is talking to is hostile or friendly and pick from
    /// the corresponding response pool. If nothing else is found, Normal mood responses will be made. 
    /// </summary>
    public enum Mood {
        Good,
        Normal,
        Bad
    }
}