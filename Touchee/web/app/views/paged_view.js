define([
  'jquery',
  'underscore',
  'Backbone'
], function($, _, Backbone) {
  
  var PagedView = Backbone.View.extend({
    
    
    // Events
    events: {
      'click button.back, .button.back': 'back'
    },
    
    
    // Initialize the paged view
    initialize: function() {
      this.viewCache = {};
      this.$el.addClass('paged');
    },
    
    
    // Returns true if this view does not contain pages
    isEmpty: function() {
      return _.size(this.viewCache) == 0;
    },
    
    
    // Gets the page corresponding to the given key
    getPage: function(key) {
      return this.viewCache[key];
    },
    
    
    // Stores the given view as page with the given key
    storePage: function(key, view) {
      this.viewCache[key] = view;
      view.el._view = view;
    },
    
    
    // Removes the given view as page
    removePage: function(view) {
      for (i in this.viewCache) {
        if (this.viewCache[i] == view) {
          delete this.viewCache[i];
          view.dispose();
          return true;
        }
      }
      return false;
    },
    
    
    // Activates the given view
    activatePage: function(view, animate) {
      var $pages      = this.$el.children(),
          $firstPage  = $pages.first(),
          _this       = this,
          animate     = animate === false ? false : true,
          $page;
      
      // Get page
      if (view == 'first')      $page = $firstPage;
      else if (view == 'back')  $page = $pages.filter(':not(.prev, .next)').prev();
      else if (view.$el)        $page = view.$el;
      
      // Check if we have a valid page
      if (!$page || !$page.length)
        return this.Log.error("No valid view given in PagedView#activate");
      
      // Get the view from the page
      view = $page[0]._view;
      
      // Check if this view is already active
      if (this.activePage == view) return;
      
      // Get animation duration
      var duration = $page.getAnimDuration();
      
      // Check if page already is present in DOM, so we can go back to it
      if ($page.parent().length) {
        var $nextAll = $page.removeClass('prev').nextAll().addClass('next');
        _.delay(function(){
          $nextAll.each(function(){
            _this.removePage( this._view );
          });
        }, duration);
      }
      
      // Else, if we have no pages, simply add it without animation
      else if (!$pages.length) {
        this.$el.append($page);
      }
      
      // Else, add it as next item
      else {
        view.render();
        $page
          .addClass('next')
          .appendTo(this.$el);
        _.delay(function(){
          $page.removeClass('next').prev().addClass('prev');
        },10);
        _.delay(function(){
          $page.prev()
            .find('.selected')
            .removeClass('selected');
        }, duration);
        
      }
      
      this.activePage = view;
      view.trigger('activated');
    },
    
    
    // Goes back one page
    back: function() {
      this.activatePage('back');
    },
    
    
    // Renders the view
    render: function() {
      var $pages = this.$el.children();
      
      var lastView;
      for (var i = 0; i < $pages.length; i++) {
        var view = $pages.eq(i).data('__view'),
            model = view.model;
        
        if (!model.collection) {
          this.activatePage(lastView);
          break;
        }
        
        lastView = view;
      }
      
    }
    
    
  });
  
  return PagedView;
  
});