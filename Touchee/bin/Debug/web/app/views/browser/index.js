define([
  'jquery',
  'underscore',
  'Backbone',
  'communicator',
  'models/server_info',
  'models/status',
  'views/media/popup',
  'views/browser/_header',
  'views/browser/_views',
  'i18n!nls/locale',
  'text!views/browser/index.html'
], function($, _, Backbone,
            Communicator, ServerInfo, Status,
            MediaPopupView, BrowserHeaderView, BrowserViewsView,
            i18n,
            browserTemplate) {
  
  browserTemplate = _.template(browserTemplate);
  
  
  var BrowserView = new(Backbone.View.extend({
    
    
    // Backbone view options
    tagName:    'section',
    id:         "browser",
    
    
    // Events
    events: {
      'tap [data-button=nav]':  'showNav'
    },
    
    
    // Custom view options
    views:              [],
    selectedContainer:  null,
    selectedView:       null,
    
    
    // Constructor
    initialize: function(params) {
      
      // Immediately render
      this.render();
      
      // Set callbacks from models
      this
        .listenTo(Communicator, 'connected', this.connected)
        .listenTo(Communicator, 'disconnected', this.disconnected)
        .listenTo(ServerInfo, 'change', this.serverInfoChanged)
        .listenTo(Status, 'change', this.statusChanged);
    },
    
    
    // Render browser view
    render: _.once(function() {
      
      // Set HTML
      this.$el
        .html( browserTemplate() )
        .hide()
        .appendTo(document.body);
      
      // Collect elements
      this.$nav = this.$('[data-button=nav]');
      this.$lcd = this.$('#lcd');
      this.$search = this.$('input[name=search]');
      this.$contents = this.$('#contents');
      this.$connecting = this.$('#connecting');
      
      // Set controls as if we are disconnected
      this.disconnected();
      
      // Init the header view
      this.$el.prepend(BrowserHeaderView.$el);

      // Init the views view
      BrowserViewsView = new BrowserViewsView();

      // Show
      this.$el.show();
      
      return this;
    }),
    
    
    
    // Model callbacks
    // ---------------
    
    // Called when the websocket has connected
    connected: function() {
      this.$el.removeClass('disconnected');
      BrowserHeaderView.enable(true);
    },


    // Called when the websocket has disconnected
    disconnected: function() {
      this.$connecting.find('> span').html(
        i18n.browser[ Communicator.connectedCount == 0 ? 'connecting' : 'reconnecting' ].replace('%s', ServerInfo.getName())
      );
      this.$el.addClass('disconnected');
      BrowserHeaderView.enable(false);
    },
    
    // Called when the ServerInfo object has changed
    serverInfoChanged: function() {
    },
    
    // Called when the Status object has changed
    statusChanged: function() {
    },
    
    
    
    
    // ???????????????
    // ---------------
    
    // Called when a container selection is made
    setSelectedContainer: function(container, view) {
      
      // Set the nav button
      this.$nav
        .removeClass()
        .addClass("icon " + container.get('contentType'))
        .children()
        .html(container.get('name') || "&nbsp;");
      
      // Update views view
      BrowserViewsView.update(container, view);
      
      // Remember selected container
      this.selectedContainer = container;
    },



    
    // Subview handling
    // ----------------
    
    // 
    getView: function(fragment) {
      return this.views[fragment];
    },
    
    
    // Sets the given view in the browser
    setView: function(view, container, selectedView) {
      
      // If this view is already selected, do nothing
      if (view == this.selectedView) return;
      this.selectedView = view;

      // Store the view
      this.views[view.fragment] = view;
      
      // Put in DOM and render if not yet
      var inDOM = view.el.parentNode == this.$contents[0];
      if (!inDOM) this.$contents.append(view.$el);
      
      // Show view, hide siblings
      view.$el.show().siblings().hide();
      
      // Render view if new
      if (!inDOM) view.render();

      // Set the selected container
      this.setSelectedContainer(container, selectedView);
    },
    
    
    // Remove the given view from the browser
    removeView: function(view) {
      if (this.views[view.fragment]) {
        // TODO: remove view, unbind UI events, unbind model events
        delete this.views[view.fragment];
      }
    },
    
    
    
    
    // Buttons
    // -------
    
    // Shows the navigation popup
    showNav: function(ev) {
      MediaPopupView.showRelativeTo(ev.target);
    }
    
    
    
    
    // Transport
    // -------
    
  }));
  
  return BrowserView;
});