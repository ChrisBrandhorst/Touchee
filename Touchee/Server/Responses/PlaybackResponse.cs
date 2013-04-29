using Touchee.Devices;

namespace Touchee.Server.Responses {

    /// <summary>
    /// Response for all playback-related data
    /// </summary>
    public class PlaybackResponse : ToucheeResponse {

        public int MasterVolume { get; protected set; }
        public bool MasterMuted { get; protected set; }

        public PlaybackResponse() {
            this.MasterVolume = Device.MasterVolume.Volume;
            this.MasterMuted = Device.MasterVolume.Muted;
        }
    }

}
