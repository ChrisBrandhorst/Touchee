﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Touchee;
using Touchee.Components.Media;
using Spotify.Media;

namespace Spotify {

    public class Watcher : IMediumWatcher, IMediaWatcher {


        #region Privates

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Watcher() { }

        #endregion




        #region IMediaWatcher implementation

        public void Watch(int interval) {
            SpotifyMedium.Instance.Save();
        }

        public List<Medium> Media {
            get { return new List<Medium>() { SpotifyMedium.Instance }; }
        }

        #endregion



        #region IMediumWatcher implementation


        /// <summary>
        /// Check whether this watcher can watch the given medium
        /// </summary>
        /// <param name="medium">The medium to check</param>
        public bool CanWatch(Medium medium) {
            //return medium == Medium.Local;
            return medium == SpotifyMedium.Instance;
        }


        /// <summary>
        /// Start watching the given medium. Only if a medium with type Local or FileStorage
        /// is given, is it going to be watched.
        /// </summary>
        /// <param name="medium">The Medium to watch</param>
        public bool Watch(Medium medium) {
            //if (medium == Medium.Local && _localMedium == null) {
            //    _localMedium = medium;
            if (medium == SpotifyMedium.Instance) {
                if (this.StartedWatching != null)
                    this.StartedWatching.Invoke(this, medium);

                SpotifyMedium.Instance.MasterPlaylist.Save();

                return true;
            }
            else
                return false;
        }


        /// <summary>
        /// Stop watching the given medium
        /// </summary>
        /// <param name="medium">The medium to stop watching</param>
        public bool UnWatch(Medium medium) {
            if (SpotifyMedium.Instance == medium) {
                // TODO: clear containers
                // For now, we can assume this call is never made, since the local
                // medium will never be ejected
                return true;
            }
            return false;
        }


        /// <summary>
        /// Stops watching all media
        /// </summary>
        public bool UnWatchAll() {
            return this.UnWatch(SpotifyMedium.Instance);
        }


        #endregion


        #region Events


        /// <summary>
        /// Called when the watcher starts to watch a medium.
        /// </summary>
        public event MediumWatcherStartedWatching StartedWatching;


        #endregion

    }


}
