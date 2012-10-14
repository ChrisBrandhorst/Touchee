using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee {

    /// <remarks>
    /// An item present in some container
    /// </remarks>
    public interface IItem {


        /// <summary>
        /// The ID of the item within the container
        /// </summary>
        int ID { get; }


        ///// <summary>
        ///// The source ID for this item
        ///// </summary>
        //object SourceID { get; }

        
        /// <summary>
        /// The application-wide, unique key string for this item
        /// </summary>
        string UniqueKey { get; }

    }

}
