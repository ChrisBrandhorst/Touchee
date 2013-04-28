using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee {

    public interface ITimedItem {

        /// <summary>
        /// The duration of this item
        /// </summary>
        TimeSpan Duration { get; }

    }

}
