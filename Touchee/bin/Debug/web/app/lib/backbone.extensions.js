define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  
  // Backbone.Singleton
  // -----------------

  // Een singleton resource. Hiervan wordt uitgegaan dat deze altijd op de server bestaat.
  var singleton = Backbone.Model.extend({

    // Als deze ID op null wordt gezet, dan wordt bij een save een POST request gedaan.
    // Anders wordt bij een save een PUT gedaan.
    id: 1,

    // URL aanpassen zodat er nooit een ID wordt aangeplakt
    url: function() {
      var id = this.id;
      this.id = null;
      var str = Backbone.Model.prototype.url.apply(this, arguments);
      this.id = id;
      return str;
    },

    // 
    destroy: function() {
      var id = this.id;
      this.id = 1;
      var ret = Backbone.Model.prototype.destroy.apply(this, arguments);
      this.id = id;
      return ret;
    }

  });
  Backbone.Singleton = function(obj){ return new (singleton.extend(obj)); };
  Backbone.Singleton.prototype = singleton.prototype;


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