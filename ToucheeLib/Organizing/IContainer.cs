using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Touchee {

    /// <remarks>
    /// Interface for a container object.
    /// </remarks>
    public interface IContainer {

        /// <summary>
        /// The name of the container
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The order number to be used for sorting the containers in the frontend
        /// </summary>
        int Order { get; }

        /// <summary>
        /// String array containing names of views by which the contents can be viewed
        /// The first view should be the default one
        /// </summary>
        string[] ViewTypes { get; }

        /// <summary>
        /// The Medium this container belongs to
        /// </summary>
        Medium Medium { get; }

        /// <summary>
        /// Returns the item with the given item ID
        /// </summary>
        /// <param name="itemID">The ID of the item to return</param>
        /// <returns>The item with the given ID, or null if it does not exist</returns>
        IItem GetItem(int itemID);

    }

}
