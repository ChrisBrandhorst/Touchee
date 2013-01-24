define([
  'jquery',
  'underscore',
  'Backbone'
], function($, _, Backbone) {
  
  var ContentsBase = Backbone.View.extend({
    
    
    // Default view params
    tagName:  'section',
    
    
    // Whether at the bottom of the page, the number of items should be displayed
    showCount: true,
    
    
    // Constructor
    initialize: function(params) {
      this.contents = params.contents;
      this.back = params.back;
      this.fragment = params.fragment;
      
      this.contents.on('change', this.update, this);
      this.on('activated', this.activated, this);
      
      this.$el.addClass('type-' + this.contents.getViewType());
      this.contentsHasBeenUpdated = false;
      
      var $el = this.$el;
      this.loadingTimeout = setTimeout(function(){
        $el.addClass('loading');
      }, 1000);
    },
    
    
    // Release events
    onDispose: function() {
      this.contents.off('change', this.update);
      this.off('activated', this.activated);
    },
    
    
    // Gets the title of the page
    getTitle: function() {
      console.error('NotImplementedException');
      return null;
    },
    
    
    // When the contents is updated
    update: function() {
      clearTimeout(this.loadingTimeout);
      this.$el.removeClass('loading');
      this.collectIndices();
    },
    
    
    // Collects the indices for the data
    collectIndices: function() {
      var contents = this.contents;
      var indices = _.groupBy(contents.get('data'), function(item){
        return item[contents.keys.index].toUpperCase();
      });
      this.indices = {
        indices:  _.keys(indices),
        count:    _.map(indices, function(value, key){ return value.length; })
      };
    }
    
    
  });
  
  
  return ContentsBase;
});