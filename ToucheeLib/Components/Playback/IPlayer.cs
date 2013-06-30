namespace Touchee.Components.Playback {

    public interface IPlayer : IComponent {


        /// <summary>
        /// The item that is being played
        /// </summary>
        IItem Item { get; }

        /// <summary>
        /// Returns whether this player can play the given item
        /// </summary>
        /// <param name="item">The item to play</param>
        /// <returns>True if the player can play the given item, otherwise false</returns>
        bool CanPlay(IItem item);

        /// <summary>
        /// Plays the given item
        /// </summary>
        /// <param name="item">The item to play</param>
        void Play(IItem item);

        /// <summary>
        /// Pauses the current playback
        /// </summary>
        void Pause();

        /// <summary>
        /// Resums playback if paused
        /// </summary>
        void Play();

        /// <summary>
        /// Stop the current playback
        /// </summary>
        void Stop();

        /// <summary>
        /// Whether the player is currently playing
        /// </summary>
        bool Playing { get; }

        /// <summary>
        /// Gets or sets the playback position in ms
        /// </summary>
        int Position { get; set; }

        /// <summary>
        /// Gets the duration of the currently playing item in ms
        /// </summary>
        int Duration { get;  }

        /// <summary>
        /// Called when the status of the player is updated
        /// </summary>
        event PlayerStatusUpdated StatusUpdated;

        /// <summary>
        /// Called when playback of the current item is finished
        /// </summary>
        event PlayerItemFinished ItemFinished;

    }

    public delegate void PlayerStatusUpdated(IPlayer player);
    public delegate void PlayerItemFinished(IPlayer player, IItem item);



}
