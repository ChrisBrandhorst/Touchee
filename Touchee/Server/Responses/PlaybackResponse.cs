using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Server.Responses {

    /// <summary>
    /// Response for all playback-related data
    /// </summary>
    public class PlaybackResponse : ToucheeResponse {

        public int MasterVolume { get; protected set; }
        public bool MasterMuted { get; protected set; }

        public PlaybackResponse() {
            this.MasterVolume = Volume.Master;
            this.MasterMuted = Volume.MasterMuted;
        }
    }

}
