define([
  'jquery',
  'underscore',
  'Backbone',
  'models/queue',
  'models/playback',
  'models/artwork',
  'models/collections/devices',
  'views/queue/popup',
  'text!views/browser/_header.html'
], function($, _, Backbone,
            Queue, Playback, Artwork, Devices,
            QueuePopup,
            browserHeaderTemplate) {

  browserHeaderTemplate = _.template(browserHeaderTemplate);

  var BrowserHeaderView = Backbone.View.extend({


    tagName:    'header',
    className:  'not_playing',


    events: {
      'slide #volume':        'volumeSlide',
      'slide #lcd_position':  'positionSlide',
      'tap [data-button]':    'button'
    },


    enabled: false,


    // Constructor
    initialize: function() {
      this.render();

      this.listenTo(Queue, 'sync reset', this.updateHeader);
      this.listenTo(Playback, 'change', this.playbackUpdated);
      this.listenTo(Devices, 'sync reset', this.devicesUpdated);
      this.listenTo(Devices, 'change', this.deviceUpdated);
    },


    // Render
    render: function() {

      // Set base HTML
      this.$el.html(browserHeaderTemplate(this));

      // Set sliders
      this.$volume = this.$('#volume').slider({
        range:  'min',
        min:    0,
        max:    100,
        start:  function() { this._sliding = true; },
        stop:   function() { delete this._sliding; }
      });
      this.$positionSlider = this.$('#lcd_position').slider({
        range:    'min',
        min:      0,
        max:      100,
        start:  function() { this._sliding = true; },
        stop:   function() { delete this._sliding; },
        change: _.bind(this.positionChanged, this) // Somehow this can't be bound through the events
      });

      // Collect buttons
      var buttons = this.buttons = {};
      this.$('[data-button]').each(function(){
        var name = this.getAttribute('data-button');
        buttons['$' + name] = $(this);
      });

      // Collect other elements
      this.$lcd = this.$('#lcd');
      this.$lcdLine1 = this.$('#lcd_line1');
      this.$lcdLine2 = this.$('#lcd_line2');
      this.$position = this.$('#lcd_position_current');
      this.$duration = this.$('#lcd_position_duration');
      this.$artwork = this.$('#lcd_artwork');
    },


    // Toggle enabled state
    enable: function(enabled) {
      this.enabled = enabled;
      this.updateHeader();
      if (!enabled) QueuePopup.hide();
    },


    // Called when the contents of the queue are updated
    updateHeader: function() {
      
      // Get first item
      var first = Queue.first();

      // Enable/disable controls
      this.$el.toggleClass('not_playing', !this.enabled || !first);

      // Set display lines
      this.$lcdLine1.html( first ? first.get('displayLine1$') : "" );
      this.$lcdLine2.html( first ? first.get('displayLine2$') : "" );

      // Toggle buttons
      this.buttons.$prev.prop('disabled', !this.enabled || !Queue.length);
      this.buttons.$play.prop('disabled', !this.enabled || !Queue.length);
      this.buttons.$next.prop('disabled', !this.enabled || Queue.length <= 1);

      // TODO: fill queue popup
      // TODO: set album artwork

      // Get the artwork
      var $artwork = this.$artwork;
      
      var setNoArtwork = function() {
        $artwork.css('backgroundImage', "");
      };
      
      if (first) {
        $artwork.attr('data-item-plugin', first.getPluginKey());
        Artwork.fetch(first, {
          size:     $artwork.width(),
          success:  function(artwork, url, img) {
            if (artwork.exists()) {
              $artwork.css('backgroundImage', 'url(' + url + ')');
            }
          },
          none: setNoArtwork,
          error: setNoArtwork
        });
      }
      else {
        $artwork.removeAttr('data-item-plugin');
        setNoArtwork();
      }


      // TODO : dev line
      // QueuePopup.showRelativeTo( this.buttons['$queue'] );
    },


    // Called when a button is pressed
    button: function(ev) {
      var $button = $(ev.currentTarget),
          button  = $button.attr('data-button');

      if (_.isFunction(Queue[button]))
        Queue[button]();
      else if (_.isFunction(Playback[button]))
        Playback[button]();
      else {
        switch(button) {
          case 'queue':   QueuePopup.showRelativeTo($button); break;
        }
      }
    },


    // Called when someone moves the volume slider
    volumeSlide: function(ev, ui) {
      Devices.getMaster().setVolume(ui.value);
    },


    // Called when devices are updated
    devicesUpdated: function(devices) {
      var master = devices.getMaster();
      if (master) this.deviceUpdated(master);
    },


    // Called when a device is changed server-side
    deviceUpdated: function(device) {
      if (device.isMaster()) {
        if (!this.$volume[0]._sliding)
          this.$volume.slider('value', device.get('volume'));
      }
    },


    //
    playbackUpdated: function() {

      if (!_.isUndefined(Playback.changed.duration)) {
        var duration    = Playback.changed.duration,
            hasDuration = duration > -1;
        this.$duration.html(hasDuration ? String.duration(duration) : "");
        this.$positionSlider.slider('option', 'max', duration);
        this.$positionSlider.slider(hasDuration ? 'enable' : 'disable');
      }

      if (!_.isUndefined(Playback.changed.playing)) {
        var playing = Playback.changed.playing;
        this.buttons.$play.toggle(!playing);
        this.buttons.$pause.toggle(playing);
      }

      if (!_.isUndefined(Playback.changed.position) && !this.$positionSlider[0]._sliding)
        this.$positionSlider.slider('option', 'value', Playback.get('position'));
    },


    //
    positionChanged: function(ev, ui) {
      this.$position.html(ui.value > -1 ? String.duration(ui.value) : "");
    },


    // 
    positionSlide: function(ev, ui) {
      this.positionChanged(ev, ui);
      Playback.setPosition(ui.value);
    }


      


  });

  return new BrowserHeaderView;

});