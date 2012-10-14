define([
  'jquery',
  'Underscore',
  'Backbone',
  'views/contents/table_base',
  'text!views/contents/grouped_table_base.html'
], function($, _, Backbone, TableBase, groupedTableBaseTemplate) {
  
  var GroupedTableBase = TableBase.extend({
    
    
    // View classname
    className: 'view-grouped-table',
    
    
    // The template to use with rendering
    template: _.template(groupedTableBaseTemplate),
    
    
    // Events
    events: {
      'click tr': 'clickedRow'
    },
    
    
    // Constructor
    initialize: function(options) {
      
      // Translate
      var _this = this;
      
      options.scrolllistOptions = {
        showIndices:  false,
        count:  function(el, elI) {
          return _this.rowToGroup.length;
        },
        indices: function() {
          return _this.indices;
        },
        render: function(el, elI, items) {
          var html = [], group, item;
          
          var groupStart  = _this.rowToGroup[items.first],
              groupEnd    = _this.rowToGroup[items.last],
              itemIndex   = _this.groupToIndex[groupStart]
          
          for (var i = groupStart; i <= groupEnd; i++) {
            
            group = _this.groups[i];
            html.push(
              '<li><img src="',
              _this.getGroupImage(group),
              '" onerror="this.src=\'app/assets/images/trans.png\';" /><h4 class="index" data-index="',
              group[0][_this.contents.keys.index],
              '">',
              _this.getGroupTitle(group).htmlEncode(),
              '</h4><table>'
            );
            
            _.each(group, function(item){
              html = html.concat(
                _this.getItemRow(item, itemIndex)
              );
              itemIndex++;
            });
            
            html.push('</table></li>');
            
          }
          
          items.first = _this.rowToGroup.indexOf(groupStart);
          items.last  = _this.rowToGroup.indexOf(groupEnd) + _this.groups[groupEnd].length - 1;
          
          return html.join("");
        },
        renderDummy: function() {
          return '<li style="min-height:0"></li>';
        }
      };
      
      TableBase.prototype.initialize.apply(this, arguments);
    },
    
    
    // Functions for retrieving implementation specific data
    getGroupImage: function(group) {
      return "";
    },
    getGroupTitle: function(group) {
      return "Group";
    },
    getItemRow: function(item) {
      return ['<tr><td>', item.join('</td><td>'), '</td></tr>'];
    },
    
    
    // Update the view
    update: function() {
      this.translate();
      TableBase.prototype.update.apply(this, arguments);
    },
    
    
    // Translates the data items into groups
    translate: function() {
      
      // Collect data
      var contents  = this.contents,
          data      = contents.get('data'),
          groupKey  = this.groupKey;
      if (!(groupKey instanceof Array))
        groupKey = [groupKey];
      
      // Make groups based on group key
      var groups =
        _.groupBy(data, function(group){
          return _.inject(groupKey, function(total, key){
            return total + "|" + group[key];
          }, "");
        });
      
      // Set group values in view, for use during rendering
      this.groups = _.values(groups);
      
      // Create row to group mapping and indices collection
      this.rowToGroup = [];
      this.groupToIndex = [];
      this.indices = {indices:[],count:[]};
      var size, index, currentIndex, total = 0;
      
      // Loop through all album
      _.each(this.groups, function(group, i){
        
        // Get how large the group is: minimum is 4 rows
        size = Math.max(group.length, 3) + 1;
        
        // Store at which data index the group starts
        this.groupToIndex[i] = total;
        total += group.length;
        
        // Add the group number to the row-to-group array
        for (var j = 0; j < size; j++)
          this.rowToGroup.push(i);
        
        // Set the index and count for the current group
        currentIndex = group[0][contents.keys.index].toUpperCase();
        if (index != currentIndex) {
          this.indices.indices.push(currentIndex);
          this.indices.count[ this.indices.indices.length - 1] = 0;
          index = currentIndex;
        }
        this.indices.count[ this.indices.indices.length - 1] += size;
        
      }, this);
      
    },
    
    
    // Collects the indices for the data
    collectIndices: function() {
      // This is done in the translate method
    },
    
    
    // Clicked on a row in the table
    clickedRow: function(ev) {
      var id = $(ev.target).closest('tr').attr('data-' + this.contents.idAttribute);
      if (typeof id != 'undefined') Backbone.history.loadUrl(this.contents.getUrl(id) );
    }
    
    
  });
  
  
  return GroupedTableBase;
  
  
});