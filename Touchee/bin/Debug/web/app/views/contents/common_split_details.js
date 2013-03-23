define([
  'jquery',
  'underscore',
  'Backbone'
], function($, _, Backbone) {  
  
  var ArtistView = Backbone.View.extend({
    
    
    // Backbone View options
    tagName:      'section',
    
    // The header (view or template) to use
    header:       null,
    // The details view
    contentView:  null,
    // Which model this view is supposed to show
    viewModel:    null,
    

    // Constructor
    initialize: function(options) {
      this.$header = (this.header instanceof Backbone.View ? this.header.$el : $('<header/>')).appendTo(this.$el);
      this.content = new this.contentView(options);
    },
    
    
    // Renders the header and content
    render: function() {
      if (this.header instanceof Backbone.View)
        this.header.render();
      else
        this.$header.html( 
          _.isString(this.header)
            ? _.template(this.header)(this.model)
            : this.header(this.model) 
        );

      this.content.$el.appendTo(this.$el);
      this.content.render();
    }
    
    
  });
  
  return ArtistView;
  
});