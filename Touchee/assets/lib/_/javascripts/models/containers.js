Containers = new JS.Singleton({
  
  // Initializes the conntainers object
  initialize: function() {
    this.$element = $('#browser > div > nav');
  },
  
  // Get the containers for the given medium
  get: function(medium) {
    Application.communicator.get(
      "media/" + medium.id + "/containers",
      cb(this, 'update')
    );
  },
  
  // Called when an update of containers is pushed from the server
  update: function(containers) {
    containers = containers.containers || containers;
    
    // Get the page for the corresponding medium
    var $page = this.$element.children('[data-medium-id=' + containers.mediumID + ']');
    
    // If we got this pushed, but do not have the page yet, do nothing
    if (!$page.length) return;
    
    
    
  },
  
  // 
  render: function(containers) {
    
  }
  
  
  
});