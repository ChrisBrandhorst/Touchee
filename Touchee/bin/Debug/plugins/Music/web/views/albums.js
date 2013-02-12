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
      var $el   = $(ev.target).closest('li'),
          item  = this.getItem($el);
      
      var zoomed = this.zoomTile($el);
      var $details = this.showDetails($el, !zoomed);
      var $content = $details.find('.content').last();
      var view = this;
      
      var colors = new AlbumColors( this.getArtworkUrl(item) );
      var s1 = new Date();
      colors.getColors(function(colors){
        s1 = new Date() - s1;
        colors = _.map(colors, function(c){ return "rgb(" + c.join(',') + ")"; });
        $content[0].innerHTML += view.getBoxes('AlbumColors ('+s1+')', colors);
      });
      
      var artwork = this._getArtwork(item);
      if (!artwork) {
        $details.css('backgroundColor', "");
        return;
      }
      var s2 = new Date();
      colors = ColorTunes.getColors(artwork.image);
      s2 = new Date() - s2;
      $content[0].innerHTML += view.getBoxes('ColorTunes ('+s2+')', [colors.backgroundColor, colors.titleColor, colors.textColor]);
      
      var s3 = new Date();
      var colors = ImageAnalyzer(this.getArtworkUrl(item), function(bgcolor, primaryColor, secondaryColor, detailColor){
        s3 = new Date() - s3;
        $content[0].innerHTML += view.getBoxes('ImageAnalyzer ('+s3+')', ["rgb("+bgcolor+")", "rgb("+primaryColor+")", "rgb("+secondaryColor+")", "rgb("+detailColor+")"]);
      });
      
    },
    
    
    getBoxes: function(name, colors) {
      console.log(colors);
      return '<div class="colors"><em>' + name + '</em><span style="background-color:'+colors[0]+'"></span><span style="background-color:'+colors[1]+'"></span><span style="background-color:'+colors[2]+'"></span><span style="background-color:'+colors[3]+'"></span></div>';
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
      
      // $details.children('.cover').on('webkitTransitionEnd', function(){
      //   $details.find('img')[0].src = item.artworkUrl();
      // });
      
    }
    
    
    
  });
  
  return AlbumsView;
  
});