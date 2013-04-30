using CoreAudioApi;

namespace Touchee.Devices {

    /// <summary>
    /// Master Volume device
    /// </summary>
    public sealed class MasterVolume : Device {


        #region Privates


        /// <summary>
        /// The default (master) device of the system
        /// </summary>
        MMDevice _defaultDevice;


        #endregion



        #region Singleton constructor

        /// <summary>
        /// Private constructor
        /// </summary>
        MasterVolume() : base("master", "Master Volume", DeviceCapabilities.Volume | DeviceCapabilities.MuteOnOff) {
            var devices = new MMDeviceEnumerator();
            _defaultDevice = devices.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            _defaultDevice.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
            this.Save();
        }

        /// <summary>
        /// The singleton instance of the MasterVolume
        /// </summary>
        public static MasterVolume Device = new MasterVolume();

        #endregion



        #region Properties


        /// <summary>
        /// Gets or sets the master system volume
        /// </summary>
        protected override int DoVolume {
            get {
                return (int)(_defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }
            set {
                _defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)value / 100.0f);
            }
        }


        /// <summary>
        /// Gets or sets the master system mute
        /// </summary>
        protected override bool DoMuted {
            get {
                return _defaultDevice.AudioEndpointVolume.Mute;
            }
            set {
                _defaultDevice.AudioEndpointVolume.Mute = value;
            }
        }


        #endregion



        #region Events


        /// <summary>
        /// Called when the master volume is changed
        /// </summary>
        void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data) {
            if (this.MasterVolumeChanged != null)
                this.MasterVolumeChanged.Invoke((int)(data.MasterVolume * 100), data.Muted);
        }

        public event MasterVolumeChangedHandler MasterVolumeChanged;
        public delegate void MasterVolumeChangedHandler(int volume, bool mute);

        #endregion


    }

}
