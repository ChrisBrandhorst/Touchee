using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Touchee.Components.FileSystem {
    


    public class DirectoriesWatcher {


        List<DirectoryWatcher> _watchers = new List<DirectoryWatcher>();
        IEnumerable<string> _extensions;


        public DirectoriesWatcher() {
        }
        public DirectoriesWatcher(IEnumerable<string> extensions) : base() {
            _extensions = extensions;
        }
        public DirectoriesWatcher(IEnumerable<DirectoryInfo> directoryInfos) {
            this.AddDirectories(directoryInfos);
        }
        public DirectoriesWatcher(IEnumerable<string> paths, IEnumerable<string> extensions) : this(extensions) {
            this.AddDirectories(paths);
        }
        public DirectoriesWatcher(IEnumerable<DirectoryInfo> directoryInfos, IEnumerable<string> extensions) : this(directoryInfos) {
            _extensions = extensions;
        }


        public void AddDirectory(string path) {
            this.AddDirectory(new DirectoryInfo(path));
        }

        public void AddDirectory(DirectoryInfo path) {
        }

        public void AddDirectories(IEnumerable<string> paths) {
            foreach (var p in paths)
                this.AddDirectory(p);
        }

        public void AddDirectories(IEnumerable<DirectoryInfo> directoryInfos) {
            foreach (var di in directoryInfos)
                this.AddDirectory(di);
        }




        public void Collect() {
        }

        public void Start() {
        }

        public void Stop() {
        }



    }

}
