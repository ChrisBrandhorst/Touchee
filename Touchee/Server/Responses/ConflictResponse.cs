using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Server.Responses {
    
    public class ConflictResponse : Nancy.Response {

        public ConflictResponse() {
            this.StatusCode = Nancy.HttpStatusCode.Conflict;
        }

    }

}
