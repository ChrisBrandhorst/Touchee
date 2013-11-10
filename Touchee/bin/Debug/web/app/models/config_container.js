define([
  'underscore',
  'Backbone',
  'models/container'
], function(_, Backbone, Container){
  
  var ConfigContainer = Container.extend({
    
    // The URL for this container
    url:            "config",

    // Default attribute values for this container
    defaults: {
      contentType:  "config",
      name:         i18n.t('config.title'),
      views:        ["config"]
    },

    //
    initialize: function() {
      Touchee.CustomContainers.add(this);
    }
    
  });
  
  return new ConfigContainer;

});