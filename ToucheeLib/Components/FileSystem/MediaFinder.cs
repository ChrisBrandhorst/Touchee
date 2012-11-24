using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

//using Touchee;

namespace Touchee.Components.FileSystem {


    /// <summary>
    /// Finds music media in a folder and places it in the appropriate medium
    /// </summary>
    public abstract class MediaFinder {


        #region Privates

        // The directory watcher used internally
        DirectoryWatcher _directoryWatcher;

        #endregion



        #region Properties


        /// <summary>
        /// The medium for this finder
        /// </summary>
        public Medium Medium { get; protected set; }


        /// <summary>
        /// The directory which is watched
        /// </summary>
        public DirectoryInfo Directory { get; protected set; }


        /// <summary>
        /// The collection state of the watcher
        /// </summary>
        public CollectionState CollectionState { get { return _directoryWatcher.CollectionState; } }


        #endregion



        #region Constructor


        /// <summary>
        /// Constructs a new MediaFinder
        /// </summary>
        /// <param name="medium">The medium in which to place new media</param>
        /// <param name="directory">The directory to watch</param>
        /// <param name="collectionRequired">Whether an initial collection is required</param>
        public MediaFinder(Medium medium, DirectoryInfo directory, IEnumerable<string> extensions, bool collectionRequired) {

            // Set properties
            this.Medium = medium;
            this.Directory = directory;

            // Create watcher
            _directoryWatcher = new DirectoryWatcher(directory, extensions);

            // Set events
            _directoryWatcher.CollectingCompleted += directoryWatcher_CollectingCompleted;
            _directoryWatcher.FileCollected += FileCollected;
            _directoryWatcher.FileCreated += FileCreated;
            _directoryWatcher.FileChanged += FileChanged;
            _directoryWatcher.FileRenamed += FileRenamed;
            _directoryWatcher.FileDeleted += FileDeleted;

            // Set collecting requirement
            if (collectionRequired)
                _directoryWatcher.MarkAsCollectionRequired();
        }

        #endregion



        #region Actions


        /// <summary>
        /// Starts the directory watcher collection
        /// </summary>
        public void Collect() {
            _directoryWatcher.Collect();
        }


        /// <summary>
        /// Starts the directory watching
        /// </summary>
        public void Start() {
            _directoryWatcher.Start();
        }


        /// <summary>
        /// Stops the directory watching
        /// </summary>
        public void Stop() {
            _directoryWatcher.Stop();
        }


        #endregion



        #region Watcher callbacks


        /// <summary>
        /// Called when the collecting has completed
        /// </summary>
        protected virtual void directoryWatcher_CollectingCompleted(DirectoryWatcher watcher, int count) {
            if (this.CollectingCompleted != null)
                this.CollectingCompleted.Invoke(this, count);
        }


        /// <summary>
        /// Called when one of the watchers collects a file
        /// </summary>
        protected abstract void FileCollected(DirectoryWatcher watcher, FileInfo file, int count);


        /// <summary>
        /// Called when one of the watchers detected the creation of a file
        /// </summary>
        protected abstract void FileCreated(DirectoryWatcher watcher, FileInfo file);


        /// <summary>
        /// Called when one of the watchers detects a change in a file
        /// </summary>
        protected abstract void FileChanged(DirectoryWatcher watcher, FileInfo file);


        /// <summary>
        /// Called when one of the watchers detects the renaming of a file
        /// </summary>
        protected abstract void FileRenamed(DirectoryWatcher watcher, FileInfo file, RenamedEventArgs e);


        /// <summary>
        /// Called when one of the watchers detects the deletion of a file
        /// </summary>
        protected abstract void FileDeleted(DirectoryWatcher watcher, FileInfo file);


        #endregion



        #region Events


        /// <summary>
        /// Called when the internal DirectoryWatcher has completed collecting
        /// </summary>
        public event CollectingCompletedHandler CollectingCompleted;

        public delegate void CollectingCompletedHandler(MediaFinder watcher, int count);

        #endregion


    }


}
