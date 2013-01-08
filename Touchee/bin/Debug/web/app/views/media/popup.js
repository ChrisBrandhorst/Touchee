define([
  'jquery',
  'underscore',
  'Backbone',
  'views/paged/pages',
  'views/popup',
  'views/media/index',
  'views/media/show'
], function($, _, Backbone, PagesView, PopupView, MediaIndexView, MediumShowView) {
  
  
  var MediaPagesView = new PagesView(); //new (PagesView.extend({  }));
  
  
  var MediaPopupView = PopupView.extend({
    
    
    // Backbone View options
    id: 'media_popup',
    
    
    // Constructor
    initialize: function() {
      PopupView.prototype.initialize.apply(this, arguments);
      this.render();
    },
    
    
    // 
    render: _.once(function() {
      this.$el.append( MediaPagesView.$el );
      MediaPagesView.addPage( MediaIndexView );
    }),
    
    
    //
    showMedium: function(medium) {
      var mediumShowView = new MediumShowView({model:medium});
      MediaPagesView.addPage(mediumShowView);
    }
    
    
  });
  
  return new MediaPopupView;
  
});