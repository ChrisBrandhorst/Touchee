using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Touchee;

namespace ListenLive {

    /// <remarks>
    /// Represents a container of radio channels
    /// </remarks>
    public class RadioChannels : Container {


        #region Properties

        /// <summary>
        /// Whether artwork should be retrieved for this container
        /// </summary>
        public bool WithArtwork { get; protected set; }


        /// <summary>
        /// The channels in this container
        /// </summary>
        public IEnumerable<RadioChannel> Channels { get; protected set; }

        #endregion


        public override IEnumerable<IItem> Items {
            get { return this.Channels.Cast<IItem>(); }
        }


        #region Overridden properties from Container

        /// <summary>
        /// The type of the container, e.g. what the container 'looks like'
        /// </summary>
        public override string Type { get { return ContainerType.Radio; } }

        /// <summary>
        /// The content type of the container, e.g. what kind of items reside inside this container
        /// </summary>
        public override string ContentType { get { return ContainerContentType.Music; } }

        /// <summary>
        /// String array containing names of views by which the contents can be viewed
        /// </summary>
        public override string[] ViewTypes {
            get {
                return WithArtwork
                    ? new string[] { Types.Channel, Types.Genre }
                    : new string[] { Types.Genre };

            }
        }

        #endregion


        #region Constructors

        /// <summary>
        /// Constructs a new radio channel
        /// </summary>
        /// <param name="name">The name of the channel</param>
        /// <param name="medium">The medium this channel came from</param>
        /// <param name="withArtwork">Whether artwork should be retrieved for this container</param>
        public RadioChannels(string name, Medium medium, bool withArtwork) : base(name, medium) {
            this.WithArtwork = withArtwork;
            this.Channels = new SortedSet<RadioChannel>();
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Returns the item with the given item ID
        /// </summary>
        /// <param name="itemID">The ID of the item to return</param>
        /// <returns>The item with the given ID, or null if it does not exist</returns>
        public override IItem GetItem(int itemID) {
            return this.Channels.FirstOrDefault(t => ((RadioChannel)t).Id == itemID) as IItem;
        }


        /// <summary>
        /// Updates the list of channels with the given collection
        /// </summary>
        /// <param name="channels">The new set of channels</param>
        public void Update(IEnumerable<RadioChannel> channels) {
            this.Channels = new SortedSet<RadioChannel>(channels);
        }

        #endregion


    }

}
