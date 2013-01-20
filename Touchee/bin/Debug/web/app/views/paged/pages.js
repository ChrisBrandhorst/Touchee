define([
  'jquery',
  'underscore',
  'Backbone',
  'views/paged/page'
], function($, _, Backbone, PageView) {
  
  var PagesView = Backbone.View.extend({
    
    
    // Backbone View options
    className:  'pages',
    tagName:    'div',
    
    // Events
    events: {
      'click button.back': 'back'
    },
    
    
    // Custom properties
    pages: [],
    
    
    // Constructor
    initialize: function(options) {
      this.render();
    },
    
    
    // Renders the framework for the paged view
    render: _.once(function() {
      this.$headers = $('<header/>').appendTo(this.$el);
      this.$pages = $('<div/>').appendTo(this.$el);
    }),
    
    
    // Adds a page to the paged view
    addPage: function(view, fragment) {
      
      // Only allow subclasses of PageView
      if (!(view instanceof PageView))
        throw "Only subclasses of PageView can be added as a page";
      
      var hasPages = !!this.pages.length;
      
      // Render the given view
      view.render({backButton:hasPages});
      
      // If we already have pages, set the new page as next page
      if (hasPages) {
        view.$header.addClass('next');
        view.$el.addClass('next');
      }
      
      // Add header and page to DOM
      view.$header.appendTo(this.$headers);
      view.$el.appendTo(this.$pages);
      
      // If we already have pages, animate the page into view
      _.defer(function(){
        if (hasPages) {
          view.$header.removeClass('next').prev().addClass('prev');
          view.$el.removeClass('next').prev().addClass('prev');
        }
      });
      
      // Store page object
      this.pages.push(view);
      view.fragment = fragment;
    },
    
    
    // 
    back: function() {
      // Cannot go back if only one page
      if (this.pages.length <= 1) return;
      
      // Remove page object
      var view = this.pages.pop();
      delete view.fragment;
      
      // Animate page out of view
      view.$header.addClass('next').prev().removeClass('prev');
      view.$el.addClass('next').prev().removeClass('prev');
      
      // 
      this.getActivePage().trigger('back');
      Backbone.history.navigate( this.getActivePage().fragment );
      
      // Remove page
      _.delay(function(){
        view.$header.remove();
        view.$el.remove();
      }, 200);
    },
    
    
    // 
    getActivePage: function() {
      return this.pages[this.pages.length - 1];
    }
    
    
    
    // // Returns true if this view does not contain pages
    // isEmpty: function() {
    //   return _.size(this.viewCache) == 0;
    // },
    // 
    // 
    // // Gets the page corresponding to the given key
    // getPage: function(key) {
    //   return this.viewCache[key];
    // },
    // 
    // 
    // // Stores the given view as page with the given key
    // storePage: function(key, view) {
    //   this.viewCache[key] = view;
    //   view.el._view = view;
    // },
    // 
    // 
    // // Removes the given view as page
    // removePage: function(view) {
    //   for (i in this.viewCache) {
    //     if (this.viewCache[i] == view) {
    //       delete this.viewCache[i];
    //       view.dispose();
    //       return true;
    //     }
    //   }
    //   return false;
    // },
    // 
    // 
    // // Activates the given view
    // activatePage: function(view, animate) {
    //   var $pages      = this.$el.children(),
    //       $firstPage  = $pages.first(),
    //       _this       = this,
    //       animate     = animate === false ? false : true,
    //       $page;
    //   
    //   // Get page
    //   if (view == 'first')      $page = $firstPage;
    //   else if (view == 'back')  $page = this.activePage.$el.prev();//$pages.filter(':not(.prev, .next)').prev();
    //   else if (view.$el)        $page = view.$el;
    //   
    //   // Check if we have a valid page
    //   if (!$page || !$page.length)
    //     return this.Log.error("No valid view given in PagedView#activate");
    //   
    //   // Get the view from the page
    //   view = $page[0]._view;
    //   
    //   // Check if this view is already active
    //   if (this.activePage == view) return;
    //   
    //   // Get animation duration
    //   var duration = $page.getAnimDuration();
    //   
    //   // Check if page already is present in DOM, so we can go back to it
    //   if ($page.parent().length) {
    //     var $nextAll = $page.removeClass('prev').nextAll().addClass('next');
    //     _.delay(function(){
    //       $nextAll.each(function(){
    //         _this.removePage( this._view );
    //       });
    //     }, duration);
    //   }
    //   
    //   // Else, if we have no pages, simply add it without animation
    //   else if (!$pages.length) {
    //     this.$el.append($page);
    //     view.render();
    //   }
    //   
    //   // Else, add it as next item
    //   else {
    //     $page
    //       .addClass('next')
    //       .appendTo(this.$el);
    //     _.delay(function(){
    //       $page.removeClass('next').prev().addClass('prev');
    //     },10);
    //     _.delay(function(){
    //       $page.prev()
    //         .find('.selected')
    //         .removeClass('selected');
    //     }, duration);
    //     view.render();
    //     
    //   }
    //   
    //   this.activePage = view;
    //   view.trigger('activated');
    // },
    // 
    // 
    // // Goes back one page
    // back: function(ev) {
    //   this.activatePage('back');
    //   ev.preventDefault();
    //   return false;
    // },
    // 
    // 
    // // Renders the view
    // render: function() {
    //   var $pages = this.$el.children();
    //   
    //   var lastView;
    //   for (var i = 0; i < $pages.length; i++) {
    //     var view = $pages.eq(i).data('__view'),
    //         model = view.model;
    //     
    //     if (!model.collection) {
    //       this.activatePage(lastView);
    //       break;
    //     }
    //     
    //     lastView = view;
    //   }
    //   
    // }
    
  });
  
  return PagesView;
  
});