define([
  'jquery',
  'underscore',
  'Backbone',
  './../models/view_models/albums',
  './../models/view_models/album',
  'views/contents/common_tiles',
  './album'
], function($, _, Backbone, Albums, Album, CommonTilesView, AlbumView) {
  
  var AlbumsView = CommonTilesView.extend({
    
    // ScrollList properties
    className:      'albums',
    
    
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

      var album = new Album(null, {
        contents: this.model.contents,
        track:    track
      });

      var view = new AlbumView({
        model:  album,
        el:     $target[0]
      });
      view.render();
      return view;
    }
    
    
  });
  
  return AlbumsView;
  
});