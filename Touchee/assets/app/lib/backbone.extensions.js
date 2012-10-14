define([
  'Underscore',
  'Backbone'
], function(_, Backbone){
  
  Backbone.readOnlySync = function(method, model, options) {
    if (method != 'read')
      return;
    else
      Backbone.sync.apply(this, arguments);
  };
  
  Backbone.View.prototype.dispose = function(){
    this.remove();
    this.unbind();
    if (typeof this.onDispose == 'function')
      this.onDispose();
  };
  
  Backbone.Collection.prototype.updateAll = function(data) {
    var coll = this, ids = [];
    
    // Create new and update existing media
    _.each(data, function(m) {
      ids.push(m.id);
      var medium = coll.get(m.id);
      if (medium)
        medium.set(m);
      else
        coll.create(m);
    });
    
    // Remove media which are not present anymore
    var toRemove = this.reject(function(m){
      return _.include(ids, m.id);
    });
    this.remove(toRemove);
    
    this.trigger('reset');
    return this;
  };
  
});