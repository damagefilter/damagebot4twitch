namespace Chatbot4.Ai {
    /// <summary>
    /// Context in which a message can appear.
    /// </summary>
    public enum ResponseContext {
        Join,
        Part,
        Timeout,
        Ban,
        Chat,
        Invalid
    }
}