﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Touchee.Media;

namespace Touchee {


    /// <summary>
    /// String constants for container content type
    /// </summary>
    public class ContainerContentType {
        public const string Unknown = "unknown";
        public const string Music = "music";
        public const string Video = "video";
        public const string Pictures = "pictures";
    }



    /// <remarks>
    /// A container element
    /// </remarks>
    public abstract class Container : Collectable<Container>, IContainer, IComparable {

        /// <summary>
        /// The name of the container
        /// </summary>
        [DataMember]
        public virtual string Name { get; protected set; }
        
        /// <summary>
        /// Whether this container is busy loading
        /// </summary>
        [DataMember]
        public virtual bool Loading { get; set; }

        /// <summary>
        /// The order number to be used for sorting the containers in the frontend.
        /// If this value is -1, the container is sorted by its name.
        /// </summary>
        public virtual int Order { get { return -1; } }

        /// <summary>
        /// The Medium this container belongs to
        /// </summary>
        public virtual Medium Medium { get; protected set; }

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
        }

        /// <summary>
        /// Compares this container to another, for sorting
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>A negative value if this object is 'smaller' then the given, 0 if it is equal and 1 otherwise</returns>
        public virtual int CompareTo(object obj) {
            var other = (Container)obj;
            
            // Same object or ID? Return 0
            if (this == other || this.Id == other.Id) return 0;

            // Compare by name or order attribute
            int result;
            if (other.Order == -1 && this.Order == -1)
                result = this.Name.CompareTo(((Container)obj).Name);
            else if (other.Order == -1)
                return -1;
            else if (this.Order == -1)
                return 1;
            else
                result = this.Order.CompareTo(other.Order);
            
            // Still equal? Compare by ID
            if (result == 0)
                result = this.Id.CompareTo(other.Id);

            return result;
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
