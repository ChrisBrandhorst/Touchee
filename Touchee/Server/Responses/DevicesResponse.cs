using System;
using System.Collections.Generic;
using System.Linq;
using Touchee.Devices;

namespace Touchee.Server.Responses {

    /// <summary>
    /// Devices response object
    /// </summary>
    public class DevicesResponse : ToucheeResponse {

        /// <summary>
        /// The devices
        /// </summary>
        public IEnumerable<Device> Items;

        /// <summary>
        /// Constructor
        /// </summary>
        public DevicesResponse() {
            this.Items = Device.All();
        }

    }

}