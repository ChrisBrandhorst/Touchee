Navigation = new JS.Singleton({
  
  // Template for navigation pages
  pageTemplate:  '<div><h1>&nbsp;</h1><div class="scrollable"></div></div>',
  
  // Initializes the navigation
  initialize: function() {
    this.$element = $('#browser > div > nav');
    this.$rootPage = $(this.pageTemplate).appendTo(this.$element).addClass('root');
    this.firstLoad = true;
  },
  
  // Sets the available media
  setMedia: function(data) {
    
    // Destroy media that are not present anymore
    Medium.each(function(id,medium){
      if (!data[id])
        medium.destroy();
    });
    
    // Create or update remaining media
    for(i in data) {
      var existed = Medium.find(data.id),
          medium = Medium.createOrUpdate(data[i]);
      if (!existed)
        App.getContainers(medium);
    }
    
    // 
    this.updateRootPage();
    
    // TODO: if currently open medium / group page does not exist anymore, browse to the beginning
  },
  
  
  // Sets the received containers
  setContainers: function(containers) {
    var medium = Medium.find(containers.mediumID);
    if (!medium) return;
    medium.setContainers(containers.items);
    this.updatePage(medium);
    
    // TODO: if currently open medium / group page does not exist anymore, browse to the beginning
  },
  
  
  updatePage: function(medium) {
    
    var $page     = this.$element.children('[data-medium-id=' + medium.id + ']'),
        $scroller = $page.find('.scrollable'),
        $oldItems = $scroller.children()
    
    this.updateRootPage();
    
    if (!$page.length || medium.isLocal()) return;
    
    var newItems = [];
    for(i in medium.containers) {
      var container = medium.containers[i];
      newItems.push(
        $('<a href="#" class="container ' + container.type + ' ' + container.contentType + '" data-id="' + container.id + '">' + container.name + '</a>')[0]
      );
    }
    
    $scroller.prepend(newItems);
    $oldItems.remove();
  },
  
  // 
  updateRootPage: function() {
    
    // Position all media in the root page
    var newItems = [];
    Medium.each(function(id,medium){
      if (medium.isLocal()) {
        this.$rootPage.attr('data-medium-id', medium.id).find('h1').html(medium.name);
        for(i in medium.containers) {
          var container = medium.containers[i];
          newItems.push(
            $('<a href="#" class="container ' + ' ' + container.type + ' ' + container.contentType + '" data-id="' + container.id + '">' + container.name + '</a>')[0]
          );
        }
      }
      else if (medium.containers.length > 1) {
        newItems.push(
          $('<a href="#" class="medium more ' + medium.type + '" data-id="' + medium.id + '">' + medium.name + '</a>')[0]
        );
      }
      else if (medium.containers.length == 1) {
        var container = medium.containers[0];
        newItems.push(
          $('<a href="#" class="container ' + container.type + ' ' + container.contentType + '" data-id="' + container.id + '">' + container.name + '</a>')[0]
        );
      }
    }, this);
    
    // Replace old by new media, retaining the selected item
    var $scroller   = this.$rootPage.find('.scrollable'),
        $oldItems   = $scroller.children(),
        selectedID  = $oldItems.filter('.selected');
    $scroller.prepend(newItems);
    $oldItems.remove();
    
  }
  
  
  
});