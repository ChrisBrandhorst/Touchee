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
    
    
    // Events
    events: {
      'tap a':    'hide'
    },
    
    
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
    },
    
    
    // After an item is clicked that does not have a 'more' class, hide the popup
    clickedItem: function(ev) {
      if (!$(ev.target).closest('a').hasClass('more'))
        this.hide();
    }
    
    
  });
  
  return new MediaPopupView;
  
});