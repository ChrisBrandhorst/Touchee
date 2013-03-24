define([
  'jquery',
  'underscore',
  'Backbone',
  'communicator',
  'models/server_info',
  'models/status',
  'views/media/popup',
  'views/_browser_views',
  'i18n!nls/locale',
  'text!views/browser.html'
], function($, _, Backbone,
            Communicator, ServerInfo, Status,
            MediaPopupView, BrowserViewsView,
            I18n,
            browserTemplate) {
  
  browserTemplate = _.template(browserTemplate);
  
  
  var BrowserView = new(Backbone.View.extend({
    
    
    // Backbone view options
    tagName:    'section',
    id:         "browser",
    className:  '',
    
    
    // Events
    events: {
      'click [data-button=nav]':  'showNav'
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
      this.$el.html( browserTemplate() );
      if (!this.el.parentNode)
        this.$el.hide().appendTo(document.body);
      
      // Set sliders
      this.$volume = this.$('#volume').slider({
        range:    'min',
        min:      0,
        max:      100,
        disabled: true
      });
      this.$lcd_position = this.$('#lcd_position').slider({
        range:    'min',
        min:      0,
        max:      100
      });
      
      // Collect elements
      var buttons = this.buttons = {};
      this.$('[data-button]').each(function(){
        var name = this.getAttribute('data-button');
        buttons['$' + name] = $(this);
      });
      this.$lcd = this.$('#lcd');
      this.$search = this.$('input[name=search]');
      this.$contents = this.$('#contents');
      this.$connecting = this.$('#connecting');
      
      // Set controls as if we are disconnected
      this.disconnected();
      
      // Init the views view
      BrowserViewsView = new BrowserViewsView();

      // Show
      this.$el.show();
      
      return this;
    }),
    
    
    // Show the browser view
    show: function() {
      this.$el.removeClass('hidden');
      return this;
    },
    
    
    
    
    // Model callbacks
    // ---------------
    
    // Called when the websocket has connected
    connected: function() {
      this.buttons.$nav.show();
      this.$volume.show(); // TODO: make sure we can actually set the volume at this stage
      this.$connecting.hide();
      this.$contents.show();
      this.$search.show();
    },
    
    // Called when the websocket has disconnected
    disconnected: function() {
      this.$lcd.addClass('disabled');
      this.buttons.$pause.hide();
      this.buttons.$play.show();
      this.buttons.$prev.disable();
      this.buttons.$play.disable();
      this.buttons.$next.disable();
      this.buttons.$airplay.hide();
      this.buttons.$nav.hide();
      this.$volume.hide();
      this.$search.hide();
      this.$contents.hide();
      
      this.$connecting.find('> span').html(
        I18n.browser[ Communicator.connectedCount == 0 ? 'connecting' : 'reconnecting' ].replace('%s', ServerInfo.getName())
      );
      
      this.$connecting.show();
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
      this.buttons.$nav
        .attr('className', "")
        .addClass(container.get('contentType'))
        .children()
        .html(container.get('name'));
      
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
    setView: function(view, options) {
      options || (options = {});
      
      // If this view is already selected, do nothing
      if (view == this.selectedView) return;
      this.selectedView = view;

      // Store the view
      this.views[view.fragment] = view;
      
      // Put in DOM if not yet
      if (view.el.parentNode != this.$contents[0])
        this.$contents.append(view.$el);
      
      // Show view, hide siblings
      view.$el.show().siblings().hide();
      
      // Render if not excluded
      if (options.render !== false)
        view.render();

      // Set the selected container
      // this.setSelectedContainer(view.model.contents.container, view.model.params.view);

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