using System;

namespace Touchee.Components.Playback {


    /// <remarks>
    /// Specifies a player which (also) emits audio
    /// </remarks>
    public interface IAudioPlayer : IPlayer {

        /// <summary>
        /// Support flags for this IAudioPlayer
        /// </summary>
        AudioPlayerSupport Support { get; }

        /// <summary>
        /// The volume of the LFE channel between and including 0 and 1
        /// </summary>
        double LFEVolume { get; set; }

        /// <summary>
        /// Whether to upmix items to 5.1 surround sound
        /// </summary>
        bool UpMixToSurround { get; set; }

    }


    /// <summary>
    /// Support flags for the IAudioPlayer Interface
    /// </summary>
    [Flags]
    public enum AudioPlayerSupport {
        LFE     = 0x00000001,
        Upmix   = 0x00000002
    }


}
