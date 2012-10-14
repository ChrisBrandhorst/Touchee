define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/base',
  'views/contents/quickscroll'
], function($, _, Backbone, ContentsBase, Quickscroll) {
  
  var ScrolllistBase = ContentsBase.extend({
    
    
    // Objects for storing options
    scrolllistOptions:    {},
    touchScrollOptions:   {},
    
    
    // Renders the base of a scroll list view
    render: function() {
      
      // Set base
      this.$el.html(this.template(
        _.extend({}, this.contents, {view:this})
      ));
      this.$header  = this.$('> header');
      this.$content = this.$('> div');
      
      // If we have recieved content, set the scrolllist, touchscrollselect and quickscroll
      if (this.contentsHasBeenUpdated) {
        
        // Set count if requested
        if (this.showCount) {
          var count = this.contents.get('data').length;
          this.$content.append(
            ['<div class="count">', count, ' ', T.t('items.' + this.contents.get('type'), {count:count}).toTitleCase()].join('')
          );
        }
        
        // Set scroll list
        this.$content.scrolllist(this.scrolllistOptions);
        
        // Set touch scroll select
        this.$content.touchscrollselect(this.touchScrollOptions);
        
        // Set quickscroll
        if (typeof this.quickscroll == 'function') {
          var qs = new Quickscroll({
            alpha:    this.contents.meta && this.contents.meta.sortedByAlpha,
            callback: _.bind(this.quickscroll, this)
          });
          qs.render();
          qs.$el.appendTo(this.$el);
        }
        
      }
      
    },
    
    
    // Update the view
    update: function() {
      ContentsBase.prototype.update.apply(this, arguments);
      
      // If we have not recieved any content yet
      if (!this.contentsHasBeenUpdated) {
        
        // Do not do this again
        this.contentsHasBeenUpdated = true;
        
        // Re-render the view
        this.render();
        
      }
      
      // Update the content by re-rendering the scrolllist
      else
        this.$content.scrolllist('render', true);
    },
    
    
    // Scroll to the given position of the content
    quickscroll: function(pos) {
      this.$content.scrolllist('scrollTo', typeof pos == 'string' ? {index:pos} : {fraction:pos});
    }
    
    
  });
  
  return ScrolllistBase;
  
});