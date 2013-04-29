using System;
using System.Collections.Generic;
using System.Linq;

namespace Touchee.Devices {


    /// <summary>
    /// Classes for devices coupled to Touchee
    /// </summary>
    public class Device : Collectable<Device> {


        #region Statics


        /// <summary>
        /// Default Master Volume device
        /// </summary>
        public static MasterVolume MasterVolume = MasterVolume.Device;


        #endregion



        #region Privates


        /// <summary>
        /// The list of WinLirc commands for this device
        /// </summary>
        Dictionary<WinLircCommand, Tuple<string, string>> _winLircCommands = new Dictionary<WinLircCommand, Tuple<string, string>>();


        #endregion



        #region Properties


        /// <summary>
        /// The name for this device
        /// </summary>
        public string Name { get; protected set; }


        /// <summary>
        /// The capabilities of this device 
        /// </summary>
        public DeviceCapabilities Capabilities { get; protected set; }


        /// <summary>
        /// Whether this device supports control through WinLirc
        /// </summary>
        public bool SupportsControlThroughWinLirc { get { return this.Supports(DeviceCapabilities.WinLirc); } }


        #endregion



        #region Constructors


        /// <summary>
        /// Constructs a new Device instance
        /// </summary>
        public Device(string name, DeviceCapabilities capabilities) {
            this.Name = name;
            this.Capabilities = capabilities;
        }


        #endregion



        #region Support & WinLirc


        /// <summary>
        /// Checks for support for a capability
        /// </summary>
        /// <param name="capability">The DeviceCapabilties flag(s) to check</param>
        /// <returns>True if this device supports the given capabilit(y/ies), otherwise false</returns>
        public bool Supports(DeviceCapabilities capability) {
            return this.Capabilities.HasFlag(capability);       
        }


        /// <summary>
        /// Checks for support of a WinLirc command
        /// </summary>
        /// <param name="command">The WinLircCommand to check</param>
        /// <returns>True if this device supports the given WinLirc command, otherwise false</returns>
        public bool Supports(WinLircCommand command) {
            return this.SupportsControlThroughWinLirc && _winLircCommands.ContainsKey(command);
        }


        /// <summary>
        /// Send a WinLirc command
        /// </summary>
        /// <param name="command">The WinLirc command to send</param>
        /// <exception cref="WinLircCommandNotSupportedException">If the given command is not supported by the device</exception>
        public void SendWinLircCommand(WinLircCommand command) {
            if (!Supports(command))
                throw new WinLircCommandNotSupportedException(command);
            else {
                var remoteAndCommand = _winLircCommands[command];
                this.SendWinLircCommand(remoteAndCommand.Item1, remoteAndCommand.Item2);
            }
        }


        /// <summary>
        /// Sends a command through WinLirc. A random Remote ID is chosen from the available commands
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <exception cref="WinLircCommandNotSupportedException">If no remote ID is found</exception>
        public void SendWinLircCommand(string command) {
            if (_winLircCommands.Values.Count() == 0)
                throw new WinLircCommandNotSupportedException("No remote ID found for device");
            else
                this.SendWinLircCommand(_winLircCommands.Values.First().Item1, command);
        }


        /// <summary>
        /// Sends a command through WinLirc
        /// </summary>
        /// <param name="remote">The remote ID to use</param>
        /// <param name="command">The command to send</param>
        public void SendWinLircCommand(string remote, string command) {
            WinLirc.Client.SendOnce(remote, command);
        }


        #endregion



        #region Commands


        /// <summary>
        /// Toggles the device On or Off
        /// </summary>
        /// <exception cref="DeviceCapabilityNotSupportedException">If this device does not support the Toggle capability</exception>
        public void Toggle() {
            if (this.Capabilities.HasFlag(DeviceCapabilities.Toggle))
                this.DoToggle();
            else
                throw new DeviceCapabilityNotSupportedException(DeviceCapabilities.Toggle);
        }


