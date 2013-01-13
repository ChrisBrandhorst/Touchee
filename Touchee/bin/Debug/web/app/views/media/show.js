define([
  'jquery',
  'underscore',
  'Backbone',
  'views/paged/page',
  'text!views/media/show.html'
], function($, _, Backbone, PageView, mediaShowTemplate) {
  mediaShowTemplate = _.template(mediaShowTemplate);
  
  var MediaShowView = PageView.extend({
    
    
    // Backbone View options
    tagName:    'nav',
    className:  'list scrollable icons',
    
    
    // Constructor
    initialize: function(options) {
      PageView.prototype.initialize.apply(this, arguments);
      this.model.on('change', this.render, this);
    },
    
    
    // Gets the header for this page
    getHeader: function() {
      return this.model.get('name');
    },
    
    
    getBackButton: function() {
      return '';
    },
    
    
    // Renders the list
    render: function() {
      PageView.prototype.render.apply(this, arguments);
      
      // Render the list while keeping the scroll position and selected item
      var $oldItems = this.$el.children(),
          $selected = $oldItems.filter('.selected'),
          $newItems = $( mediaShowTemplate(this) );
      this.$el.append($newItems);
      $oldItems.remove();
      if ($selected.length)
        this.$el.find('[href=' + $selected.attr('href') + ']').addClass('selected');
      
      // Set select scrolling
      this.$el.touchscrollselect();
    }
    
    
  });
  
  return MediaShowView;
  
});