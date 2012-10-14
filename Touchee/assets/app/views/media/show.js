define([
  'jquery',
  'Underscore',
  'Backbone',
  'models/collections/media',
  'text!views/media/show.html',
  'views/containers/list'
], function($, _, Backbone, Media, showTemplate, ContainersListView) {
  showTemplate = _.template(showTemplate);
  
  var MediumShow = Backbone.View.extend({
    
    // Constructor
    initialize: function(options) {
      this.contentType = options.contentType;
      this.isRootView = this.model.isLocal() && !this.contentType;
      
      // Build empty template
      this.$el = $(showTemplate(
        _.extend(this.model, {contentType:this.contentType})
      ));
      this.el = this.$el[0];
      
      // Build inner containers list
      this.containersList = new ContainersListView({
        collection:   this.model.containers,
        contentType:  this.contentType
      });
      this.$el.append(this.containersList.$el);
      
      // Render page if model is changed
      this.model.on('change', this.render, this);
      
      // Render local medium collection list if some items are removed from the collection
      if (this.isRootView)
        this.model.collection.on('remove', this.renderContainersList, this);
    },
    
    // Renders the medium show, including containers list
    render: function() {
      this.$el.contents().first().text( this.contentType ? this.contentType.toTitleCase() : this.model.name );
      this.containersList.render();
      return this;
    },
    
    // Render only the containers list
    renderContainersList: function() {
      this.containersList.render();
    },
    
    // Releases events
    onDispose: function() {
      this.containersList.dispose();
      this.model.off('change', this.render);
      if (this.isRootView)
        this.model.collection.off('remove', this.renderContainersList);
    }
    
  });
  
  
  return MediumShow;
});