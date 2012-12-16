using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;

using Touchee.Media;

namespace Touchee {


    /// <remarks>
    /// Respresent some media, e.g. disc, usb stick
    /// </remarks>
    public abstract class Medium : Collectable<Medium> {

        /// <summary>
        /// The name of this medium
        /// </summary>
        [DataMember]
        public virtual string Name { get; protected set; }
        
        /// <summary>
        /// Text representation of the type of this medium
        /// </summary>
        [DataMember]
        public string Type { get {
            return this.GetType() == typeof(Medium) ? "unknown" : Regex.Replace(this.GetType().Name, "Medium$", "").ToUnderscore();
        } }

        /// <summary>
        /// Constructs a new medium
        /// </summary>
        /// <param name="name">The name of the medium</param>
        public Medium(string name) : base() {
            Name = name;
        }

        /// <summary>
        /// Gets all containers for this medium
        /// </summary>
        public SortedSet<Container> Containers { get {
            var mediumContainers = Container.Where(c => c.Medium == this);
            return new SortedSet<Container>( mediumContainers );
        } }

        /// <summary>
        /// The local medium
        /// </summary>
        public static Medium Local { get { return LocalMedium.Instance; } }

        /// <summary>
        /// The web medium
        /// </summary>
        public static Medium Web { get { return WebMedium.Instance; } }

    }



    /// <summary>
    /// The local medium
    /// </summary>
    public class LocalMedium : Medium {

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="name">The name of this medium</param>
        LocalMedium(string name) : base(name) { }

        /// <summary>
        /// The local medium singleton instance
        /// </summary>
        public readonly static LocalMedium Instance = new LocalMedium("Local");

        /// <summary>
        /// Instantiates the local medium singleton
        /// </summary>
        /// <param name="name">The name of the local medium</param>
        public static void Init(string name) {
            Instance.Name = name;
            Instance.Save();
        }

    }



    /// <summary>
    /// The web medium
    /// </summary>
    public class WebMedium : Medium {

        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="name">The name of this medium</param>
        WebMedium(string name) : base(name) { }

        /// <summary>
        /// The local web singleton instance
        /// </summary>
        public readonly static WebMedium Instance = new WebMedium("Web");

        /// <summary>
        /// Instantiates the web medium singleton
        /// </summary>
        /// <param name="name">The name of the web medium</param>
        public static void Init(string name) {
            Instance.Name = name;
            Instance.Save();
        }

    }



    /// <remarks>
    /// A medium based on a physical drive
    /// </remarks>
    public class DriveMedium : Medium {

        /// <summary>
        /// Gets the drive info belonging to this medium
        /// </summary>
        public DriveInfo DriveInfo { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="driveInfo">The drive info for this medium</param>
        public DriveMedium(DriveInfo driveInfo) : base(driveInfo.VolumeLabel.ToTitleCase()) {
            DriveInfo = driveInfo;
        }

        /// <summary>
        /// Checks whether the DriveInfo of this medium corresponds with the given DriveInfo
        /// </summary>
        /// <param name="driveInfo">The DriveInfo to check</param>
        /// <returns>True if equal, otherwise false</returns>
        public bool Is(DriveInfo driveInfo, Type type) {
            return this.GetType() == type && this.DriveInfo.Name == driveInfo.Name;
        }

    }



    /// <summary>
    /// Audio CD Medium
    /// </summary>
    public class AudioDiscMedium : DriveMedium {
        public AudioDiscMedium(DriveInfo driveInfo) : base(driveInfo) { }
    }



    /// <summary>
    /// DVD Video Medium
    /// </summary>
    public class DVDVideoMedium : DriveMedium {
        public DVDVideoMedium(DriveInfo driveInfo) : base(driveInfo) { }
    }



    /// <summary>
    /// BluRay video Medium
    /// </summary>
    public class BlurayVideoMedium : DriveMedium {
        public BlurayVideoMedium(DriveInfo driveInfo) : base(driveInfo) { }
    }



    /// <summary>
    /// Generic file storage Medium
    /// </summary>
    public abstract class FileStorageMedium : DriveMedium {
        public FileStorageMedium(DriveInfo driveInfo) : base(driveInfo) { }
    }


    /// <summary>
    /// File disk Medium
    /// </summary>
    public class DataDiscMedium : FileStorageMedium {
        public DataDiscMedium(DriveInfo driveInfo) : base(driveInfo) { }
    }



    /// <summary>
    /// Removable drive Medium
    /// </summary>
    public class RemoveableDriveMedium : FileStorageMedium {
        public RemoveableDriveMedium(DriveInfo driveInfo) : base(driveInfo) { }
    }


}
