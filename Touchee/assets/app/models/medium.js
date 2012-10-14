define([
  'Underscore',
  'Backbone',
  'models/collections/containers'
], function(_, Backbone, Containers){
  
  var Medium = Backbone.Model.extend({
    
    sync: Backbone.readOnlySync,
    
    initialize: function() {
      // Build conatiners collection
      this.containers = new Containers({medium:this});
      
      // If the containers collection is reset, trigger event for rendering the local medium page
      if (!this.isLocal()) {
        this.containers.on('reset', function(){
          // alert('containers reset from medium: ' + this.get('name'));
          this.collection.getLocal().containers.trigger('reset');
        }, this);
      }
      
      // Get containers belonging to this model
      this.containers.fetch();
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