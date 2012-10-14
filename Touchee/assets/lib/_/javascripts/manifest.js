var JS_PATH = "resources/javascripts/";
var JS_LIB_PATH = "lib/";
var JS_MODELS_PATH = JS_PATH + "models/";


JS.Packages(function(){with(this){
  
  file(JS_PATH + "application.js")
    .provides('Application')
    .requires('JS.Singleton',
              'Logger')
    .uses(    'Communicator',
              'Navigation');
  
  file(JS_PATH + "logger.js")
    .provides('Logger')
    .requires('JS.Singleton');
  
  file(JS_PATH + "communicator.js")
    .provides('Communicator')
    .requires('JS.Singleton');
  
  file(JS_PATH + "navigation.js")
    .provides('Navigation')
    .requires('JS.Singleton')
    .uses(    'Medium',
              'Container');
  
  file(JS_MODELS_PATH + "active_record_base.js")
    .provides('ActiveRecordBase')
    .requires('JS.Class');
            
  file(JS_MODELS_PATH + "medium.js")
    .provides('Medium')
    .requires('ActiveRecordBase');
  
  file(JS_MODELS_PATH + "container.js")
    .provides('Container')
    .requires('ActiveRecordBase');
    
}});