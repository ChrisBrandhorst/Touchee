using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Devices {

    /// <summary>
    /// Airplay Device
    /// </summary>
    public class AirplayDevice : Device {


        #region Constructor

        /// <summary>
        /// Constructs a new Airplay device instance
        /// </summary>
        /// <param name="name">The name of the device</param>
        public AirplayDevice(string name) : base("airplay", name, DeviceCapabilities.Volume | DeviceCapabilities.ToggleMuted) { }

        #endregion



        #region Commands


        /// <summary>
        /// Gets or sets the volume for this Airplay device
        /// </summary>
        protected override int DoVolume {
            get {
                return base.DoVolume;
            }
            set {
                base.DoVolume = value;
            }
        }


        /// <summary>
        /// Gets or sets the muted state for this Airplay device
        /// </summary>
        protected override bool DoMuted {
            get {
                return base.DoMuted;
            }
            set {
                base.DoMuted = value;
            }
        }


        #endregion


    }

}
