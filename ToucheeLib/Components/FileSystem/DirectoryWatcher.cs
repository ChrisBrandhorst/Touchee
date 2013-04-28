using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace Touchee.Components.FileSystem {


    public enum CollectionState {
        Uncollected,
        CollectionRequired,
        Collecting,
        Collected
    }


    /// <summary>
    /// 
    /// </summary>
    public abstract class DirectoryWatcher : Base {



        #region Privates

        // The internally used watcher
        FileSystemWatcher _watcher;
        // List of supported extensions
        IEnumerable<string> _extensions;
        // Regex built from the extensions list
        Regex _extensionsRegex;
        // Whether to catch files with all extensions
        bool _catchAllExtensions;
 
        #endregion



        #region Properties


        /// <summary>
        /// The DirectoryInfo object for the directory that is being watched
        /// </summary>
        public DirectoryInfo Directory { get; protected set; }


        /// <summary>
        /// The extensions to use when filtering the files in the directory
        /// </summary>
        public IEnumerable<string> Extensions {
            get { return _extensions; }
            set {
                _extensions = value;
                _catchAllExtensions = _extensions.Count() == 0;
                _extensionsRegex = new Regex(@"^\.(" + String.Join("|", _extensions) + ")$", RegexOptions.IgnoreCase);
            }
        }


        /// <summary>
        /// The search option to use when enumerating through files
        /// </summary>
        public SearchOption SearchOption { get; set; }


        /// <summary>
        /// The state of the collecting
        /// </summary>
        public CollectionState CollectionState { get; protected set; }


        /// <summary>
        /// The medium this watcher is watching
        /// </summary>
        public Medium Medium { get; protected set; }
        

        #endregion



        #region Constructors


        /// <summary>
        /// Constructs a new DirectoryWatcher for the given path
        /// </summary>
        /// <param name="path">The path of the directory to watch</param>
        public DirectoryWatcher(Medium medium, string path) : this(medium, new DirectoryInfo(path)) { }


        /// <summary>
        /// Constructs a new DirectoryWatcher for the given path
        /// </summary>
        /// <param name="path">The path of the directory to watch</param>
        /// <param name="extensions">The extensions to watch for</param>
        public DirectoryWatcher(Medium medium, string path, IEnumerable<string> extensions) : this(medium, new DirectoryInfo(path), extensions) { }


        /// <summary>
        /// Constructs a new DirectoryWatcher for the given directory
        /// </summary>
        /// <param name="directoryInfo">The DirectoryInfo instance for the directory to watch</param>
        public DirectoryWatcher(Medium medium, DirectoryInfo directoryInfo) : this(medium, directoryInfo, new List<string>()) { }


        /// <summary>
        /// Constructs a new DirectoryWatcher for the given directory
        /// </summary>
        /// <param name="directoryInfo">The DirectoryInfo instance for the directory to watch</param>
        /// <param name="extensions">The extensions to watch for</param>
        public DirectoryWatcher(Medium medium, DirectoryInfo directoryInfo, IEnumerable<string> extensions) {
            this.Directory = directoryInfo;
            this.Medium = medium;
            this.SearchOption = SearchOption.AllDirectories;
            this.Extensions = extensions;
            this.CollectionState = CollectionState.Uncollected;

            _watcher = new FileSystemWatcher(this.Directory.FullName, "*.*");
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.LastWrite;
            _watcher.IncludeSubdirectories = true;
            _watcher.Deleted += FileSystemWatcherDeleted;
            _watcher.Changed += FileSystemWatcherChanged;
            _watcher.Created += FileSystemWatcherCreated;
            _watcher.Renamed += FileSystemWatcherRenamed;
            _watcher.Error += FileSystemWatcherError;
        }


        #endregion



        #region Actions


        /// <summary>
        /// Marks the current watcher as requiring collection
        /// </summary>
        public void MarkAsCollectionRequired() {
            if (this.CollectionState != FileSystem.CollectionState.Collecting)
                this.CollectionState = CollectionState.CollectionRequired;
        }


        /// <summary>
        /// Collect all currently existing files
        /// </summary>
        public void Collect() {
            if (this.Directory == null)
                throw new ArgumentNullException("Directory", "Directory is not set. Use the correct constructor.");
            else {
                if (this.CollectionState != CollectionState.Collected && this.CollectionState != CollectionState.Collecting)
                    new Thread(() => CollectThread()).Start();
            }
        }


        /// <summary>
        /// Collect all currently existing files
        /// </summary>
        void CollectThread() {
            this.CollectionState = CollectionState.Collecting;

            // Get all files
            var files = this.Directory.EnumerateFiles("*.*", SearchOption.AllDirectories);

            // If any extensions are set, filter the list
            if (this.Extensions.Count() > 0)
                files = files.Where(f => _extensionsRegex.IsMatch(f.Extension));

            // Go through all files
            var count = 0;
            foreach (var file in files) {
                count++;
                if (count % 50 == 0) Log(count.ToString() + " files found");
                this.OnFileCollected(file, count);
                if (this.FileCollected != null)
                    this.FileCollected.Invoke(this, file, count);
            }

            // Completed callback
            Log(count.ToString() + " files found");
            this.CollectionState = CollectionState.Collected;
            this.OnCollectingCompleted(count);
            if (this.CollectingCompleted != null)
                this.CollectingCompleted.Invoke(this, count);
        }


        /// <summary>
        /// Start watching the directory
        /// </summary>
        public void Start() {
            if (this.Directory == null)
                throw new ArgumentNullException("Directory", "Directory is not set. Use the correct constructor.");
            _watcher.EnableRaisingEvents = true;
        }


        /// <summary>
        /// Stop watching the directory
        /// </summary>
        public void Stop() {
            if (this.Directory == null)
                throw new ArgumentNullException("Directory", "Directory is not set. Use the correct constructor.");
            _watcher.EnableRaisingEvents = false;
        }


        #endregion



        #region Watcher callbacks


        /// <summary>
        /// Occurs when an error occured in the watcher
        /// </summary>
        void FileSystemWatcherError(object sender, ErrorEventArgs e) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Occurs when a file has been collected
        /// </summary>
        protected virtual void OnFileCollected(FileInfo file, int count) { }

        /// <summary>
        /// Occurs when file collection has completed
        /// </summary>
        protected virtual void OnCollectingCompleted(int count) { }

        /// <summary>
        /// Occurs when a file is changed (deleted, changed or created)
        /// </summary>
        void FileSystemWatcherCreated(object sender, FileSystemEventArgs e) {
            var file = new FileInfo(e.FullPath);
            if (_catchAllExtensions || _extensionsRegex.IsMatch(file.Extension))
                this.AfterFileClosed(file, () => this.OnFileCreated(file));
        }
        protected virtual void OnFileCreated(FileInfo file) { }

        /// <summary>
        /// Occurs when a file is changed
        /// </summary>
        void FileSystemWatcherChanged(object sender, FileSystemEventArgs e) {
            var file = new FileInfo(e.FullPath);
            if (_catchAllExtensions || _extensionsRegex.IsMatch(file.Extension))
                this.AfterFileClosed(file, () => this.OnFileChanged(file));
        }
        protected virtual void OnFileChanged(FileInfo file) { }

        /// <summary>
        /// Occurs when a file is renamed
        /// </summary>
        void FileSystemWatcherRenamed(object sender, RenamedEventArgs e) {
            var file = new FileInfo(e.FullPath);
            if (_catchAllExtensions || _extensionsRegex.IsMatch(file.Extension))
                this.AfterFileClosed(file, () => this.OnFileRenamed(file, e));
        }
        protected virtual void OnFileRenamed(FileInfo file, RenamedEventArgs e) { }

        /// <summary>
        /// Occurs when a file is changed (deleted, changed or created)
        /// </summary>
        void FileSystemWatcherDeleted(object sender, FileSystemEventArgs e) {
            var file = new FileInfo(e.FullPath);
            if (_catchAllExtensions || _extensionsRegex.IsMatch(file.Extension))
                this.OnFileDeleted(file);
        }
        protected virtual void OnFileDeleted(FileInfo file) { }

        #endregion



        /// <summary>
        /// Waits for the given file to close before invoking the given action
        /// </summary>
        /// <param name="file">The file to check</param>
        /// <param name="action">The action to invoke</param>
        void AfterFileClosed(FileInfo file, Action action) {
            // Always wait a while
            Thread.Sleep(500);

            // Stop after ten tries
            var i = 0;
            while (i < 10) {
                try {
                    var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fs.Close();
                    action.Invoke();
                    return;
                }
                catch (IOException) {
                    Thread.Sleep(1000);
                    i++;
                }
            }
            Log(String.Format("Unable to open file {0}", file.FullName), Logger.LogLevel.Error);
        }




        #region Events

        public event FileCollectedHandler FileCollected;
        public event CollectingCompletedHandler CollectingCompleted;

        public delegate void FileCollectedHandler(DirectoryWatcher watcher, FileInfo file, int count);
        public delegate void CollectingCompletedHandler(DirectoryWatcher watcher, int count);


        #endregion


    }


}
