Media = new JS.Singleton({
  
  // Media types
  Types: {
    LOCAL:        'local',
    // AUDIO_CD:     'audio_cd',
    // FILE_STORAGE: 'file_storage'
  },
  
  // Template for navigation pages
  navPageTemplate:  '<div><h1></h1><div class="scrollable"></div></div>',
  
  // Initializes the media object
  initialize: function() {
    this.$element = $('#browser > div > nav');
  },
  
  // Update is always pushed
  update: function(media) {
    
    // Check if we have a first nav page. If not, build it
    var $rootPage = this.$element.children();
    if (!$rootPage.length)
      $rootPage = $(this.navPageTemplate).appendTo(this.$element).addClass('root');
    
    // Loop through media and add to new media array
    var newMedia = [];
    for(i in media.items) {
      var medium = media.items[i];
      // Set local medium
      if (medium.type == this.Types.LOCAL) {
        $rootPage.attr('data-medium-id', medium.id).find('h1').html(medium.name);
        this.localMedium = medium;
      }
      // Other media
      else
        newMedia.push( 
          // $('<a href="#"/>').attr('data-id', medium.id).html(medium.name).addClass('medium ' + medium.type)[0]
          $('<a href="#" class="medium ' + medium.type + '" data-id="' + medium.id + '">' + medium.name + '</a>')[0]
        );
    }
    
    // Replace old by new media, retaining the selected item
    var $oldMedia   = $rootPage.find('.medium'),
        selectedID  = $oldMedia.filter('.selected').attr('data-id');
    $rootPage.find('.scrollable').append(newMedia);
    $rootPage.find('.medium[data-id=' + selectedID + ']').addClass('selected');
    $oldMedia.remove();
    
    // Store data
    this.data = media;
  },
  
  // Gets the containers for the local medium
  getLocalContainers: function() {
    if (!this.localMedium) return;
    Containers.get(this.localMedium);
  }
  
});