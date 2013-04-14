define([
  'jquery',
  'underscore',
  'Backbone',
  'views/paged/pages',
  'views/popup/base',
  'views/media/index',
  'views/media/show'
], function($, _, Backbone, PagesView, PopupView, MediaIndexView, MediumShowView) {
  
  
  var MediaPagesView = new PagesView();
  
  
  var MediaPopupView = PopupView.extend({
    
    
    // Backbone View options
    id: 'media_popup',
    
    
    // 
    render: function() {
      this.$el.append( MediaPagesView.$el );
      MediaPagesView.addPage( MediaIndexView );
      this.resizeToContents();
    },
    
    
    //
    showMedium: function(medium) {
      var mediumShowView = new MediumShowView({model:medium});
      MediaPagesView.addPage(mediumShowView);
      this.resizeToContents();
    },
    
    
    // 
    getRequiredContentHeight: function() {
      var activePageView = MediaPagesView.getActivePage();
      return activePageView.$el.outerHeight() + activePageView.$header.outerHeight();
    }
    
    
  });
  
  return new MediaPopupView;
  
});