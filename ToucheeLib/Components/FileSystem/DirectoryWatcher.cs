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


    public class DirectoryWatcher : Base {



        #region Privates

        // The internally used watcher
        FileSystemWatcher _watcher;
        // List of supported extensions
        IEnumerable<string> _extensions;
        // Regex built from the extensions list
        Regex _extensionsRegex;
 
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
                _extensionsRegex = new Regex(@"^\.(" + String.Join("|", _extensions) + ")", RegexOptions.IgnoreCase);
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


        #endregion



        #region Constructors


        /// <summary>
        /// Constructs a new DirectoryWatcher for the given path
        /// </summary>
        /// <param name="path">The path of the directory to watch</param>
        public DirectoryWatcher(string path) : this(new DirectoryInfo(path)) { }


        /// <summary>
        /// Constructs a new DirectoryWatcher for the given path
        /// </summary>
        /// <param name="path">The path of the directory to watch</param>
        /// <param name="extensions">The extensions to watch for</param>
        public DirectoryWatcher(string path, IEnumerable<string> extensions) : this(new DirectoryInfo(path), extensions) { }


        /// <summary>
        /// Constructs a new DirectoryWatcher for the given directory
        /// </summary>
        /// <param name="directoryInfo">The DirectoryInfo instance for the directory to watch</param>
        public DirectoryWatcher(DirectoryInfo directoryInfo) : this(directoryInfo, new List<string>()) { }


        /// <summary>
        /// Constructs a new DirectoryWatcher for the given directory
        /// </summary>
        /// <param name="directoryInfo">The DirectoryInfo instance for the directory to watch</param>
        /// <param name="extensions">The extensions to watch for</param>
        public DirectoryWatcher(DirectoryInfo directoryInfo, IEnumerable<string> extensions) {
            this.Directory = directoryInfo;
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
                if (this.FileCollected != null)
                    this.FileCollected.Invoke(this, file, count);
            }

            // Completed callback
            Log(count.ToString() + " files found");
            this.CollectionState = CollectionState.Collected;
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
        /// Occurs when a file is changed (deleted, changed or created)
        /// </summary>
        void FileSystemWatcherCreated(object sender, FileSystemEventArgs e) {
            var file = new FileInfo(e.FullPath);
            if (_extensionsRegex.IsMatch(file.Extension) && this.FileCreated != null)
                this.FileCreated.Invoke(this, file);
        }
        /// <summary>
        /// Occurs when a file is changed
        /// </summary>
        void FileSystemWatcherChanged(object sender, FileSystemEventArgs e) {
            var file = new FileInfo(e.FullPath);
            if (_extensionsRegex.IsMatch(file.Extension) && this.FileChanged != null)
                this.FileChanged.Invoke(this, file);
        }
        /// <summary>
        /// Occurs when a file is renamed
        /// </summary>
        void FileSystemWatcherRenamed(object sender, RenamedEventArgs e) {
            var file = new FileInfo(e.FullPath);
            if (_extensionsRegex.IsMatch(file.Extension) && this.FileRenamed != null)
                this.FileRenamed.Invoke(this, file, e);
        }
        /// <summary>
        /// Occurs when a file is changed (deleted, changed or created)
        /// </summary>
        void FileSystemWatcherDeleted(object sender, FileSystemEventArgs e) {
            var file = new FileInfo(e.FullPath);
            if (_extensionsRegex.IsMatch(file.Extension) && this.FileDeleted != null)
                this.FileDeleted.Invoke(this, file);
        }


        #endregion



        #region Events


        public event FileCreatedHandler FileCreated;
        public event FileChangedHandler FileChanged;
        public event FileRenamedHandler FileRenamed;
        public event FileDeletedHandler FileDeleted;
        public event FileCollectedHandler FileCollected;
        public event CollectingCompletedHandler CollectingCompleted;

        public delegate void FileCreatedHandler(DirectoryWatcher watcher, FileInfo file);
        public delegate void FileChangedHandler(DirectoryWatcher watcher, FileInfo file);
        public delegate void FileRenamedHandler(DirectoryWatcher watcher, FileInfo file, RenamedEventArgs e);
        public delegate void FileDeletedHandler(DirectoryWatcher watcher, FileInfo file);
        public delegate void FileCollectedHandler(DirectoryWatcher watcher, FileInfo file, int count);
        public delegate void CollectingCompletedHandler(DirectoryWatcher watcher, int count);


        #endregion


    }


}
