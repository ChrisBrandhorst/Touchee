using System;
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


        /// <summary>
        /// The LFE volume of the system
        /// </summary>
        float _lfeVolume = 1F;

        #endregion



        #region Constants

        /// <summary>
        /// The maximum LFE Volume
        /// </summary>
        public const float MAX_LFE_VOLUME = 2F;

        #endregion



        #region Singleton constructor

        /// <summary>
        /// Private constructor
        /// </summary>
        MasterVolume() : base("master", "Master Volume", DeviceCapabilities.Volume | DeviceCapabilities.SetMuted | DeviceCapabilities.GetMuted | DeviceCapabilities.LFEVolume) {
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
        /// Gets or sets the master LFE volume
        /// </summary>
        protected override float DoLFEVolume {
            get {
                return _lfeVolume;
            }
            set {
                _lfeVolume = Math.Min(Math.Max(0F, value), MAX_LFE_VOLUME);
                this.Save();
            }
        }


        #endregion



        #region Events


        /// <summary>
        /// Called when the master volume is changed
        /// </summary>
        void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data) {
            this.Save();
        }

        #endregion


    }

}
