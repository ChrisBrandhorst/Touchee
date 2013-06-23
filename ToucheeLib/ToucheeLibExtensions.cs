using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee {
    
    public static class ToucheeLibExtensions {

        /// <summary>
        /// Returns the first Container for this Medium for which the property IsMaster returns true.
        /// </summary>
        public static Container GetMasterContainer(this Medium medium) {
            return medium.Containers.First(c => c.IsMaster);
        }

    }

}
