using Touchee.Components.Playback;
using Newtonsoft.Json;

namespace Touchee.Server.Responses {

    /// <summary>
    /// Response for all playback-related data
    /// </summary>
    public class PlaybackResponse : ToucheeResponse {

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Duration { get; protected set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Position { get; protected set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Playing { get; protected set; }

        public PlaybackResponse(IPlayer player) {
            if (player == null) {
                this.Duration = -1;
                this.Position = -1;
            }
            else {
                this.Duration = player.Duration;
                this.Position = player.Position;
                this.Playing = player.Playing;
            }
        }

    }

}
