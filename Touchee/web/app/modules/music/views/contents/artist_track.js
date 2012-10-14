define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/grouped_table_base'
], function($, _, Backbone, GroupedTableBase) {
  
  var ArtistTracksList = GroupedTableBase.extend({
    
    // Update the view
    update: function() {
      this.groupKey = [this.contents.keys.albumArtist, this.contents.keys.album];
      GroupedTableBase.prototype.update.apply(this, arguments);
    },
    
    // Functions for retrieving implementation specific data
    getGroupImage: function(group) {
      return this.contents.container.getArtworkUrl() + group[0][this.contents.keys.id];
    },
    getGroupTitle: function(group) {
      return group[0][this.contents.keys.album] || T.T.unknown + ' ' + T.T.items.album.one;
    },
    getItemRow: function(track, index) {
      var keys    = this.contents.keys,
          number  = Number(track[keys.number]);
      return [
        '<tr data-', this.contents.idAttribute, '="', index, '"><td>',
        number > 0 ? number + '.' : '',
        '</td><td>',
        track[keys.name].htmlEncode(),
        !this.contents.filter.get('artist') && track[keys.artist] != track[keys.albumArtist] ? ' <span>(' + track[keys.artist].htmlEncode() + ')</span>' : '',
        '</td><td>',
        track[keys.duration],
        '</td></tr>'
      ];
    }
    
  });
  
  return ArtistTracksList;
  
});