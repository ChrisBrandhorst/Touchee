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
      
      // If the containers collection is reset, trigger event for rendering the local medium page
      if (!this.isLocal()) {
        this
          .listenTo(this.containers, 'reset', function(){
            this.trigger('reset:containers', this);
          })
          .listenTo(this.containers, 'update', function(){
            this.trigger('update:containers', this);
          });
      }
    },
    
    
    // Returns whether this medium is a local medium
    isLocal: function() {
      return this.get('type') == 'local';
    }
    
    
  });
  
  
  return Medium;
  
});