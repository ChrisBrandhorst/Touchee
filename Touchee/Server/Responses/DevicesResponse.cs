using System.Collections.Generic;
using Touchee.Devices;

namespace Touchee.Server.Responses {

    /// <summary>
    /// Devices response object
    /// </summary>
    public class DevicesResponse : List<Device> {

        public DevicesResponse() {
            this.AddRange(Device.All());
        }

    }

}