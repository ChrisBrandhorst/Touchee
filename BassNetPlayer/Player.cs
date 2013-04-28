using System;
using System.IO;
using System.Collections.Generic;

using Touchee;
using Touchee.Components.Playback;
using Touchee.Media.Music;

using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.AddOn.Mix;

namespace BassNetPlayer {

    /// <remarks>
    /// The class responsible for playback of the Bass.NET player.
    /// </remarks>
    public class Player : Base, IAudioPlayer {


        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Player() {
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_PLAYLIST, 1);
            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
        }

        #endregion



        #region IPlayer implementation

        /// <summary>
        /// The item that is being played
        /// </summary>
        public IItem Item { get; protected set; }


        /// <summary>
        /// Returns whether this player can play the given item
        /// </summary>
        /// <param name="item">The item to play</param>
        /// <returns>True if the player can play the given item, otherwise false</returns>
        public bool CanPlay(IItem item) {
            return item is IAudioItem;
        }


        /// <summary>
        /// Plays the given item
        /// </summary>
        /// <param name="item">The item to play</param>
        public void Play(IItem item) {
            
            // Stop playing current stream
            //this.Stop();
            
            // Create stream
            this.CreateStream((IAudioItem)item);

            //
            this.Item = item;
        }


        /// <summary>
        /// Pauses the current playback
        /// </summary>
        public void Pause() {
            Bass.BASS_Pause();
        }


        /// <summary>
        /// Resums playback if paused
        /// </summary>
        public void Play() {
            Bass.BASS_Start();
        }


        /// <summary>
        /// Stop the current playback
        /// </summary>
        public void Stop() {
            Bass.BASS_ChannelStop(_mixer);
            BassMix.BASS_Mixer_ChannelRemove(_currentStream);
            Bass.BASS_Stop();
            Bass.BASS_StreamFree(_currentStream);
            Bass.BASS_StreamFree(_mixer);
            _currentStream = -1;
            _mixer = -1;
        }


        /// <summary>
        /// Called when playback of the current item is finished
        /// </summary>
        public event PlayerPlaybackFinished PlaybackFinished;


        /// <summary>
        /// Called when the status of the player is updated
        /// </summary>
        public event PlayerStatusUpdated StatusUpdated;


        #endregion



        #region IAudioPlayer implementation


        /// <summary>
        /// 
        /// </summary>
        public AudioPlayerSupport Support {
            get { return AudioPlayerSupport.LFE | AudioPlayerSupport.Upmix; }
        }


        /// <summary>
        /// Get or set the volume of the LFE channel between and including 0 and 1
        /// After a set, this method modifies the stream matrix
        /// </summary>
        public double LFEVolume {
            get { return _lfeVolume; }
            set {
                _lfeVolume = Math.Max(0, Math.Min(1, value));
                if (_currentStream != 0)
                    SetMatrix(_currentStream);
            }
        }


        /// <summary>
        /// Whether to upmix items to 5.1 surround sound
        /// </summary>
        public bool UpMixToSurround { get; set; }


        #endregion



        #region Privates

        // Private holding the value for the LFEVolume property
        double _lfeVolume = 0.5;

        // The current stream pointer
        int _currentStream = -1;

        // The current mixer pointer
        int _mixer = -1;

        // Channel ending callback proc
        SYNCPROC _channelEndCallback;

        #endregion



        #region Stream handling


