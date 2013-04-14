define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  
  // Filter object
  var Queue = Backbone.Model.extend({
    
    
    url: function() { return "queue"; },
    
    
    // Constructor
    initialize: function(params) {
      // this.id = 0;
    },



    // Resets the queue to the given model(s) and starts playback from the
    // first item of the new queue or the item given through options.startAt
    reset: function(model, options) {
      options || (options = {});

      var attrs = { shuffle: options.shuffle };
      if (options.start) attrs.start = options.start.id;

      this.save( attrs, {path: model.url(), command: 'reset'} );
    },


    // Adds the given model(s) right after the currently playing item
    unshift: function(model) {
      this.save({}, {path: model.url(), command: 'unshift'});
    },


    // 
    push: function(model) {
      this.save({}, {path: model.url(), command: 'push'});
    },



    
    play: function() {},
    pause: function() {},
    next: function() {},
    prev: function() {},
    
    
    // 
    save: function(attributes, options) {
      options || (options = {});
      options.wait = true;

      options.url = [this.url(), options.command, options.path].join('/');

      Backbone.Model.prototype.save.call(this, attributes, options);
    }
    
    
  });
  
  return Touchee.Queue = new Queue;
  
});