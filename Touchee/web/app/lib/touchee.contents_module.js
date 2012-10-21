define([
  'underscore',
  'Backbone'
], function(_, Backbone){
  
  
  // Modules receive requests for content pages from the user interface and process them
  // specificly for the module type
  var ContentsModule = Touchee.ContentsModule = function(options){
    options || (options = {});
    this.initialize.apply(this, arguments);
  };
  
  
  // Set up all inheritable **Touchee.ContentsModule** properties and methods.
  _.extend(ContentsModule.prototype, Backbone.Events, {
    
    
    // Defines which content / view types should be inherited from other modules
    // - { track: true }                        both model and view generic
    // - { genre: {model: true} }               model generic, view custom
    // - { genre: {model:'music'} }             model from music module, view custom
    // - { genre: {model:'music', view: true} } model from music module, view generic
    inheritedTypes: {},
    
    
    // Initialize is an empty function by default. Override it with your own
    // initialization logic.
    initialize: function(){},
    
    
    // Gets the model this module uses for displaying the given container
    // with the given type.
    getContentModelPath: function(container, type) {
      var opts = this.inheritedTypes[type];
      var module = this.name;
      
      if (opts) {
        if (opts === true || opts.model === true)
          module = false;
        else if (typeof opts.model == 'string')
          module = opts.model;
      }
      
      return module
        ? ('modules/' + module + '/models/contents/' + type)
        : 'models/contents';
    },
    
    
    // Gets the view this module uses for displaying the given contents.
    getContentViewPath: function(contents) {
      var type      = contents.getViewType(),
          opts      = this.inheritedTypes[type];
          var module = this.name;
      
      if (opts) {
        if (opts === true || opts.view === true)
          module = false;
        else if (typeof opts.view == 'string')
          module = opts.view;
      }
      
      return module
        ? ('modules/' + module + '/views/contents/' + type)
        : 'views/contents/table_base';
    },
    
    
    // Default setContentPage
    setContentPage: function(containerView, type, filter) {
      var containerIsEmpty  = containerView.isEmpty();
      
      // If there are no pages yet
      // if (containerIsEmpty && filter || !containerIsEmpty && !filter)
      //   return console.error('ShouldNotHappenException');
      
      // Get the container from the view
      var container         = containerView.container,
          contentModelPath  = this.getContentModelPath(container, type);
          _this             = this;
      
      // No content model path? Something went wrong
      if (!contentModelPath)
        return console.error('ShouldNotHappenException');
      
      // Create the contents
      require([contentModelPath], function(Contents){
        
        var contents = new Contents({
          container:  container,
          type:       type,
          filter:     filter
        });
        
        // Get and check content view path
        var contentViewPath = _this.getContentViewPath(contents);
        if (!contentViewPath)
          return console.error('ShouldNotHappenException');
        
        // Get the corresponding view
        require([contentViewPath], function(ContentsItemView){
          
          // Init the view
          var itemView = new ContentsItemView({
            contents:     contents,
            back:         containerIsEmpty ? false : containerView.activePage.contents.getTitle(),
            fragment:     Backbone.history.fragment
          });
          
          // Set the view
          _this.setContentsView(containerView, itemView);
          
          // Load the content
          contents.fetch();
          
        });
        
      });
      
    },
    
    
    // Default setContentsView
    setContentsView: function(containerView, itemView) {
      itemView.render();
      containerView.storePage(itemView.contents.filter.toString(), itemView);
      containerView.activatePage(itemView);
    }
    
    
  });
  
  // Set up inheritance for the contents module
  ContentsModule.extend = Backbone.Model.extend;
  
  return ContentsModule;
  
});