        /// <summary>
        /// The actual implementation of the Toggle command
        /// </summary>
        /// <exception cref="NotImplementedException">If the Toggle WinLirc command is not supported</exception>
        protected virtual void DoToggle() {
            if (this.Supports(WinLircCommand.Toggle))
                this.SendWinLircCommand(WinLircCommand.Toggle);
            else
                throw new NotImplementedException("Device has DeviceControlSupport.Toggle flag, but no WinLirc command support or DoToggle implementation");
        }


        /// <summary>
        /// Turns the device on
        /// </summary>
        /// <exception cref="DeviceCapabilityNotSupportedException">If this device does not support the On capability</exception>
        public void On() {
            if (this.Capabilities.HasFlag(DeviceCapabilities.OnOff))
                this.DoOn();
            else
                throw new DeviceCapabilityNotSupportedException(DeviceCapabilities.OnOff);
        }


        /// <summary>
        /// The actual implementation of the On command
        /// </summary>
        /// <exception cref="NotImplementedException">If the On WinLirc command is not supported</exception>
        protected virtual void DoOn() {
            if (this.Supports(WinLircCommand.On))
                this.SendWinLircCommand(WinLircCommand.On);
            else
                throw new NotImplementedException("Device has DeviceControlSupport.OnOff flag, but no WinLirc command support or DoOn implementation");
        }


        /// <summary>
        /// Turns the device off
        /// </summary>
        /// <exception cref="DeviceCapabilityNotSupportedException">If this device does not support the Off capability</exception>
        public void Of() {
            if (this.Capabilities.HasFlag(DeviceCapabilities.OnOff))
                this.DoOff();
            else
                throw new DeviceCapabilityNotSupportedException(DeviceCapabilities.OnOff);
        }


        /// <summary>
        /// The actual implementation of the Off command
        /// </summary>
        /// <exception cref="NotImplementedException">If the Off WinLirc command is not supported</exception>
        protected virtual void DoOff() {
            if (this.Supports(WinLircCommand.Off))
                this.SendWinLircCommand(WinLircCommand.Off);
            else
                throw new NotImplementedException("Device has DeviceControlSupport.OnOff flag, but no WinLirc command support or DoOff implementation");
        }


        /// <summary>
        /// Toggles mute for the device On or Off
        /// </summary>
        /// <exception cref="DeviceCapabilityNotSupportedException">If this device does not support the MuteToggle capability</exception>
        public void MuteToggle() {
            if (this.Capabilities.HasFlag(DeviceCapabilities.MuteToggle))
                this.DoMuteToggle();
            else
                throw new DeviceCapabilityNotSupportedException(DeviceCapabilities.MuteToggle);
        }


        /// <summary>
        /// The actual implementation of the MuteToggle command
        /// </summary>
        /// <exception cref="NotImplementedException">If the MuteToggle WinLirc command is not supported</exception>
        protected virtual void DoMuteToggle() {
            if (this.Supports(WinLircCommand.MuteToggle))
                this.SendWinLircCommand(WinLircCommand.MuteToggle);
            else
                throw new NotImplementedException("Device has DeviceControlSupport.MuteToggle flag, but no WinLirc command support or DoMuteToggle implementation");
        }


        /// <summary>
        /// Gets or sets the volume for this device
        /// </summary>
        /// <exception cref="DeviceCapabilityNotSupportedException">If this device does not support the Volume capability</exception>
        public int Volume {
            get {
                if (this.Capabilities.HasFlag(DeviceCapabilities.Volume))
                    return this.DoVolume;
                else
                    throw new DeviceCapabilityNotSupportedException(DeviceCapabilities.Volume);
            }
            set {
                if (this.Capabilities.HasFlag(DeviceCapabilities.Volume))
                    this.DoVolume = value;
                else
                    throw new DeviceCapabilityNotSupportedException(DeviceCapabilities.Volume);
            }
        }


        /// <summary>
        /// The actual implementation of the Volume command
        /// </summary>
        /// <exception cref="NotImplementedException">If no implementation is provided in a subclass</exception>
        protected virtual int DoVolume {
            get {
                throw new NotImplementedException("Device has DeviceControlSupport.Volume flag, but no DoVolume get implementation");
            }
            set {
                throw new NotImplementedException("Device has DeviceControlSupport.Volume flag, but no DoVolume set implementation");
            }
        }


