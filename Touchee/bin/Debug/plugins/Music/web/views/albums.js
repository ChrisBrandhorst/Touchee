define([
  'jquery',
  'underscore',
  'Backbone',
  'models/artwork',
  'views/contents/tiles',
  './_album_details'
], function($, _, Backbone, Artwork, TilesView, AlbumDetailsView) {
  
  var AlbumsView = TilesView.extend({
    
    // ScrollList properties
    contentType:    'albums',
    indexAttribute: 'albumArtistSort',
    
    
    // Tiles view properties
    line1:      'album',
    line2:      'albumArtist',
    artworkSize: 250,
    
    
    // Constructor
    initialize: function(options) {
      TilesView.prototype.initialize.apply(this, arguments);
      this.model.on('reset add remove change', this.contentChanged, this);
    },
    
    
    // Gets the model count
    getCount: function() {
      return this.model.length;
    },
    
    
    // Gets the set of models for the given range
    getModels: function(first, count) {
      return this.model.models.slice(first, first + count);
    },
    
    
    // Gets the index of the given item
    getIndex: function(item) {
      return this.model.models.indexOf(item);
    },
    
    
    // Gets the unknown value for the given attribute of the model
    getUnknownAttributeValue: function(model, attr) {
      var val = I18n.unknown;
      switch (attr) {
        case 'album':       val += " " + I18n.p.music.models.album.one;   break;
        case 'albumArtist': val += " " + I18n.p.music.models.artist.one;  break;
        case 'title':       val += " " + I18n.p.music.models.track.one;   break;
      }
      return val;
    },
    
    
    // Gets the artwork url for the given item
    getArtworkUrl: function(item, size) {
      return item.artworkUrl
        ? item.artworkUrl( {size: size || this.calculated.size.inner.width} )
        : null;
    },
    
    
    // When an album is clicked, zoom the tile and show the details
    clickedTile: function(ev) {
      var $el     = $(ev.target).closest('li'),
          zoomed  = this.zoomTile($el);
      this.showDetails(zoomed ? $el : false);
    },
    
    
    // Getss the view for the given detail view
    getDetailsView: function(track, $target) {
      return new AlbumDetailsView({
        model:  track,
        el:     $target[0],
        master: this
      }).render();
    }
    
    
  });
  
  return AlbumsView;
  
});