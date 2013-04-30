using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Touchee.Devices {


    /// <summary>
    /// Class for devices coupled to Touchee
    /// </summary>
    public class Device : Collectable<Device> {


        #region Statics


        /// <summary>
        /// Default Master Volume device
        /// </summary>
        public static MasterVolume MasterVolume = MasterVolume.Device;


        /// <summary>
        /// Create all devices from the gviven devices config
        /// </summary>
        /// <param name="devicesConfig">The configuration</param>
        /// <returns>A List of Devices which could be parsed</returns>
        public static List<Device> Parse(List<dynamic> devicesConfig) {
            var devices = new List<Device>();

            foreach (var deviceConfig in devicesConfig) {
                try {
                    devices.Add( Device.Parse(deviceConfig) );
                }
                catch (InvalidDeviceConfigException e) {
                    var message = e.Message;
                    if (e.InnerException != null)
                        message += " --- " + e.InnerException.Message;
                    Logger.Log(message, Logger.LogLevel.Error);
                }
            }

            return devices;
        }


        /// <summary>
        /// Create a Device instance from a config
        /// </summary>
        /// <param name="deviceConfig">The configuration of the device</param>
        /// <returns>The parsed device</returns>
        /// <exception cref="InvalidDeviceConfigException">If the configuration is invalid</exception>
        public static Device Parse(dynamic deviceConfig) {
            Device device = null;

            // Collect values from config
            string type = deviceConfig.GetString("type", null);
            string name = deviceConfig.GetString("name", null);
            string[] capabilitiesStrings = deviceConfig.GetStringArray("capabilities");

            // Check if we have a type
            if (type == null) throw new InvalidDeviceConfigException("Invalid Device config: supplying a type is required");
        
            // Find the type
            var fullTypeString = "Touchee.Devices." + type.ToTitleCase() + "Device";
            var deviceType = typeof(Device);
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies()) {
                var t = ass.GetType(fullTypeString);
                if (t != null) {
                    deviceType = t;
                    break;
                }
            }

            // Collect capabilities
            DeviceCapabilities capabilities = 0;
            foreach (var c in capabilitiesStrings) {
                try {
                    capabilities = capabilities | (DeviceCapabilities)Enum.Parse(typeof(DeviceCapabilities), c.ToCamelCase());
                }
                catch (ArgumentException) {
                    Logger.Log("Unknown DeviceCapability: " + c + ". Skipping it", Logger.LogLevel.Error);
                }
            }

            // Construct the device
            try {
                device = (Device)Activator.CreateInstance(deviceType, new object[]{type, name, capabilities});
            }
            catch (Exception e) {
                throw new InvalidDeviceConfigException("The device could not be instantiated. Is there a (string, string, DeviceCapabilities) constructor?", e);
            }

            // Apply config
            try {
                device.ApplyConfigBase(deviceConfig);
                device.ApplyConfig(deviceConfig);
            }
            catch (Exception e) {
                throw new InvalidDeviceConfigException("The configuration for the device " + device.Name + " could not be applied.", e);
            }

            // Save it
            device.Save();

            return device;

        }


        #endregion



        #region Privates


        /// <summary>
        /// The list of WinLirc commands for this device. command->(remote, command)
        /// </summary>
        Dictionary<string, Tuple<string, string>> _winLircCommands = new Dictionary<string, Tuple<string, string>>();


        #endregion



        #region Properties


        /// <summary>
        /// The type of this device
        /// </summary>
        [DataMember]
        public string Type { get; protected set; }


        /// <summary>
        /// The name for this device
        /// </summary>
        [DataMember]
        public string Name { get; protected set; }


        /// <summary>
        /// The capabilities of this device 
        /// </summary>
        public DeviceCapabilities Capabilities { get; protected set; }


        /// <summary>
        /// The capabilities of this device, as seen by the client
        /// </summary>
        [DataMember]
        IEnumerable<string> capabilities {
            get {
                return Enum
                    .GetValues(typeof(DeviceCapabilities))
                    .Cast<Enum>()
                    .Where(f => Capabilities.HasFlag(f))
                    .Select(c => Enum.GetName(typeof(DeviceCapabilities), c).ToCamelCase(false));
            }
        }



        /// <summary>
        /// Whether this device supports control through WinLirc
        /// </summary>
        public bool SupportsControlThroughWinLirc { get { return this.SupportsCapability(DeviceCapabilities.WinLirc); } }


        /// <summary>
        /// The timeout before the device is turned off automatically
        /// </summary>
        public TimeSpan AutoOffTimeout { get; protected set; }


        #endregion



        #region Constructors


        /// <summary>
        /// Constructs a new Device instance
        /// </summary>
        /// <param name="type">The type of the device</param>
        /// <param name="name">The name of the device</param>
        /// <param name="capabilities">The capabilities of the device</param>
        public Device(string type, string name, DeviceCapabilities capabilities) {
            this.Type = type;
            this.Name = name;
            this.Capabilities = capabilities;
        }


        /// <summary>
        /// Base config applying
        /// TODO: cleanup
        /// </summary>
        /// <param name="config">The config to apply</param>
        void ApplyConfigBase(dynamic config) {

            if (this.SupportsControlThroughWinLirc) {

                // Check if config remote exists
                if (!config.ContainsKey("remote"))
                    throw new InvalidDeviceConfigException("Capability winLirc was given, but no remote value supplied.");

                dynamic remoteConfig = config["remote"];
                List<string> keys = remoteConfig.Keys;

                if (keys.Count == 0)
                    throw new InvalidDeviceConfigException("Capability winLirc was given, but no or empty remote value supplied.");
                else {

                    string mainRemoteID = null;
                    if (remoteConfig.ContainsKey("id")) {
                        mainRemoteID = (string)remoteConfig["id"];
                        keys.Remove("id");
                    }

                    foreach (var k in keys) {
                        var command = (string)remoteConfig[k];
                        string remoteID;

                        var remoteAndCommand = command.Split(' ');
                        if (mainRemoteID == null && remoteAndCommand.Count() < 2)
                            throw new InvalidDeviceConfigException("No main remote ID given for device and no remote ID given for the command: " + command);
                        else if (remoteAndCommand.Count() == 2) {
                            remoteID = remoteAndCommand[0];
                            command = remoteAndCommand[1];
                        }
                        else
                            remoteID = mainRemoteID;

                        _winLircCommands.Add(k.ToLower(), new Tuple<string, string>(remoteID, command));

                    }

                }
            }

            if (this.SupportsCapability(DeviceCapabilities.AutoOnOff)) {
                int autoOffTimeout;
                config.TryGetInt("autoOff", out autoOffTimeout);
                if (autoOffTimeout == 0)
                    throw new InvalidDeviceConfigException("Capability autoOnOff was given, but no autoOff value supplied.");
                else
                    this.AutoOffTimeout = new TimeSpan(0, 0, autoOffTimeout);
            }

        }


        /// <summary>
        /// Applies the given config to this device
        /// </summary>
        /// <param name="config">The config to apply</param>
        protected virtual void ApplyConfig(dynamic config) {
        }


        #endregion



        #region Support & WinLirc


        /// <summary>
        /// Checks for support for a capability
        /// </summary>
        /// <param name="capability">The DeviceCapabilties flag(s) to check</param>
        /// <returns>True if this device supports the given capabilit(y/ies), otherwise false</returns>
        public bool SupportsCapability(DeviceCapabilities capability) {
            return this.Capabilities.HasFlag(capability);       
        }


        /// <summary>
        /// Checks for support of a WinLirc command
        /// </summary>
        /// <param name="command">The WinLircCommand to check</param>
        /// <returns>True if this device supports the given WinLirc command, otherwise false</returns>
        public bool SupportsWinLircCommand(WinLircCommand command) {
            return this.SupportsWinLircCommand(
                Enum.GetName(typeof(WinLircCommand), command).ToLower()
            );
        }


        /// <summary>
        /// Checks for support of a WinLirc command
        /// </summary>
        /// <param name="command">The WinLircCommand to check</param>
        /// <returns>True if this device supports the given WinLirc command, otherwise false</returns>
        public bool SupportsWinLircCommand(string command) {
            return this.SupportsControlThroughWinLirc && _winLircCommands.ContainsKey(command);
        }


        /// <summary>
        /// Send a WinLirc command
        /// </summary>
        /// <param name="command">The WinLirc command to send</param>
        /// <exception cref="WinLircCommandNotSupportedException">If the given command is not supported by the device</exception>
        public void SendWinLircCommand(WinLircCommand command) {
            if (!SupportsWinLircCommand(command))
                throw new WinLircCommandNotSupportedException(command);
            else {
                var commandString = Enum.GetName(typeof(WinLircCommand), command).ToLower();
                var remoteAndCommand = _winLircCommands[commandString];
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
            if (this.SupportsWinLircCommand(WinLircCommand.Toggle))
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
            if (this.SupportsWinLircCommand(WinLircCommand.On))
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
            if (this.SupportsWinLircCommand(WinLircCommand.Off))
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
            if (this.SupportsWinLircCommand(WinLircCommand.MuteToggle))
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

    public class InvalidDeviceConfigException : Exception {
        public InvalidDeviceConfigException(string message) : base(message) { }
        public InvalidDeviceConfigException(string message, Exception innerException) : base(message, innerException) { }
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


}
