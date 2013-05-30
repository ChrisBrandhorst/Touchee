define([
  'underscore',
  'Touchee',
  '../music/module',
  'i18n!./nls/locale'
], function(_, Touchee, MusicModule, Locale) {
  
  var SpotifyPlugin = Touchee.Plugin.extend({
    
    name:   "Spotify",
    module: new MusicModule,
    locale: Locale,
    css:    'full'
    
  });
  
  return new SpotifyPlugin;
  
});