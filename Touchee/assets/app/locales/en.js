Touchee.T = {
  
  unknown:  "Unknown",
  back:     "Back",
  
  shuffle:  "Shuffle",
  
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
  }
  
};


Touchee.t = function(item, options) {
  var trans = eval('Touchee.T.' + item);
  
  if (typeof options.count == 'number')
    trans = trans[options.count == 1 ? 'one' : 'more'];
  
  return trans;
};