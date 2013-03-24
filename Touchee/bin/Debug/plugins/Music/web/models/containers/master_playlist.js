define([
  'underscore',
  'Backbone',
  'models/container',
  './../track'
], function(_, Backbone, Container, Track){
  
  var MasterPlaylist = Container.extend({
    
    
    // The model used for the items within the contents object
    contentsItemModel: Track,
    
    
    // The different views that are available for this container
    views: ['album', 'artist', 'track',
      'genre', 'playlist', 'componist'
    ]

    
  });

  return MasterPlaylist;
  
});