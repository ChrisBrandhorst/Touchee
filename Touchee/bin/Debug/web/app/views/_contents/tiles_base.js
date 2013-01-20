define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/scrolllist_base',
  'text!views/contents/tiles_base.html'
], function($, _, Backbone, ScrolllistBase, tilesBaseTemplate) {
  
  var TilesBase = ScrolllistBase.extend({
    
    
    // View classname
    className: 'view-tiles',
    
    
    // The template to use with rendering
    template: _.template(tilesBaseTemplate),
    
    
    // Events
    events: {
      'click li': 'clickedTile'
    },
    
    
    // Default touchscroll options for a tile view
    touchScrollOptions: {
      selectable:     'li',
      keepSelection:  false,
      delay:          40
    },
    
    
    // Constructor
    initialize: function(options) {
      ScrolllistBase.prototype.initialize.apply(this, arguments);
      
      // Default scrolllist options for a table view
      var contents = this.contents, view = this;
      var scrolllistOptions = this.scrolllistOptions = _.extend({
        rows:         5,
        showIndices:  false,
        count:  function(el, elI) {
          return contents.get('data').length;
        },
        indices: function() {
          return view.indices;
        },
        data:   function(el, elI, first, last) {
          return $.extend(true, [], typeof last != 'number'
            ? contents.get('data')[first]
            : contents.get('data').slice(first, last + 1)
          );
        },
        renderDummy: function() {
          return '<li><span>dummy</span><span>dummy</span></li>'
        },
        renderItem: function(id, data, odd) {
          
          var item = ['<li data-', contents.idAttribute, '="', data[contents.keys.id], '"><span'];
          if (scrolllistOptions.tileArtworkURL) {
            var artworkItem = encodeForFilter(data[contents.keys.artworkid || contents.keys.id]);
            item.push(' style="background-image:url(', scrolllistOptions.tileArtworkURL, artworkItem, ')"');
          }
          item.push('>');
          if (scrolllistOptions.tileLine1Key)
            item.push( (data[contents.keys[scrolllistOptions.tileLine1Key]] || T.T.unknown).htmlEncode() );
          else
            item.push( (data[contents.keys.name] || T.T.unknown).htmlEncode() );
          item.push('</span>');
          if (scrolllistOptions.tileLine2Key)
            item.push('<span>', (data[contents.keys[scrolllistOptions.tileLine2Key]] || T.T.unknown).htmlEncode(), '</span>');
          item.push('</li>');
          
          return item;
        }
      }, options.scrolllistOptions);
    },
    
    
    // 
    clickedTile: function(ev) {
      var id = $(ev.target).closest('li').attr('data-' + this.contents.idAttribute);
      if (typeof id != 'undefined')
        Backbone.history.navigate(this.contents.getUrl(id), {trigger:true});
    }
    
    
  });
  
  
  return TilesBase;
  
});