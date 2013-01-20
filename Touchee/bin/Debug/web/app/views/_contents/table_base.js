define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/scrolllist_base',
  'text!views/contents/table_base.html'
], function($, _, Backbone, ScrolllistBase, tableBaseTemplate) {
  
  var TableBase = ScrolllistBase.extend({
    
    
    // View classname
    className: 'view-table',
    
    
    // Events
    events: {
      'click tr:not(.index)': 'clickedRow'
    },
    
    
    // The template to use with rendering
    template: _.template(tableBaseTemplate),
    
    
    // Default touchscroll options for a table view
    touchScrollOptions: {
      selectable:     'tr:not(.index), .action',
      keepSelection:  false
    },
    
    
    // Constructor
    initialize: function(options) {
      ScrolllistBase.prototype.initialize.apply(this, arguments);
      
      // Default scrolllist options for a table view
      var contents = this.contents, view = this;
      this.scrolllistOptions = _.extend({
        count:  function(el, elI) {
          return contents.get('data').length;
        },
        indices: function() {
          return view.indices;
        },
        data:   function(el, elI, first, last) {
          return $.extend(true, [], !_.isNumber(last)
            ? contents.get('data')[first]
            : contents.get('data').slice(first, last + 1)
          );
        }
      }, options.scrolllistOptions);
    },
    
    
    // Clicked on a row in the table
    clickedRow: function(ev) {
      var id = $(ev.target).closest('tr').attr('data-' + this.contents.idAttribute);
      if (!_.isUndefined(id))
        Backbone.history.navigate(this.contents.getUrl(id), {trigger:true});
    },
    
    
    // Gets the title of the page
    getTitle: function() {
      return this.$('header h1').text();
    }
    
    
  });
  
  
  return TableBase;
});