define([
  'underscore',
  'Touchee',
  './contents_module',
  'i18n!./nls/locale'
], function(_, Touchee, MusicContentsModule, Locale) {
  
  var MusicPlugin = Touchee.Plugin.extend({
    
    name:   "Music",
    module: new MusicContentsModule,
    locale: Locale,
    css:    'full'
    
  });
  
  return new MusicPlugin;
  
});