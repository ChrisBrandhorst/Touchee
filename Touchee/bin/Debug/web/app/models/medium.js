define([
  'underscore',
  'Backbone',
  'models/collections/containers'
], function(_, Backbone, Containers){
  
  var Medium = Backbone.Model.extend({
    
    
    // Constructor
    initialize: function(attributes, options) {
      // Build conatiners collection
      this.containers = new Containers([], {medium:this});
      
      // If the containers collection is changed, trigger event for rendering the local medium page
      this
        .listenTo(this.containers, 'sync', function(){ debugger;this.trigger('sync:containers', this); })
        .listenTo(this.containers, 'change', function(){ this.trigger('change:containers',  this); });

      // 
      this.containers.fetch();
    },
    
    
    // Returns whether this medium is a local medium
    isLocal: function() {
      return this.get('type') == 'local';
    }
    
    
  });
  
  
  return Medium;
  
});