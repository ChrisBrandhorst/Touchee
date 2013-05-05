using Touchee.Devices;

namespace Touchee.Server.Responses {

    /// <summary>
    /// Response for all playback-related data
    /// </summary>
    public class PlaybackResponse : ToucheeResponse {

        public int Duration { get; protected set; }
        public int Position { get; protected set; }
        public bool Playing { get; protected set; }

        public PlaybackResponse() {
            
        }
    }

}
