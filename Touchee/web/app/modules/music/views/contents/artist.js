define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/table_base'
], function($, _, Backbone, TableBase) {
  
  var ArtistsList = TableBase.extend({
    
    events: _.extend({}, TableBase.prototype.events, {
      'click .action.albums': 'clickedAllAlbums'
    }),
    
    // Constructor
    initialize: function(options) {
      var artworkURL = options.contents.container.getArtworkUrl({size:'small', item:"artist:"});
      
      options.scrolllistOptions = {
        showID:     true,
        renderItem: function(id, data, odd) {
          return [
            '<tr ', (odd ? 'class="odd" ' : ''), 'data-', options.contents.idAttribute, '="', data[0], '">',
            '<td><img src="', artworkURL, encodeForFilter(data[0]), '" onload="this.style.display=\'inline-block\'" />',
            // '<td style="background-image:url(\'', artworkURL, encodeForFilter(data[0]), '\')">',
            '</td><td>',
            (data[0] || T.T.unknown).htmlEncode(),
            '</td><td>',
            data[1], ' album', (data[1] == 1 ? '' : 's'), ', ',
            data[2], ' track', (data[2] == 1 ? '' : 's'),
            '</td></tr>'
          ]
        }
      };
      TableBase.prototype.initialize.apply(this, arguments);
    },
    
    // 
    render: function() {
      TableBase.prototype.render.apply(this, arguments);
      this.$content.prepend('<div class="action albums">All albums</div>');
    },
    
    // 
    clickedAllAlbums: function() {
      Backbone.history.loadUrl(this.contents.getUrl(""));
    }
    
    
  });
  
  
  return ArtistsList;
});