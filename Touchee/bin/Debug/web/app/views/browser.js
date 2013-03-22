define([
  'jquery',
  'underscore',
  'Backbone',
  'communicator',
  'models/server_info',
  'models/status',
  'views/media/popup',
  'i18n!nls/locale',
  'text!views/browser.html',
  'text!views/_browser_views.html'
], function($, _, Backbone,
            Communicator, ServerInfo, Status,
            MediaPopupView,
            I18n,
            browserTemplate, browserViewsTemplate) {
  
  browserTemplate = _.template(browserTemplate);
  browserViewsTemplate = _.template(browserViewsTemplate);
  
  
  var BrowserView = new(Backbone.View.extend({
    
    
    // Backbone view options
    tagName:    'section',
    id:         "browser",
    className:  '',
    
    
    // Events
    events: {
      'click [data-button=nav]': 'showNav'
    },
    
    
    // Custom view options
    views:              [],
    selectedContainer:  null,
    
    
    // Constructor
    initialize: function(params) {
      
      // Immediately render
      this.render();
      
      // Set callbacks from models
      Communicator.on('connected', this.connected, this);
      Communicator.on('disconnected', this.disconnected, this);
      ServerInfo.on('change', this.serverInfoChanged, this);
      Status.on('change', this.statusChanged, this);
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
      // this.$viewButtons = this.$('#view_buttons');
      // this.$viewName = this.$('#view_name');
      this.$views = this.$('#views');
      
      // Set controls as if we are disconnected
      this.disconnected();
      
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
    setSelectedContainer: function(container, selectedView) {
      
      // Set the nav button
      this.buttons.$nav
        .addClass(container.get('contentType'))
        .html(container.get('name'));
      
      this.$views.html(
        browserViewsTemplate({
          container:    container,
          viewText:     this._getViewText,
          selectedView: selectedView
        })
      );
      
      this.selectedContainer = container;
      
    },
    
    
    // Gets the view text for the given container and view
    _getViewText: function(container, view) {
      var plugin  = container.get('plugin'),
          key     = 'p.'+plugin+'.views.'+view,
          text    = I18n.t(key);
      
      if (key == text) {
        key     = 'p.'+plugin+'.models.'+view;
        text    = I18n.t(key, {count:2});
        if (key == text)
          text = view;
      }
      
      return text.toTitleCase();
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
    
    showNav: function(ev) {
      MediaPopupView.showRelativeTo(ev.target);
    },
    
    
    
    
    // Transport
    // -------
    
  }));
  
  return BrowserView;
});