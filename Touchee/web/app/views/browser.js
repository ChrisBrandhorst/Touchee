define([
  'jquery',
  'underscore',
  'Backbone',
  'models/collections/media',
  'views/media/list',
  'views/contents/list',
  'text!views/browser.html'
], function($, _, Backbone,
            Media,
            MediaListView, ContentsListView,
            browserTemplate) {
  
  browserTemplate = _.template(browserTemplate);
  
  
  var BrowserView =  Backbone.View.extend({
    
    
    // Backbone view options
    tagName:    'section',
    id:         "browser",
    className:  'hidden',
    
    
    // Events
    events: {
      'touchstart [data-ontouchstart]': 'followOnDown',
      'click [data-href]':              'followNonAnchor',
      // 'click [data-button]':            'button'
    },
    
    
    // Internal list of container views
    containerViews: {},
    
    
    // Constructor
    initialize: function(params) {
      
      // Immediately render
      this.render();
      
      // Init media list view
      this.mediaListView = new MediaListView({el:this.$('#navigation')[0]});
      
      
    },
    
    
    // Render browser view
    render: function() {
      this.$el.html(
        browserTemplate()
      ).appendTo(document.body);
      this.$containersViews = this.$('> div');
      return this;
    },
    
    
    // Show the browser view
    show: function() {
      this.$el.removeClass('hidden');
      return this;
    },
    
    
    // Navigate the media list
    navigate: function(medium, group, fragment) {
      this.mediaListView.navigate(medium, group, fragment);
    },
    
    
    
    // === Container view handling ===
    
    // Gets the container view for the given container and content type
    getContainerView: function(container, type) {
      var views = this.containerViews[container.id];
      return views ? views[type] : null;
    },
    
    
    // Gets or creates the container view for the given container and content type
    getOrCreateContainerView: function(container, type) {
      var views = this.containerViews[container.id];
      if (!views)
        views = this.containerViews[container.id] = {};
      
      var view = views[type];
      if (!view)
        view = views[type] = new ContentsListView({
          container:  container,
          type:       type
        });
      return view;
    },
    
    
    // Activates the given container view
    activateContainerView: function(view) {
      if (view == this.activeContainerView)
        return;
      if (!this.containerViews[view.container.id])
        view = this.getOrCreateContainerView(view.container, view.container.type);
      if (!view.$el.parent().length)
        this.$containersViews.append(view.$el);
      
      // We add class hidden instead of hide/show, because the scroll positions are lost otherwise...
      view.$el.removeClass('hidden').siblings('.contents').addClass('hidden');
      this.setViewButtons(view);
      this.activeContainerView = view;
    },
    
    // === / ===
    
    
    
    // === Buttons ===
    
    // Sets the view buttons for the given view
    setViewButtons: function(view) {
      if (view == this.activeContainerView) return;
      this.activeContainerView = view;
      var viewTypes = view.container.get('viewTypes');
      
      var $footer = this.$('> footer');
      $footer.find('> nav, > h1').remove();
      
      if (viewTypes.length == 1)
        $footer.append('<h1>' + T.T.viewTypes[viewTypes[0]] + '</h1>');
      else {
        $nav = $('<nav/>').appendTo($footer);
        _.each(viewTypes, function(v){
          var $button = $('<a class="button" data-ontouchstart="true" />')
            .text( T.T.viewTypes[v] )
            .attr('href', "#" + view.container.url() + "/contents/type:" + v);
          if (view.type == v) $button.addClass('selected');
          $nav.append($button);
        });
      }
      
    },
    
    
    // Handles buttons / links which should be called on touchstart
    followOnDown: function(ev) {
      var $button = $(ev.target).closest('[data-href], [href]');
      Backbone.history.navigate(
        $button.attr('data-href') || $button.attr('href'),
        {trigger:true}
      );
      return false;
    },
    
    
    // Handles tags which are not anchors, but do have a data-href which should be handled
    followNonAnchor: function(ev) {
      Backbone.history.navigate(
        $(ev.target).closest('[data-href]').attr('data-href'),
        {trigger:true}
      );
    },
    
    
    // button: function(ev) {
    //   var button = $(ev.target).closest('[data-button]').attr('data-button')
    //   if (_.isFunction(this[button]))
    //     this[button].apply(this, arguments);
    // },
    
    // === / ===
    
    
    
    // === Transport ===
    
    prev:   function() { Backbone.history.loadUrl("queue/" + 1 + "/prev"); },
    next:   function() { Backbone.history.loadUrl("queue/" + 1 + "/next"); },
    play:   function() { Backbone.history.loadUrl("queue/" + 1 + "/play"); },
    pause:  function() { Backbone.history.loadUrl("queue/" + 1 + "/pause"); },
    
    volume: function() {
      Touchee.setOverlay($('#devices_popup'));
      // $('#devices_popup').withOverlay();
    }
    
    // === / ===
    
  });
  
  return new BrowserView;
});