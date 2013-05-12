using System;
using System.IO;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;

using Touchee;
using SpotiFire;

namespace Spotify {
    
    /// <summary>
    /// 
    /// </summary>
    public class SpotifyHandler : Base {


        #region Singleton

        /// <summary>
        /// Private constructor
        /// </summary>
        SpotifyHandler() { }

        /// <summary>
        /// The singleton instance of the spotify class
        /// </summary>
        public static SpotifyHandler Instance = new SpotifyHandler();

        #endregion



        #region Statics

        static string Cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Touchee", "Spotify", "cache");
        static string Settings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Touchee", "Spotify", "settings");
        static string UserAgent = "Touchee";

        #endregion



        #region Properties

        Session _session;

        Error _loginError;

        #endregion





        /// <summary>
        /// Initialises the Spotify handler
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="key"></param>
        public async Task Init(string username, string password, byte[] key) {
            
            // Create a session
            _session = await SpotiFire.Spotify.CreateSession(key, Cache, Settings, UserAgent);
            _session.ConnectionError += _session_ConnectionError;
            _session.Exception += _session_Exception;
            _session.ConnectionstateUpdated += _session_ConnectionstateUpdated;

            // We don't wait for login
            this.Login("chris.brandhorst", password);
        }


        async void Login(string username, string password) {
            _loginError = await _session.Login(username, password, true);
        }
        async void Login() {
            _loginError = await _session.Relogin();
        }


        void _session_ConnectionstateUpdated(Session sender, SessionEventArgs e) {
            Log("Connection state: " + sender.ConnectionState.ToString());

            // We are now logged in
            if (sender.ConnectionState == ConnectionState.LoggedIn) {
                // TODO: Continue
            }

            // We are not logged in (anymore)
            if (sender.ConnectionState != ConnectionState.LoggedIn) {
                
                // If any of these login errors have occured, we can try again
                if (_loginError == Error.OK ||
                    _loginError == Error.UNABLE_TO_CONTACT_SERVER ||
                    _loginError == Error.OTHER_TRANSIENT ||
                    _loginError == Error.NETWORK_DISABLED ||
                    _loginError == Error.SYSTEM_FAILURE) {
                        this.Login();
                }

                // Else, permanent failure which may be overcome by user interaction
                else {
                    // TODO: broadcast error
                    Log(String.Format("Cannot login: {0}. Will NOT try again", _loginError.ToString()));
                }

            }

        }

        void _session_ConnectionError(Session sender, SessionEventArgs e) {
            throw new NotImplementedException();
        }

        void _session_Exception(Session sender, SessionEventArgs e) {
            throw new NotImplementedException();
        }








    }

}
