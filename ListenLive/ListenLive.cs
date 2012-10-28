using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Touchee;
using Touchee.Artwork;
using System.Drawing;
using HtmlAgilityPack;
using System.Threading;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace ListenLive {


    /// <summary>
    /// The media types used
    /// </summary>
    public static class Types {
        public const string Channel = "channel";
        public const string Genre = "genre";
    }


    /// <remarks>
    /// Plugin for retrieving radio channels from www.listenlive.eu
    /// </remarks>
    public class ListenLive : Base, IPlugin, IMediumWatcher, IContentsPlugin {


        #region Privates

        RadioChannels _container;
        Medium _radioMedium;
        int _fetchHour = 3;
        string _url = "http://www.listenlive.eu/netherlands.html";
        string _containerName = "Nederlands";
        Timer _timer;

        #endregion


        #region IPlugin implementation

        /// <summary>
        /// Starts the plugin.
        /// </summary>
        /// <param name="config">The configuration object for this plugin</param>
        /// <returns>Always true</returns>
        public bool Start(dynamic config) {

            // Set url and name from config
            if (config != null) {
                _url = config.GetString("url", _url);
                _containerName = config.GetString("containerName", _containerName);
                _fetchHour = config.GetInt("fetchHour", _fetchHour);
            }
            
            return true;
        }

        /// <summary>
        /// The name of this plugin
        /// </summary>
        public string Name {
            get { return "ListenLive.eu"; }
        }

        /// <summary>
        /// Shuts down the plugin.
        /// </summary>
        /// <returns>True</returns>
        public bool Shutdown() {
            return true;
        }

        #endregion


        #region IMediumWatcher implementation

        /// <summary>
        /// Start watching the given medium. Only if a medium with type Local is given, is ListenLive going to be watched.
        /// </summary>
        /// <param name="medium">The Medium to watch</param>
        public bool Watch(Medium medium) {

            // Do nothing if we already have a local medium
            if (medium.Type != MediumType.Web || _radioMedium != null) return false;
            _radioMedium = medium;

            // Set container
            _container = new RadioChannels(_containerName, _radioMedium, true);

            // Start timer for fetching channels
            this.StartFetching();

            return true;
        }


        /// <summary>
        /// Stop watching the given medium
        /// </summary>
        /// <param name="medium">The medium to stop watching</param>
        public bool UnWatch(Medium medium) {
            if (_radioMedium == medium) {
                _radioMedium = null;
                // TODO: clear container(s)
                // For now, we can assume this call is never made, since the radio
                // medium will never be ejected
                return true;
            }
            return false;
        }


        #endregion


        #region IContentsPlugin implementation


        /// <summary>
        /// Gets the items collection for the given parameters
        /// </summary>
        /// <param name="container">The container for which the items should be retreived</param>
        /// <param name="filter">The filter object which contains the parameters with which to query for items</param>
        /// <returns>The results</returns>
        public IEnumerable<IItem> GetItems(IContainer container,Options filter) {
            var channels = GetChannels(container, filter);
            return channels == null ? null : channels.Cast<IItem>();
        }


        /// <summary>
        /// Gets the contents object for the given parameters
        /// </summary>
        /// <param name="container">The container for which the contents should be retreived</param>
        /// <param name="filter">The filter object which contains the parameters with which to query for items</param>
        /// <returns>The results</returns>
        public Contents GetContents(IContainer container, Options filter) {
            var channels = GetChannels(container, filter);
            if (channels == null) return null;
            var channelsContainer = (RadioChannels)container;
            return BuildContents(channels, filter, channelsContainer);
        }


        /// <summary>
        /// Gets the channels collection for the given parameters
        /// </summary>
        /// <param name="container">The container for which the channels should be retreived</param>
        /// <param name="filter">The filter object which contains the parameters with which to query for items</param>
        /// <returns>The results</returns>
        IEnumerable<RadioChannel> GetChannels(IContainer container, Options filter) {

            // Check if we have a radiochannels container, just to be sure
            if (!(container is RadioChannels)) return null;

            // Get channels container
            var channelsContainer = (RadioChannels)container;

            // Get all channels
            var channels = channelsContainer.Channels;

            // Filter channels
            channels = FilterChannels(channels, filter);

            // Return channels
            return channels;
        }



        /// <summary>
        /// Filters the given collection of channels by the given filter
        /// </summary>
        /// <param name="channels">The channels to filter</param>
        /// <param name="filter">The filter object which contains the parameters with which to query for items</param>
        /// <returns>A IEnumerable of filtered channels</returns>
        IEnumerable<RadioChannel> FilterChannels(IEnumerable<RadioChannel> channels, Options filter) {

            foreach (var key in filter.Keys) {
                var value = filter[key];

                switch (key) {

                    case "genre":
                        channels = channels.Where(t => Util.Equals(t.Genre, value, true));
                        break;

                    case "query":
                        channels = channels.Where(t => t.Name.Matches(value));
                        break;

                }
            }

            return channels;
        }
        
        
        /// <summary>
        /// Builds the contents object
        /// </summary>
        /// <param name="channels">The channels source</param>
        /// <param name="filter">The filter object which contains the parameters with which to query for items</param>
        /// <param name="channelsContainer">The radio channels container the tracks are sources from</param>
        /// <returns>A filled contents object</returns>
        Contents BuildContents(IEnumerable<RadioChannel> channels, Options filter, RadioChannels channelsContainer) {

            // Create contents instance
            var contents = new Contents(channelsContainer);

            var type = (filter.ContainsKey("type") ? filter["type"] : channelsContainer.ViewTypes.FirstOrDefault()) ?? Types.Channel;

            // Set meta data
            dynamic meta = new ExpandoObject();
            contents.Meta = meta;
            meta.SortedByAlpha = true;

            // Get the data for the given type
            switch (type) {
                case Types.Channel:
                    contents.Data = GetChannelsData(channels);
                    contents.Keys = new string[] { "id", "name", "index" };
                    break;
                case Types.Genre:
                    contents.Data = GetGenresData(channels);
                    contents.Keys = new string[] { "genre", "index" };
                    break;
            }

            return contents;
        }


        /// <summary>
        /// Retrieves an array of channel data (id, name, index) for the given channels set.
        /// The result is sorted by name of the channel.
        /// </summary>
        /// <param name="channels">The channels source</param>
        /// <returns>An array of channel data</returns>
        object GetChannelsData(IEnumerable<RadioChannel> channels) {
            return channels
                .Select(c => new object[]{
                    c.ID,
                    c.Name,
                    Util.GetIndex(c.SortName)
                });
        }


        /// <summary>
        /// Retrieves an array of genre data (name) for the given channels set
        /// </summary>
        /// <param name="channels">The channels source</param>
        /// <returns>An array of genre data</returns>
        object GetGenresData(IEnumerable<RadioChannel> channels) {
            return channels
                .Select(c => c.Genre)
                .Distinct()
                .Where(g => g != null)
                .Select(g =>
                    new Tuple<string, string>(g, g.ToSortName())
                )
                .OrderBy(g => !Util.FirstIsAlpha(g.Item2))
                .ThenBy(g => g.Item2)
                .Select(g =>
                    new string[] { g.Item1, Util.GetIndex(g.Item2) }
                );
        }


        /// <summary>
        /// Gets the artwork for the given radiochannel item
        /// </summary>
        /// <param name="container">The container the item resides in</param>
        /// <param name="item">Should be a RadioChannel</param>
        /// <param name="artwork">The resulting artwork</param>
        /// <returns>An ArtworkStatus</returns>
        public ArtworkStatus GetArtwork(IContainer container, IItem item, out Image artwork) {

            // Get query
            var channel = (RadioChannel)item;
            var query = Regex.Replace(channel.Website ?? "", @"^(http:\/\/)?(www.)?", "", RegexOptions.Compiled) + " \"" + channel.Name + "\"";

            // Get plugins for artwork retrieval
            var plugins = Plugins.Get<Touchee.Service.ICustomArtworkService>();
            artwork = null;
            foreach (var plugin in plugins) {
                plugin.GetCustomArtwork(query, out artwork);
                if (artwork != null) break;
            }
            return artwork == null ? ArtworkStatus.Unavailable : ArtworkStatus.Retrieved;
        }


        /// <summary>
        /// Not used.
        /// </summary>
        /// <returns>ArtworkStatus.Unavailable</returns>
        public ArtworkStatus GetArtwork(IContainer container, Options filter, out Image artwork) {
            artwork = null;
            return ArtworkStatus.Unavailable;
        }


        #endregion


        #region Channel fetching

        /// <summary>
        /// Starts fetching for channels every night at 3AM
        /// </summary>
        void StartFetching() {
            var now = DateTime.Now;
            var lastMidnight = now.Date;
            var due = lastMidnight.AddHours(_fetchHour);
            if (due < now) due = due + TimeSpan.FromHours(24);
            if (due - now < TimeSpan.FromHours(1)) due = due + TimeSpan.FromHours(24);
            _timer = new Timer(this.FetchChannels, null, due - now, TimeSpan.FromHours(24));
            this.FetchChannels(null);
        }
        

        /// <summary>
        /// Fetches the channels in a new thread.
        /// </summary>
        void FetchChannels(object stateInfo) {

            // Get channels
            var channels = this.GetChannels(_url);

            if (channels.Count() > 0) {
                _container.Update(channels);
                _container.Save();
            }
            else
                Log("No channels found. Error somewhere? See above log...", Logger.LogLevel.Error);
        }

        /// <summary>
        /// Gets channels from the URL
        /// </summary>
        IEnumerable<RadioChannel> GetChannels(string url) {
            var channels = new List<RadioChannel>();

            // Get document
            var doc = Util.DownloadHTMLDocument(url);
            if (doc == null) return channels;

            // Get the table rows containing info about the channels
            var channelRows = doc.DocumentNode.SelectNodes("//*[@id='content']//tr");

            // Bail out if no match
            if (channelRows == null) {
                Log("Could not get channel rows from the document. Perhaps the structure changed?", Logger.LogLevel.Error);
                return channels;
            }

            // Loop through the rows, skip the first (header)
            for (int i = 1; i < channelRows.Count; i++) {

                // Get the row
                var row = channelRows[i];

                // Get url and name node
                var urlAndNameNode = row.SelectSingleNode("td[1]//a");
                if (urlAndNameNode == null) continue;
                var channelURL = urlAndNameNode.GetAttributeValue("href", "");
                var channelName = urlAndNameNode.InnerText;

                // Get genre
                var genreNode = row.SelectSingleNode("td[5]");
                var genre = genreNode == null ? null : genreNode.InnerText;

                // Build channel object
                var channel = RadioChannel.FindOrCreateByName(channelName, genre, channelURL);

                // Get encodings
                var encodingStrings = row.SelectNodes("td[3]/img").Select(n => n.GetAttributeValue("alt", "").ToLower());
                if (encodingStrings == null) continue;

                // Get streams
                var streamNodes = row.SelectNodes("td[4]/*");
                if (streamNodes == null) continue;
                var walkNode = streamNodes[0];
                HtmlNode lastStreamNode = null;

                // Loop through encodings
                foreach (var encodingString in encodingStrings) {

                    // Get encoding enum
                    StreamEncoding streamEncoding;
                    switch (encodingString) {
                        case "windows media": streamEncoding = StreamEncoding.WindowsMedia; break;
                        case "aacplus": streamEncoding = StreamEncoding.HEAAC; break;
                        case "mp3": streamEncoding = StreamEncoding.MP3; break;
                        default: continue;
                    }

                    // Find the latest node for the current encoding
                    while (walkNode != null && walkNode.Name != "br") {
                        if (walkNode.Name == "a")
                            lastStreamNode = walkNode;
                        walkNode = walkNode.NextSibling;
                    }
                    if (lastStreamNode == null) continue;
                    else if (walkNode != null) walkNode = walkNode.NextSibling;

                    // Add stream to channel
                    string streamHref = "";
                    try {
                        streamHref = lastStreamNode.GetAttributeValue("href", "");
                        var streamUri = new Uri(streamHref);
                        var stream = new StreamInfo(streamUri, streamEncoding);
                        channel.Streams.Add(stream);
                    }
                    catch (Exception) {
                        Log("Invalid uri: " + streamHref);
                    }
                }

                // Add channel
                channel.Save();
                channels.Add(channel);
            }

            return channels;
        }

        #endregion


    }

}
