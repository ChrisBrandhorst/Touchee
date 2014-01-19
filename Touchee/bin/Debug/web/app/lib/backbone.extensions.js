define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  

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



  // Module inclusion
  Backbone.Model.include = Backbone.Model.including = Backbone.Collection.include = Backbone.Collection.including = function() {
    var proto = this.prototype;
    _.each(arguments, function(module){
      _.extend(proto, module);
    });
    return this;
  };



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
  var collectionSet = Backbone.Collection.prototype.set;
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
    },

    // Add 'set' trigger
    set: function(models, options) {
      var ret = collectionSet.apply(this, arguments);
      if (!options || !options.silent) this.trigger('set', this, options);
      return ret;
    }
    
  });



  // 
  Backbone.SmartGet = {

    // Override get method for getting custom values for attributes (e.g. "Unknown ...").
    // If the attribute name is suffixed with an $, the display value is retrieved.
    // Implement get$ for extra custom behaviour.
    get: function(attr) {

      // See if a custom value is required
      var custom = attr.charAt(attr.length - 1) == '$';
      if (custom) attr = attr.slice(0, -1);
      
      // First, get the original value and return it if not custom
      var val = Backbone.Model.prototype.get.call(this, attr);
      if (!custom) return val;
      
      // Then go for a custom display if present
      if (this.computed) {
        var comp = this.computed[attr];
        if (_.isFunction(comp))
          val = comp.call(this, val);
        else if (_.isString(comp))
          val = this.get(comp);
      }
      
      // If we have an array now, join it into a single string
      // or set to null if the array is empty
      if (_.isArray(val)) {
        val = val.length == 0 ? null : val.join(", ");
      }

      // If we have nothing yet, try the "unknown ..."
      if (!val) {
        val = i18n.models[ attr ];
        if (_.isObject(val))
          val = val.one;
        if (val)
          val = i18n.unknown + " " + val.toTitleCase();
      }

      return val;
    }

  };


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