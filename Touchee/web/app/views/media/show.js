define([
  'jquery',
  'underscore',
  'Backbone',
  'models/collections/media',
  'text!views/media/show.html',
  'text!views/media/show_header.html',
  'text!views/media/show_containers.html'
], function($, _, Backbone, Media, showTemplate, showHeaderTemplate, showContainersTemplate) {
  showTemplate = _.template(showTemplate);
  showHeaderTemplate = _.template(showHeaderTemplate);
  showContainersTemplate = _.template(showContainersTemplate);
  
  var MediumShow = Backbone.View.extend({
    
    
    // Constructor
    initialize: function(options) {
      this.contentType = options.contentType;
      
      // Check if this is the root view
      this.root = this.model.isLocal() && !this.contentType;
      
      // Bind on medium and/or media changes
      this.model.on('change', this.renderMediumInfo, this);
      (this.root ? this.model.collection : this.model).on('reset:containers update:containers', this.renderContainers, this);
      
    },
    
    
    // Renders the medium
    render: function() {
      
      // Render it
      this.$el
        .toggleClass( 'root', this.root )
        .html( showTemplate(this) );
      
      // Set scrolling select
      this.$el.touchscrollselect();
      
      // Collect elements
      this.$header = this.$('h1');
      this.$containers = this.$('.scrollable');
      
      // Render parts
      this.renderMediumInfo();
      this.renderContainers();
      
      return this;
    },
    
    
    // Render the info of the medium (the header)
    renderMediumInfo: function() {
      this.$header && this.$header.html( showHeaderTemplate(this) );
    },
    
    
    // Render the contents of the medium
    renderContainers: function() {
      if (!this.$containers) return;
      
      // Collect containers and/or groups
      if (this.contentType) {
        this.containers = this.model.containers.getByContentType(this.contentType);
        this.groups = [];
      }
      else {
        this.groups = this.model.containers.groupByContentType();
        if (this.groups.length == 1 && !this.model.isLocal())
          this.containers = groups[0].members;
        else
          this.containers = null;
      }
      
      // Render the list while keeping the scroll position
      var $oldItems   = this.$containers.children(),
          $selected   = $oldItems.filter('.selected'),
          $newItems   = $( showContainersTemplate(this) );
      
      this.$containers.append($newItems);
      $oldItems.remove();
      
      if ($selected.length)
        this.$containers.find('[href=' + $selected.attr('href') + ']').addClass('selected');
    },
    
    
    // Returns the title for this view
    getTitle: function() {
      return this.contentType ? this.contentType.toTitleCase() : this.model.get('name');
    },
    
    
    // Gets the title for the previous page
    getPreviousPageTitle: function() {
      var $prev = this.$el.prev();
      return $prev.length ? $prev[0]._view.getTitle() : null;
    },
    
    
    // Releases events
    onDispose: function() {
      this.model.off('change', this.renderMediumInfo);
      (this.root ? this.model.collection : this.model).off('reset:containers update:containers', this.renderContainers);
    }
    
  });
  
  
  return MediumShow;
});