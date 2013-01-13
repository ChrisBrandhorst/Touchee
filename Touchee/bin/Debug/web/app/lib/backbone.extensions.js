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
    }
    
  });
  
  
});








// // 
// Backbone.readOnlySync = function(method, model, options) {
//   if (method != 'read')
//     return;
//   else
//     Backbone.sync.apply(this, arguments);
// };
//