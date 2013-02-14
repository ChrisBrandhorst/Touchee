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
    getArtworkUrl: function(item, size) {
      return item.artworkUrl
        ? item.artworkUrl( {size: size || this.calculated.size.inner.width} )
        : null;
    },
    
    
    // 
    clickedTile: function(ev) {
      var $el   = $(ev.target).closest('li'),
          item  = this.getItem($el);
      
      var zoomed = this.zoomTile($el);
      if (!zoomed) {
        this.showDetails(false);
        return;
      }
      
      var view = this;
      var defaultDetails = function(){
        var details = view.showDetails($el);
        details.$el.css('backgroundColor', "");
      };
      
      Artwork.fetch(item, {
        size:   250,
        success: function(artwork, img) {
          var details = view.showDetails($el);
          console.log(details.$content.html());
          if (img)
            details.$content.find('img')[0].src = img.src;
          if (artwork.colors) {
            details.$el.css('backgroundColor', "rgb(" + artwork.colors.background + ")");
            details.$el.find('.prim').css('color', "rgb(" + artwork.colors.foreground + ")");
            details.$el.find('.sec').css('color', "rgb(" + artwork.colors.foreground2 + ")");
          }
          else {
            details.$el.css('backgroundColor', "")
          }
        },
        error: defaultDetails,
        none:  defaultDetails
      });
      
    },
    
    
    
    // 
    getDetailsContent: function(item) {
      return albumDetailsTemplate({
        artwork:  Artwork.fromCache(item),
        item:     item
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