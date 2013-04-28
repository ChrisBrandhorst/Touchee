using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Components.Devices {


    /// <summary>
    /// 
    /// </summary>
    public class Device : Collectable<Device> {

        /// <summary>
        /// Gets the capabilities for this device
        /// </summary>
        public DeviceSupport Support { get; protected set; }

        /// <summary>
        /// The type 
        /// </summary>
        public DeviceType Type { get; protected set; }

        /// <summary>
        /// Name of the device
        /// </summary>
        public string Name { get; protected set; }


        public int Volume { get; protected set; }
        public bool Muted { get; protected set; }
        public int AutoOffTimeout { get; protected set; }
        


        /// <summary>
        /// Build a Device object
        /// </summary>
        /// <param name="deviceConfig">The configuration the device object should be built from</param>
        /// <returns>The created Device, or null if none could be created</returns>
        public static Device Build(dynamic deviceConfig) {
            Device device = null;

            try {
                // Start device and set name
                device = new Device();
                deviceConfig.TryGetString("name", device.Name);
            
                // Set type
                device.Type = Enum.Parse(typeof(DeviceType), deviceConfig["type"].ToCamelCase());

                // Check capabilties
                foreach (var capability in "volume mute auto remote".Split(' ')) {
                    if (deviceConfig.ContainsKey(capability))
                        device.Support = device.Support | (DeviceSupport)Enum.Parse(typeof(DeviceSupport), capability.ToCamelCase());
                }

                // Save the device, giving it an ID and making sure it can be found later on
                device.Save();
            }

            catch(Exception) {
                Logger.Log("Device config could not be parsed", Logger.LogLevel.Error);
            }

            return device;
        }


        static Device _masterVolume;
        public static Device MasterVolume {
            get {
                if (_masterVolume == null) {
                    _masterVolume = new Device() {
                        Type = DeviceType.MasterVolume,
                        Support = DeviceSupport.Volume | DeviceSupport.Mute
                    };
                    _masterVolume.Save();
                }
                return _masterVolume;
            }
        }


    }


    /// <summary>
    /// Device types
    /// </summary>
    public enum DeviceType {
        MasterVolume,
        Amplifier,
        Speaker,
        Subwoofer,
        Television,
        Airplay
    }



    /// <summary>
    /// Support flags for devices
    /// </summary>
    [Flags]
    public enum DeviceSupport {
        Volume  = 0x00000001,
        Mute    = 0x00000002,
        Auto    = 0x00000004,
        Remote  = 0x00000008
    }


}
