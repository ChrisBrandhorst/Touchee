define([
  'jquery',
  'underscore',
  'Backbone',
  'models/collections/media',
  'models/server_info',
  'views/paged/page',
  'text!views/media/index.html'
], function($, _, Backbone, Media, ServerInfo, PageView, mediaIndexTemplate) {
  mediaIndexTemplate = _.template(mediaIndexTemplate);
  
  var MediaIndexView = PageView.extend({
    
    
    // Backbone View options
    model:      Media,
    tagName:    'nav',
    className:  'list scrollable icons',
    
    
    // Constructor
    initialize: function() {
      PageView.prototype.initialize.apply(this, arguments);

      this
        .listenTo(this.model, 'reset update add remove', this.render)
        .listenTo(this, 'back', this.removeSelection);
    },
    
    
    // Gets the header for this page
    getHeader: function() {
      return I18n.models.media.more;
    },
    
    
    // Renders the list
    render: _.debounce(function() {
      PageView.prototype.render.apply(this, arguments);
      
      // Render the list while keeping the scroll position
      var $oldItems = this.$el.children(),
          $selected = $oldItems.filter('.selected'),
          $newItems = $( mediaIndexTemplate(this) );
      
      this.$el.append($newItems);
      $oldItems.remove();
      
      if ($selected.length)
        this.$el.find('[href=' + $selected.attr('href') + ']').addClass('selected');
      
      this.$el.touchscrollselect();
    }, 100),
    
    
    // Removes the selection
    removeSelection: function() {
      this.$el.find('.selected').removeClass('selected');
    }
    
    
  });
  
  return new MediaIndexView;
  
});