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
        /// If this container is a sub-container, this property returns
        /// the master container.
        /// </summary>
        public virtual Container Master { get; protected set; }

        /// <summary>
        /// Whether this container is a master container
        /// </summary>
        public virtual bool IsMaster { get { return this.Master == null; } }

        /// <summary>
        /// The ID of the Medium this Container belongs to
        /// </summary>
        [DataMember]
        int MediumID { get { return Medium.Id; } }

        /// <summary>
        /// Whether this container is busy loading
        /// </summary>
        [DataMember]
        public virtual bool Loaded { get; set; }

        /// <summary>
        /// Whether this container is empty
        /// </summary>
        [DataMember]
        public virtual bool IsEmpty { get { return false; } }

        /// <summary>
        /// The ID of the master container when this object is serialized
        /// </summary>
        [DataMember(Name = "MasterID")]
        protected virtual object OutputMasterID { get { return this.IsMaster ? null : this.Master.OutputId; } }

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
        public abstract string[] Views { get; }

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
