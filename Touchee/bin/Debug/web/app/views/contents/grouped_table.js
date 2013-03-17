define([
  'jquery',
  'underscore',
  'Backbone',
  'views/contents/scroll_list'
], function($, _, Backbone, ScrollListView) {
  
  var GroupedTableView = ScrollListView.extend({
    

    // ScrollList properties
    dummy:        '<li><table><tr><td colspan="4">&nbsp;</td></tr></table></li>',
    listType:     'grouped_table',
    innerTagName: 'ul',
    showIndex:  true,
    quickscroll:  true,
    selectable:   'tr:not(.index)',
    floatingIndex:  '<div><img/><span></span></div>',

    // The columns to show
    columns:  ['id'],
    // The attribute or function used for grouping
    groupBy:  'id',




    // ScrollList overrides
    // --------------------
    
    // Renders a set of items
    renderItems: function(items) {

      // Get the first and last groups
      var firstGroupIdx = this.indexToGroup[items.first],
          lastGroupIdx = this.indexToGroup[items.first + items.count - 1];

      // Modify the given object to reflect the actual rendering
      items.first = this.indexToGroup.indexOf(firstGroupIdx),
      items.count = this.indexToGroup.lastIndexOf(lastGroupIdx) - items.first + 1;
      
      // var firstGroupInViewIdx = this.indexToGroup(items.firstInView);

      // Go through all groups that should be rendered
      var rendered = "", view = this;
      for (var i = firstGroupIdx; i <= lastGroupIdx; i++) {

        // Get the group
        var group = this.groups[i];
        
        // Render the base
        var margin = Math.max(3 - group.length, 0) * this.calculated.size.height;
        rendered += '<li style="padding-bottom:' + margin + 'px">';

        // Image
        switch(items.block.idxIdx) {
          // case i:
          //   rendered += '<img class="hidden" />';
          //   break;
          // case i - 1:
          //   var a = Math.max(
          //     0,
          //     10 - (items.block.height + items.block.blockHeight - items.scrollTop)
          //   );
          //   rendered += '<img style="-webkit-transform:translate3d(0,'+a+'px,0)" />';
          //   break;
          default:
            rendered += '<img/>';
            break;
        }

        // Title
        rendered += '<h2 class="index">' + this.getIndex(group[0]) + '</h2><table>';

        _.each(group, function(item, i){
          rendered += '<tr>';
          _.each(view.columns, function(col){
            rendered += "<td>" + view.getAttribute(item, col).toString().htmlEncode() + "</td>";
          });
          rendered += '</tr>';
        });

        rendered += '</table></li>';
      }

      return rendered;
    },


    // Sets the index display
    // VIRTUAL
    setIndexDisplay: function($index, index) {
      $index[0].childNodes[1].innerText = index;
    },


    // Gets the index for the given item
    // VIRTUAL
    getIndex: function(item) {
      return _.isString(this.index)
        ? item.get(this.index)
        : this.index.call(this, item);
    },


    //
    // PRIVATE
    _positionFloatingIndex: function() {

      var lastCalc    = this.data.lastCalc,
          lastRender  = this.data.lastRender;

      // Position the index, and get the resulting offset
      var indexOffset = ScrollListView.prototype._positionFloatingIndex.apply(this, arguments),
          // The index element
          $index      = this.$index,
          // The image in the index
          img         = $index[0].childNodes[0],
          // Calculate the offset of the current index image
          imgOffset   = lastCalc.block.height + lastCalc.block.blockHeight - this.scroller.scrollTop - img.height - 9;

      // Set the image offset
      img.style.webkitTransform = "translate3d(0," + (Math.min(imgOffset, 0) - indexOffset) + "px,0)";

      // The visible element index
      var elIdx     = lastRender.block.idxIdx - lastRender.indices.above,
          // The blocks in view
          $blocks   = this.$inner.children(),
          // The block element corresponding to the current index
          current   = $blocks[elIdx],
          // The previous block
          before    = $blocks[elIdx - 1],
          // The next block
          after     = $blocks[elIdx + 1];

      // Hide the image in the current block and show the others
      current.childNodes[0].style.display = 'none';
      
      if (before) before.childNodes[0].style.display = '';
      if (after)  after.childNodes[0].style.display = '';

      if (after) {
        var b = lastRender.block.height + lastRender.block.blockHeight - this.scroller.scrollTop;
        b = 10 - Math.max(0, Math.min(10, b));
        after.childNodes[0].style.webkitTransform = "translate3d(0,"+b+"px,0)";
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
    },




    // Model querying
    // --------------

    // Gets the model count
    // VIRTUAL
    getCount: function() {
      return this.items.length;
    },


    // Gets the models
    // VIRTUAL
    getModels: function(first, count) {
      return this.items.slice(first, first + count);
    }


  });
  
  return GroupedTableView;

});