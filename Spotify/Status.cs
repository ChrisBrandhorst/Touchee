using System;
using System.Runtime.Serialization;

using SpotiFire;
using Touchee;

namespace Spotify {

    public class Status {

        [DataMember]
        public string State { get; private set; }
        [DataMember]
        public string Username { get; private set; }

        public Status(SessionHandler sessionHandler) {
            this.State = Enum.GetName(typeof(SessionState), sessionHandler.State).ToUnderscore();
            this.Username = sessionHandler.Session.UserName;
        }
        
    }

}
