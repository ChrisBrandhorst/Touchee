define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/table_base'
], function($, _, Backbone, TableBase) {
  
  var ChannelsList = TableBase.extend({
    
    
    // Artists are sorted by alpha num
    alphaNum: true,
    
    
    // Constructor
    initialize: function(options) {
      var contents    = options.contents,
          artworkURL  = contents.container.getArtworkUrl({size:'small',resizeMode:'containAndFill'});
      
      options.scrolllistOptions = {
        showID:     true,
        renderItem: function(id, data, odd) {
          return [
            '<tr ', (odd ? 'class="odd" ' : ''), 'data-', contents.idAttribute, '="', data[contents.keys.id], '">',
            '<td><img src="', artworkURL, data[contents.keys.id], '" onload="this.style.display=\'inline-block\'" />',
            '</td><td>',
            (data[contents.keys.name] || T.T.unknown).htmlEncode(),
            '</td></tr>'
          ]
        }
      };
      TableBase.prototype.initialize.apply(this, arguments);
    }
    
    
  });
  
  
  return ChannelsList;
});