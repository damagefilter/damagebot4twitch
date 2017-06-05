using System;
using System.Collections.Generic;

namespace DamageBot.Logging {
    /// <summary>
    /// This is where you will get all your loggers from.
    /// Nowhere else.
    /// Here.
    /// Okay?
    /// Okay.
    /// This class exists to hide the implementation specific log manager.
    /// So we can swap that thing behind the scenes if we have to.
    /// We probably won't. But still.
    /// </summary>
    public class LogManager {
        #region Singleton
        private static LogManager _instance;

        private static LogManager Instance {
            get {
                if (_instance == null) {
                    _instance = new LogManager();
                }
                return _instance;
            }
        }
        #endregion

        private readonly Dictionary<Type, Logger> loadedLoggers;

        private LogManager() {
            loadedLoggers = new Dictionary<Type, Logger>();
        }

        /// <summary>
        /// Get the logger for the given type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Logger GetLogger(Type t) {
            if (Instance.loadedLoggers.ContainsKey(t)) {
                return Instance.loadedLoggers[t];
            }
            Logger log = new Logger(log4net.LogManager.GetLogger(t));
            Instance.loadedLoggers.Add(t, log);
            return log;
        }

        public static void ConfigureLogger() {
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}