using System;
using Nancy;
using Touchee.Server.Responses;

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

            return null;
        }


    }

}