        /// <summary>
        /// Gets or sets mute for this device
        /// </summary>
        /// <exception cref="DeviceCapabilityNotSupportedException">If this device does not support the MuteOnOff capability</exception>
        public bool Muted {
            get {
                if (this.Capabilities.HasFlag(DeviceCapabilities.MuteOnOff))
                    return this.DoMuted;
                else
                    throw new DeviceCapabilityNotSupportedException(DeviceCapabilities.MuteOnOff);
            }
            set {
                if (this.Capabilities.HasFlag(DeviceCapabilities.MuteOnOff))
                    this.DoMuted = value;
                else
                    throw new DeviceCapabilityNotSupportedException(DeviceCapabilities.MuteOnOff);
            }
        }


        /// <summary>
        /// The actual implementation of the Mute command
        /// </summary>
        /// <exception cref="NotImplementedException">If no implementation is provided in a subclass</exception>
        protected virtual bool DoMuted {
            get {
                throw new NotImplementedException("Device has DeviceControlSupport.MuteOnOff flag, but no DoMuted get implementation");
            }
            set {
                throw new NotImplementedException("Device has DeviceControlSupport.MuteOnOff flag, but no DoMuted set implementation");
            }
        }


        #endregion



    }


    public class Speaker : Device {
        public Speaker(string name, DeviceCapabilities capabilities) : base(name, capabilities) { }
    }

    public class AirplayDevice : Device {
        public AirplayDevice(string name, DeviceCapabilities capabilities) : base(name, capabilities) { }
    }



    public class DeviceCapabilityNotSupportedException : Exception {
        public DeviceCapabilities Capability;
        public DeviceCapabilityNotSupportedException(DeviceCapabilities capability) : base() {
            Capability = capability;
        }
        public DeviceCapabilityNotSupportedException(DeviceCapabilities capability, string message) : base(message) {
            Capability = capability;
        }
    }


    public class WinLircCommandNotSupportedException : Exception {
        public WinLircCommand Command;
        public WinLircCommandNotSupportedException(string message) : base(message) { }
        public WinLircCommandNotSupportedException(WinLircCommand command) : base() {
            Command = command;
        }
        public WinLircCommandNotSupportedException(WinLircCommand command, string message) : base(message) {
            Command = command;
        }
    }


    [Flags]
    public enum DeviceCapabilities {
        Toggle      = 0x00000001,
        OnOff       = 0x00000002,
        AutoOnOff   = 0x00000004,
        Volume      = 0x00000008,
        MuteToggle  = 0x00000010,
        MuteOnOff   = 0x00000020,
        WinLirc     = 0x00000040
    }


    public enum WinLircCommand {
        Toggle,
        On,
        Off,
        MuteToggle
    }




    //    /// <summary>
    //    /// Build a Device object
    //    /// </summary>
    //    /// <param name="deviceConfig">The configuration the device object should be built from</param>
    //    /// <returns>The created Device, or null if none could be created</returns>
    //    public static Device Build(dynamic deviceConfig) {
    //        Device device = null;

    //        try {
    //            // Start device and set name
    //            device = new Device();
    //            deviceConfig.TryGetString("name", device.Name);
            
    //            // Set type
    //            device.Type = Enum.Parse(typeof(DeviceType), deviceConfig["type"].ToCamelCase());

    //            // Check capabilties
    //            foreach (var capability in "volume mute auto remote".Split(' ')) {
    //                if (deviceConfig.ContainsKey(capability))
    //                    device.Support = device.Support | (DeviceSupport)Enum.Parse(typeof(DeviceSupport), capability.ToCamelCase());
    //            }
                

    //            // Save the device, giving it an ID and making sure it can be found later on
    //            device.Save();
    //        }

    //        catch(Exception) {
    //            Logger.Log("Device config could not be parsed", Logger.LogLevel.Error);
    //        }

    //        return device;
    //    }

}
