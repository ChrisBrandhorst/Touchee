using System;

namespace Touchee.Components.Playback {


    /// <remarks>
    /// Specifies a player which (also) emits audio
    /// </remarks>
    public interface IAudioPlayer : IPlayer {

        /// <summary>
        /// Support flags for this IAudioPlayer
        /// </summary>
        AudioPlayerCapabilities Capabilities { get; }

        /// <summary>
        /// Whether to upmix items to 5.1 surround sound
        /// </summary>
        bool UpMixToSurround { get; set; }

    }


    /// <summary>
    /// Support flags for the IAudioPlayer Interface
    /// </summary>
    [Flags]
    public enum AudioPlayerCapabilities {
        Upmix   = 0x00000001
    }


}
