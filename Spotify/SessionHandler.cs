using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Touchee;
using SpotiFire;

namespace Spotify {



    /// <summary>
    /// 
    /// </summary>
    public class SessionHandler : Base {


        #region Privates


        /// <summary>
        /// The most recent login error
        /// </summary>
        Error _loginError;


        /// <summary>
        /// The API key that is to be used
        /// </summary>
        byte[] _key;


        /// <summary>
        /// Whether we are loggin in at the moment
        /// </summary>
        bool _loggingIn;


        #endregion


        /// <summary>
        /// Internal Session storage
        /// </summary>
        internal Session Session;


        #region Constructor


        /// <summary>
        /// Constructs a new SessionHandler instance
        /// </summary>
        /// <param name="key">The API key to use</param>
        public SessionHandler(byte[] key) {
            _key = key;
        }


        #endregion



        #region Initialization


        /// <summary>
        /// Initialises the Spotify Session
        /// </summary>
        /// <returns>The new Session instance</returns>
        public async Task<Session> Init() {
            
            // Create a session
            Session = await SpotiFire.Spotify.CreateSession(
                _key,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Touchee", "Spotify", "cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Touchee", "Spotify", "settings"),
                "Touchee"
            );

            // Set props
            Session.PreferredBitrate = BitRate.Bitrate320k;

            // Set callbacks
            Session.ConnectionError += ConnectionError;
            Session.ConnectionstateUpdated += ConnectionstateUpdated;
            Session.CredentialsBlobUpdated += Session_CredentialsBlobUpdated;
            
            // Login if we have a username and credentials
            if (this.CredentialsStored) {
                string username = Plugin.Config.Get("username");
                string credentials = Plugin.Config.Get("credentials");
                this.Login(username, null, credentials);
            }

            // Return the session
            return Session;
        }


        /// <summary>
        /// Called when a connection error occurs
        /// </summary>
        void ConnectionError(Session sender, SessionEventArgs e) {
            Log("Spotify: Connection Error: " + e.Error.ToString(), Logger.LogLevel.Error);
        }


        #endregion



        #region Login


        /// <summary>
        /// Login into Spotify with the given username and password or username and credentials.
        /// </summary>
        /// <param name="username">The username to login</param>
        /// <param name="password">The password to use</param>
        /// <param name="credentials">The stored credentials to use</param>
        /// <returns>The resulting error code</returns>
        public async Task<Error> Login(string username, string password, string credentials = null) {
            if (Session == null) {
                Session = await this.Init();
                if (Session == null)
                    return Error.OTHER_TRANSIENT;
            }

            if (Session.ConnectionState == ConnectionState.LoggedIn)
                return Error.OK;

            if (!_loggingIn) {
                _loggingIn = true;
                if (credentials == null)
                    _loginError = await Session.Login(username, password, true);
                else
                    _loginError = await Session.Login(username, credentials);
                _loggingIn = false;
                return _loginError;
            }

            return Error.OTHER_TRANSIENT;
        }


        /// <summary>
        /// Relogin the user. Only works if the user was previously logged in
        /// </summary>
        /// <returns>The resulting error code</returns>
        public async Task<Error> Relogin() {
            if (!_loggingIn) {
                _loggingIn = true;
                _loginError = await Session.Relogin();
                _loggingIn = false;
                return _loginError;
            }
            return Error.OTHER_TRANSIENT;
        }

        
        /// <summary>
        /// Logout the current user
        /// </summary>
        public async void Logout() {
            if (Session != null) {
                Session.ForgetMe();
                Plugin.Config.Remove("username");
                Plugin.Config.Remove("credentials");
                Plugin.Config.Save();
                await Session.Logout();
            }
        }


        /// <summary>
        /// Returns whether the username and credentials have been stored
        /// </summary>
        public bool CredentialsStored {
            get {
                string username = Plugin.Config.Get("username");
                string credentials = Plugin.Config.Get("credentials");
                return !String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(credentials);
            }
        }


        /// <summary>
        /// Called when the credentials blob has been updated from the Spotify session.
        /// Stores the username and these credentials into the Spotify plugin config.
        /// </summary>
        void Session_CredentialsBlobUpdated(Session sender, SessionEventArgs e) {
            Plugin.Config.Set("username", sender.UserName);
            Plugin.Config.Set("credentials", e.Message);
            Plugin.Config.Save();
        }
        

        #endregion



        #region Status updates


        /// <summary>
        /// Called when the Session connection status is updated
        /// </summary>
        async void ConnectionstateUpdated(Session sender, SessionEventArgs e) {
            Log("Spotify: Connection state: " + sender.ConnectionState.ToString());
            this.OnStateUpdated();


            switch (sender.ConnectionState) {
                
                // Logged out
                case ConnectionState.LoggedOut:
                    break;

                // Offline state (when logged in, but not completely yet)
                case ConnectionState.Offline:
                    break;

                // User has logged in
                case ConnectionState.LoggedIn:
                    break;

                // User has been disconnected after having been logged in
                case ConnectionState.Disconnected:
                    break;

                // No clue
                case ConnectionState.Undefined:
                    throw new NotImplementedException();

            }

        }



        /// <summary>
        /// Invoke the StateUpdated event
        /// </summary>
        void OnStateUpdated() {
            if (this.StateUpdated != null)
                this.StateUpdated.Invoke(this);
        }



        public delegate void StateUpdatedEventHandler(SessionHandler sender);
        public event StateUpdatedEventHandler StateUpdated;


        #endregion

        
    }

}
