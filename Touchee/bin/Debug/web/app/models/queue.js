define([
  'underscore',
  'Backbone',
  'models/collections/media'
], function(_, Backbone, Media){
  
  
  // Filter object
  var Queue = Backbone.Collection.extend({
    
    
    url: "queue",
    
    
    // Constructor
    initialize: function(params) {
    },
    
    
    // 
    parse: function(response) {
      var models = [];
      _.each(response.items, function(itemData){
        var container = Media.get(itemData.mediumID).containers.get(itemData.containerID);
        var model = new container.contentsItemModel(itemData.item);
        models.push(model);
      });
      this.priorityCount = response.priorityCount;
      return models;
    },
    
    
    // Resets the queue to the given model(s) and starts playback from the
    // first item of the new queue or the item given through options.startAt
    resetAndPlay: function(model, options) {
      options || (options = {});
      var attrs = { shuffle: options.shuffle };
      if (_.isNumber(options.start)) attrs.start = options.start;
      QueueCommand.execute('reset', model.url(), attrs);
    },


    // Adds the given model(s) right after the currently playing item
    prioritize: function(model) {
      QueueCommand.execute('prioritize', model.url());
    },


    // 
    push: function(model) {
      QueueCommand.execute('push', model.url());
    },


    // Basic controls
    play: function() { QueueCommand.execute('play'); },
    pause: function() { QueueCommand.execute('pause'); },
    next: function() { QueueCommand.execute('next'); },
    prev: function() { QueueCommand.execute('prev'); }
    
    
  });






  var QueueCommand = Backbone.Model.extend({

    url: function() { return "queue"; },

    save: function(attributes, options) {
      options || (options = {});
      options.wait = true;
      options.url = [this.url(), options.action, options.path].join('/');
      Backbone.Model.prototype.save.call(this, attributes, options);
    }

  }, {

    execute: function(action, path, attributes) {
      var options = { action: action };
      if (_.isString(path)) 
        options.path = path;
      else
        attributes = path;
      new QueueCommand().save(attributes || {}, options);
    }
  });




  return Touchee.Queue = new Queue;
  
});