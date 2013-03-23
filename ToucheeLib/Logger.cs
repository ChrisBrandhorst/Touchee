using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Touchee {

    /// <remarks>
    /// Static class taking care of logging.
    /// </remarks>
    public static class Logger {

        /// <remarks>
        /// Available log levels.
        /// </remarks>
        public enum LogLevel {
            Trace, Debug, Info, Warn, Error, Fatal
        }

        /// <summary>
        /// Gets or sets the current log level of the logger.
        /// </summary>
        static LogLevel _level = LogLevel.Info;
        public static LogLevel Level {
            get { return _level; }
            set { _level = value; }
        }

        /// <summary>
        /// Logs a message at the given log level, if the current loglevel is lower or equal to the given.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="level">The log level of the message.</param>
        /// <returns>A complete log string.</returns>
        public static string Log(string message, LogLevel level = LogLevel.Info) {
            System.Reflection.MethodBase methodBase = new StackTrace().GetFrame(1).GetMethod();
            if (methodBase.Name == "Log")
                methodBase = new StackTrace().GetFrame(2).GetMethod();

            //string m = String.Format(
            //    "{0} @ {1}#{2}\n       {3}",
            //    level.ToString(),
            //    methodBase.ReflectedType.FullName,
            //    methodBase.Name,
            //    message
            //);

            string m = String.Format(
                "[{0}]: {3} @ {1}#{2}",
                level.ToString(),
                methodBase.ReflectedType.FullName,
                methodBase.Name,
                message
            );

            if (Level <= level)
                Debug.WriteLine(m);

            return m;
        }

        /// <summary>
        /// Logs an exception message at the given log level, if the current loglevel is lower or equal to the given.
        /// </summary>
        /// <param name="exception">The exception to be logged.</param>
        /// <param name="level">The log level of the message.</param>
        /// <returns>A complete log string.</returns>
        public static string Log(Exception exception, LogLevel level = LogLevel.Info) {
            string message = exception.Message;
            if (exception.InnerException is Exception) {
                message += "\n\nInner exception:\n" + exception.InnerException.Message;
            }
            return Log(message, level);
        }

    }
}
