using System;

namespace DamageBot.Tasking {
    public class Executable {
        private Action action;


        public Executable(Action action) {
            this.action = action;
        }

        /// <summary>
        /// Callback used in the threadpool task queue thing.
        /// Requires that there's an object parameter but it can be null.
        /// Also we totally don't need it.
        /// </summary>
        /// <param name="stateInfo"></param>
        public void Run(object stateInfo) {
            this.action?.Invoke();
        }
    }
}