define([
  'jquery',
  'underscore',
  'Backbone',
  'models/artwork',
  'views/contents/common_tiles',
  './_album_details'
], function($, _, Backbone, Artwork, CommonTilesView, AlbumDetailsView) {
  
  var AlbumsView = CommonTilesView.extend({
    
    // ScrollList properties
    contentType:    'albums',
    indexAttribute: 'albumArtistSort',
    
    
    // Tiles view properties
    line1:      'album$',
    line2:      'albumArtist$',
    
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