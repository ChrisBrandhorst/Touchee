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
    
    
    // Events
    events: {
      'click a':    'hide'
    },
    
    
    // Constructor
    initialize: function() {
      PopupView.prototype.initialize.apply(this, arguments);
      this.render();
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