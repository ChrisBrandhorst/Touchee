define([
  'underscore',
  'Touchee',
  '../music/contents_module',
  './models/session',
  // './views/config',
  'i18n!./nls/locale'
], function(_, Touchee, MusicContentsModule, Session, Locale) {
  
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

      Session.fetch();

    },


    responseReceived: function(response) {
      var obj;

      if (obj = response.session) {
        Session.set(obj);
      }

    }
    
  });
  
  return new SpotifyPlugin;
  
});