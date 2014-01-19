define([
  'underscore',
  'Backbone',
  'models/contents',
  'models/item'
], function(_, Backbone){
  
  var Container = Backbone.Model.extend({
    
    // The URL for this container
    url:            null,

    // Default attribute values for this container
    defaults: {
      contentType:  "undefined_content_type",
      name:         "undefined_name",
      views:        ["undefined_view"]
    }
    
  });
  
  return Container.including(Backbone.SmartGet);

});