using Touchee.Components.Playback;

namespace Touchee.Server.Responses {

    /// <summary>
    /// Response for all playback-related data
    /// </summary>
    public class PlaybackResponse : ToucheeResponse {

        public int Duration { get; protected set; }
        public int Position { get; protected set; }
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
