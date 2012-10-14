ActiveRecordBase = new JS.Class({
  
  extend: {
    
    // Find by ID
    find: function(id) {
      return this.__collection__[id];
    },
    
    // Create new medium or update existing
    createOrUpdate: function(obj) {
      var existing = this.find(obj.id);
      return existing ? existing.update(obj) : new this(obj);
    },
    
    // Loop through 
    each: function(func, scope) {
      for(id in this.__collection__)
        func.call(scope || this.__collection__[id], id, this.__collection__[id]);
    }
  },
  
  initialize: function(data) {
    if (typeof this.update == 'function')
      this.update(data);
    this.klass.__collection__[this.id] = this;
  },
  
  destroy: function() {
    this.__destroyed__ = true;
    delete this.klass.__collection__[this.id];
  },
  
  isDestroyed: function() {
    return !!this.__destroyed__;
  }
  
});