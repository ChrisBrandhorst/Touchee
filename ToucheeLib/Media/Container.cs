using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Touchee.Media;

namespace Touchee {


    /// <remarks>
    /// A container element
    /// </remarks>
    public abstract class Container : Collectable<Container>, IComparable<Container> {

        /// <summary>
        /// The name of the container
        /// </summary>
        [DataMember]
        public virtual string Name { get; protected set; }

        /// <summary>
        /// The Medium this container belongs to
        /// </summary>
        public virtual Medium Medium { get; protected set; }

        /// <summary>
        /// Whether this container is busy loading
        /// </summary>
        [DataMember]
        int MediumID { get { return Medium.Id; } }

        /// <summary>
        /// Whether this container is busy loading
        /// </summary>
        [DataMember]
        public virtual bool IsLoading { get; set; }

        /// <summary>
        /// Whether this container is empty
        /// </summary>
        [DataMember]
        public virtual bool IsEmpty { get { return false; } }

        /// <summary>
        /// Whether this container is a master container
        /// </summary>
        [DataMember]
        public virtual bool IsMaster { get { return false; } }

        ///// <summary>
        ///// The collection of items within this container
        ///// </summary>
        //public abstract IEnumerable<IItem> Items { get; }

        ///// <summary>
        ///// Returns the item with the given item ID
        ///// </summary>
        ///// <param name="itemID">The ID of the item to return</param>
        ///// <returns>The item with the given ID, or null if it does not exist</returns>
        //public virtual IItem GetItem(int itemID) { return null; }

        /// <summary>
        /// The type of the container, e.g. what the container 'looks like'
        /// </summary>
        [DataMember]
        public abstract string Type { get; }

        /// <summary>
        /// The content type of the container, e.g. what kind of items reside inside this container
        /// </summary>
        [DataMember]
        public abstract string ContentType { get; }

        /// <summary>
        /// String array containing names of views by which the contents can be viewed
        /// </summary>
        [DataMember]
        public abstract string[] ViewTypes { get; }

        /// <summary>
        /// The name of the plugin this Container resides in
        /// </summary>
        [DataMember]
        public string Plugin { get { return this.GetType().Assembly.GetName().Name.ToUnderscore(); } }

        /// <summary>
        /// Constructs a new container instance
        /// </summary>
        /// <param name="name">The name of the container</param>
        /// <param name="medium">The medium the container belongs to</param>
        public Container(string name, Medium medium) {
            this.Name = name;
            this.Medium = medium;

            _contentsChangedDebouncer = new Debouncer(this.OnContentsChanged, new TimeSpan(0, 0, 1));
        }


        # region Contents change callback

        /// <summary>
        /// Internal content change debouncer
        /// </summary>
        Debouncer _contentsChangedDebouncer;

        /// <summary>
        /// Should be called whenever the contents of the container are changed
        /// </summary>
        protected void NotifyContentsChanged() {
            _contentsChangedDebouncer.Call();
        }

        /// <summary>
        /// Invokes the content changed event and saves the container
        /// </summary>
        void OnContentsChanged() {
            if (Container.ContentsChanged != null)
                Container.ContentsChanged.Invoke(this);
        }

        public delegate void ContentsChangedEventHandler(Container container);
        public static event ContentsChangedEventHandler ContentsChanged;

        #endregion


        public virtual int CompareTo(Container other) {
            int ret;
            if (this.Name == null && other.Name == null)
                ret = 0;
            else if (this.Name == null)
                ret = 1;
            else if (other.Name == null)
                ret = -1;
            else
                ret = Util.ToSortName(this.Name).CompareTo(Util.ToSortName(other.Name));
            return ret;
        }
    }

    

    //public class FilesystemMusicContainer : Container {
    //    public override string Type { get { return ContainerType.Filesystem; } }
    //    public override string ContentType { get { return ContainerContentType.Music; } }
    //    public override string[] ViewTypes { get { return new string[0]; } }
    //    public FilesystemMusicContainer(string name, Medium medium) : base(name, medium) { }
    //}
    //public class FilesystemVideosContainer : Container {
    //    public override string Type { get { return ContainerType.Filesystem; } }
    //    public override string ContentType { get { return ContainerContentType.Video; } }
    //    public override string[] ViewTypes { get { return new string[0]; } }
    //    public FilesystemVideosContainer(string name, Medium medium) : base(name, medium) { }
    //}
    //public class FilesystemPicturesContainer : Container {
    //    public override string Type { get { return ContainerType.Filesystem; } }
    //    public override string ContentType { get { return ContainerContentType.Pictures; } }
    //    public override string[] ViewTypes { get { return new string[0]; } }
    //    public FilesystemPicturesContainer(string name, Medium medium) : base(name, medium) { }
    //}
    //public class DiscMusicContainer : Container {
    //    public override string Type { get { return ContainerType.Disc; } }
    //    public override string ContentType { get { return ContainerContentType.Music; } }
    //    public override string[] ViewTypes { get { return new string[0]; } }
    //    public DiscMusicContainer(string name, Medium medium) : base(name, medium) { }
    //}
    

}
