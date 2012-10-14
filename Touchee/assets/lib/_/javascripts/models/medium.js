Medium = new JS.Class(ActiveRecordBase, {
  
  extend: {
    __collection__:{},
    
    // Media types
    Type: {
      LOCAL:        'local',
      AUDIO_CD:     'audio_cd',
      FILE_STORAGE: 'file_storage'
    }
  },
  
  // Contains containers
  containers: [],
  
  // Constructor
  initialize: function(medium) {
    this.callSuper(medium);
  },
  
  // Update medium with the given attributes
  update: function(medium) {
    this.id   = medium.id;
    this.name = medium.name;
    this.type = medium.type;
    return this;
  },
  
  // 
  destroy: function() {
    for(i in this.containers)
      this.containers[i].destroy();
    this.callSuper();
  },
  
  // 
  isLocal: function() {
    return this.type == Medium.Type.LOCAL;
  },
  
  // 
  setContainers: function(containers) {
    this.containers = [];
    for(i in containers)
      this.containers.push( new Container(containers[i]) );
  }
  
});