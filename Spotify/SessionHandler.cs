using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Touchee;
using SpotiFire;

namespace Spotify {


    public enum SessionState {
        LoggedOut,
        LoggingIn,
        LoggedIn
    }


    /// <summary>
    /// 
    /// </summary>
    public class SessionHandler : Base {



        #region Statics

        #endregion



        #region Privates

        Error _loginError;

        bool _loggingIn;

        byte[] _key;

        #endregion



        internal Session Session;
        internal SessionState State = SessionState.LoggedOut;


        #region Constructor


        /// <summary>
        /// Constructor
        /// </summary>
        public SessionHandler(byte[] key) {
            _key = key;
        }


        #endregion


        /// <summary>
        /// Initialises the Spotify handler
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="key"></param>
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
            Session.UserinfoUpdated += _session_UserinfoUpdated;
            
            // But return the session directly
            return Session;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public async Task<Error> Login(string username, string password) {
            if (Session == null) {
                Session = await this.Init();
                if (Session == null)
                    return Error.OTHER_TRANSIENT;
            }
            
            if (Session.ConnectionState == ConnectionState.LoggedIn)
                return Error.OK;
            
            if (!_loggingIn) {
                State = SessionState.LoggingIn;
                this.OnStateUpdated();
                var err = await Session.Login(username, password, true);
                State = err == Error.OK ? SessionState.LoggedIn : SessionState.LoggedOut;
                this.OnStateUpdated();
                return err;
            }

            return Error.OTHER_TRANSIENT;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void ConnectionstateUpdated(Session sender, SessionEventArgs e) {
            Log("Connection state: " + sender.ConnectionState.ToString());
            Log("Loggin error:     " + _loginError.ToString());

            this.OnStateUpdated();

            return;

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


        public delegate void StateUpdatedEventHandler(SessionHandler sender);
        public event StateUpdatedEventHandler StateUpdated;

        void OnStateUpdated() {
            if (this.StateUpdated != null)
                this.StateUpdated.Invoke(this);
        }

        void _session_UserinfoUpdated(Session sender, SessionEventArgs e) {
            this.OnStateUpdated();
        }








        /// <summary>
        /// 
        /// </summary>
        async void Login() {
            if (!_loggingIn) {
                _loggingIn = true;
                _loginError = await Session.Relogin();
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



    }

}
