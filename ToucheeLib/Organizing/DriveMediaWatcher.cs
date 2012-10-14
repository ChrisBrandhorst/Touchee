using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using Touchee;

namespace Touchee {

    /// <remarks>
    /// Searches for connected drives and gives notifications when a new drive has been found or when
    /// an existing drive was ejected.
    /// </remarks>
    public class DriveMediaWatcher : Base, IMediaWatcher {

        // The polling interval
        public int Interval { get; protected set; }
        // Timer used for polling
        Timer _timer;
        // The current list of present drive media
        List<DriveMedium> _driveMedia = new List<DriveMedium>();
        // Thread of detection
        Thread _thread;


        /// <summary>
        /// Constructor
        /// </summary>
        public DriveMediaWatcher() {}


        /// <summary>
        /// Start the medium detection in a new thread.
        /// </summary>
        public void Watch(int interval) {
            Interval = Math.Max(interval, 100);
            if (_thread != null) return;
            _thread = new Thread(StartDetecting);
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.IsBackground = true;
            _thread.Start();
        }


        /// <summary>
        /// Start the medium detection
        /// </summary>
        void StartDetecting() {
            FindMedia(null);
        }

        
        /// <summary>
        /// Stop the medium detection.
        /// </summary>
        public void Stop() {
            _thread = null;
            if (_timer != null) {
                _timer.Dispose();
                _timer = null;
            }
        }


        /// <summary>
        /// Checks what media are present and signals listeners when a medium is inserted or removed.
        /// </summary>
        /// <param name="state">Not used</param>
        void FindMedia(object state) {
            DetectDriveMedium();

            // Do it again!
            _timer = new Timer(new TimerCallback(FindMedia), null, this.Interval, Timeout.Infinite);
        }


        /// <summary>
        /// Checks what drives are present and signals listeners when a medium is inserted or removed.
        /// </summary>
        void DetectDriveMedium() {

            // Loop through all current drives
            string[] driveLetters = Environment.GetLogicalDrives();
            var foundDrives = new Dictionary<DriveInfo, string>();
            foreach (var l in driveLetters) {

                // Get drive info for this drive
                var driveInfo = new System.IO.DriveInfo(l);

                // Skip if drive is not ready
                if (!driveInfo.IsReady) continue;

                // Get drive type
                string mediumType = MediumType.Unknown;
                DriveType driveType = DriveType.Unknown;
                try { driveType = driveInfo.DriveType; }
                catch (IOException e) {
                    Log("Cannot get drivetype for drive " + l, e);
                    continue;
                }

                // Switch on type
                switch (driveType) {

                    // We have a disc
                    case DriveType.CDRom:

                        try {
                            if (Directory.GetFiles(l, "*.cda", SearchOption.TopDirectoryOnly).Length > 0) {
                                mediumType = MediumType.AudioCD;
                            }
                            else if (Directory.Exists(l + "VIDEO_TS") && Directory.GetFiles(l + "VIDEO_TS", "*.vob").Length > 0) {
                                mediumType = MediumType.DVDVideo;
                            }
                            else if (File.Exists(l + @"BDMV\index.bdmv")) {
                                mediumType = MediumType.BluRayVideo;
                            }
                            else {
                                mediumType = MediumType.FileStorage;
                            }
                        }
                        catch (Exception e) {
                            // Many things can go wrong
                            Log("Unable to get mediumtype for drive " + l, e);
                            continue;
                        }

                        break;

                    // We have a removable drive (usb stick / hd)
                    case DriveType.Removable:
                        mediumType = MediumType.FileStorage;
                        break;
                }

                // If we support the medium type, store it
                if (mediumType != MediumType.Unknown)
                    foundDrives[driveInfo] = mediumType;

            }


            // Check which drives are new and which are no longer present
            List<DriveMedium> currentDriveMedia = new List<DriveMedium>();
            foreach (var d in foundDrives) {

                // Find medium
                DriveMedium existingDriveMedium = _driveMedia.FirstOrDefault(m => m.Is(d.Key, d.Value));

                // New medium!
                if (existingDriveMedium == null) {
                    DriveMedium newDriveMedium;
                    try { newDriveMedium = new DriveMedium(d.Key, d.Value); }
                    catch (Exception e) {
                        Log("Unable to get drive information for drive " + d.Key.Name, e);
                        continue;
                    }
                    newDriveMedium.Save();
                    currentDriveMedia.Add(newDriveMedium);
                    Log("New drive medium: " + newDriveMedium.Name);

                    

                    // TODO: remove this debugging stuff
                    if (newDriveMedium.Type == MediumType.FileStorage) {
                        new FilesystemMusicContainer("Music Everywhere", newDriveMedium).Save();
                        new FilesystemMusicContainer("More Music", newDriveMedium).Save();
                        new FilesystemVideosContainer("Videos", newDriveMedium).Save();
                        new FilesystemPicturesContainer("Pictures", newDriveMedium).Save();
                    }
                    else if (newDriveMedium.Type == MediumType.AudioCD) {
                        new DiscMusicContainer("Superstrings 2", newDriveMedium).Save();
                    }
                    // /TODO

                }

                // Medium already exists
                else {
                    _driveMedia.Remove(existingDriveMedium);
                    currentDriveMedia.Add(existingDriveMedium);
                }

            }

            // Media that are still present in _media, must have been removed
            foreach (var m in _driveMedia) {
                Log("Removed drive medium: " + m.Name);
                //foreach (var c in m.Containers)
                //    c.Dispose();
                m.Dispose();
            }

            // Set the new current media
            _driveMedia = currentDriveMedia;
        }


        /// <summary>
        /// Returns the currently present media
        /// </summary>
        public List<Medium> Media {
            get {
                return this._driveMedia.Cast<Medium>().ToList();
            }
        }

    }

}
