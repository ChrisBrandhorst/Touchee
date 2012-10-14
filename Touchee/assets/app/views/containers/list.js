define([
  'jquery',
  'Underscore',
  'Backbone',
  'models/collections/media',
  'text!views/containers/list.html'
], function($, _, Backbone, Media, listTemplate) {
  listTemplate = _.template(listTemplate);
  
  var ContainersList = Backbone.View.extend({
    
    // Constructor
    initialize: function(options) {
      this.contentType = options.contentType;
      
      // Init the list
      this.$el.addClass('scrollable');
      
      // When the underlying collection is reset, re-render this
      this.collection.on('reset', this.render, this);
    },
    
    // Renders the list of containers in the collection
    render: function() {
      var $oldItems = this.$el.children(),
          $selected = $oldItems.filter('.selected'),
          $newList  = $(listTemplate(
            _.extend(this.collection, {contentType:this.contentType})
          ));
      
      // TODO: set selected on new items
      this.$el.append($newList.children());
      $oldItems.remove();
      
      this.$el.touchscrollselect();
      return this;
    },
    
    // Releases events
    onDispose: function() {
      this.collection.off('reset', this.render);
    }
    
  });
  
  
  return ContainersList;
});