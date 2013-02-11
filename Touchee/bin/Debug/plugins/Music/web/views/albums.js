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
    
    
    // Tiles properties
    line1:    'album',
    line2:    'albumArtist',
    
    
    // Constructor
    initialize: function(options) {
      TilesView.prototype.initialize.apply(this, arguments);
      // this.filter = options.filter;
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
    getArtworkUrl: function(item) {
      return item.artworkUrl ? item.artworkUrl({size:this.calculated.size.inner.width}) : null;
    },
    
    
    // 
    clickedTile: function(ev) {
      var $el = $(ev.target).closest('li')
      
      var zoomed = this.zoomTile($el);
      this.showDetails($el, !zoomed);
    },
    
    
    // 
    getDetailsContent: function(item) {
      return albumDetailsTemplate({
        artwork:  this._getArtwork(item),
        item:     item
      });
    },
    
    
    //
    setDetailsStyle: function($details, item) {
      
      var artwork = this._getArtwork(item);
      if (!artwork) {
        $details.css('backgroundColor', "");
        return;
      }
      
      var colors = ColorTunes.getColors(artwork.image);
      $details.css('backgroundColor', colors.backgroundColor);
      
      $details.children('.cover').on('webkitTransitionEnd', function(){
        $details.find('img')[0].src = item.artworkUrl();
      });
      
    }
    
    
    
  });
  
  return AlbumsView;
  
});