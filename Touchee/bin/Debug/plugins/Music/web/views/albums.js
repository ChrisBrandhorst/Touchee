define([
  'jquery',
  'underscore',
  'Backbone',
  'models/artwork',
  'views/contents/tiles',
  'text!./album_details.html'
], function($, _, Backbone, Artwork, TilesView, albumDetailsTemplate) {
  albumDetailsTemplate = _.template(albumDetailsTemplate);
  
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
    
    
    // Gets the models
    getModels: function(items) {
      return this.model.models.slice(items.first, items.first + items.count);
    },
    
    
    // Gets the unknown value for the given attribute of the model
    getUnknownAttributeValue: function(model, attr) {
      var val = I18n.unknown;
      switch (attr) {
        case 'album':       val += " " + I18n.p.music.models.album.one;   break;
        case 'albumArtist': val += " " + I18n.p.music.models.artist.one;  break;
      }
      return val;
    },
    
    
    // Gets the artwork url for the given item
    getArtworkUrl: function(item, size) {
      return item.artworkUrl
        ? item.artworkUrl( {size: size || this.calculated.size.inner.width} )
        : null;
    },
    
    
    // 
    clickedTile: function(ev) {
      var $el   = $(ev.target).closest('li'),
          item  = this.getItem($el);
      
      // Zoom the tile
      var zoomed = this.zoomTile($el);
      
      // Hide details if we are zooming out
      if (!zoomed) return this.showDetails(false);
      
      // Render the details
      var details = this.showDetails($el);
      
      // Get the artwork
      var artwork = Artwork.fromCache(item);
      
      // Set the colors if the artwork has them
      if (artwork.colors) {
        details.$el.css('backgroundColor', "rgb(" + artwork.colors.background + ")");
        details.$el.find('.prim').css('color', "rgb(" + artwork.colors.foreground + ")");
        details.$el.find('.sec').css('color', "rgb(" + artwork.colors.foreground2 + ")");
      }
      else {
        details.$el.css('backgroundColor', "");
      }
    },
    
    
    
    // 
    getDetailsContent: function(track) {
      return albumDetailsTemplate({
        artwork:  Artwork.fromCache(track),
        tracks:   track.getTracksOfAlbum()
      });
    },
    
    
    //
    setDetailsStyle: function($details, item) {
      
      // $details.children('.cover').on('webkitTransitionEnd', function(){
      //   $details.find('img')[0].src = item.artworkUrl();
      // });
      
    }
    
    
    
  });
  
  return AlbumsView;
  
});