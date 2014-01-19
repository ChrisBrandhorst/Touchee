define([
  'underscore',
  'Backbone',
  'models/collections/contents_containers'
], function(_, Backbone, ContentsContainers){
  
  var Medium = Backbone.Model.extend({
    
    
    // Constructor
    initialize: function(attributes, options) {
      this.containers = new ContentsContainers([], {medium:this});

      // If the containers collection is changed, trigger event on the medium
      this.listenTo(this.containers, 'set', function(){ this.trigger('set:containers', this); });
    },
    
    
    // Returns whether this medium is a local medium
    isLocal: function() {
      return this.get('type') == 'local';
    }
    
    
  });
  
  
  return Medium;
  
});