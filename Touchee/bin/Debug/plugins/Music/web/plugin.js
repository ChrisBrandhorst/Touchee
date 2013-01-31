define([
  'underscore',
  'Touchee',
  './module',
  'i18n!./nls/locale'
], function(_, Touchee, MusicModule, Locale) {
  
  var MusicPlugin = Touchee.Plugin.extend({
    
    name:   "Music",
    module: new MusicModule,
    locale: Locale,
    css:    'full'
    
  });
  
  return new MusicPlugin;
  
});