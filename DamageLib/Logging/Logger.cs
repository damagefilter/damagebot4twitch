using System;
using log4net;

namespace DamageBot.Logging {
    /// <summary>
    /// Logger wrapping a logging framework.
    /// This exists because we might, eventually, someday,
    /// want to change the logging framework and crawling through the whole application
    /// to change all occurences of some apache logger... blergh.
    /// </summary>
    public class Logger {

        private readonly ILog log;

        public Logger(ILog logger) {
            this.log = logger;
        }

        public void Debug(object message) {
            this.log.Debug(message);
        }

        public void Debug(object message, Exception exception) {
            this.log.Debug(message, exception);
        }


        public void Error(object message) {
            this.log.Error(message);
        }

        public void Error(object message, Exception exception) {
            this.log.Error(message, exception);
        }


        public void Fatal(object message) {
            this.log.Fatal(message);
        }

        public void Fatal(object message, Exception exception) {
            this.log.Fatal(message, exception);
        }

        public void Info(object message) {
            this.log.Info(message);
        }

        public void Info(object message, Exception exception) {
            this.log.Info(message, exception);
        }

        public void Warn(object message) {
            this.log.Warn(message);
        }

        public void Warn(object message, Exception exception) {
            this.log.Warn(message, exception);
        }
    }
}