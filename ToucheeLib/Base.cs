using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee {

    /// <remarks>
    /// Base class for all Touchee objects. For now, only takes care of logging.
    /// </remarks>
    public abstract class Base {

        public string LastLogMessage { get; protected set; }

        public virtual void Log(string message, Logger.LogLevel level = Logger.LogLevel.Info) {
            LastLogMessage = Logger.Log(message, level);
        }
        public virtual void Log(string message, Exception e, Logger.LogLevel level = Logger.LogLevel.Error) {
            LastLogMessage = Logger.Log(message + "\nException thrown was: " + e.ToString(), level);
        }

        // Event handlers for messages and fatal errors.
        public delegate void FatalErrorOccuredHandler(Base toucheeObject, string message);
        public event FatalErrorOccuredHandler FatalErrorOccured;
        protected void OnFatalErrorOccured(string message) {
            if (FatalErrorOccured != null)
                FatalErrorOccured.Invoke(this, message);
        }

    }
}
