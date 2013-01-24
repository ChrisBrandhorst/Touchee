// Out of global scope
(function(){
  
  // Default locale
  var Locale = {
    
    touchee:  "Touchee",
    unknown:  "Unknown",
    
    back:     "Back",
    shuffle:  "Shuffle",
    
    
    models: {
      media: {
        one:  'medium',
        more: 'media'
      }
    },
    
    // For plugins
    p: {},
    
    
    
    
    viewTypes: {
      track:    "Songs",
      artist:   "Artists",
      album:    "Albums",
      genre:    "Genres",
      channel:  "Channels",
      webcast:  "Webcasts"
    },
  
    items: {
      track: {
        one:    'song',
        more:   'songs'
      },
      artist: {
        one:    'artist',
        more:   'artists'
      },
      album: {
        one:    'album',
        more:   'albums'
      },
      genre: {
        one:    'genre',
        more:   'genres'
      },
      webcast: {
        one:    'webcast',
        more:   'webcasts'
      }
    },
    
    time: {
      minute: {
        one:  'minute',
        more: 'minutes',
        short: {
          one:  'min.',
          more: 'mins.'
        }  
      }
    },
    
    browser: {
      connecting:   "Connecting to %s...",
      reconnecting: "Connection lost. Reconnecting to %s..."
    }
  
  };
  
  
  
  // Translate method
  Locale.t = function(item, options) {
    options || (options = {});
    
    var trans;
    
    try { trans = eval('Locale.' + item); }
    catch(e) { trans = null; }
    
    if (typeof trans == 'object' && typeof options.count == 'number')
      trans = trans[options.count == 1 ? 'one' : 'more'];
    else if (trans == null)
      trans = item;
    
    return trans;
  };
  
  
  
  // Define as module
  define({
    root:     Locale,
    'nl-nl':  true
  });
  
})();