        /// <summary>
        /// Creates a stream for the given track.
        /// If the stream is created successfully, the track is automatically played. Thus,
        /// this implementation is synchronous.
        /// </summary>
        /// <param name="track">The track to create the stream for and play.</param>
        protected virtual void CreateStream(IAudioItem track) {
            int stream = 0;

            // If we have a webcast, create a stream for one of the available URIs
            if (track is IWebcast) {

                var streams = ((IWebcast)track).Streams;
                foreach (var si in streams) {
                    var url = si.Uri.ToString();
                    stream = Bass.BASS_StreamCreateURL(url, 0, BASSFlag.BASS_STREAM_STATUS | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT, null, IntPtr.Zero);
                    Log(Bass.BASS_ErrorGetCode().ToString());
                    if (stream != 0) {
                        Un4seen.Bass.AddOn.Tags.TAG_INFO tagInfo = new TAG_INFO(url);
                        if (BassTags.BASS_TAG_GetFromURL(stream, tagInfo)) {
                            // display the tags...
                        }
                        break;
                    }
                }

            }

            // Else, just load the track
            else if (track is IFileTrack) {
                var path = Path.GetFullPath(((ITrack)track).Uri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped));
                stream = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_STREAM_STATUS | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);
            }

            // Start the stream if successfull
            if (stream != 0)
                this.StreamCreated(track, stream);
            else
                throw new NotImplementedException("Unhandled: no valid stream created");
        }


        /// <summary>
        /// Called when a stream has been created. This actually starts the playback of the stream.
        /// </summary>
        /// <param name="track">The track that is to be played.</param>
        /// <param name="stream">The BASS.NET stream pointer to play.</param>
        protected virtual void StreamCreated(IAudioItem track, int stream) {

            // Init mixer
            if (_mixer == -1) {
                _mixer = BassMix.BASS_Mixer_StreamCreate(44100, 6, BASSFlag.BASS_MIXER_END);
                
                // Set playback done callback on mixer
                _channelEndCallback = new SYNCPROC(ChannelEnd);
                Bass.BASS_ChannelSetSync(_mixer, BASSSync.BASS_SYNC_END | BASSSync.BASS_SYNC_MIXTIME, 0, _channelEndCallback, IntPtr.Zero);
            }

            // Load streamin mixer
            bool ok = BassMix.BASS_Mixer_StreamAddChannel(_mixer, stream, BASSFlag.BASS_STREAM_AUTOFREE | BASSFlag.BASS_MIXER_MATRIX);
            if (!ok) Log(Bass.BASS_ErrorGetCode().ToString(), Logger.LogLevel.Error);

            // Set matrix
            SetMatrix(stream);

            // Remove current channel from mixer
            if (_currentStream != -1) {
                BassMix.BASS_Mixer_ChannelRemove(_currentStream);
                Bass.BASS_StreamFree(_currentStream);
            }

            // 
            if (track is IWebcast) {
                SYNCPROC _mySync = new SYNCPROC(MetaSync);
                Bass.BASS_ChannelSetSync(_currentStream, BASSSync.BASS_SYNC_META, 0, _mySync, IntPtr.Zero);
            }

            // Play it!
            Bass.BASS_ChannelSetPosition(_mixer, 0);
            Bass.BASS_Start();
            Bass.BASS_ChannelPlay(_mixer, false);

            // Set current stuff
            _currentStream = stream;
        }


        /// <summary>
        /// Called when a channel has ended the playback. This triggers the PlaybackFinished event.
        /// </summary>
        void ChannelEnd(int handle, int channel, int data, IntPtr user) {
            this.Item = null;
            if (PlaybackFinished != null)
                PlaybackFinished.Invoke(this, this.Item);
        }


        /// <summary>
        /// Called when metadata for a url stream is changed
        /// </summary>
        void MetaSync(int handle, int channel, int data, IntPtr user) {
            string[] tags = Bass.BASS_ChannelGetTagsMETA(channel);
            foreach (string tag in tags)
                Console.WriteLine(tag);
        }


        #endregion



        #region Mix matrix stuff


        /// <summary>
        /// Sets the correct mix matrix for the given stream pointer.
        /// </summary>
        /// <param name="stream">The stream pointer to set the mix matrix for</param>
        void SetMatrix(int stream) {
            if (UpMixToSurround) {
                var matrix = GetMixMatrix(stream);
                if (matrix != null)
                    BassMix.BASS_Mixer_ChannelSetMatrix(stream, matrix);
            }
        }


        /// <summary>
        /// Gets the correct mix matrix for the given stream.
        /// </summary>
        /// <param name="stream">The stream pointer to get the mix matrix for</param>
        /// <returns>The mix matrix, or null if something fails</returns>
        float[,] GetMixMatrix(int stream) {

            // Get the stream info
            var streamInfo = Bass.BASS_ChannelGetInfo(stream);
            if (streamInfo == null || !_mixMatrices.ContainsKey(streamInfo.chans)) return null;
            
            // Get the matrix
            var matrix = _mixMatrices[streamInfo.chans];

            // Set the LFE channel
            for (int i = 0; i < streamInfo.chans; i++)
                matrix[3, i] = (float)Math.Max(0, _maxLFELevel * Math.Min(1, LFEVolume) / streamInfo.chans);

            // Return the matrix
            return matrix;
        }


        // The maximum LFE level
        float _maxLFELevel = 0.2F;


        #endregion



        #region Matrices

        // When streaming multi-channel sample data, the channel order of each sample is as follows.
        // 3 channels	    left-front, right-front, center. 
        // 4 channels	    left-front, right-front, left-rear/side, right-rear/side. 
        // 5 channels	    left-front, right-front, center, left-rear/side, right-rear/side. 
        // 6 channels (5.1)	left-front, right-front, center, LFE, left-rear/side, right-rear/side. 
        // 8 channels (7.1)	left-front, right-front, center, LFE, left-rear/side, right-rear/side, left-rear center, right-rear center
        Dictionary<int, float[,]> _mixMatrices = new Dictionary<int, float[,]>() {

            // Stereo: upmix to 5.1
            { 2, new float[,]{
                { 1, 0 },
                { 0, 1 },
                { (float)Math.Sqrt(1/2), (float)Math.Sqrt(1/2) },
                { 0.1F, 0.1F },
                { -(float)Math.Sqrt(2/3), (float)Math.Sqrt(1/3) },
                { -(float)Math.Sqrt(1/3), (float)Math.Sqrt(2/3) }
            } },

            // 3.0: upmix to 5.1
            { 3, new float[,]{
                { 1, 0, 0 },
                { 0, 1, 0 },
                { 0, 0, 1 },
                { 0.033F, 0.033F, 0.033F },
                { -(float)Math.Sqrt(2/3), (float)Math.Sqrt(1/3), 0 },
                { -(float)Math.Sqrt(1/3), (float)Math.Sqrt(2/3), 0 }
            } },

            // Quadraphonic: add LFE channel, center remains silent
            { 4, new float[,]{
                { 1, 0, 0, 0 },
                { 0, 1, 0, 0 },
                { 0, 0, 0, 0 },
                { 0.05F, 0.05F, 0.05F, 0.05F },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 }
            } },

            // 5.0 to 5.1: add LFE channel
            { 5, new float[,]{
                { 1, 0, 0, 0, 0 },
                { 0, 1, 0, 0, 0 },
                { 0, 0, 1, 0, 0 },
                { 0.04F, 0.04F, 0.04F, 0.04F, 0.04F },
                { 0, 0, 0, 1, 0 },
                { 0, 0, 0, 0, 1 }
            } }

        };

        #endregion
        
    }

}
