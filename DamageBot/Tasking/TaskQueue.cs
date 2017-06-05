using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DamageBot.EventSystem;
using DamageBot.Logging;

namespace DamageBot.Tasking {
    /// <summary>
    /// Queues tasks. So that a message won't block the main thread.
    /// </summary>
    public class TaskQueue {
        /// <summary>
        /// Backlog of things to do.
        /// So we can basically move the high-load management from the pool to here.
        /// </summary>
        private BlockingCollection<Executable> backlog;

        private int maxThreads;
        private int currentRunnigThreads;
        private Logger log;

        /// <summary>
        /// Checks whether the execution queue should be polled for more or not.
        /// If this is false, the event pumping is effectively off.
        /// </summary>
        public bool IsPolling {
            get;
            private set;
        }

        public TaskQueue() {
            this.backlog = new BlockingCollection<Executable>();
            log = LogManager.GetLogger(GetType());
            int doNotNeed;
            ThreadPool.GetMaxThreads(out maxThreads, out doNotNeed);
        }

        public void Add(Action action) {
            backlog.Add(new Executable(() => {
                try {
                    action?.Invoke();
                }
                catch (Exception e) {
                    log.Error("Task action failed: " + e.Message, e);
                }
                finally {
                    currentRunnigThreads--;
                }
                
            }));
        }

        public void StartPolling() {
            log.Info("Starting the pumping of messages");
            IsPolling = true;
            // do taht in parallel so the polling doesn't block the mein thread.
            Task.Run(() => Process());
            log.Info("Started the message pump");
        }

        public void StopPolling() {
            IsPolling = false;
            log.Info("Stopping message pumping.");
        }

        public void Process() {
            while (IsPolling) {
                if (backlog.Count > 0) {
                    if (currentRunnigThreads >= maxThreads) {
                        continue; // Wait for some older threads to return.
                    }
                    var execution = backlog.Take();
                    currentRunnigThreads++;
                    ThreadPool.QueueUserWorkItem(execution.Run);
                }
            }
        }
    }
}