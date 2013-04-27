define([
  'jquery',
  'underscore',
  'Backbone',
  'text!views/browser/_header.html'
], function($, _, Backbone , browserHeaderTemplate) {
  browserHeaderTemplate = _.template(browserHeaderTemplate);

  var BrowserHeaderView = Backbone.View.extend({


    tagName:    'header',
    className:  'not_playing',


    events: {
      'slide #volume':      'volumeSlide',
      'tap [data-button]':  'button'
    },


    // Constructor
    initialize: function() {
      this.render();
      this.listenTo(Touchee.Queue, 'reset', this.queueUpdated);
    },


    // Render
    render: function() {

      // Set base HTML
      this.$el.html(browserHeaderTemplate(this));

      // Set sliders
      this.$volume = this.$('#volume').slider({
        range:    'min',
        min:      0,
        max:      100
      });
      this.$lcd_position = this.$('#lcd_position').slider({
        range:    'min',
        min:      0,
        max:      100
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
    },


    // Toggle enabled state
    enable: function(enable) {
      // this.$el.toggleClass('disabled', !enable);
      // this.$('button').prop('disabled', !enable);
    },


    // Called when the contents of the queue are updated
    queueUpdated: function(queue) {
      
      // Get first item
      var first = queue.first();

      // Enable/disable controls
      this.$el.toggleClass('not_playing', !first);

      if (first) {

        // Set display lines
        this.$lcdLine1.html( first.get('displayLine1$') );
        this.$lcdLine2.html( first.get('displayLine2$') );

        // Toggle buttons
        this.buttons.$prev.prop('disabled', !queue.length);
        this.buttons.$play.prop('disabled', !queue.length);
        this.buttons.$next.prop('disabled', queue.length <= 1);

      }

      // TODO: fill queue popup

    },


    // Called when a button is pressed
    button: function(ev) {
      var button = $(ev.currentTarget).attr('data-button');

      if (Touchee.Queue[button])
        Touchee.Queue[button]();

    },


    // Called when someone moves the volume slider
    volumeSlide: function(ev, ui) {
      
      console.log(ui.value);
    },


  });

  return new BrowserHeaderView;

});