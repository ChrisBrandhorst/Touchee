define([
  'jquery',
  'underscore',
  'Backbone',
  './../models/view_models/albums',
  'views/contents/common_tiles',
  './_album_details'
], function($, _, Backbone, Albums, CommonTilesView, AlbumDetailsView) {
  
  var AlbumsView = CommonTilesView.extend({
    
    // ScrollList properties
    className:      'albums',
    indexAttribute: 'albumArtistSort',
    
    
    // Tiles view properties
    line1:        'album$',
    line2:        'albumArtist$',
    

    // Which model this view is supposed to show
    viewModel:    Albums,
    

    // When an album is clicked, zoom the tile and show the details
    clickedTile: function(ev) {
      var $el     = $(ev.target).closest('li'),
          zoomed  = this.zoomTile($el);
      this.showDetails(zoomed ? $el : false);
    },
    
    
    // Getss the view for the given detail view
    getDetailsView: function(track, $target) {
      var view = new AlbumDetailsView({
        model:  track,
        el:     $target[0]
      });
      view.render();
      return view;
    }
    
    
  });
  
  return AlbumsView;
  
});