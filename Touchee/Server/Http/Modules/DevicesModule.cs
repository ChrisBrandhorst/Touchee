using System;
using Nancy;
using Touchee.Server.Responses;
using Touchee.Devices;

namespace Touchee.Server.Http.Modules {

    /// <summary>
    /// Devices module
    /// </summary>
    public class DevicesModule : ToucheeNancyModule {

        public DevicesModule() : base("/devices") {
            
            Get["/"] = _ => GetDevices();

            Put["/{deviceID}/{command}"] = _ => ExecuteCommand(_);

        }


        /// <summary>
        /// Gets the current devices
        /// </summary>
        public Response GetDevices() {
            return Response.AsJson(
                new DevicesResponse()
            );
        }


        /// <summary>
        /// Executes a command on a device
        /// </summary>
        /// <param name="parameters"></param>
        public Response ExecuteCommand(dynamic parameters) {

            // No command given or non-existing device ID
            if (!parameters.ContainsKey("command") || !Device.Exists(parameters["deviceID"]))
                return new NotFoundResponse();

            // Get device
            var device = Device.Find((int)parameters["deviceID"]);
            
            // Process different commands
            string command = parameters["command"];
            try {
                switch (command) {

                    case "active":
                        if (Request.Form.ContainsKey("value"))
                            device.Active = (bool)Request.Form["value"];
                        else
                            device.ToggleActive();
                        break;

                    case "muted":
                        if (Request.Form.ContainsKey("value"))
                            device.Muted = (bool)Request.Form["value"];
                        else
                            device.ToggleMuted();
                        break;

                    case "lfe_volume":
                        device.LFEVolume = (int)Request.Form["value"];
                        break;
                    
                    case "volume":
                        device.Volume = (int)Request.Form["value"];
                        break;

                }
                return null;
            }

            // Invalid command
            catch (DeviceCapabilityNotSupportedException) {
                return new ConflictResponse();
            }
            // Other errors
            catch (Exception) {
                return new BadRequestResponse();
            }

        }


    }

}
