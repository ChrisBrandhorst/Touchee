using System;
using System.Runtime.Serialization;

using SpotiFire;
using Touchee;

namespace Spotify {

    public class SessionStatus {

        public string State { get; private set; }
        public string Username { get; private set; }

        public SessionStatus(SessionHandler sessionHandler) {
            this.State = Enum.GetName(typeof(ConnectionState), sessionHandler.Session.ConnectionState).ToUnderscore();
            this.Username = sessionHandler.Session.ConnectionState == ConnectionState.LoggedIn ? sessionHandler.Session.UserName : null;
        }
        
    }

}
