using CoreAudioApi;

namespace Touchee {

    /// <summary>
    /// 
    /// </summary>
    public static class Volume {

        static MMDevice _defaultDevice;

        /// <summary>
        /// Initializes the volume stuff
        /// </summary>
        public static void Init() {
            var devices = new MMDeviceEnumerator();
            Volume._defaultDevice = devices.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            Volume._defaultDevice.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
        }

        /// <summary>
        /// Gets or sets the master volume of the system
        /// </summary>
        public static int Master {
            get {
                return (int)(Volume._defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }
            set {
                Volume._defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)value / 100.0f);
            }
        }

        /// <summary>
        /// Gets or sets the master mute of the system
        /// </summary>
        public static bool MasterMuted {
            get {
                return Volume._defaultDevice.AudioEndpointVolume.Mute;
            }
            set {
                Volume._defaultDevice.AudioEndpointVolume.Mute = value;
            }
        }

        /// <summary>
        /// Called when the master volume is changed
        /// </summary>
        static void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data) {
            if (Volume.MasterVolumeChanged != null)
                Volume.MasterVolumeChanged.Invoke((int)(data.MasterVolume * 100), data.Muted);
        }


        public static event MasterVolumeChangedHandler MasterVolumeChanged;
        public delegate void MasterVolumeChangedHandler(int volume, bool mute);

    }

}
