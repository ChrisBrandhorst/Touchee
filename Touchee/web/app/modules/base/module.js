define([
  'underscore',
  'Backbone',
  'Touchee',
  'models/contents',
  'views/contents/table_base'
], function(_, Backbone, Touchee, Contents, TableBaseView) {
  
  
  // Touchee.Module
  // -----------------
  
  // Modules receive requests for content pages from the user interface and process them
  // specificly for the module type
  var Module = function() {
    this.initialize.apply(this, arguments);
  };
  Module.extend = Backbone.Model.extend;
  
  
  // Set up all inheritable **Touchee.Module** properties and methods.
  _.extend(Module.prototype, Backbone.Events, {
    
    
    // Logger
    Log: Touchee.Log,
    
    
    // Initialize is an empty function by default. Override it with your own
    // initialization logic.
    initialize: function(){},
    
    
    // Get the Contents model for the given type
    getContentsModel: function(type) {
      return Contents;
    },
    
    
    // Get the view for the given
    getContentsView: function(type, contents) {
      return TableBaseView;
    },
    
    
    // Default setContentsView
    setContentsView: function(containerView, itemView) {
      containerView.storePage(itemView.fragment, itemView);
      containerView.activatePage(itemView);
      itemView.render();
    }
    
  });
  
  
  return Module;
  
  
});