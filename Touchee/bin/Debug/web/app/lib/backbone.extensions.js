define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  
  // View removal callback
  var viewRemove = Backbone.View.prototype.remove;
  Backbone.View.prototype.remove = function(){
    viewRemove.call(this);
    if (_.isFunction(this.onRemove))
      this.onRemove();
    this.trigger('removed');
    return this;
  };
  
  
  // Model and Collection disposal
  Backbone.Model.prototype.dispose = Backbone.Collection.prototype.dispose = function() {
    this.stopListening();
    if (_.isFunction(this.onDispose))
      this.onDispose();
    this.trigger('disposed');
    return this;
  };
  
  
  // Collection extensions
  _.extend(Backbone.Collection.prototype, {
    
    
    // Build method for collections.
    // Like create, but without saving it to the server.
    build: function(model, options) {
      var coll = this;
      options = options ? _.clone(options) : {};
      model = this._prepareModel(model, options);
      if (!model) return false;
      if (options.add)
        coll.add(model, options);
      return model;
    },
    
    // Gets the LinQ enumerator for the collection.
    getEnum: function() {
      return Enumerable.From(this.models);
    }
    
    
  });


  // // Modify extractParameters to not decode URIs when it's not necessary
  // _.extend(Backbone.Router.prototype, {
  //   _extractParameters: function(route, fragment) {
  //     var params = route.exec(fragment).slice(1);
  //     return _.map(params, function(param) {
  //       return param ? decodeURI(param) : null;
  //     });
  //   }
  // });
  
  
});