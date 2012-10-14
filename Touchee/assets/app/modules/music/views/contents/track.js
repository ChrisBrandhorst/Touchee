define([
  'jquery',
  'Underscore',
  'Backbone',
  'views/contents/table_base'
], function($, _, Backbone, TableBase) {
  
  var TracksList = TableBase.extend({
    
    // Constructor
    initialize: function(options) {
      
      var contents = options.contents,
          _this = this;
      
      // Filter out number data
      options.scrolllistOptions = {
        data:   function(el, elI, first, last) {
          data = $.extend(true, [], typeof last != 'number'
            ? contents.get('data')[first]
            : contents.get('data').slice(first, last + 1)
          );
          
          if (typeof last != 'number') {
            data.splice(_this.contents.keys.number,1);
            data.splice(_this.contents.keys.albumArtist,1);
          }
          else {
            _.each(data, function(d){
              d.splice(_this.contents.keys.number, 1);
              d.splice(_this.contents.keys.albumArtist, 1);
            });
          }
          
          return data;
        }
      };
      
      if (contents.container.get('type') == 'playlist')
        options.scrolllistOptions.showIndices = false;
      
      TableBase.prototype.initialize.apply(this, arguments);
    }
    
  });
  
  return TracksList;
});