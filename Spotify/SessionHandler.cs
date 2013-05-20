using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
using System.Threading;
using System.Threading.Tasks;


using Touchee;
using SpotiFire;

namespace Spotify {
    
    /// <summary>
    /// 
    /// </summary>
    public class SessionHandler : Base {



        #region Statics

        static string Cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Touchee", "Spotify", "cache");
        static string Settings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Touchee", "Spotify", "settings");
        static string UserAgent = "Touchee";

        #endregion



        #region Privates

        Session _session;

        Error _loginError;

        bool _loggingIn;

        #endregion




        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public SessionHandler() { }

        #endregion



        /// <summary>
        /// Initialises the Spotify handler
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="key"></param>
        public async Task<Session> Init(string username, string password, byte[] key) {
            
            // Create a session
            _session = await SpotiFire.Spotify.CreateSession(key, Cache, Settings, UserAgent);

            // Set props
            _session.PreferredBitrate = BitRate.Bitrate320k;

            // Set callbacks
            _session.ConnectionError += ConnectionError;
            _session.ConnectionstateUpdated += ConnectionstateUpdated;
            
            // We don't wait for login
            this.Login(username, password);

            // But return the session directly
            return _session;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        async void Login(string username, string password) {
            if (!_loggingIn) {
                _loggingIn = true;
                _loginError = await _session.Login(username, password, true);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        async void Login() {
            if (!_loggingIn) {
                _loggingIn = true;
                _loginError = await _session.Relogin();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ConnectionError(Session sender, SessionEventArgs e) {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void ConnectionstateUpdated(Session sender, SessionEventArgs e) {
            Log("Connection state: " + sender.ConnectionState.ToString());
            Log("Loggin error:     " + _loginError.ToString());

            // We are not logged in (anymore)
            if (sender.ConnectionState != ConnectionState.LoggedIn && !_loggingIn) {
                
                // If any of these login errors have occured, we can try again
                // Using a delay of 5 seconds
                if (_loginError == Error.OK ||
                    _loginError == Error.UNABLE_TO_CONTACT_SERVER ||
                    _loginError == Error.OTHER_TRANSIENT ||
                    _loginError == Error.NETWORK_DISABLED ||
                    _loginError == Error.SYSTEM_FAILURE) {
                        new Timer(o => this.Login(), null, 5000, -1);
                }
                    
                // Else, permanent failure which may be overcome by user interaction
                else {
                    // TODO: broadcast error
                    Log(String.Format("Cannot login: {0}. Will NOT try again", _loginError.ToString()));
                }

            }

            _loggingIn = false;

        }



    }

}
