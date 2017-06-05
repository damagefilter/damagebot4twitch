namespace DamageBot.EventSystem {

    public abstract class Event<TImplementor> : IEvent where TImplementor : IEvent {

        /// <summary>
        /// Call this this hook on the EventDispatcher.
        /// </summary>
        public void Call() {
            EventDispatcher.Instance.Call<TImplementor>(this);
        }
    }
}