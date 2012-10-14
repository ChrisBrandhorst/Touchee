using System.Collections;

namespace Touchee.Server.Responses {

    public class ServerInfoResponse : ToucheeResponse {
        public string Name { get; set; }
        public string WelcomeMessage { get; set; }
        public int WebsocketPort { get; set; }
        public ArrayList Devices { get; set; }
        public string[] Plugins { get; set; }
        public long UtcTime { get; set; }
        public long UtcOffset { get; set; }
    }

}
