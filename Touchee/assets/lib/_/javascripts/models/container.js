Container = new JS.Class(ActiveRecordBase, {
  
  extend: {
    __collection__:{}
  },
  
  // Constructor
  initialize: function(container) {
    this.callSuper(container);
  },
  
  // Update container with the given attributes
  update: function(container) {
    this.id           = container.id;
    this.name         = container.name;
    this.type         = container.type;
    this.contentType  = container.contentType
    return this;
  }
  
});