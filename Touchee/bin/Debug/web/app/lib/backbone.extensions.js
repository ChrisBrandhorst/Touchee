define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  
  // View disposal
  Backbone.View.prototype.dispose = function(){
    this.remove();
    this.unbind();
    if (typeof this.onDispose == 'function')
      this.onDispose();
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


    // When you have an existing set of models in a collection, 
    // you can do in-place updates of these models, reusing existing instances.
    // - Items are matched against existing items in the collection by id
    // - New items are added
    // - matching models are updated using set(), triggers 'change'.
    // - existing models not present in the update are removed unless 'removeMissing: false' is passed.
    // - a collection change event will be dispatched for each add() and remove()
    update : function(models, options) {
      models  || (models = []);
      options || (options = {});

      // Keep track of the models we've updated, cause we're gunna delete the rest if 'removeMissing' is set.
      var updateMap = _.reduce(this.models, function(map, model){ map[model.id] = false; return map },{});

      _.each( models, function(model) {

        if (options.parse) model = this.model.prototype.parse(model);

        var idAttribute = this.model.prototype.idAttribute;
        var modelId = model[idAttribute];

        if ( modelId == undefined ) throw new Error("Can't update a model with no id attribute. Please use 'reset'.");
        
        // Model is updated
        if ( this._byId[modelId] ) {
          var attrs = (model instanceof Backbone.Model) ? _.clone(model.attributes) : _.clone(model);
          delete attrs[idAttribute];
          var model = this._byId[modelId];
          model.set( attrs );
          updateMap[modelId] = true;
        }
        // Model is created
        else {
          this.add( model, options );
        }
      }, this);

      if ( options.removeMissing !== false ) {
        _.select(updateMap, function(updated, modelId){
          // Model is deleted
          if (!updated) this.remove( modelId );
        }, this);
      }

      if (!options.silent) this.trigger('update', this, options);
      return this;
    },


    // Fetch the default set of models for this collection, resetting the
    // collection when they arrive. If `add: true` is passed, appends the
    // models to the collection instead of resetting. If `update: true` is
    // passed, merges the result with the existing collection.
    fetch: function(options) {
      options = options ? _.clone(options) : {};
      if (options.parse === undefined) options.parse = true;
      var collection = this;
      var success = options.success;
      options.success = function(resp, status, xhr) {
        collection[options.update ? 'update' : options.add ? 'add' : 'reset'](collection.parse(resp, xhr), options);
        if (success) success(collection, resp);
      };
      options.error = Backbone.wrapError(options.error, collection, options);
      return (this.sync || Backbone.sync).call(this, 'read', this, options);
    }

  });
  
  
  
  // Sync for taking into account a base URL
  Backbone.origSync = Backbone.sync;
  Backbone.sync = function(method, model, options) {
    var url = _.isFunction(model.url) ? model.url() : model.url;
    if (url) {
      options || (options = {});
      options.url = options.url || T.Config.get('baseURL') + url;
    }
    Backbone.origSync.call(this, method, model, options);
  };
  
});








// // 
// Backbone.readOnlySync = function(method, model, options) {
//   if (method != 'read')
//     return;
//   else
//     Backbone.sync.apply(this, arguments);
// };
//