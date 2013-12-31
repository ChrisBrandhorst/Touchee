namespace Touchee.Components.Media {

    public interface IMediumWatcher : IComponent {
        bool CanWatch(Medium medium);
        bool Watch(Medium medium);
        bool UnWatch(Medium medium);
        bool UnWatchAll();
        event MediumWatcherStartedWatching StartedWatching;
        event MediumWatcherStoppedWatching StoppedWatching;
    }

    public delegate void MediumWatcherStartedWatching(IMediumWatcher watcher, Medium medium);
    public delegate void MediumWatcherStoppedWatching(IMediumWatcher watcher, Medium medium);

}
