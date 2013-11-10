using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Server {

    /// <remarks>
    /// Represents a client connected to the server.
    /// </remarks>
    public interface IClient {

        // The HTTP session ID of this client
        string SessionId { get; }

    }
}
