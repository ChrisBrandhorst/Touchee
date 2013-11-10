define([
  'underscore',
  'Touchee',
  '../music/contents_module',
  'i18n!./nls/locale'
], function(_, Touchee, MusicContentsModule, Locale) {
  
  var SpotifyPlugin = Touchee.Plugin.extend({
    
    name:   "Spotify",
    module: new MusicContentsModule,
    locale: Locale,
    css:    'full',

    initialize: function() {
      Touchee.Config.register({
        id:   'spotify',
        name: i18n.t('spotify.spotify'),
        view: 'plugins/spotify/views/config'
      });
    }
    
  });
  
  return new SpotifyPlugin;
  
});