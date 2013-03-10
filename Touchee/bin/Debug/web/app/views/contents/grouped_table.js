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
    

    // The columns to show
    columns:  ['id'],
    // The attribute or function used for grouping
    groupBy:  'id',




    // ScrollList overrides
    // --------------------
    
    // Renders a set of items
    renderItems: function(items) {

      // Get the first and last groups
      var firstGroupIdx = this.indexToGroup[items.first];
      var lastGroupIdx = this.indexToGroup[items.first + items.count - 1];

      // Modify the given object to reflect the actual rendering
      var first = this.indexToGroup.indexOf(firstGroupIdx);
      items.count = this.indexToGroup.lastIndexOf(lastGroupIdx) - first + 1;
      if (first != items.first) {
        items.count++;
        items.first = first;
      }

      // var firstGroupInViewIdx = this.indexToGroup(items.firstInView);

      // Go through all groups that should be rendered
      var rendered = "", view = this;
      for (var i = firstGroupIdx; i <= lastGroupIdx; i++) {

        // Get the group
        var group = this.groups[i];
        
        // Render the base and title
        var margin = Math.max(3 - group.length, 0) * this.calculated.size.height;
        rendered += '<li style="padding-bottom:' + margin + 'px"><img /><h2 class="index">' + this.getIndex(group[0]) + '</h2><table>';

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


    // Gets the index for the given item
    // VIRTUAL
    getIndex: function(item) {
      return _.isString(this.index)
        ? item.get(this.index)
        : this.index.call(this, item);
    },


//     //
//     // PRIVATE
//     _positionFloatingIndex: function() {
//       var groupIdx  = ScrollListView.prototype._positionFloatingIndex.apply(this, arguments);

//       var elIdx = this.data.lastRender.indices.above - groupIdx;
      
//       var $el   = this.$inner.children().eq(elIdx),
//           $img  = $el.find('img');

//       var min = 10;
//       var max = $el.position().top + $el.outerHeight();
//       var top = $img.position().top;
// console.log(max + " " + top);
//       var newTop = Math.max(top, min);
//       newTop = Math.min(newTop, max - $img.outerHeight());

//       var diff = newTop - top;
      
//       $img.css('-webkit-transform', "translate3d(0," + diff + "px,0)");

//       if (max <= min) {
//         $el = $el.next();
//         $img = $el.find('img');
//         $img.css('-webkit-transform', "translate3d(0," + (min-max) + "px,0)");
//       }
//     },




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