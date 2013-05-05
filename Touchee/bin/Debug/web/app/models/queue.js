define([
  'underscore',
  'Backbone',
  'models/collections/media'
], function(_, Backbone, Media){
  
  
  // Queue object
  var Queue = Backbone.Collection.extend({
    
    
    // URL
    url: "queue",
    
    
    // 
    parse: function(response) {
      var models = [];
      _.each(response.items, function(itemData){
        var container = Media.get(itemData.mediumID).containers.get(itemData.containerID);
        var model = new container.contentsItemModel(itemData.item);
        models.push(model);
      });
      this.priorityCount = response.priorityCount;
      this.repeat = response.repeat;
      this.shuffle = response.shuffle;
      return models;
    },
    
    
    
    
    // Action calls
    // ------------
    
    // Resets the queue to the given model(s) and starts playback from the
    // first item of the new queue or the item given through options.startAt
    resetAndPlay: function(model, options) {
      options || (options = {});
      var attrs = { shuffle: options.shuffle };
      if (_.isNumber(options.start)) attrs.start = options.start;
      this._sendCommand('reset', model.url(), attrs);
    },


    // Adds the given model(s) right after the currently playing item
    prioritize: function(model) {
      this._sendCommand('prioritize', model.url());
    },


    // 
    push: function(model) {
      this._sendCommand('push', model.url());
    },


    // 
    next: function() {
      this._sendCommand('next');
    },


    //
    prev: function() {
      this._sendCommand('prev');
    },


    //
    shuffle: function() {},


    // Repeat
    repeat: function() {
      var mode;
      switch(this.repeat) {
        case 'off': mode = 'all'; break;
        case 'all': mode = 'one'; break;
        case 'off': mode = 'all'; break;
      }
      this._sendCommand('repeat', model.url(), {mode:repeat});
    },



    // Internals
    // ---------

    // 
    _sendCommand: function(command, path, data) {
      var params = {};
      if (_.isObject(path)) {
        data = path;
        path = null;
      }
      return Backbone.ajax({
        url:  _.compact([_.result(this, 'url'), command, path]).join('/'),
        type: 'PUT',
        data: data
      });
    }
    
    
  });


  return Touchee.Queue = new Queue;
  
});