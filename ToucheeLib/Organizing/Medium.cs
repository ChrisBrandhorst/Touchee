using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Touchee.Media;

namespace Touchee {

    /// <summary>
    /// String constants for medium type
    /// </summary>
    public class MediumType {
        public const string Unknown = "unknown";
        public const string Local = "local";
        public const string Web = "web";
        public const string AudioCD = "audio_cd";
        public const string DVDVideo = "dvd_video";
        public const string BluRayVideo = "bluray_video";
        public const string FileStorage = "file_storage";
    }


    /// <remarks>
    /// Respresent some media, e.g. disc, usb stick
    /// </remarks>
    public class Medium : Collectable<Medium> {

        public string Name { get; protected set; }
        public string Type { get; protected set; }
        public bool Ready { get; set; }
        
        public Medium(string name, string type) : base() {
            if (type == MediumType.Local && Local != null)
                throw new ArgumentException("Only one local medium can exist!");
            Name = name;
            Type = type;
        }

        public SortedSet<Container> Containers { get {
            var mediumContainers = Container.Where(c => c.Medium == this);
            return new SortedSet<Container>( mediumContainers );
        } }

        public static Medium Local { get { return FirstOrDefault(m => m.Type == MediumType.Local); } }

    }


    /// <remarks>
    /// A medium based on a drive
    /// </remarks>
    public class DriveMedium : Medium {

        public DriveInfo DriveInfo { get; protected set; }

        public DriveMedium(DriveInfo driveInfo, string type)
            : base(driveInfo.VolumeLabel.ToTitleCase(), type) {
            DriveInfo = driveInfo;
        }

        public bool Is(DriveInfo driveInfo, string type) {
            return this.Type == type && this.DriveInfo.Name == driveInfo.Name;
        }

    }


}
