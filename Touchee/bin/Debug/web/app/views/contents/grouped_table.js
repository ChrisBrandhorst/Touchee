define([
  'jquery',
  'underscore',
  'Backbone',
  'models/artwork',
  'views/contents/scroll_list'
], function($, _, Backbone, Artwork, ScrollListView) {
  
  var GroupedTableView = ScrollListView.extend({


    // ScrollList properties
    dummy:          '<li><aside></aside><table><tr><td colspan="4">&nbsp;</td></tr></table></li>',
    listType:       'grouped_table',
    innerTagName:   'ul',
    showIndex:      true,
    quickscroll:    true,
    selectable:     'tr:not(.index)',
    floatingIndex:  '<div></div>',


    // Grouped Table properties
    // The columns to show
    columns:  ['id'],
    // The attribute or function used for grouping
    groupBy:  'id',
    // Whether artwork is to be loaded
    artwork:        true,
    // Override the size of the artwork to be loaded for each tile
    artworkSize:    null,




    // ScrollList overrides
    // --------------------

    // Additional calculation for the size of the artwork
    calculateSizes: function() {
      var $dummy = ScrollListView.prototype.calculateSizes.apply(this, arguments).appendTo(this.$inner);

      // Get the metrics
      var size  = this.calculated.size,
          img   = $dummy[0].childNodes[0];
      size.artwork = {
        height: img.clientHeight,
        width:  img.clientWidth
      };
      
      return $dummy.remove();
    },


    // Renders a set of items
    renderItems: function(items) {

      // Get the first and last groups
      var firstGroupIdx = this.indexToGroup[items.first],
          lastGroupIdx = this.indexToGroup[items.first + items.count - 1];

      // Modify the given object to reflect the actual rendering
      items.first = this.indexToGroup.indexOf(firstGroupIdx),
      items.count = this.indexToGroup.lastIndexOf(lastGroupIdx) - items.first + 1;

      // Go through all groups that should be rendered
      var rendered = "", view = this;
      for (var i = firstGroupIdx; i <= lastGroupIdx; i++) {

        // Get the group
        var group = this.groups[i];
        
        // Render the base
        var margin = Math.max(3 - group.length, 0) * this.calculated.size.height;
        rendered += '<li style="padding-bottom:' + margin + 'px">';

        // Image
        var style = this.getArtworkStyle(group[0], {string:true});
        rendered += '<aside' + (style ? ' style="'+style+'"' : '') + '></aside>';

        // Title
        rendered += '<h2 class="index">' + this.getIndex(group[0]) + '</h2><table>';

        // Items per group
        _.each(group, function(item, i){
          rendered += '<tr>';
          _.each(view.columns, function(col){
            rendered += "<td>" + _.escape(view.getAttribute(item, col)) + "</td>";
          });
          rendered += '</tr>';
        });

        rendered += '</table></li>';
      }

      return rendered;
    },


    // Called when the render is complete
    // Starts the iterative getting of artwork
    // VIRTUAL
    afterRender: function(items) {
      
      // Do nothing if we have no items in view or no artwork provisioning
      if (!items.count || this.artwork === false) return;
      
      // Start setting artwork on the first element in view
      this._setArtworkIter(
        this.indexToGroup[items.first],
        this.indexToGroup[items.first + items.count - 1],
        items.indices.above,
        this.data.lastRender.timestamp
      );
    },


    // Renders the floating index
    // VIRTUAL
    renderFloatingIndex: function() {
      // Render index title
      ScrollListView.prototype.renderFloatingIndex.apply(this, arguments);

      // Render index image
      this.$indexArtwork = $('<aside/>')
        .addClass('scroll_list-' + this.listType + '-index_artwork index')
        .prependTo(this.$el).hide();
      this.indexArtwork = this.$indexArtwork[0];
    },


    // Sets the index display
    // VIRTUAL
    updateFloatingIndex: function($index, index) {
      // Set index title
      ScrollListView.prototype.updateFloatingIndex.apply(this, arguments);

      // Set image
      this.updateFloatingIndexArtwork();
    },


    // Gets the model count
    // VIRTUAL
    getCount: function() {
      return this.items.length;
    },


    // Gets the models
    // VIRTUAL
    getItems: function(first, count) {
      return this.items.slice(first, first + count);
    },


    // Gets the index for the given item
    // VIRTUAL
    getIndex: function(item, idx) {
      return _.isString(this.index)
        ? item.get(this.index)
        : this.index.call(this, item, idx);
    },


    // Gets the index of the item in the items collection for the given rendered element
    // VIRTUAL
    getItemIndexByElement: function(el) {
      var $el             = $(el),
          $group          = $el.closest('li'),
          $groupsBefore   = $group.prevAll();
      return this.data.lastRender.first + $groupsBefore.find('tr').length + $el.prevAll().length;
    },




    // Rendering helpers
    // -----------------

    // Sets the index artwork
    // VIRTUAL
    updateFloatingIndexArtwork: function() {

      // Collect some vars
      var pulled        = this.scroller.scrollTop < 0,
          lastBlock     = this.data.lastCalc.block;
      
      // Set the appropriate image
      var group = this.groups[lastBlock.idxIdx];
      var item = group[0];
      
      var artwork = Artwork.fromCache(item);
      if (artwork && artwork.exists())
        _.extend(this.indexArtwork.style, this.getArtworkStyle(item));
      else
        _.extend(this.indexArtwork.style, {backgroundImage:'',height:'',width:''});

      // Hide index artwork if we are pulling
      this.indexArtwork.style.display = pulled ? 'none' : '';

      // Set index artwork offset
      var artworkOffset = Math.min(
        lastBlock.height + lastBlock.blockHeight - this.scroller.scrollTop - this.indexArtwork.clientHeight + 1 - 10,
        0
      ) + 10;
      this.indexArtwork.style.webkitTransform = "translate3d(0," + artworkOffset + "px,0)";

      // Get the corresponding block elements
      var lastRender  = this.data.lastRender,
          elIdx       = lastBlock.idxIdx - lastRender.indices.above,
          // The blocks in view
          $blocks     = this.$inner.children(),
          // The block element corresponding to the current index
          current     = $blocks[elIdx],
          // The previous block
          before      = $blocks[elIdx - 1],
          // The next block
          after       = $blocks[elIdx + 1];

      // Hide the image in the current block and show the others
      if (current) {
        current.childNodes[0].style.webkitTransform = '';
        current.childNodes[0].className = pulled ? 'current' : 'hidden';
      }
      if (before) before.childNodes[0].className = '';
      if (after)  after.childNodes[0].className = '';

      // Set the offset of the next image
      if (after) {
        var b = lastBlock.height + lastBlock.blockHeight - this.scroller.scrollTop;
        b = 10 - Math.max(0, Math.min(10, b));
        after.childNodes[0].style.webkitTransform = "translate3d(0,"+b+"px,0)";
      }
    },


    // Gets the style used for displaying the artwork of a group
    // VIRTUAL
    getArtworkStyle: function(item, options) {
      options || (options = {});
      var artwork = Artwork.fromCache(item),
          style   = {};

      // If we have any artwork
      if (artwork && artwork.exists() === true) {

        // Get data
        var artworkElementSize  = this.calculated.size.artwork,
            artworkSize         = options.size || this.artworkSize || artworkElementSize.width;

        // The image
        style['background-image'] = "url(" + (options.url || artwork.url({size:artworkSize})) + ")";

        // Square artwork: nothing special
        if (artwork.isSquare()) { }
        // Portrait artwork
        else if (artwork.isPortrait())
          style['width'] = Math.ceil(artworkElementSize.height * artwork.get('ratio')) + 'px';
        // Landscape artwork
        else
          style['height'] = Math.ceil(artworkElementSize.width / artwork.get('ratio')) + 'px';
      }

      // Convert to string representation if requested
      return options.string ? _.asCssString(style) : style;
    },


    // Sets the artwork for the groups iteratively
    _setArtworkIter: function(groupIdx, lastGroupIdx, groupsAbove, timestamp) {
      
      // Remember that we are busy with the current index
      this.data.lastRender.artworkIdx = groupIdx;
      
      // Function for doing the next group
      var view = this, doNext = function() {
        // Bail out if we have a different timestamp (new render) or if all groups in view are done
        if (view.data.lastRender.timestamp != timestamp || groupIdx >= lastGroupIdx) return;
        view._setArtworkIter(
          groupIdx + 1,
          lastGroupIdx,
          groupsAbove,
          timestamp
        );
      };

      // Get the element
      var el = this.$inner[0].childNodes[groupIdx - groupsAbove].childNodes[0];
      
      // See if we already have an image
      if (el.style.backgroundImage) {
        doNext();
      }
      
      // If not, load an image
      else {

        // Get some params
        var item  = this.groups[groupIdx][0];
        
        // Get the artwork
        Artwork.fetch(item, {
          size:     this.artworkSize,
          success:  function(artwork, url, img) {
            // If we have artwork, set it
            if (artwork.exists() === true) {
              _.extend(el.style, view.getArtworkStyle(item, {url:url}));
              view.updateFloatingIndexArtwork();
            }
            // Do the next item
            doNext();
          },
          none:   doNext,
          error:  doNext
        });

      }
    },




    // Attribute value getting
    // -----------------------
    
    // Gets the value for the given attribute for the given model
    // VIRTUAL
    getAttribute: function(model, attr) {
      var val = attr.call ? attr.call(model, model) : model.get(attr);
      return val || "";
    },




    // GroupedTableView stuff
    // ----------------------

    // Generates the necessary group objects
    // VIRTUAL
    contentChanged: function() {

      // Get grouping function
      var groupBy = this.index;
      if (_.isString(this.index)) {
        var attr = this.index;
        groupBy = function(item) { return item.get(attr); };
      }

      // Group!
      this.groups = _.values(this.getGroups(groupBy));

      // Go through all groups
      var indexToGroup = this.indexToGroup = [];
      var items = this.items = [];
      _.each(this.groups, function(group, i) {

        // Get how large the group is: minimum is 3 rows
        var size = Math.max(group.length, 3) + 1;

        // Link item indices to groups
        for (var j = 0; j < size - 1; j++) {
          indexToGroup.push(i);
          items.push(group[
            Math.max(0, Math.min(j, group.length - 1))
          ]);
        }
      });
    },


    // Get the available groups from the model
    // ABSTRACT
    getGroups: function(groupBy) {
      throw("NotImplementedException");
    }


  });
  
  return GroupedTableView;

});