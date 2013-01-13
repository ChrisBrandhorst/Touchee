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
        this.containers.on('reset', function(){
          this.trigger('reset:containers', this);
        }, this);
        this.containers.on('update', function(){
          this.trigger('update:containers', this);
        }, this);
      }
    },
    
    
    // Returns whether this medium is a local medium
    isLocal: function() {
      return this.get('type') == Medium.Types.LOCAL;
    }
    
    
  });
  
  
  Medium.Types = {
    LOCAL:        'local',
    AUDIO_CD:     'audio_cd',
    FILE_STORAGE: 'file_storage'
  };
  
  
  return Medium;
  
});